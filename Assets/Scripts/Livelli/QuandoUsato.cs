using UnityEngine;
using UnityEngine.Events;

// Aspetta che il giocatore usi un oggetto consumabile (per esempio la Pozione di Ilse, tasto "Usa oggetto") e poi
// fa partire "Quando Usato", una volta sola. "Oggetto" vuoto = va bene qualsiasi consumabile.
// Come montarlo: su un oggetto vuoto della scena. Parte spento: lo si avvia da un evento con QuandoUsato > Avvia
// (per esempio dopo che Ilse ha dato le pozioni); gli oggetti usati prima non contano.
public class QuandoUsato : MonoBehaviour
{
    [SerializeField] DatiConsumabile oggetto;
    [SerializeField] UnityEvent quandoUsato;

    bool attivo, finito;

    public void Avvia()
    {
        if (attivo || finito) return;
        attivo = true;
    }

    void OnEnable() => UsoConsumabili.Usato += Usato;
    void OnDisable() => UsoConsumabili.Usato -= Usato;

    void Usato(DatiConsumabile usato)
    {
        if (!attivo || finito || (oggetto != null && usato != oggetto)) return;
        finito = true;
        attivo = false;
        quandoUsato.Invoke();
    }
}
