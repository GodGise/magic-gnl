using UnityEngine;
using UnityEngine.Events;

// Una missione di raccolta: "porta 3 fiori di riva". Conta quante copie di un oggetto ha il giocatore nello zaino,
// le scrive nell'obiettivo ("Fiori di riva raccolti: 2/3", la chiave di Lingua ha {0}) e, quando sono abbastanza,
// fa partire "Quando Completato" (una volta sola).
// Finché non sono abbastanza, la bussola indica l'oggetto più vicino ancora da raccogliere fra quelli in "Punti"
// (di solito gli OggettoRaccoglibile sparsi nel mondo: quelli raccolti si spengono e non contano più).
// Come montarlo: su un oggetto vuoto della scena. Parte spento: lo si avvia da un evento con RaccoltaOggetti > Avvia
// (per esempio alla fine del dialogo con Ilse).
public class RaccoltaOggetti : MonoBehaviour
{
    [SerializeField] DatiOggetto oggetto;
    [SerializeField, Min(1)] int quanti = 3;
    [Tooltip("Chiave di Lingua dell'obiettivo, con {0} al posto del numero raccolto.")]
    [SerializeField] string chiaveObiettivo;
    [Tooltip("Dove si trovano gli oggetti da raccogliere (la bussola indica il più vicino ancora acceso).")]
    [SerializeField] Transform[] punti;
    [SerializeField] UnityEvent quandoCompletato;

    GiocatoreControllo giocatore;
    bool attivo, finito;
    int ultimo = -1;

    public void Avvia()
    {
        if (attivo || finito) return;
        attivo = true;
        ultimo = -1;
    }

    void Update()
    {
        if (!attivo || finito) return;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;
        int n = Mathf.Min(UsoConsumabili.Quanti(oggetto), quanti);
        if (n != ultimo)
        {
            ultimo = n;
            if (Missioni.Istanza != null && !string.IsNullOrEmpty(chiaveObiettivo)) Missioni.Istanza.ImpostaObiettivoConNumero(chiaveObiettivo, n);
        }
        if (n >= quanti)
        {
            finito = true;
            attivo = false;
            quandoCompletato.Invoke();
            return;
        }
        // la bussola verso il punto più vicino ancora da raccogliere
        if (Missioni.Istanza == null || punti == null) return;
        Transform migliore = null;
        float minimo = float.MaxValue;
        foreach (var p in punti)
        {
            if (p == null || !p.gameObject.activeInHierarchy) continue;
            float d = (p.position - giocatore.transform.position).sqrMagnitude;
            if (d < minimo) { minimo = d; migliore = p; }
        }
        if (migliore != null && Missioni.Istanza.Meta != migliore) Missioni.Istanza.ImpostaMeta(migliore);
    }
}
