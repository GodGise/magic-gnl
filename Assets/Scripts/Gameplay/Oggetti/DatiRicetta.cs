using UnityEngine;

// Una voce di un negozio: "dai questo, ricevi quello". Ci sono tre modi (decisione di Lorenzo, 13 ottobre):
//   - Baratto: ingredienti -> oggetto ("3 Fiori di riva -> 2 Pozioni di Ilse"). Il modo più conveniente.
//   - Compra:  monete (Prezzo) -> oggetto ("30 Lacrime -> 1 Pozione di Ilse"). Quando mancano i fiori; costa di più.
//   - Vendi:   oggetto -> monete (Ricavo) ("1 Fiore di riva -> 4 Lacrime"). "Da" resta vuoto.
// Si possono anche mescolare (fiori + monete -> pozione): conta come baratto.
// Le monete sono le Lacrime del lago (Borsa, Docs/monete.md). Fiori, pozioni e prezzi: Docs/erbe-e-pozioni.md.
// La stessa voce si può usare in più negozi.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Ricetta; oppure il menu
// "magic-gnl > Erbe: crea fiori, pozioni e ricette", che crea quelle della bottega di Ilse in Assets/Dati/Ricette.
// Poi la si mette nell'elenco "Ricette" di un Negozio.
[CreateAssetMenu(fileName = "nuova-ricetta", menuName = "magic-gnl/Oggetti/Ricetta")]
public class DatiRicetta : ScriptableObject
{
    public enum Modo { Baratto, Compra, Vendi }

    [System.Serializable]
    public struct Ingrediente
    {
        public DatiOggetto oggetto;
        [Min(1)] public int quanti;
    }

    [Tooltip("Oggetti da dare al negoziante (fiori). Vuoto = si compra solo con le monete.")]
    public Ingrediente[] chiede;
    [Tooltip("Lacrime del lago da pagare (0 = niente).")]
    [Min(0)] public int prezzo;
    [Tooltip("Cosa si riceve. Vuoto = si vende e si ricevono solo monete.")]
    public DatiOggetto da;
    [Min(1)] public int quantiDa = 1;
    [Tooltip("Lacrime del lago ricevute (per vendere).")]
    [Min(0)] public int ricavo;

    bool HaIngredienti
    {
        get
        {
            if (chiede == null) return false;
            foreach (var i in chiede) if (i.oggetto != null) return true;
            return false;
        }
    }

    public Modo Tipo => da == null ? Modo.Vendi : !HaIngredienti && prezzo > 0 ? Modo.Compra : Modo.Baratto;

    // Quante copie di un oggetto ci sono nello zaino.
    public static int Hai(Zaino zaino, DatiOggetto oggetto)
    {
        int n = 0;
        if (zaino == null || oggetto == null) return 0;
        foreach (var o in zaino.Oggetti) if (o == oggetto) n++;
        return n;
    }

    // Qualcosa si dà e qualcosa si riceve.
    public bool Valida => (da != null || ricavo > 0) && (HaIngredienti || prezzo > 0);

    // Si ricevono pozioni ma lo zaino ne ha già troppe (massimo 3 all'inizio del gioco, Zaino.MassimoConsumabili).
    public bool TroppiConsumabili(Zaino zaino) => da is DatiConsumabile && zaino != null && !zaino.PostoPerConsumabili(quantiDa);

    // Nello zaino e nella borsa c'è tutto quello che serve, e c'è posto per quello che si riceve?
    public bool Possibile(Zaino zaino, Borsa borsa)
    {
        if (!Valida || TroppiConsumabili(zaino)) return false;
        if (prezzo > 0 && (borsa == null || borsa.Quante < prezzo)) return false;
        if (chiede != null)
            foreach (var i in chiede)
                if (i.oggetto != null && Hai(zaino, i.oggetto) < Mathf.Max(1, i.quanti)) return false;
        return true;
    }

    // Toglie ingredienti e monete, dà oggetto e monete. Falso se manca qualcosa (allora non tocca niente).
    public bool Esegui(Zaino zaino, Borsa borsa)
    {
        if (!Possibile(zaino, borsa)) return false;
        if (prezzo > 0) borsa.Spendi(prezzo);
        if (chiede != null)
            foreach (var i in chiede)
                for (int k = 0; k < Mathf.Max(1, i.quanti) && i.oggetto != null; k++) zaino.Togli(i.oggetto);
        if (da != null) for (int k = 0; k < quantiDa; k++) zaino.Aggiungi(da);
        if (ricavo > 0 && borsa != null) borsa.Aggiungi(ricavo);
        return true;
    }
}
