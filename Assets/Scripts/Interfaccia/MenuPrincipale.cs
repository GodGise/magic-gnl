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
// Sequenza di avvio: schermo scuro e titolo che sfuma; "Premi un tasto" compare dopo qualche secondo e finche' non e'
// del tutto visibile i tasti non contano (niente spam all'avvio). Alla pressione suona un colpo, il paesaggio passa da
// sfumato (nebbia grigia) a colorato, entra la musica e compare il menu. Dopo la prima pressione gli altri tasti non
// contano. Tempi regolabili dall'Inspector (campi "Sequenza di avvio"). F2 salta la sequenza (comodo per provare il gioco).
// Musica: parte solo durante la transizione (sulla schermata del titolo c'e' silenzio). Sale lentamente e si spegne quando
// inizia la partita. Se il campo "Musica" e' vuoto si carica Assets/Resources/Audio/Musica/menu-principale.
// Suoni: tick quando ci si sposta su una voce, suono di conferma, colpo del titolo e (facoltativi) onda della transizione e
// ambiente del titolo. Se i campi sono vuoti si caricano da Assets/Resources/Audio/Effetti (menu-passaggio, menu-conferma,
// titolo-pressione, titolo-transizione) e Assets/Resources/Audio/Ambiente (titolo): non serve trascinare nulla.
// Stile dark fantasy (GraficaMenu.cs): titolo inciso, riquadri con cornice di bronzo, braci che salgono dal basso.
// Come montarlo: su un oggetto vuoto della scena Menu. Il menu "magic-gnl > Crea scena menu" prepara tutto da solo.
[RequireComponent(typeof(AudioSource))]
public class MenuPrincipale : MonoBehaviour
{
    // Nome del gioco, deciso il 10 ottobre 2026. Sullo schermo va su due righe: "Sun of the" piccolo sopra,
    // "Black Lake" grande sotto (tutto su una riga non entrerebbe con le lettere spaziate).
    const string NomeSopra = "Sun of the", NomeSotto = "Black Lake";

    [Header("Partita")]
    [SerializeField] string scenaIniziale = "VillaggioLagoNero";
    [SerializeField] string scenaRiserva = "ZonaProva";

    [Header("Musica")]
    [SerializeField] AudioClip musica;
    [Tooltip("Secondi per far salire (o scendere) la musica.")]
    [SerializeField] float dissolvenzaMusica = 4f;

    [Header("Suoni (se un campo e' vuoto si carica da Assets/Resources/Audio/...)")]
    [Tooltip("Colpo quando si preme il tasto sulla schermata del titolo. Resources: Audio/Effetti/titolo-pressione")]
    [SerializeField] AudioClip suonoTitolo;
    [Tooltip("Facoltativo: onda sonora che sale durante la transizione verso il menu. Resources: Audio/Effetti/titolo-transizione")]
    [SerializeField] AudioClip suonoTransizione;
    [Tooltip("Facoltativo: vento e drone quasi impercettibili sulla schermata del titolo, in ciclo. Resources: Audio/Ambiente/titolo")]
    [SerializeField] AudioClip ambienteTitolo;
    [Tooltip("Tick quando ci si sposta su una voce. Resources: Audio/Effetti/menu-passaggio")]
    [SerializeField] AudioClip suonoPassaggio;
    [Tooltip("Suono quando si conferma una voce. Resources: Audio/Effetti/menu-conferma")]
    [SerializeField] AudioClip suonoConferma;
    [Tooltip("Quanto cambia a caso l'altezza del tick, cosi non suona sempre uguale.")]
    [SerializeField, Range(0f, 0.2f)] float variazioneTono = 0.05f;
    [Header("Volumi dei suoni (0 = muto, 1 = pieno)")]
    [Tooltip("Volume del tick quando ci si sposta sulle voci.")]
    [SerializeField, Range(0f, 1f)] float volumeTick = 0.35f;
    [Tooltip("Volume del suono di conferma.")]
    [SerializeField, Range(0f, 1f)] float volumeConferma = 0.6f;
    [Tooltip("Volume del colpo su Premi un tasto e dell'onda di transizione.")]
    [SerializeField, Range(0f, 1f)] float volumeTitolo = 0.8f;

    [Header("Sequenza di avvio (secondi)")]
    [Tooltip("Dopo quanto compare Premi un tasto.")]
    [SerializeField] float ritardoPremi = 2.5f;
    [Tooltip("Durata della dissolvenza di Premi un tasto. I tasti si accettano solo a dissolvenza finita.")]
    [SerializeField] float durataPremi = 1.5f;
    [Tooltip("Durata della transizione dopo la pressione (da sfumato a colorato).")]
    [SerializeField] float durataTransizione = 3.5f;
    [Tooltip("A che punto della transizione entra la musica (0 = subito, 1 = alla fine).")]
    [SerializeField, Range(0f, 1f)] float puntoMusica = 0.55f;
    [Tooltip("A che punto della transizione compare il menu.")]
    [SerializeField, Range(0f, 1f)] float puntoMenu = 0.7f;
    [Tooltip("Dopo quanto dalla pressione parte l'onda sonora della transizione (se c'e').")]
    [SerializeField] float ritardoSuonoTransizione = 0.5f;
    [Tooltip("F2 salta la sequenza di avvio (comodo quando provi il gioco).")]
    [SerializeField] bool saltoConF2 = true;

    [Header("Atmosfera")]
    [Tooltip("Quante braci salgono dal basso dello schermo.")]
    [SerializeField] int numeroBraci = 46;

    enum Schermata { Titolo, Principale, Classe, Opzioni, Crediti, Multigiocatore, Indirizzo, Collegamento, ConfermaEsci }

    // Come parte la partita dopo la scelta della classe: da soli, ospitando gli amici, o entrando da un amico.
    enum Modo { DaSolo, Ospita, Entra }
    Modo modo = Modo.DaSolo;
    Impostazioni.Sezione sezioneOpzioni = Impostazioni.Sezione.Principale;   // quale pagina delle opzioni è aperta
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
    AudioSource sorgente;                 // la musica
    AudioSource effetti, ambiente;        // colpi e tick / suono d'ambiente del titolo (creati nello Start)
    bool musicaAttiva;                    // la musica entra solo dopo Premi un tasto
    bool inTransizione;
    float tempoTransizione;
    float grigio = 1f;                    // velo di nebbia grigia sul paesaggio: 1 = sfumato, 0 = colorato
    float ambienteLivello;                // 0..1, per la dissolvenza del suono d'ambiente
    float durataComparsa = 0.6f;          // secondi di dissolvenza dei riquadri quando si cambia schermata
    bool titoloGiaVisto;
    float ritardoAttuale, durataAttuale;  // tempi di Premi un tasto (piu brevi se si torna al titolo con Indietro)
    float altoTitoloVisibile = 290f;      // il titolo sale piano quando compare il menu

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
        if (musica == null) musica = Resources.Load<AudioClip>("Audio/Musica/menu-principale");
        if (musica != null) sorgente.clip = musica;   // parte piu avanti, durante la transizione (AvviaMusica)

        if (suonoTitolo == null) suonoTitolo = Resources.Load<AudioClip>("Audio/Effetti/titolo-pressione");
        if (suonoTransizione == null) suonoTransizione = Resources.Load<AudioClip>("Audio/Effetti/titolo-transizione");
        if (ambienteTitolo == null) ambienteTitolo = Resources.Load<AudioClip>("Audio/Ambiente/titolo");
        if (suonoPassaggio == null) suonoPassaggio = Resources.Load<AudioClip>("Audio/Effetti/menu-passaggio");
        if (suonoConferma == null) suonoConferma = Resources.Load<AudioClip>("Audio/Effetti/menu-conferma");

        effetti = CreaSorgente(false);
        ambiente = CreaSorgente(true);
        if (ambienteTitolo != null) ambiente.clip = ambienteTitolo;
        elenco.alMuovere = () => SuonaEffetto(suonoPassaggio, true, volumeTick);
        elenco.alConfermare = () => SuonaEffetto(suonoConferma, true, volumeConferma);

        braci = new Brace[Mathf.Max(0, numeroBraci)];
        for (int i = 0; i < braci.Length; i++) NuovaBrace(ref braci[i], true);

        indirizzo = PlayerPrefs.GetString(ChiaveIndirizzo, "127.0.0.1");
        if (Keyboard.current != null) Keyboard.current.onTextInput += TestoScritto;
        if (ReteCoop.Istanza != null) ReteCoop.Istanza.EsitoCollegamento += Collegato;

        // Tornati qui da una partita in rete finita male (host uscito, connessione caduta): si mostra il perché.
        if (!string.IsNullOrEmpty(ReteCoop.AvvisoPerMenu))
        {
            // niente sequenza di avvio: si torna direttamente al menu, con musica e paesaggio a colori
            titoloGiaVisto = true;
            grigio = 0f;
            altoTitoloVisibile = 110f;
            AvviaMusica();
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
        durataComparsa = 0.6f;
        elenco.Pulisci();

        if (nuova == Schermata.Titolo)
        {
            // la prima volta Premi un tasto compare con calma; tornando indietro dal menu e' quasi subito
            ritardoAttuale = titoloGiaVisto ? 0.2f : ritardoPremi;
            durataAttuale = titoloGiaVisto ? 0.5f : durataPremi;
            titoloGiaVisto = true;
        }

        switch (nuova)
        {
            case Schermata.Principale:
                elenco.Aggiungi(() => Lingua.T("menu.nuova"), () => { modo = Modo.DaSolo; VaiA(Schermata.Classe); });
                elenco.Aggiungi(() => Lingua.T("menu.multigiocatore"), () => VaiA(Schermata.Multigiocatore));
                elenco.Aggiungi(() => Lingua.T("menu.continua"), null).attiva = false;
                elenco.Aggiungi(() => Lingua.T("menu.opzioni"), () => { sezioneOpzioni = Impostazioni.Sezione.Principale; VaiA(Schermata.Opzioni); });
                elenco.Aggiungi(() => Lingua.T("menu.crediti"), () => VaiA(Schermata.Crediti));
                elenco.Aggiungi(() => Lingua.T("menu.esci"), () => VaiA(Schermata.ConfermaEsci));
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
                Impostazioni.AggiungiVoci(elenco, sezioneOpzioni, s => { sezioneOpzioni = s; VaiA(Schermata.Opzioni); }, IndietroOpzioni);
                break;

            // Uscire dal gioco: prima la conferma, di partenza su "No".
            case Schermata.ConfermaEsci:
                elenco.Aggiungi(() => Lingua.T("comune.no"), () => VaiA(Schermata.Principale));
                elenco.Aggiungi(() => Lingua.T("comune.si"), Esci);
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
            case Schermata.Opzioni: IndietroOpzioni(); break;
            case Schermata.Crediti:
            case Schermata.ConfermaEsci:
            case Schermata.Multigiocatore: VaiA(Schermata.Principale); break;
            case Schermata.Indirizzo: VaiA(Schermata.Multigiocatore); break;
            case Schermata.Collegamento: AnnullaCollegamento(); break;
        }
    }

    // Indietro nelle opzioni: da una sezione (Audio, Video, Controlli) si torna alla pagina principale delle opzioni,
    // con la selezione sulla sezione appena lasciata; dalla pagina principale si torna al menu.
    void IndietroOpzioni()
    {
        if (sezioneOpzioni == Impostazioni.Sezione.Principale) { VaiA(Schermata.Principale); return; }
        var da = sezioneOpzioni;
        sezioneOpzioni = Impostazioni.Sezione.Principale;
        VaiA(Schermata.Opzioni);
        elenco.selezione = (int)da;   // le sezioni sono le voci 1, 2, 3 della pagina principale
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
        livelloMusica = Mathf.MoveTowards(livelloMusica, (avvioInCorso || !musicaAttiva) ? 0f : 1f, dt / Mathf.Max(0.1f, dissolvenzaMusica));
        if (sorgente != null) sorgente.volume = livelloMusica * Impostazioni.VolumeMusica;
        AggiornaSequenza(dt);

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

        // durante la transizione verso il menu i comandi non contano: un solo tasto avvia la sequenza
        if (inTransizione) return;
        // la schermata dei comandi (Opzioni > Comandi) legge da sola i suoi tasti, anche Esc
        if (MenuComandi.Aperto || MenuComandi.FotogrammaChiusura == Time.frameCount) return;

        if (schermata == Schermata.Titolo)
        {
            if (saltoConF2 && tastiera != null && tastiera.f2Key.wasPressedThisFrame)
            {
                SaltaSequenza();
                return;
            }
            bool premuto = (tastiera != null && tastiera.anyKey.wasPressedThisFrame)
                || (mouse != null && mouse.leftButton.wasPressedThisFrame)
                || (pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame));
            // Premi un tasto si accetta solo quando e' del tutto visibile
            if (premuto && tempoSchermata >= ritardoAttuale + durataAttuale) AvviaTransizione();
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
                elenco.alMuovere?.Invoke();
            }
            if (c.verticale != 0)
            {
                elenco.selezione = elenco.selezione < classi ? classi : ultimaClasse;
                elenco.alMuovere?.Invoke();
            }
            c.orizzontale = c.verticale = 0;
        }

        if (c.indietro) { Indietro(); return; }
        elenco.Applica(c);
    }

    // ---------- sequenza di avvio e suoni ----------

    // Suono d'ambiente del titolo, transizione dopo Premi un tasto, velo di nebbia grigia e salita del titolo.
    void AggiornaSequenza(float dt)
    {
        // suono d'ambiente (facoltativo): c'e' solo sulla schermata del titolo e si spegne con la transizione
        float bersaglioAmbiente = (schermata == Schermata.Titolo && !inTransizione && !avvioInCorso) ? 1f : 0f;
        ambienteLivello = Mathf.MoveTowards(ambienteLivello, bersaglioAmbiente, dt / (bersaglioAmbiente > 0f ? 3f : Mathf.Max(0.5f, durataTransizione)));
        if (ambiente != null && ambiente.clip != null)
        {
            ambiente.volume = ambienteLivello * 0.4f * Impostazioni.VolumeMusica;
            if (ambienteLivello > 0f && !ambiente.isPlaying) ambiente.Play();
            else if (ambienteLivello <= 0f && ambiente.isPlaying) ambiente.Stop();
        }

        // il titolo sale quando compare il menu (e scende se si torna al titolo)
        altoTitoloVisibile = Mathf.MoveTowards(altoTitoloVisibile, schermata == Schermata.Titolo ? 290f : 110f, dt * 420f);

        if (inTransizione)
        {
            tempoTransizione += dt;
            float p = Mathf.Clamp01(tempoTransizione / Mathf.Max(0.1f, durataTransizione));
            grigio = 1f - Mathf.SmoothStep(0f, 1f, p);
            if (p >= puntoMusica) AvviaMusica();
            if (p >= puntoMenu && schermata == Schermata.Titolo)
            {
                VaiA(Schermata.Principale);
                durataComparsa = 1.4f;   // il menu compare piu lentamente della prima volta
            }
            if (p >= 1f)
            {
                inTransizione = false;
                AvviaMusica();
            }
        }
        else
        {
            grigio = Mathf.MoveTowards(grigio, schermata == Schermata.Titolo ? 1f : 0f, dt / 1.2f);
        }
    }

    // Un solo tasto avvia la sequenza: colpo, onda sonora (se c'e'), poi paesaggio a colori, musica e menu.
    void AvviaTransizione()
    {
        inTransizione = true;
        tempoTransizione = 0f;
        SuonaEffetto(suonoTitolo, false, volumeTitolo);
        if (suonoTransizione != null) Invoke(nameof(SuonoDellaTransizione), Mathf.Max(0f, ritardoSuonoTransizione));
    }

    void SuonoDellaTransizione()
    {
        SuonaEffetto(suonoTransizione, false, volumeTitolo);
    }

    // F2: salta tutta la sequenza e va dritto al menu.
    void SaltaSequenza()
    {
        CancelInvoke();
        inTransizione = false;
        grigio = 0f;
        AvviaMusica();
        VaiA(Schermata.Principale);
    }

    void AvviaMusica()
    {
        if (musicaAttiva) return;
        musicaAttiva = true;
        if (sorgente != null && sorgente.clip != null && !sorgente.isPlaying) sorgente.Play();
    }

    AudioSource CreaSorgente(bool ciclo)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = ciclo;
        s.spatialBlend = 0f;
        s.volume = ciclo ? 0f : 1f;
        return s;
    }

    // Suona un effetto dell'interfaccia (il volume segue il Volume generale delle opzioni).
    void SuonaEffetto(AudioClip clip, bool variaTono, float volume)
    {
        if (clip == null || effetti == null) return;
        effetti.pitch = variaTono ? 1f + Random.Range(-variazioneTono, variazioneTono) : 1f;
        effetti.PlayOneShot(clip, volume * Impostazioni.VolumeEffetti);   // il volume effetti scelto in Opzioni > Audio
    }

    // I comandi non contano durante la transizione e mentre parte la partita.
    bool Bloccato => avvioInCorso || inTransizione;

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
        // nebbia grigia sopra il paesaggio: sfumato sulla schermata del titolo, si scioglie nella transizione
        if (grigio > 0.001f)
            GraficaMenu.Riempi(new Rect(0, 0, Screen.width, Screen.height), new Color(0.30f, 0.32f, 0.37f, 0.62f * grigio));
        GraficaMenu.FoglioVirtuale();
        if (MenuComandi.Aperto) return;   // sopra c'è la schermata dei comandi: il paesaggio resta, il menu no
        elenco.InizioGUI();
        DisegnaBraci();

        float comparsa = Mathf.Clamp01(tempoSchermata / Mathf.Max(0.05f, durataComparsa));
        bool schermoTitolo = schermata == Schermata.Titolo;

        // titolo, con un alone caldo dietro e il divisore sotto (nei crediti no: occupano tutto lo schermo)
        float altoTitolo = altoTitoloVisibile;
        if (schermata != Schermata.Crediti)
        {
            GraficaMenu.Alone(new Rect(Larghezza * 0.5f - 720f, altoTitolo - 130f, 1440f, 400f), new Color(1f, 0.5f, 0.2f, 0.08f));
            var sopra = new GUIStyle(GraficaMenu.Titolo) { fontSize = 40 };
            GraficaMenu.Scritta(new Rect(0, altoTitolo - 6f, Larghezza, 46), GraficaMenu.Spaziato(NomeSopra.ToUpperInvariant()), sopra, GraficaMenu.Bronzo, 1f);
            GraficaMenu.Scritta(new Rect(0, altoTitolo + 22f, Larghezza, 140), GraficaMenu.Spaziato(NomeSotto.ToUpperInvariant()), GraficaMenu.Titolo, GraficaMenu.Testo, 1f);
            GraficaMenu.Divisore(Larghezza * 0.5f, altoTitolo + 162f, 640f, 1f);
        }

        switch (schermata)
        {
            case Schermata.Titolo:
            {
                float pulsa = 0.4f + 0.5f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.2f));
                // Premi un tasto compare lentamente (i tasti contano solo a dissolvenza finita) e sparisce alla pressione
                float entrata = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((tempoSchermata - ritardoAttuale) / Mathf.Max(0.01f, durataAttuale)));
                float uscita = inTransizione ? 1f - Mathf.Clamp01(tempoTransizione / 0.4f) : 1f;
                float alfaPremi = entrata * uscita;
                string premi = Lingua.T("menu.premi");
                var area = new Rect(0, 760, Larghezza, 60);
                GraficaMenu.Scritta(area, premi, GraficaMenu.Voce, GraficaMenu.Testo, pulsa * alfaPremi);
                float w = GraficaMenu.Voce.CalcSize(new GUIContent(premi)).x;
                Color rombo = GraficaMenu.Con(GraficaMenu.Bronzo, pulsa * alfaPremi);
                GraficaMenu.Rombo(new Vector2(Larghezza * 0.5f - w * 0.5f - 34f, area.center.y), 10f, rombo);
                GraficaMenu.Rombo(new Vector2(Larghezza * 0.5f + w * 0.5f + 34f, area.center.y), 10f, rombo);
                break;
            }
            case Schermata.Principale:
                if (elenco.DisegnaElenco(380f, 600f, comparsa, Bloccato)) return;
                break;
            case Schermata.Multigiocatore:
                if (DisegnaMultigiocatore(comparsa)) return;
                break;
            case Schermata.Indirizzo:
                GraficaMenu.Scritta(new Rect(0, 348, Larghezza, 44), Lingua.T("rete.scrivi_indirizzo"), GraficaMenu.Sottotitolo, GraficaMenu.Testo, comparsa);
                if (elenco.DisegnaElenco(420f, 760f, comparsa, Bloccato)) return;
                GraficaMenu.Scritta(new Rect(Larghezza * 0.5f - 560f, 725f, 1120f, 120f), Lingua.T("rete.spiega_indirizzo"), GraficaMenu.Descrizione, GraficaMenu.Spento, comparsa);
                break;
            case Schermata.Collegamento:
            {
                int puntini = 1 + (int)(Time.unscaledTime * 2f) % 3;
                GraficaMenu.Scritta(new Rect(0, 420, Larghezza, 60), Lingua.T("rete.collegamento") + " " + indirizzo + new string('.', puntini), GraficaMenu.Voce, GraficaMenu.Testo, comparsa);
                if (elenco.DisegnaElenco(540f, 440f, comparsa, Bloccato)) return;
                break;
            }
            case Schermata.Opzioni:
                if (elenco.DisegnaOpzioni(370f, comparsa, Bloccato)) return;
                break;
            case Schermata.ConfermaEsci:
                GraficaMenu.Scritta(new Rect(Larghezza * 0.5f - 600f, 360f, 1200f, 100f), Lingua.T("menu.conferma_esci"), GraficaMenu.Descrizione, GraficaMenu.Testo, comparsa);
                if (elenco.DisegnaElenco(480f, 440f, comparsa, Bloccato)) return;
                break;
            case Schermata.Classe:
                if (DisegnaClassi(comparsa)) return;
                break;
            case Schermata.Crediti:
            {
                // a tutto schermo, senza riquadro: lo sfondo si scurisce e i crediti scorrono su quasi tutta l'altezza
                GUI.matrix = Matrix4x4.identity;
                GraficaMenu.Riempi(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.6f * comparsa));
                GraficaMenu.FoglioVirtuale();
                DisegnaCrediti(new Rect(0f, 20f, Larghezza, 870f), comparsa);
                if (elenco.voci.Count > 0)
                {
                    var area = new Rect(Larghezza * 0.5f - 220f, 910f, 440f, 60f);
                    if (elenco.Mouse(area, 0, Bloccato)) return;
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
        if (elenco.DisegnaElenco(420f, 640f, comparsa, Bloccato)) return true;

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
            if (elenco.Mouse(r, i, Bloccato)) return true;

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
            if (elenco.Mouse(area, classi, Bloccato)) return true;
            elenco.VoceCentrata(area, elenco.voci[classi], elenco.selezione == classi, comparsa);
        }
        return false;
    }

    // Crediti che scorrono dal basso verso l'alto su quasi tutto lo schermo, e ricominciano alla fine.
    void DisegnaCrediti(Rect fascia, float comparsa)
    {
        const float passo = 165f, velocita = 70f, primaRiga = 220f;
        int n = TestiCrediti.Elenco.Length;
        float lunghezza = fascia.height + primaRiga + n * passo;
        float scorrimento = (tempoSchermata * velocita) % lunghezza;

        GUI.BeginGroup(fascia);
        float y = fascia.height - scorrimento;
        Riga(ref y, Lingua.T("menu.crediti_testo"), null, fascia, comparsa, primaRiga);
        for (int i = 0; i < n; i++)
            Riga(ref y, TestiCrediti.Ruolo(i), TestiCrediti.Elenco[i].nomi, fascia, comparsa, passo);
        GUI.EndGroup();
    }

    void Riga(ref float y, string ruolo, string nomi, Rect fascia, float comparsa, float passo)
    {
        if (y > -passo && y < fascia.height)
        {
            // sfuma vicino al bordo alto e a quello basso
            float centro = y + 50f;
            float alfa = Mathf.Clamp01(Mathf.Min(centro, fascia.height - centro) / 150f) * comparsa;
            if (string.IsNullOrEmpty(nomi))
                GraficaMenu.Scritta(new Rect(0, y, fascia.width, 90), ruolo, GraficaMenu.CreditiIntestazione, GraficaMenu.Selezione, alfa);
            else
            {
                GraficaMenu.Scritta(new Rect(0, y, fascia.width, 48), ruolo, GraficaMenu.CreditiRuolo, GraficaMenu.Bronzo, alfa);
                GraficaMenu.Scritta(new Rect(0, y + 52, fascia.width, 70), nomi, GraficaMenu.CreditiNomi, GraficaMenu.Testo, alfa);
            }
        }
        y += passo;
    }
}
