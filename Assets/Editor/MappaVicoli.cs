using UnityEditor;
using UnityEngine;

// Finestra dell'editor che mostra la mappa vista dall'alto fatta dal comando "Villaggio: chiudi i vicoli ciechi"
// (ChiudiVicoliCiechi.cs): nord in alto, un pixel = 20 cm. Si apre da sola alla fine del comando, oppure dal menu
// "magic-gnl > Villaggio: mostra la mappa dei vicoli" (legge Temp/vicoli-villaggio.png).
// Rotellina del mouse = zoom, trascinare con il tasto sinistro = spostarsi.
// Non va montato su niente: è solo una finestra dell'editor.
public class MappaVicoli : EditorWindow
{
    Texture2D mappa;
    float zoom = 1f;
    Vector2 spostamento;

    public static void Mostra(Texture2D tex)
    {
        var w = GetWindow<MappaVicoli>("Mappa dei vicoli");
        if (w.mappa != null && w.mappa != tex) DestroyImmediate(w.mappa);
        w.mappa = tex;
        w.zoom = 1f;
        w.spostamento = Vector2.zero;
        w.Repaint();
    }

    [MenuItem("magic-gnl/Villaggio: mostra la mappa dei vicoli")]
    static void Apri()
    {
        string percorso = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, "Temp", "vicoli-villaggio.png");
        if (!System.IO.File.Exists(percorso))
        {
            Debug.LogWarning("Mappa dei vicoli: non c'è ancora. Usa prima \"magic-gnl > Villaggio: chiudi i vicoli ciechi\".");
            return;
        }
        var tex = new Texture2D(2, 2) { filterMode = FilterMode.Point };
        tex.LoadImage(System.IO.File.ReadAllBytes(percorso));
        tex.filterMode = FilterMode.Point;
        Mostra(tex);
    }

    void OnGUI()
    {
        if (mappa == null) { GUILayout.Label("Nessuna mappa: usa \"magic-gnl > Villaggio: chiudi i vicoli ciechi\"."); return; }
        const float Legenda = 22f;
        var area = new Rect(0f, Legenda, position.width, position.height - Legenda);

        var e = Event.current;
        if (e.type == EventType.ScrollWheel && area.Contains(e.mousePosition))
        {
            float prima = zoom;
            zoom = Mathf.Clamp(zoom * (e.delta.y < 0 ? 1.25f : 0.8f), 1f, 16f);
            // zoom verso il punto sotto il mouse
            Vector2 m = e.mousePosition - area.position;
            spostamento = m - (m - spostamento) * (zoom / prima);
            e.Use();
        }
        if (e.type == EventType.MouseDrag && e.button == 0 && area.Contains(e.mousePosition))
        {
            spostamento += e.delta;
            e.Use();
        }

        float scala = Mathf.Min(area.width / mappa.width, area.height / mappa.height) * zoom;
        var disegno = new Rect(area.x + spostamento.x, area.y + spostamento.y, mappa.width * scala, mappa.height * scala);
        GUI.BeginClip(area);
        GUI.DrawTexture(new Rect(disegno.x - area.x, disegno.y - area.y, disegno.width, disegno.height), mappa, ScaleMode.StretchToFill);
        GUI.EndClip();

        GUI.Label(new Rect(4f, 2f, position.width - 8f, Legenda - 2f),
            "Bianco: aperto   Giallo: passaggi stretti che portano da qualche parte   Viola: staccionate   Rosso: vicoli ciechi rimasti   "
            + "Azzurro: lasciati aperti per una quest   Grigio: costruzioni   Verde: alberi   Blu: confini invisibili   Nero: dove non si arriva");
    }
}
