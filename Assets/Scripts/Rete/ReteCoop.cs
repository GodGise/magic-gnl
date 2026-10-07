using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Co-op in rete, prima fase: prova in locale (due finestre di gioco, oppure due PC nella stessa rete).
// A cosa serve: avvia Netcode for GameObjects con il trasporto di base di Unity (niente Steam per ora),
// fa partire una partita come host o ci entra come giocatore, e fa comparire gli altri giocatori
// (vedi GiocatoreRete). Massimo 3 giocatori.
// Come montarlo: non va montato su niente, si crea da solo in ogni scena di gioco (non nel menu iniziale).
// Comandi: F6 ospita, F7 entra (all'indirizzo scritto nel pannello, di base 127.0.0.1 = questo stesso PC),
// F8 disconnette, F9 apre e chiude il pannello con l'indirizzo.
// Il prefab "Resources/Rete/GiocatoreRete" lo crea da solo l'editor la prima volta (Assets/Editor/CreaPrefabRete.cs).
// Per ora il giocatore locale resta com'è: ogni giocatore comanda il proprio personaggio e gli altri si vedono come
// sagome che si muovono. Combattimento e nemici in rete arrivano nelle fasi successive (Docs/rete-coop.md).
public class ReteCoop : MonoBehaviour
{
    public const int GiocatoriMassimi = 3;
    const ushort Porta = 7777;
    const string PercorsoPrefab = "Rete/GiocatoreRete";

    public static ReteCoop Istanza { get; private set; }

    NetworkManager rete;
    UnityTransport trasporto;
    GameObject prefabGiocatore;
    string indirizzo = "127.0.0.1";
    bool pannelloAperto;
    string errore;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Crea()
    {
        if (Istanza != null) return;
        var oggetto = new GameObject("ReteCoop");
        DontDestroyOnLoad(oggetto);
        oggetto.AddComponent<ReteCoop>();
    }

    void Awake()
    {
        if (Istanza != null) { Destroy(gameObject); return; }
        Istanza = this;

        // Si configura con l'oggetto spento, così Netcode parte solo quando è tutto pronto.
        gameObject.SetActive(false);
        trasporto = gameObject.AddComponent<UnityTransport>();
        rete = gameObject.AddComponent<NetworkManager>();
        rete.NetworkConfig = new NetworkConfig
        {
            NetworkTransport = trasporto,
            EnableSceneManagement = false,   // ogni giocatore carica la propria scena
            ConnectionApproval = true,       // serve a rifiutare il quarto giocatore
        };
        gameObject.SetActive(true);

        prefabGiocatore = Resources.Load<GameObject>(PercorsoPrefab);
        if (prefabGiocatore == null)
            errore = "Manca il prefab Resources/Rete/GiocatoreRete: chiudere e riaprire Unity (lo crea da solo).";
        else if (prefabGiocatore.GetComponent<NetworkObject>() == null)
            errore = "Il prefab GiocatoreRete non ha il NetworkObject.";
        else
            rete.AddNetworkPrefab(prefabGiocatore);

        rete.ConnectionApprovalCallback = Approva;
        rete.OnClientConnectedCallback += AlleConnessione;
    }

    void OnDestroy()
    {
        if (Istanza != this) return;
        Istanza = null;
        if (rete != null) rete.OnClientConnectedCallback -= AlleConnessione;
    }

    // Sulla macchina dell'host: accetta fino a 3 giocatori in tutto.
    void Approva(NetworkManager.ConnectionApprovalRequest richiesta, NetworkManager.ConnectionApprovalResponse risposta)
    {
        bool c_e_posto = rete.ConnectedClientsIds.Count < GiocatoriMassimi;
        risposta.Approved = c_e_posto;
        risposta.CreatePlayerObject = false;
        if (!c_e_posto) risposta.Reason = "Partita piena (massimo " + GiocatoriMassimi + " giocatori).";
    }

    // Sulla macchina dell'host: per ogni giocatore che entra (host compreso) crea la sua sagoma in rete.
    void AlleConnessione(ulong idGiocatore)
    {
        if (!rete.IsServer || prefabGiocatore == null) return;
        var copia = Instantiate(prefabGiocatore);
        copia.GetComponent<NetworkObject>().SpawnWithOwnership(idGiocatore);
    }

    public bool Collegato => rete != null && (rete.IsClient || rete.IsServer);

    public void Ospita()
    {
        if (rete == null || Collegato || prefabGiocatore == null) return;
        trasporto.SetConnectionData("127.0.0.1", Porta, "0.0.0.0");
        errore = rete.StartHost() ? null : "Non riesco a ospitare la partita (porta " + Porta + " occupata?).";
    }

    public void Entra()
    {
        if (rete == null || Collegato || prefabGiocatore == null) return;
        trasporto.SetConnectionData(indirizzo.Trim(), Porta);
        errore = rete.StartClient() ? null : "Non riesco a collegarmi a " + indirizzo + ".";
    }

    public void Disconnetti()
    {
        if (rete != null && Collegato) rete.Shutdown();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Menu") return;
        var tastiera = Keyboard.current;
        if (tastiera == null) return;
        if (tastiera.f9Key.wasPressedThisFrame)
        {
            pannelloAperto = !pannelloAperto;
            // Il cursore serve per scrivere l'indirizzo; la camera lo riblocca con un clic nella partita.
            if (pannelloAperto) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }
        if (tastiera.f6Key.wasPressedThisFrame) Ospita();
        if (tastiera.f7Key.wasPressedThisFrame) Entra();
        if (tastiera.f8Key.wasPressedThisFrame) Disconnetti();
    }

    void OnGUI()
    {
        if (SceneManager.GetActiveScene().name == "Menu") return;

        string stato = !Collegato ? Lingua.T("rete.spento")
            : rete.IsHost ? Lingua.T("rete.host") : Lingua.T("rete.client");
        int numero = GiocatoreRete.Presenti;

        if (!pannelloAperto)
        {
            // Una riga piccola in alto a destra, per ricordare il tasto.
            var stile = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight, fontSize = 13 };
            string riga = Lingua.T("rete.suggerimento") + (Collegato ? "  -  " + stato + " (" + numero + "/" + GiocatoriMassimi + ")" : "");
            GUI.Label(new Rect(Screen.width - 420f, 4f, 410f, 22f), riga, stile);
            return;
        }

        var r = new Rect(Screen.width - 340f, 28f, 330f, 190f);
        GUI.Box(r, Lingua.T("rete.titolo"));
        GUI.Label(new Rect(r.x + 10f, r.y + 26f, r.width - 20f, 22f), stato + "   " + Lingua.T("rete.giocatori") + ": " + numero + "/" + GiocatoriMassimi);
        GUI.Label(new Rect(r.x + 10f, r.y + 50f, r.width - 20f, 22f), Lingua.T("rete.indirizzo"));
        GUI.enabled = !Collegato;
        indirizzo = GUI.TextField(new Rect(r.x + 10f, r.y + 72f, r.width - 20f, 24f), indirizzo, 40);
        if (GUI.Button(new Rect(r.x + 10f, r.y + 104f, 150f, 28f), Lingua.T("rete.ospita"))) Ospita();
        if (GUI.Button(new Rect(r.x + 170f, r.y + 104f, 150f, 28f), Lingua.T("rete.entra"))) Entra();
        GUI.enabled = Collegato;
        if (GUI.Button(new Rect(r.x + 10f, r.y + 138f, r.width - 20f, 28f), Lingua.T("rete.esci"))) Disconnetti();
        GUI.enabled = true;
        if (!string.IsNullOrEmpty(errore))
        {
            var rosso = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 12 };
            rosso.normal.textColor = new Color(1f, 0.45f, 0.4f);
            GUI.Label(new Rect(r.x, r.yMax + 2f, r.width, 60f), errore, rosso);
        }
    }
}
