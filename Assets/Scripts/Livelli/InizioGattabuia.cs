using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Il risveglio nella gattabuia, dopo lo scontro perso con l'orco enorme (Docs/gattabuia.md).
// Cosa fa all'avvio della scena: segna il prologo finito per il personaggio (da qui si apre il co-op:
// Salvataggio.SegnaPrologoFinito), schermo nero che si schiarisce piano con la scritta del risveglio, poi "All'Avvio"
// (per esempio il primo obiettivo "Forza la sbarra della cella").
// Come montarlo: su un oggetto vuoto della scena Gattabuia (uno solo). Lo mette il comando "magic-gnl > Gattabuia:
// crea la scena della gattabuia".
public class InizioGattabuia : MonoBehaviour
{
    [SerializeField, Min(0f)] float secondiNero = 3f;
    [SerializeField] UnityEvent allAvvio;

    float nero = 1f;
    string scritta;

    IEnumerator Start()
    {
        Salvataggio.SegnaPrologoFinito();
        scritta = Lingua.T("msg.gb.risveglio");
        Suoni.SuonaOvunque(Suono.Rombo, 0.3f);
        yield return new WaitForSeconds(secondiNero);
        scritta = null;
        for (float t = 0f; t < 2f; t += Time.deltaTime) { nero = 1f - t / 2f; yield return null; }
        nero = 0f;
        allAvvio.Invoke();
    }

    void OnGUI()
    {
        if (nero <= 0.001f) return;
        GUI.depth = -80;
        GUI.color = new Color(0f, 0f, 0f, nero);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        if (string.IsNullOrEmpty(scritta)) return;
        GraficaMenu.PreparaStili();
        float L = GraficaMenu.FoglioIntero();
        GraficaMenu.Scritta(new Rect(0f, 500f, L, 80f), scritta, GraficaMenu.Voce, GraficaMenu.Spento, 1f);
    }
}
