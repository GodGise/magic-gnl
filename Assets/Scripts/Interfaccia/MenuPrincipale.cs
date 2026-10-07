using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Menu iniziale del gioco.
// Schermate: titolo ("premi un tasto"), menu principale (Nuova partita, Continua, Opzioni, Crediti, Esci),
// scelta della classe (Guerriero, Ladro, Stregone), opzioni (lingua, volumi, schermo intero, effetto retro) e crediti
// (provvisori, che scorrono: l'elenco è in TestiCrediti.cs).
// Tutti i testi passano da Lingua.T(...): si traducono nelle 8 lingue del gioco (vedi Lingua.cs).
// Si usa con mouse, tastiera (frecce o WASD, Invio, Esc) o pad (croce o levetta, A per confermare, B per tornare).
// Le opzioni restano salvate anche chiudendo il gioco. La classe scelta va in SceltaPartita.Classe.
// "Nuova partita" carica la scena di gioco indicata in "Scena iniziale"; se non è nelle Build Settings
// usa la "Scena di riserva" (ZonaProva).
// Musica: trascinare un file audio nel campo "Musica" (per esempio la traccia fatta con Suno). Parte piano,
// sale lentamente e si spegne quando inizia la partita.
// Come montarlo: su un oggetto vuoto della scena Menu. Il menu "magic-gnl > Crea scena menu" prepara tutto da solo.
[RequireComponent(typeof(AudioSource))]
public class MenuPrincipale : MonoBehaviour
{
    [Header("Testi")]
    [SerializeField] string titolo = "magic-GNL";
    [Tooltip("Mostra la scritta \"nome provvisorio\" sotto il titolo, finché il nome del gioco non è deciso.")]
    [SerializeField] bool nomeProvvisorio = true;
    [Tooltip("Carattere per le scritte. Vuoto = carattere di base di Unity.")]
    [SerializeField] Font carattere;

    [Header("Partita")]
    [SerializeField] string scenaIniziale = "VillaggioLagoNero";
    [SerializeField] string scenaRiserva = "ZonaProva";

    [Header("Musica")]
    [SerializeField] AudioClip musica;
    [Tooltip("Secondi per far salire (o scendere) la musica.")]
    [SerializeField] float dissolvenzaMusica = 4f;

    [Header("Colori")]
    [SerializeField] Color coloreTesto = new Color(0.85f, 0.81f, 0.74f);
    [SerializeField] Color coloreSelezione = new Color(0.95f, 0.63f, 0.3f);
    [SerializeField] Color coloreSpento = new Color(0.45f, 0.43f, 0.4f);

    enum Schermata { Titolo, Principale, Classe, Opzioni, Crediti }

    class Voce
    {
        public Func<string> testo;
        public bool attiva = true;
        public Action conferma;
        public Action<int> regola;   // frecce sinistra e destra (-1 o +1)
    }

    const float Larghezza = 1920f, Altezza = 1080f;
    const string ChiaveVolume = "VolumeGenerale", ChiaveMusica = "VolumeMusica", ChiaveRetro = "EffettoRetro";

    Schermata schermata = Schermata.Titolo;
    List<Voce> voci = new List<Voce>();
    int selezione;
    float tempoSchermata;
    float nero = 1f;              // velo nero sopra tutto: 1 = schermo nero
    float livelloMusica;          // 0..1, per la dissolvenza
    bool avvioInCorso;
    string scenaDaCaricare;
    string avviso;
    float tempoAvviso;
    float prossimoScatto;         // per ripetere il movimento tenendo premuta la levetta
    Vector2 ultimoMouse = new Vector2(-1f, -1f);
    AudioSource sorgente;

    float volumeGenerale, volumeMusica;
    bool effettoRetro;

    GUIStyle stTitolo, stSottotitolo, stVoce, stDescrizione, stPiccolo;
    Texture2D texNero, texSfumatura;

    static readonly string[] chiaviClassi = { "classe.guerriero", "classe.ladro", "classe.stregone" };

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        volumeGenerale = PlayerPrefs.GetFloat(ChiaveVolume, 0.8f);
        volumeMusica = PlayerPrefs.GetFloat(ChiaveMusica, 0.7f);
        effettoRetro = PlayerPrefs.GetInt(ChiaveRetro, 1) == 1;
        ApplicaOpzioni();

        sorgente = GetComponent<AudioSource>();
        sorgente.playOnAwake = false;
        sorgente.loop = true;
        sorgente.spatialBlend = 0f;
        sorgente.volume = 0f;
        if (musica != null)
        {
            sorgente.clip = musica;
            sorgente.Play();
        }

        texNero = new Texture2D(1, 1);
        texNero.SetPixel(0, 0, Color.black);
        texNero.Apply();
        texSfumatura = new Texture2D(1, 64) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < 64; y++)
            texSfumatura.SetPixel(0, y, new Color(0f, 0f, 0f, Mathf.Lerp(0.85f, 0f, y / 63f)));
        texSfumatura.Apply();

        VaiA(Schermata.Titolo);
    }

    void OnDestroy()
    {
        if (texNero != null) Destroy(texNero);
        if (texSfumatura != null) Destroy(texSfumatura);
    }

    // ---------- schermate ----------

    void VaiA(Schermata nuova)
    {
        schermata = nuova;
        tempoSchermata = 0f;
        voci.Clear();
        selezione = 0;

        switch (nuova)
        {
            case Schermata.Principale:
                Aggiungi(() => Lingua.T("menu.nuova"), () => VaiA(Schermata.Classe));
                voci.Add(new Voce { testo = () => Lingua.T("menu.continua"), attiva = false });
                Aggiungi(() => Lingua.T("menu.opzioni"), () => VaiA(Schermata.Opzioni));
                Aggiungi(() => Lingua.T("menu.crediti"), () => VaiA(Schermata.Crediti));
                Aggiungi(() => Lingua.T("menu.esci"), Esci);
                break;

            case Schermata.Classe:
                for (int i = 0; i < chiaviClassi.Length; i++)
                {
                    int indice = i;
                    Aggiungi(() => Lingua.T(chiaviClassi[indice]), () => IniziaPartita((ClasseGiocatore)indice));
                }
                Aggiungi(() => Lingua.T("menu.indietro"), () => VaiA(Schermata.Principale));
                selezione = Mathf.Clamp((int)SceltaPartita.Classe, 0, chiaviClassi.Length - 1);
                break;

            case Schermata.Opzioni:
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.lingua") + "   ◄ " + Lingua.NomeAttuale + " ►",
                    conferma = () => Lingua.Indice = Lingua.Indice + 1,
                    regola = d => Lingua.Indice = Lingua.Indice + d,
                });
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.volume_generale") + "   ◄ " + Mathf.RoundToInt(volumeGenerale * 100) + "% ►",
                    conferma = () => CambiaVolume(ref volumeGenerale, 1, true),
                    regola = d => CambiaVolume(ref volumeGenerale, d, false),
                });
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.volume_musica") + "   ◄ " + Mathf.RoundToInt(volumeMusica * 100) + "% ►",
                    conferma = () => CambiaVolume(ref volumeMusica, 1, true),
                    regola = d => CambiaVolume(ref volumeMusica, d, false),
                });
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.schermo_intero") + ":  " + SiNo(Screen.fullScreen),
                    conferma = () => Screen.fullScreen = !Screen.fullScreen,
                    regola = d => Screen.fullScreen = !Screen.fullScreen,
                });
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.effetto_retro") + ":  " + SiNo(effettoRetro),
                    conferma = () => { effettoRetro = !effettoRetro; ApplicaOpzioni(); },
                    regola = d => { effettoRetro = !effettoRetro; ApplicaOpzioni(); },
                });
                Aggiungi(() => Lingua.T("menu.indietro"), () => VaiA(Schermata.Principale));
                break;

            case Schermata.Crediti:
                Aggiungi(() => Lingua.T("menu.indietro"), () => VaiA(Schermata.Principale));
                break;
        }
    }

    static string SiNo(bool valore) => Lingua.T(valore ? "comune.si" : "comune.no");

    void Aggiungi(Func<string> testo, Action conferma)
    {
        voci.Add(new Voce { testo = testo, conferma = conferma });
    }

    void Indietro()
    {
        switch (schermata)
        {
            case Schermata.Principale: VaiA(Schermata.Titolo); break;
            case Schermata.Classe:
            case Schermata.Opzioni:
            case Schermata.Crediti: VaiA(Schermata.Principale); break;
        }
    }

    // ---------- azioni ----------

    void CambiaVolume(ref float volume, int direzione, bool giraIntorno)
    {
        float nuovo = Mathf.Round((volume + 0.1f * direzione) * 10f) / 10f;
        if (giraIntorno && nuovo > 1.001f) nuovo = 0f;
        volume = Mathf.Clamp01(nuovo);
        ApplicaOpzioni();
    }

    void ApplicaOpzioni()
    {
        AudioListener.volume = volumeGenerale;
        PlayerPrefs.SetFloat(ChiaveVolume, volumeGenerale);
        PlayerPrefs.SetFloat(ChiaveMusica, volumeMusica);
        PlayerPrefs.SetInt(ChiaveRetro, effettoRetro ? 1 : 0);
        PlayerPrefs.Save();
        foreach (var retro in FindObjectsByType<EffettoRetro>(FindObjectsSortMode.None))
            retro.attivo = effettoRetro;
    }

    void IniziaPartita(ClasseGiocatore classe)
    {
        if (avvioInCorso) return;
        string scena = null;
        if (!string.IsNullOrEmpty(scenaIniziale) && Application.CanStreamedLevelBeLoaded(scenaIniziale)) scena = scenaIniziale;
        else if (!string.IsNullOrEmpty(scenaRiserva) && Application.CanStreamedLevelBeLoaded(scenaRiserva)) scena = scenaRiserva;

        if (scena == null)
        {
            Avvisa(Lingua.T("menu.nessuna_scena") + " (" + scenaIniziale + " / " + scenaRiserva + ")");
            return;
        }
        SceltaPartita.Classe = classe;
        scenaDaCaricare = scena;
        avvioInCorso = true;
    }

    void Esci()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Avvisa(string testo)
    {
        avviso = testo;
        tempoAvviso = 4f;
        Debug.LogWarning("[Menu] " + testo);
    }

    // ---------- comandi ----------

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        tempoSchermata += dt;
        if (tempoAvviso > 0f) tempoAvviso -= dt;

        // velo nero: si schiarisce all'inizio, si scurisce quando parte la partita
        nero = Mathf.MoveTowards(nero, avvioInCorso ? 1f : 0f, dt / (avvioInCorso ? 1.5f : 2.5f));

        // musica
        float obiettivo = avvioInCorso ? 0f : 1f;
        livelloMusica = Mathf.MoveTowards(livelloMusica, obiettivo, dt / Mathf.Max(0.1f, dissolvenzaMusica));
        if (sorgente != null) sorgente.volume = livelloMusica * volumeMusica;

        if (avvioInCorso)
        {
            if (nero >= 1f && livelloMusica <= 0.05f) SceneManager.LoadScene(scenaDaCaricare);
            return;
        }

        var tastiera = Keyboard.current;
        var pad = Gamepad.current;
        var mouse = Mouse.current;

        if (schermata == Schermata.Titolo)
        {
            bool premuto = (tastiera != null && tastiera.anyKey.wasPressedThisFrame)
                || (mouse != null && mouse.leftButton.wasPressedThisFrame)
                || (pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame));
            if (premuto && tempoSchermata > 0.5f) VaiA(Schermata.Principale);
            return;
        }

        // crediti: Spazio (o A del pad) li salta e torna al menu
        if (schermata == Schermata.Crediti &&
            ((tastiera != null && tastiera.spaceKey.wasPressedThisFrame) || (pad != null && pad.buttonSouth.wasPressedThisFrame)))
        {
            VaiA(Schermata.Principale);
            return;
        }

        int verticale = 0, orizzontale = 0;
        bool conferma = false, indietro = false;

        if (tastiera != null)
        {
            if (tastiera.upArrowKey.wasPressedThisFrame || tastiera.wKey.wasPressedThisFrame) verticale = -1;
            if (tastiera.downArrowKey.wasPressedThisFrame || tastiera.sKey.wasPressedThisFrame) verticale = 1;
            if (tastiera.leftArrowKey.wasPressedThisFrame || tastiera.aKey.wasPressedThisFrame) orizzontale = -1;
            if (tastiera.rightArrowKey.wasPressedThisFrame || tastiera.dKey.wasPressedThisFrame) orizzontale = 1;
            conferma |= tastiera.enterKey.wasPressedThisFrame || tastiera.numpadEnterKey.wasPressedThisFrame || tastiera.spaceKey.wasPressedThisFrame;
            indietro |= tastiera.escapeKey.wasPressedThisFrame || tastiera.backspaceKey.wasPressedThisFrame;
        }
        if (pad != null)
        {
            if (pad.dpad.up.wasPressedThisFrame) verticale = -1;
            if (pad.dpad.down.wasPressedThisFrame) verticale = 1;
            if (pad.dpad.left.wasPressedThisFrame) orizzontale = -1;
            if (pad.dpad.right.wasPressedThisFrame) orizzontale = 1;
            conferma |= pad.buttonSouth.wasPressedThisFrame;
            indietro |= pad.buttonEast.wasPressedThisFrame;

            // levetta sinistra, con ripetizione se resta inclinata
            Vector2 leva = pad.leftStick.ReadValue();
            if (leva.magnitude < 0.5f) prossimoScatto = 0f;
            else if (Time.unscaledTime >= prossimoScatto)
            {
                if (Mathf.Abs(leva.y) > Mathf.Abs(leva.x)) verticale = leva.y > 0 ? -1 : 1;
                else orizzontale = leva.x > 0 ? 1 : -1;
                prossimoScatto = Time.unscaledTime + (prossimoScatto == 0f ? 0.35f : 0.15f);
            }
        }

        if (verticale != 0) Sposta(verticale);
        if (orizzontale != 0 && Selezionata()?.regola != null) Selezionata().regola(orizzontale);
        if (conferma) Conferma(selezione);
        else if (indietro) Indietro();
    }

    Voce Selezionata() => selezione >= 0 && selezione < voci.Count ? voci[selezione] : null;

    void Sposta(int direzione)
    {
        if (voci.Count == 0) return;
        for (int i = 0; i < voci.Count; i++)
        {
            selezione = (selezione + direzione + voci.Count) % voci.Count;
            if (voci[selezione].attiva) return;
        }
    }

    void Conferma(int indice)
    {
        if (indice < 0 || indice >= voci.Count) return;
        var voce = voci[indice];
        if (voce.attiva && voce.conferma != null) voce.conferma();
    }

    // ---------- disegno ----------

    void PreparaStili()
    {
        if (stTitolo != null) return;
        stTitolo = Stile(108, FontStyle.Bold);
        stSottotitolo = Stile(26, FontStyle.Italic);
        stVoce = Stile(40, FontStyle.Normal);
        stDescrizione = Stile(28, FontStyle.Italic);
        stDescrizione.wordWrap = true;
        stPiccolo = Stile(20, FontStyle.Normal);
    }

    GUIStyle Stile(int dimensione, FontStyle tipo)
    {
        var s = new GUIStyle(GUI.skin.label)
        {
            fontSize = dimensione,
            fontStyle = tipo,
            alignment = TextAnchor.MiddleCenter,
            richText = false,
            clipping = TextClipping.Overflow,
        };
        s.normal.textColor = Color.white;
        if (carattere != null) s.font = carattere;
        return s;
    }

    void OnGUI()
    {
        PreparaStili();

        // sfumatura scura in basso, per leggere meglio le scritte
        GUI.matrix = Matrix4x4.identity;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(0, Screen.height * 0.35f, Screen.width, Screen.height * 0.65f), texSfumatura, ScaleMode.StretchToFill);

        // tutto il resto è disegnato su un foglio virtuale 1920x1080, ingrandito per lo schermo
        float s = Screen.height / Altezza;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - Larghezza * s) * 0.5f, 0f, 0f), Quaternion.identity, new Vector3(s, s, 1f));

        float comparsa = Mathf.Clamp01(tempoSchermata / 0.6f);

        // titolo
        float altoTitolo = schermata == Schermata.Titolo ? 360f : 190f;
        Scritta(new Rect(0, altoTitolo, Larghezza, 140), Spaziato(titolo.ToUpperInvariant()), stTitolo, coloreTesto, 1f);
        if (nomeProvvisorio)
            Scritta(new Rect(0, altoTitolo + 120, Larghezza, 40), Lingua.T("menu.nome_provvisorio"), stSottotitolo, coloreSpento, 1f);

        switch (schermata)
        {
            case Schermata.Titolo:
                float pulsa = 0.45f + 0.45f * Mathf.Sin(Time.unscaledTime * 2.2f);
                Scritta(new Rect(0, 760, Larghezza, 50), Lingua.T("menu.premi"), stVoce, coloreTesto, pulsa * comparsa);
                break;

            case Schermata.Classe:
                Scritta(new Rect(0, 400, Larghezza, 50), Lingua.T("classe.scegli"), stSottotitolo, coloreTesto, comparsa);
                DisegnaVoci(500f, comparsa);
                if (selezione < chiaviClassi.Length)
                    Scritta(new Rect(360, 860, Larghezza - 720, 80), Lingua.T(chiaviClassi[selezione] + ".descrizione"), stDescrizione, coloreTesto, comparsa);
                break;

            case Schermata.Crediti:
                DisegnaCrediti(comparsa);
                DisegnaVoci(930f, comparsa);
                break;

            default:
                DisegnaVoci(470f, comparsa);
                break;
        }

        if (schermata != Schermata.Titolo)
            Scritta(new Rect(0, 1010, Larghezza, 30),
                schermata == Schermata.Crediti ? Lingua.T("menu.salta") : Lingua.T("menu.aiuto"),
                stPiccolo, coloreSpento, 0.8f * comparsa);

        if (tempoAvviso > 0f && !string.IsNullOrEmpty(avviso))
            Scritta(new Rect(0, 960, Larghezza, 40), avviso, stPiccolo, coloreSelezione, Mathf.Clamp01(tempoAvviso));

        // velo nero per le dissolvenze
        if (nero > 0.001f)
        {
            GUI.matrix = Matrix4x4.identity;
            GUI.color = new Color(1f, 1f, 1f, nero);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texNero);
            GUI.color = Color.white;
        }
    }

    // Crediti che scorrono dal basso verso l'alto dentro una fascia dello schermo, e ricominciano alla fine.
    void DisegnaCrediti(float comparsa)
    {
        const float alto = 380f, altezzaFascia = 520f, passo = 104f, velocita = 55f;
        int n = TestiCrediti.Elenco.Length;
        float lunghezza = altezzaFascia + 140f + n * passo;
        float scorrimento = (tempoSchermata * velocita) % lunghezza;

        GUI.BeginGroup(new Rect(0f, alto, Larghezza, altezzaFascia));
        float y = altezzaFascia - scorrimento;
        Riga(ref y, Lingua.T("menu.crediti_testo"), null, altezzaFascia, comparsa, 140f);
        for (int i = 0; i < n; i++)
            Riga(ref y, TestiCrediti.Ruolo(i), TestiCrediti.Elenco[i].nomi, altezzaFascia, comparsa, passo);
        GUI.EndGroup();
    }

    void Riga(ref float y, string ruolo, string nomi, float altezzaFascia, float comparsa, float passo)
    {
        if (y > -passo && y < altezzaFascia)
        {
            // sfuma vicino ai bordi della fascia
            float centro = y + 30f;
            float alfa = Mathf.Clamp01(Mathf.Min(centro, altezzaFascia - centro) / 90f) * comparsa;
            if (string.IsNullOrEmpty(nomi))
                Scritta(new Rect(0, y, Larghezza, 60), ruolo, stVoce, coloreTesto, alfa);
            else
            {
                Scritta(new Rect(0, y, Larghezza, 34), ruolo, stSottotitolo, coloreSpento, alfa);
                Scritta(new Rect(0, y + 36, Larghezza, 46), nomi, stVoce, coloreTesto, alfa);
            }
        }
        y += passo;
    }

    void DisegnaVoci(float alto, float comparsa)
    {
        const float passo = 66f;
        var e = Event.current;
        // il mouse seleziona solo quando si muove davvero (così non ruba la scelta alla tastiera)
        bool mouseMosso = false;
        if (e.type == EventType.Repaint)
        {
            mouseMosso = (e.mousePosition - ultimoMouse).sqrMagnitude > 1f && ultimoMouse.x >= 0f;
            ultimoMouse = e.mousePosition;
            if (ultimoMouse.x < 0f) ultimoMouse = Vector2.zero;
        }
        for (int i = 0; i < voci.Count; i++)
        {
            var voce = voci[i];
            var area = new Rect(Larghezza * 0.5f - 420f, alto + i * passo, 840f, 56f);

            // mouse: passando sopra si seleziona, il clic conferma
            if (voce.attiva && area.Contains(e.mousePosition))
            {
                if (mouseMosso) selezione = i;
                if (e.type == EventType.MouseDown && e.button == 0 && !avvioInCorso)
                {
                    selezione = i;
                    Conferma(i);
                    e.Use();
                    return;
                }
            }

            bool scelta = i == selezione && voce.attiva;
            Color colore = !voce.attiva ? coloreSpento : scelta ? coloreSelezione : coloreTesto;
            string testo = voce.testo();
            if (scelta) testo = "—   " + testo + "   —";
            Scritta(area, testo, stVoce, colore, comparsa);
        }
    }

    void Scritta(Rect area, string testo, GUIStyle stile, Color colore, float alfa)
    {
        if (alfa <= 0.001f) return;
        var ombra = new Rect(area.x + 3, area.y + 3, area.width, area.height);
        GUI.color = new Color(0f, 0f, 0f, 0.7f * alfa);
        GUI.Label(ombra, testo, stile);
        GUI.color = new Color(colore.r, colore.g, colore.b, colore.a * alfa);
        GUI.Label(area, testo, stile);
        GUI.color = Color.white;
    }

    static string Spaziato(string testo)
    {
        var parti = new System.Text.StringBuilder();
        for (int i = 0; i < testo.Length; i++)
        {
            parti.Append(testo[i]);
            if (i < testo.Length - 1) parti.Append(testo[i] == ' ' ? "  " : " ");
        }
        return parti.ToString();
    }
}
