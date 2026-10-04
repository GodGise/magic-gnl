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
// Con l'aggancio del bersaglio attivo (vedi AggancioBersaglio) il personaggio guarda sempre il nemico:
// A e D girano attorno al nemico, S indietreggia, attacchi e schivate partono verso di lui.
// Come montarlo: su un oggetto con CharacterController (aggiunto in automatico insieme a Resistenza).
// Il modo più rapido è il menu "magic-gnl > Crea scena di prova", che prepara tutto da solo.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Resistenza))]
[RequireComponent(typeof(AggancioBersaglio))]
public class GiocatoreControllo : MonoBehaviour
{
    public enum Stato { Libero, Parata, Attacco, Schivata, Stordito, Morto }

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

    [Header("Altro")]
    [Tooltip("Per quanti secondi un tasto premuto in anticipo resta valido.")]
    [SerializeField] float memoriaComandi = 0.2f;
    [SerializeField] float durataBarcollamento = 0.3f;

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
    public float VitaMassima => vitaMassima;

    CharacterController controller;
    Resistenza resistenza;
    AggancioBersaglio aggancio;
    InputAction comandoMuovi, comandoSchiva, comandoAttacca, comandoPara, comandoSprint;
    bool staSprintando;

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
    float schivataPrenotataFino = -1f;
    float attaccoPrenotatoFino = -1f;
    // Nemici e muri crepati già colpiti da questo attacco (ognuno una volta sola per colpo).
    readonly HashSet<MonoBehaviour> colpitiInQuestoAttacco = new HashSet<MonoBehaviour>();

    bool SchivataRichiesta => Time.time <= schivataPrenotataFino;
    bool AttaccoRichiesto => Time.time <= attaccoPrenotatoFino;

    void Awake()
    {
        // Aspetto provvisorio da figura umana al posto della capsula (vedi AspettoUmanoide).
        AspettoUmanoide.Prepara(gameObject, new Color(0.05f, 0.05f, 0.06f), AspettoUmanoide.Arma.Spada);

        controller = GetComponent<CharacterController>();
        resistenza = GetComponent<Resistenza>();
        aggancio = GetComponent<AggancioBersaglio>();
        Vita = vitaMassima;
        puntoRinascita = transform.position;
        rotazioneRinascita = transform.rotation;
        CreaComandi();
    }

    void OnEnable()
    {
        comandoMuovi.Enable();
        comandoSchiva.Enable();
        comandoAttacca.Enable();
        comandoPara.Enable();
        comandoSprint.Enable();
    }

    void OnDisable()
    {
        comandoMuovi.Disable();
        comandoSchiva.Disable();
        comandoAttacca.Disable();
        comandoPara.Disable();
        comandoSprint.Disable();
    }

    void OnDestroy()
    {
        comandoMuovi.Dispose();
        comandoSchiva.Dispose();
        comandoAttacca.Dispose();
        comandoPara.Dispose();
        comandoSprint.Dispose();
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
    }

    void Update()
    {
        float dt = Time.deltaTime;
        tempoNelloStato += dt;

        // Caduto nel vuoto: muore subito (e rinasce al checkpoint).
        if (stato != Stato.Morto && transform.position.y < quotaVuoto) PerdiVita(Vita);

        if (comandoSchiva.WasPressedThisFrame()) schivataPrenotataFino = Time.time + memoriaComandi;
        if (comandoAttacca.WasPressedThisFrame()) attaccoPrenotatoFino = Time.time + memoriaComandi;

        Vector3 direzioneInput = DirezioneDaInput(comandoMuovi.ReadValue<Vector2>());
        Vector3 movimento = Vector3.zero;

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

            case Stato.Morto:
                break;
        }

        resistenza.InPausaRecupero = stato == Stato.Parata;

        if (controller.isGrounded && velocitaVerticale < 0f) velocitaVerticale = -2f;
        velocitaVerticale += gravita * dt;

        controller.Move((movimento + Vector3.up * velocitaVerticale) * dt);
    }

    Vector3 AggiornaLiberoOParata(Vector3 direzioneInput, float dt)
    {
        // Priorità: schivata, poi attacco, poi parata.
        if (SchivataRichiesta && resistenza.HaResistenza)
        {
            IniziaSchivata(direzioneInput);
            return Vector3.zero;
        }
        if (AttaccoRichiesto && resistenza.HaResistenza)
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
        return direzioneInput * velocita;
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
        float fineColpo = preparazioneAttacco + colpoAttivo;
        float fineAttacco = fineColpo + recuperoAttacco;

        if (tempoNelloStato < preparazioneAttacco) return Vector3.zero;

        if (tempoNelloStato < fineColpo)
        {
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
        resistenza.Spendi(costoSchivata);
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

    // concatenato = attacco fatto durante il recupero del precedente: passa al colpo dopo della combo (1, 2, poi di nuovo 0).
    void IniziaAttacco(bool concatenato = false)
    {
        colpoCombo = concatenato ? (colpoCombo + 1) % 3 : 0;
        resistenza.Spendi(costoAttacco);
        attaccoPrenotatoFino = -1f;
        colpitiInQuestoAttacco.Clear();
        if (DirezioneVersoBersaglio(out Vector3 versoBersaglio)) transform.rotation = Quaternion.LookRotation(versoBersaglio);
        CambiaStato(Stato.Attacco);
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
                bersaglio.RiceviColpo(dannoAttacco, transform.position);
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

    // Chiamato dai nemici quando un loro colpo arriva.
    public void RiceviColpo(float danno, Vector3 origineColpo)
    {
        if (stato == Stato.Morto) return;

        if (stato == Stato.Schivata && tempoNelloStato < invulnerabilitaSchivata)
        {
            Debug.Log("Schivato!");
            return;
        }

        if (stato == Stato.Parata && resistenza.HaResistenza && NellArcoFrontale(origineColpo, arcoParata))
        {
            resistenza.Spendi(costoColpoParato);
            PerdiVita(danno * (1f - dannoAssorbitoInParata));
            if (stato == Stato.Morto) return;

            if (!resistenza.HaResistenza)
            {
                Debug.Log("Guardia rotta!");
                Stordisci(durataGuardiaRotta);
            }
            else
            {
                Debug.Log("Parato!");
            }
            return;
        }

        PerdiVita(danno);
        if (stato != Stato.Morto) Stordisci(durataBarcollamento);
    }

    void PerdiVita(float quantita)
    {
        Vita = Mathf.Max(0f, Vita - quantita);
        if (Vita > 0f) return;

        Debug.Log("Sei morto.");
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
        Vita = vitaMassima;
        resistenza.Ripristina();
        CambiaStato(Stato.Libero);
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
        if (stato == Stato.Morto) return;
        if (stato == Stato.Schivata && tempoNelloStato < invulnerabilitaSchivata)
        {
            Debug.Log("Schivato!");
            return;
        }

        PerdiVita(danno);
        if (stato != Stato.Morto) Stordisci(durataBarcollamento);
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
        GUI.Box(new Rect(10, 10, 540, 112), GUIContent.none);
        GUI.Label(new Rect(20, 14, 450, 20), "Stato: " + stato + (SonoAgganciato ? "   Agganciato a " + aggancio.Attuale.name : ""));
        GUI.Label(new Rect(20, 32, 280, 20), "Vita " + Mathf.CeilToInt(Vita) + " / " + Mathf.CeilToInt(vitaMassima));
        DisegnaBarra(new Rect(20, 52, 450, 12), Vita / vitaMassima, new Color(0.8f, 0.15f, 0.15f));
        GUI.Label(new Rect(20, 66, 280, 20), "Resistenza");
        DisegnaBarra(new Rect(20, 86, 450, 10), resistenza.Attuale / resistenza.Massimo, new Color(0.2f, 0.75f, 0.3f));
        GUI.Label(new Rect(20, 98, 520, 20), "WASD muovi, Shift sprint, Spazio schiva, Sx attacca, Dx para, rotellina aggancia");

        if (stato == Stato.Morto)
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
