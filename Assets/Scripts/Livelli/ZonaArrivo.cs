using UnityEngine;
using UnityEngine.Events;

// Una zona che si attiva quando il giocatore ci arriva vicino: "sei arrivato dal fabbro".
// A cosa serve: chiudere gli obiettivi del tipo "Vai da..." e far partire cose quando si entra in un posto.
// Come montarlo: su un oggetto vuoto nel punto giusto; "Raggio" in metri (nella scena si vede un cerchio azzurro
// quando l'oggetto è selezionato). "All'Arrivo" si attiva una volta sola. Per accenderla solo a un certo punto della
// quest, si lascia l'oggetto spento e lo si accende da un evento (GameObject > SetActive).
public class ZonaArrivo : MonoBehaviour
{
    [SerializeField, Min(0.5f)] float raggio = 5f;
    [SerializeField] UnityEvent allArrivo;

    GiocatoreControllo giocatore;
    bool arrivato;

    void Update()
    {
        if (arrivato) return;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;
        Vector3 d = giocatore.transform.position - transform.position;
        d.y = 0f;
        if (d.magnitude > raggio) return;
        arrivato = true;
        allArrivo.Invoke();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, raggio);
    }
}
