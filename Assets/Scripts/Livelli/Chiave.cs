using UnityEngine;
using UnityEngine.InputSystem;

// Chiave da raccogliere con E.
// A cosa serve: una chiave dorata che ruota piano e brilla (si vede anche al buio). Quando il
// giocatore è vicino compare "E  Raccogli: ..."; premendo E la chiave sparisce e finisce
// nell'Inventario. Apre la Serratura che ha lo stesso codice (per esempio "chiesa").
// Come montarlo:
//   1. GameObject > Create Empty, mettilo dove vuoi la chiave, a circa 1 metro da terra.
//   2. Add Component > Chiave, e scrivi nel campo Codice lo stesso codice della Serratura da aprire.
//   L'aspetto della chiave si crea da solo quando parte il gioco; nella vista Scene è segnata in giallo.
// Co-op: la chiave è del gruppo. Quando un giocatore la raccoglie sparisce per tutti e tutti possono aprire la porta.
public class Chiave : MonoBehaviour, IOggettoCondiviso
{
    [Tooltip("Codice della chiave: deve essere uguale al Codice della Serratura che apre.")]
    [SerializeField] string codice = "chiesa";
    [Tooltip("Nome mostrato al giocatore.")]
    [SerializeField] string nomeVisibile = "Chiave della chiesa";
    [Tooltip("Distanza (in orizzontale) entro cui si può raccogliere.")]
    [SerializeField] float raggioRaccolta = 1.8f;
    [SerializeField] Color colore = new Color(1f, 0.78f, 0.3f);

    GiocatoreControllo giocatore;
    InputAction comandoInteragisci;
    Transform aspetto;
    bool raccolta;

    // Stesso tasto di leva e checkpoint: E sulla tastiera, A (Xbox) o Croce (PS) sul pad.
    public int NumeroRete { get; private set; }
    float richiestaFino;   // co-op: richiesta mandata all'host, si aspetta la risposta

    void Awake()
    {
        NumeroRete = RegistroCondivisi.Iscrivi(this);
        comandoInteragisci = new InputAction("Interagisci", InputActionType.Button);
        comandoInteragisci.AddBinding("<Keyboard>/e");
        comandoInteragisci.AddBinding("<Gamepad>/buttonSouth");
        Comandi.Collega(comandoInteragisci, Azione.Interagisci, this);   // tasto scelto in Opzioni > Comandi
    }

    void OnEnable() => comandoInteragisci.Enable();
    void OnDisable() => comandoInteragisci.Disable();
    void OnDestroy()
    {
        comandoInteragisci.Dispose();
        RegistroCondivisi.Togli(this, NumeroRete);
    }

    void Start()
    {
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
        CreaAspetto();
    }

    void Update()
    {
        if (raccolta) return;

        // Ruota e ondeggia piano, per farsi notare.
        aspetto.localRotation = Quaternion.Euler(0f, Time.time * 90f, 0f);
        aspetto.localPosition = Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.08f);

        if (!PuoRaccogliere()) return;
        HudGioco.MostraAzione("E", Lingua.T("hud.raccogli") + ": " + nomeVisibile);
        if (!comandoInteragisci.WasPressedThisFrame() || MenuPausa.InPausa || InventarioGioco.Aperto) return;
        if (MondoRete.ChiediUso(this, 1)) { richiestaFino = Time.time + 2f; return; }   // co-op: decide l'host
        Raccogli();
        MondoRete.InviaEvento(this, Rete.MioId, 1);
    }

    bool PuoRaccogliere()
    {
        if (raccolta || Time.time < richiestaFino || giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        Vector3 distanza = giocatore.transform.position - transform.position;
        if (Mathf.Abs(distanza.y) > 2.5f) return false;
        distanza.y = 0f;
        return distanza.magnitude <= raggioRaccolta;
    }

    // La chiave va nell'inventario del giocatore di questo PC (in co-op ce l'hanno tutti).
    // daCompagno: l'ha raccolta un altro giocatore (il messaggio lo dice).
    void Raccogli(bool conMessaggio = true, bool daCompagno = false)
    {
        if (raccolta) return;
        raccolta = true;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore != null) Inventario.Di(giocatore).AggiungiChiave(codice);
        if (conMessaggio)
        {
            if (!daCompagno) Suoni.Suona(Suono.Raccolta, transform.position);
            MessaggiSchermo.Mostra(Lingua.T(daCompagno ? "hud.raccolto_compagno" : "hud.raccolto") + ": " + nomeVisibile, 3f);
        }
        gameObject.SetActive(false);
    }

    // ---------- co-op (IOggettoCondiviso) ----------
    public void UsaDaRete(ulong chi, int valore, Vector3 punto)
    {
        if (raccolta) return;
        Raccogli(true, chi != Rete.MioId);
        MondoRete.InviaEvento(this, chi, 1);
    }
    public void EventoDaRete(ulong chi, int valore, Vector3 punto) => Raccogli(true, chi != Rete.MioId);
    public int StatoRete => raccolta ? 1 : 0;
    public void StatoDaRete(int stato) { if (stato == 1) Raccogli(false); }

    // Chiave a blocchi: anello, gambo e due denti, dorata e luminosa, con una piccola luce.
    void CreaAspetto()
    {
        aspetto = new GameObject("Aspetto chiave").transform;
        aspetto.SetParent(transform, false);

        Material oro = null;
        oro = Parte("Anello", PrimitiveType.Cylinder, new Vector3(0f, 0.16f, 0f), new Vector3(0.2f, 0.02f, 0.2f), Quaternion.Euler(90f, 0f, 0f), oro);
        Parte("Gambo", PrimitiveType.Cube, new Vector3(0f, -0.06f, 0f), new Vector3(0.04f, 0.34f, 0.04f), Quaternion.identity, oro);
        Parte("Dente basso", PrimitiveType.Cube, new Vector3(0.05f, -0.19f, 0f), new Vector3(0.08f, 0.04f, 0.03f), Quaternion.identity, oro);
        Parte("Dente alto", PrimitiveType.Cube, new Vector3(0.04f, -0.12f, 0f), new Vector3(0.06f, 0.035f, 0.03f), Quaternion.identity, oro);

        Light luce = new GameObject("Luce chiave").AddComponent<Light>();
        luce.transform.SetParent(aspetto, false);
        luce.type = LightType.Point;
        luce.color = colore;
        luce.range = 4f;
        luce.intensity = 1.5f;
    }

    // Crea un pezzo; la prima volta crea anche il materiale dorato e lo restituisce, così gli altri pezzi lo riusano.
    Material Parte(string nome, PrimitiveType tipo, Vector3 posizione, Vector3 scala, Quaternion rotazione, Material materiale)
    {
        GameObject parte = GameObject.CreatePrimitive(tipo);
        parte.name = nome;
        DestroyImmediate(parte.GetComponent<Collider>());
        parte.transform.SetParent(aspetto, false);
        parte.transform.localPosition = posizione;
        parte.transform.localRotation = rotazione;
        parte.transform.localScale = scala;

        Renderer r = parte.GetComponent<Renderer>();
        if (materiale == null)
        {
            materiale = new Material(r.sharedMaterial) { color = colore };
            materiale.EnableKeyword("_EMISSION");
            materiale.SetColor("_EmissionColor", colore * 0.7f);
        }
        r.sharedMaterial = materiale;
        return materiale;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(transform.position, new Vector3(0.2f, 0.45f, 0.2f));
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, raggioRaccolta);
    }
}
