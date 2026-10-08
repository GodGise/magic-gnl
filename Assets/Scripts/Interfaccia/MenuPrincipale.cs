using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Menu iniziale del gioco.
// Schermate: titolo ("premi un tasto"), menu principale (Nuova partita, Multigiocatore, Continua, Opzioni, Crediti, Esci),
// scelta della classe (Guerriero, Ladro, Stregone), opzioni (lingua, volumi, schermo intero, effetto retro) e crediti
// (provvisori, che scorrono e si saltano con Spazio: l'elenco è in TestiCrediti.cs).
// Tutti i testi passano da Lingua.T(...): si traducono nelle 8 lingue del gioco (vedi Lingua.cs).
// Si usa con mouse, tastiera (frecce o WASD, Invio, Esc) o pad (croce o levetta, A per confermare, B per tornare).
// Le opzioni restano salvate (Impostazioni.cs). La classe scelta va in SceltaPartita.Classe.
// "Nuova partita" carica la scena di gioco indicata in "Scena iniziale"; se non è nelle Build Settings
// usa la "Scena di riserva" (ZonaProva).
// Multigiocatore (co-op fino a 3, vedi ReteCoop): "Ospita una partita" (scelta della classe, poi la partita parte e
// gli amici possono entrare) oppure "Entra in una partita" (si scrive l'indirizzo dell'host con numeri e punti,
// Backspace cancella; poi la classe e il collegamento). Nella schermata Multigiocatore si vedono gli indirizzi di
// questo PC da dare agli amici. Se l'host chiude o la connessione cade, si torna qui con un avviso.
// Musica: trascinare un file audio nel campo "Musica". Parte piano, sale lentamente e si spegne quando inizia la partita.
// Stile dark fantasy (GraficaMenu.cs): titolo inciso, riquadri con cornice di bronzo, braci che salgono dal basso.
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

    [Header("Atmosfera")]
    [Tooltip("Quante braci salgono dal basso dello schermo.")]
    [SerializeField] int numeroBraci = 46;

    enum Schermata { Titolo, Principale, Classe, Opzioni, Crediti, Multigiocatore, Indirizzo, Collegamento }

    // Come parte la partita dopo la scelta della classe: da soli, ospitando gli amici, o entrando da un amico.
    enum Modo { DaSolo, Ospita, Entra }
    Modo modo = Modo.DaSolo;
    const string ChiaveIndirizzo = "UltimoIndirizzoHost";
    string indirizzo = "127.0.0.1";
    List<string> indirizziMiei = new List<string>();

    const float Larghezza = GraficaMenu.Larghezza, Altezza = GraficaMenu.Altezza;

    readonly ElencoMenu elenco = new ElencoMenu();
    Schermata schermata = Schermata.Titolo;
    float tempoSchermata;
    float nero = 1f;              // velo nero sopra tutto: 1 = schermo nero
    float livelloMusica;          // 0..1, per la dissolvenza
    bool avvioInCorso;
    string scenaDaCaricare;
    string avviso;
    float tempoAvviso;
    int ultimaClasse;
    AudioSource sorgente;

    struct Brace { public Vector2 pos, vel; public float vita, durata, taglia; }
    Brace[] braci;
    readonly System.Random caso = new System.Random();

    static readonly string[] chiaviClassi = { "classe.guerriero", "classe.ladro", "classe.stregone" };

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Impostazioni.Applica();

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

        braci = new Brace[Mathf.Max(0, numeroBraci)];
        for (int i = 0; i < braci.Length; i++) NuovaBrace(ref braci[i], true);

        indirizzo = PlayerPrefs.GetString(ChiaveIndirizzo, "127.0.0.1");
        if (Keyboard.current != null) Keyboard.current.onTextInput += TestoScritto;
        if (ReteCoop.Istanza != null) ReteCoop.Istanza.EsitoCollegamento += Collegato;

        // Tornati qui da una partita in rete finita male (host uscito, connessione caduta): si mostra il perché.
        if (!string.IsNullOrEmpty(ReteCoop.AvvisoPerMenu))
        {
            VaiA(Schermata.Multigiocatore);
            Avvisa(ReteCoop.AvvisoPerMenu);
            ReteCoop.AvvisoPerMenu = null;
            return;
        }
        VaiA(Schermata.Titolo);
    }

    void OnDestroy()
    {
        if (Keyboard.current != null) Keyboard.current.onTextInput -= TestoScritto;
        if (ReteCoop.Istanza != null) ReteCoop.Istanza.EsitoCollegamento -= Collegato;
    }

    // ---------- schermate ----------

    void VaiA(Schermata nuova)
    {
        schermata = nuova;
        tempoSchermata = 0f;
        elenco.Pulisci();

        switch (nuova)
        {
            case Schermata.Principale:
                elenco.Aggiungi(() => Lingua.T("menu.nuova"), () => { modo = Modo.DaSolo; VaiA(Schermata.Classe); });
                elenco.Aggiungi(() => Lingua.T("menu.multigiocatore"), () => VaiA(Schermata.Multigiocatore));
                elenco.Aggiungi(() => Lingua.T("menu.continua"), null).attiva = false;
                elenco.Aggiungi(() => Lingua.T("menu.opzioni"), () => VaiA(Schermata.Opzioni));
                elenco.Aggiungi(() => Lingua.T("menu.crediti"), () => VaiA(Schermata.Crediti));
                elenco.Aggiungi(() => Lingua.T("menu.esci"), Esci);
                break;

            case Schermata.Classe:
                for (int i = 0; i < chiaviClassi.Length; i++)
                {
                    int indice = i;
                    elenco.Aggiungi(() => Lingua.T(chiaviClassi[indice]), () => IniziaPartita((ClasseGiocatore)indice));
                }
                elenco.Aggiungi(() => Lingua.T("menu.indietro"), Indietro);
                elenco.selezione = Mathf.Clamp((int)SceltaPartita.Classe, 0, chiaviClassi.Length - 1);
                ultimaClasse = elenco.selezione;
                break;

            case Schermata.Opzioni:
                Impostazioni.AggiungiVoci(elenco, () => VaiA(Schermata.Principale));
                break;

            case Schermata.Crediti:
                elenco.Aggiungi(() => Lingua.T("menu.indietro"), () => VaiA(Schermata.Principale));
                break;

            case Schermata.Multigiocatore:
                indirizziMiei = ReteCoop.IndirizziLocali();
                elenco.Aggiungi(() => Lingua.T("rete.ospita"), () => { modo = Modo.Ospita; VaiA(Schermata.Classe); });
                elenco.Aggiungi(() => Lingua.T("rete.entra"), () => VaiA(Schermata.Indirizzo));
                elenco.Aggiungi(() => Lingua.T("menu.indietro"), () => VaiA(Schermata.Principale));
                break;

            case Schermata.Indirizzo:
                // La prima voce mostra l'indirizzo mentre lo si scrive (con il cursore che lampeggia).
                elenco.Aggiungi(() => Lingua.T("rete.indirizzo") + ":  " + indirizzo + (Mathf.Repeat(Time.unscaledTime, 1f) < 0.5f ? "_" : " "), ConfermaIndirizzo);
                elenco.Aggiungi(() => Lingua.T("rete.continua"), ConfermaIndirizzo);
                elenco.Aggiungi(() => Lingua.T("menu.indietro"), () => VaiA(Schermata.Multigiocatore));
                break;

            case Schermata.Collegamento:
                elenco.Aggiungi(() => Lingua.T("rete.annulla"), AnnullaCollegamento);
                break;
        }
    }

    void Indietro()
    {
        switch (schermata)
        {
            case Schermata.Principale: VaiA(Schermata.Titolo); break;
            case Schermata.Classe:
                if (modo == Modo.Ospita) VaiA(Schermata.Multigiocatore);
                else if (modo == Modo.Entra) VaiA(Schermata.Indirizzo);
                else VaiA(Schermata.Principale);
                break;
            case Schermata.Opzioni:
            case Schermata.Crediti:
            case Schermata.Multigiocatore: VaiA(Schermata.Principale); break;
            case Schermata.Indirizzo: VaiA(Schermata.Multigiocatore); break;
            case Schermata.Collegamento: AnnullaCollegamento(); break;
        }
    }

    // ---------- multigiocatore ----------

    // Lettere scritte dalla tastiera: nella schermata dell'indirizzo valgono solo numeri e punti.
    void TestoScritto(char c)
    {
        if (schermata != Schermata.Indirizzo || avvioInCorso) return;
        if ((char.IsDigit(c) || c == '.') && indirizzo.Length < 15) indirizzo += c;
    }

    void ConfermaIndirizzo()
    {
        if (string.IsNullOrWhiteSpace(indirizzo)) indirizzo = "127.0.0.1";
        PlayerPrefs.SetString(ChiaveIndirizzo, indirizzo);
        PlayerPrefs.Save();
        modo = Modo.Entra;
        VaiA(Schermata.Classe);
    }

    void AnnullaCollegamento()
    {
        // Se la scena dell'host si sta già caricando non si può più annullare: si aspetta.
        if (ReteCoop.Istanza != null && ReteCoop.Istanza.StaSincronizzando) return;
        if (ReteCoop.Istanza != null) ReteCoop.Istanza.AnnullaEntrata();
        VaiA(Schermata.Indirizzo);
    }

    // Risposta al tentativo di entrare: collegati (la scena dell'host si carica da sola) oppure no.
    void Collegato(bool riuscito, string motivo)
    {
        if (schermata != Schermata.Collegamento) return;
        if (riuscito)
        {
            scenaDaCaricare = null;   // la carica Netcode, scelta dall'host
            avvioInCorso = true;
            return;
        }
        VaiA(Schermata.Indirizzo);
        Avvisa(motivo);
    }

    // ---------- azioni ----------

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

        if (modo == Modo.Entra)
        {
            string errore = Lingua.T("rete.errore_prefab");
            if (ReteCoop.Istanza == null || !ReteCoop.Istanza.Entra(indirizzo, out errore))
            {
                Avvisa(errore);
                return;
            }
            VaiA(Schermata.Collegamento);
            return;
        }
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
        AggiornaBraci(dt);

        // velo nero: si schiarisce all'inizio, si scurisce quando parte la partita
        nero = Mathf.MoveTowards(nero, avvioInCorso ? 1f : 0f, dt / (avvioInCorso ? 1.5f : 2.5f));

        // musica
        livelloMusica = Mathf.MoveTowards(livelloMusica, avvioInCorso ? 0f : 1f, dt / Mathf.Max(0.1f, dissolvenzaMusica));
        if (sorgente != null) sorgente.volume = livelloMusica * Impostazioni.VolumeMusica;

        if (avvioInCorso)
        {
            if (nero >= 1f && livelloMusica <= 0.05f && !string.IsNullOrEmpty(scenaDaCaricare))
            {
                string scena = scenaDaCaricare;
                scenaDaCaricare = null;
                if (modo != Modo.Ospita) SceneManager.LoadScene(scena);
                else
                {
                    string errore = Lingua.T("rete.errore_prefab");
                    if (ReteCoop.Istanza == null || !ReteCoop.Istanza.Ospita(scena, out errore))
                    {
                        // Non si riesce a ospitare (per esempio la porta è già usata): si torna al menu con l'avviso.
                        avvioInCorso = false;
                        VaiA(Schermata.Multigiocatore);
                        Avvisa(errore);
                    }
                }
            }
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

        var c = elenco.LeggiComandi();

        // Indirizzo: Backspace cancella l'ultima cifra (e torna indietro solo se non c'è più niente da cancellare).
        if (schermata == Schermata.Indirizzo && tastiera != null && tastiera.backspaceKey.wasPressedThisFrame && indirizzo.Length > 0)
        {
            indirizzo = indirizzo.Substring(0, indirizzo.Length - 1);
            if (!tastiera.escapeKey.wasPressedThisFrame) c.indietro = false;
        }

        // scelta della classe: tre riquadri affiancati (sinistra e destra), "Indietro" sotto (su e giù)
        if (schermata == Schermata.Classe)
        {
            int classi = chiaviClassi.Length;
            if (c.orizzontale != 0 && elenco.selezione < classi)
            {
                elenco.selezione = (elenco.selezione + c.orizzontale + classi) % classi;
                ultimaClasse = elenco.selezione;
            }
            if (c.verticale != 0) elenco.selezione = elenco.selezione < classi ? classi : ultimaClasse;
            c.orizzontale = c.verticale = 0;
        }

        if (c.indietro) { Indietro(); return; }
        elenco.Applica(c);
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
            GraficaMenu.Alone(new Rect(b.pos.x - b.taglia, b.pos.y - b.taglia, b.taglia * 2f, b.taglia * 2f),
                new Color(1f, 0.5f + 0.25f * (1f - t), 0.18f, Mathf.Sin(t * Mathf.PI) * 0.75f));
        }
    }

    // ---------- disegno ----------

    void OnGUI()
    {
        GraficaMenu.PreparaStili();
        GraficaMenu.Atmosfera(0f);
        GraficaMenu.FoglioVirtuale();
        elenco.InizioGUI();
        DisegnaBraci();

        float comparsa = Mathf.Clamp01(tempoSchermata / 0.6f);
        bool schermoTitolo = schermata == Schermata.Titolo;

        // titolo, con un alone caldo dietro e il divisore sotto
        float altoTitolo = schermoTitolo ? 290f : 110f;
        GraficaMenu.Alone(new Rect(Larghezza * 0.5f - 720f, altoTitolo - 130f, 1440f, 400f), new Color(1f, 0.5f, 0.2f, 0.08f));
        GraficaMenu.Scritta(new Rect(0, altoTitolo, Larghezza, 150), GraficaMenu.Spaziato(titolo.ToUpperInvariant()), GraficaMenu.Titolo, GraficaMenu.Testo, 1f);
        GraficaMenu.Divisore(Larghezza * 0.5f, altoTitolo + 162f, 640f, 1f);
        if (nomeProvvisorio)
            GraficaMenu.Scritta(new Rect(0, altoTitolo + 180, Larghezza, 40), Lingua.T("menu.nome_provvisorio"), GraficaMenu.Sottotitolo, GraficaMenu.Spento, 1f);

        switch (schermata)
        {
            case Schermata.Titolo:
            {
                float pulsa = 0.4f + 0.5f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.2f));
                string premi = Lingua.T("menu.premi");
                var area = new Rect(0, 760, Larghezza, 60);
                GraficaMenu.Scritta(area, premi, GraficaMenu.Voce, GraficaMenu.Testo, pulsa * comparsa);
                float w = GraficaMenu.Voce.CalcSize(new GUIContent(premi)).x;
                Color rombo = GraficaMenu.Con(GraficaMenu.Bronzo, pulsa * comparsa);
                GraficaMenu.Rombo(new Vector2(Larghezza * 0.5f - w * 0.5f - 34f, area.center.y), 10f, rombo);
                GraficaMenu.Rombo(new Vector2(Larghezza * 0.5f + w * 0.5f + 34f, area.center.y), 10f, rombo);
                break;
            }
            case Schermata.Principale:
                if (elenco.DisegnaElenco(380f, 600f, comparsa, avvioInCorso)) return;
                break;
            case Schermata.Multigiocatore:
                if (DisegnaMultigiocatore(comparsa)) return;
                break;
            case Schermata.Indirizzo:
                GraficaMenu.Scritta(new Rect(0, 348, Larghezza, 44), Lingua.T("rete.scrivi_indirizzo"), GraficaMenu.Sottotitolo, GraficaMenu.Testo, comparsa);
                if (elenco.DisegnaElenco(420f, 760f, comparsa, avvioInCorso)) return;
                GraficaMenu.Scritta(new Rect(Larghezza * 0.5f - 560f, 725f, 1120f, 120f), Lingua.T("rete.spiega_indirizzo"), GraficaMenu.Descrizione, GraficaMenu.Spento, comparsa);
                break;
            case Schermata.Collegamento:
            {
                int puntini = 1 + (int)(Time.unscaledTime * 2f) % 3;
                GraficaMenu.Scritta(new Rect(0, 420, Larghezza, 60), Lingua.T("rete.collegamento") + " " + indirizzo + new string('.', puntini), GraficaMenu.Voce, GraficaMenu.Testo, comparsa);
                if (elenco.DisegnaElenco(540f, 440f, comparsa, avvioInCorso)) return;
                break;
            }
            case Schermata.Opzioni:
                if (elenco.DisegnaOpzioni(370f, comparsa, avvioInCorso)) return;
                break;
            case Schermata.Classe:
                if (DisegnaClassi(comparsa)) return;
                break;
            case Schermata.Crediti:
            {
                var fascia = new Rect(Larghezza * 0.5f - 560f, 365f, 1120f, 520f);
                GraficaMenu.Cornice(fascia, comparsa, false);
                DisegnaCrediti(new Rect(fascia.x + 12f, fascia.y + 12f, fascia.width - 24f, fascia.height - 24f), comparsa);
                if (elenco.voci.Count > 0)
                {
                    var area = new Rect(Larghezza * 0.5f - 220f, 910f, 440f, 60f);
                    if (elenco.Mouse(area, 0, avvioInCorso)) return;
                    elenco.VoceCentrata(area, elenco.voci[0], elenco.selezione == 0, comparsa);
                }
                break;
            }
        }

        if (!schermoTitolo)
            GraficaMenu.Scritta(new Rect(0, 1030, Larghezza, 30),
                schermata == Schermata.Crediti ? Lingua.T("menu.salta") : Lingua.T("menu.aiuto"),
                GraficaMenu.Piccolo, GraficaMenu.Spento, 0.8f * comparsa);

        if (tempoAvviso > 0f && !string.IsNullOrEmpty(avviso))
            GraficaMenu.Scritta(new Rect(0, 985, Larghezza, 40), avviso, GraficaMenu.Piccolo, GraficaMenu.Selezione, Mathf.Clamp01(tempoAvviso));

        // velo nero per le dissolvenze
        if (nero > 0.001f)
        {
            GUI.matrix = Matrix4x4.identity;
            GraficaMenu.Riempi(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, nero));
        }
    }

    // Multigiocatore: le due scelte, poi gli indirizzi di questo PC da dare agli amici e due righe di spiegazione.
    bool DisegnaMultigiocatore(float comparsa)
    {
        GraficaMenu.Scritta(new Rect(0, 348, Larghezza, 44), Lingua.T("rete.sottotitolo"), GraficaMenu.Sottotitolo, GraficaMenu.Testo, comparsa);
        if (elenco.DisegnaElenco(420f, 640f, comparsa, avvioInCorso)) return true;

        string miei = indirizziMiei.Count > 0 ? string.Join(GraficaMenu.Separatore, indirizziMiei) : "—";
        GraficaMenu.Scritta(new Rect(0, 730f, Larghezza, 40f), Lingua.T("rete.tuo_indirizzo") + ":   " + miei, GraficaMenu.Voce, GraficaMenu.Bronzo, comparsa);
        GraficaMenu.Scritta(new Rect(Larghezza * 0.5f - 620f, 790f, 1240f, 150f), Lingua.T("rete.spiega"), GraficaMenu.Descrizione, GraficaMenu.Spento, comparsa);
        return false;
    }

    // Scelta della classe: tre riquadri affiancati con numero romano, nome e descrizione; sotto "Indietro".
    bool DisegnaClassi(float comparsa)
    {
        GraficaMenu.Scritta(new Rect(0, 348, Larghezza, 44), Lingua.T("classe.scegli"), GraficaMenu.Sottotitolo, GraficaMenu.Testo, comparsa);

        const float larghezza = 440f, altezza = 440f, spazio = 50f, alto = 420f;
        string[] numeri = { "I", "II", "III" };
        int classi = chiaviClassi.Length;
        float inizio = Larghezza * 0.5f - (classi * larghezza + (classi - 1) * spazio) * 0.5f;

        for (int i = 0; i < classi; i++)
        {
            bool scelta = i == elenco.selezione;
            var r = new Rect(inizio + i * (larghezza + spazio), alto - (scelta ? 10f : 0f), larghezza, altezza);
            if (elenco.Mouse(r, i, avvioInCorso)) return true;

            if (scelta) GraficaMenu.Alone(new Rect(r.x - 90f, r.y - 90f, r.width + 180f, r.height + 180f), new Color(1f, 0.5f, 0.2f, 0.16f * comparsa));
            GraficaMenu.Cornice(r, comparsa, scelta);

            Color accento = scelta ? GraficaMenu.Selezione : GraficaMenu.Bronzo;
            GraficaMenu.Scritta(new Rect(r.x, r.y + 26f, r.width, 110f), numeri[Mathf.Min(i, numeri.Length - 1)], GraficaMenu.Emblema, accento, comparsa);
            GraficaMenu.Scritta(new Rect(r.x, r.y + 150f, r.width, 50f), Lingua.T(chiaviClassi[i]).ToUpperInvariant(), GraficaMenu.NomeClasse,
                scelta ? GraficaMenu.Selezione : GraficaMenu.Testo, comparsa);
            GraficaMenu.Divisore(r.center.x, r.y + 222f, 260f, comparsa);
            GraficaMenu.Scritta(new Rect(r.x + 34f, r.y + 248f, r.width - 68f, r.height - 270f), Lingua.T(chiaviClassi[i] + ".descrizione"),
                GraficaMenu.Descrizione, scelta ? GraficaMenu.Testo : GraficaMenu.Spento, comparsa);
        }

        if (elenco.voci.Count > classi)
        {
            var area = new Rect(Larghezza * 0.5f - 220f, 905f, 440f, 60f);
            if (elenco.Mouse(area, classi, avvioInCorso)) return true;
            elenco.VoceCentrata(area, elenco.voci[classi], elenco.selezione == classi, comparsa);
        }
        return false;
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
                GraficaMenu.Scritta(new Rect(0, y, fascia.width, 60), ruolo, GraficaMenu.Voce, GraficaMenu.Selezione, alfa);
            else
            {
                GraficaMenu.Scritta(new Rect(0, y, fascia.width, 34), ruolo, GraficaMenu.Sottotitolo, GraficaMenu.Bronzo, alfa);
                GraficaMenu.Scritta(new Rect(0, y + 36, fascia.width, 46), nomi, GraficaMenu.Voce, GraficaMenu.Testo, alfa);
            }
        }
        y += passo;
    }
}
