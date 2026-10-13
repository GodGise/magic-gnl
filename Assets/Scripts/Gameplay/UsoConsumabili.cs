using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// L'uso degli oggetti consumabili (pozioni): il tasto "Usa oggetto" (F all'inizio, si cambia in Opzioni > Comandi;
// freccia giù sul pad) usa l'oggetto scelto dello zaino, che sparisce. La Pozione di Ilse ridà vita in un secondo;
// le altre pozioni (Docs/erbe-e-pozioni.md) ridanno anche mana e resistenza o fanno ricaricare più in fretta la resistenza.
// Quale oggetto si usa: quello scelto nell'inventario (scheda Varie, E o doppio clic); se non è stato scelto niente,
// o è finito, il primo consumabile dello zaino.
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
    InputAction comandoUsa;
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
    }

    void OnEnable()
    {
        comandoUsa.Enable();
        SceneManager.sceneLoaded += SceneCaricata;
    }

    void OnDisable()
    {
        comandoUsa.Disable();
        SceneManager.sceneLoaded -= SceneCaricata;
    }

    void OnDestroy()
    {
        comandoUsa.Dispose();
        if (Istanza == this) Istanza = null;
    }

    void SceneCaricata(Scene s, LoadSceneMode m)
    {
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
        string effetti = oggetto.Effetti();
        string testo = oggetto.Nome + (effetti.Length > 0 ? "   " + effetti : "");
        MessaggiSchermo.Mostra(testo, 2.5f);
        Usato?.Invoke(oggetto);
    }

    // Rombo in basso a sinistra, sopra le armi: l'oggetto pronto, quante copie e il tasto. Solo se c'è qualcosa da usare.
    void OnGUI()
    {
        if (giocatore == null || !HudGioco.Attivo || MenuPausa.InPausa || InventarioGioco.Aperto) return;
        var oggetto = Pronto();
        if (oggetto == null) return;
        GUI.depth = 10;
        GraficaMenu.PreparaStili();
        GraficaMenu.FoglioIntero();
        var centro = new Vector2(110f, GraficaMenu.Altezza - 320f);
        const float lato = 84f;
        bool attesa = Time.time < prontoDa;
        Color bordo = attesa ? GraficaMenu.Spento : GraficaMenu.Bronzo;
        GraficaMenu.Rombo(centro, lato, bordo);
        GraficaMenu.Rombo(centro, lato - 8f, new Color(0.025f, 0.025f, 0.035f, 0.9f));
        if (oggetto.icona != null) GraficaMenu.Icona(new Rect(centro.x - 21f, centro.y - 21f, 42f, 42f), oggetto, 1f);
        else
        {
            // boccetta disegnata: corpo tondo del colore della pozione, collo e tappo
            GraficaMenu.Rombo(centro + new Vector2(0f, 6f), 30f, oggetto.colore);
            GraficaMenu.Riempi(new Rect(centro.x - 4f, centro.y - 22f, 8f, 14f), new Color(0.75f, 0.8f, 0.82f, 0.9f));
            GraficaMenu.Riempi(new Rect(centro.x - 6f, centro.y - 26f, 12f, 5f), new Color(0.45f, 0.32f, 0.2f));
        }
        // il tasto in un piccolo rombo color fiamma, poi nome e copie
        var tasto = new Vector2(centro.x + lato * 0.5f + 22f, centro.y);
        GraficaMenu.Rombo(tasto, 34f, attesa ? GraficaMenu.Spento : GraficaMenu.Selezione);
        GraficaMenu.Rombo(tasto, 28f, new Color(0.025f, 0.025f, 0.035f, 0.95f));
        var stileTasto = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
        GraficaMenu.Scritta(new Rect(tasto.x - 30f, tasto.y - 14f, 60f, 28f), Comandi.NomeTasto(Azione.UsaOggetto), stileTasto,
            attesa ? GraficaMenu.Spento : GraficaMenu.Testo, 1f);
        var stile = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft };
        GraficaMenu.Scritta(new Rect(tasto.x + 26f, centro.y - 14f, 360f, 28f), oggetto.Nome + "  x" + Quanti(oggetto), stile, GraficaMenu.Testo, 1f);
    }
}
