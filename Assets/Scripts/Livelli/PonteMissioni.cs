using System.Collections.Generic;
using UnityEngine;

// Il ponte fra una zona in una scena a parte (per esempio la cripta) e le missioni del villaggio.
// A cosa serve: gli oggetti di una scena non possono collegarsi negli eventi a quelli di un'altra scena. Nella zona si
// collegano a questo componente (PonteMissioni > ImpostaObiettivo, Suggerisci, Messaggio, ImpostaMeta, Segnale), che
// passa tutto a Missioni del villaggio. "Segnale" manda una parola (per esempio q5.medaglione) che nel villaggio
// riceve AscoltaSegnale, con i suoi eventi.
// Come montarlo: su un oggetto vuoto della scena della zona (lo mette da solo il comando che crea la zona); poi si
// trascina negli eventi.
// "Finito Se": se quella parola è già stata mandata (per esempio q5.medaglione, la quest della zona è fatta), obiettivi,
// suggerimenti e meta non cambiano più: rientrando nella zona a quest finita non ricompare "Trova la tomba".
public class PonteMissioni : MonoBehaviour
{
    [Tooltip("Se questa parola è già stata mandata, obiettivi, suggerimenti e meta della zona non cambiano più.")]
    [SerializeField] string finitoSe;

    bool Attivo => Missioni.Istanza != null && (string.IsNullOrEmpty(finitoSe) || !SegnaliMissioni.GiaMandato(finitoSe));

    public void ImpostaObiettivo(string chiave) { if (Attivo) Missioni.Istanza.ImpostaObiettivo(chiave); }
    public void Suggerisci(string chiave) { if (Attivo) Missioni.Istanza.Suggerisci(chiave); }
    public void TogliObiettivo() { if (Attivo) Missioni.Istanza.TogliObiettivo(); }
    // La bussola indica questo punto (un oggetto della zona).
    public void ImpostaMeta(Transform punto) { if (Attivo) Missioni.Istanza.ImpostaMeta(punto); }
    // Una scritta in alto per qualche secondo (chiave di Lingua).
    public void Messaggio(string chiave) => MessaggiSchermo.Mostra(Dialogo.SostituisciTasti(Lingua.T(chiave)), 4f);
    public void Segnale(string parola) => SegnaliMissioni.Invia(parola);
}

// Le parole mandate fra le scene (vedi PonteMissioni e AscoltaSegnale). Ricorda anche quelle già mandate, così
// chi si mette in ascolto dopo (per esempio rientrando) sa che è già successo.
public static class SegnaliMissioni
{
    public static event System.Action<string> Arrivato;
    static readonly HashSet<string> mandati = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Azzera() { mandati.Clear(); Arrivato = null; }

    public static void Invia(string parola)
    {
        if (string.IsNullOrEmpty(parola)) return;
        mandati.Add(parola);
        Arrivato?.Invoke(parola);
    }

    public static bool GiaMandato(string parola) => mandati.Contains(parola);
}
