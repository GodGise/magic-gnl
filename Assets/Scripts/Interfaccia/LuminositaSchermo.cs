using UnityEngine;

// Luminosità dello schermo, scelta in Opzioni > Video (Impostazioni.Luminosita).
// A cosa serve: con la nebbia e la notte, a qualcuno il gioco sembra troppo scuro o troppo chiaro.
// Il valore di mezzo (50%) non cambia niente; sotto il 50% l'immagine si scurisce, sopra si schiarisce.
// Come funziona: un velo trasparente disegnato sopra l'immagine del gioco ma SOTTO tutte le scritte e i menu
// (quindi interfaccia e testi restano com'erano, cambia solo la scena 3D). Nero per scurire, bianco per schiarire.
// Come montarlo: non serve, si crea da solo all'avvio del gioco e resta fra una scena e l'altra.
public class LuminositaSchermo : MonoBehaviour
{
    // Forza massima del velo: abbastanza da sentire la differenza senza rovinare l'immagine.
    const float MassimoScuro = 0.6f, MassimoChiaro = 0.22f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreaAllAvvio()
    {
        if (FindFirstObjectByType<LuminositaSchermo>() != null) return;
        var oggetto = new GameObject("Luminosita schermo");
        DontDestroyOnLoad(oggetto);
        oggetto.AddComponent<LuminositaSchermo>();
    }

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        float v = Impostazioni.Luminosita;
        if (Mathf.Abs(v - 0.5f) < 0.01f) return;

        GUI.depth = 10000;   // dietro a tutto il resto: l'interfaccia non viene toccata
        GUI.matrix = Matrix4x4.identity;
        Color colore = v < 0.5f
            ? new Color(0f, 0f, 0f, (0.5f - v) / 0.5f * MassimoScuro)
            : new Color(1f, 1f, 1f, (v - 0.5f) / 0.5f * MassimoChiaro);
        GraficaMenu.Riempi(new Rect(0, 0, Screen.width, Screen.height), colore);
    }
}
