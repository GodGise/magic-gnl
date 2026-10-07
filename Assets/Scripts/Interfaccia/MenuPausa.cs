using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Menu di pausa durante la partita.
// Esc (o Start sul pad) ferma il gioco e apre il menu: Riprendi, Opzioni (le stesse del menu iniziale),
// Torna al menu principale, Esci dal gioco. Le ultime due chiedono conferma. Esc, B o "Riprendi" tornano a giocare.
// Mentre è aperto: il tempo è fermo, i suoni sono in pausa, il cursore del mouse è libero e i comandi del
// giocatore, della camera e dell'aggancio sono spenti (si riaccendono alla ripresa).
// Si crea da solo all'avvio del gioco e resta attivo in tutte le scene, tranne in quella del menu iniziale:
// non va messo in nessuna scena.
// Co-op: in partita in rete il tempo non si ferma (gli altri continuano a giocare): il menu resta aperto, il proprio
// personaggio sta fermo e i nemici possono ancora colpirlo. "Torna al menu principale" lascia la partita; se lo fa
// chi ospita, la partita finisce per tutti (lo dice anche la domanda di conferma).
public class MenuPausa : MonoBehaviour
{
    [Tooltip("Nome della scena del menu iniziale.")]
    [SerializeField] string scenaMenu = "Menu";

    enum Schermata { Chiuso, Pausa, Opzioni, ConfermaMenu, ConfermaEsci }

    const float Larghezza = GraficaMenu.Larghezza;

    readonly ElencoMenu elenco = new ElencoMenu();
    readonly List<Behaviour> spenti = new List<Behaviour>();
    Schermata schermata = Schermata.Chiuso;
    float tempoSchermata;
    bool nelMenuIniziale;
    bool riprendiAlProssimoFotogramma;
    float scalaTempoPrima = 1f;
    bool tempoFermato;
    CursorLockMode cursorePrima;
    bool cursoreVisibilePrima;

    public static bool InPausa { get; private set; }
    // Fotogramma in cui si è aperta la pausa: l'inventario non si apre con lo stesso tasto nello stesso fotogramma.
    public static int FotogrammaApertura { get; private set; } = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreaAllAvvio()
    {
        if (FindFirstObjectByType<MenuPausa>() != null) return;
        var oggetto = new GameObject("Menu di pausa");
        DontDestroyOnLoad(oggetto);
        oggetto.AddComponent<MenuPausa>();
    }

    void OnEnable() => SceneManager.sceneLoaded += SceneCaricata;
    void OnDisable() => SceneManager.sceneLoaded -= SceneCaricata;

    void Start() => nelMenuIniziale = FindFirstObjectByType<MenuPrincipale>() != null;

    void SceneCaricata(Scene scena, LoadSceneMode modo)
    {
        if (schermata != Schermata.Chiuso) Chiudi(false);
        nelMenuIniziale = FindFirstObjectByType<MenuPrincipale>() != null;
    }

    // ---------- apri e chiudi ----------

    void Apri()
    {
        // In rete il tempo non si ferma: gli altri giocatori continuano.
        tempoFermato = !Rete.Attiva;
        if (tempoFermato)
        {
            scalaTempoPrima = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }
        cursorePrima = Cursor.lockState;
        cursoreVisibilePrima = Cursor.visible;

        spenti.Clear();
        foreach (var b in FindObjectsByType<GiocatoreControllo>(FindObjectsSortMode.None)) Spegni(b);
        foreach (var b in FindObjectsByType<CameraTerzaPersona>(FindObjectsSortMode.None)) Spegni(b);
        foreach (var b in FindObjectsByType<AggancioBersaglio>(FindObjectsSortMode.None)) Spegni(b);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InPausa = true;
        FotogrammaApertura = Time.frameCount;
        VaiA(Schermata.Pausa);
    }

    void Spegni(Behaviour b)
    {
        if (b == null || !b.enabled) return;
        b.enabled = false;
        spenti.Add(b);
    }

    // ripristina = true: rimette i comandi e il cursore com'erano (si torna a giocare).
    void Chiudi(bool ripristina)
    {
        if (tempoFermato)
        {
            Time.timeScale = scalaTempoPrima;
            AudioListener.pause = false;
            tempoFermato = false;
        }
        if (ripristina)
        {
            foreach (var b in spenti)
                if (b != null) b.enabled = true;
            Cursor.lockState = cursorePrima;
            Cursor.visible = cursoreVisibilePrima;
        }
        spenti.Clear();
        InPausa = false;
        schermata = Schermata.Chiuso;
        elenco.Pulisci();
    }

    // La ripresa avviene al fotogramma dopo, così lo stesso tasto Esc non viene letto anche dalla camera.
    void Riprendi() => riprendiAlProssimoFotogramma = true;

    void TornaAlMenu()
    {
        Chiudi(false);
        Time.timeScale = 1f;
        // In rete: si lascia la partita (ReteCoop spegne la rete e carica il menu).
        if (Rete.Attiva && ReteCoop.Istanza != null)
        {
            ReteCoop.Istanza.Esci();
            return;
        }
        if (Application.CanStreamedLevelBeLoaded(scenaMenu)) SceneManager.LoadScene(scenaMenu);
        else SceneManager.LoadScene(0);
    }

    void EsciDalGioco()
    {
        Chiudi(false);
        if (Rete.Attiva && ReteCoop.Istanza != null) ReteCoop.Istanza.Spegni();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- schermate ----------

    void VaiA(Schermata nuova)
    {
        schermata = nuova;
        tempoSchermata = 0f;
        elenco.Pulisci();
        switch (nuova)
        {
            case Schermata.Pausa:
                elenco.Aggiungi(() => Lingua.T("pausa.riprendi"), Riprendi);
                elenco.Aggiungi(() => Lingua.T("menu.opzioni"), () => VaiA(Schermata.Opzioni));
                elenco.Aggiungi(() => Lingua.T("pausa.menu_principale"), () => VaiA(Schermata.ConfermaMenu));
                elenco.Aggiungi(() => Lingua.T("pausa.esci_gioco"), () => VaiA(Schermata.ConfermaEsci));
                break;
            case Schermata.Opzioni:
                Impostazioni.AggiungiVoci(elenco, () => VaiA(Schermata.Pausa));
                break;
            case Schermata.ConfermaMenu:
                elenco.Aggiungi(() => Lingua.T("comune.no"), () => VaiA(Schermata.Pausa));
                elenco.Aggiungi(() => Lingua.T("comune.si"), TornaAlMenu);
                break;
            case Schermata.ConfermaEsci:
                elenco.Aggiungi(() => Lingua.T("comune.no"), () => VaiA(Schermata.Pausa));
                elenco.Aggiungi(() => Lingua.T("comune.si"), EsciDalGioco);
                break;
        }
    }

    // ---------- comandi ----------

    void Update()
    {
        if (riprendiAlProssimoFotogramma)
        {
            riprendiAlProssimoFotogramma = false;
            if (schermata != Schermata.Chiuso) Chiudi(true);
            return;
        }
        if (nelMenuIniziale) return;

        var tastiera = Keyboard.current;
        var pad = Gamepad.current;
        bool start = pad != null && pad.startButton.wasPressedThisFrame;

        if (schermata == Schermata.Chiuso)
        {
            // con l'inventario aperto Esc chiude l'inventario, non apre la pausa
            if (InventarioGioco.Aperto || InventarioGioco.FotogrammaChiusura == Time.frameCount) return;
            if ((tastiera != null && tastiera.escapeKey.wasPressedThisFrame) || start) Apri();
            return;
        }

        tempoSchermata += Time.unscaledDeltaTime;
        var c = elenco.LeggiComandi();
        if (c.indietro || start)
        {
            if (schermata == Schermata.Pausa) Riprendi();
            else VaiA(Schermata.Pausa);
            return;
        }
        elenco.Applica(c);
    }

    // ---------- disegno ----------

    void OnGUI()
    {
        if (schermata == Schermata.Chiuso) return;
        GUI.depth = -100;   // sopra a tutte le altre scritte del gioco

        GraficaMenu.PreparaStili();
        float comparsa = Mathf.Clamp01(tempoSchermata / 0.25f);
        GraficaMenu.Atmosfera(0.6f);
        GraficaMenu.FoglioVirtuale();
        elenco.InizioGUI();

        // intestazione
        string intestazione = schermata == Schermata.Opzioni ? Lingua.T("menu.opzioni") : Lingua.T("pausa.titolo");
        GraficaMenu.Alone(new Rect(Larghezza * 0.5f - 520f, 40f, 1040f, 320f), new Color(1f, 0.5f, 0.2f, 0.07f));
        GraficaMenu.Scritta(new Rect(0, 150, Larghezza, 90), intestazione.ToUpperInvariant(), GraficaMenu.Intestazione, GraficaMenu.Testo, 1f);
        GraficaMenu.Divisore(Larghezza * 0.5f, 262f, 520f, 1f);

        bool bloccato = riprendiAlProssimoFotogramma;
        switch (schermata)
        {
            case Schermata.Pausa:
                if (elenco.DisegnaElenco(360f, 640f, comparsa, bloccato)) return;
                break;
            case Schermata.Opzioni:
                if (elenco.DisegnaOpzioni(330f, comparsa, bloccato)) return;
                break;
            case Schermata.ConfermaMenu:
            case Schermata.ConfermaEsci:
            {
                string domanda = Lingua.T(schermata == Schermata.ConfermaMenu ? "pausa.conferma_menu" : "pausa.conferma_esci");
                // Chi ospita: uscendo, la partita finisce anche per gli altri giocatori.
                if (ReteCoop.Istanza != null && ReteCoop.Istanza.SonoHost) domanda += "\n" + Lingua.T("rete.avviso_host_esce");
                GraficaMenu.Scritta(new Rect(Larghezza * 0.5f - 600f, 305, 1200f, 135), domanda, GraficaMenu.Descrizione, GraficaMenu.Testo, comparsa);
                if (elenco.DisegnaElenco(450f, 440f, comparsa, bloccato)) return;
                break;
            }
        }

        GraficaMenu.Scritta(new Rect(0, 1030, Larghezza, 30), Lingua.T("menu.aiuto"), GraficaMenu.Piccolo, GraficaMenu.Spento, 0.8f * comparsa);
    }
}
