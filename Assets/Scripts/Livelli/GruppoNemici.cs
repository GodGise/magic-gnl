using UnityEngine;
using UnityEngine.Events;

// Un gruppo di nemici da sconfiggere tutti (per esempio i tre ratti del granaio di Hobb).
// A cosa serve: quando sono morti tutti i nemici del gruppo si attiva "Quando Sconfitti" (per esempio Hobb che
// ringrazia e la quest che finisce). I nemici sono i Bersaglio figli di questo oggetto (anche spenti all'inizio).
// Mentre il gruppo è attivo, l'obiettivo a schermo mostra quanti ne restano (per esempio "Ratti rimasti: 2"),
// se "Chiave Rimasti" è una chiave di Lingua con {0} al posto del numero.
// Come montarlo: su un oggetto vuoto, con i nemici come figli. Di solito si lascia spento e lo si accende da un
// evento quando lo scontro deve iniziare (GameObject > SetActive). Si attiva una volta sola.
public class GruppoNemici : MonoBehaviour
{
    [Tooltip("Chiave di Lingua per l'obiettivo con il numero dei nemici rimasti ({0}). Vuoto = non cambia l'obiettivo.")]
    [SerializeField] string chiaveRimasti;
    [SerializeField] UnityEvent quandoSconfitti;

    Bersaglio[] nemici;
    bool finito;
    int ultimiRimasti = -1;

    void OnEnable()
    {
        nemici = GetComponentsInChildren<Bersaglio>(true);
    }

    void Update()
    {
        if (finito || nemici == null || nemici.Length == 0) return;
        int rimasti = 0;
        foreach (var n in nemici) if (n != null && !n.Morto) rimasti++;
        if (rimasti != ultimiRimasti)
        {
            ultimiRimasti = rimasti;
            if (!string.IsNullOrEmpty(chiaveRimasti) && rimasti > 0 && Missioni.Istanza != null)
                Missioni.Istanza.ImpostaObiettivoConNumero(chiaveRimasti, rimasti);
        }
        if (rimasti > 0) return;
        finito = true;
        quandoSconfitti.Invoke();
    }
}
