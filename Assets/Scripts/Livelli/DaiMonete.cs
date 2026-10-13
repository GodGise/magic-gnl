using UnityEngine;

// Dà al giocatore delle Lacrime del lago (le monete, vedi Borsa) come ricompensa: per esempio Hobb, alla fine della
// quest 3, ne dà 20 per i ratti. In alto compare "Ricevuto: 20 lacrime".
// Come montarlo: su un oggetto vuoto (o sul personaggio che paga); "Quante" = le monete da dare. Da un evento
// (per esempio "Al Termine" di un dialogo) si sceglie DaiMonete > Dai. Dà una volta sola.
public class DaiMonete : MonoBehaviour
{
    [SerializeField, Min(1)] int quante = 10;
    [Tooltip("Secondi a schermo della scritta.")]
    [SerializeField] float secondiScritta = 3f;

    bool fatto;

    public void Dai()
    {
        if (fatto) return;
        var giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;
        fatto = true;
        Borsa.Di(giocatore).Aggiungi(quante);
        MessaggiSchermo.Mostra(Lingua.T("hud.ricevuto") + ": " + Borsa.Testo(quante), secondiScritta);
        Suoni.Suona(Suono.Raccolta, giocatore.transform.position + Vector3.up, 0.8f, 1.2f);
    }
}
