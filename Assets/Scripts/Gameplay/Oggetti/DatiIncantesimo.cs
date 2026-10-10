using UnityEngine;

// Un incantesimo dello Stregone (Docs/incantesimi-stregone.md). Si trova nel mondo come gli altri oggetti, sta
// nello zaino e si mette in una delle caselle degli incantesimi (4 di base, fino a 6 con certi libri).
// A cosa serve: contiene i numeri dell'incantesimo: scuola, mana, danno, carica, recupero, attesa e i numeri del
// suo effetto (rallentamento, blocco, area, evocazione...). Cosa fa davvero lo decide "Effetto" (vedi MagiaStregone
// e IncantesimiEffetti). Bastoni, libri, vesti e amuleti cambiano questi numeri con i loro pro e contro.
// Parole usate: Carica = dal tasto al lancio; Recupero = dopo il lancio, prima di poter lanciare altro;
// Attesa = prima di poter rilanciare LO STESSO incantesimo (parte dal lancio; per le evocazioni dalla fine della
// precedente).
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Incantesimo, oppure con il menu
// "magic-gnl > Crea oggetti dello Stregone" per i 12 già decisi.
[CreateAssetMenu(fileName = "nuovo-incantesimo", menuName = "magic-gnl/Oggetti/Incantesimo")]
public class DatiIncantesimo : DatiOggetto
{
    public enum Effetto
    {
        Proiettile,     // Scintilla, Scheggia di ghiaccio, Dardo d'ombra: sfera verso il nemico
        PallaDiFuoco,   // sfera lenta che esplode e colpisce tutti nell'area
        Scia,           // striscia di fuoco a terra davanti
        Onda,           // onda davanti: spinge e stordisce i nemici bassi, rallenta quelli alti
        Pozza,          // pozza dove si mira: blocca, poi rallenta
        PassoOmbra,     // invisibile e più veloce; il primo incantesimo fa danno moltiplicato
        Velo,           // nebbia attorno: i nemici ti vedono solo da vicinissimo
        FuocoFatuo,     // luce che segue e colpisce il nemico più vicino
        Lupo,           // spirito del lupo che combatte
        Bambola,        // bambola che attira i nemici, poi scheletro
    }

    [Header("Incantesimo")]
    public ScuolaMagia scuola = ScuolaMagia.Brace;
    public Effetto effetto = Effetto.Proiettile;

    [Header("Costo e tempi")]
    public float costoMana = 5f;
    [Tooltip("Secondi dal tasto al lancio.")]
    public float carica = 0.3f;
    [Tooltip("Secondi dopo il lancio prima di poter lanciare altro (la schivata si può fare subito).")]
    public float recupero = 0.3f;
    [Tooltip("Secondi prima di poter rilanciare questo stesso incantesimo.")]
    public float attesa = 0.5f;

    [Header("Danno")]
    [Tooltip("Danno del colpo (o dell'esplosione). 0 = non fa danno.")]
    public float danno = 0f;
    [Tooltip("Danno al secondo (scia, gas, morso del lupo...).")]
    public float dannoAlSecondo = 0f;
    [Tooltip("Moltiplicatore del danno se il nemico è ignaro: non ha visto nessuno e non sta inseguendo nessuno (Dardo d'ombra: 2).")]
    public float moltiplicatoreIgnaro = 1f;

    [Header("Proiettile")]
    [Tooltip("Velocità della sfera, in metri al secondo.")]
    public float velocita = 18f;
    [Tooltip("Distanza a cui la sfera cerca il nemico, in metri.")]
    public float portata = 22f;
    [Tooltip("Grandezza della sfera (1 = normale).")]
    public float grandezza = 1f;
    public Color colore = new Color(1f, 0.55f, 0.2f);

    [Header("Area")]
    [Tooltip("Raggio dell'area, in metri (esplosione, pozza, nebbia, gas). Per scia e onda: larghezza.")]
    public float raggio = 0f;
    [Tooltip("Lunghezza davanti al personaggio, in metri (scia, onda).")]
    public float lunghezza = 0f;
    [Tooltip("Quanto resta l'area o l'effetto sul personaggio, in secondi (scia, nebbia, invisibilità).")]
    public float durata = 0f;

    [Header("Effetti sui nemici")]
    [Tooltip("Rallentamento, in percentuale (35 = vanno il 35% più piano).")]
    public float rallentamento = 0f;
    [Tooltip("Secondi di rallentamento.")]
    public float durataRallentamento = 0f;
    [Tooltip("Secondi di blocco (non si muovono).")]
    public float blocco = 0f;
    [Tooltip("Secondi di stordimento (non si muovono e non attaccano).")]
    public float stordimento = 0f;
    [Tooltip("Metri di spinta indietro.")]
    public float spinta = 0f;
    [Tooltip("Onda: altezza dell'onda in metri. I nemici più bassi vengono spinti e storditi, quelli più alti solo rallentati.")]
    public float altezza = 0f;
    [Tooltip("Pozza: rallentamento dopo il blocco, in percentuale.")]
    public float rallentamentoDopo = 0f;
    [Tooltip("Pozza: secondi di rallentamento dopo il blocco.")]
    public float durataRallentamentoDopo = 0f;

    [Header("Personaggio (Passo d'ombra)")]
    [Tooltip("Velocità in più mentre l'effetto è attivo, in percentuale (40 = +40%).")]
    public float velocitaInPiu = 0f;
    [Tooltip("Moltiplicatore del danno del primo incantesimo lanciato durante l'effetto (2,5).")]
    public float moltiplicatoreSuccessivo = 1f;
    [Tooltip("Se si lancia un incantesimo durante l'effetto: rallentamento del personaggio, in percentuale.")]
    public float rallentamentoInterruzione = 0f;
    [Tooltip("Secondi di quel rallentamento.")]
    public float durataRallentamentoInterruzione = 0f;

    [Header("Evocazione")]
    [Tooltip("Quante evocazioni di questo tipo insieme (l'Occhio del lago raddoppia lupo e bambola).")]
    public int massimoInsieme = 1;
    public float vitaEvocazione = 0f;
    [Tooltip("Danno di ogni colpo dell'evocazione.")]
    public float dannoEvocazione = 0f;
    [Tooltip("Secondi fra un colpo e l'altro dell'evocazione.")]
    public float intervalloColpi = 1.5f;
    [Tooltip("Vita recuperata dall'evocazione per ogni nemico che uccide (Spirito del lupo).")]
    public float vitaPerUccisione = 0f;
    [Tooltip("Lupo: lunghezza della scia dietro di lui, in metri.")]
    public float lunghezzaScia = 0f;
    [Tooltip("Bambola: raggio in cui attira i nemici, in metri.")]
    public float raggioAttrazione = 0f;
    [Tooltip("Bambola: secondi prima di animarsi in scheletro.")]
    public float tempoTrasformazione = 0f;
    public float vitaScheletro = 0f;
    public float dannoScheletro = 0f;
    public float intervalloScheletro = 1.6f;
    public float durataScheletro = 0f;
}
