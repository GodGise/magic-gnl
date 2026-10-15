using System.Collections.Generic;
using UnityEngine;

// Un personaggio dà degli oggetti al giocatore (Brannoc la lama, Edric il bastone e il libro).
// A cosa serve: gli oggetti vanno subito addosso (al posto di quelli che c'erano, che tornano nello zaino) e in alto
// compare "Raccolto: ..." per ognuno. Gli incantesimi vanno nelle caselle libere.
// Ogni classe riceve la sua lista: "Per Guerriero", "Per Ladro", "Per Stregone" (si trascinano i file degli
// oggetti, per esempio da Assets/Dati/Oggetti/Guerriero/Armi).
// Come montarlo: su un oggetto qualsiasi (di solito il personaggio che dona). Da un evento si sceglie
// DonaOggetti > Dona. Dona una volta sola.
// Su un Baule (stesso oggetto): la lista della classe finisce dentro il baule e si prende dalla finestra del baule.
public class DonaOggetti : MonoBehaviour
{
    [SerializeField] DatiOggetto[] perGuerriero;
    [SerializeField] DatiOggetto[] perLadro;
    [SerializeField] DatiOggetto[] perStregone;
    [Tooltip("Secondi a schermo della scritta per ogni oggetto ricevuto.")]
    [SerializeField] float secondiScritta = 2.5f;

    bool donato;

    // La lista della classe del giocatore, senza donarla: la usa il Baule sullo stesso oggetto, che la mette dentro
    // il baule (si prende trascinandola nello zaino). Dopo, Dona non fa più niente.
    public DatiOggetto[] PrendiLista()
    {
        donato = true;
        DatiOggetto[] lista = SceltaPartita.Classe == ClasseGiocatore.Guerriero ? perGuerriero
            : SceltaPartita.Classe == ClasseGiocatore.Ladro ? perLadro : perStregone;
        return lista ?? new DatiOggetto[0];
    }

    public void Dona()
    {
        if (donato) return;
        var giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;
        var equip = giocatore.GetComponent<Equipaggiamento>();
        if (equip == null) return;
        donato = true;

        DatiOggetto[] lista = SceltaPartita.Classe == ClasseGiocatore.Guerriero ? perGuerriero
            : SceltaPartita.Classe == ClasseGiocatore.Ladro ? perLadro : perStregone;
        if (lista == null) return;
        var zaino = Zaino.Di(giocatore);
        var nomi = new List<string>();
        foreach (var oggetto in lista)
        {
            if (oggetto == null) continue;
            DatiOggetto vecchio = Vecchio(equip, oggetto);
            if (zaino != null && zaino.Contiene(oggetto)) zaino.Togli(oggetto);
            equip.Equipaggia(oggetto);
            if (vecchio != null && vecchio != oggetto && zaino != null && !zaino.Contiene(vecchio)) zaino.Aggiungi(vecchio);
            nomi.Add(oggetto.Nome);
        }
        if (nomi.Count > 0)
            MessaggiSchermo.Mostra(Lingua.T("hud.raccolto") + ": " + string.Join(", ", nomi), secondiScritta + nomi.Count);
    }

    // Quello che c'era addosso nella stessa casella (per rimetterlo nello zaino).
    static DatiOggetto Vecchio(Equipaggiamento e, DatiOggetto nuovo)
    {
        switch (nuovo)
        {
            case DatiArma _: return e.Arma;
            case DatiScudo _: return e.Scudo;
            case DatiArmatura _: return e.Armatura;
            case DatiAmuleto _: return e.Amuleto;
            case DatiArmaDistanza _: return e.ArmaDistanza;
            case DatiBastone _: return e.Bastone;
            case DatiLibro _: return e.Libro;
            default: return null;
        }
    }
}
