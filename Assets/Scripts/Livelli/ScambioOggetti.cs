using UnityEngine;

// Un personaggio prende degli oggetti dallo zaino del giocatore e ne dà altri in cambio (Ilse prende i tre fiori
// di riva e dà le pozioni). Gli oggetti dati vanno nello zaino; in alto compare "Raccolto: Pozione di Ilse x2".
// Il primo consumabile ricevuto diventa quello pronto per il tasto "Usa oggetto" (UsoConsumabili).
// Come montarlo: su un oggetto qualsiasi (di solito il personaggio). "Prende": oggetto e quante copie (se nello
// zaino ce ne sono meno, prende quelle che ci sono). "Dà": gli oggetti da dare, una voce per copia (due pozioni =
// due voci). Da un evento si sceglie ScambioOggetti > Scambia. Scambia una volta sola.
public class ScambioOggetti : MonoBehaviour
{
    [SerializeField] DatiOggetto prende;
    [SerializeField, Min(0)] int quantiPrende = 1;
    [SerializeField] DatiOggetto[] da;
    [Tooltip("Secondi a schermo della scritta degli oggetti ricevuti.")]
    [SerializeField] float secondiScritta = 3.5f;

    bool fatto;

    public void Scambia()
    {
        if (fatto) return;
        var giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;
        fatto = true;
        var zaino = Zaino.Di(giocatore);
        for (int i = 0; i < quantiPrende && prende != null; i++)
            if (!zaino.Togli(prende)) break;

        if (da == null || da.Length == 0) return;
        DatiConsumabile primo = null;
        var conti = new System.Collections.Generic.Dictionary<DatiOggetto, int>();
        var ordine = new System.Collections.Generic.List<DatiOggetto>();
        foreach (var o in da)
        {
            if (o == null) continue;
            zaino.Aggiungi(o);
            if (primo == null && o is DatiConsumabile c) primo = c;
            if (!conti.ContainsKey(o)) { conti[o] = 0; ordine.Add(o); }
            conti[o]++;
        }
        if (primo != null) UsoConsumabili.Seleziona(primo);
        var nomi = new System.Collections.Generic.List<string>();
        foreach (var o in ordine) nomi.Add(conti[o] > 1 ? o.Nome + " x" + conti[o] : o.Nome);
        if (nomi.Count > 0)
        {
            MessaggiSchermo.Mostra(Lingua.T("hud.raccolto") + ": " + string.Join(", ", nomi), secondiScritta);
            Suoni.Suona(Suono.Raccolta, giocatore.transform.position + Vector3.up, 0.8f);
        }
    }
}
