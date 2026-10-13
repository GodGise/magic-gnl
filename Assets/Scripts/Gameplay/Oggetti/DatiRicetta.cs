using UnityEngine;

// Una ricetta (o uno scambio) di un negozio: "dai questi oggetti, ricevi quest'altro".
// A cosa serve: nella bottega di Ilse "3 Fiori di riva -> 2 Pozioni di Ilse", "1 Fiore di riva + 1 Cardo nero +
// 1 Lunaria -> 1 Elisir del crepuscolo". Ogni fiore, o combinazione di fiori, dà una pozione precisa
// (elenco e numeri in Docs/erbe-e-pozioni.md). La stessa ricetta si può usare in più negozi.
// Pronto per le monete: quando ci saranno, la moneta sarà un oggetto come gli altri e basterà metterla fra gli
// ingredienti ("5 Monete -> 1 Pozione di Ilse") oppure come risultato ("1 Cardo nero -> 2 Monete", per vendere).
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Ricetta; oppure il menu
// "magic-gnl > Erbe: crea fiori, pozioni e ricette", che crea quelle della bottega di Ilse in Assets/Dati/Ricette.
// Poi la si mette nell'elenco "Ricette" di un Negozio.
[CreateAssetMenu(fileName = "nuova-ricetta", menuName = "magic-gnl/Oggetti/Ricetta")]
public class DatiRicetta : ScriptableObject
{
    [System.Serializable]
    public struct Ingrediente
    {
        public DatiOggetto oggetto;
        [Min(1)] public int quanti;
    }

    [Tooltip("Cosa si dà al negoziante (fiori, e in futuro monete).")]
    public Ingrediente[] chiede;
    [Tooltip("Cosa si riceve.")]
    public DatiOggetto da;
    [Min(1)] public int quantiDa = 1;

    // Quante copie di un oggetto ci sono nello zaino.
    public static int Hai(Zaino zaino, DatiOggetto oggetto)
    {
        int n = 0;
        if (zaino == null || oggetto == null) return 0;
        foreach (var o in zaino.Oggetti) if (o == oggetto) n++;
        return n;
    }

    public bool Valida => da != null && chiede != null && chiede.Length > 0;

    // Nello zaino c'è tutto quello che serve?
    public bool Possibile(Zaino zaino)
    {
        if (!Valida) return false;
        foreach (var i in chiede)
            if (i.oggetto != null && Hai(zaino, i.oggetto) < Mathf.Max(1, i.quanti)) return false;
        return true;
    }

    // Toglie gli ingredienti e mette nello zaino il risultato. Falso se manca qualcosa (allora non tocca niente).
    public bool Esegui(Zaino zaino)
    {
        if (!Possibile(zaino)) return false;
        foreach (var i in chiede)
            for (int k = 0; k < Mathf.Max(1, i.quanti) && i.oggetto != null; k++) zaino.Togli(i.oggetto);
        for (int k = 0; k < quantiDa; k++) zaino.Aggiungi(da);
        return true;
    }
}
