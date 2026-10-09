using UnityEngine;

// Un amuleto, Magico o Arcano. Si equipaggia con Equipaggiamento (uno alla volta).
// Ogni amuleto dà un vantaggio ma ha anche un MALUS: per esempio +10% di danno ma lo scudo si alza più lento.
// - Magico: bonus semplici alle statistiche (danno, critico, armatura), sempre attivi.
// - Arcano: un effetto speciale (per esempio vita a ogni nemico ucciso, resistenza che si ricarica più in
//   fretta, vita rubata a ogni colpo, mana per ogni nemico ucciso). Può avere anche qualche bonus semplice.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Amuleto.
[CreateAssetMenu(fileName = "nuovo-amuleto", menuName = "magic-gnl/Oggetti/Amuleto")]
public class DatiAmuleto : DatiOggetto
{
    public enum Tipo { Magico, Arcano }
    public enum Effetto
    {
        Nessuno,
        VitaPerUccisione,     // "valore" = vita recuperata per ogni nemico sconfitto
        RecuperoResistenza,   // "valore" = percentuale in più di ricarica della resistenza (30 = +30%)
        RubaVita,             // "valore" = percentuale del danno inflitto che torna come vita (8 = 8%)
        SvanireNellOmbra,     // si attiva con Q: invisibile ai nemici per "valore" secondi; poi "ricarica" secondi di attesa
        ManaPerUccisione,     // "valore" = mana recuperato per ogni nemico sconfitto (Stregone)
        RubaMana,             // "valore" = percentuale del danno degli incantesimi che torna come mana (5 = 5%)
    }

    [Header("Tipo")]
    public Tipo tipo = Tipo.Magico;

    [Header("Bonus alle statistiche")]
    public Statistiche.Modificatore bonus = new Statistiche.Modificatore();
    [Header("Malus (il prezzo dell'amuleto): valori negativi")]
    public Statistiche.Modificatore malus = new Statistiche.Modificatore();

    [Header("Effetto arcano")]
    public Effetto effetto = Effetto.Nessuno;
    [Tooltip("Quanto vale l'effetto (vedi l'elenco degli effetti).")]
    public float valore = 0f;
    [Tooltip("Solo per gli effetti da attivare con Q: secondi di attesa prima di poterlo riusare (360 = 6 minuti).")]
    public float ricarica = 0f;

    public Statistiche.Modificatore Modificatore() => Statistiche.Modificatore.Somma(Nome, bonus, malus);

    // Valore dell'effetto se è quello richiesto, altrimenti 0.
    public float ValoreEffetto(Effetto quale) => effetto == quale ? valore : 0f;
}
