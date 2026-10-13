using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Il passaggio fra il mondo e una zona chiusa in una scena a parte (per esempio la cripta sotto il cimitero), con la
// schermata di caricamento (Lorenzo, 13 ottobre: zone separate per non caricare troppo il mondo e la mappa).
// Come funziona: la scena della zona si carica IN AGGIUNTA a quella del villaggio (che resta lì con le quest, Ilse,
// Edric...), il giocatore viene portato al punto "Arrivo" della zona e il villaggio si nasconde. Uscendo si torna
// dove si era entrati e la zona si scarica. Così zaino, monete, vita e stato delle quest restano quelli di prima.
// Durante il passaggio: schermo nero con il nome della zona e "Caricamento", comandi fermi.
// Dentro la zona: niente sole, luna e cielo; luce e nebbia sono quelle di AmbienteZona (nella scena della zona);
// le torce restano accese al massimo. Si rinasce all'ingresso della zona (uscendo torna il punto di rinascita di prima).
// Lo stato della zona (leve tirate, bauli aperti, oggetti raccolti, nemici uccisi) lo ricorda MemoriaZona, così
// rientrando la zona è come la si era lasciata.
// Si crea da solo la prima volta che serve (lo chiama PassaggioScena): non va messo nelle scene.
public class CaricamentoZona : MonoBehaviour
{
    // Vero mentre si è dentro una zona chiusa (lo legge Torcia per restare accesa).
    public static bool InZona => istanza != null && istanza.scenaAperta != null;
    public static bool InPassaggio => istanza != null && istanza.passaggio;

    static CaricamentoZona istanza;

    string scenaAperta;
    string chiaveTitolo;
    bool passaggio;
    float nero;              // 0 trasparente, 1 tutto nero
    bool mostraCaricamento;
    Vector3 ritornoPosizione;
    Quaternion ritornoRotazione;
    Vector3 rinascitaPrima;
    Quaternion rotazioneRinascitaPrima;
    readonly List<GameObject> nascosti = new List<GameObject>();
    readonly List<Behaviour> spenti = new List<Behaviour>();
    readonly List<Light> luciSpente = new List<Light>();
    // luce e cielo di prima, da rimettere uscendo
    Color ambientePrima, nebbiaPrima;
    float densitaPrima;
    bool nebbiaAccesa;
    Material cieloPrima;
    CameraClearFlags sfondoPrima;
    Color coloreSfondoPrima;

    static CaricamentoZona Istanza
    {
        get
        {
            if (istanza == null)
            {
                var o = new GameObject("Caricamento delle zone");
                DontDestroyOnLoad(o);
                istanza = o.AddComponent<CaricamentoZona>();
            }
            return istanza;
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += SceneCaricata;
    void OnDisable() => SceneManager.sceneLoaded -= SceneCaricata;

    // Una scena caricata da sola (per esempio il menu): si dimentica la zona.
    void SceneCaricata(Scene s, LoadSceneMode modo)
    {
        if (modo != LoadSceneMode.Single) return;
        StopAllCoroutines();
        scenaAperta = null;
        passaggio = false;
        nero = 0f;
        nascosti.Clear();
        spenti.Clear();
        luciSpente.Clear();
    }

    // Entra nella zona "scena" (deve essere nelle Build Settings). "nascondi": oggetti del mondo da spegnere mentre si
    // è dentro (per esempio "Villaggio"). "dopo": cosa fare una volta dentro.
    public static void Entra(string scena, string chiaveTitolo, GameObject[] nascondi, System.Action dopo)
    {
        if (InPassaggio || InZona) return;
        Istanza.StartCoroutine(Istanza.EntraPiano(scena, chiaveTitolo, nascondi, dopo));
    }

    // Esce dalla zona aperta e torna dove si era entrati.
    public static void Esci(System.Action dopo)
    {
        if (InPassaggio || !InZona) return;
        Istanza.StartCoroutine(Istanza.EsciPiano(dopo));
    }

    IEnumerator EntraPiano(string scena, string titolo, GameObject[] nascondi, System.Action dopo)
    {
        var giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) yield break;
        passaggio = true;
        chiaveTitolo = titolo;
        yield return Scurisci(giocatore);
        float inizio = Time.unscaledTime;

        var carica = SceneManager.LoadSceneAsync(scena, LoadSceneMode.Additive);
        if (carica == null)
        {
            Debug.LogWarning("CaricamentoZona: la scena " + scena + " non è nelle Build Settings.");
            yield return Schiarisci(giocatore);
            passaggio = false;
            yield break;
        }
        while (!carica.isDone) yield return null;
        Scene zona = SceneManager.GetSceneByName(scena);
        scenaAperta = scena;

        ritornoPosizione = giocatore.transform.position;
        ritornoRotazione = giocatore.transform.rotation;
        rinascitaPrima = giocatore.PuntoRinascita;
        rotazioneRinascitaPrima = giocatore.RotazioneRinascita;

        nascosti.Clear();
        if (nascondi != null)
            foreach (var o in nascondi)
                if (o != null && o.activeSelf) { o.SetActive(false); nascosti.Add(o); }
        ApplicaAmbiente(zona);

        Transform arrivo = Trova(zona, "Arrivo");
        if (arrivo != null)
        {
            giocatore.Teletrasporta(arrivo.position + Vector3.up, arrivo.rotation);
            giocatore.ImpostaRinascita(arrivo.position + Vector3.up, arrivo.rotation);
        }
        else Debug.LogWarning("CaricamentoZona: nella scena " + scena + " manca l'oggetto \"Arrivo\".");

        while (Time.unscaledTime - inizio < 1.4f) yield return null;   // il nome della zona si legge
        dopo?.Invoke();
        yield return Schiarisci(giocatore);
        passaggio = false;
    }

    IEnumerator EsciPiano(System.Action dopo)
    {
        var giocatore = FindFirstObjectByType<GiocatoreControllo>();
        passaggio = true;
        chiaveTitolo = null;
        if (giocatore != null) yield return Scurisci(giocatore);
        float inizio = Time.unscaledTime;

        string scena = scenaAperta;
        Scene zona = SceneManager.GetSceneByName(scena);
        foreach (var memoria in FindObjectsByType<MemoriaZona>(FindObjectsSortMode.None))
            if (memoria.gameObject.scene == zona) memoria.Salva();

        RimettiAmbiente();
        foreach (var o in nascosti) if (o != null) o.SetActive(true);
        nascosti.Clear();
        if (giocatore != null)
        {
            giocatore.Teletrasporta(ritornoPosizione, ritornoRotazione * Quaternion.Euler(0f, 180f, 0f));   // girato verso fuori
            giocatore.ImpostaRinascita(rinascitaPrima, rotazioneRinascitaPrima);
        }

        var scarica = SceneManager.UnloadSceneAsync(zona);
        if (scarica != null) while (!scarica.isDone) yield return null;
        scenaAperta = null;
        while (Time.unscaledTime - inizio < 0.8f) yield return null;
        dopo?.Invoke();
        if (giocatore != null) yield return Schiarisci(giocatore);
        passaggio = false;
    }

    // ---------- luce ----------

    void ApplicaAmbiente(Scene zona)
    {
        ambientePrima = RenderSettings.ambientLight;
        nebbiaPrima = RenderSettings.fogColor;
        densitaPrima = RenderSettings.fogDensity;
        nebbiaAccesa = RenderSettings.fog;
        cieloPrima = RenderSettings.skybox;

        spenti.Clear();
        luciSpente.Clear();
        var ciclo = CicloGiornoNotte.Istanza;
        if (ciclo != null && ciclo.enabled) { ciclo.enabled = false; spenti.Add(ciclo); }
        foreach (var luce in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (luce.type == LightType.Directional && luce.enabled && luce.gameObject.scene != zona) { luce.enabled = false; luciSpente.Add(luce); }

        var ambiente = FindFirstObjectByType<AmbienteZona>();
        Color luce = ambiente != null ? ambiente.luceAmbiente : new Color(0.08f, 0.08f, 0.1f);
        Color nebbia = ambiente != null ? ambiente.nebbia : Color.black;
        RenderSettings.ambientLight = luce;
        RenderSettings.fog = true;
        RenderSettings.fogColor = nebbia;
        RenderSettings.fogDensity = ambiente != null ? ambiente.densitaNebbia : 0.05f;
        RenderSettings.skybox = null;
        var cam = Camera.main;
        if (cam != null)
        {
            sfondoPrima = cam.clearFlags;
            coloreSfondoPrima = cam.backgroundColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = nebbia;
        }
    }

    void RimettiAmbiente()
    {
        RenderSettings.ambientLight = ambientePrima;
        RenderSettings.fogColor = nebbiaPrima;
        RenderSettings.fogDensity = densitaPrima;
        RenderSettings.fog = nebbiaAccesa;
        RenderSettings.skybox = cieloPrima;
        var cam = Camera.main;
        if (cam != null) { cam.clearFlags = sfondoPrima; cam.backgroundColor = coloreSfondoPrima; }
        foreach (var b in spenti) if (b != null) b.enabled = true;
        foreach (var l in luciSpente) if (l != null) l.enabled = true;
        spenti.Clear();
        luciSpente.Clear();
    }

    // ---------- schermo nero ----------

    IEnumerator Scurisci(GiocatoreControllo giocatore)
    {
        giocatore.enabled = false;   // comandi fermi durante il passaggio
        mostraCaricamento = true;
        for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime) { nero = t / 0.5f; yield return null; }
        nero = 1f;
    }

    IEnumerator Schiarisci(GiocatoreControllo giocatore)
    {
        mostraCaricamento = false;
        for (float t = 0f; t < 0.7f; t += Time.unscaledDeltaTime) { nero = 1f - t / 0.7f; yield return null; }
        nero = 0f;
        if (giocatore != null) giocatore.enabled = true;
    }

    void OnGUI()
    {
        if (nero <= 0.001f) return;
        GUI.depth = -90;
        GUI.color = new Color(0f, 0f, 0f, nero);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        if (!mostraCaricamento || nero < 0.95f) return;
        GraficaMenu.PreparaStili();
        float L = GraficaMenu.FoglioIntero();
        if (!string.IsNullOrEmpty(chiaveTitolo))
        {
            string titolo = Lingua.T(chiaveTitolo).ToUpperInvariant();
            if (Lingua.Indice < 6) titolo = GraficaMenu.Spaziato(titolo);
            GraficaMenu.Scritta(new Rect(0f, 440f, L, 80f), titolo, GraficaMenu.Intestazione, GraficaMenu.Testo, 1f);
            GraficaMenu.Divisore(L * 0.5f, 540f, 520f, 1f);
        }
        // "Caricamento" con un rombo che pulsa, in basso a destra
        float pulsa = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f);
        GraficaMenu.Rombo(new Vector2(L - 90f, 1000f), 16f, GraficaMenu.Con(GraficaMenu.Selezione, 0.4f + 0.6f * pulsa));
        var stile = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleRight };
        GraficaMenu.Scritta(new Rect(L - 520f, 984f, 400f, 32f), Lingua.T("car.caricamento"), stile, GraficaMenu.Spento, 1f);
    }

    static Transform Trova(Scene scena, string nome)
    {
        if (!scena.IsValid()) return null;
        foreach (var radice in scena.GetRootGameObjects())
            foreach (var t in radice.GetComponentsInChildren<Transform>(true))
                if (t.name == nome) return t;
        return null;
    }
}
