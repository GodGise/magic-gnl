using System;
using UnityEngine;

// La difficoltà che cresce con i giocatori (regola in Docs/rete-coop.md, sezione "Classi e difficoltà in co-op").
// A cosa serve: da soli il gioco è com'è; in 2 i nemici diventano molto più forti, in 3 ancora di più.
// Cosa cambia (tutti i numeri sono qui sotto e si regolano dall'Inspector; i valori di partenza sono provvisori,
// il bilanciamento definitivo lo decide Lorenzo):
//   - vita dei nemici e, a parte, vita dei boss (moltiplicatori della "Vita Massima" scritta sul Bersaglio);
//   - danno dei nemici (moltiplicatore del "Danno Attacco" scritto sul Bersaglio);
//   - frequenza degli attacchi (2 = attaccano il doppio più spesso; il preavviso rosso non cambia).
// Il conto si fa con i giocatori collegati in quel momento: se uno entra o esce, la difficoltà si adatta subito e i
// nemici già feriti tengono la stessa percentuale di vita. Lo calcola l'host (come tutto sui nemici) e lo manda agli
// altri (vedi MondoRete.InviaDifficolta), così la barra della vita di un nemico è uguale su tutti i PC.
// Da soli e nel menu il numero è 1 e tutti i moltiplicatori valgono 1: niente cambia.
// Come montarlo: non serve montarlo, si crea da solo la prima volta che un nemico lo chiede (con i valori di partenza).
// Per cambiare i numeri in modo permanente: metti il componente su un oggetto vuoto della scena (per esempio
// "Difficolta co-op") e modifica le tre tabelle nell'Inspector. Se ci sono due oggetti, vale il primo trovato.
// Regole contro le combinazioni troppo forti in co-op (Lorenzo, 10 ottobre; valgono solo con 2 o più giocatori):
//   - per ogni Stregone nel gruppo, rallentamenti, blocchi e stordimenti sui nemici durano il 30% in meno
//     (si moltiplica: 1 Stregone ×0,7, 2 Stregoni ×0,49, 3 Stregoni ×0,34; vedi EffettiNemico);
//   - ogni Stregone può avere al massimo 2 Fuochi fatui insieme (da soli 3; vedi MagiaStregone).
// Nota: il numero di nemici per gruppo non cambia, perché i gruppi sono messi a mano nelle scene (area di Lorenzo).
[DefaultExecutionOrder(-100)]
public class DifficoltaCoop : MonoBehaviour
{
    [Serializable]
    public struct Livello
    {
        [Tooltip("Moltiplicatore della vita dei nemici normali (2 = il doppio).")]
        public float vitaNemici;
        [Tooltip("Moltiplicatore della vita dei boss e miniboss.")]
        public float vitaBoss;
        [Tooltip("Moltiplicatore del danno dei colpi dei nemici (prima dell'armatura del giocatore).")]
        public float dannoNemici;
        [Tooltip("Quante volte più spesso attaccano (2 = il doppio; il tempo fra un attacco e l'altro si divide per questo numero).")]
        public float frequenzaAttacchi;

        public Livello(float vitaNemici, float vitaBoss, float dannoNemici, float frequenzaAttacchi)
        {
            this.vitaNemici = vitaNemici;
            this.vitaBoss = vitaBoss;
            this.dannoNemici = dannoNemici;
            this.frequenzaAttacchi = frequenzaAttacchi;
        }
    }

    [Header("Moltiplicatori per numero di giocatori (valori provvisori)")]
    [SerializeField] Livello unGiocatore = new Livello(1f, 1f, 1f, 1f);
    [SerializeField] Livello dueGiocatori = new Livello(2.5f, 2.5f, 1.5f, 1.25f);
    [SerializeField] Livello treGiocatori = new Livello(4f, 4f, 2f, 1.5f);

    [Header("Combinazioni troppo forti in co-op (solo con 2 o più giocatori)")]
    [Tooltip("Per ogni Stregone nel gruppo, rallentamenti, blocchi e stordimenti durano questa percentuale in meno (si moltiplica).")]
    [SerializeField, Range(0f, 90f)] float riduzioneControlliPerStregone = 30f;
    [Tooltip("Quanti Fuochi fatui può avere insieme ogni Stregone in co-op (da soli vale il numero dell'incantesimo).")]
    [SerializeField, Min(1)] int fuochiFatuiInCoop = 2;

    // Scatta quando cambia il numero di giocatori (chi ascolta, per esempio Bersaglio, ricalcola i suoi numeri).
    public static event Action Cambiata;

    // Quanti giocatori ci sono adesso (da 1 a ReteCoop.GiocatoriMassimi).
    public static int Giocatori { get; private set; } = 1;

    static DifficoltaCoop istanza;
    float prossimoControllo;

    // Quello della scena, se c'è; altrimenti ne nasce uno con i valori di partenza.
    static DifficoltaCoop Istanza
    {
        get
        {
            if (istanza != null) return istanza;
            istanza = FindFirstObjectByType<DifficoltaCoop>();
            if (istanza != null) return istanza;
            var oggetto = new GameObject("Difficolta co-op");
            DontDestroyOnLoad(oggetto);
            istanza = oggetto.AddComponent<DifficoltaCoop>();
            return istanza;
        }
    }

    static Livello Attuale
    {
        get
        {
            var d = Istanza;
            switch (Giocatori)
            {
                case 1: return d.unGiocatore;
                case 2: return d.dueGiocatori;
                default: return d.treGiocatori;
            }
        }
    }

    // Letti dai nemici (Bersaglio). Un numero a zero o negativo non ha senso e viene ignorato (vale 1).
    public static float VitaNemici(bool boss) => Valido(boss ? Attuale.vitaBoss : Attuale.vitaNemici);
    public static float DannoNemici => Valido(Attuale.dannoNemici);
    public static float FrequenzaAttacchi => Valido(Attuale.frequenzaAttacchi);

    static float Valido(float v) => v > 0.01f ? v : 1f;

    // Quanti Stregoni ci sono nel gruppo: questo PC più le figure degli altri giocatori (la classe arriva da GiocatoreRete).
    public static int Stregoni
    {
        get
        {
            int n = SceltaPartita.Classe == ClasseGiocatore.Stregone ? 1 : 0;
            foreach (var altro in GiocatoreRete.Altri)
                if (altro != null && altro.Classe == ClasseGiocatore.Stregone) n++;
            return n;
        }
    }

    // Durata di rallentamenti, blocchi e stordimenti sui nemici: 1 da soli, ×0,7 per ogni Stregone in co-op.
    public static float MoltiplicatoreControlli
    {
        get
        {
            if (Giocatori <= 1) return 1f;
            float perStregone = 1f - Mathf.Clamp(Istanza.riduzioneControlliPerStregone, 0f, 90f) / 100f;
            return Mathf.Pow(perStregone, Stregoni);
        }
    }

    // Fuochi fatui insieme per ogni Stregone: in co-op al massimo "fuochiFatuiInCoop"; da soli nessun limite in più.
    public static int MassimoFuochiFatui => Giocatori <= 1 ? int.MaxValue : Mathf.Max(1, Istanza.fuochiFatuiInCoop);

    void Awake()
    {
        if (istanza == null) istanza = this;
    }

    void OnDestroy()
    {
        if (istanza == this) istanza = null;
    }

    void Update()
    {
        if (Time.unscaledTime < prossimoControllo) return;
        prossimoControllo = Time.unscaledTime + 0.5f;

        // Fuori dalla rete si è soli. Chi non ospita aspetta il numero dall'host (vedi ImpostaDaRete).
        if (!Rete.Attiva) Imposta(1);
        else if (Rete.ComandaIlMondo) Imposta(1 + GiocatoreRete.Altri.Count);
    }

    // Su chi non ospita: l'host ha mandato quanti giocatori ci sono (vedi MondoRete).
    public static void ImpostaDaRete(int giocatori) => Imposta(giocatori);

    static void Imposta(int giocatori)
    {
        giocatori = Mathf.Clamp(giocatori, 1, ReteCoop.GiocatoriMassimi);
        if (giocatori == Giocatori) return;
        Giocatori = giocatori;
        Debug.Log("[Difficoltà] Giocatori: " + giocatori);
        Cambiata?.Invoke();
        // L'host dice a tutti le nuove vite dei nemici (già ricalcolate da Cambiata).
        if (Rete.Attiva && Rete.ComandaIlMondo) MondoRete.InviaDifficolta(giocatori);
    }
}
