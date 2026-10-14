using UnityEngine;

// Un oggetto che il giocatore porta in braccio durante una quest (la legna dal capanno al camino).
// A cosa serve: si vede quello che si sta portando. Con "Prendi" l'oggetto va davanti al petto del giocatore e lo
// segue; con "Posa" sparisce (di solito nello stesso momento si accende la sua copia nel posto giusto, per esempio
// la legna nel cesto accanto al camino).
// Come montarlo: sull'oggetto da portare (spento all'inizio, va bene). Dagli eventi: TrasportaOggetto > Prendi e
// TrasportaOggetto > Posa. "Posizione In Braccio" sposta l'oggetto rispetto al giocatore.
// Co-op: il prologo si gioca da soli, quindi non serve condividerlo.
public class TrasportaOggetto : MonoBehaviour
{
    [Tooltip("Dove sta l'oggetto rispetto al giocatore (destra, su, avanti), in metri.")]
    [SerializeField] Vector3 posizioneInBraccio = new Vector3(0f, 0.15f, 0.5f);

    public void Prendi()
    {
        var giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;
        // Niente collisioni mentre lo si porta: non deve spingere il giocatore.
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;
        transform.SetParent(giocatore.transform, false);
        transform.localPosition = posizioneInBraccio;
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(true);
    }

    public void Posa() => gameObject.SetActive(false);
}
