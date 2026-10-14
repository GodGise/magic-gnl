using UnityEngine;
using UnityEngine.Events;

// Riceve una parola mandata da un'altra scena (PonteMissioni > Segnale, per esempio "q5.medaglione" quando si apre il
// baule della cripta) e fa partire "Quando Arriva". Le uscite dalle zone mandano da sole "uscita.<nome scena>"
// (per esempio "uscita.Cripta").
// Come montarlo: su un oggetto vuoto della scena del villaggio; "Parola" = la parola da aspettare. Parte una volta
// sola. Se è spento non ascolta: si accende da un evento quando serve (per esempio dopo il baule, per aspettare
// l'uscita dalla cripta).
public class AscoltaSegnale : MonoBehaviour
{
    [SerializeField] string parola;
    [SerializeField] UnityEvent quandoArriva;

    bool fatto;

    void OnEnable() => SegnaliMissioni.Arrivato += Arrivato;
    void OnDisable() => SegnaliMissioni.Arrivato -= Arrivato;

    void Arrivato(string arrivata)
    {
        if (fatto || arrivata != parola) return;
        fatto = true;
        quandoArriva.Invoke();
    }
}
