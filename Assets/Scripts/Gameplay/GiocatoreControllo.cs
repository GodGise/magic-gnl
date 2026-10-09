using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Controllo del personaggio giocante: movimento, sprint, schivata, parata e attacco.
// A cosa serve: è il cuore del combattimento. Oltre alle tre mosse fondamentali c'è lo sprint:
//   - Sprint:   tieni premuto Shift (o la pressione della levetta sinistra del pad) mentre ti muovi.
//               Si corre più veloci ma la resistenza scende ogni secondo; a resistenza zero lo sprint
//               si ferma e ricomincia solo quando ne è tornata un po'.
// Le tre mosse fondamentali sono
//   - Schivata: Spazio / tasto B (Xbox) o Cerchio (PS). Breve invulnerabilità all'inizio.
//   - Parata:   tieni premuto il tasto destro del mouse / LB (Xbox) o L1 (PS). Riduce il danno
//               dei colpi frontali ma consuma resistenza; a resistenza zero la guardia si rompe.
//   - Attacco:  tasto sinistro del mouse / X (Xbox) o Quadrato (PS). Preparazione, colpo, recupero;
//               durante il recupero si può annullare con una schivata o concatenare un altro attacco.
// Morte e rinascita: quando la vita arriva a zero (o si cade nel vuoto) il personaggio muore e dopo
// qualche secondo rinasce all'ultimo Checkpoint toccato, oppure al punto di partenza se non ne ha toccati.
// Acqua bassa (vedi AcquaBassa): nel lago, dove l'acqua arriva alle ginocchia, il personaggio va più piano.
// Esecuzione furtiva: alle spalle di un nemico che non ti ha visto (vedi InseguimentoNemico), l'attacco con la
// spada diventa un'esecuzione: il personaggio si mette dietro di lui, lo afferra e gli taglia la gola. Il nemico
// muore sul colpo; durante l'esecuzione il giocatore non subisce danni. In basso compare l'avviso quando è possibile.
// Bastone magico (proposta, si trova nel baule della chiesetta): tasto 2 per impugnarlo, 1 per tornare alla
// spada. Con il bastone l'attacco lancia una sfera luminosa verso il nemico agganciato o il più vicino;
// costa mana (terza barra). Il mana si ricarica da solo dopo una breve pausa dall'ultimo lancio (Docs/mana.md)
// e un po' anche sconfiggendo i nemici.
// Stregone: equipaggiando un bastone o una verga (DatiArma magica) il bastone si impugna da solo e i numeri della
// sfera (danno, costo in mana, carica, recupero, portata, armatura ignorata) diventano quelli dell'arma. Libro,
// vesti e amuleti cambiano mana massimo, ricarica e potenza degli incantesimi (vedi Statistiche).
// Abilità dell'amuleto: tasto Q / croce su del pad. Con l'amuleto Ultimo respiro il personaggio svanisce
// nell'ombra: per qualche secondo i nemici non lo vedono, smettono di inseguirlo e non lo attaccano.
// L'invisibilità finisce allo scadere del tempo oppure appena si attacca.
// Con l'aggancio del bersaglio attivo (vedi AggancioBersaglio) il personaggio guarda sempre il nemico:
// A e D girano attorno al nemico, S indietreggia, attacchi e schivate partono verso di lui.
// Strettoie (vedi PassaggioStretto): fra due pareti vicine il personaggio rallenta piano piano, si gira
// di fianco e stringe il suo ingombro, così passa anche dove prima urtava. Quando la strettoia è vera
// rinfodera l'arma sulla schiena e la riprende appena esce; dentro non si attacca, non si para e non si scatta.
// Co-op: è sempre il giocatore di questo PC. Gli altri giocatori si vedono come figure (GiocatoreRete) che copiano
// i loro movimenti; i nemici li prendono di mira tutti (vedi ObiettiviNemici). I colpi ai nemici passano da
// Bersaglio, che in rete li manda a chi ospita la partita.
// Come montarlo: su un oggetto con CharacterController (aggiunto in automatico insieme a Resistenza).
// Il modo più rapido è il menu "magic-gnl > Crea scena di prova", che prepara tutto da solo.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Resistenza))]
[RequireComponent(typeof(AggancioBersaglio))]
public class GiocatoreControllo : MonoBehaviour, IObiettivoNemico, IPersonaggioAnimato
{
    public enum Stato { Libero, Parata, Attacco, Schivata, Stordito, Morto, Esecuzione }

    [Header("Riferimenti")]
    [Tooltip("Se vuoto usa la camera principale.")]
    [SerializeField] Transform cameraRiferimento;

    [Header("Vita")]
    [SerializeField] float vitaMassima = 100f;
    [SerializeField] float secondiPerRinascere = 2f;
    [Tooltip("Se il personaggio scende sotto questa altezza (è caduto nel vuoto) muore e rinasce al checkpoint.")]
    [SerializeField] float quotaVuoto = -30f;

    [Header("Movimento")]
    [SerializeField] float velocitaCorsa = 5f;
    [SerializeField] float velocitaInParata = 2f;

    [Header("Sprint")]
    [SerializeField] float velocitaSprint = 8f;
    [Tooltip("Resistenza consumata ogni secondo mentre si fa lo sprint.")]
    [SerializeField] float costoSprintAlSecondo = 15f;
    [Tooltip("Resistenza minima per iniziare uno sprint (evita che parta e si fermi di continuo a barra quasi vuota).")]
    [SerializeField] float resistenzaMinimaSprint = 10f;

    [Header("Movimento (altro)")]
    [SerializeField] float velocitaRotazione = 720f;
    [SerializeField] float gravita = -20f;

    [Header("Schivata")]
    [SerializeField] float costoSchivata = 25f;
    [SerializeField] float durataSchivata = 0.45f;
    [SerializeField] float distanzaSchivata = 4f;
    [Tooltip("Per quanti secondi dall'inizio della schivata i colpi non fanno danno.")]
    [SerializeField] float invulnerabilitaSchivata = 0.25f;

    [Header("Parata")]
    [SerializeField] float costoColpoParato = 20f;
    [Range(0f, 1f)]
    [SerializeField] float dannoAssorbitoInParata = 0.9f;
    [Tooltip("Ampiezza in gradi dell'arco frontale coperto dalla parata.")]
    [SerializeField] float arcoParata = 120f;
    [Tooltip("Secondi per alzare lo scudo: prima di questo tempo la parata non ferma i colpi. Lo cambiano gli amuleti (Statistiche > Velocita Parata).")]
    [SerializeField] float tempoAlzataScudo = 0.1f;
    [SerializeField] float durataGuardiaRotta = 1f;

    [Header("Attacco")]
    [SerializeField] float costoAttacco = 20f;
    [SerializeField] float dannoAttacco = 25f;
    [SerializeField] float preparazioneAttacco = 0.25f;
    [SerializeField] float colpoAttivo = 0.15f;
    [SerializeField] float recuperoAttacco = 0.35f;
    [SerializeField] float raggioColpo = 1.3f;
    [SerializeField] float portataColpo = 1.8f;
    [Tooltip("Ampiezza in gradi dell'arco davanti al personaggio in cui il colpo va a segno.")]
    [SerializeField] float arcoAttacco = 120f;
    [SerializeField] float velocitaAffondo = 3f;

    [Header("Esecuzione furtiva (alle spalle di un nemico che non ti ha visto)")]
    [Tooltip("Distanza massima dal nemico per l'esecuzione, in metri.")]
    [SerializeField] float distanzaEsecuzione = 2f;
    [Tooltip("Quanto bisogna essere dietro al nemico: ampiezza dell'arco alle sue spalle, in gradi.")]
    [SerializeField] float arcoAlleSpalle = 120f;
    [Tooltip("Durata di tutta l'esecuzione, in secondi.")]
    [SerializeField] float durataEsecuzione = 1.4f;
    [Tooltip("Dopo quanti secondi parte il taglio (e il nemico muore).")]
    [SerializeField] float momentoTaglio = 0.7f;

    [Header("Bastone magico (si trova nel baule della chiesetta)")]
    [SerializeField] float manaMassimo = 100f;
    [Tooltip("Mana speso per ogni sfera.")]
    [SerializeField] float costoSfera = 20f;
    [Tooltip("Mana recuperato per ogni nemico sconfitto, in percentuale del massimo (15 = 15%).")]
    [SerializeField] float manaPerUccisionePercento = 15f;
    [SerializeField] float dannoSfera = 20f;
    [SerializeField] float velocitaSfera = 14f;
    [Tooltip("Se non c'è un nemico agganciato, la sfera va verso il nemico più vicino entro questa distanza.")]
    [SerializeField] float portataSfera = 20f;
    [Tooltip("Secondi di carica prima che parta la sfera.")]
    [SerializeField] float preparazioneIncantesimo = 0.3f;
    [Tooltip("Secondi dopo il lancio prima di poter rifare un'azione (la schivata si può fare subito).")]
    [SerializeField] float recuperoIncantesimo = 0.45f;
    [Tooltip("Secondi dall'ultimo lancio prima che il mana cominci a ricaricarsi da solo.")]
    [SerializeField] float pausaRicaricaMana = 0.8f;
    [Tooltip("Secondi per riempire tutta la barra del mana, una volta partita la ricarica (libri e amuleti la accorciano).")]
    [SerializeField] float secondiRicaricaMana = 12.5f;

    [Header("Altro")]
    [Tooltip("Per quanti secondi un tasto premuto in anticipo resta valido.")]
    [SerializeField] float memoriaComandi = 0.2f;
    [SerializeField] float durataBarcollamento = 0.3f;

    [Header("Strettoie")]
    [Tooltip("Velocità nella strettoia piena, rispetto alla corsa normale (0,35 = 35%).")]
    [SerializeField] float velocitaInStrettoia = 0.35f;
    [Tooltip("Raggio dell'ingombro nella strettoia piena (metri). Normale: 0,5. Il varco più stretto in cui si passa è circa il doppio.")]
    [SerializeField] float raggioInStrettoia = 0.22f;
    [Tooltip("Oltre questo valore di strettoia (da 0 a 1) l'arma va nel fodero.")]
    [SerializeField] float sogliaFodero = 0.5f;
    [Tooltip("Sotto questo valore l'arma torna in mano.")]
    [SerializeField] float sogliaRiprendiArma = 0.12f;
    [Tooltip("Quanto dura il gesto di rinfoderare o riprendere l'arma, in secondi.")]
    [SerializeField] float durataGestoFodero = 0.45f;

    public Stato StatoAttuale => stato;

    // Letti da AnimazioneUmanoide per muovere la figura nel momento giusto.
    public float TempoNelloStato => tempoNelloStato;
    public int ColpoCombo => colpoCombo;               // 0, 1, 2: quale colpo della combo sta facendo
    public float DurataPreparazioneAttacco => preparazioneAttacco;
    public float DurataColpoAttivo => colpoAttivo;
    public float DurataRecuperoAttacco => recuperoAttacco;
    public float DurataSchivata => durataSchivata;
    public Vector3 DirezioneSchivata => direzioneSchivata;
    public float Vita { get; private set; }
    // Vita massima vera: quella dell'Inspector cambiata dalle Statistiche (amuleti: -15 = il 15% in meno).
    public float VitaMassima => statistiche != null ? vitaMassima * (1f + statistiche.VitaMassimaPercento / 100f) : vitaMassima;
    public float DurataEsecuzione => durataEsecuzione;
    public float MomentoTaglio => momentoTaglio;

    // Armi: la spada c'è sempre, il bastone magico si trova nel baule. Tasto 1 spada, tasto 2 bastone.
    public enum ArmaImpugnata { Spada, Bastone }
    public ArmaImpugnata Arma => arma;
    // Il bastone c'è se è stato trovato nel baule della chiesetta o se è equipaggiato un bastone dello Stregone.
    public bool HaBastone => haBastone || bastoneEquipaggiato;
    public float Mana { get; private set; }
    // Mana massimo vero: quello dell'Inspector più libro, veste e amuleto (vedi Statistiche).
    public float ManaMassimo => statistiche != null
        ? Mathf.Max(1f, (manaMassimo + statistiche.ManaMassimo) * (1f + statistiche.ManaMassimoPercento / 100f))
        : manaMassimo;
    public bool AttaccoMagico => attaccoMagico;
    public float DurataPreparazioneIncantesimo => preparazioneIncantesimo;
    public float DurataRecuperoIncantesimo => recuperoIncantesimo;

    // Strettoie: quanto è stretto (0-1), se l'arma è nel fodero e da quanto è iniziato il gesto del fodero.
    public float Strettoia => passaggio != null ? passaggio.Valore : 0f;
    public bool ArmaNelFodero => armaNelFodero;
    public float TempoGestoFodero => Time.time - inizioGestoFodero;
    public float DurataGestoFodero => durataGestoFodero;

    CharacterController controller;
    Resistenza resistenza;
    AggancioBersaglio aggancio;
    PassaggioStretto passaggio;
    float raggioNormale;
    bool armaNelFodero;
    float inizioGestoFodero = -10f;
    InputAction comandoMuovi, comandoSchiva, comandoAttacca, comandoPara, comandoSprint, comandoArma1, comandoArma2, comandoAbilita;
    bool staSprintando;

    ArmaImpugnata arma = ArmaImpugnata.Spada;
    bool haBastone;
    bool bastoneEquipaggiato;   // è equipaggiato un bastone o una verga dello Stregone
    float ultimoLancio = -10f;  // per la pausa prima della ricarica del mana
    float[] valoriSferaBase;    // numeri della sfera scritti nell'Inspector: valgono senza bastone equipaggiato
    float penetrazioneIncantesimo; // quota di armatura nemica ignorata dalla sfera
    float manaPerUccisione;     // amuleto arcano
    float rubaManaPercento;     // amuleto arcano
    bool attaccoMagico;      // l'attacco in corso è un lancio di sfera (bastone), non un colpo di spada
    bool sferaLanciata;
    Bersaglio bersaglioSfera;
    float prossimoAvvisoMana;

    // Esecuzione furtiva: il nemico che si può giustiziare adesso (per l'avviso) e quello che si sta giustiziando.
    float velocitaAcqua = 1f;   // 1 fuori dall'acqua, meno di 1 nell'acqua bassa
    InseguimentoNemico vittimaPossibile;
    InseguimentoNemico vittima;
    bool taglioFatto;

    // Dove rinasce il personaggio: all'inizio è il punto di partenza, poi l'ultimo checkpoint toccato.
    Vector3 puntoRinascita;
    Quaternion rotazioneRinascita;
    Checkpoint ultimoCheckpoint;
    public Checkpoint UltimoCheckpoint => ultimoCheckpoint;

    Stato stato = Stato.Libero;
    float tempoNelloStato;
    float durataStordimento;
    float velocitaVerticale;
    Vector3 direzioneSchivata;
    int colpoCombo;
    bool fendenteSuonato;
    // Tono del sibilo per ogni colpo della combo, così i tre colpi suonano diversi.
    static readonly float[] TonoColpi = { 1f, 1.12f, 0.85f };
    float schivataPrenotataFino = -1f;
    float attaccoPrenotatoFino = -1f;
    // Nemici e muri crepati già colpiti da questo attacco (ognuno una volta sola per colpo).
    readonly HashSet<MonoBehaviour> colpitiInQuestoAttacco = new HashSet<MonoBehaviour>();
    Statistiche statistiche;
    // Numeri del colpo e della parata scritti nell'Inspector: valgono senza arma e scudo (vedi AggiornaEquipaggiamento).
    float[] valoriSenzaArma;
    // Effetti dell'equipaggiamento (vedi AggiornaEquipaggiamento).
    float schivataExtraArmatura;       // peso di scudo e armatura sulla schivata
    float velocitaArmatura = 1f;       // peso di scudo e armatura sulla corsa
    float penetrazioneArma;            // quota di armatura nemica ignorata (mazze)
    float dannoAlleSpalle = 1f;        // moltiplicatore del danno colpendo un nemico da dietro (pugnali)
    float finestraParataPerfetta;      // scudo piccolo: secondi utili per la parata perfetta
    float sbilanciamentoParata = 1.5f; // secondi in cui il nemico resta sbilanciato dopo una parata perfetta
    float dannoSuSbilanciato = 1f;     // moltiplicatore del danno sui nemici sbilanciati
    float vitaPerUccisione;            // amuleto arcano
    float rubaVitaPercento;            // amuleto arcano
    float durataOmbra, ricaricaOmbra;  // amuleto arcano "svanire nell'ombra" (tasto Q)
    float invisibileFino = -1f, ombraProntaDa;
    readonly List<Renderer> partiInOmbra = new List<Renderer>();
    readonly List<Color> coloriPrimaDellOmbra = new List<Color>();

    // Armatura, bonus e critici del giocatore (vedi Statistiche): li usano tutti i calcoli del danno.
    public Statistiche Statistiche => statistiche;

    bool SchivataRichiesta => Time.time <= schivataPrenotataFino;
    bool AttaccoRichiesto => Time.time <= attaccoPrenotatoFino;

    void Awake()
    {
        // Aspetto provvisorio da figura umana al posto della capsula (vedi AspettoUmanoide).
        AspettoUmanoide.Prepara(gameObject, new Color(0.05f, 0.05f, 0.06f), AspettoUmanoide.Arma.Spada);
        // Suono dei passi in base al pavimento (vedi PassiSonori).
        if (GetComponent<PassiSonori>() == null) gameObject.AddComponent<PassiSonori>();

        controller = GetComponent<CharacterController>();
        resistenza = GetComponent<Resistenza>();
        aggancio = GetComponent<AggancioBersaglio>();
        statistiche = Statistiche.Di(this);
        valoriSenzaArma = new[] { dannoAttacco, costoAttacco, preparazioneAttacco, colpoAttivo, recuperoAttacco,
            portataColpo, raggioColpo, arcoAttacco, velocitaAffondo, dannoAssorbitoInParata, costoColpoParato, arcoParata };
        valoriSferaBase = new[] { costoSfera, dannoSfera, portataSfera, preparazioneIncantesimo, recuperoIncantesimo, velocitaSfera };
        Mana = ManaMassimo;
        if (GetComponent<Equipaggiamento>() == null) gameObject.AddComponent<Equipaggiamento>();
        passaggio = GetComponent<PassaggioStretto>();
        if (passaggio == null) passaggio = gameObject.AddComponent<PassaggioStretto>();
        raggioNormale = controller.radius;
        Vita = VitaMassima;
        puntoRinascita = transform.position;
        rotazioneRinascita = transform.rotation;
        CreaComandi();
        ObiettiviNemici.Iscrivi(this);
    }

    void OnEnable()
    {
        comandoMuovi.Enable();
        comandoSchiva.Enable();
        comandoAttacca.Enable();
        comandoPara.Enable();
        comandoSprint.Enable();
        comandoArma1.Enable();
        comandoArma2.Enable();
        comandoAbilita.Enable();
    }

    void OnDisable()
    {
        comandoMuovi.Disable();
        comandoSchiva.Disable();
        comandoAttacca.Disable();
        comandoPara.Disable();
        comandoSprint.Disable();
        comandoArma1.Disable();
        comandoArma2.Disable();
        comandoAbilita.Disable();
    }

    void OnDestroy()
    {
        ObiettiviNemici.Togli(this);
        comandoMuovi.Dispose();
        comandoSchiva.Dispose();
        comandoAttacca.Dispose();
        comandoPara.Dispose();
        comandoSprint.Dispose();
        comandoArma1.Dispose();
        comandoArma2.Dispose();
        comandoAbilita.Dispose();
    }

    void CreaComandi()
    {
        comandoMuovi = new InputAction("Muovi", InputActionType.Value);
        comandoMuovi.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        comandoMuovi.AddBinding("<Gamepad>/leftStick");

        comandoSchiva = new InputAction("Schiva", InputActionType.Button);
        comandoSchiva.AddBinding("<Keyboard>/space");
        comandoSchiva.AddBinding("<Gamepad>/buttonEast");

        comandoAttacca = new InputAction("Attacca", InputActionType.Button);
        comandoAttacca.AddBinding("<Mouse>/leftButton");
        comandoAttacca.AddBinding("<Gamepad>/buttonWest");

        comandoPara = new InputAction("Para", InputActionType.Button);
        comandoPara.AddBinding("<Mouse>/rightButton");
        comandoPara.AddBinding("<Gamepad>/leftShoulder");

        // "<Keyboard>/shift" vale sia per Shift sinistro sia per quello destro.
        comandoSprint = new InputAction("Sprint", InputActionType.Button);
        comandoSprint.AddBinding("<Keyboard>/shift");
        comandoSprint.AddBinding("<Gamepad>/leftStickPress");

        // Cambio arma: 1 spada, 2 bastone (sul pad: croce direzionale sinistra e destra).
        comandoArma1 = new InputAction("ImpugnaSpada", InputActionType.Button);
        comandoArma1.AddBinding("<Keyboard>/1");
        comandoArma1.AddBinding("<Gamepad>/dpad/left");
        comandoArma2 = new InputAction("ImpugnaBastone", InputActionType.Button);
        comandoArma2.AddBinding("<Keyboard>/2");
        comandoArma2.AddBinding("<Gamepad>/dpad/right");
        // Abilità dell'amuleto (per esempio Ultimo respiro: svanire nell'ombra).
        comandoAbilita = new InputAction("Abilita", InputActionType.Button);
        comandoAbilita.AddBinding("<Keyboard>/q");
        comandoAbilita.AddBinding("<Gamepad>/dpad/up");
    }

    void Update()
    {
        float dt = Time.deltaTime;
        tempoNelloStato += dt;

        // Caduto nel vuoto: muore subito (e rinasce al checkpoint).
        if (stato != Stato.Morto && transform.position.y < quotaVuoto) PerdiVita(Vita);

        if (comandoSchiva.WasPressedThisFrame()) schivataPrenotataFino = Time.time + memoriaComandi;
        if (comandoAttacca.WasPressedThisFrame()) attaccoPrenotatoFino = Time.time + memoriaComandi;
        if (comandoAbilita.WasPressedThisFrame()) SvanisciNellOmbra();
        AggiornaOmbra();
        RicaricaMana(dt);

        // Cambio arma, non durante un attacco, non da morti e non con l'arma nel fodero.
        if (stato != Stato.Morto && stato != Stato.Attacco && !armaNelFodero && !GestoFoderoInCorso)
        {
            if (comandoArma1.WasPressedThisFrame()) ImpugnaArma(ArmaImpugnata.Spada);
            if (comandoArma2.WasPressedThisFrame()) ImpugnaArma(ArmaImpugnata.Bastone);
        }

        Vector3 direzioneInput = DirezioneDaInput(comandoMuovi.ReadValue<Vector2>());
        Vector3 movimento = Vector3.zero;
        AggiornaStrettoia(direzioneInput, dt);

        switch (stato)
        {
            case Stato.Libero:
            case Stato.Parata:
                movimento = AggiornaLiberoOParata(direzioneInput, dt);
                break;

            case Stato.Schivata:
                movimento = direzioneSchivata * (distanzaSchivata / durataSchivata);
                if (tempoNelloStato >= durataSchivata) CambiaStato(Stato.Libero);
                break;

            case Stato.Attacco:
                movimento = AggiornaAttacco(direzioneInput);
                break;

            case Stato.Stordito:
                if (tempoNelloStato >= durataStordimento) CambiaStato(Stato.Libero);
                break;

            case Stato.Esecuzione:
                AggiornaEsecuzione();
                break;

            case Stato.Morto:
                break;
        }

        // Nell'acqua bassa si va più piano (vedi AcquaBassa), con un passaggio graduale entrando e uscendo.
        float fattoreAcqua = AcquaBassa.FattoreVelocita(transform.position + Vector3.down * 0.9f);
        velocitaAcqua = Mathf.MoveTowards(velocitaAcqua, fattoreAcqua, 3f * dt);
        if (stato != Stato.Esecuzione) movimento *= velocitaAcqua;

        // Per l'avviso a schermo: c'è un nemico ignaro da giustiziare qui davanti?
        vittimaPossibile = stato == Stato.Libero && arma == ArmaImpugnata.Spada ? CercaVittima() : null;
        if (vittimaPossibile != null) HudGioco.MostraAzione(Lingua.T("hud.tasto_attacco"), Lingua.T("hud.esecuzione"));

        resistenza.InPausaRecupero = stato == Stato.Parata;

        if (controller.isGrounded && velocitaVerticale < 0f) velocitaVerticale = -2f;
        velocitaVerticale += gravita * dt;

        controller.Move((movimento + Vector3.up * velocitaVerticale) * dt);
    }

    Vector3 AggiornaLiberoOParata(Vector3 direzioneInput, float dt)
    {
        // In una strettoia si avanza soltanto, di fianco e piano: niente schivate, attacchi, parate e sprint.
        if (InStrettoia)
        {
            schivataPrenotataFino = -1f;
            attaccoPrenotatoFino = -1f;
            staSprintando = false;
            if (stato == Stato.Parata) CambiaStato(Stato.Libero);
            if (direzioneInput.sqrMagnitude <= 0.0001f) return Vector3.zero;
            RuotaVerso(direzioneInput, dt);
            return direzioneInput * velocitaCorsa * Mathf.Lerp(1f, velocitaInStrettoia, Strettoia);
        }

        // Priorità: schivata, poi attacco, poi parata.
        if (SchivataRichiesta && resistenza.HaResistenza)
        {
            IniziaSchivata(direzioneInput);
            return Vector3.zero;
        }
        if (AttaccoRichiesto && PuoAttaccare())
        {
            IniziaAttacco();
            return Vector3.zero;
        }

        bool vuoleParare = comandoPara.IsPressed();
        if (vuoleParare && stato == Stato.Libero) CambiaStato(Stato.Parata);
        else if (!vuoleParare && stato == Stato.Parata) CambiaStato(Stato.Libero);

        // Agganciato: lo sguardo resta sul nemico e ci si muove di lato o indietro.
        if (DirezioneVersoBersaglio(out Vector3 versoBersaglio)) RuotaVerso(versoBersaglio, dt);

        bool inMovimento = direzioneInput.sqrMagnitude > 0.0001f;
        AggiornaSprint(inMovimento);
        if (!inMovimento) return Vector3.zero;

        if (!SonoAgganciato) RuotaVerso(direzioneInput, dt);

        float velocita = velocitaCorsa;
        if (stato == Stato.Parata)
        {
            velocita = velocitaInParata;
        }
        else if (staSprintando)
        {
            velocita = velocitaSprint;
            // Spendere resistenza ogni frame ferma anche la ricarica, come per le altre azioni.
            resistenza.Spendi(costoSprintAlSecondo * dt);
        }
        // Spazio che si stringe ma non ancora strettoia vera: si rallenta già un po'.
        return direzioneInput * velocita * velocitaArmatura * Mathf.Lerp(1f, velocitaInStrettoia, Strettoia);
    }

    // ---------- Strettoie ----------

    bool GestoFoderoInCorso => Time.time - inizioGestoFodero < durataGestoFodero;
    bool InStrettoia => Strettoia > 0.3f || armaNelFodero || GestoFoderoInCorso;

    void AggiornaStrettoia(Vector3 direzioneInput, float dt)
    {
        bool puoMuoversi = stato == Stato.Libero || stato == Stato.Parata;
        passaggio.Aggiorna(puoMuoversi ? direzioneInput : Vector3.zero, dt);

        // L'ingombro si stringe insieme alla strettoia: così il personaggio passa nei varchi stretti.
        controller.radius = Mathf.Lerp(raggioNormale, raggioInStrettoia, Strettoia);

        if (stato == Stato.Morto) return;
        if (!armaNelFodero && Strettoia > sogliaFodero && stato == Stato.Libero && !GestoFoderoInCorso)
        {
            armaNelFodero = true;
            inizioGestoFodero = Time.time;
            Suoni.Suona(Suono.CambioArma, transform.position + Vector3.up, 0.5f, 0.85f);
        }
        else if (armaNelFodero && Strettoia < sogliaRiprendiArma && !GestoFoderoInCorso)
        {
            armaNelFodero = false;
            inizioGestoFodero = Time.time;
            Suoni.Suona(Suono.CambioArma, transform.position + Vector3.up, 0.6f, 1.1f);
        }
    }

    // Decide se lo sprint è attivo: serve Shift premuto, il personaggio libero e in movimento.
    // Parte solo con un po' di resistenza e si ferma quando la barra è vuota.
    void AggiornaSprint(bool inMovimento)
    {
        bool vuoleSprint = comandoSprint.IsPressed() && inMovimento && stato == Stato.Libero;

        if (!vuoleSprint) staSprintando = false;
        else if (!staSprintando && resistenza.Attuale >= resistenzaMinimaSprint) staSprintando = true;
        else if (staSprintando && !resistenza.HaResistenza) staSprintando = false;
    }

    Vector3 AggiornaAttacco(Vector3 direzioneInput)
    {
        if (attaccoMagico) return AggiornaIncantesimo(direzioneInput);

        float fineColpo = preparazioneAttacco + colpoAttivo;
        float fineAttacco = fineColpo + recuperoAttacco;

        if (tempoNelloStato < preparazioneAttacco) return Vector3.zero;

        if (tempoNelloStato < fineColpo)
        {
            if (!fendenteSuonato)
            {
                fendenteSuonato = true;
                Suoni.Suona(Suono.Fendente, transform.position + Vector3.up, 0.8f, TonoColpi[colpoCombo % TonoColpi.Length]);
            }
            ControllaColpi();
            return transform.forward * velocitaAffondo;
        }

        // Recupero: si può annullare con una schivata o concatenare un secondo attacco.
        if (SchivataRichiesta && resistenza.HaResistenza)
        {
            IniziaSchivata(direzioneInput);
            return Vector3.zero;
        }
        if (AttaccoRichiesto && resistenza.HaResistenza && tempoNelloStato >= fineColpo + recuperoAttacco * 0.4f)
        {
            if (!SonoAgganciato && direzioneInput.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(direzioneInput);
            IniziaAttacco(true);
            return Vector3.zero;
        }
        if (tempoNelloStato >= fineAttacco) CambiaStato(Stato.Libero);
        return Vector3.zero;
    }

    void IniziaSchivata(Vector3 direzioneInput)
    {
        resistenza.Spendi(costoSchivata + schivataExtraArmatura);
        Suoni.Suona(Suono.Schivata, transform.position + Vector3.up, 0.7f);
        schivataPrenotataFino = -1f;

        if (direzioneInput.sqrMagnitude > 0.0001f)
        {
            direzioneSchivata = direzioneInput.normalized;
            // Da agganciati la schivata non gira il personaggio: resta rivolto al nemico.
            if (!SonoAgganciato) transform.rotation = Quaternion.LookRotation(direzioneSchivata);
        }
        else
        {
            // Senza direzione: passo indietro, mantenendo lo sguardo in avanti.
            direzioneSchivata = -transform.forward;
        }
        CambiaStato(Stato.Schivata);
    }

    // Con la spada serve resistenza; con il bastone serve abbastanza mana (altrimenti avvisa).
    bool PuoAttaccare()
    {
        if (arma == ArmaImpugnata.Spada) return resistenza.HaResistenza;
        if (Mana >= costoSfera) return true;

        attaccoPrenotatoFino = -1f;
        if (Time.time >= prossimoAvvisoMana)
        {
            prossimoAvvisoMana = Time.time + 1f;
            MessaggiSchermo.Mostra(Lingua.T("hud.mana_insufficiente"), 1.5f);
            Suoni.Suona(Suono.Negato, transform.position + Vector3.up, 0.7f);
        }
        return false;
    }

    // concatenato = attacco fatto durante il recupero del precedente: passa al colpo dopo della combo (1, 2, poi di nuovo 0).
    void IniziaAttacco(bool concatenato = false)
    {
        RompiOmbra(); // attaccare (anche con il bastone o con l'esecuzione furtiva) fa tornare visibili
        attaccoMagico = arma == ArmaImpugnata.Bastone;
        if (attaccoMagico)
        {
            // Bastone: la sfera parte alla fine della carica, verso il nemico scelto adesso.
            colpoCombo = 0;
            sferaLanciata = false;
            Mana -= costoSfera;
            ultimoLancio = Time.time;
            bersaglioSfera = ScegliBersaglioSfera();
            if (bersaglioSfera != null)
            {
                Vector3 verso = bersaglioSfera.transform.position - transform.position;
                verso.y = 0f;
                if (verso.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(verso);
            }
            attaccoPrenotatoFino = -1f;
            CambiaStato(Stato.Attacco);
            return;
        }

        // Alle spalle di un nemico ignaro il primo colpo diventa un'esecuzione furtiva.
        if (!concatenato && CercaVittima() is InseguimentoNemico bersaglioFurtivo)
        {
            IniziaEsecuzione(bersaglioFurtivo);
            return;
        }

        colpoCombo = concatenato ? (colpoCombo + 1) % 3 : 0;
        fendenteSuonato = false;
        resistenza.Spendi(costoAttacco);
        attaccoPrenotatoFino = -1f;
        colpitiInQuestoAttacco.Clear();
        if (DirezioneVersoBersaglio(out Vector3 versoBersaglio)) transform.rotation = Quaternion.LookRotation(versoBersaglio);
        CambiaStato(Stato.Attacco);
    }

    // ---------- Bastone magico ----------

    // Lancio con il bastone: carica, la sfera parte, poi un breve recupero (annullabile con una schivata).
    Vector3 AggiornaIncantesimo(Vector3 direzioneInput)
    {
        if (tempoNelloStato < preparazioneIncantesimo) return Vector3.zero;

        if (!sferaLanciata)
        {
            sferaLanciata = true;
            LanciaSfera();
        }

        if (SchivataRichiesta && resistenza.HaResistenza)
        {
            IniziaSchivata(direzioneInput);
            return Vector3.zero;
        }
        if (tempoNelloStato >= preparazioneIncantesimo + recuperoIncantesimo) CambiaStato(Stato.Libero);
        return Vector3.zero;
    }

    void LanciaSfera()
    {
        Bersaglio obiettivo = bersaglioSfera != null && !bersaglioSfera.Morto ? bersaglioSfera : ScegliBersaglioSfera();
        Vector3 partenza = transform.position + Vector3.up * 0.5f + transform.forward * 0.7f;
        Vector3 direzione = obiettivo != null
            ? (obiettivo.transform.position + Vector3.up * 0.3f - partenza).normalized
            : transform.forward;

        // Potenza degli incantesimi (libri, amuleti): +15 = sfera il 15% più forte.
        float danno = dannoSfera * (1f + statistiche.PotenzaIncantesimi / 100f);
        SferaMagica.Lancia(partenza, direzione, obiettivo, velocitaSfera, danno, transform, penetrazioneIncantesimo);
        Suoni.Suona(Suono.SferaLancio, partenza, 0.9f);
        MondoRete.InviaSfera(partenza, direzione, obiettivo, velocitaSfera); // gli altri giocatori la vedono partire
    }

    // Il nemico agganciato; se non c'è, il nemico vivo più vicino entro la portata (o nessuno).
    Bersaglio ScegliBersaglioSfera()
    {
        if (SonoAgganciato) return aggancio.Attuale;

        Bersaglio migliore = null;
        float minima = portataSfera;
        foreach (Bersaglio b in FindObjectsByType<Bersaglio>(FindObjectsSortMode.None))
        {
            if (b == null || b.Morto || !b.isActiveAndEnabled) continue;
            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < minima)
            {
                minima = d;
                migliore = b;
            }
        }
        return migliore;
    }

    void ImpugnaArma(ArmaImpugnata nuova)
    {
        if (nuova == arma) return;
        if (nuova == ArmaImpugnata.Bastone && !HaBastone) return;

        arma = nuova;
        AspettoUmanoide.MostraArma(gameObject, arma == ArmaImpugnata.Bastone ? AspettoUmanoide.Arma.Bastone : AspettoUmanoide.Arma.Spada);
        Suoni.Suona(Suono.CambioArma, transform.position + Vector3.up, 0.7f);
    }

    // ---------- Equipaggiamento (vedi Equipaggiamento e gli oggetti in Gameplay/Oggetti) ----------

    // Chiamato da Equipaggiamento ogni volta che cambia qualcosa addosso al giocatore.
    // - L'arma decide i numeri del colpo; senza arma valgono quelli scritti nell'Inspector.
    // - La parata la decide lo scudo; senza scudo si para con l'arma (peggio); senza niente, l'Inspector.
    // - Scudo e armatura pesano: schivata più cara e corsa più lenta.
    // - Gli amuleti arcani danno il loro effetto speciale.
    public void AggiornaEquipaggiamento(DatiArma armaNuova, DatiScudo scudo, DatiArmatura armatura, DatiAmuleto amuleto, DatiLibro libro = null)
    {
        if (valoriSenzaArma == null) return;
        float[] v = valoriSenzaArma;
        // Bastoni e verghe dello Stregone non cambiano il colpo di spada: cambiano la sfera (vedi sotto).
        bool magica = armaNuova != null && armaNuova.Magica;
        bool a = armaNuova != null && !magica;
        dannoAttacco = a ? armaNuova.danno : v[0];
        costoAttacco = a ? armaNuova.costoAttacco : v[1];
        // Velocità d'attacco delle Statistiche (amuleti): -7,5 = carica, colpo e recupero durano il 7,5% in più.
        float lentezza = 1f - statistiche.VelocitaAttacco / 100f;
        preparazioneAttacco = (a ? armaNuova.preparazione : v[2]) * lentezza;
        colpoAttivo = (a ? armaNuova.colpoAttivo : v[3]) * lentezza;
        recuperoAttacco = (a ? armaNuova.recupero : v[4]) * lentezza;
        portataColpo = a ? armaNuova.portata : v[5];
        raggioColpo = a ? armaNuova.raggio : v[6];
        arcoAttacco = a ? armaNuova.arco : v[7];
        velocitaAffondo = a ? armaNuova.affondo : v[8];
        penetrazioneArma = a ? armaNuova.penetrazioneArmatura : 0f;
        dannoAlleSpalle = a ? armaNuova.moltiplicatoreAlleSpalle : 1f;
        // In mano resta la spada provvisoria: il modello vero dell'arma arriverà con Nazar (DatiOggetto.modello).

        // Sfera magica: con un bastone o una verga equipaggiati valgono i loro numeri, altrimenti quelli dell'Inspector.
        float[] sb = valoriSferaBase;
        float lentezzaLancio = 1f - statistiche.VelocitaAttacco / 100f;
        costoSfera = magica ? armaNuova.costoMana : sb[0];
        dannoSfera = magica ? armaNuova.danno : sb[1];
        portataSfera = magica ? armaNuova.portata : sb[2];
        preparazioneIncantesimo = (magica ? armaNuova.preparazione : sb[3]) * lentezzaLancio;
        recuperoIncantesimo = (magica ? armaNuova.recupero : sb[4]) * lentezzaLancio;
        velocitaSfera = magica ? armaNuova.velocitaIncantesimo : sb[5];
        penetrazioneIncantesimo = magica ? armaNuova.penetrazioneArmatura : 0f;
        bool avevaBastone = bastoneEquipaggiato;
        bastoneEquipaggiato = magica;
        if (magica && !avevaBastone)
        {
            AspettoUmanoide.AggiungiBastone(gameObject);
            ImpugnaArma(ArmaImpugnata.Bastone);
        }
        else if (!magica && avevaBastone && !haBastone && arma == ArmaImpugnata.Bastone)
        {
            ImpugnaArma(ArmaImpugnata.Spada);
        }

        if (scudo != null)
        {
            dannoAssorbitoInParata = scudo.dannoAssorbito;
            costoColpoParato = scudo.costoColpoParato;
            arcoParata = scudo.arcoParata;
            finestraParataPerfetta = scudo.finestraParataPerfetta;
            sbilanciamentoParata = scudo.sbilanciamento;
            dannoSuSbilanciato = scudo.moltiplicatoreDannoSbilanciato;
        }
        else
        {
            // Si para con l'arma (anche con il bastone dello Stregone, che para male).
            bool conArma = armaNuova != null;
            dannoAssorbitoInParata = conArma ? armaNuova.dannoAssorbitoSenzaScudo : v[9];
            costoColpoParato = conArma ? armaNuova.costoParataSenzaScudo : v[10];
            arcoParata = v[11];
            finestraParataPerfetta = 0f;
        }

        schivataExtraArmatura = (armatura != null ? armatura.costoSchivataExtra : 0f) + (scudo != null ? scudo.costoSchivataExtra : 0f)
            + (libro != null ? libro.costoSchivataExtra : 0f);
        velocitaArmatura = (armatura != null ? armatura.moltiplicatoreVelocita : 1f) * (scudo != null ? scudo.moltiplicatoreVelocita : 1f)
            * (libro != null ? libro.moltiplicatoreVelocita : 1f);

        vitaPerUccisione = amuleto != null ? amuleto.ValoreEffetto(DatiAmuleto.Effetto.VitaPerUccisione) : 0f;
        rubaVitaPercento = amuleto != null ? amuleto.ValoreEffetto(DatiAmuleto.Effetto.RubaVita) : 0f;
        durataOmbra = amuleto != null ? amuleto.ValoreEffetto(DatiAmuleto.Effetto.SvanireNellOmbra) : 0f;
        ricaricaOmbra = durataOmbra > 0f ? amuleto.ricarica : 0f;
        manaPerUccisione = amuleto != null ? amuleto.ValoreEffetto(DatiAmuleto.Effetto.ManaPerUccisione) : 0f;
        rubaManaPercento = amuleto != null ? amuleto.ValoreEffetto(DatiAmuleto.Effetto.RubaMana) : 0f;
        // Se il mana massimo scende (amuleto, libro tolto), il mana attuale non può restare sopra.
        Mana = Mathf.Min(Mana, ManaMassimo);
        // Se la vita massima scende (amuleto), la vita attuale non può restare sopra.
        Vita = Mathf.Min(Vita, VitaMassima);

        if (resistenza != null)
        {
            resistenza.MoltiplicatoreRecupero = 1f + (amuleto != null ? amuleto.ValoreEffetto(DatiAmuleto.Effetto.RecuperoResistenza) : 0f) / 100f;
            // Resistenza massima delle Statistiche (amuleti: -10 = il 10% in meno).
            resistenza.MoltiplicatoreMassimo = 1f + statistiche.ResistenzaMassimaPercento / 100f;
        }
    }

    // Tempo per alzare lo scudo, rallentato o velocizzato dalle Statistiche (-5 = il 5% più lento).
    float TempoAlzataScudo => tempoAlzataScudo * (1f - statistiche.VelocitaParata / 100f);

    // ---------- Svanire nell'ombra (amuleto arcano, tasto Q) ----------

    // Vero mentre il giocatore è invisibile ai nemici: non lo vedono, smettono di inseguirlo e non lo attaccano.
    public bool Invisibile => Time.time < invisibileFino;
    // Vero se l'amuleto equipaggiato dà l'abilità del tasto Q (per l'indicatore in HudGioco).
    public bool HaAbilitaOmbra => durataOmbra > 0f;
    // Da 0 (appena usato) a 1 (pronto): per la barra del pannello.
    public float OmbraPronta => ricaricaOmbra <= 0f ? 1f : Mathf.Clamp01(1f - (ombraProntaDa - Time.time) / ricaricaOmbra);

    void SvanisciNellOmbra()
    {
        if (durataOmbra <= 0f || stato == Stato.Morto || Time.time < ombraProntaDa) return;
        invisibileFino = Time.time + durataOmbra;
        ombraProntaDa = Time.time + ricaricaOmbra;
        Suoni.Suona(Suono.Schivata, transform.position + Vector3.up, 0.8f, 0.55f);

        // Il personaggio diventa scuro, quasi un'ombra (il giocatore deve comunque vedersi).
        partiInOmbra.Clear();
        coloriPrimaDellOmbra.Clear();
        foreach (Renderer parte in GetComponentsInChildren<Renderer>())
        {
            if (!parte.enabled) continue;
            partiInOmbra.Add(parte);
            coloriPrimaDellOmbra.Add(parte.material.color);
            parte.material.color = new Color(0.04f, 0.04f, 0.08f);
        }
    }

    // Un'azione che si fa notare (per ora: attaccare) interrompe subito l'invisibilità.
    // Quando ci sarà il tiro con l'arco, va chiamato anche lì.
    public void RompiOmbra()
    {
        if (!Invisibile) return;
        invisibileFino = Time.time;
        AggiornaOmbra();
    }

    // Finita l'invisibilità, il personaggio torna dei suoi colori.
    void AggiornaOmbra()
    {
        if (Invisibile || partiInOmbra.Count == 0) return;
        for (int i = 0; i < partiInOmbra.Count; i++)
            if (partiInOmbra[i] != null) partiInOmbra[i].material.color = coloriPrimaDellOmbra[i];
        partiInOmbra.Clear();
        coloriPrimaDellOmbra.Clear();
    }

    void Cura(float quantita)
    {
        if (quantita <= 0f || stato == Stato.Morto) return;
        Vita = Mathf.Min(VitaMassima, Vita + quantita);
    }

    // Chiamato dal Baule della chiesetta: il giocatore ottiene il bastone, con il mana pieno, e lo impugna.
    public void SbloccaBastone()
    {
        if (haBastone) return;
        haBastone = true;
        Mana = ManaMassimo;
        AspettoUmanoide.AggiungiBastone(gameObject);
        ImpugnaArma(ArmaImpugnata.Bastone);
    }

    // Chiamato da un nemico quando muore: con il bastone si recupera un po' di mana.
    public void NemicoSconfitto()
    {
        Cura(vitaPerUccisione); // amuleto arcano
        if (!HaBastone) return;
        Mana = Mathf.Min(ManaMassimo, Mana + ManaMassimo * manaPerUccisionePercento / 100f + manaPerUccisione);
    }

    // Chiamato da SferaMagica quando la sfera colpisce un nemico: con l'amuleto Cuore del lago nero
    // una parte del danno torna come mana.
    public void IncantesimoASegno(float danno)
    {
        if (rubaManaPercento <= 0f || danno <= 0f) return;
        Mana = Mathf.Min(ManaMassimo, Mana + danno * rubaManaPercento / 100f);
    }

    // Il mana si ricarica da solo dopo una breve pausa dall'ultimo lancio (Docs/mana.md). Libri e amuleti
    // con "Recupero Mana" la rendono più veloce. Da morti non si ricarica.
    void RicaricaMana(float dt)
    {
        if (!HaBastone || stato == Stato.Morto || Mana >= ManaMassimo) return;
        if (Time.time - ultimoLancio < pausaRicaricaMana) return;
        float alSecondo = ManaMassimo / Mathf.Max(0.1f, secondiRicaricaMana) * (1f + statistiche.RecuperoMana / 100f);
        Mana = Mathf.Min(ManaMassimo, Mana + alSecondo * dt);
    }

    bool SonoAgganciato => aggancio != null && aggancio.Agganciato;

    bool DirezioneVersoBersaglio(out Vector3 direzione)
    {
        direzione = Vector3.zero;
        if (!SonoAgganciato) return false;
        direzione = aggancio.Attuale.transform.position - transform.position;
        direzione.y = 0f;
        if (direzione.sqrMagnitude < 0.0001f) return false;
        direzione.Normalize();
        return true;
    }

    // ---------- Esecuzione furtiva ----------

    // Il nemico più vicino che si può giustiziare: vicino, ignaro, con il giocatore alle sue spalle e girato verso di lui.
    // Vero se il giocatore è alle spalle di quel nemico (nell'arco "Arco Alle Spalle" dietro di lui).
    bool DietroA(Transform nemico)
    {
        Vector3 dalNemico = transform.position - nemico.position;
        dalNemico.y = 0f;
        return dalNemico.sqrMagnitude > 0.0001f && Vector3.Angle(nemico.forward, dalNemico) >= 180f - arcoAlleSpalle * 0.5f;
    }

    InseguimentoNemico CercaVittima()
    {
        if (arma != ArmaImpugnata.Spada) return null;
        InseguimentoNemico migliore = null;
        float distanzaMigliore = float.MaxValue;
        foreach (Collider c in Physics.OverlapSphere(transform.position, distanzaEsecuzione, ~0, QueryTriggerInteraction.Ignore))
        {
            InseguimentoNemico nemico = c.GetComponentInParent<InseguimentoNemico>();
            if (nemico == null || !nemico.Ignaro) continue;

            Vector3 dalNemico = transform.position - nemico.transform.position;
            dalNemico.y = 0f;
            float distanza = dalNemico.magnitude;
            if (distanza < 0.01f || distanza > distanzaEsecuzione) continue;
            // Dietro di lui: lontano dal suo sguardo.
            if (Vector3.Angle(nemico.transform.forward, dalNemico) < 180f - arcoAlleSpalle * 0.5f) continue;
            // Il giocatore deve guardare più o meno verso il nemico.
            if (Vector3.Angle(transform.forward, -dalNemico) > 80f) continue;

            if (distanza < distanzaMigliore)
            {
                distanzaMigliore = distanza;
                migliore = nemico;
            }
        }
        return migliore;
    }

    void IniziaEsecuzione(InseguimentoNemico bersaglioFurtivo)
    {
        vittima = bersaglioFurtivo;
        taglioFatto = false;
        attaccoPrenotatoFino = -1f;
        vittima.IniziaEsecuzione();

        // Si mette subito dietro al nemico, guardando nella sua stessa direzione.
        Vector3 avanti = vittima.transform.forward;
        avanti.y = 0f;
        avanti.Normalize();
        Vector3 posto = vittima.transform.position - avanti * 0.85f;
        posto.y = transform.position.y;
        controller.enabled = false;
        transform.SetPositionAndRotation(posto, Quaternion.LookRotation(avanti));
        controller.enabled = true;

        Suoni.Suona(Suono.Schivata, transform.position + Vector3.up, 0.5f, 0.7f);
        CambiaStato(Stato.Esecuzione);
    }

    void AggiornaEsecuzione()
    {
        if (!taglioFatto && tempoNelloStato >= momentoTaglio)
        {
            taglioFatto = true;
            Suoni.Suona(Suono.Fendente, transform.position + Vector3.up * 1.4f, 0.9f, 0.7f);
            if (vittima != null) vittima.Giustizia(transform.position);
        }
        if (tempoNelloStato >= durataEsecuzione)
        {
            vittima = null;
            CambiaStato(Stato.Libero);
        }
    }

    void ControllaColpi()
    {
        Vector3 centro = transform.position + transform.forward * (portataColpo * 0.5f);
        Collider[] trovati = Physics.OverlapSphere(centro, raggioColpo);
        foreach (Collider c in trovati)
        {
            Bersaglio bersaglio = c.GetComponentInParent<Bersaglio>();
            if (bersaglio != null)
            {
                if (colpitiInQuestoAttacco.Contains(bersaglio)) continue;
                if (!NellArcoFrontale(bersaglio.transform.position, arcoAttacco)) continue;

                colpitiInQuestoAttacco.Add(bersaglio);
                float danno = CalcoloDanno.Calcola(dannoAttacco, statistiche, bersaglio.Statistiche, out bool critico, penetrazioneArma);
                if (DietroA(bersaglio.transform)) danno *= dannoAlleSpalle; // colpo alle spalle
                bersaglio.RiceviColpo(danno, transform.position, critico);
                Cura(danno * rubaVitaPercento / 100f); // amuleto arcano
                // Il critico suona più forte e più grave, così si sente senza guardare i numeri.
                Suoni.Suona(Suono.ImpattoColpo, bersaglio.transform.position + Vector3.up, critico ? 1f : 0.9f, critico ? 0.75f : 1f);
                continue;
            }

            // Muri crepati (vedi MuroFragile): un muro è largo, quindi per l'arco conta il suo punto più vicino.
            MuroFragile muro = c.GetComponentInParent<MuroFragile>();
            if (muro == null || colpitiInQuestoAttacco.Contains(muro)) continue;
            if (!NellArcoFrontale(c.ClosestPoint(transform.position), arcoAttacco)) continue;

            colpitiInQuestoAttacco.Add(muro);
            muro.RiceviColpo(transform.position);
        }
    }

    // ---------- Nemici (vedi ObiettiviNemici) ----------

    public Transform Corpo => transform;
    public bool Abbattuto => stato == Stato.Morto;
    public float Furtivita => statistiche != null ? statistiche.Furtivita : 0f;

    // Il colpo di un nemico arriva (anche da un nemico dell'host, in co-op): il danno si calcola qui,
    // con l'armatura e i critici (vedi CalcoloDanno), poi vale tutto quello che fa RiceviColpo (parata, schivata).
    public void ColpitoDaNemico(Bersaglio nemico)
    {
        if (nemico == null) return;
        float danno = CalcoloDanno.Calcola(nemico.DannoAttacco, nemico.Statistiche, statistiche, out bool critico);
        RiceviColpo(danno, nemico.transform.position, critico, nemico);
    }

    // Co-op: chi entra nella partita di un altro parte un po' di lato, per non comparire dentro l'host.
    public void SpostaPartenza(Vector3 spostamento)
    {
        controller.enabled = false;
        transform.position += spostamento;
        controller.enabled = true;
        if (ultimoCheckpoint == null) puntoRinascita += spostamento;
    }

    // Chiamato dai nemici quando un loro colpo arriva. "danno" è già calcolato (armatura e critico compresi).
    // Un critico non parato fa barcollare il doppio.
    // Con uno scudo piccolo, un colpo che arriva subito dopo aver alzato lo scudo è una parata perfetta:
    // niente danno, niente resistenza persa, e il nemico (attaccante) resta sbilanciato.
    public void RiceviColpo(float danno, Vector3 origineColpo, bool critico = false, Bersaglio attaccante = null)
    {
        if (stato == Stato.Morto || stato == Stato.Esecuzione) return;

        if (stato == Stato.Schivata && tempoNelloStato < invulnerabilitaSchivata)
        {
            Debug.Log("Schivato!");
            return;
        }

        // Lo scudo para solo quando è alzato del tutto; la parata perfetta conta da quel momento.
        float alzata = TempoAlzataScudo;
        if (stato == Stato.Parata && tempoNelloStato >= alzata && resistenza.HaResistenza && NellArcoFrontale(origineColpo, arcoParata))
        {
            if (finestraParataPerfetta > 0f && tempoNelloStato <= alzata + finestraParataPerfetta)
            {
                Debug.Log("Parata perfetta!");
                Suoni.Suona(Suono.Parata, transform.position + Vector3.up, 1f, 1.35f);
                if (attaccante != null) attaccante.Sbilancia(sbilanciamentoParata, transform.position, dannoSuSbilanciato);
                return;
            }
            resistenza.Spendi(costoColpoParato);
            PerdiVita(danno * (1f - dannoAssorbitoInParata));
            if (stato == Stato.Morto) return;

            if (!resistenza.HaResistenza)
            {
                Debug.Log("Guardia rotta!");
                Suoni.Suona(Suono.GuardiaRotta, transform.position + Vector3.up);
                Stordisci(durataGuardiaRotta);
            }
            else
            {
                Debug.Log("Parato!");
                Suoni.Suona(Suono.Parata, transform.position + Vector3.up);
            }
            return;
        }

        PerdiVita(danno);
        if (stato != Stato.Morto)
        {
            Stordisci(durataBarcollamento * (critico ? 2f : 1f));
            Suoni.Suona(Suono.Colpito, transform.position + Vector3.up, 1f, critico ? 0.8f : 1f);
        }
    }

    void PerdiVita(float quantita)
    {
        Vita = Mathf.Max(0f, Vita - quantita);
        if (Vita > 0f) return;

        Debug.Log("Sei morto.");
        Suoni.Suona(Suono.Morte, transform.position + Vector3.up);
        CambiaStato(Stato.Morto);
        Invoke(nameof(Rinasci), secondiPerRinascere);
    }

    void Rinasci()
    {
        // Il CharacterController va spento per un attimo, altrimenti non lascia spostare il personaggio di colpo.
        controller.enabled = false;
        transform.SetPositionAndRotation(puntoRinascita, rotazioneRinascita);
        controller.enabled = true;

        velocitaVerticale = 0f;
        armaNelFodero = false;   // si rinasce con l'arma in mano
        inizioGestoFodero = -10f;
        Vita = VitaMassima;
        resistenza.Ripristina();
        CambiaStato(Stato.Libero);
        Suoni.Suona(Suono.Rinascita, transform.position + Vector3.up, 0.8f);
    }

    // Chiamato da un Checkpoint quando il giocatore lo raggiunge: da ora si rinasce lì.
    // Il checkpoint di prima si spegne.
    public void RaggiungiCheckpoint(Checkpoint nuovo, Vector3 posizione, Quaternion rotazione)
    {
        if (ultimoCheckpoint != null && ultimoCheckpoint != nuovo) ultimoCheckpoint.Spegni();
        ultimoCheckpoint = nuovo;
        puntoRinascita = posizione;
        rotazioneRinascita = rotazione;
    }

    // Danno dall'ambiente (trappole, fuoco...): non si può parare, ma la schivata fatta al momento giusto lo evita.
    public void RiceviDannoAmbiente(float danno)
    {
        if (stato == Stato.Morto || stato == Stato.Esecuzione) return;
        if (stato == Stato.Schivata && tempoNelloStato < invulnerabilitaSchivata)
        {
            Debug.Log("Schivato!");
            return;
        }

        PerdiVita(danno);
        if (stato != Stato.Morto)
        {
            Stordisci(durataBarcollamento);
            Suoni.Suona(Suono.Colpito, transform.position + Vector3.up);
        }
    }

    void Stordisci(float durata)
    {
        durataStordimento = durata;
        CambiaStato(Stato.Stordito);
    }

    void CambiaStato(Stato nuovo)
    {
        stato = nuovo;
        tempoNelloStato = 0f;
        staSprintando = false;
    }

    bool NellArcoFrontale(Vector3 punto, float arcoInGradi)
    {
        Vector3 verso = punto - transform.position;
        verso.y = 0f;
        if (verso.sqrMagnitude < 0.0001f) return true;
        return Vector3.Angle(transform.forward, verso) <= arcoInGradi * 0.5f;
    }

    Vector3 DirezioneDaInput(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f) return Vector3.zero;

        Transform cam = cameraRiferimento;
        if (cam == null && Camera.main != null) cam = Camera.main.transform;

        Vector3 avanti = cam != null ? cam.forward : Vector3.forward;
        Vector3 destra = cam != null ? cam.right : Vector3.right;
        avanti.y = 0f;
        destra.y = 0f;
        avanti.Normalize();
        destra.Normalize();

        return Vector3.ClampMagnitude(avanti * input.y + destra * input.x, 1f);
    }

    void RuotaVerso(Vector3 direzione, float dt)
    {
        Quaternion obiettivo = Quaternion.LookRotation(direzione);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, obiettivo, velocitaRotazione * dt);
    }

    // Pannello di prova in alto a sinistra: stato, vita, resistenza e comandi.
    void OnGUI()
    {
        // Pannello di prova: con le barre vere in partita (HudGioco) si vede solo premendo F1.
        if (!HudGioco.Attivo || HudGioco.PannelloProva)
        {
            // Con il bastone in mano il pannello si allunga per la terza barra, quella del mana.
            bool conBastone = arma == ArmaImpugnata.Bastone;
            GUI.Box(new Rect(10, 10, 540, conBastone ? 152 : 112), GUIContent.none);
            string armaTesto = HaBastone ? (conBastone ? "   Arma: Bastone (1 spada)" : "   Arma: Spada (2 bastone)") : "";
            GUI.Label(new Rect(20, 14, 520, 20), "Stato: " + stato + armaTesto + (SonoAgganciato ? "   Agganciato a " + aggancio.Attuale.name : ""));
            GUI.Label(new Rect(20, 32, 280, 20), "Vita " + Mathf.CeilToInt(Vita) + " / " + Mathf.CeilToInt(VitaMassima));
            DisegnaBarra(new Rect(20, 52, 450, 12), Vita / VitaMassima, new Color(0.8f, 0.15f, 0.15f));
            GUI.Label(new Rect(20, 66, 280, 20), "Resistenza");
            DisegnaBarra(new Rect(20, 86, 450, 10), resistenza.Attuale / resistenza.Massimo, new Color(0.2f, 0.75f, 0.3f));
            // Abilità dell'amuleto (Q): barra viola accanto alla resistenza, piena quando è pronta.
            if (durataOmbra > 0f) DisegnaBarra(new Rect(480, 86, 60, 10), Invisibile ? 0f : OmbraPronta, new Color(0.55f, 0.3f, 0.85f));
            GUI.Label(new Rect(20, 98, 520, 20), "WASD muovi, Shift sprint, Spazio schiva, Sx attacca, Dx para, rotellina aggancia");
            if (conBastone)
            {
                GUI.Label(new Rect(20, 116, 280, 20), "Mana " + Mathf.FloorToInt(Mana) + " / " + Mathf.CeilToInt(ManaMassimo));
                DisegnaBarra(new Rect(20, 136, 450, 10), Mana / ManaMassimo, new Color(0.25f, 0.45f, 0.95f));
            }
        }

        // Con l'interfaccia vera, avviso dell'esecuzione e scritta della morte li disegna HudGioco.
        if (vittimaPossibile != null && !HudGioco.Attivo)
        {
            var stileAvviso = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(0f, Screen.height - 90f, Screen.width, 30f), "Tasto sinistro: esecuzione furtiva", stileAvviso);
        }

        if (stato == Stato.Morto && !HudGioco.Attivo)
        {
            var stileMorte = new GUIStyle(GUI.skin.label) { fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            Color primaMorte = GUI.color;
            GUI.color = new Color(0.75f, 0.1f, 0.1f);
            GUI.Label(new Rect(0f, Screen.height * 0.5f - 40f, Screen.width, 80f), "SEI MORTO", stileMorte);
            GUI.color = primaMorte;
        }
    }

    static void DisegnaBarra(Rect area, float frazione, Color colore)
    {
        Color prima = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(area, Texture2D.whiteTexture);
        GUI.color = colore;
        GUI.DrawTexture(new Rect(area.x, area.y, area.width * Mathf.Clamp01(frazione), area.height), Texture2D.whiteTexture);
        GUI.color = prima;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (portataColpo * 0.5f), raggioColpo);
    }
}
