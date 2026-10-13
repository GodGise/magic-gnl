using UnityEngine;

// La borsa del giocatore: le monete del gioco, le **Lacrime del lago** (proposta di Lorenzo e del suo Claude,
// 13 ottobre; altre proposte di nome e materiale in Docs/monete.md).
// Cosa sono: sassolini neri e lisci come vetro che il Lago Nero lascia sulla riva solo dopo le tempeste.
// Nessuno sa da dove vengano, e sono così rare che al villaggio valgono più del ferro.
// A cosa serve: tiene il numero di Lacrime. Si spendono e si guadagnano nei negozi (Negozio, ricette con "Prezzo"
// e "Ricavo") e si ricevono come ricompensa (DaiMonete). Non è un oggetto dello zaino: è solo un numero,
// così mille monete non riempiono l'inventario.
// Come montarlo: non serve. Si aggiunge da sola al giocatore la prima volta che serve (Borsa.Di); per cambiare le
// monete di partenza si mette il componente sul Giocatore e si cambia "Partenza" nell'Inspector.
// Per ora non si salva (il salvataggio vero è ancora da fare, come per lo zaino).
public class Borsa : MonoBehaviour
{
    [Tooltip("Lacrime del lago all'inizio della partita.")]
    [SerializeField, Min(0)] int partenza = 15;

    public int Quante { get; private set; }
    // Avvisa quando il numero cambia (per ridisegnare i numeri a schermo).
    public event System.Action Cambiata;

    // Colore della moneta disegnata (nero lucido con un riflesso blu, finché Nazar non fa l'icona).
    public static readonly Color Colore = new Color(0.12f, 0.14f, 0.2f);
    public static readonly Color Riflesso = new Color(0.45f, 0.6f, 0.85f);

    void Awake() => Quante = partenza;

    public void Aggiungi(int quante)
    {
        if (quante <= 0) return;
        Quante += quante;
        Cambiata?.Invoke();
    }

    // Toglie le monete se ci sono tutte; altrimenti non toglie niente e restituisce falso.
    public bool Spendi(int quante)
    {
        if (quante <= 0) return true;
        if (Quante < quante) return false;
        Quante -= quante;
        Cambiata?.Invoke();
        return true;
    }

    // La borsa del giocatore (la aggiunge se manca).
    public static Borsa Di(Component giocatore)
    {
        if (giocatore == null) return null;
        var borsa = giocatore.GetComponent<Borsa>();
        if (borsa == null) borsa = giocatore.gameObject.AddComponent<Borsa>();
        return borsa;
    }

    // "1 lacrima", "45 lacrime" (nelle 8 lingue).
    public static string Testo(int quante) =>
        quante == 1 ? Lingua.T("valuta.una") : string.Format(Lingua.T("valuta.tante"), quante);

    // La moneta disegnata: una goccia nera lucida (rombo scuro con un riflesso).
    public static void Disegna(Vector2 centro, float lato, float alfa)
    {
        GraficaMenu.Rombo(centro, lato, GraficaMenu.Con(Riflesso, 0.8f * alfa));
        GraficaMenu.Rombo(centro, lato * 0.8f, GraficaMenu.Con(Colore, alfa));
        GraficaMenu.Rombo(centro + new Vector2(-lato * 0.15f, -lato * 0.15f), lato * 0.2f, GraficaMenu.Con(Riflesso, alfa));
    }
}
