using UnityEngine;

// Scritte temporanee in alto al centro dello schermo ("Hai raccolto: ...", "Mana insufficiente"), nello stile dei menu.
// Il testo va passato già tradotto (Lingua.T).
// A cosa serve: dare al giocatore un riscontro veloce quando raccoglie o trova qualcosa.
// Come si usa (dagli altri script): MessaggiSchermo.Mostra("Hai raccolto: Chiave", 3f);
// Come montarlo: non serve montarlo. L'oggetto che disegna la scritta si crea da solo durante il Play.
public class MessaggiSchermo : MonoBehaviour
{
    static MessaggiSchermo istanza;

    string testo = "";
    float finoA;

    public static void Mostra(string messaggio, float secondi = 3f)
    {
        if (istanza == null) istanza = new GameObject("Messaggi a schermo").AddComponent<MessaggiSchermo>();
        istanza.testo = messaggio;
        istanza.finoA = Time.time + secondi;
    }

    void OnGUI()
    {
        if (Time.time > finoA || string.IsNullOrEmpty(testo)) return;
        if (MenuPausa.InPausa || InventarioGioco.Aperto) return;

        // Sparisce piano nell'ultimo mezzo secondo. Stesso stile dei menu (riquadro scuro con cornice di bronzo).
        float trasparenza = Mathf.Clamp01((finoA - Time.time) / 0.5f);
        GUI.depth = 5;
        GraficaMenu.PreparaStili();
        float larghezza = GraficaMenu.FoglioIntero();
        var stile = new GUIStyle(GraficaMenu.TestoSinistra) { alignment = TextAnchor.MiddleCenter, wordWrap = false };
        float w = Mathf.Clamp(stile.CalcSize(new GUIContent(testo)).x + 90f, 420f, 1100f);
        var r = new Rect(larghezza * 0.5f - w * 0.5f, 170f, w, 60f);
        GraficaMenu.Cornice(r, 0.95f * trasparenza, false);
        GraficaMenu.Scritta(r, testo, stile, GraficaMenu.Testo, trasparenza);
    }
}
