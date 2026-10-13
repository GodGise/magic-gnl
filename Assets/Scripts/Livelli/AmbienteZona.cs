using UnityEngine;

// La luce di una zona chiusa (per esempio la cripta): luce d'ambiente e nebbia mentre il giocatore è dentro.
// A cosa serve: nelle zone sotto terra non ci sono sole, luna e cielo; CaricamentoZona legge questi colori entrando.
// Come montarlo: su un oggetto vuoto della scena della zona (uno solo). Il comando "magic-gnl > Cripta: crea la scena
// della cripta" lo mette da solo.
public class AmbienteZona : MonoBehaviour
{
    [Tooltip("Luce d'ambiente (quanto si vede anche lontano dalle torce).")]
    public Color luceAmbiente = new Color(0.1f, 0.1f, 0.13f);
    [Tooltip("Colore della nebbia e del fondo.")]
    public Color nebbia = new Color(0.01f, 0.01f, 0.02f);
    [Range(0f, 0.2f)] public float densitaNebbia = 0.06f;
}
