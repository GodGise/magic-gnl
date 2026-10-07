using System.Collections.Generic;
using UnityEngine;

// Gli oggetti che il giocatore porta con sé ma non ha addosso (armi, scudi, armature, amuleti).
// Quello che ha addosso sta in Equipaggiamento; l'inventario (InventarioGioco) sposta gli oggetti fra i due:
// equipaggiando un oggetto, quello che c'era prima nella stessa casella torna nello zaino.
// Gli oggetti si raccolgono nel mondo con OggettoRaccoglibile.
// Come montarlo: sul Giocatore. Se manca si aggiunge da solo la prima volta che serve (Zaino.Di).
// Per provare: trascinare degli oggetti nella lista "Oggetti" qui nell'Inspector, oppure durante il Play
// usare il menu "magic-gnl > Prova: metti tutti gli oggetti nello zaino".
public class Zaino : MonoBehaviour
{
    [Tooltip("Oggetti nello zaino. Quelli messi qui prima del Play sono gli oggetti di partenza.")]
    [SerializeField] List<DatiOggetto> oggetti = new List<DatiOggetto>();

    public IReadOnlyList<DatiOggetto> Oggetti => oggetti;
    public int Numero => oggetti.Count;

    // Avvisa quando lo zaino cambia, per ridisegnare l'inventario.
    public event System.Action Cambiato;

    public void Aggiungi(DatiOggetto oggetto)
    {
        if (oggetto == null) return;
        oggetti.Add(oggetto);
        Cambiato?.Invoke();
    }

    public bool Togli(DatiOggetto oggetto)
    {
        if (oggetto == null || !oggetti.Remove(oggetto)) return false;
        Cambiato?.Invoke();
        return true;
    }

    public bool Contiene(DatiOggetto oggetto) => oggetto != null && oggetti.Contains(oggetto);

    // Lo zaino del giocatore (lo aggiunge se manca).
    public static Zaino Di(Component giocatore)
    {
        if (giocatore == null) return null;
        var zaino = giocatore.GetComponent<Zaino>();
        if (zaino == null) zaino = giocatore.gameObject.AddComponent<Zaino>();
        return zaino;
    }
}
