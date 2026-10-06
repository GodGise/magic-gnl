using System.Collections.Generic;
using UnityEngine;

// Statistiche di combattimento di un personaggio o di un nemico: armatura, bonus al danno e colpi critici.
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

    readonly List<Modificatore> modificatori = new List<Modificatore>();

    // Valori finali, base più modificatori: sono quelli che usa il calcolo del danno.
    public float Armatura { get { float v = armatura; foreach (var m in modificatori) v += m.armatura; return Mathf.Max(0f, v); } }
    public float BonusDanno { get { float v = bonusDanno; foreach (var m in modificatori) v += m.bonusDanno; return v; } }
    public float ProbabilitaCritico { get { float v = probabilitaCritico; foreach (var m in modificatori) v += m.probabilitaCritico; return Mathf.Clamp(v, 0f, 100f); } }
    public float MoltiplicatoreCritico { get { float v = moltiplicatoreCritico; foreach (var m in modificatori) v += m.moltiplicatoreCritico; return Mathf.Max(1f, v); } }

    // Un pezzo di equipaggiamento (o un livello, una pozione...) che cambia le statistiche finché è attivo.
    [System.Serializable]
    public class Modificatore
    {
        public string fonte = "";              // per esempio "Amuleto del lupo", utile per capire da dove arriva
        public float armatura;                 // si somma all'armatura
        public float bonusDanno;               // punti percentuali in più
        public float probabilitaCritico;       // punti percentuali in più
        public float moltiplicatoreCritico;    // si somma (0,25 = critico da 1,75 a 2)
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
