using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

// Co-op in rete (da 1 a 3 giocatori), per ora senza Steam: ci si collega con l'indirizzo IP di chi ospita.
// A cosa serve: avvia Netcode for GameObjects con il trasporto di base di Unity. Lo usa il menu iniziale
// (voce "Multigiocatore"): "Ospita una partita" fa partire la partita come host, "Entra in una partita" si collega
// all'indirizzo scritto. La scena la sceglie l'host: chi entra la carica da solo (gestione scene di Netcode).
// Quando la scena di gioco è pronta, l'host crea il MondoRete (nemici e ora condivisi) e una figura di rete
// (GiocatoreRete) per ogni giocatore. Chi entra dopo riceve tutto appena collegato.
// Se l'host chiude la partita, o la connessione cade, si torna al menu iniziale con un avviso.
// Dove funziona: sullo stesso PC (due finestre del gioco, indirizzo 127.0.0.1), fra PC della stessa casa
// (indirizzo locale, per esempio 192.168.1.20), oppure su internet con una rete privata gratuita come
// ZeroTier o Radmin VPN. Con Steam (più avanti) basterà un invito fra amici.
// Come montarlo: non va montato su niente, si crea da solo all'avvio del gioco.
// I prefab Resources/Rete/GiocatoreRete e Resources/Rete/MondoRete li crea l'editor (Assets/Editor/CreaPrefabRete.cs).
public class ReteCoop : MonoBehaviour
{
    public const int GiocatoriMassimi = 3;
    public const ushort Porta = 7777;
    const string ScenaMenu = "Menu";

    public static ReteCoop Istanza { get; private set; }

    // Avviso da mostrare nel menu iniziale dopo essere tornati (per esempio "L'host ha chiuso la partita").
    public static string AvvisoPerMenu;

    // Esito del tentativo di entrare: true = collegati, false = non riuscito (con il motivo, già tradotto).
    public event Action<bool, string> EsitoCollegamento;

    NetworkManager rete;
    UnityTransport trasporto;
    GameObject prefabGiocatore, prefabMondo;
    bool scenaPronta;
    bool staEntrando;
    bool chiusuraVoluta;

    public bool Collegato => rete != null && rete.IsListening;
    public bool SonoHost => Collegato && rete.IsHost;
    public int NumeroGiocatori => FindObjectsByType<GiocatoreRete>(FindObjectsSortMode.None).Length;

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
            EnableSceneManagement = true,   // la scena la sceglie l'host, chi entra la carica da solo
            ConnectionApproval = true,      // serve a rifiutare il quarto giocatore
        };
        gameObject.SetActive(true);

        prefabGiocatore = CaricaPrefab("Rete/GiocatoreRete");
        prefabMondo = CaricaPrefab("Rete/MondoRete");

        rete.ConnectionApprovalCallback = Approva;
        rete.OnClientConnectedCallback += AllaConnessione;
        rete.OnClientDisconnectCallback += AllaDisconnessione;
    }

    GameObject CaricaPrefab(string percorso)
    {
        var prefab = Resources.Load<GameObject>(percorso);
        if (prefab == null || prefab.GetComponent<NetworkObject>() == null)
        {
            Debug.LogError("[Rete] Manca il prefab Resources/" + percorso + ": menu \"magic-gnl > Rete: ricrea i prefab di rete\".");
            return null;
        }
        rete.AddNetworkPrefab(prefab);
        return prefab;
    }

    void OnDestroy()
    {
        if (Istanza != this) return;
        Istanza = null;
        if (rete == null) return;
        rete.OnClientConnectedCallback -= AllaConnessione;
        rete.OnClientDisconnectCallback -= AllaDisconnessione;
    }

    bool PrefabPronti => prefabGiocatore != null && prefabMondo != null;

    // ---------- ospitare ----------

    // Fa partire la partita come host e carica la scena per tutti. Restituisce false se non è possibile.
    public bool Ospita(string scena, out string errore)
    {
        errore = null;
        if (!PrefabPronti) { errore = Lingua.T("rete.errore_prefab"); return false; }
        if (Collegato || rete.ShutdownInProgress) { errore = Lingua.T("rete.errore_occupato"); return false; }

        trasporto.SetConnectionData("127.0.0.1", Porta, "0.0.0.0");
        if (!rete.StartHost()) { errore = Lingua.T("rete.errore_ospita"); return false; }

        chiusuraVoluta = false;
        scenaPronta = false;
        rete.SceneManager.OnLoadEventCompleted += ScenaCaricata;
        if (rete.SceneManager.LoadScene(scena, LoadSceneMode.Single) != SceneEventProgressStatus.Started)
        {
            rete.Shutdown();
            errore = Lingua.T("rete.errore_ospita");
            return false;
        }
        return true;
    }

    // Sull'host: la scena è pronta per tutti, si creano il mondo condiviso e le figure dei giocatori.
    void ScenaCaricata(string scena, LoadSceneMode modo, List<ulong> completati, List<ulong> inRitardo)
    {
        if (!rete.IsServer || scena == ScenaMenu) return;
        scenaPronta = true;
        if (MondoRete.Istanza == null) Instantiate(prefabMondo).GetComponent<NetworkObject>().Spawn();
        foreach (ulong id in rete.ConnectedClientsIds) CreaFigura(id);
    }

    void CreaFigura(ulong id)
    {
        var esistenti = FindObjectsByType<GiocatoreRete>(FindObjectsSortMode.None);
        var occupati = new HashSet<int>();
        foreach (var g in esistenti)
        {
            if (g.IsSpawned && g.OwnerClientId == id) return;   // c'è già
            occupati.Add(g.Numero);
        }
        int numero = 1;
        while (occupati.Contains(numero)) numero++;

        var copia = Instantiate(prefabGiocatore);
        copia.GetComponent<GiocatoreRete>().ImpostaNumero(numero);
        copia.GetComponent<NetworkObject>().SpawnWithOwnership(id);
    }

    // Sull'host: accetta fino a 3 giocatori in tutto (host compreso).
    void Approva(NetworkManager.ConnectionApprovalRequest richiesta, NetworkManager.ConnectionApprovalResponse risposta)
    {
        bool posto = rete.ConnectedClientsIds.Count < GiocatoriMassimi;
        risposta.Approved = posto;
        risposta.CreatePlayerObject = false;
        if (!posto) risposta.Reason = "piena";
    }

    // ---------- entrare ----------

    public bool Entra(string indirizzo, out string errore)
    {
        errore = null;
        if (!PrefabPronti) { errore = Lingua.T("rete.errore_prefab"); return false; }
        if (Collegato || rete.ShutdownInProgress) { errore = Lingua.T("rete.errore_occupato"); return false; }

        trasporto.SetConnectionData(string.IsNullOrWhiteSpace(indirizzo) ? "127.0.0.1" : indirizzo.Trim(), Porta);
        chiusuraVoluta = false;
        staEntrando = true;
        if (!rete.StartClient())
        {
            staEntrando = false;
            errore = Lingua.T("rete.errore_collega");
            return false;
        }
        return true;
    }

    // ---------- collegamenti e uscite ----------

    void AllaConnessione(ulong id)
    {
        if (rete.IsServer)
        {
            if (scenaPronta) CreaFigura(id);
            return;
        }
        if (id == rete.LocalClientId && staEntrando)
        {
            staEntrando = false;
            EsitoCollegamento?.Invoke(true, null);
        }
    }

    void AllaDisconnessione(ulong id)
    {
        // Se ne va un altro giocatore: Netcode toglie da solo la sua figura. Qui conta solo la nostra connessione.
        if (id != rete.LocalClientId && (rete.IsServer || rete.IsConnectedClient)) return;

        string motivo = rete.DisconnectReason;
        string avviso = motivo == "piena" ? Lingua.T("rete.piena")
            : staEntrando ? Lingua.T("rete.errore_collega")
            : Lingua.T("rete.host_uscito");

        if (staEntrando)
        {
            staEntrando = false;
            if (rete.IsListening) rete.Shutdown();
            EsitoCollegamento?.Invoke(false, avviso);
            return;
        }
        if (chiusuraVoluta) return;
        if (rete.IsListening) rete.Shutdown();
        TornaAlMenu(avviso);
    }

    // Lascia la partita (dal menu di pausa) e torna al menu iniziale. Se esce l'host, la partita finisce per tutti.
    public void Esci()
    {
        chiusuraVoluta = true;
        staEntrando = false;
        scenaPronta = false;
        if (rete != null && rete.IsListening) rete.Shutdown();
        TornaAlMenu(null);
    }

    // Spegne la rete senza cambiare scena (per esempio chiudendo il gioco).
    public void Spegni()
    {
        chiusuraVoluta = true;
        staEntrando = false;
        if (rete != null && rete.IsListening) rete.Shutdown();
    }

    // Annulla un tentativo di entrare ancora in corso.
    public void AnnullaEntrata()
    {
        if (!staEntrando) return;
        staEntrando = false;
        chiusuraVoluta = true;
        if (rete.IsListening) rete.Shutdown();
    }

    void TornaAlMenu(string avviso)
    {
        AvvisoPerMenu = avviso;
        scenaPronta = false;
        if (rete != null && rete.SceneManager != null) rete.SceneManager.OnLoadEventCompleted -= ScenaCaricata;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (SceneManager.GetActiveScene().name == ScenaMenu) return;
        if (Application.CanStreamedLevelBeLoaded(ScenaMenu)) SceneManager.LoadScene(ScenaMenu);
        else SceneManager.LoadScene(0);
    }

    // Gli indirizzi di questo PC da dare agli amici (rete di casa, ZeroTier, Radmin...), senza 127.0.0.1.
    public static List<string> IndirizziLocali()
    {
        var elenco = new List<string>();
        try
        {
            foreach (var scheda in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (scheda.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                foreach (var indirizzo in scheda.GetIPProperties().UnicastAddresses)
                {
                    var ip = indirizzo.Address;
                    if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork || System.Net.IPAddress.IsLoopback(ip)) continue;
                    string testo = ip.ToString();
                    if (testo.StartsWith("169.254.")) continue;   // indirizzo automatico senza rete vera
                    if (!elenco.Contains(testo)) elenco.Add(testo);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Rete] Non riesco a leggere gli indirizzi di questo PC: " + e.Message);
        }
        return elenco;
    }
}
