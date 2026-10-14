using UnityEngine;
using UnityEngine.InputSystem;

// Il tiro con l'arco (e con la balestra) del Ladro.
// Come si gioca: con un'arma a distanza addosso, il tasto 2 prende l'arco in mano (il tasto 1 torna al pugnale).
// Con l'arco in mano si tiene premuto il tasto di Attacco per tendere: quando la carica (il numero "Carica"
// dell'arma) è piena compare il segno al centro dello schermo acceso; lasciando il tasto parte la freccia.
// Lasciando prima del tempo il tiro si annulla. Ogni tiro costa resistenza ("Costo Tiro") e dopo bisogna
// aspettare la "Ricarica". Con un nemico agganciato la freccia va verso di lui (e lo segue un poco); senza
// aggancio va dove punta la camera.
// Danno: quello dell'arma, con armatura ignorata e critico dell'arma; contro un nemico che non si è accorto di te
// (ignaro) si moltiplica per "Moltiplicatore Non Visto" (l'assassino che colpisce da lontano).
// Con l'arco in mano non si attacca corpo a corpo e non si fanno esecuzioni (lo controlla GiocatoreControllo).
// La freccia per ora è una piccola luce color legno (SferaMagica): l'arte vera arriverà dopo.
// Co-op: il danno passa da Bersaglio.RiceviColpo come tutti i colpi; gli altri vedono partire il proiettile.
// Come montarlo: non si monta. Lo aggiunge da solo GiocatoreControllo sul giocatore.
[RequireComponent(typeof(GiocatoreControllo))]
public class TiroConLArco : MonoBehaviour
{
    static readonly Color ColoreFreccia = new Color(0.85f, 0.65f, 0.35f);

    GiocatoreControllo giocatore;
    Equipaggiamento equipaggiamento;
    Resistenza resistenza;
    AggancioBersaglio aggancio;
    InputAction comandoArco, comandoPugnale, comandoTira;

    bool inMano;
    bool tende;
    float inizioTensione;
    float prossimoTiro;
    Transform arcoVisibile;
    DatiArmaDistanza arcoMostrato;

    // Vero quando il Ladro ha l'arco in mano (GiocatoreControllo non attacca corpo a corpo).
    public bool ArcoInMano => inMano;
    // Quante frecce sono partite da quando è iniziata la scena (per esempio per il tutorial di Brannoc).
    public int Tiri { get; private set; }
    // Da 0 a 1 mentre si tende l'arco (1 = pronto a tirare).
    public float Tensione => tende && Arco != null ? Mathf.Clamp01((Time.time - inizioTensione) / Mathf.Max(0.05f, Arco.carica)) : 0f;

    DatiArmaDistanza Arco => equipaggiamento != null ? equipaggiamento.ArmaDistanza : null;
    bool Ladro => SceltaPartita.Classe == ClasseGiocatore.Ladro;

    void Awake()
    {
        giocatore = GetComponent<GiocatoreControllo>();
        resistenza = GetComponent<Resistenza>();
        aggancio = GetComponent<AggancioBersaglio>();

        // 1 e 2 sono tasti fissi (armi e incantesimi), come in GiocatoreControllo: non si cambiano nelle Opzioni.
        comandoPugnale = new InputAction("ArcoVia", InputActionType.Button);
        comandoPugnale.AddBinding("<Keyboard>/1");
        comandoPugnale.AddBinding("<Gamepad>/dpad/left");
        comandoArco = new InputAction("ArcoInMano", InputActionType.Button);
        comandoArco.AddBinding("<Keyboard>/2");
        comandoArco.AddBinding("<Gamepad>/dpad/right");
        comandoTira = new InputAction("TiraConLArco", InputActionType.Button);
        comandoTira.AddBinding("<Mouse>/leftButton");
        comandoTira.AddBinding("<Gamepad>/buttonWest");
        Comandi.Collega(comandoTira, Azione.Attacca, this);   // stesso tasto dell'attacco, scelto in Opzioni > Comandi
    }

    void OnEnable()
    {
        comandoPugnale.Enable();
        comandoArco.Enable();
        comandoTira.Enable();
    }

    void OnDisable()
    {
        comandoPugnale.Disable();
        comandoArco.Disable();
        comandoTira.Disable();
    }

    void OnDestroy()
    {
        comandoPugnale.Dispose();
        comandoArco.Dispose();
        comandoTira.Dispose();
    }

    void Update()
    {
        if (equipaggiamento == null) equipaggiamento = GetComponent<Equipaggiamento>();
        if (aggancio == null) aggancio = GetComponent<AggancioBersaglio>();
        bool bloccato = Dialogo.InCorso || MenuPausa.InPausa || InventarioGioco.Aperto;
        bool vivo = giocatore.StatoAttuale != GiocatoreControllo.Stato.Morto;

        // Senza arco addosso (o se non si è Ladro) l'arco non si può tenere in mano.
        if (inMano && (!Ladro || Arco == null || !vivo)) MettiVia();

        if (!bloccato && vivo && Ladro && Arco != null && giocatore.StatoAttuale != GiocatoreControllo.Stato.Attacco)
        {
            if (comandoArco.WasPressedThisFrame()) inMano = true;
            if (comandoPugnale.WasPressedThisFrame()) MettiVia();
        }

        AggiornaAspetto();
        if (!inMano) return;

        bool libero = giocatore.StatoAttuale == GiocatoreControllo.Stato.Libero;
        if (!bloccato && libero && !tende && comandoTira.WasPressedThisFrame())
        {
            if (Time.time < prossimoTiro) return;
            if (resistenza != null && !resistenza.HaResistenza) return;
            tende = true;
            inizioTensione = Time.time;
        }
        if (!tende) return;
        if (!libero || bloccato) { tende = false; return; }   // una schivata o un colpo subito annullano il tiro
        if (comandoTira.IsPressed()) return;

        // Tasto lasciato: si tira solo se l'arco era teso del tutto.
        bool pronto = Tensione >= 1f;
        tende = false;
        if (pronto) Tira();
    }

    void LateUpdate()
    {
        // Mentre tende, il personaggio guarda dove mira.
        if (!tende) return;
        Vector3 d = DirezioneMira(out _);
        d.y = 0f;
        if (d.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(d.normalized);
    }

    void MettiVia()
    {
        inMano = false;
        tende = false;
    }

    void Tira()
    {
        var arco = Arco;
        if (arco == null) return;
        if (resistenza != null) resistenza.Spendi(arco.costoTiro);
        prossimoTiro = Time.time + arco.ricarica;
        Tiri++;

        Vector3 partenza = transform.position + Vector3.up * 0.55f + transform.forward * 0.6f;
        Vector3 direzione = DirezioneMira(out Bersaglio obiettivo);
        if (direzione.sqrMagnitude < 0.001f) direzione = transform.forward;
        float durata = arco.portata / Mathf.Max(1f, arco.velocitaFreccia);
        Statistiche mie = giocatore.Statistiche;
        SferaMagica.LanciaIncantesimo(partenza, direzione, obiettivo, arco.velocitaFreccia, transform, ColoreFreccia, 0.45f, durata,
            (colpito, punto) => { if (colpito != null) Colpisci(colpito, arco, mie); });
        MondoRete.InviaSfera(partenza, direzione, obiettivo, arco.velocitaFreccia);   // gli altri giocatori la vedono partire
    }

    void Colpisci(Bersaglio b, DatiArmaDistanza arco, Statistiche mie)
    {
        if (b == null || b.Morto) return;
        float danno = arco.danno;
        var guardia = b.GetComponent<InseguimentoNemico>();
        if (guardia != null && guardia.Ignaro) danno *= arco.moltiplicatoreNonVisto;
        float finale = CalcoloDanno.Calcola(danno, mie, b.Statistiche, out bool critico, arco.penetrazioneArmatura);
        // Il critico dell'arma si aggiunge a quello del personaggio.
        if (!critico && Random.value * 100f < arco.probabilitaCritico)
        {
            critico = true;
            finale *= 1.75f;
        }
        if (critico) finale *= 1f + arco.moltiplicatoreCritico / 1.75f;
        b.RiceviColpo(finale, transform.position, critico);
    }

    // Verso il nemico agganciato; altrimenti dove punta la camera (fino alla portata dell'arco).
    Vector3 DirezioneMira(out Bersaglio obiettivo)
    {
        obiettivo = null;
        Vector3 partenza = transform.position + Vector3.up * 0.55f;
        if (aggancio != null && aggancio.Attuale != null && !aggancio.Attuale.Morto)
        {
            obiettivo = aggancio.Attuale;
            return (obiettivo.transform.position + Vector3.up * 0.3f - partenza).normalized;
        }
        Camera cam = Camera.main;
        if (cam == null) return transform.forward;
        float portata = Arco != null ? Arco.portata : 30f;
        Ray raggio = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 punto = raggio.origin + raggio.direction * portata;
        foreach (var colpo in Physics.RaycastAll(raggio, portata + 10f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (colpo.transform.IsChildOf(transform)) continue;
            if (Vector3.Dot(colpo.point - partenza, raggio.direction) < 0.5f) continue;   // cose fra camera e personaggio
            if ((colpo.point - raggio.origin).sqrMagnitude < (punto - raggio.origin).sqrMagnitude) punto = colpo.point;
        }
        return (punto - partenza).normalized;
    }

    // L'arco in mano sinistra e la lama nascosta mentre si usa l'arco; tutto come prima quando lo si mette via.
    void AggiornaAspetto()
    {
        Transform spada = Trova(transform, "Spada");
        Transform schiena = Trova(transform, "Schiena (oggetti)");
        bool mostra = inMano && Arco != null;
        if (spada != null && spada.parent != null && spada.parent.name == "Mano destra" && spada.gameObject.activeSelf == mostra)
            spada.gameObject.SetActive(!mostra);
        if (schiena != null && schiena.gameObject.activeSelf == mostra) schiena.gameObject.SetActive(!mostra);

        if (!mostra || arcoMostrato != Arco)
        {
            if (arcoVisibile != null) Destroy(arcoVisibile.gameObject);
            arcoVisibile = null;
            arcoMostrato = null;
        }
        if (!mostra || arcoVisibile != null) return;
        Transform mano = Trova(transform, "Mano sinistra") ?? Trova(transform, "Mano destra");
        if (mano == null) return;
        var gruppo = new GameObject("Arco in mano").transform;
        gruppo.SetParent(mano, false);
        gruppo.localRotation = Quaternion.Euler(35f, 0f, 0f);
        FormeOggetti.Crea(Arco, gruppo);
        arcoVisibile = gruppo;
        arcoMostrato = Arco;
    }

    static Transform Trova(Transform da, string nome)
    {
        foreach (Transform figlio in da)
        {
            if (figlio.name == nome) return figlio;
            Transform trovato = Trova(figlio, nome);
            if (trovato != null) return trovato;
        }
        return null;
    }

    // Il segno di mira al centro dello schermo, con l'anello che si riempie mentre si tende.
    void OnGUI()
    {
        if (!inMano || !HudGioco.Attivo || MenuPausa.InPausa || InventarioGioco.Aperto || Dialogo.InCorso) return;
        GUI.depth = 10;
        GraficaMenu.PreparaStili();
        float larghezza = GraficaMenu.FoglioIntero();
        var centro = new Vector2(larghezza * 0.5f, GraficaMenu.Altezza * 0.5f);
        float t = Tensione;
        bool pronto = t >= 1f;
        Color colore = pronto ? GraficaMenu.Selezione : GraficaMenu.Con(GraficaMenu.Testo, 0.7f);
        GraficaMenu.Rombo(centro, pronto ? 16f : 12f, GraficaMenu.Con(Color.black, 0.6f));
        GraficaMenu.Rombo(centro, pronto ? 10f : 6f, colore);
        if (tende && !pronto)
        {
            var barra = new Rect(centro.x - 40f, centro.y + 22f, 80f, 6f);
            GraficaMenu.Riempi(barra, new Color(0f, 0f, 0f, 0.6f));
            GraficaMenu.Riempi(new Rect(barra.x, barra.y, barra.width * t, barra.height), GraficaMenu.Bronzo);
        }
    }
}
