using UnityEngine;

// Un'arma da corpo a corpo. Guerriero: spada, spadone, ascia o mazza. Ladro: pugnale, stiletto o doppi pugnali
// (veloci, danno basso, ma molto forti colpendo alle spalle: vedi "Moltiplicatore Alle Spalle").
// Stregone: bastone o verga. Sono armi MAGICHE: l'attacco lancia una sfera che costa MANA invece di resistenza;
// "Portata" è la distanza a cui la sfera cerca il nemico, "Preparazione" e "Recupero" sono carica e pausa del
// lancio. Colpo attivo, ampiezza, arco e affondo non contano. Il bastone del lago è a due mani: niente libro.
// A cosa serve: contiene tutti i numeri del colpo che dipendono dall'arma: danno, velocità, portata, ampiezza,
// resistenza spesa, critico e quanta armatura del nemico ignora. Ogni tipo cambia il modo di combattere:
// - Spada: equilibrata, veloce, la base di confronto.
// - Ascia: più lenta, colpo largo, critici forti.
// - Mazza: lenta, colpo stretto, ma ignora gran parte dell'armatura (ottima contro i nemici corazzati).
// - Spadone: a due mani (niente scudo), lentissimo, portata e danno altissimi; para con la lama, male.
// Quando il giocatore la equipaggia (vedi Equipaggiamento), questi numeri prendono il posto di quelli
// scritti in Giocatore Controllo.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Arma, oppure con il menu
// "magic-gnl > Crea oggetti del Guerriero" (del Ladro, dello Stregone) per quelli già decisi.
[CreateAssetMenu(fileName = "nuova-arma", menuName = "magic-gnl/Oggetti/Arma")]
public class DatiArma : DatiOggetto
{
    public enum Tipo { Spada, Spadone, Ascia, Mazza, Pugnale, Stiletto, DoppiPugnali, Bastone, Verga }

    [Header("Tipo")]
    public Tipo tipo = Tipo.Spada;
    [Tooltip("Arma a due mani: con questa non si può tenere lo scudo (per lo Stregone: il libro).")]
    public bool dueMani = false;

    [Header("Colpo")]
    [Tooltip("Danno di un colpo, prima di critico e armatura del nemico.")]
    public float danno = 25f;
    [Tooltip("Resistenza spesa per ogni colpo.")]
    public float costoAttacco = 20f;
    [Tooltip("Secondi di caricamento prima che il colpo parta (più alto = arma più lenta).")]
    public float preparazione = 0.25f;
    [Tooltip("Secondi in cui il colpo può andare a segno.")]
    public float colpoAttivo = 0.15f;
    [Tooltip("Secondi scoperti dopo il colpo, prima di poter fare altro.")]
    public float recupero = 0.35f;
    [Tooltip("Fin dove arriva il colpo davanti al personaggio, in metri. Bastoni e verghe: distanza a cui la sfera cerca il nemico.")]
    public float portata = 1.8f;
    [Tooltip("Ampiezza del colpo, in metri (raggio della zona colpita).")]
    public float raggio = 1.3f;
    [Tooltip("Ampiezza in gradi dell'arco davanti al personaggio colpito dal fendente (più largo = prende più nemici).")]
    public float arco = 120f;
    [Tooltip("Spinta in avanti durante il colpo, in metri al secondo.")]
    public float affondo = 3f;
    [Tooltip("Quota dell'armatura del nemico che il colpo ignora (0,5 = metà). Alta per le mazze.")]
    [Range(0f, 1f)] public float penetrazioneArmatura = 0f;
    [Tooltip("Colpo alle spalle: se colpisci un nemico da dietro, il danno si moltiplica per questo (2 = doppio). Le armi del Ladro lo hanno alto.")]
    public float moltiplicatoreAlleSpalle = 1f;

    [Header("Incantesimo (solo bastoni e verghe dello Stregone)")]
    [Tooltip("Mana speso per ogni lancio. Per bastoni e verghe sostituisce il costo in resistenza.")]
    public float costoMana = 3f;
    [Tooltip("Velocità della sfera, in metri al secondo.")]
    public float velocitaIncantesimo = 14f;

    [Header("Critico (si somma a quello del personaggio)")]
    [Tooltip("Punti percentuali di critico in più (5 = +5%).")]
    public float probabilitaCritico = 0f;
    [Tooltip("Aggiunta al moltiplicatore del critico (0,25 = da ×1,75 a ×2).")]
    public float moltiplicatoreCritico = 0f;

    [Header("Parata senza scudo (con lo scudo conta lo scudo)")]
    [Tooltip("Quota di danno fermata parando con l'arma (0,5 = metà).")]
    [Range(0f, 1f)] public float dannoAssorbitoSenzaScudo = 0.5f;
    [Tooltip("Resistenza persa per ogni colpo parato con l'arma.")]
    public float costoParataSenzaScudo = 25f;

    // Vero per bastoni e verghe: l'attacco è un incantesimo che costa mana.
    public bool Magica => tipo == Tipo.Bastone || tipo == Tipo.Verga;

    // Bonus di critico dell'arma, da aggiungere alle Statistiche finché è equipaggiata.
    public Statistiche.Modificatore ModificatoreCritico() => new Statistiche.Modificatore
    {
        fonte = Nome,
        probabilitaCritico = probabilitaCritico,
        moltiplicatoreCritico = moltiplicatoreCritico,
    };
}
