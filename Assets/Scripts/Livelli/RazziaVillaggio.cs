using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// La razzia degli orchi a Villaggio Lago Nero, la notte dopo la quest 6 (Lorenzo, 13 ottobre).
// Come va:
//   1. Il giocatore esce dalla taverna: è già notte e la prima ondata (circa 10 orchi sparsi per il villaggio) sta già
//      distruggendo le case (RazziatoreOrco: pareti, porte, calci, fuoco). Diverse case bruciano già (CasaInRovina).
//      Gli abitanti spariscono dalle strade. Obiettivo: "Respingi gli orchi (N da abbattere)", la bussola indica
//      l'orco vivo più vicino.
//   2. Abbattuto il 65% della prima ondata, gli altri battono in ritirata (scappano verso i punti di ritirata e
//      spariscono).
//   3. Per 1 minuto tamburi e urla dal bosco, sempre più forti.
//   4. Arriva la seconda ondata: circa 16 orchi, alcuni corazzati con l'ascia orchesca. Entrano correndo da fuori vista
//      e vanno ai loro posti (dove riprendono a distruggere); chi vede il giocatore lo attacca.
//   5. Abbattuto il 70% della seconda ondata, appena il giocatore è ad almeno 20 metri dalla piazza la terra trema
//      (camera che trema, rombo) e l'orco enorme salta giù in piazza: lo scontro scritto per essere perso
//      (ScontroOrcoEnorme: invulnerabile dopo 3 minuti). Quando il giocatore muore, tutto diventa nero e ci si
//      risveglia nella gattabuia (scena "Scena Gattabuia", se è nelle Build Settings).
// Abitanti in fuga (Giuseppe, 14 ottobre): durante le ondate ci sono sempre alcuni abitanti (AbitanteInFuga) che
// scappano per il villaggio, inseguiti dagli orchi vicini; compaiono fuori dalla vista del giocatore e, morti o
// salvati, vengono sostituiti. Non contano per le percentuali delle ondate.
// Ogni passaggio ha un evento ("All'Inizio", "Alla Ritirata", "Alla Seconda Ondata", "All'Orco Enorme") per altre cose.
// Percentuali, secondi e distanze si regolano dall'Inspector.
// Come montarlo: su un oggetto vuoto della scena del villaggio, con le due ondate (oggetti con gli orchi dentro, spenti)
// e l'orco enorme (spento). Lo prepara da solo il comando "magic-gnl > Prologo: prepara la quest 6 e la razzia", che
// lo collega anche all'evento "Alla razzia" di OrarioPrologo. Per provare: in Play, tasto destro sul componente.
// Il prologo si gioca da soli: la razzia la decide solo questo PC.
public class RazziaVillaggio : MonoBehaviour
{
    public enum Fase { Attesa, PrimaOndata, Tamburi, SecondaOndata, Lontano, Terremoto, OrcoEnorme }

    [Header("Chi c'è")]
    [Tooltip("Gli orchi della prima ondata (figli di questo oggetto, spento finché non parte la razzia).")]
    [SerializeField] Transform primaOndata;
    [Tooltip("Gli orchi della seconda ondata, già ai loro posti (spento): entrano correndo da fuori vista.")]
    [SerializeField] Transform secondaOndata;
    [SerializeField] GameObject orcoEnorme;
    [SerializeField] Transform piazza;
    [Tooltip("Dove scappano gli orchi in ritirata (ai bordi del villaggio).")]
    [SerializeField] Transform[] puntiRitirata;
    [Tooltip("Abitanti da togliere dalle strade quando parte la razzia.")]
    [SerializeField] GameObject[] nascondiAllaRazzia;

    [Header("Regole")]
    [SerializeField, Range(1f, 100f)] float percentualeRitirata = 65f;
    [SerializeField, Range(1f, 100f)] float percentualeOrcoEnorme = 70f;
    [SerializeField, Min(0f)] float secondiTamburi = 60f;
    [Tooltip("L'orco enorme arriva solo quando il giocatore è almeno a questa distanza dalla piazza.")]
    [SerializeField, Min(0f)] float distanzaDallaPiazza = 20f;
    [SerializeField, Min(0f)] float secondiTerremoto = 4f;
    [Tooltip("Da che altezza salta giù l'orco enorme.")]
    [SerializeField, Min(0f)] float altezzaSalto = 24f;
    [Tooltip("Gli orchi della seconda ondata partono da questa distanza dal loro posto, fuori dalla vista del giocatore.")]
    [SerializeField, Min(4f)] float distanzaEntrata = 16f;

    [Header("Abitanti in fuga")]
    [Tooltip("Quanti abitanti scappano per il villaggio nello stesso momento (0 = nessuno).")]
    [SerializeField, Min(0)] int abitantiInScena = 5;
    [Tooltip("Secondi fra un abitante nuovo e il successivo, quando ne manca qualcuno.")]
    [SerializeField, Min(0.5f)] float secondiRicambio = 6f;
    [Tooltip("Gli abitanti nuovi compaiono fra queste distanze dal giocatore (in metri), fuori dalla sua vista.")]
    [SerializeField] Vector2 distanzaAbitanti = new Vector2(18f, 50f);

    [Header("Eventi")]
    [SerializeField] UnityEvent allInizio;
    [SerializeField] UnityEvent allaRitirata;
    [SerializeField] UnityEvent allaSecondaOndata;
    [SerializeField] UnityEvent allOrcoEnorme;

    [Tooltip("La scena dove ci si risveglia dopo lo scontro perso con l'orco enorme.")]
    [SerializeField] string scenaGattabuia = "Gattabuia";

    [Tooltip("Materiale della polvere quando atterra l'orco enorme (lo mette il comando).")]
    [SerializeField] Material materialePolvere;

    public static RazziaVillaggio Istanza { get; private set; }
    public Fase Attuale => fase;

    Fase fase = Fase.Attesa;
    readonly List<Bersaglio> prima = new List<Bersaglio>();
    readonly List<Bersaglio> seconda = new List<Bersaglio>();
    readonly List<(InseguimentoNemico orco, float da)> inFuga = new List<(InseguimentoNemico, float)>();
    GiocatoreControllo giocatore;
    float prossimaMeta;
    readonly List<AbitanteInFuga> abitanti = new List<AbitanteInFuga>();
    float prossimoAbitante;
    int ultimoNumero = -1;

    void Awake() => Istanza = this;
    void OnDestroy() { if (Istanza == this) Istanza = null; }

    // ---------- 1. inizio ----------

    // Fa partire la razzia (da "Alla razzia" di OrarioPrologo). Una volta sola.
    public void Inizia()
    {
        if (fase != Fase.Attesa) return;
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (nascondiAllaRazzia != null) foreach (var o in nascondiAllaRazzia) if (o != null) o.SetActive(false);
        foreach (var casa in CasaInRovina.Tutte.ToArray()) if (casa != null) casa.Razzia();
        Accendi(primaOndata, prima);
        // si rinasce dove si è usciti dalla taverna
        if (giocatore != null) giocatore.ImpostaRinascita(giocatore.transform.position, giocatore.transform.rotation);
        fase = Fase.PrimaOndata;
        Suoni.SuonaOvunque(Suono.UrloOrco, 0.5f);
        MessaggiSchermo.Mostra(Lingua.T("msg.razzia.inizio"), 4f);
        allInizio.Invoke();
    }

    static void Accendi(Transform ondata, List<Bersaglio> elenco)
    {
        elenco.Clear();
        if (ondata == null) return;
        ondata.gameObject.SetActive(true);
        foreach (Transform figlio in ondata)
        {
            figlio.gameObject.SetActive(true);
            var b = figlio.GetComponent<Bersaglio>();
            if (b != null) elenco.Add(b);
        }
    }

    void Update()
    {
        if (fase == Fase.Attesa) return;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        AggiornaFuga();
        AggiornaAbitanti();
        switch (fase)
        {
            case Fase.PrimaOndata:
                Obiettivo("ob.razzia.prima", prima, percentualeRitirata);
                if (Morti(prima) >= Servono(prima, percentualeRitirata)) Ritirata();
                break;
            case Fase.SecondaOndata:
                Obiettivo("ob.razzia.seconda", seconda, percentualeOrcoEnorme);
                if (Morti(seconda) >= Servono(seconda, percentualeOrcoEnorme))
                {
                    fase = Fase.Lontano;
                    Missioni.Istanza?.ImpostaObiettivo("ob.razzia.cerca");
                    Missioni.Istanza?.TogliMeta();
                }
                break;
            case Fase.OrcoEnorme:
                if (!sconfitto && giocatore != null && giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) StartCoroutine(Sconfitta());
                break;
            case Fase.Lontano:
                if (giocatore != null && piazza != null && Orizzontale(giocatore.transform.position, piazza.position) >= distanzaDallaPiazza)
                    StartCoroutine(Terremoto());
                break;
        }
    }

    // ---------- abitanti in fuga ----------

    void AggiornaAbitanti()
    {
        for (int i = abitanti.Count - 1; i >= 0; i--)
            if (abitanti[i] == null || abitanti[i].Finito)
            {
                if (abitanti[i] != null) Destroy(abitanti[i].gameObject);
                abitanti.RemoveAt(i);
            }
        bool durante = fase == Fase.PrimaOndata || fase == Fase.Tamburi || fase == Fase.SecondaOndata || fase == Fase.Lontano;
        if (!durante || Rete.Ospite || giocatore == null) return;
        int vivi = 0;
        foreach (var a in abitanti) if (!a.Morto) vivi++;
        if (vivi >= abitantiInScena || Time.time < prossimoAbitante) return;
        prossimoAbitante = Time.time + secondiRicambio;
        if (TrovaPostoAbitante(out Vector3 dove)) abitanti.Add(CreaAbitante(dove));
    }

    // Davanti a una casa a caso, abbastanza lontano dal giocatore e fuori dalla sua vista.
    bool TrovaPostoAbitante(out Vector3 dove)
    {
        dove = Vector3.zero;
        var case_ = CasaInRovina.Tutte;
        if (case_.Count == 0) return false;
        Vector3 occhi = giocatore.transform.position + Vector3.up * 0.8f;
        for (int prova = 0; prova < 12; prova++)
        {
            var c = case_[Random.Range(0, case_.Count)];
            if (c == null) continue;
            Vector3 p = c.Davanti + c.Fuori * 2.2f + Vector3.up * 0.9f;
            float d = Orizzontale(p, giocatore.transform.position);
            if (d < distanzaAbitanti.x || d > distanzaAbitanti.y) continue;
            if (Physics.CheckCapsule(p + Vector3.up * -0.3f, p + Vector3.up * 0.6f, 0.4f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (!Physics.Linecast(occhi, p + Vector3.up * 0.5f, ~0, QueryTriggerInteraction.Ignore)) continue;   // il giocatore lo vedrebbe comparire
            dove = p;
            return true;
        }
        return false;
    }

    AbitanteInFuga CreaAbitante(Vector3 dove)
    {
        var o = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        o.name = "Abitante in fuga";
        o.transform.SetParent(transform, true);
        o.transform.position = dove;
        o.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);   // alto 1,7 metri
        var a = o.AddComponent<AbitanteInFuga>();
        Vector3 uscita = puntiRitirata != null && puntiRitirata.Length > 0 && puntiRitirata[0] != null
            ? puntiRitirata[Random.Range(0, puntiRitirata.Length)].position + Vector3.up * 0.85f
            : dove + Random.onUnitSphere * 40f;
        a.Prepara(uscita);
        return a;
    }

    // "Respingi gli orchi (3 da abbattere)" e la bussola sull'orco vivo più vicino.
    void Obiettivo(string chiave, List<Bersaglio> ondata, float percentuale)
    {
        int restano = Mathf.Max(0, Servono(ondata, percentuale) - Morti(ondata));
        if (restano != ultimoNumero)
        {
            ultimoNumero = restano;
            Missioni.Istanza?.ImpostaObiettivoConNumero(chiave, restano);
        }
        if (Time.time < prossimaMeta || giocatore == null || Missioni.Istanza == null) return;
        prossimaMeta = Time.time + 1.5f;
        Bersaglio vicino = null;
        float minima = float.MaxValue;
        foreach (var b in ondata)
        {
            if (b == null || b.Morto || !b.gameObject.activeInHierarchy) continue;
            float d = (b.transform.position - giocatore.transform.position).sqrMagnitude;
            if (d < minima) { minima = d; vicino = b; }
        }
        if (vicino != null) Missioni.Istanza.ImpostaMeta(vicino.transform);
    }

    static int Morti(List<Bersaglio> ondata)
    {
        int n = 0;
        foreach (var b in ondata) if (b == null || b.Morto) n++;
        return n;
    }

    static int Servono(List<Bersaglio> ondata, float percentuale) => Mathf.Max(1, Mathf.CeilToInt(ondata.Count * percentuale / 100f));

    // ---------- 2. ritirata e 3. tamburi ----------

    void Ritirata()
    {
        fase = Fase.Tamburi;
        ultimoNumero = -1;
        foreach (var b in prima)
        {
            if (b == null || b.Morto || !b.TryGetComponent(out InseguimentoNemico vista)) continue;
            vista.Ritirati(PuntoRitirata(b.transform.position));
            inFuga.Add((vista, Time.time));
        }
        MessaggiSchermo.Mostra(Lingua.T("msg.razzia.ritirata"), 4f);
        Missioni.Istanza?.ImpostaObiettivo("ob.razzia.attesa");
        Missioni.Istanza?.TogliMeta();
        allaRitirata.Invoke();
        StartCoroutine(Tamburi());
    }

    // Il punto di ritirata più lontano dal giocatore fra i due più vicini all'orco (così non gli scappa incontro).
    Vector3 PuntoRitirata(Vector3 da)
    {
        if (puntiRitirata == null || puntiRitirata.Length == 0) return da + (da - (giocatore != null ? giocatore.transform.position : da)).normalized * 60f;
        Transform migliore = null;
        float punteggio = float.MinValue;
        foreach (var p in puntiRitirata)
        {
            if (p == null) continue;
            float lontanoDalGiocatore = giocatore != null ? Orizzontale(p.position, giocatore.transform.position) : 0f;
            float s = lontanoDalGiocatore - Orizzontale(p.position, da) * 0.8f;
            if (s > punteggio) { punteggio = s; migliore = p; }
        }
        return migliore != null ? migliore.position + Vector3.up * 1.1f : da;
    }

    // Chi scappa sparisce quando arriva, o quando è lontano dal giocatore da un po'.
    void AggiornaFuga()
    {
        for (int i = inFuga.Count - 1; i >= 0; i--)
        {
            var (orco, da) = inFuga[i];
            if (orco == null) { inFuga.RemoveAt(i); continue; }
            bool lontano = giocatore == null || Orizzontale(orco.transform.position, giocatore.transform.position) > 30f;
            if (orco.Arrivato || (Time.time - da > 12f && lontano) || Time.time - da > 40f)
            {
                orco.gameObject.SetActive(false);
                inFuga.RemoveAt(i);
            }
        }
    }

    IEnumerator Tamburi()
    {
        yield return new WaitForSeconds(3f);
        MessaggiSchermo.Mostra(Lingua.T("msg.razzia.tamburi"), 4f);
        float inizio = Time.time;
        float prossimoUrlo = Time.time + 4f;
        int colpo = 0;
        // ritmo: BUM . BUM . BUM BUM BUM . (sempre più forte e un po' più veloce)
        bool[] ritmo = { true, false, true, false, true, true, true, false };
        while (Time.time - inizio < secondiTamburi)
        {
            float s = Mathf.Clamp01((Time.time - inizio) / Mathf.Max(1f, secondiTamburi));
            if (ritmo[colpo % ritmo.Length]) Suoni.SuonaOvunque(Suono.Tamburo, Mathf.Lerp(0.25f, 0.95f, s) * (colpo % 8 == 0 ? 1f : 0.8f));
            colpo++;
            if (Time.time >= prossimoUrlo)
            {
                prossimoUrlo = Time.time + Random.Range(Mathf.Lerp(7f, 2.5f, s), Mathf.Lerp(11f, 4f, s));
                Suoni.SuonaOvunque(Suono.UrloOrco, Mathf.Lerp(0.2f, 0.65f, s));
            }
            yield return new WaitForSeconds(Mathf.Lerp(0.42f, 0.3f, s));
        }
        SecondaOndata();
    }

    // ---------- 4. seconda ondata ----------

    void SecondaOndata()
    {
        Accendi(secondaOndata, seconda);
        foreach (var b in seconda)
        {
            if (b == null || !b.TryGetComponent(out InseguimentoNemico vista)) continue;
            Vector3 posto = b.transform.position;
            Vector3 entrata = Entrata(posto);
            vista.CambiaPosto(posto, b.transform.forward);
            b.transform.position = entrata;
            vista.VaiAlPosto(true);
        }
        fase = Fase.SecondaOndata;
        Suoni.SuonaOvunque(Suono.UrloOrco, 0.9f);
        Suoni.SuonaOvunque(Suono.Tamburo, 1f);
        MessaggiSchermo.Mostra(Lingua.T("msg.razzia.seconda"), 4f);
        allaSecondaOndata.Invoke();
    }

    // Da dove entra un orco della seconda ondata: a "Distanza Entrata" dal suo posto, lontano dal giocatore, fuori dalla
    // sua vista e in un punto libero. Se non ne trova, parte dal posto.
    Vector3 Entrata(Vector3 posto)
    {
        Vector3 occhi = giocatore != null ? giocatore.transform.position + Vector3.up * 0.8f : posto;
        Vector3 migliore = posto;
        float punteggio = float.MinValue;
        for (int i = 0; i < 12; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, i * 30f, 0f) * Vector3.forward;
            Vector3 p = posto + dir * distanzaEntrata;
            if (Physics.CheckCapsule(p + Vector3.up * -0.4f, p + Vector3.up * 0.6f, 0.45f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (Physics.Linecast(posto + Vector3.up * 0.5f, p + Vector3.up * 0.5f, ~0, QueryTriggerInteraction.Ignore)) continue;   // non attraverso le case
            float s = giocatore != null ? Orizzontale(p, giocatore.transform.position) : 0f;
            if (giocatore != null && !Physics.Linecast(occhi, p + Vector3.up * 0.8f, ~0, QueryTriggerInteraction.Ignore)) s -= 1000f;   // il giocatore lo vedrebbe comparire
            if (s > punteggio) { punteggio = s; migliore = p; }
        }
        return migliore;
    }

    // ---------- 5. terremoto e orco enorme ----------

    IEnumerator Terremoto()
    {
        fase = Fase.Terremoto;
        MessaggiSchermo.Mostra(Lingua.T("msg.razzia.terremoto"), secondiTerremoto + 1f);
        float inizio = Time.time;
        float prossimoRombo = 0f;
        while (Time.time - inizio < secondiTerremoto)
        {
            float s = (Time.time - inizio) / Mathf.Max(0.1f, secondiTerremoto);
            CameraTerzaPersona.Scuoti(Mathf.Lerp(0.04f, 0.18f, s), 0.3f);
            if (Time.time >= prossimoRombo)
            {
                prossimoRombo = Time.time + 2.5f;
                Suoni.SuonaOvunque(Suono.Rombo, Mathf.Lerp(0.6f, 1f, s));
            }
            yield return null;
        }
        if (orcoEnorme == null || piazza == null) { fase = Fase.OrcoEnorme; yield break; }

        // salta giù dall'alto in mezzo alla piazza
        Vector3 terra = piazza.position;
        orcoEnorme.transform.position = terra + Vector3.up * AltezzaCentro(orcoEnorme);
        orcoEnorme.SetActive(true);   // Awake: il suo posto è la piazza
        var bersaglio = orcoEnorme.GetComponent<Bersaglio>();
        var vista = orcoEnorme.GetComponent<InseguimentoNemico>();
        if (bersaglio != null) bersaglio.enabled = false;
        if (vista != null) vista.enabled = false;
        if (giocatore != null)
        {
            Vector3 verso = giocatore.transform.position - terra;
            verso.y = 0f;
            if (verso.sqrMagnitude > 0.01f) orcoEnorme.transform.rotation = Quaternion.LookRotation(verso);
        }
        Vector3 arrivo = orcoEnorme.transform.position;
        Vector3 partenza = arrivo + Vector3.up * altezzaSalto;
        const float durata = 1.1f;
        for (float t = 0f; t < durata; t += Time.deltaTime)
        {
            float k = t / durata;
            orcoEnorme.transform.position = Vector3.Lerp(partenza, arrivo, k * k);   // cade sempre più veloce
            yield return null;
        }
        orcoEnorme.transform.position = arrivo;
        CameraTerzaPersona.Scuoti(0.5f, 1.4f);
        Suoni.SuonaOvunque(Suono.CrolloMuro, 1f);
        Suoni.SuonaOvunque(Suono.Rombo, 1f);
        Polvere(terra);
        yield return new WaitForSeconds(0.6f);
        Suoni.SuonaOvunque(Suono.UrloOrco, 1f);
        if (bersaglio != null) bersaglio.enabled = true;
        if (vista != null) vista.enabled = true;
        if (orcoEnorme.GetComponent<ScontroOrcoEnorme>() == null) orcoEnorme.AddComponent<ScontroOrcoEnorme>();

        fase = Fase.OrcoEnorme;
        MessaggiSchermo.Mostra(Lingua.T("msg.razzia.enorme"), 4f);
        Missioni.Istanza?.ImpostaObiettivo("ob.razzia.enorme");
        Missioni.Istanza?.ImpostaMeta(orcoEnorme.transform);
        allOrcoEnorme.Invoke();
    }

    // ---------- sconfitta: risveglio nella gattabuia ----------

    bool sconfitto;
    float nero;
    string scrittaNero;

    System.Collections.IEnumerator Sconfitta()
    {
        sconfitto = true;
        yield return new WaitForSeconds(1.2f);
        scrittaNero = Lingua.T("msg.razzia.buio");
        for (float t = 0f; t < 1.5f; t += Time.unscaledDeltaTime) { nero = t / 1.5f; yield return null; }
        nero = 1f;
        yield return new WaitForSecondsRealtime(2.5f);
        if (Application.CanStreamedLevelBeLoaded(scenaGattabuia))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(scenaGattabuia);
            yield break;
        }
        // la gattabuia non c'è ancora: si torna al villaggio
        Debug.LogWarning("RazziaVillaggio: la scena " + scenaGattabuia + " non è nelle Build Settings.");
        scrittaNero = null;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime) { nero = 1f - t; yield return null; }
        nero = 0f;
        sconfitto = false;
    }

    void OnGUI()
    {
        if (nero <= 0.001f) return;
        GUI.depth = -80;
        GUI.color = new Color(0f, 0f, 0f, nero);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        if (string.IsNullOrEmpty(scrittaNero) || nero < 0.9f) return;
        GraficaMenu.PreparaStili();
        float L = GraficaMenu.FoglioIntero();
        GraficaMenu.Scritta(new Rect(0f, 500f, L, 80f), scrittaNero, GraficaMenu.Voce, GraficaMenu.Spento, nero);
    }

    static float AltezzaCentro(GameObject o) => o.transform.lossyScale.y;   // cilindro di Unity: alto 2 x scala, centro a metà

    // Un anello di polvere che si allarga a terra.
    void Polvere(Vector3 dove)
    {
        var o = new GameObject("Polvere dell'orco enorme");
        o.transform.SetPositionAndRotation(dove + Vector3.up * 0.3f, Quaternion.Euler(-90f, 0f, 0f));
        var ps = o.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 11f);
        main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.startColor = new Color(0.32f, 0.28f, 0.24f, 0.7f);
        main.stopAction = ParticleSystemStopAction.Destroy;
        var emissione = ps.emission;
        emissione.rateOverTime = 0f;
        emissione.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });
        var forma = ps.shape;
        forma.shapeType = ParticleSystemShapeType.Circle;   // nel piano: l'anello si allarga a terra
        forma.radius = 1.5f;
        var limite = ps.limitVelocityOverLifetime;
        limite.enabled = true;
        limite.dampen = 0.15f;
        limite.limit = 1f;
        var colore = ps.colorOverLifetime;
        colore.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colore.color = g;
        var r = o.GetComponent<ParticleSystemRenderer>();
        if (materialePolvere != null) r.sharedMaterial = materialePolvere;
        ps.Play();
    }

    static float Orizzontale(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // ---------- prove ----------

    [ContextMenu("Prova: fai partire la razzia")]
    void ProvaInizia()
    {
        if (OrarioPrologo.Istanza != null && !OrarioPrologo.Istanza.TempoLibero) OrarioPrologo.Istanza.NotteSubito();
        else Inizia();
    }

    [ContextMenu("Prova: abbatti gli orchi dell'ondata in corso (tranne 2)")]
    public void ProvaAbbatti()
    {
        var ondata = fase == Fase.PrimaOndata ? prima : fase == Fase.SecondaOndata ? seconda : null;
        if (ondata == null) return;
        int vivi = 0;
        foreach (var b in ondata) if (b != null && !b.Morto) vivi++;
        foreach (var b in ondata)
        {
            if (vivi <= 2) break;
            if (b == null || b.Morto) continue;
            b.RiceviColpo(b.VitaMassima * 20f, b.transform.position + b.transform.forward);
            vivi--;
        }
    }

    [ContextMenu("Prova: salta i tamburi")]
    public void ProvaSaltaTamburi()
    {
        if (fase != Fase.Tamburi) return;
        StopAllCoroutines();
        SecondaOndata();
    }
}
