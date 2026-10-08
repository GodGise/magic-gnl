using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Domande veloci sulla rete, usate dagli script di gioco (giocatore, nemici, menu).
// A cosa serve: lo stesso codice funziona da soli e in co-op. Da soli la rete è spenta e "ComandaIlMondo" è vero:
// i nemici pensano e si muovono qui. In co-op il mondo lo comanda chi ospita (host): i nemici pensano solo da lui,
// gli altri ricevono posizioni ed eventi (vedi MondoRete) e gli mandano i loro colpi.
// Come montarlo: non si monta, si usa dal codice (per esempio: if (Rete.SoloOspite) ...).
public static class Rete
{
    static NetworkManager Gestore => NetworkManager.Singleton;

    // Vero quando si è in una partita in rete (come host o come ospite).
    public static bool Attiva => Gestore != null && Gestore.IsListening;

    // Vero da soli e per chi ospita: qui si decidono nemici, danni ai nemici e morti.
    public static bool ComandaIlMondo => !Attiva || Gestore.IsServer;

    // Vero per chi è entrato nella partita di un altro: i nemici qui sono solo "figure" mosse dall'host.
    public static bool Ospite => Attiva && !Gestore.IsServer;

    public static ulong MioId => Attiva ? Gestore.LocalClientId : 0UL;
}

// Elenco dei nemici della scena con un numero uguale su tutti i PC, per dire "il nemico 12345 è stato colpito".
// Il numero viene dal nome del nemico, dalla sua posizione nella gerarchia e dalla posizione di partenza nella scena:
// tutti i giocatori caricano la stessa scena, quindi ottengono gli stessi numeri.
// Come montarlo: non si monta. Bersaglio si iscrive da solo.
public static class RegistroNemici
{
    static readonly Dictionary<int, Bersaglio> perNumero = new Dictionary<int, Bersaglio>();
    static readonly List<Bersaglio> elenco = new List<Bersaglio>();

    public static IReadOnlyList<Bersaglio> Tutti => elenco;

    public static int Iscrivi(Bersaglio b)
    {
        int numero = CalcolaNumero(b.transform);
        while (perNumero.ContainsKey(numero) && perNumero[numero] != b && perNumero[numero] != null) numero++;
        perNumero[numero] = b;
        if (!elenco.Contains(b)) elenco.Add(b);
        return numero;
    }

    public static void Togli(Bersaglio b, int numero)
    {
        if (perNumero.TryGetValue(numero, out var trovato) && trovato == b) perNumero.Remove(numero);
        elenco.Remove(b);
    }

    public static Bersaglio Trova(int numero) => perNumero.TryGetValue(numero, out var b) && b != null ? b : null;

    // Il numero uguale su tutti i PC per un oggetto della scena (lo usa anche RegistroCondivisi).
    public static int NumeroDi(Transform t) => CalcolaNumero(t);

    static int CalcolaNumero(Transform t)
    {
        var testo = new System.Text.StringBuilder(t.gameObject.scene.name);
        for (Transform p = t; p != null; p = p.parent) testo.Append('/').Append(p.name);
        Vector3 pos = t.position;
        testo.Append('@').Append(Mathf.RoundToInt(pos.x * 10f)).Append(',').Append(Mathf.RoundToInt(pos.y * 10f)).Append(',').Append(Mathf.RoundToInt(pos.z * 10f));
        // FNV-1a: un "riassunto" del testo che è uguale su tutti i PC (string.GetHashCode non lo è).
        unchecked
        {
            uint h = 2166136261;
            string s = testo.ToString();
            for (int i = 0; i < s.Length; i++) { h ^= s[i]; h *= 16777619; }
            return (int)(h & 0x7FFFFFFF);
        }
    }
}

// Un oggetto della scena che in co-op è uguale per tutti: porte, leve, bauli, chiavi, muri crepati, oggetti da raccogliere.
// Come funziona: chi lo usa, se non ospita, lo chiede all'host (MondoRete.ChiediUso); l'host lo usa davvero e manda
// l'evento a tutti (MondoRete.InviaEvento). Chi entra a partita iniziata riceve lo stato di ognuno (StatoRete).
// Da soli non cambia niente: l'oggetto si usa subito come prima.
public interface IOggettoCondiviso
{
    int NumeroRete { get; }
    // Sull'host: il giocatore "chi" chiede di usarlo (valore e punto dipendono dall'oggetto).
    void UsaDaRete(ulong chi, int valore, Vector3 punto);
    // Su chi non ospita: l'host ha usato l'oggetto, si mostra quello che è successo.
    void EventoDaRete(ulong chi, int valore, Vector3 punto);
    // Lo stato in un numero, per chi entra dopo (per esempio porta: 0 chiusa, 1 aperta).
    int StatoRete { get; }
    void StatoDaRete(int stato);
}

// Elenco degli oggetti condivisi della scena, con lo stesso numero su tutti i PC (come RegistroNemici).
// Come montarlo: non si monta. Gli oggetti condivisi si iscrivono da soli in Awake.
public static class RegistroCondivisi
{
    static readonly Dictionary<int, IOggettoCondiviso> perNumero = new Dictionary<int, IOggettoCondiviso>();
    static readonly List<IOggettoCondiviso> elenco = new List<IOggettoCondiviso>();

    public static IReadOnlyList<IOggettoCondiviso> Tutti => elenco;

    public static int Iscrivi(MonoBehaviour oggetto)
    {
        var condiviso = (IOggettoCondiviso)oggetto;
        int numero = RegistroNemici.NumeroDi(oggetto.transform) ^ oggetto.GetType().Name.Length * 7919;
        while (perNumero.TryGetValue(numero, out var altro) && Esiste(altro) && !ReferenceEquals(altro, condiviso)) numero++;
        perNumero[numero] = condiviso;
        if (!elenco.Contains(condiviso)) elenco.Add(condiviso);
        return numero;
    }

    public static void Togli(IOggettoCondiviso oggetto, int numero)
    {
        if (perNumero.TryGetValue(numero, out var trovato) && ReferenceEquals(trovato, oggetto)) perNumero.Remove(numero);
        elenco.Remove(oggetto);
    }

    public static IOggettoCondiviso Trova(int numero) => perNumero.TryGetValue(numero, out var o) && Esiste(o) ? o : null;

    static bool Esiste(IOggettoCondiviso o) => o != null && !(o is Object u && u == null);
}
