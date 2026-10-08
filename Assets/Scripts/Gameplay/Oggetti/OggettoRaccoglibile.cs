using UnityEngine;
using UnityEngine.InputSystem;

// Un oggetto (arma, scudo, armatura, amuleto) appoggiato nel mondo, da raccogliere con E.
// Quando il giocatore è vicino compare "E  Raccogli: ..." in basso; premendo E (o A sul pad) l'oggetto
// finisce nello zaino e da lì si equipaggia dall'inventario (Tab).
// Aspetto: se l'oggetto ha un modello 3D (fatto da Nazar) si vede quello, altrimenti un segnaposto che ruota.
// Come montarlo: su un oggetto vuoto nella scena, poi trascinare nel campo "Oggetto" il file dell'oggetto
// (per esempio Assets/Dati/Oggetti/Guerriero/Armi/...). Va bene anche dentro un baule o su un altare.
// Co-op: l'oggetto lo prende il primo che lo raccoglie (lo decide l'host) e sparisce per tutti.
public class OggettoRaccoglibile : MonoBehaviour, IOggettoCondiviso
{
    [SerializeField] DatiOggetto oggetto;
    [Tooltip("Distanza massima per raccogliere, in metri.")]
    [SerializeField] float distanza = 2f;

    GiocatoreControllo giocatore;
    InputAction comandoRaccogli;
    Transform aspetto;
    bool raccolto;

    // Un solo oggetto per pressione di E, anche se ce ne sono diversi vicini.
    static int fotogrammaUltimaRaccolta = -1;

    public int NumeroRete { get; private set; }
    float richiestoFino;   // co-op: richiesta mandata all'host, si aspetta la risposta

    void Awake()
    {
        NumeroRete = RegistroCondivisi.Iscrivi(this);
        comandoRaccogli = new InputAction("Raccogli", InputActionType.Button);
        comandoRaccogli.AddBinding("<Keyboard>/e");
        comandoRaccogli.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable() => comandoRaccogli.Enable();
    void OnDisable() => comandoRaccogli.Disable();
    void OnDestroy()
    {
        comandoRaccogli.Dispose();
        RegistroCondivisi.Togli(this, NumeroRete);
    }

    void Start()
    {
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
        CreaAspetto();
    }

    void CreaAspetto()
    {
        aspetto = new GameObject("Aspetto").transform;
        aspetto.SetParent(transform, false);
        if (oggetto != null && oggetto.modello != null)
        {
            Instantiate(oggetto.modello, aspetto, false);
            return;
        }

        // segnaposto: un piccolo blocco caldo che ruota, con una luce
        var blocco = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blocco.name = "Segnaposto";
        Destroy(blocco.GetComponent<Collider>());
        blocco.transform.SetParent(aspetto, false);
        blocco.transform.localPosition = Vector3.up * 0.5f;
        blocco.transform.localScale = new Vector3(0.25f, 0.5f, 0.08f);
        var renderer = blocco.GetComponent<Renderer>();
        renderer.material.color = new Color(0.85f, 0.65f, 0.35f);
        var luce = new GameObject("Luce").AddComponent<Light>();
        luce.transform.SetParent(aspetto, false);
        luce.transform.localPosition = Vector3.up * 0.7f;
        luce.type = LightType.Point;
        luce.color = new Color(1f, 0.7f, 0.4f);
        luce.range = 3f;
        luce.intensity = 1.2f;
    }

    void Update()
    {
        if (raccolto || oggetto == null) return;
        aspetto.localRotation = Quaternion.Euler(0f, Time.time * 80f, 0f);
        aspetto.localPosition = Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.06f);

        if (Time.time < richiestoFino || !Vicino()) return;
        HudGioco.MostraAzione("E", Lingua.T("hud.raccogli") + ": " + oggetto.Nome);
        if (comandoRaccogli.WasPressedThisFrame() && !MenuPausa.InPausa && !InventarioGioco.Aperto
            && fotogrammaUltimaRaccolta != Time.frameCount)
        {
            fotogrammaUltimaRaccolta = Time.frameCount;
            if (MondoRete.ChiediUso(this, 1)) { richiestoFino = Time.time + 2f; return; }   // co-op: decide l'host chi lo prende
            PresoDa(Rete.MioId);
        }
    }

    // L'oggetto lo prende "chi": finisce nel suo zaino (sul suo PC) e sparisce per tutti.
    void PresoDa(ulong chi)
    {
        if (raccolto) return;
        MondoRete.InviaEvento(this, chi, 1);
        if (chi == Rete.MioId) Raccogli();
        else { raccolto = true; gameObject.SetActive(false); }
    }

    // ---------- co-op (IOggettoCondiviso) ----------
    public void UsaDaRete(ulong chi, int valore, Vector3 punto) => PresoDa(chi);
    public void EventoDaRete(ulong chi, int valore, Vector3 punto)
    {
        if (raccolto) return;
        if (chi == Rete.MioId) Raccogli();
        else { raccolto = true; gameObject.SetActive(false); }
    }
    public int StatoRete => raccolto ? 1 : 0;
    public void StatoDaRete(int stato)
    {
        if (stato == 1 && !raccolto) { raccolto = true; gameObject.SetActive(false); }
    }

    bool Vicino()
    {
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        Vector3 d = giocatore.transform.position - transform.position;
        if (Mathf.Abs(d.y) > 2.5f) return false;
        d.y = 0f;
        return d.magnitude <= distanza;
    }

    void Raccogli()
    {
        raccolto = true;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore != null) Zaino.Di(giocatore).Aggiungi(oggetto);
        MessaggiSchermo.Mostra(Lingua.T("hud.raccolto") + ": " + oggetto.Nome, 3f);
        // Nascosto e non distrutto: in co-op resta nell'elenco, così chi entra dopo sa che è già stato preso.
        gameObject.SetActive(false);
    }
}
