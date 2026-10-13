using UnityEngine;

// La borsa del giocatore: le monete d'oro (deciso da Giuseppe il 13 ottobre; nel parlato e nelle interfacce
// basta "monete"; le proposte scartate sono in Docs/monete.md).
// A cosa serve: tiene il numero di monete. Si spendono e si guadagnano nei negozi (Negozio, ricette con "Prezzo"
// e "Ricavo") e si ricevono come ricompensa (DaiMonete). Non è un oggetto dello zaino: è solo un numero,
// così mille monete non riempiono l'inventario.
// Come montarlo: non serve. Si aggiunge da sola al giocatore la prima volta che serve (Borsa.Di); per cambiare le
// monete di partenza si mette il componente sul Giocatore e si cambia "Partenza" nell'Inspector.
// Per ora non si salva (il salvataggio vero è ancora da fare, come per lo zaino).
public class Borsa : MonoBehaviour
{
    [Tooltip("Monete all'inizio della partita.")]
    [SerializeField, Min(0)] int partenza = 15;

    public int Quante { get; private set; }
    // Avvisa quando il numero cambia (per ridisegnare i numeri a schermo).
    public event System.Action Cambiata;

    // Colori della moneta disegnata (oro con il bordo scuro e un riflesso, finché Nazar non fa l'icona).
    public static readonly Color Colore = new Color(0.9f, 0.72f, 0.28f);
    public static readonly Color Bordo = new Color(0.55f, 0.4f, 0.12f);
    public static readonly Color Riflesso = new Color(1f, 0.95f, 0.7f);

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

    // "1 moneta", "45 monete" (nelle 8 lingue).
    public static string Testo(int quante) =>
        quante == 1 ? Lingua.T("valuta.una") : string.Format(Lingua.T("valuta.tante"), quante);

    // La moneta disegnata: tonda d'oro (un quadrato e un rombo sovrapposti) con il bordo scuro e un riflesso.
    public static void Disegna(Vector2 centro, float lato, float alfa)
    {
        float m = lato * 0.36f;
        GraficaMenu.Rombo(centro, lato, GraficaMenu.Con(Bordo, alfa));
        GraficaMenu.Riempi(new Rect(centro.x - m, centro.y - m, m * 2f, m * 2f), GraficaMenu.Con(Bordo, alfa));
        GraficaMenu.Rombo(centro, lato * 0.82f, GraficaMenu.Con(Colore, alfa));
        GraficaMenu.Riempi(new Rect(centro.x - m * 0.82f, centro.y - m * 0.82f, m * 1.64f, m * 1.64f), GraficaMenu.Con(Colore, alfa));
        GraficaMenu.Rombo(centro + new Vector2(-lato * 0.15f, -lato * 0.15f), lato * 0.2f, GraficaMenu.Con(Riflesso, alfa));
    }
}
