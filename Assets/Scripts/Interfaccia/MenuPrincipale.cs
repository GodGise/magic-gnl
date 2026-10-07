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
// Stile dark fantasy: titolo inciso con caratteri romani (Cinzel), testi da vecchio libro (IM Fell),
// riquadri scuri con doppia cornice color bronzo e rombi agli angoli, braci che salgono dal basso,
// bordi dello schermo scuri. I caratteri sono in Assets/Resources/Caratteri (licenza OFL, gratuita, file
// di licenza accanto). Per russo e cinese si usa il carattere di base, perché quei due non hanno le lettere.
// Come montarlo: su un oggetto vuoto della scena Menu. Il menu "magic-gnl > Crea scena menu" prepara tutto da solo.
[RequireComponent(typeof(AudioSource))]
public class MenuPrincipale : MonoBehaviour
{
    [Header("Testi")]
    [SerializeField] string titolo = "magic-GNL";
    [Tooltip("Mostra la scritta \"nome provvisorio\" sotto il titolo, finché il nome del gioco non è deciso.")]
    [SerializeField] bool nomeProvvisorio = true;

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
    [SerializeField] Color coloreBronzo = new Color(0.55f, 0.44f, 0.28f);
    [SerializeField] Color coloreRiquadro = new Color(0.025f, 0.025f, 0.035f, 0.82f);
    [Tooltip("Quante braci salgono dal basso dello schermo.")]
    [SerializeField] int numeroBraci = 46;

    enum Schermata { Titolo, Principale, Classe, Opzioni, Crediti }

    class Voce
    {
        public Func<string> testo;
        public Func<string> valore;  // solo nelle opzioni: il valore mostrato a destra
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

    GUIStyle stTitolo, stSottotitolo, stVoce, stVoceSinistra, stDescrizione, stPiccolo, stEmblema, stNomeClasse;
    Texture2D texNero, texSfumatura, texPixel, texVignetta, texBagliore, texFascia;
    Font fontTitolo, fontVoce, fontLibro, fontLibroCorsivo;
    int linguaStili = -1;
    int ultimaClasse;

    struct Brace { public Vector2 pos, vel; public float vita, durata, taglia; }
    Brace[] braci;
    System.Random caso = new System.Random();

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
        texPixel = new Texture2D(1, 1);
        texPixel.SetPixel(0, 0, Color.white);
        texPixel.Apply();
        texVignetta = Radiale(128, 0.45f, 1.35f, 0f, 0.92f, Color.black);
        texBagliore = Radiale(64, 0f, 1f, 1f, 0f, Color.white);
        texFascia = new Texture2D(128, 1) { wrapMode = TextureWrapMode.Clamp };
        for (int x = 0; x < 128; x++)
        {
            float t = 1f - Mathf.Abs(x / 127f * 2f - 1f);
            texFascia.SetPixel(x, 0, new Color(1f, 1f, 1f, Mathf.SmoothStep(0f, 1f, t)));
        }
        texFascia.Apply();

        fontTitolo = Resources.Load<Font>("Caratteri/Cinzel-Bold");
        fontVoce = Resources.Load<Font>("Caratteri/Cinzel-Regular");
        fontLibro = Resources.Load<Font>("Caratteri/IMFeENrm28P");
        fontLibroCorsivo = Resources.Load<Font>("Caratteri/IMFeENit28P");

        braci = new Brace[Mathf.Max(0, numeroBraci)];
        for (int i = 0; i < braci.Length; i++) NuovaBrace(ref braci[i], true);

        VaiA(Schermata.Titolo);
    }

    void OnDestroy()
    {
        if (texNero != null) Destroy(texNero);
        if (texSfumatura != null) Destroy(texSfumatura);
        if (texPixel != null) Destroy(texPixel);
        if (texVignetta != null) Destroy(texVignetta);
        if (texBagliore != null) Destroy(texBagliore);
        if (texFascia != null) Destroy(texFascia);
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
                ultimaClasse = selezione;
                break;

            case Schermata.Opzioni:
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.lingua"),
                    valore = () => Lingua.NomeAttuale,
                    conferma = () => Lingua.Indice = Lingua.Indice + 1,
                    regola = d => Lingua.Indice = Lingua.Indice + d,
                });
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.volume_generale"),
                    valore = () => Mathf.RoundToInt(volumeGenerale * 100) + "%",
                    conferma = () => CambiaVolume(ref volumeGenerale, 1, true),
                    regola = d => CambiaVolume(ref volumeGenerale, d, false),
                });
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.volume_musica"),
                    valore = () => Mathf.RoundToInt(volumeMusica * 100) + "%",
                    conferma = () => CambiaVolume(ref volumeMusica, 1, true),
                    regola = d => CambiaVolume(ref volumeMusica, d, false),
                });
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.schermo_intero"),
                    valore = () => SiNo(Screen.fullScreen),
                    conferma = () => Screen.fullScreen = !Screen.fullScreen,
                    regola = d => Screen.fullScreen = !Screen.fullScreen,
                });
                voci.Add(new Voce
                {
                    testo = () => Lingua.T("opzioni.effetto_retro"),
                    valore = () => SiNo(effettoRetro),
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
        AggiornaBraci(dt);
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

        // scelta della classe: tre riquadri affiancati (sinistra e destra), "Indietro" sotto (su e giù)
        if (schermata == Schermata.Classe)
        {
            int classi = chiaviClassi.Length;
            if (orizzontale != 0 && selezione < classi)
            {
                selezione = (selezione + orizzontale + classi) % classi;
                ultimaClasse = selezione;
            }
            if (verticale != 0) selezione = selezione < classi ? classi : ultimaClasse;
            orizzontale = verticale = 0;
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

    // ---------- braci ----------

    void NuovaBrace(ref Brace b, bool ovunque)
    {
        float Caso() => (float)caso.NextDouble();
        b.pos = new Vector2(Caso() * Larghezza, ovunque ? Caso() * Altezza : Altezza + 20f);
        b.vel = new Vector2((Caso() - 0.5f) * 30f, -(30f + Caso() * 70f));
        b.durata = 4f + Caso() * 6f;
        b.vita = ovunque ? Caso() * b.durata : 0f;
        b.taglia = 3f + Caso() * 6f;
    }

    void AggiornaBraci(float dt)
    {
        if (braci == null) return;
        for (int i = 0; i < braci.Length; i++)
        {
            ref Brace b = ref braci[i];
            b.vita += dt;
            b.pos += b.vel * dt;
            b.pos.x += Mathf.Sin((Time.unscaledTime + i) * 1.3f) * 12f * dt;
            if (b.vita >= b.durata || b.pos.y < -20f) NuovaBrace(ref b, false);
        }
    }

    void DisegnaBraci()
    {
        if (braci == null) return;
        foreach (var b in braci)
        {
            float t = Mathf.Clamp01(b.vita / b.durata);
            float alfa = Mathf.Sin(t * Mathf.PI) * 0.75f;
            GUI.color = new Color(1f, 0.5f + 0.25f * (1f - t), 0.18f, alfa);
            GUI.DrawTexture(new Rect(b.pos.x - b.taglia, b.pos.y - b.taglia, b.taglia * 2f, b.taglia * 2f), texBagliore);
        }
        GUI.color = Color.white;
    }

    // ---------- stili e trame ----------

    static Texture2D Radiale(int lato, float inizio, float fine, float alfaCentro, float alfaBordo, Color colore)
    {
        var t = new Texture2D(lato, lato) { wrapMode = TextureWrapMode.Clamp };
        float c = (lato - 1) * 0.5f;
        for (int y = 0; y < lato; y++)
            for (int x = 0; x < lato; x++)
            {
                float dx = (x - c) / c, dy = (y - c) / c;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float k = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(inizio, fine, d));
                t.SetPixel(x, y, new Color(colore.r, colore.g, colore.b, Mathf.Lerp(alfaCentro, alfaBordo, k)));
            }
        t.Apply();
        return t;
    }

    void PreparaStili()
    {
        int lingua = Lingua.Indice;
        if (stTitolo != null && lingua == linguaStili) return;
        linguaStili = lingua;

        // russo (6) e cinese (7): i caratteri antichi non hanno quelle lettere, si usa quello di base
        bool latino = lingua < 6;
        Font voce = latino ? fontVoce : null;
        Font libro = latino ? fontLibro : null;
        Font corsivo = latino ? fontLibroCorsivo : null;

        stTitolo = Stile(108, fontTitolo != null ? FontStyle.Normal : FontStyle.Bold, fontTitolo);
        stEmblema = Stile(92, fontTitolo != null ? FontStyle.Normal : FontStyle.Bold, fontTitolo);
        stNomeClasse = Stile(38, latino && fontTitolo != null ? FontStyle.Normal : FontStyle.Bold, latino ? fontTitolo : null);
        stSottotitolo = Stile(30, corsivo != null ? FontStyle.Normal : FontStyle.Italic, corsivo);
        stVoce = Stile(36, FontStyle.Normal, voce);
        stVoceSinistra = Stile(32, FontStyle.Normal, voce);
        stVoceSinistra.alignment = TextAnchor.MiddleLeft;
        stDescrizione = Stile(29, FontStyle.Normal, libro);
        stDescrizione.wordWrap = true;
        stDescrizione.alignment = TextAnchor.UpperCenter;
        stPiccolo = Stile(20, FontStyle.Normal, null);
    }

    GUIStyle Stile(int dimensione, FontStyle tipo, Font font)
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
        if (font != null) s.font = font;
        return s;
    }

    // ---------- disegno ----------

    bool mouseMosso;

    void OnGUI()
    {
        PreparaStili();

        // bordi scuri e sfumatura in basso, su tutto lo schermo
        GUI.matrix = Matrix4x4.identity;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texVignetta, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(0, Screen.height * 0.45f, Screen.width, Screen.height * 0.55f), texSfumatura, ScaleMode.StretchToFill);

        // tutto il resto è disegnato su un foglio virtuale 1920x1080, ingrandito per lo schermo
        float s = Screen.height / Altezza;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - Larghezza * s) * 0.5f, 0f, 0f), Quaternion.identity, new Vector3(s, s, 1f));

        var e = Event.current;
        mouseMosso = false;
        if (e.type == EventType.Repaint)
        {
            mouseMosso = (e.mousePosition - ultimoMouse).sqrMagnitude > 1f && ultimoMouse.x >= 0f;
            ultimoMouse = e.mousePosition;
            if (ultimoMouse.x < 0f) ultimoMouse = Vector2.zero;
        }

        DisegnaBraci();

        float comparsa = Mathf.Clamp01(tempoSchermata / 0.6f);
        bool schermoTitolo = schermata == Schermata.Titolo;

        // titolo, con un alone caldo dietro e il divisore sotto
        float altoTitolo = schermoTitolo ? 290f : 110f;
        GUI.color = new Color(1f, 0.5f, 0.2f, 0.08f);
        GUI.DrawTexture(new Rect(Larghezza * 0.5f - 720f, altoTitolo - 130f, 1440f, 400f), texBagliore);
        GUI.color = Color.white;
        Scritta(new Rect(0, altoTitolo, Larghezza, 150), Spaziato(titolo.ToUpperInvariant()), stTitolo, coloreTesto, 1f);
        Divisore(Larghezza * 0.5f, altoTitolo + 162f, 640f, 1f);
        if (nomeProvvisorio)
            Scritta(new Rect(0, altoTitolo + 180, Larghezza, 40), Lingua.T("menu.nome_provvisorio"), stSottotitolo, coloreSpento, 1f);

        switch (schermata)
        {
            case Schermata.Titolo:
            {
                float pulsa = 0.4f + 0.5f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.2f));
                string premi = Lingua.T("menu.premi");
                var area = new Rect(0, 760, Larghezza, 60);
                Scritta(area, premi, stVoce, coloreTesto, pulsa * comparsa);
                float w = stVoce.CalcSize(new GUIContent(premi)).x;
                Rombo(new Vector2(Larghezza * 0.5f - w * 0.5f - 34f, area.center.y), 10f, Con(coloreBronzo, pulsa * comparsa));
                Rombo(new Vector2(Larghezza * 0.5f + w * 0.5f + 34f, area.center.y), 10f, Con(coloreBronzo, pulsa * comparsa));
                break;
            }
            case Schermata.Principale:
                DisegnaElenco(380f, 600f, comparsa);
                break;
            case Schermata.Opzioni:
                DisegnaOpzioni(370f, comparsa);
                break;
            case Schermata.Classe:
                DisegnaClassi(comparsa);
                break;
            case Schermata.Crediti:
            {
                var fascia = new Rect(Larghezza * 0.5f - 560f, 365f, 1120f, 520f);
                Cornice(fascia, comparsa, false);
                DisegnaCrediti(new Rect(fascia.x + 12f, fascia.y + 12f, fascia.width - 24f, fascia.height - 24f), comparsa);
                if (voci.Count > 0)
                {
                    var area = new Rect(Larghezza * 0.5f - 220f, 910f, 440f, 60f);
                    if (Mouse(area, 0)) return;
                    VoceCentrata(area, voci[0], selezione == 0, comparsa);
                }
                break;
            }
        }

        if (!schermoTitolo)
            Scritta(new Rect(0, 1030, Larghezza, 30),
                schermata == Schermata.Crediti ? Lingua.T("menu.salta") : Lingua.T("menu.aiuto"),
                stPiccolo, coloreSpento, 0.8f * comparsa);

        if (tempoAvviso > 0f && !string.IsNullOrEmpty(avviso))
            Scritta(new Rect(0, 985, Larghezza, 40), avviso, stPiccolo, coloreSelezione, Mathf.Clamp01(tempoAvviso));

        // velo nero per le dissolvenze
        if (nero > 0.001f)
        {
            GUI.matrix = Matrix4x4.identity;
            GUI.color = new Color(1f, 1f, 1f, nero);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texNero);
            GUI.color = Color.white;
        }
    }

    // Mouse sopra una voce: passandoci la seleziona, il clic la conferma. Restituisce true se ha cliccato
    // (la schermata può essere cambiata: chi chiama deve smettere di disegnare).
    bool Mouse(Rect area, int indice)
    {
        var e = Event.current;
        if (avvioInCorso || indice < 0 || indice >= voci.Count || !voci[indice].attiva || !area.Contains(e.mousePosition)) return false;
        if (mouseMosso) selezione = indice;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            selezione = indice;
            e.Use();
            Conferma(indice);
            return true;
        }
        return false;
    }

    // Menu principale: riquadro con le voci una sotto l'altra.
    void DisegnaElenco(float alto, float larghezza, float comparsa)
    {
        const float passo = 72f, margine = 38f;
        var riquadro = new Rect(Larghezza * 0.5f - larghezza * 0.5f, alto, larghezza, voci.Count * passo + margine * 2f - 12f);
        Cornice(riquadro, comparsa, false);
        for (int i = 0; i < voci.Count; i++)
        {
            var area = new Rect(riquadro.x + 24f, alto + margine + i * passo, larghezza - 48f, 60f);
            if (Mouse(area, i)) return;
            VoceCentrata(area, voci[i], i == selezione, comparsa);
        }
    }

    void VoceCentrata(Rect area, Voce voce, bool scelta, float alfa)
    {
        scelta &= voce.attiva;
        string testo = voce.testo();
        if (scelta)
        {
            GUI.color = Con(coloreSelezione, 0.2f * alfa);
            GUI.DrawTexture(new Rect(area.x - 20f, area.y + 4f, area.width + 40f, area.height - 8f), texFascia, ScaleMode.StretchToFill);
            GUI.color = Color.white;
            float w = stVoce.CalcSize(new GUIContent(testo)).x;
            Rombo(new Vector2(area.center.x - w * 0.5f - 30f, area.center.y), 10f, Con(coloreSelezione, alfa));
            Rombo(new Vector2(area.center.x + w * 0.5f + 30f, area.center.y), 10f, Con(coloreSelezione, alfa));
        }
        Color colore = !voce.attiva ? coloreSpento : scelta ? coloreSelezione : coloreTesto;
        Scritta(area, testo, stVoce, colore, alfa);
    }

    // Opzioni: nome a sinistra, valore a destra fra due frecce (cliccabili).
    void DisegnaOpzioni(float alto, float comparsa)
    {
        const float larghezza = 1000f, passo = 74f, margine = 38f;
        var riquadro = new Rect(Larghezza * 0.5f - larghezza * 0.5f, alto, larghezza, voci.Count * passo + margine * 2f - 12f);
        Cornice(riquadro, comparsa, false);
        var e = Event.current;

        for (int i = 0; i < voci.Count; i++)
        {
            var voce = voci[i];
            var area = new Rect(riquadro.x + 24f, alto + margine + i * passo, larghezza - 48f, 60f);

            if (voce.valore == null)
            {
                if (Mouse(area, i)) return;
                VoceCentrata(area, voce, i == selezione, comparsa);
                continue;
            }

            float centroValore = area.xMax - 200f;
            var frecciaSinistra = new Rect(centroValore - 175f, area.y, 50f, area.height);
            var frecciaDestra = new Rect(centroValore + 125f, area.y, 50f, area.height);
            if (!avvioInCorso && e.type == EventType.MouseDown && e.button == 0 && voce.regola != null &&
                (frecciaSinistra.Contains(e.mousePosition) || frecciaDestra.Contains(e.mousePosition)))
            {
                selezione = i;
                voce.regola(frecciaSinistra.Contains(e.mousePosition) ? -1 : 1);
                e.Use();
                return;
            }
            if (Mouse(area, i)) return;

            bool scelta = i == selezione;
            if (scelta)
            {
                GUI.color = Con(coloreSelezione, 0.16f * comparsa);
                GUI.DrawTexture(new Rect(area.x - 10f, area.y + 4f, area.width + 20f, area.height - 8f), texFascia, ScaleMode.StretchToFill);
                GUI.color = Color.white;
                Rombo(new Vector2(area.x + 18f, area.center.y), 9f, Con(coloreSelezione, comparsa));
            }
            Color colore = scelta ? coloreSelezione : coloreTesto;
            Scritta(new Rect(area.x + 44f, area.y, area.width * 0.6f, area.height), voce.testo(), stVoceSinistra, colore, comparsa);
            Scritta(new Rect(centroValore - 125f, area.y, 250f, area.height), voce.valore(), stVoce, colore, comparsa);
            Color frecce = scelta ? coloreSelezione : coloreBronzo;
            Scritta(frecciaSinistra, "‹", stVoce, frecce, comparsa);
            Scritta(frecciaDestra, "›", stVoce, frecce, comparsa);
        }
    }

    // Scelta della classe: tre riquadri affiancati con numero romano, nome e descrizione; sotto "Indietro".
    void DisegnaClassi(float comparsa)
    {
        Scritta(new Rect(0, 348, Larghezza, 44), Lingua.T("classe.scegli"), stSottotitolo, coloreTesto, comparsa);

        const float larghezza = 440f, altezza = 440f, spazio = 50f, alto = 420f;
        string[] numeri = { "I", "II", "III" };
        int classi = chiaviClassi.Length;
        float inizio = Larghezza * 0.5f - (classi * larghezza + (classi - 1) * spazio) * 0.5f;

        for (int i = 0; i < classi; i++)
        {
            bool scelta = i == selezione;
            var r = new Rect(inizio + i * (larghezza + spazio), alto - (scelta ? 10f : 0f), larghezza, altezza);
            if (Mouse(r, i)) return;

            if (scelta)
            {
                GUI.color = new Color(1f, 0.5f, 0.2f, 0.16f * comparsa);
                GUI.DrawTexture(new Rect(r.x - 90f, r.y - 90f, r.width + 180f, r.height + 180f), texBagliore);
                GUI.color = Color.white;
            }
            Cornice(r, comparsa, scelta);

            Color accento = scelta ? coloreSelezione : coloreBronzo;
            Scritta(new Rect(r.x, r.y + 26f, r.width, 110f), numeri[Mathf.Min(i, numeri.Length - 1)], stEmblema, accento, comparsa);
            Scritta(new Rect(r.x, r.y + 150f, r.width, 50f), Lingua.T(chiaviClassi[i]).ToUpperInvariant(), stNomeClasse,
                scelta ? coloreSelezione : coloreTesto, comparsa);
            Divisore(r.center.x, r.y + 222f, 260f, comparsa);
            Scritta(new Rect(r.x + 34f, r.y + 248f, r.width - 68f, r.height - 270f), Lingua.T(chiaviClassi[i] + ".descrizione"),
                stDescrizione, scelta ? coloreTesto : coloreSpento, comparsa);
        }

        if (voci.Count > classi)
        {
            var area = new Rect(Larghezza * 0.5f - 220f, 905f, 440f, 60f);
            if (Mouse(area, classi)) return;
            VoceCentrata(area, voci[classi], selezione == classi, comparsa);
        }
    }

    // Crediti che scorrono dal basso verso l'alto dentro la fascia, e ricominciano alla fine.
    void DisegnaCrediti(Rect fascia, float comparsa)
    {
        const float passo = 104f, velocita = 55f;
        int n = TestiCrediti.Elenco.Length;
        float lunghezza = fascia.height + 140f + n * passo;
        float scorrimento = (tempoSchermata * velocita) % lunghezza;

        GUI.BeginGroup(fascia);
        float y = fascia.height - scorrimento;
        Riga(ref y, Lingua.T("menu.crediti_testo"), null, fascia, comparsa, 140f);
        for (int i = 0; i < n; i++)
            Riga(ref y, TestiCrediti.Ruolo(i), TestiCrediti.Elenco[i].nomi, fascia, comparsa, passo);
        GUI.EndGroup();
    }

    void Riga(ref float y, string ruolo, string nomi, Rect fascia, float comparsa, float passo)
    {
        if (y > -passo && y < fascia.height)
        {
            // sfuma vicino ai bordi della fascia
            float centro = y + 30f;
            float alfa = Mathf.Clamp01(Mathf.Min(centro, fascia.height - centro) / 90f) * comparsa;
            if (string.IsNullOrEmpty(nomi))
                Scritta(new Rect(0, y, fascia.width, 60), ruolo, stVoce, coloreSelezione, alfa);
            else
            {
                Scritta(new Rect(0, y, fascia.width, 34), ruolo, stSottotitolo, coloreBronzo, alfa);
                Scritta(new Rect(0, y + 36, fascia.width, 46), nomi, stVoce, coloreTesto, alfa);
            }
        }
        y += passo;
    }

    // ---------- forme ----------

    static Color Con(Color c, float alfa) => new Color(c.r, c.g, c.b, c.a * alfa);

    void Riempi(Rect r, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(r, texPixel);
        GUI.color = Color.white;
    }

    // Quadratino ruotato di 45 gradi.
    void Rombo(Vector2 centro, float lato, Color c)
    {
        Matrix4x4 prima = GUI.matrix;
        GUI.matrix = prima * Matrix4x4.TRS(centro, Quaternion.Euler(0f, 0f, 45f), Vector3.one) * Matrix4x4.TRS(-centro, Quaternion.identity, Vector3.one);
        Riempi(new Rect(centro.x - lato * 0.5f, centro.y - lato * 0.5f, lato, lato), c);
        GUI.matrix = prima;
    }

    void Bordo(Rect r, float spessore, Color c)
    {
        Riempi(new Rect(r.x, r.y, r.width, spessore), c);
        Riempi(new Rect(r.x, r.yMax - spessore, r.width, spessore), c);
        Riempi(new Rect(r.x, r.y, spessore, r.height), c);
        Riempi(new Rect(r.xMax - spessore, r.y, spessore, r.height), c);
    }

    // Riquadro scuro con doppia cornice color bronzo, rombi agli angoli e al centro dei lati.
    void Cornice(Rect r, float alfa, bool acceso)
    {
        Color bordo = acceso ? coloreSelezione : coloreBronzo;
        Riempi(r, Con(coloreRiquadro, alfa));
        Bordo(r, 2f, Con(bordo, 0.9f * alfa));
        Bordo(new Rect(r.x + 7f, r.y + 7f, r.width - 14f, r.height - 14f), 1f, Con(bordo, 0.4f * alfa));

        Vector2[] angoli = { new Vector2(r.x, r.y), new Vector2(r.xMax, r.y), new Vector2(r.x, r.yMax), new Vector2(r.xMax, r.yMax) };
        foreach (var a in angoli)
        {
            Rombo(a, 14f, Con(bordo, alfa));
            Rombo(a, 6f, Con(coloreRiquadro, alfa));
        }
        Rombo(new Vector2(r.center.x, r.y), 10f, Con(bordo, alfa));
        Rombo(new Vector2(r.center.x, r.yMax), 10f, Con(bordo, alfa));
    }

    // Linea che sfuma ai lati, con un rombo al centro e due più piccoli accanto.
    void Divisore(float centroX, float y, float larghezza, float alfa)
    {
        GUI.color = Con(coloreBronzo, 0.9f * alfa);
        GUI.DrawTexture(new Rect(centroX - larghezza * 0.5f, y - 1f, larghezza, 2f), texFascia, ScaleMode.StretchToFill);
        GUI.color = Color.white;
        Rombo(new Vector2(centroX, y), 12f, Con(coloreBronzo, alfa));
        Rombo(new Vector2(centroX, y), 5f, Con(coloreRiquadro, alfa));
        Rombo(new Vector2(centroX - 24f, y), 6f, Con(coloreBronzo, alfa));
        Rombo(new Vector2(centroX + 24f, y), 6f, Con(coloreBronzo, alfa));
    }

    void Scritta(Rect area, string testo, GUIStyle stile, Color colore, float alfa)
    {
        if (alfa <= 0.001f) return;
        var ombra = new Rect(area.x + 3, area.y + 3, area.width, area.height);
        GUI.color = new Color(0f, 0f, 0f, 0.75f * alfa);
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
