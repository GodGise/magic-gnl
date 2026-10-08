using Unity.Netcode;
using UnityEngine;

// Il "mondo condiviso" del co-op: nemici, ora del giorno e sfere magiche.
// A cosa serve: chi ospita (host) è l'unico su cui i nemici pensano. Questo oggetto di rete:
//   - dall'host manda a tutti, 12 volte al secondo, posizione e stato di ogni nemico e l'ora del giorno;
//   - dall'host manda gli eventi dei nemici: attacco (preavviso rosso), colpito, sbilanciato, morto, rinato, "!";
//   - da chi non ospita porta all'host i colpi ai nemici, le parate perfette e le esecuzioni furtive;
//   - fa vedere a tutti le sfere magiche lanciate dagli altri (solo aspetto, il danno lo manda chi lancia).
// I colpi dei nemici ai giocatori non passano di qui: li porta GiocatoreRete al giocatore colpito.
// Da soli questo oggetto non esiste e tutte le funzioni "Invia..." e "Chiedi..." non fanno niente.
// Come montarlo: non si monta a mano. Sta nel prefab Resources/Rete/MondoRete (creato da Assets/Editor/CreaPrefabRete.cs)
// e lo crea ReteCoop sul PC dell'host quando la scena di gioco è pronta.
public class MondoRete : NetworkBehaviour
{
    public static MondoRete Istanza { get; private set; }

    const float IntervalloFoto = 1f / 12f;   // ogni quanto l'host manda le posizioni dei nemici
    const int NemiciPerPacchetto = 40;       // per non superare la dimensione di un pacchetto di rete
    float prossimaFoto;

    static bool SonoHost => Istanza != null && Istanza.IsSpawned && Istanza.IsServer;
    static bool SonoOspite => Istanza != null && Istanza.IsSpawned && !Istanza.IsServer;

    public override void OnNetworkSpawn()
    {
        Istanza = this;
        if (!IsServer) ChiediStatoRpc();
    }

    public override void OnNetworkDespawn()
    {
        if (Istanza == this) Istanza = null;
    }

    void Update()
    {
        if (!IsSpawned || !IsServer || Time.unscaledTime < prossimaFoto) return;
        prossimaFoto = Time.unscaledTime + IntervalloFoto;
        InviaFoto();
    }

    // ---------- posizioni dei nemici e ora (host -> tutti) ----------

    void InviaFoto()
    {
        var nemici = RegistroNemici.Tutti;
        float ora = CicloGiornoNotte.Istanza != null ? CicloGiornoNotte.Istanza.ora : -1f;
        int totale = nemici.Count;
        int inizio = 0;
        do
        {
            int n = Mathf.Clamp(totale - inizio, 0, NemiciPerPacchetto);
            var numeri = new int[n];
            var posizioni = new Vector3[n];
            var direzioni = new float[n];
            var stati = new byte[n];
            for (int i = 0; i < n; i++)
            {
                Bersaglio b = nemici[inizio + i];
                if (b == null) { numeri[i] = -1; continue; }
                numeri[i] = b.NumeroRete;
                posizioni[i] = b.transform.position;
                direzioni[i] = b.DirezioneAttuale;
                var vista = b.GetComponent<InseguimentoNemico>();
                if (vista != null) stati[i] = (byte)((vista.StaInseguendo ? 1 : 0) | (vista.InEsecuzione ? 2 : 0));
            }
            FotoRpc(numeri, posizioni, direzioni, stati, ora);
            inizio += NemiciPerPacchetto;
        } while (inizio < totale);
    }

    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    void FotoRpc(int[] numeri, Vector3[] posizioni, float[] direzioni, byte[] stati, float ora)
    {
        for (int i = 0; i < numeri.Length; i++)
        {
            Bersaglio b = RegistroNemici.Trova(numeri[i]);
            if (b == null) continue;
            b.ImpostaPosizioneRete(posizioni[i], direzioni[i]);
            var vista = b.GetComponent<InseguimentoNemico>();
            if (vista != null) vista.StatoDaRete((stati[i] & 1) != 0, (stati[i] & 2) != 0);
        }
        if (ora >= 0f && CicloGiornoNotte.Istanza != null) CicloGiornoNotte.Istanza.ora = ora;
    }

    // Chi entra a partita iniziata chiede vita e morte di tutti i nemici.
    [Rpc(SendTo.Server)]
    void ChiediStatoRpc(RpcParams parametri = default)
    {
        var nemici = RegistroNemici.Tutti;
        var numeri = new int[nemici.Count];
        var vite = new float[nemici.Count];
        var morti = new byte[nemici.Count];
        for (int i = 0; i < nemici.Count; i++)
        {
            Bersaglio b = nemici[i];
            numeri[i] = b != null ? b.NumeroRete : -1;
            vite[i] = b != null ? b.Vita : 0f;
            morti[i] = (byte)(b != null && b.Morto ? 1 : 0);
        }
        StatoRpc(numeri, vite, morti, RpcTarget.Single(parametri.Receive.SenderClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void StatoRpc(int[] numeri, float[] vite, byte[] morti, RpcParams parametri)
    {
        for (int i = 0; i < numeri.Length; i++)
        {
            Bersaglio b = RegistroNemici.Trova(numeri[i]);
            if (b != null) b.StatoDaRete(vite[i], morti[i] != 0);
        }
    }

    // ---------- eventi dei nemici (host -> tutti) ----------

    public static void InviaAttacco(Bersaglio b) { if (SonoHost) Istanza.AttaccoRpc(b.NumeroRete, b.NumeroAttacco); }
    public static void InviaColpito(Bersaglio b, bool critico) { if (SonoHost) Istanza.ColpitoRpc(b.NumeroRete, b.Vita, critico); }
    public static void InviaSbilanciato(Bersaglio b, float durata, float moltiplicatore) { if (SonoHost) Istanza.SbilanciatoRpc(b.NumeroRete, durata, moltiplicatore); }
    public static void InviaMorto(Bersaglio b) { if (SonoHost) Istanza.MortoRpc(b.NumeroRete); }
    public static void InviaRinato(Bersaglio b) { if (SonoHost) Istanza.RinatoRpc(b.NumeroRete); }
    public static void InviaAllarme(Bersaglio b) { if (SonoHost) Istanza.AllarmeRpc(b.NumeroRete); }

    // Il premio dell'uccisione va al giocatore che ha dato l'ultimo colpo, sul suo PC.
    public static void InviaSconfitto(ulong giocatore)
    {
        if (SonoHost) Istanza.SconfittoRpc(Istanza.RpcTarget.Single(giocatore, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.NotServer)]
    void AttaccoRpc(int numero, int numeroAttacco) => RegistroNemici.Trova(numero)?.AttaccoDaRete(numeroAttacco);

    [Rpc(SendTo.NotServer)]
    void ColpitoRpc(int numero, float vita, bool critico) => RegistroNemici.Trova(numero)?.ColpitoDaRete(vita, critico);

    [Rpc(SendTo.NotServer)]
    void SbilanciatoRpc(int numero, float durata, float moltiplicatore) => RegistroNemici.Trova(numero)?.SbilanciatoDaRete(durata, moltiplicatore);

    [Rpc(SendTo.NotServer)]
    void MortoRpc(int numero) => RegistroNemici.Trova(numero)?.MortoDaRete();

    [Rpc(SendTo.NotServer)]
    void RinatoRpc(int numero) => RegistroNemici.Trova(numero)?.RinatoDaRete();

    [Rpc(SendTo.NotServer)]
    void AllarmeRpc(int numero)
    {
        Bersaglio b = RegistroNemici.Trova(numero);
        if (b != null && b.TryGetComponent(out InseguimentoNemico vista)) vista.AllarmeDaRete();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void SconfittoRpc(RpcParams parametri)
    {
        if (ObiettiviNemici.Locale != null) ObiettiviNemici.Locale.NemicoSconfitto();
    }

    // ---------- azioni dei giocatori sui nemici (chi non ospita -> host) ----------

    public static void ChiediColpo(Bersaglio b, float danno, Vector3 origine, bool critico)
    {
        if (SonoOspite) Istanza.ColpoRpc(b.NumeroRete, danno, origine, critico);
    }

    public static void ChiediSbilancia(Bersaglio b, float durata, Vector3 daDove, float moltiplicatore)
    {
        if (SonoOspite) Istanza.SbilanciaRpc(b.NumeroRete, durata, daDove, moltiplicatore);
    }

    public static void ChiediEsecuzione(Bersaglio b) { if (SonoOspite) Istanza.EsecuzioneRpc(b.NumeroRete); }
    public static void ChiediGiustizia(Bersaglio b, Vector3 daDove) { if (SonoOspite) Istanza.GiustiziaRpc(b.NumeroRete, daDove); }

    [Rpc(SendTo.Server)]
    void ColpoRpc(int numero, float danno, Vector3 origine, bool critico, RpcParams parametri = default)
    {
        RegistroNemici.Trova(numero)?.RiceviColpoDa(parametri.Receive.SenderClientId, danno, origine, critico);
    }

    [Rpc(SendTo.Server)]
    void SbilanciaRpc(int numero, float durata, Vector3 daDove, float moltiplicatore)
    {
        RegistroNemici.Trova(numero)?.Sbilancia(durata, daDove, moltiplicatore);
    }

    [Rpc(SendTo.Server)]
    void EsecuzioneRpc(int numero)
    {
        Bersaglio b = RegistroNemici.Trova(numero);
        if (b != null && b.TryGetComponent(out InseguimentoNemico vista)) vista.IniziaEsecuzione();
    }

    [Rpc(SendTo.Server)]
    void GiustiziaRpc(int numero, Vector3 daDove, RpcParams parametri = default)
    {
        Bersaglio b = RegistroNemici.Trova(numero);
        if (b != null && b.TryGetComponent(out InseguimentoNemico vista)) vista.GiustiziaDa(parametri.Receive.SenderClientId, daDove);
    }

    // ---------- sfere magiche (chi lancia -> tutti gli altri) ----------

    public static void InviaSfera(Vector3 partenza, Vector3 direzione, Bersaglio obiettivo, float velocita)
    {
        if (Istanza != null && Istanza.IsSpawned) Istanza.SferaRpc(partenza, direzione, obiettivo != null ? obiettivo.NumeroRete : -1, velocita);
    }

    [Rpc(SendTo.NotMe)]
    void SferaRpc(Vector3 partenza, Vector3 direzione, int numeroObiettivo, float velocita)
    {
        // Danno 0: è solo da vedere. Il danno vero lo manda il PC di chi l'ha lanciata.
        Transform io = ObiettiviNemici.Locale != null ? ObiettiviNemici.Locale.transform : null;
        SferaMagica.Lancia(partenza, direzione, RegistroNemici.Trova(numeroObiettivo), velocita, 0f, io);
        Suoni.Suona(Suono.SferaLancio, partenza, 0.9f);
    }
}
