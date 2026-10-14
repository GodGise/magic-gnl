using UnityEngine;

// Fa esistere un oggetto solo per certe classi: all'avvio della scena, se il giocatore non è di una delle classi
// spuntate, l'oggetto si spegne (con tutto quello che ha dentro).
// A cosa serve: le parti di una quest che cambiano con la classe, per esempio la sala di studio di Edric solo per
// lo Stregone, o i bersagli di tiro solo per il Ladro.
// Come montarlo: sull'oggetto (o sul gruppo) da mostrare solo ad alcune classi; spuntare le classi giuste.
[DefaultExecutionOrder(-50)]
public class SoloPerClasse : MonoBehaviour
{
    [SerializeField] bool guerriero = true;
    [SerializeField] bool ladro = true;
    [SerializeField] bool stregone = true;

    public static bool Vale(bool guerriero, bool ladro, bool stregone)
    {
        switch (SceltaPartita.Classe)
        {
            case ClasseGiocatore.Guerriero: return guerriero;
            case ClasseGiocatore.Ladro: return ladro;
            default: return stregone;
        }
    }

    void Awake()
    {
        if (!Vale(guerriero, ladro, stregone)) gameObject.SetActive(false);
    }
}
