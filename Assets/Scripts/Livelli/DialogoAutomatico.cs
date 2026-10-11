using UnityEngine;
using UnityEngine.Events;

// Un dialogo che parte da solo, senza premere il tasto: all'avvio della scena (per esempio il risveglio in casa)
// oppure quando lo chiama un altro evento (per esempio Tobin che parla appena prendi la legna).
// A cosa serve: le righe "di scena" del prologo, che il giocatore non sceglie di ascoltare.
// Come montarlo: su un oggetto qualsiasi (anche vuoto). "Righe" come nell'Interlocutore (chi parla e testo, chiavi
// di Lingua). "All'Avvio" lo fa partire appena inizia la scena, dopo "Ritardo" secondi; altrimenti si fa partire da
// un evento scegliendo DialogoAutomatico > Avvia. "Al Termine" si attiva quando il giocatore ha letto l'ultima riga.
// Parte una volta sola.
public class DialogoAutomatico : MonoBehaviour
{
    [SerializeField] RigaDialogo[] righe;
    [Tooltip("Parte da solo all'inizio della scena.")]
    [SerializeField] bool allAvvio;
    [Tooltip("Secondi di attesa prima di partire (per esempio per lasciar finire la dissolvenza dal nero).")]
    [SerializeField, Min(0f)] float ritardo;
    [SerializeField] UnityEvent alTermine;

    bool partito;
    float partiAlle = -1f;

    void Start()
    {
        if (allAvvio) Avvia();
    }

    public void Avvia()
    {
        if (partito || partiAlle >= 0f) return;
        partiAlle = Time.time + ritardo;
    }

    void Update()
    {
        if (partito || partiAlle < 0f || Time.time < partiAlle || Dialogo.InCorso) return;
        partito = true;
        Dialogo.Avvia(righe, () => alTermine.Invoke());
    }
}
