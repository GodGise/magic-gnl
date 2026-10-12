using UnityEngine;

// Un oggetto che si usa e sparisce (per ora le pozioni curative, per esempio la Pozione di Ilse della quest 4).
// Come si usa in partita: sta nello zaino; il tasto "Usa oggetto" (F all'inizio, si cambia in Opzioni > Comandi)
// usa quello scelto. Nell'inventario (scheda Varie) E o doppio clic sceglie quale usare con F (UsoConsumabili).
// Più copie dello stesso oggetto = più voci uguali nello zaino.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Consumabile, poi nome e descrizione
// come chiavi di Lingua (per esempio oggetto.pozione_di_ilse.nome) e la spunta "Tutte Le Classi".
[CreateAssetMenu(fileName = "nuovo-consumabile", menuName = "magic-gnl/Oggetti/Consumabile")]
public class DatiConsumabile : DatiOggetto
{
    [Header("Effetto")]
    [Tooltip("Vita recuperata.")]
    [Min(0f)] public float cura = 60f;
    [Tooltip("In quanti secondi arriva tutta la cura (0 = subito).")]
    [Min(0f)] public float durataCura = 1f;
    [Tooltip("Secondi prima di poter usare un altro oggetto.")]
    [Min(0f)] public float attesa = 1.2f;
    [Tooltip("Colore della forma provvisoria (la boccetta) finché Nazar non fa il modello.")]
    public Color colore = new Color(0.75f, 0.15f, 0.12f);
}
