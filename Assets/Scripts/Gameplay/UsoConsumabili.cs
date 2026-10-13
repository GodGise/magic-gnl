using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// L'uso degli oggetti consumabili (pozioni): il tasto "Usa oggetto" (F all'inizio, si cambia in Opzioni > Comandi;
// freccia giù sul pad) usa l'oggetto scelto dello zaino, che sparisce. La Pozione di Ilse ridà vita in un secondo;
// le altre pozioni (Docs/erbe-e-pozioni.md) ridanno anche mana e resistenza o fanno ricaricare più in fretta la resistenza.
// Quale oggetto si usa: quello scelto nell'inventario (scheda Varie, E o doppio clic) o con il tasto "Cambia oggetto"
// (C all'inizio, si cambia in Opzioni > Comandi; Y / Triangolo sul pad), che passa al consumabile dopo dello zaino
// (Lorenzo, 13 ottobre); se non è stato scelto niente, o è finito, il primo consumabile dello zaino.
// In basso a sinistra, sopra le armi, un rombo mostra l'oggetto pronto, quante copie ci sono e il tasto.
// Si usa solo da fermi o camminando: premuto a metà di un attacco, una schivata o una parata, parte appena finiscono
// (se entro 0,4 secondi). Non durante un dialogo, con l'inventario aperto o in pausa.
// Le missioni possono sapere quando si usa un oggetto con l'evento UsoConsumabili.Usato (per esempio QuandoUsato).
// Si crea da solo all'avvio del gioco e funziona in ogni scena con un giocatore: non va messo nelle scene.
public class UsoConsumabili : MonoBehaviour
{
    public static UsoConsumabili Istanza { get; private set; }
    // Avvisa quando il giocatore usa un oggetto (dopo averlo tolto dallo zaino).
    public static event System.Action<DatiConsumabile> Usato;

    // L'oggetto pronto per il tasto (null = il primo consumabile dello zaino).
    public static DatiConsumabile Scelto => Istanza != null ? Istanza.Pronto() : null;

    GiocatoreControllo giocatore;
    InputAction comandoUsa, comandoCambia;
    DatiConsumabile scelto;
    float prontoDa;            // prima di questo momento non si può usare un altro oggetto
    float curaRimasta, curaAlSecondo;
    float premutoFino = -1f;   // tasto premuto a metà di un attacco o di una schivata: si usa appena si è liberi

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreaAllAvvio()
    {
        if (FindFirstObjectByType<UsoConsumabili>() != null) return;
        var oggetto = new GameObject("Uso dei consumabili");
        DontDestroyOnLoad(oggetto);
        oggetto.AddComponent<UsoConsumabili>();
    }

    void Awake()
    {
        Istanza = this;
        comandoUsa = new InputAction("UsaOggetto", InputActionType.Button);
        comandoUsa.AddBinding("<Keyboard>/f");
        comandoUsa.AddBinding("<Gamepad>/dpad/down");
        Comandi.Collega(comandoUsa, Azione.UsaOggetto, this);   // tasto scelto in Opzioni > Comandi
        comandoCambia = new InputAction("CambiaOggetto", InputActionType.Button);
        comandoCambia.AddBinding("<Keyboard>/c");
        comandoCambia.AddBinding("<Gamepad>/buttonNorth");
        Comandi.Collega(comandoCambia, Azione.CambiaOggetto, this);
    }

    void OnEnable()
    {
        comandoUsa.Enable();
        comandoCambia.Enable();
        SceneManager.sceneLoaded += SceneCaricata;
    }

    void OnDisable()
    {
        comandoUsa.Disable();
        comandoCambia.Disable();
        SceneManager.sceneLoaded -= SceneCaricata;
    }

    void OnDestroy()
    {
        comandoUsa.Dispose();
        comandoCambia.Dispose();
        if (Istanza == this) Istanza = null;
    }

    void SceneCaricata(Scene s, LoadSceneMode m)
    {
        if (m == LoadSceneMode.Additive) return;   // una zona caricata in aggiunta (la cripta): il giocatore è lo stesso
        giocatore = null;
        scelto = null;
        curaRimasta = 0f;
    }

    // Sceglie quale oggetto usare con il tasto (dall'inventario).
    public static void Seleziona(DatiConsumabile oggetto)
    {
        if (Istanza != null) Istanza.scelto = oggetto;
    }

    // Quante copie di un oggetto ci sono nello zaino del giocatore.
    public static int Quanti(DatiOggetto oggetto)
    {
        var g = Istanza != null ? Istanza.giocatore : null;
        if (g == null) g = FindFirstObjectByType<GiocatoreControllo>();
        var zaino = g != null ? g.GetComponent<Zaino>() : null;
        if (zaino == null || oggetto == null) return 0;
        int n = 0;
        foreach (var o in zaino.Oggetti) if (o == oggetto) n++;
        return n;
    }

    // Passa al consumabile dopo (tipi diversi, nell'ordine dello zaino) e lo scrive in alto.
    void Cambia()
    {
        var zaino = giocatore.GetComponent<Zaino>();
        if (zaino == null) return;
        var tipi = new System.Collections.Generic.List<DatiConsumabile>();
        foreach (var o in zaino.Oggetti)
            if (o is DatiConsumabile c && !tipi.Contains(c)) tipi.Add(c);
        if (tipi.Count == 0)
        {
            MessaggiSchermo.Mostra(Lingua.T("hud.niente_da_usare"), 2f);
            Suoni.Suona(Suono.Negato, giocatore.transform.position + Vector3.up, 0.5f);
            return;
        }
        int i = tipi.IndexOf(Pronto());
        scelto = tipi[(i + 1) % tipi.Count];
        MessaggiSchermo.Mostra(scelto.Nome + "  x" + Quanti(scelto), 1.5f);
        Suoni.Suona(Suono.CambioArma, giocatore.transform.position + Vector3.up, 0.45f, 1.1f);
    }

    // Quanti consumabili diversi ci sono nello zaino (per mostrare il tasto "Cambia oggetto" solo se serve).
    int TipiDiversi()
    {
        var zaino = giocatore != null ? giocatore.GetComponent<Zaino>() : null;
        if (zaino == null) return 0;
        var tipi = new System.Collections.Generic.HashSet<DatiConsumabile>();
        foreach (var o in zaino.Oggetti) if (o is DatiConsumabile c) tipi.Add(c);
        return tipi.Count;
    }

    DatiConsumabile Pronto()
    {
        var zaino = giocatore != null ? giocatore.GetComponent<Zaino>() : null;
        if (zaino == null) return null;
        if (scelto != null && zaino.Contiene(scelto)) return scelto;
        foreach (var o in zaino.Oggetti)
            if (o is DatiConsumabile c) return c;
        return null;
    }

    void Update()
    {
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;

        // la cura arriva un po' alla volta
        if (curaRimasta > 0f)
        {
            if (giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) curaRimasta = 0f;
            else
            {
                float ora = curaAlSecondo > 0f ? Mathf.Min(curaRimasta, curaAlSecondo * Time.deltaTime) : curaRimasta;
                giocatore.Cura(ora);
                curaRimasta -= ora;
            }
        }

        if (comandoCambia.WasPressedThisFrame() && !MenuPausa.InPausa && !InventarioGioco.Aperto && !Dialogo.InCorso) Cambia();
        if (comandoUsa.WasPressedThisFrame()) premutoFino = Time.time + 0.4f;
        if (Time.time > premutoFino || MenuPausa.InPausa || InventarioGioco.Aperto || Dialogo.InCorso) return;
        if (giocatore.StatoAttuale != GiocatoreControllo.Stato.Libero || Time.time < prontoDa) return;
        premutoFino = -1f;
        var oggetto = Pronto();
        if (oggetto == null)
        {
            MessaggiSchermo.Mostra(Lingua.T("hud.niente_da_usare"), 2f);
            Suoni.Suona(Suono.Negato, giocatore.transform.position + Vector3.up, 0.5f);
            return;
        }
        Usa(oggetto);
    }

    void Usa(DatiConsumabile oggetto)
    {
        var zaino = Zaino.Di(giocatore);
        if (!zaino.Togli(oggetto)) return;
        prontoDa = Time.time + oggetto.attesa;
        curaRimasta += oggetto.cura;
        curaAlSecondo = oggetto.durataCura > 0f ? oggetto.cura / oggetto.durataCura : 0f;
        Suoni.Suona(Suono.Raccolta, giocatore.transform.position + Vector3.up, 0.8f, 0.8f);
        if (oggetto.mana > 0f) giocatore.RidaiMana(oggetto.mana);
        var resistenza = giocatore.GetComponent<Resistenza>();
        if (resistenza != null)
        {
            resistenza.Recupera(oggetto.resistenza);
            if (oggetto.recuperoResistenzaPercento > 0f && oggetto.durataEffetto > 0f)
                resistenza.Potenzia(oggetto.recuperoResistenzaPercento, oggetto.durataEffetto);
        }
        var effetti = EffettiPozioni.Di(giocatore);
        effetti.Aggiungi(EffettiPozioni.Tipo.RiduzioneDanni, oggetto.riduzioneDanniPercento, oggetto.durataEffetto, oggetto.cumulabile);
        float manaPrima = giocatore.ManaMassimo;
        effetti.Aggiungi(EffettiPozioni.Tipo.ManaMassimo, oggetto.manaMassimoPercento, oggetto.durataEffetto, oggetto.cumulabile);
        // il mana in più arriva subito: la barra piena resta piena
        if (giocatore.ManaMassimo > manaPrima) giocatore.RidaiMana(giocatore.ManaMassimo - manaPrima);
        string elenco = oggetto.Effetti();
        string testo = oggetto.Nome + (elenco.Length > 0 ? "   " + elenco : "");
        MessaggiSchermo.Mostra(testo, 2.5f);
        Usato?.Invoke(oggetto);
    }

    // Rombo in basso a sinistra, sopra le armi: l'oggetto pronto, quante copie e il tasto. Solo se c'è qualcosa da usare.
    void OnGUI()
    {
        if (giocatore == null || !HudGioco.Attivo || MenuPausa.InPausa || InventarioGioco.Aperto) return;
        GUI.depth = 10;
        GraficaMenu.PreparaStili();
        GraficaMenu.FoglioIntero();
        DisegnaEffetti();
        var oggetto = Pronto();
        if (oggetto == null) return;
        var centro = new Vector2(110f, GraficaMenu.Altezza - 320f);
        const float lato = 84f;
        bool attesa = Time.time < prontoDa;
        Color bordo = attesa ? GraficaMenu.Spento : GraficaMenu.Bronzo;
        GraficaMenu.Rombo(centro, lato, bordo);
        GraficaMenu.Rombo(centro, lato - 8f, new Color(0.025f, 0.025f, 0.035f, 0.9f));
        // l'icona di Nazar, oppure la boccetta del colore della pozione (GraficaMenu.Icona)
        GraficaMenu.Icona(new Rect(centro.x - 26f, centro.y - 26f, 52f, 52f), oggetto, 1f);
        // il tasto in un piccolo rombo color fiamma, poi nome e copie
        var tasto = new Vector2(centro.x + lato * 0.5f + 22f, centro.y);
        GraficaMenu.Rombo(tasto, 34f, attesa ? GraficaMenu.Spento : GraficaMenu.Selezione);
        GraficaMenu.Rombo(tasto, 28f, new Color(0.025f, 0.025f, 0.035f, 0.95f));
        var stileTasto = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
        GraficaMenu.Scritta(new Rect(tasto.x - 30f, tasto.y - 14f, 60f, 28f), Comandi.NomeTasto(Azione.UsaOggetto), stileTasto,
            attesa ? GraficaMenu.Spento : GraficaMenu.Testo, 1f);
        var stile = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft };
        GraficaMenu.Scritta(new Rect(tasto.x + 26f, centro.y - 14f, 360f, 28f), oggetto.Nome + "  x" + Quanti(oggetto), stile, GraficaMenu.Testo, 1f);
        // con più consumabili diversi: sotto il nome, il tasto per passare al prossimo
        if (TipiDiversi() > 1)
        {
            var piccolo = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft, fontSize = 18 };
            GraficaMenu.Scritta(new Rect(tasto.x + 26f, centro.y + 12f, 360f, 24f),
                Comandi.NomeTasto(Azione.CambiaOggetto) + "  " + Lingua.T("hud.cambia_oggetto"), piccolo, GraficaMenu.Testo, 0.75f);
        }
    }

    // Gli effetti a tempo delle pozioni bevute, sopra il rombo: "Danni subiti -10%  2:41", "Mana massimo +5%  1:10".
    void DisegnaEffetti()
    {
        var effetti = giocatore.GetComponent<EffettiPozioni>();
        if (effetti == null) return;
        var stile = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft, fontSize = 20 };
        float y = GraficaMenu.Altezza - 410f;
        foreach (EffettiPozioni.Tipo tipo in System.Enum.GetValues(typeof(EffettiPozioni.Tipo)))
        {
            float totale = effetti.Totale(tipo);
            if (totale <= 0f) continue;
            string chiave = tipo == EffettiPozioni.Tipo.RiduzioneDanni ? "eff.breve_danni" : "eff.breve_mana";
            string testo = string.Format(Lingua.T(chiave), Mathf.RoundToInt(totale)) + "   " + EffettiPozioni.Orologio(effetti.SecondiRimasti(tipo));
            GraficaMenu.Rombo(new Vector2(78f, y + 14f), 12f, tipo == EffettiPozioni.Tipo.RiduzioneDanni ? new Color(0.55f, 0.35f, 0.65f) : new Color(0.35f, 0.5f, 0.95f));
            GraficaMenu.Scritta(new Rect(94f, y, 420f, 28f), testo, stile, GraficaMenu.Testo, 1f);
            y -= 32f;
        }
    }
}
