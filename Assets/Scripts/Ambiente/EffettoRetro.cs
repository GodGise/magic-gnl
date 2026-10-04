using UnityEngine;
using UnityEngine.InputSystem;

// Effetto retro in stile PS2: il gioco viene disegnato a bassa risoluzione e poi ingrandito
// senza sfumare i pixel, così l'immagine diventa ruvida come nei giochi di quegli anni.
// Le scritte del pannello di prova restano nitide perché vengono disegnate dopo.
// Per confrontare: F2 accende e spegne l'effetto mentre giochi.
// Come montarlo: sulla Main Camera (funziona con la pipeline grafica di base di Unity).
[RequireComponent(typeof(Camera))]
public class EffettoRetro : MonoBehaviour
{
    [Tooltip("Righe di pixel in verticale. 480 è vicino alla PS2; meno righe = più pixellato.")]
    [SerializeField] int righeVerticali = 360;
    public bool attivo = true;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame) attivo = !attivo;
    }

    void OnRenderImage(RenderTexture sorgente, RenderTexture destinazione)
    {
        if (!attivo || righeVerticali <= 0 || sorgente.height <= righeVerticali)
        {
            Graphics.Blit(sorgente, destinazione);
            return;
        }

        int altezza = righeVerticali;
        int larghezza = Mathf.Max(1, Mathf.RoundToInt(altezza * (float)sorgente.width / sorgente.height));

        RenderTexture piccola = RenderTexture.GetTemporary(larghezza, altezza, 0, sorgente.format);
        piccola.filterMode = FilterMode.Point;
        Graphics.Blit(sorgente, piccola);
        Graphics.Blit(piccola, destinazione);
        RenderTexture.ReleaseTemporary(piccola);
    }
}
