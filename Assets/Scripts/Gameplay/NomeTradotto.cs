using UnityEngine;

// Il nome di un nemico (o di un manichino) nelle 8 lingue, per la barra in basso quando lo si aggancia.
// Senza questo componente la barra mostra il nome dell'oggetto nella scena (solo in italiano).
// Come montarlo: sul nemico, scrivendo in "Chiave" la chiave di Lingua del nome (per esempio nem.ratto).
public class NomeTradotto : MonoBehaviour
{
    [SerializeField] string chiave;

    public string Nome => string.IsNullOrEmpty(chiave) ? name : Lingua.T(chiave);

    public static string Di(Component c)
    {
        var n = c != null ? c.GetComponent<NomeTradotto>() : null;
        return n != null ? n.Nome : c != null ? c.gameObject.name.Replace("(Clone)", "").Trim() : "";
    }
}
