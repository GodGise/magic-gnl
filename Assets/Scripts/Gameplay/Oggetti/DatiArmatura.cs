using UnityEngine;

// Un'armatura: per il Guerriero pesante, media o leggera; per il Ladro leggera, di cuoio o d'ombra.
// Le armature del Ladro proteggono poco ma danno FURTIVITÀ: i nemici le notano da più vicino.
// Le vesti dello Stregone (leggera, media, pesante) proteggono pochissimo ma danno MANA massimo.
// A cosa serve: dà armatura (meno danno dai colpi, vedi Statistiche) ma pesa: più è pesante, più la schivata
// costa resistenza e più la corsa è lenta. Si equipaggia con Equipaggiamento.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Armatura.
[CreateAssetMenu(fileName = "nuova-armatura", menuName = "magic-gnl/Oggetti/Armatura")]
public class DatiArmatura : DatiOggetto
{
    public enum Peso { Pesante, Media, Leggera, Cuoio, Ombra }

    [Header("Tipo")]
    public Peso peso = Peso.Media;

    [Header("Protezione")]
    [Tooltip("Armatura: 25 = -20% di danno, 50 = -33%, 100 = -50%.")]
    public float armatura = 10f;

    [Header("Furtività (armature del Ladro)")]
    [Tooltip("Percentuale di vista tolta ai nemici: 20 = ti vedono da 14 m invece che da 18.")]
    public float furtivita = 0f;

    [Header("Mana (vesti dello Stregone)")]
    [Tooltip("Mana massimo in più, in punti (15 = +15).")]
    public float manaMassimo = 0f;

    [Header("Peso")]
    [Tooltip("Resistenza in più spesa per ogni schivata (0 = armatura leggera).")]
    public float costoSchivataExtra = 0f;
    [Tooltip("Velocità di corsa rispetto a quella normale (1 = nessun rallentamento, 0,9 = -10%).")]
    [Range(0.5f, 1f)] public float moltiplicatoreVelocita = 1f;

    [Header("Altri bonus (facoltativi)")]
    public Statistiche.Modificatore bonus = new Statistiche.Modificatore();

    public Statistiche.Modificatore Modificatore() => new Statistiche.Modificatore
    {
        fonte = Nome,
        armatura = armatura + bonus.armatura,
        armaturaPercento = bonus.armaturaPercento,
        bonusDanno = bonus.bonusDanno,
        probabilitaCritico = bonus.probabilitaCritico,
        moltiplicatoreCritico = bonus.moltiplicatoreCritico,
        velocitaParata = bonus.velocitaParata,
        velocitaAttacco = bonus.velocitaAttacco,
        vitaMassimaPercento = bonus.vitaMassimaPercento,
        furtivita = furtivita + bonus.furtivita,
        resistenzaMassimaPercento = bonus.resistenzaMassimaPercento,
        manaMassimo = manaMassimo + bonus.manaMassimo,
        manaMassimoPercento = bonus.manaMassimoPercento,
        recuperoMana = bonus.recuperoMana,
        potenzaIncantesimi = bonus.potenzaIncantesimi,
        durataEvocazioni = bonus.durataEvocazioni,
    };
}
