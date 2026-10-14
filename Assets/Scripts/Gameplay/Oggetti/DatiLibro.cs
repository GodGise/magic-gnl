using UnityEngine;

// Un libro dello Stregone. Sta nella seconda casella dell'equipaggiamento, quella che per il Guerriero è lo Scudo
// (il Ladro lì mette l'arma a distanza): così le caselle restano 4 per tutte le classi.
// A cosa serve: dà caselle in più per gli incantesimi, mana massimo, ricarica del mana e altri pro e contro
// (sezione "Magia", vedi ModificatoriMagia). Un libro grosso pesa un po' (schivata più cara, corsa più lenta).
// Regola delle due mani: con il Bastone del lago (a due mani) il libro si toglie, come lo scudo con lo spadone.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Libro, oppure con il menu
// "magic-gnl > Crea oggetti dello Stregone". Per provarlo: trascinarlo nella casella Libro di Equipaggiamento.
[CreateAssetMenu(fileName = "nuovo-libro", menuName = "magic-gnl/Oggetti/Libro")]
public class DatiLibro : DatiOggetto
{
    [Header("Mana")]
    [Tooltip("Mana massimo in più o in meno, in punti (30 = +30, -25 = -25).")]
    public float manaMassimo = 0f;
    [Tooltip("Il mana si ricarica più in fretta o più piano, in percentuale (-25 = il 25% più piano).")]
    public float recuperoMana = 0f;

    [Header("Magia (pro e contro)")]
    [Tooltip("Qui anche le caselle in più per gli incantesimi (Caselle Extra).")]
    public ModificatoriMagia magia = new ModificatoriMagia();

    [Header("Peso")]
    [Tooltip("Resistenza in più spesa per ogni schivata.")]
    public float costoSchivataExtra = 0f;
    [Tooltip("Velocità di corsa rispetto a quella normale (1 = nessun rallentamento, 0,99 = -1%).")]
    [Range(0.5f, 1f)] public float moltiplicatoreVelocita = 1f;

    public Statistiche.Modificatore Modificatore() => new Statistiche.Modificatore
    {
        fonte = Nome,
        manaMassimo = manaMassimo,
        recuperoMana = recuperoMana,
    };
}
