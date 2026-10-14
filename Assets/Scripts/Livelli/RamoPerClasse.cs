using UnityEngine;
using UnityEngine.Events;

// Un evento che fa cose diverse secondo la classe del giocatore.
// A cosa serve: nelle quest, quando la stessa azione deve portare in posti diversi (per esempio Brannoc dà la lama
// al Guerriero e al Ladro, ma manda lo Stregone al tempio da Edric).
// Come montarlo: su un oggetto qualsiasi; riempire i tre eventi. Da un altro evento (per esempio "Al Termine" di un
// Interlocutore) si sceglie RamoPerClasse > Esegui.
public class RamoPerClasse : MonoBehaviour
{
    [SerializeField] UnityEvent perGuerriero;
    [SerializeField] UnityEvent perLadro;
    [SerializeField] UnityEvent perStregone;

    public void Esegui()
    {
        switch (SceltaPartita.Classe)
        {
            case ClasseGiocatore.Guerriero: perGuerriero.Invoke(); break;
            case ClasseGiocatore.Ladro: perLadro.Invoke(); break;
            default: perStregone.Invoke(); break;
        }
    }
}
