using UnityEngine;

// Animazione provvisoria della figura a blocchi creata da AspettoUmanoide.
// A cosa serve: mentre il personaggio si muove, le gambe oscillano avanti e indietro, le braccia
// fanno il contrario (braccio sinistro avanti con la gamba destra, come camminando davvero) e il
// corpo saltella appena a ogni passo. Più si va veloci (corsa, sprint, schivata), più il passo è
// ampio e rapido; da fermo la figura torna piano dritta. Funziona sia per il giocatore sia per i nemici.
// Come montarlo: non serve montarlo. Lo aggiunge da solo AspettoUmanoide quando crea la figura.
// I numeri si possono regolare dall'Inspector durante il Play (sul giocatore o sul nemico).
public class AnimazioneUmanoide : MonoBehaviour
{
    [Tooltip("Ampiezza massima dell'oscillazione delle gambe, in gradi.")]
    [SerializeField] float angoloGambe = 35f;
    [Tooltip("Ampiezza massima dell'oscillazione delle braccia, in gradi.")]
    [SerializeField] float angoloBraccia = 30f;
    [Tooltip("Metri percorsi a ogni passo: più è piccolo, più le gambe vanno veloci.")]
    [SerializeField] float lunghezzaPasso = 0.9f;
    [Tooltip("Velocità (metri al secondo) a cui il passo raggiunge l'ampiezza massima.")]
    [SerializeField] float velocitaPienaAmpiezza = 5f;
    [Tooltip("Di quanto si alza il corpo a ogni passo, in metri.")]
    [SerializeField] float saltelloCorpo = 0.05f;
    [Tooltip("Quanto in fretta l'oscillazione parte e si ferma.")]
    [SerializeField] float prontezza = 4f;

    // Oltre questa velocità non è camminare ma uno spostamento di colpo (per esempio la rinascita).
    const float VelocitaTeletrasporto = 30f;

    Transform figura, gambaSinistra, gambaDestra, braccioSinistro, braccioDestro;
    Vector3 posizioneFigura;
    Vector3 ultimaPosizione;
    float fase;
    float ampiezza;

    // Chiamato da AspettoUmanoide: la figura e i quattro perni (anche e spalle) da far oscillare.
    public void Imposta(Transform figura, Transform gambaSinistra, Transform gambaDestra, Transform braccioSinistro, Transform braccioDestro)
    {
        this.figura = figura;
        this.gambaSinistra = gambaSinistra;
        this.gambaDestra = gambaDestra;
        this.braccioSinistro = braccioSinistro;
        this.braccioDestro = braccioDestro;
        posizioneFigura = figura.localPosition;
        ultimaPosizione = transform.position;
    }

    void LateUpdate()
    {
        if (figura == null) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Quanto si è spostato in orizzontale dall'ultimo fotogramma.
        Vector3 spostamento = transform.position - ultimaPosizione;
        spostamento.y = 0f;
        ultimaPosizione = transform.position;
        float distanza = spostamento.magnitude;
        if (distanza / dt > VelocitaTeletrasporto) distanza = 0f;

        // Ampiezza del passo: cresce con la velocità e si spegne piano quando ci si ferma.
        float obiettivo = Mathf.Clamp01(distanza / dt / velocitaPienaAmpiezza);
        ampiezza = Mathf.MoveTowards(ampiezza, obiettivo, dt * prontezza);

        // Un ciclo completo (sinistra e destra) ogni due passi.
        fase += distanza / lunghezzaPasso * Mathf.PI;
        float onda = Mathf.Sin(fase);

        float gamba = onda * angoloGambe * ampiezza;
        float braccio = onda * angoloBraccia * ampiezza;
        gambaSinistra.localRotation = Quaternion.Euler(gamba, 0f, 0f);
        gambaDestra.localRotation = Quaternion.Euler(-gamba, 0f, 0f);
        braccioSinistro.localRotation = Quaternion.Euler(-braccio, 0f, 0f);
        braccioDestro.localRotation = Quaternion.Euler(braccio, 0f, 0f);

        // Piccolo saltello: il corpo sale a metà di ogni passo.
        figura.localPosition = posizioneFigura + Vector3.up * (Mathf.Abs(Mathf.Cos(fase)) * saltelloCorpo * ampiezza);
    }
}
