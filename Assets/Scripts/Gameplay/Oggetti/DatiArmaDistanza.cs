using UnityEngine;

// Un'arma a distanza del Ladro: arco corto, arco lungo o balestra.
// A cosa serve: contiene i numeri del tiro (danno, carica, ricarica, portata, velocità della freccia, critico,
// armatura ignorata) e il bonus contro i nemici che non si sono accorti di te: l'assassino colpisce da lontano
// e di soppiatto.
// - Arco corto: veloce, poco danno, buono anche in mischia.
// - Arco lungo: carica lunga, portata e danno alti.
// - Balestra: danno altissimo e armatura ignorata, ma ricarica lentissima.
// ATTENZIONE: il tiro con l'arco non esiste ancora nel gioco. Questi oggetti sono già pronti con i loro numeri;
// la meccanica (mirare, tendere, frecce) si farà dopo e li userà così come sono.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Arma a distanza.
[CreateAssetMenu(fileName = "nuova-arma-distanza", menuName = "magic-gnl/Oggetti/Arma a distanza")]
public class DatiArmaDistanza : DatiOggetto
{
    public enum Tipo { ArcoCorto, ArcoLungo, Balestra }

    [Header("Tipo")]
    public Tipo tipo = Tipo.ArcoCorto;

    [Header("Tiro")]
    [Tooltip("Danno di una freccia (o dardo), prima di critico e armatura del nemico.")]
    public float danno = 20f;
    [Tooltip("Resistenza spesa per ogni tiro.")]
    public float costoTiro = 15f;
    [Tooltip("Secondi per tendere l'arco (o puntare la balestra) prima di poter tirare.")]
    public float carica = 0.5f;
    [Tooltip("Secondi dopo il tiro prima di poterne fare un altro (ricarica).")]
    public float ricarica = 0.4f;
    [Tooltip("Distanza massima utile del tiro, in metri.")]
    public float portata = 30f;
    [Tooltip("Velocità della freccia, in metri al secondo.")]
    public float velocitaFreccia = 35f;
    [Tooltip("Quota dell'armatura del nemico che il colpo ignora (0,5 = metà). Alta per le balestre.")]
    [Range(0f, 1f)] public float penetrazioneArmatura = 0f;

    [Header("Assassino")]
    [Tooltip("Contro un nemico che non ti ha ancora visto, il danno si moltiplica per questo (1,5 = +50%).")]
    public float moltiplicatoreNonVisto = 1.5f;

    [Header("Critico (si somma a quello del personaggio)")]
    [Tooltip("Punti percentuali di critico in più (5 = +5%).")]
    public float probabilitaCritico = 0f;
    [Tooltip("Aggiunta al moltiplicatore del critico (0,25 = da ×1,75 a ×2).")]
    public float moltiplicatoreCritico = 0f;
}
