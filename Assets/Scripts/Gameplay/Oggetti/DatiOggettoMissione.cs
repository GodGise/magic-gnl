using UnityEngine;

// Un oggetto che serve a una missione (per esempio i Fiori di riva della quest 4, da portare a Ilse).
// Non si equipaggia e non si usa: sta nello zaino (scheda Varie) finché qualcuno non lo prende (ScambioOggetti)
// o una missione non lo conta (RaccoltaOggetti).
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Oggetto della missione, poi nome e
// descrizione come chiavi di Lingua e la spunta "Tutte Le Classi".
[CreateAssetMenu(fileName = "nuovo-oggetto-missione", menuName = "magic-gnl/Oggetti/Oggetto della missione")]
public class DatiOggettoMissione : DatiOggetto
{
    [Tooltip("Colore della forma provvisoria finché Nazar non fa il modello.")]
    public Color colore = new Color(0.55f, 0.8f, 1f);
    [Tooltip("Forma provvisoria: Fiore (stelo e petali che brillano) oppure Sacchetto.")]
    public FormaProvvisoria forma = FormaProvvisoria.Fiore;

    public enum FormaProvvisoria { Fiore, Sacchetto }
}
