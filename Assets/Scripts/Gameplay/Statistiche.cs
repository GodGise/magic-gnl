using System.Collections.Generic;
using UnityEngine;

// Statistiche di combattimento di un personaggio o di un nemico: armatura, bonus al danno e colpi critici
// (e, per lo Stregone, mana massimo e ricarica del mana; il resto della magia è in ModificatoriMagia).
// A cosa serve: ogni colpo passa da CalcoloDanno, che parte dal danno dell'arma e lo cambia in base
// alle statistiche di chi colpisce (bonus, critico) e di chi viene colpito (armatura).
// I valori scritti qui sono quelli "di base". Armi, amuleti, armature e livelli in futuro
// aggiungeranno dei Modificatori (AggiungiModificatore / TogliModificatore) senza toccare questi numeri:
// così il bilanciamento di oggi resta il punto di partenza e gli oggetti lo spostano.
// Come montarlo: sul Giocatore e su ogni nemico (accanto a GiocatoreControllo o Bersaglio).
// Se manca, GiocatoreControllo e Bersaglio lo aggiungono da soli con i valori qui sotto.
public class Statistiche : MonoBehaviour
{
    [Tooltip("Armatura: riduce il danno ricevuto. 0 = nessuna riduzione, 25 = -20%, 100 = -50% (danno × 100 / (100 + armatura)).")]
    [Min(0f)] [SerializeField] float armatura = 0f;
    [Tooltip("Bonus al danno inflitto, in percentuale (20 = +20%). Più avanti lo daranno livelli e amuleti.")]
    [SerializeField] float bonusDanno = 0f;
    [Tooltip("Probabilità di colpo critico, in percentuale (10 = un colpo su dieci, in media).")]
    [Range(0f, 100f)] [SerializeField] float probabilitaCritico = 10f;
    [Tooltip("Quanto vale un colpo critico rispetto a uno normale (1,75 = +75% di danno).")]
    [Min(1f)] [SerializeField] float moltiplicatoreCritico = 1.75f;
    [Tooltip("Velocità con cui si alza lo scudo, in percentuale (-5 = il 5% più lento, 10 = il 10% più veloce). Di base 0.")]
    [SerializeField] float velocitaParata = 0f;
    [Tooltip("Velocità degli attacchi, in percentuale (-7,5 = il 7,5% più lenti: carica, colpo e recupero durano di più). Di base 0.")]
    [SerializeField] float velocitaAttacco = 0f;
    [Tooltip("Vita massima in più o in meno, in percentuale (-15 = il 15% in meno). Di base 0. Vale per il giocatore.")]
    [SerializeField] float vitaMassimaPercento = 0f;
    [Tooltip("Furtività, in percentuale: i nemici vedono il giocatore da più vicino (30 = vista ridotta del 30%). Di base 0.")]
    [SerializeField] float furtivita = 0f;
    [Tooltip("Resistenza massima in più o in meno, in percentuale (-10 = il 10% in meno). Di base 0. Vale per il giocatore.")]
    [SerializeField] float resistenzaMassimaPercento = 0f;

    [Header("Magia (Stregone): si sommano ai valori di Giocatore Controllo")]
    [Tooltip("Mana massimo in più, in punti (20 = +20). Di base 0: il mana di partenza è in Giocatore Controllo.")]
    [SerializeField] float manaMassimo = 0f;
    [Tooltip("Mana massimo in più o in meno, in percentuale (-10 = il 10% in meno). Di base 0.")]
    [SerializeField] float manaMassimoPercento = 0f;
    [Tooltip("Velocità di ricarica del mana, in percentuale (15 = si ricarica il 15% più in fretta). Di base 0.")]
    [SerializeField] float recuperoMana = 0f;
    [Tooltip("Vita recuperata per ogni nemico ucciso, in punti. Di base 0 (la danno certi amuleti).")]
    [SerializeField] float vitaPerUccisione = 0f;

    readonly List<Modificatore> modificatori = new List<Modificatore>();

    // Valori finali, base più modificatori: sono quelli che usa il calcolo del danno.
    // Armatura finale: base più i punti dei modificatori, poi le percentuali (-2,5 = il 2,5% in meno del totale).
    public float Armatura
    {
        get
        {
            float v = armatura, percento = 0f;
            foreach (var m in modificatori) { v += m.armatura; percento += m.armaturaPercento; }
            return Mathf.Max(0f, v * (1f + percento / 100f));
        }
    }
    public float BonusDanno { get { float v = bonusDanno; foreach (var m in modificatori) v += m.bonusDanno; return v; } }
    public float ProbabilitaCritico { get { float v = probabilitaCritico; foreach (var m in modificatori) v += m.probabilitaCritico; return Mathf.Clamp(v, 0f, 100f); } }
    public float MoltiplicatoreCritico { get { float v = moltiplicatoreCritico; foreach (var m in modificatori) v += m.moltiplicatoreCritico; return Mathf.Max(1f, v); } }
    public float Furtivita { get { float v = furtivita; foreach (var m in modificatori) v += m.furtivita; return Mathf.Clamp(v, -100f, 90f); } }
    public float VitaMassimaPercento { get { float v = vitaMassimaPercento; foreach (var m in modificatori) v += m.vitaMassimaPercento; return Mathf.Max(-90f, v); } }
    public float VelocitaAttacco { get { float v = velocitaAttacco; foreach (var m in modificatori) v += m.velocitaAttacco; return Mathf.Max(-90f, v); } }
    public float VelocitaParata { get { float v = velocitaParata; foreach (var m in modificatori) v += m.velocitaParata; return Mathf.Max(-90f, v); } }
    public float ResistenzaMassimaPercento { get { float v = resistenzaMassimaPercento; foreach (var m in modificatori) v += m.resistenzaMassimaPercento; return Mathf.Max(-90f, v); } }
    public float ManaMassimo { get { float v = manaMassimo; foreach (var m in modificatori) v += m.manaMassimo; return v; } }
    public float ManaMassimoPercento { get { float v = manaMassimoPercento; foreach (var m in modificatori) v += m.manaMassimoPercento; return Mathf.Max(-90f, v); } }
    public float RecuperoMana { get { float v = recuperoMana; foreach (var m in modificatori) v += m.recuperoMana; return Mathf.Max(-90f, v); } }
    public float VitaPerUccisione { get { float v = vitaPerUccisione; foreach (var m in modificatori) v += m.vitaPerUccisione; return Mathf.Max(0f, v); } }

    // Un pezzo di equipaggiamento (o un livello, una pozione...) che cambia le statistiche finché è attivo.
    [System.Serializable]
    public class Modificatore
    {
        public string fonte = "";              // per esempio "Amuleto del lupo", utile per capire da dove arriva
        public float armatura;                 // si somma all'armatura
        public float armaturaPercento;         // percentuale sull'armatura totale (-2,5 = il 2,5% in meno)
        public float bonusDanno;               // punti percentuali in più
        public float probabilitaCritico;       // punti percentuali in più
        public float moltiplicatoreCritico;    // si somma (0,25 = critico da 1,75 a 2)
        public float velocitaParata;           // punti percentuali: -5 = lo scudo si alza il 5% più lento
        public float velocitaAttacco;          // punti percentuali: -7,5 = attacchi il 7,5% più lenti
        public float vitaMassimaPercento;      // punti percentuali: -15 = vita massima il 15% in meno
        public float furtivita;                // punti percentuali: 30 = i nemici ti vedono a 30% di distanza in meno
        public float resistenzaMassimaPercento; // punti percentuali: -10 = resistenza massima il 10% in meno
        public float manaMassimo;              // punti di mana massimo in più (Stregone)
        public float manaMassimoPercento;      // punti percentuali: -10 = mana massimo il 10% in meno
        public float recuperoMana;             // punti percentuali: 15 = il mana si ricarica il 15% più in fretta
        public float vitaPerUccisione;         // punti di vita recuperati per ogni nemico ucciso

        // Somma di due modificatori (per esempio il bonus e il malus di un amuleto).
        public static Modificatore Somma(string fonte, Modificatore a, Modificatore b) => new Modificatore
        {
            fonte = fonte,
            armatura = a.armatura + b.armatura,
            armaturaPercento = a.armaturaPercento + b.armaturaPercento,
            bonusDanno = a.bonusDanno + b.bonusDanno,
            probabilitaCritico = a.probabilitaCritico + b.probabilitaCritico,
            moltiplicatoreCritico = a.moltiplicatoreCritico + b.moltiplicatoreCritico,
            velocitaParata = a.velocitaParata + b.velocitaParata,
            velocitaAttacco = a.velocitaAttacco + b.velocitaAttacco,
            vitaMassimaPercento = a.vitaMassimaPercento + b.vitaMassimaPercento,
            furtivita = a.furtivita + b.furtivita,
            resistenzaMassimaPercento = a.resistenzaMassimaPercento + b.resistenzaMassimaPercento,
            manaMassimo = a.manaMassimo + b.manaMassimo,
            manaMassimoPercento = a.manaMassimoPercento + b.manaMassimoPercento,
            recuperoMana = a.recuperoMana + b.recuperoMana,
            vitaPerUccisione = a.vitaPerUccisione + b.vitaPerUccisione,
        };

        // Le righe "pro e contro" per l'inventario (vedi VoceEffetto): solo i valori diversi da zero.
        public void Descrivi(System.Collections.Generic.List<VoceEffetto> voci)
        {
            Voce(voci, "mod.armatura", armatura, true, "");
            Voce(voci, "mod.armatura_percento", armaturaPercento, true);
            Voce(voci, "mod.danno_tutto", bonusDanno, true);
            Voce(voci, "mod.critico", probabilitaCritico, true);
            Voce(voci, "mod.velocita_parata", velocitaParata, true);
            Voce(voci, "mod.velocita_attacco", velocitaAttacco, true);
            Voce(voci, "mod.vita_massima", vitaMassimaPercento, true);
            Voce(voci, "mod.furtivita", furtivita, true);
            Voce(voci, "mod.resistenza_massima", resistenzaMassimaPercento, true);
            Voce(voci, "mod.mana_massimo", manaMassimo, true, "");
            Voce(voci, "mod.mana_massimo_percento", manaMassimoPercento, true);
            Voce(voci, "mod.recupero_mana", recuperoMana, true);
            Voce(voci, "mod.vita_per_uccisione", vitaPerUccisione, true, "");
        }

        static void Voce(System.Collections.Generic.List<VoceEffetto> voci, string chiave, float valore, bool piuAltoMeglio, string unita = "%")
        {
            if (Mathf.Abs(valore) < 0.001f) return;
            voci.Add(new VoceEffetto
            {
                chiave = chiave,
                testoValore = (valore > 0f ? "+" : "") + ModificatoriMagia.Formatta(valore) + unita,
                positivo = (valore > 0f) == piuAltoMeglio,
            });
        }
    }

    public void AggiungiModificatore(Modificatore m) { if (m != null && !modificatori.Contains(m)) modificatori.Add(m); }
    public void TogliModificatore(Modificatore m) => modificatori.Remove(m);

    // Le statistiche di un oggetto; se non ci sono le aggiunge con i valori di base.
    public static Statistiche Di(Component chi)
    {
        if (chi == null) return null;
        Statistiche s = chi.GetComponentInParent<Statistiche>();
        return s != null ? s : chi.gameObject.AddComponent<Statistiche>();
    }
}

// Il calcolo di ogni colpo, uguale per giocatore e nemici. Si parte dal danno dell'arma e:
// 1. si aggiunge il bonus al danno di chi colpisce;
// 2. si tira il critico (probabilità e moltiplicatore di chi colpisce);
// 3. si toglie la parte fermata dall'armatura di chi è colpito.
// Con questa formula l'armatura non arriva mai a rendere immuni: più ne hai, meno conta ogni punto in più.
public static class CalcoloDanno
{
    // penetrazioneArmatura: quota dell'armatura del difensore che il colpo ignora (0,5 = metà, per le mazze).
    public static float Calcola(float dannoArma, Statistiche attaccante, Statistiche difensore, out bool critico, float penetrazioneArmatura = 0f)
    {
        float danno = dannoArma;
        critico = false;
        if (attaccante != null)
        {
            danno *= 1f + attaccante.BonusDanno / 100f;
            critico = Random.value * 100f < attaccante.ProbabilitaCritico;
            if (critico) danno *= attaccante.MoltiplicatoreCritico;
        }
        if (difensore != null) danno *= 100f / (100f + difensore.Armatura * (1f - Mathf.Clamp01(penetrazioneArmatura)));
        return Mathf.Max(0f, danno);
    }
}
