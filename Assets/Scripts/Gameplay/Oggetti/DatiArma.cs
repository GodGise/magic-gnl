using UnityEngine;

// Un'arma (per il Guerriero, per esempio spada e scudo, ascia, mazza).
// A cosa serve: contiene tutti i numeri del combattimento corpo a corpo che dipendono dall'arma: danno,
// velocità del colpo, portata, resistenza spesa, critico e quanto para. Ogni tipo d'arma cambia così il modo
// di combattere, non solo il danno. Quando il giocatore la equipaggia (vedi Equipaggiamento), questi numeri
// prendono il posto di quelli scritti in Giocatore Controllo.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Arma, poi si compilano i
// numeri nell'Inspector. Oppure con il menu "magic-gnl > Crea oggetti del Guerriero" per quelli già decisi.
[CreateAssetMenu(fileName = "nuova-arma", menuName = "magic-gnl/Oggetti/Arma")]
public class DatiArma : DatiOggetto
{
    public enum Tipo { SpadaEScudo, Ascia, Mazza, Spadone }

    [Header("Tipo")]
    public Tipo tipo = Tipo.SpadaEScudo;

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

    [Header("Critico (si somma a quello del personaggio)")]
    [Tooltip("Punti percentuali di critico in più (5 = +5%).")]
    public float probabilitaCritico = 0f;
    [Tooltip("Aggiunta al moltiplicatore del critico (0,25 = da ×1,75 a ×2).")]
    public float moltiplicatoreCritico = 0f;

    [Header("Parata")]
    [Tooltip("Quota di danno fermata parando (0,9 = 90%). Con lo scudo è alta, con un'arma a due mani molto meno.")]
    [Range(0f, 1f)] public float dannoAssorbitoInParata = 0.9f;
    [Tooltip("Resistenza persa per ogni colpo parato.")]
    public float costoColpoParato = 20f;

    // Bonus di critico dell'arma, da aggiungere alle Statistiche finché è equipaggiata.
    public Statistiche.Modificatore ModificatoreCritico() => new Statistiche.Modificatore
    {
        fonte = Nome,
        probabilitaCritico = probabilitaCritico,
        moltiplicatoreCritico = moltiplicatoreCritico,
    };
}
