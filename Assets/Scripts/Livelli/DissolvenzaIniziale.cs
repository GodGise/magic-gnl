using UnityEngine;

// All'inizio della scena lo schermo parte nero e si schiarisce piano (il risveglio in casa).
// A cosa serve: il passaggio dal menu (che finisce sul nero) al mondo, senza uno stacco brusco.
// Come montarlo: su un oggetto vuoto della scena; "Secondi Nero" quanto resta tutto nero, "Secondi Schiarita" quanto
// dura la schiarita. Lo crea il menu "magic-gnl > Prologo: prepara la quest 1".
public class DissolvenzaIniziale : MonoBehaviour
{
    [SerializeField, Min(0f)] float secondiNero = 1f;
    [SerializeField, Min(0.1f)] float secondiSchiarita = 3f;

    float inizio;

    void Start() => inizio = Time.time;

    void OnGUI()
    {
        float t = (Time.time - inizio - secondiNero) / secondiSchiarita;
        float nero = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
        if (nero <= 0f) { enabled = false; return; }
        GUI.depth = -100;   // sopra a tutto, anche all'interfaccia
        GUI.color = new Color(0f, 0f, 0f, nero);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
