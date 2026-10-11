using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Orario "a tappe" del prologo a Villaggio Lago Nero (decisione di Lorenzo, 11 ottobre).
// A cosa serve: durante le 6 quest del giorno il tempo NON scorre da solo, così la notte (e la razzia) non può
// arrivare prima che le quest siano finite. Ogni quest finita fa avanzare il sole di qualche ora, piano piano;
// dopo la quinta si arriva al tramonto (la quest 6 è in taverna, al tramonto); quando si finisce la sesta cala la
// notte e parte la razzia. Da lì in poi il tempo torna a scorrere normalmente (CicloGiornoNotte).
// Tutte le quest sono obbligatorie: la razzia parte solo quando sono finite tutte.
// Come montarlo:
//   1. Metti il componente su un oggetto vuoto della scena del villaggio (per esempio "Orario del prologo").
//      Nella scena deve esserci anche il CicloGiornoNotte.
//   2. Alla fine di ogni quest chiama "QuestFinita" con il numero della quest (1-6). Per esempio, sul personaggio
//      che chiude la quest, nell'evento "Al Termine" dell'Interlocutore: trascina questo oggetto, scegli
//      OrarioPrologo > QuestFinita e scrivi il numero. Una quest contata due volte viene ignorata.
//   3. "All'ultima quest" si attiva quando sono finite tutte tranne l'ultima: serve per accendere la quest 6
//      (per esempio attivare Hobb in taverna).
//   4. "Alla razzia" si attiva quando, dopo l'ultima quest, il cielo è diventato notte: qui si fa partire la razzia.
// Le ore e i secondi del passaggio si regolano dall'Inspector. Per provare senza fare le quest: tasto destro sul
// componente > "Prova: finisci la prossima quest".
// Il prologo si gioca da soli (il co-op si apre dalla gattabuia), quindi l'ora la decide solo questo PC.
[DefaultExecutionOrder(-10)]   // prima di CicloGiornoNotte, così l'ora è già quella giusta quando calcola le luci
public class OrarioPrologo : MonoBehaviour
{
    [Tooltip("Ora del gioco all'inizio del prologo (risveglio in casa).")]
    [SerializeField, Range(0f, 24f)] float oraIniziale = 7f;
    [Tooltip("Ora raggiunta dopo la 1ª, 2ª, 3ª... quest finita (conta quante ne hai finite, non quali). L'ultima è il tramonto, per la quest in taverna.")]
    [SerializeField] float[] oreDopoLeQuest = { 9f, 11f, 13f, 15f, 18f };
    [Tooltip("Ora della notte della razzia, raggiunta dopo l'ultima quest.")]
    [SerializeField, Range(0f, 24f)] float oraDellaRazzia = 21f;
    [Tooltip("Quante quest bisogna finire prima della razzia.")]
    [SerializeField, Min(1)] int questDaFinire = 6;
    [Tooltip("Secondi reali per passare da un'ora alla successiva: il cielo cambia piano piano.")]
    [SerializeField, Min(0f)] float secondiPassaggio = 20f;
    [Tooltip("Secondi reali per far calare la notte dopo l'ultima quest (poi parte la razzia).")]
    [SerializeField, Min(0f)] float secondiVersoLaNotte = 12f;
    [Tooltip("Quando sono finite tutte le quest tranne l'ultima (per esempio: attiva Hobb in taverna).")]
    [SerializeField] UnityEvent allUltimaQuest;
    [Tooltip("Quando la notte è calata dopo l'ultima quest: qui parte la razzia.")]
    [SerializeField] UnityEvent allaRazzia;

    public static OrarioPrologo Istanza { get; private set; }
    public int QuestFinite => finite.Count;
    // Vero dopo la razzia: il tempo scorre di nuovo da solo.
    public bool TempoLibero { get; private set; }

    readonly HashSet<int> finite = new HashSet<int>();
    float partenza, arrivo, inizio, durata;
    bool versoLaNotte;

    void Awake()
    {
        Istanza = this;
        partenza = arrivo = oraIniziale;
        inizio = -1000f;
        durata = 0f;
    }

    void OnDestroy()
    {
        if (Istanza == this) Istanza = null;
    }

    void Update()
    {
        if (TempoLibero) return;
        var ciclo = CicloGiornoNotte.Istanza;
        if (ciclo == null) return;
        float t = durata <= 0f ? 1f : Mathf.Clamp01((Time.time - inizio) / durata);
        ciclo.ora = Mathf.Lerp(partenza, arrivo, Mathf.SmoothStep(0f, 1f, t));
        if (versoLaNotte && t >= 1f)
        {
            versoLaNotte = false;
            TempoLibero = true;   // da qui il CicloGiornoNotte riprende a far scorrere il tempo
            allaRazzia.Invoke();
        }
    }

    // Da chiamare alla fine di ogni quest (1-6), per esempio dall'evento "Al Termine" di un Interlocutore.
    public void QuestFinita(int numero)
    {
        if (TempoLibero || versoLaNotte || !finite.Add(numero)) return;
        int n = finite.Count;
        if (n >= questDaFinire)
        {
            VaiA(oraDellaRazzia, secondiVersoLaNotte);
            versoLaNotte = true;
            return;
        }
        if (oreDopoLeQuest != null && oreDopoLeQuest.Length > 0)
            VaiA(oreDopoLeQuest[Mathf.Min(n - 1, oreDopoLeQuest.Length - 1)], secondiPassaggio);
        if (n == questDaFinire - 1) allUltimaQuest.Invoke();
    }

    void VaiA(float ora, float secondi)
    {
        var ciclo = CicloGiornoNotte.Istanza;
        partenza = ciclo != null ? ciclo.ora : arrivo;
        arrivo = ora;
        inizio = Time.time;
        durata = secondi;
    }

    [ContextMenu("Prova: finisci la prossima quest")]
    void ProvaProssima()
    {
        for (int i = 1; i <= questDaFinire; i++)
            if (!finite.Contains(i)) { QuestFinita(i); return; }
    }
}
