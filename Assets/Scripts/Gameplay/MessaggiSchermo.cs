using UnityEngine;

// Scritte temporanee in alto al centro dello schermo ("Hai raccolto: ...", "Mana insufficiente").
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

        // Sparisce piano nell'ultimo mezzo secondo.
        float trasparenza = Mathf.Clamp01((finoA - Time.time) / 0.5f);
        var stile = new GUIStyle(GUI.skin.box) { fontSize = 22, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        Color prima = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, trasparenza);
        GUI.Box(new Rect(Screen.width * 0.5f - 260f, Screen.height * 0.18f, 520f, 50f), testo, stile);
        GUI.color = prima;
    }
}
