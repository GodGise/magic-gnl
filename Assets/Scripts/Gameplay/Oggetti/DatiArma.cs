using UnityEngine;

// Un'arma del Guerriero: spada, spadone, ascia o mazza.
// A cosa serve: contiene tutti i numeri del colpo che dipendono dall'arma: danno, velocità, portata, ampiezza,
// resistenza spesa, critico e quanta armatura del nemico ignora. Ogni tipo cambia il modo di combattere:
// - Spada: equilibrata, veloce, la base di confronto.
// - Ascia: più lenta, colpo largo, critici forti.
// - Mazza: lenta, colpo stretto, ma ignora gran parte dell'armatura (ottima contro i nemici corazzati).
// - Spadone: a due mani (niente scudo), lentissimo, portata e danno altissimi; para con la lama, male.
// Quando il giocatore la equipaggia (vedi Equipaggiamento), questi numeri prendono il posto di quelli
// scritti in Giocatore Controllo.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Arma, oppure con il menu
// "magic-gnl > Crea oggetti del Guerriero" per quelli già decisi.
[CreateAssetMenu(fileName = "nuova-arma", menuName = "magic-gnl/Oggetti/Arma")]
public class DatiArma : DatiOggetto
{
    public enum Tipo { Spada, Spadone, Ascia, Mazza }

    [Header("Tipo")]
    public Tipo tipo = Tipo.Spada;
    [Tooltip("Arma a due mani: con questa non si può tenere lo scudo.")]
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
    [Tooltip("Fin dove arriva il colpo davanti al personaggio, in metri.")]
    public float portata = 1.8f;
    [Tooltip("Ampiezza del colpo, in metri (raggio della zona colpita).")]
    public float raggio = 1.3f;
    [Tooltip("Ampiezza in gradi dell'arco davanti al personaggio colpito dal fendente (più largo = prende più nemici).")]
    public float arco = 120f;
    [Tooltip("Spinta in avanti durante il colpo, in metri al secondo.")]
    public float affondo = 3f;
    [Tooltip("Quota dell'armatura del nemico che il colpo ignora (0,5 = metà). Alta per le mazze.")]
    [Range(0f, 1f)] public float penetrazioneArmatura = 0f;

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

    // Bonus di critico dell'arma, da aggiungere alle Statistiche finché è equipaggiata.
    public Statistiche.Modificatore ModificatoreCritico() => new Statistiche.Modificatore
    {
        fonte = Nome,
        probabilitaCritico = probabilitaCritico,
        moltiplicatoreCritico = moltiplicatoreCritico,
    };
}
