using UnityEngine;

// Strettoie: il personaggio si adatta agli spazi stretti fra due pareti.
// A cosa serve: misura quanto spazio libero c'è ai lati del giocatore, dove si trova e un po' più avanti
// nella direzione in cui cammina. Da questo calcola "Valore", da 0 (spazio normale) a 1 (strettoia piena),
// che cresce e cala in modo graduale. GiocatoreControllo lo usa per rallentare, stringere il suo ingombro
// (così passa dove prima urtava), vietare attacchi e sprint e rinfoderare la spada; AnimazioneUmanoide
// per girare la figura di fianco e farla avanzare a passetti laterali.
// Come montarlo: non serve montarlo. Lo aggiunge da solo GiocatoreControllo e lo aggiorna ogni fotogramma.
// I numeri si regolano dall'Inspector sul Giocatore (componente Passaggio Stretto).
public class PassaggioStretto : MonoBehaviour
{
    [Tooltip("Sotto questa larghezza libera (metri, da parete a parete) il personaggio inizia ad adattarsi.")]
    [SerializeField] float larghezzaInizio = 1.8f;
    [Tooltip("A questa larghezza (o meno) la strettoia è piena: tutto di fianco, al minimo della velocità.")]
    [SerializeField] float larghezzaPiena = 1.0f;
    [Tooltip("Quanto in fretta si entra e si esce dalla posa stretta (unità al secondo): più è basso, più è graduale.")]
    [SerializeField] float prontezza = 1.6f;
    [Tooltip("Fin dove guarda in avanti per accorgersi di una strettoia prima di entrarci, in metri.")]
    [SerializeField] float anticipo = 1.3f;
    [Tooltip("Fin dove guarda ai lati, in metri. Oltre questa distanza conta come spazio aperto.")]
    [SerializeField] float portataLaterale = 2f;

    // Altezze (rispetto al centro del personaggio, alto 2 metri) a cui si misura lo spazio: gambe, busto, spalle.
    static readonly float[] Altezze = { -0.6f, 0f, 0.5f };
    readonly RaycastHit[] colpi = new RaycastHit[8];

    // Da 0 a 1: quanto è stretto il passaggio in questo momento (già ammorbidito).
    public float Valore { get; private set; }
    // Larghezza libera misurata nell'ultimo controllo (metri), utile per regolare i numeri.
    public float UltimaLarghezza { get; private set; }

    // Chiamato da GiocatoreControllo ogni fotogramma. direzione = dove sta camminando (zero se è fermo).
    public void Aggiorna(Vector3 direzione, float dt)
    {
        direzione.y = 0f;
        // Da fermo misura lungo lo sguardo: se si è fermato in mezzo a una strettoia, ci resta.
        Vector3 avanti = direzione.sqrMagnitude > 0.0001f ? direzione.normalized : transform.forward;
        bool inMovimento = direzione.sqrMagnitude > 0.0001f;

        float larghezza = LarghezzaIn(transform.position, avanti);
        if (inMovimento)
        {
            // Guarda anche un po' avanti, così si stringe PRIMA di urtare gli spigoli dell'ingresso.
            larghezza = Mathf.Min(larghezza, LarghezzaAvanti(avanti, anticipo * 0.5f));
            larghezza = Mathf.Min(larghezza, LarghezzaAvanti(avanti, anticipo));
        }
        UltimaLarghezza = larghezza;

        float obiettivo = Mathf.InverseLerp(larghezzaInizio, larghezzaPiena, larghezza);
        Valore = Mathf.MoveTowards(Valore, obiettivo, prontezza * dt);
    }

    // Larghezza libera in un punto più avanti, se ci si arriva in linea retta (niente muri in mezzo).
    float LarghezzaAvanti(Vector3 avanti, float distanza)
    {
        Vector3 punto = transform.position + avanti * distanza;
        if (OstacoloPiuVicino(transform.position, avanti, distanza, out _)) return float.MaxValue; // c'è un muro davanti, non un varco
        if (Physics.CheckSphere(punto, 0.05f, ~0, QueryTriggerInteraction.Ignore)) return float.MaxValue;
        return LarghezzaIn(punto, avanti);
    }

    // Spazio libero a sinistra + spazio libero a destra, preso all'altezza dove è minore.
    float LarghezzaIn(Vector3 centro, Vector3 avanti)
    {
        Vector3 destra = Vector3.Cross(Vector3.up, avanti).normalized;
        float minima = float.MaxValue;
        foreach (float h in Altezze)
        {
            Vector3 da = centro + Vector3.up * h;
            float sinistra = OstacoloPiuVicino(da, -destra, portataLaterale, out float ds) ? ds : portataLaterale * 2f;
            float dx = OstacoloPiuVicino(da, destra, portataLaterale, out float dd) ? dd : portataLaterale * 2f;
            minima = Mathf.Min(minima, sinistra + dx);
        }
        return minima;
    }

    // Il muro più vicino in quella direzione. Non contano il giocatore stesso, i nemici e gli oggetti che si muovono.
    bool OstacoloPiuVicino(Vector3 da, Vector3 direzione, float portata, out float distanza)
    {
        distanza = float.MaxValue;
        int n = Physics.RaycastNonAlloc(da, direzione, colpi, portata, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            Collider c = colpi[i].collider;
            if (c.transform.IsChildOf(transform)) continue;
            if (c.GetComponentInParent<Bersaglio>() != null) continue;
            if (c.attachedRigidbody != null && !c.attachedRigidbody.isKinematic) continue;
            if (colpi[i].distance < distanza) distanza = colpi[i].distance;
        }
        return distanza < float.MaxValue;
    }
}
