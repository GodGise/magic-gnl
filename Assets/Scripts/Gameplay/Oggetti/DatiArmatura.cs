using UnityEngine;

// Un'armatura del Guerriero: pesante, media o leggera.
// A cosa serve: dà armatura (meno danno dai colpi, vedi Statistiche) ma pesa: più è pesante, più la schivata
// costa resistenza e più la corsa è lenta. Si equipaggia con Equipaggiamento.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Armatura.
[CreateAssetMenu(fileName = "nuova-armatura", menuName = "magic-gnl/Oggetti/Armatura")]
public class DatiArmatura : DatiOggetto
{
    public enum Peso { Pesante, Media, Leggera }

    [Header("Tipo")]
    public Peso peso = Peso.Media;

    [Header("Protezione")]
    [Tooltip("Armatura: 25 = -20% di danno, 50 = -33%, 100 = -50%.")]
    public float armatura = 10f;

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
    };
}
