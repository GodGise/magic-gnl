using UnityEngine;

// Un libro dello Stregone. Sta nella seconda casella dell'equipaggiamento, quella che per il Guerriero è lo Scudo
// (il Ladro lì mette l'arma a distanza): così le caselle restano 4 per tutte le classi.
// A cosa serve: lo Stregone non para, quindi la mano libera tiene un libro di incantesimi che dà più mana,
// una ricarica del mana più veloce, incantesimi più forti o evocazioni più lunghe. Un libro grosso pesa un po'
// (schivata più cara, corsa più lenta), come uno scudo.
// Regola delle due mani: con il bastone del lago (a due mani) il libro si toglie, come lo scudo con lo spadone.
// Le evocazioni non ci sono ancora: il loro numero è pronto per quando si faranno.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Libro, oppure con il menu
// "magic-gnl > Crea oggetti dello Stregone". Per provarlo: trascinarlo nella casella Libro di Equipaggiamento.
[CreateAssetMenu(fileName = "nuovo-libro", menuName = "magic-gnl/Oggetti/Libro")]
public class DatiLibro : DatiOggetto
{
    [Header("Magia")]
    [Tooltip("Mana massimo in più, in punti (20 = +20).")]
    public float manaMassimo = 20f;
    [Tooltip("Il mana si ricarica più in fretta, in percentuale (10 = +10%).")]
    public float recuperoMana = 0f;
    [Tooltip("Danno degli incantesimi in più, in percentuale (15 = +15%).")]
    public float potenzaIncantesimi = 0f;
    [Tooltip("Durata delle evocazioni in più, in percentuale (40 = +40%). Le evocazioni non ci sono ancora.")]
    public float durataEvocazioni = 0f;

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
        potenzaIncantesimi = potenzaIncantesimi,
        durataEvocazioni = durataEvocazioni,
    };
}
