using UnityEngine;

// Un bastone (o una verga) dello Stregone. Sta nella prima casella dell'equipaggiamento, quella dell'arma.
// A cosa serve: non fa danno da solo. Cambia gli incantesimi con un pro e un contro (sezione "Magia"), per
// esempio "Brace +15% di danno, ma gli effetti del Lago Nero durano il 25% in meno" (Docs/incantesimi-stregone.md).
// Un bastone a due mani (il Bastone del lago) non permette il libro, come lo spadone con lo scudo.
// Senza bastone lo Stregone lancia lo stesso, senza pro e contro.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Bastone, oppure con il menu
// "magic-gnl > Crea oggetti dello Stregone".
[CreateAssetMenu(fileName = "nuovo-bastone", menuName = "magic-gnl/Oggetti/Bastone")]
public class DatiBastone : DatiOggetto
{
    public enum Tipo { Bastone, Verga }

    [Header("Tipo")]
    public Tipo tipo = Tipo.Bastone;
    [Tooltip("A due mani: con questo bastone non si può tenere il libro.")]
    public bool dueMani = false;

    [Header("Magia (pro e contro)")]
    public ModificatoriMagia magia = new ModificatoriMagia();

    [Header("Altri bonus (facoltativi)")]
    public Statistiche.Modificatore bonus = new Statistiche.Modificatore();

    public Statistiche.Modificatore Modificatore() => Statistiche.Modificatore.Somma(Nome, bonus, new Statistiche.Modificatore());
}
