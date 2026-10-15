using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Baule da aprire con E, con degli oggetti dentro.
// A cosa serve: quando il giocatore è vicino compare "E  Apri il baule"; premendo E il coperchio si
// alza e dal baule esce una luce dorata. Con Contenuto = BastoneMagico il giocatore riceve subito il bastone della
// chiesetta. Con Contenuto = Oggetto si apre la finestra del baule (FinestraBaule, richiesta di Lorenzo del
// 15 ottobre): a sinistra cosa c'è dentro, a destra lo zaino, e gli oggetti si trascinano da una parte all'altra
// (niente raccolta automatica). Dentro ci sono "Oggetto", gli "Altri Oggetti" e, se sullo stesso oggetto c'è un
// DonaOggetti, la lista della classe del giocatore (per esempio il baule delle armi della gattabuia).
// Dopo la prima apertura il baule resta aperto: con E ("Guarda nel baule") si riapre la finestra, anche per
// lasciarci qualcosa.
// "Quando Aperto" parte quando il baule viene svuotato la prima volta (per esempio per cambiare l'obiettivo dopo aver
// preso il medaglione); se il baule è vuoto fin dall'inizio, parte subito.
// Come montarlo:
//   1. GameObject > Create Empty, mettilo a terra e giralo (Rotation Y) con il davanti verso chi arriva.
//   2. Add Component > Baule e scegli il Contenuto.
//   L'aspetto del baule (legno, fasce di ferro, coperchio) si crea da solo quando parte il gioco;
//   nella vista Scene è segnato da un riquadro marrone.
// Co-op: il baule si apre per tutti, ma il contenuto lo vede e lo prende solo chi l'ha aperto.
public class Baule : MonoBehaviour, IOggettoCondiviso
{
    public enum Contenuto { BastoneMagico, Oggetto }

    [SerializeField] Contenuto contenuto = Contenuto.BastoneMagico;
    [Tooltip("Con Contenuto = Oggetto: un oggetto che sta nel baule.")]
    [SerializeField] DatiOggetto oggetto;
    [Tooltip("Con Contenuto = Oggetto: altri oggetti nel baule.")]
    [SerializeField] DatiOggetto[] altriOggetti;
    [Tooltip("Cosa succede quando chi l'ha aperto ha svuotato il baule la prima volta (subito, se è vuoto).")]
    [SerializeField] UnityEvent quandoAperto;
    [Tooltip("Distanza (in orizzontale) entro cui si può aprire.")]
    [SerializeField] float raggioInterazione = 2f;
    [Tooltip("Secondi che impiega il coperchio ad aprirsi.")]
    [SerializeField] float durataApertura = 0.7f;

    GiocatoreControllo giocatore;
    InputAction comandoInteragisci;
    Transform coperchio;
    bool aperto;
    readonly List<DatiOggetto> dentro = new List<DatiOggetto>();
    bool mio;          // l'ho aperto io: posso guardarci dentro
    bool svuotato;     // "Quando Aperto" è già partito

    public int NumeroRete { get; private set; }
    float richiestoFino;     // co-op: richiesta mandata all'host, si aspetta la risposta (poi si può riprovare)
    bool apertoDaRete;       // co-op: chi entra dopo trova il baule già aperto (si applica quando c'è l'aspetto)

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
        if (apertoDaRete) coperchio.localRotation = Quaternion.Euler(110f, 0f, 0f);
    }

    void Update()
    {
        if (aperto && mio && contenuto == Contenuto.Oggetto)
        {
            // già aperto: si può riguardare dentro (e lasciarci qualcosa)
            if (giocatore == null || FinestraBaule.Aperto || MenuPausa.InPausa || InventarioGioco.Aperto || Dialogo.InCorso || !Vicino(giocatore)) return;
            if (giocatore.StatoAttuale != GiocatoreControllo.Stato.Libero) return;
            HudGioco.MostraAzione("E", Lingua.T("hud.baule_guarda"));
            if (comandoInteragisci.WasPressedThisFrame()) FinestraBaule.Apri(this, dentro, giocatore);
            return;
        }
        if (!PuoAprire()) return;
        HudGioco.MostraAzione("E", Lingua.T("hud.baule"));
        if (!comandoInteragisci.WasPressedThisFrame() || MenuPausa.InPausa || InventarioGioco.Aperto) return;
        if (MondoRete.ChiediUso(this, 1)) { richiestoFino = Time.time + 2f; return; }   // co-op: lo apre l'host per tutti
        ApriDa(Rete.MioId);
    }

    void ApriDa(ulong chi)
    {
        if (aperto) return;
        StartCoroutine(Apri(chi));
        MondoRete.InviaEvento(this, chi, 1);
    }

    // ---------- co-op (IOggettoCondiviso) ----------
    public void UsaDaRete(ulong chi, int valore, Vector3 punto) => ApriDa(chi);
    public void EventoDaRete(ulong chi, int valore, Vector3 punto)
    {
        if (!aperto && coperchio != null) StartCoroutine(Apri(chi));
    }
    public int StatoRete => aperto ? 1 : 0;
    public void StatoDaRete(int stato)
    {
        if (stato != 1 || aperto) return;
        aperto = true;
        apertoDaRete = true;
        if (coperchio != null) coperchio.localRotation = Quaternion.Euler(110f, 0f, 0f);
    }

    bool PuoAprire()
    {
        if (aperto || Time.time < richiestoFino || giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        return Vicino(giocatore);
    }

    // Il giocatore è abbastanza vicino per usare il baule (la finestra si chiude se si allontana).
    public bool Vicino(GiocatoreControllo g)
    {
        if (g == null) return false;
        Vector3 distanza = g.transform.position - transform.position;
        distanza.y = 0f;
        return distanza.magnitude <= raggioInterazione + 0.5f;
    }

    // La finestra ha spostato degli oggetti: al primo svuotamento parte "Quando Aperto".
    public void ContenutoCambiato()
    {
        if (svuotato || dentro.Count > 0) return;
        svuotato = true;
        quandoAperto?.Invoke();
    }

    public void FinestraChiusa() { }

    // "chi" ha aperto il baule: solo lui riceve il contenuto (da soli sei sempre tu).
    IEnumerator Apri(ulong chi)
    {
        aperto = true;
        Suoni.Suona(Suono.BauleAperto, transform.position + Vector3.up * 0.5f);

        // Luce dorata che esce dal baule mentre si apre, poi si spegne piano.
        Light luce = new GameObject("Luce baule").AddComponent<Light>();
        luce.transform.SetParent(transform, false);
        luce.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        luce.type = LightType.Point;
        luce.color = new Color(1f, 0.8f, 0.4f);
        luce.range = 5f;

        // Il coperchio ruota all'indietro attorno al bordo posteriore.
        for (float t = 0f; t < durataApertura; t += Time.deltaTime)
        {
            float s = Mathf.SmoothStep(0f, 1f, t / durataApertura);
            coperchio.localRotation = Quaternion.Euler(110f * s, 0f, 0f);
            luce.intensity = 3f * s;
            yield return null;
        }
        coperchio.localRotation = Quaternion.Euler(110f, 0f, 0f);

        if (chi == Rete.MioId) DaiContenuto();

        for (float t = 0f; t < 1.5f; t += Time.deltaTime)
        {
            luce.intensity = Mathf.Lerp(3f, 0.6f, t / 1.5f);
            yield return null;
        }
    }

    void DaiContenuto()
    {
        switch (contenuto)
        {
            case Contenuto.BastoneMagico:
                if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
                if (giocatore != null) giocatore.SbloccaBastone();
                MessaggiSchermo.Mostra(Lingua.T("hud.trovato_bastone"), 5f);
                break;
            case Contenuto.Oggetto:
                if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
                RiempiDentro();
                mio = true;
                if (dentro.Count == 0) { ContenutoCambiato(); return; }
                if (giocatore != null) FinestraBaule.Apri(this, dentro, giocatore);
                return;
        }
        quandoAperto?.Invoke();
    }

    // Cosa c'è nel baule: Oggetto, Altri Oggetti e la lista della classe di un DonaOggetti sullo stesso oggetto.
    void RiempiDentro()
    {
        dentro.Clear();
        if (oggetto != null) dentro.Add(oggetto);
        if (altriOggetti != null)
            foreach (var o in altriOggetti) if (o != null) dentro.Add(o);
        var dona = GetComponent<DonaOggetti>();
        if (dona != null)
            foreach (var o in dona.PrendiLista()) if (o != null) dentro.Add(o);
    }

    // Baule a blocchi: cassa di legno con fasce di ferro e coperchio incernierato dietro.
    // Il davanti del baule guarda verso -Z (la direzione opposta alla freccia blu dell'oggetto).
    void CreaAspetto()
    {
        Transform aspetto = new GameObject("Aspetto baule").transform;
        aspetto.SetParent(transform, false);

        Material legno = null, ferro = null;
        legno = Parte(aspetto, "Cassa", new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 0.7f), new Color(0.32f, 0.2f, 0.1f), legno, true);
        ferro = Parte(aspetto, "Fascia sinistra", new Vector3(-0.4f, 0.3f, 0f), new Vector3(0.08f, 0.62f, 0.72f), new Color(0.3f, 0.3f, 0.32f), ferro, false);
        Parte(aspetto, "Fascia destra", new Vector3(0.4f, 0.3f, 0f), new Vector3(0.08f, 0.62f, 0.72f), Color.white, ferro, false);

        // Il perno del coperchio sta sul bordo alto posteriore della cassa.
        coperchio = new GameObject("Coperchio").transform;
        coperchio.SetParent(aspetto, false);
        coperchio.localPosition = new Vector3(0f, 0.6f, 0.35f);
        Parte(coperchio, "Tavola coperchio", new Vector3(0f, 0.11f, -0.35f), new Vector3(1.22f, 0.22f, 0.72f), Color.white, legno, false);
        Parte(coperchio, "Fascia coperchio", new Vector3(0f, 0.11f, -0.35f), new Vector3(0.1f, 0.24f, 0.74f), Color.white, ferro, false);
        Parte(coperchio, "Serratura", new Vector3(0f, 0.02f, -0.72f), new Vector3(0.14f, 0.16f, 0.04f), Color.white, ferro, false);
    }

    // Crea un pezzo; se "materiale" è vuoto ne crea uno nuovo del colore dato e lo restituisce, per riusarlo.
    Material Parte(Transform genitore, string nome, Vector3 posizione, Vector3 scala, Color colore, Material materiale, bool tieniCollider)
    {
        GameObject parte = GameObject.CreatePrimitive(PrimitiveType.Cube);
        parte.name = nome;
        if (!tieniCollider) DestroyImmediate(parte.GetComponent<Collider>());
        parte.transform.SetParent(genitore, false);
        parte.transform.localPosition = posizione;
        parte.transform.localScale = scala;

        Renderer r = parte.GetComponent<Renderer>();
        if (materiale == null) materiale = new Material(r.sharedMaterial) { color = colore };
        r.sharedMaterial = materiale;
        return materiale;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.7f, 0.45f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 0.7f));
    }
}
