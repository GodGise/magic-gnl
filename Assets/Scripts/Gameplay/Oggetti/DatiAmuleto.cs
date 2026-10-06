using UnityEngine;

// Un amuleto: un piccolo oggetto che migliora le statistiche finché lo si porta.
// A cosa serve: per esempio +10% di critico, +15% di danno o +10 di armatura. Si equipaggia con Equipaggiamento
// e i suoi bonus si sommano alle Statistiche del giocatore; togliendolo spariscono.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Amuleto.
[CreateAssetMenu(fileName = "nuovo-amuleto", menuName = "magic-gnl/Oggetti/Amuleto")]
public class DatiAmuleto : DatiOggetto
{
    [Header("Bonus")]
    public Statistiche.Modificatore bonus = new Statistiche.Modificatore();

    public Statistiche.Modificatore Modificatore() => new Statistiche.Modificatore
    {
        fonte = Nome,
        armatura = bonus.armatura,
        bonusDanno = bonus.bonusDanno,
        probabilitaCritico = bonus.probabilitaCritico,
        moltiplicatoreCritico = bonus.moltiplicatoreCritico,
    };
}
