using UnityEngine;

// Un amuleto, Magico o Arcano. Si equipaggia con Equipaggiamento (uno alla volta).
// - Magico: bonus semplici alle statistiche (danno, critico, armatura), sempre attivi.
// - Arcano: un effetto speciale (per esempio vita a ogni nemico ucciso, resistenza che si ricarica più in
//   fretta, vita rubata a ogni colpo). Può avere anche qualche bonus semplice.
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
    }

    [Header("Tipo")]
    public Tipo tipo = Tipo.Magico;

    [Header("Bonus alle statistiche")]
    public Statistiche.Modificatore bonus = new Statistiche.Modificatore();

    [Header("Effetto arcano")]
    public Effetto effetto = Effetto.Nessuno;
    [Tooltip("Quanto vale l'effetto (vedi l'elenco degli effetti).")]
    public float valore = 0f;

    public Statistiche.Modificatore Modificatore() => new Statistiche.Modificatore
    {
        fonte = Nome,
        armatura = bonus.armatura,
        bonusDanno = bonus.bonusDanno,
        probabilitaCritico = bonus.probabilitaCritico,
        moltiplicatoreCritico = bonus.moltiplicatoreCritico,
    };

    // Valore dell'effetto se è quello richiesto, altrimenti 0.
    public float ValoreEffetto(Effetto quale) => effetto == quale ? valore : 0f;
}
