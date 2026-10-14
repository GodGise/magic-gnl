using UnityEngine;
using UnityEngine.Events;

// Una zona davanti a una porta chiusa: quando il giocatore ci arriva e la porta è ancora chiusa, parte "Davanti Alla
// Porta Chiusa" (per esempio "Il cancello è chiuso. Da qualche parte ci sarà un meccanismo." e l'obiettivo di trovare
// la leva). Se la porta è già aperta non succede niente.
// Come montarlo: su un oggetto vuoto davanti alla porta; "Porta" = la porta; "Raggio" in metri (cerchio arancione
// nella scena). Parte una volta sola.
public class ZonaPortaChiusa : MonoBehaviour
{
    [SerializeField] Porta porta;
    [SerializeField, Min(0.5f)] float raggio = 3f;
    [SerializeField] UnityEvent davantiAllaPortaChiusa;

    GiocatoreControllo giocatore;
    bool fatto;

    void Update()
    {
        if (fatto || porta == null) return;
        if (porta.Aperta) { fatto = true; return; }
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;
        Vector3 d = giocatore.transform.position - transform.position;
        d.y = 0f;
        if (d.magnitude > raggio) return;
        fatto = true;
        davantiAllaPortaChiusa.Invoke();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, raggio);
    }
}
