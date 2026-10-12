using System.Collections.Generic;
using UnityEngine;

// Una zona (per esempio l'interno di una casa o un capanno) dove la camera sta più vicina al personaggio e non sale
// troppo in alto: nelle stanze piccole la camera normale (lontana 6 m) finirebbe schiacciata contro soffitto e muri
// e guarderebbe dall'alto. Quando il giocatore è dentro la zona, la camera si avvicina piano; uscendo torna normale.
// Come montarlo: su un oggetto vuoto al centro della stanza; "Dimensioni" è la grandezza della zona in metri (nella
// scena si vede un riquadro azzurro quando l'oggetto è selezionato; la zona gira con l'oggetto).
// "Distanza Massima" e "Inclinazione Massima" sono i limiti della camera dentro la zona.
// La legge CameraTerzaPersona; non serve collegare niente.
public class ZonaCamera : MonoBehaviour
{
    [SerializeField] Vector3 dimensioni = new Vector3(8f, 4f, 7f);
    [Tooltip("Distanza massima della camera dal personaggio dentro la zona, in metri.")]
    [SerializeField, Min(1f)] float distanzaMassima = 3.2f;
    [Tooltip("Quanto può guardare dall'alto la camera dentro la zona, in gradi.")]
    [SerializeField, Range(0f, 80f)] float inclinazioneMassima = 30f;

    static readonly List<ZonaCamera> zone = new List<ZonaCamera>();

    void OnEnable() => zone.Add(this);
    void OnDisable() => zone.Remove(this);

    bool Contiene(Vector3 punto)
    {
        Vector3 locale = Quaternion.Inverse(transform.rotation) * (punto - transform.position);
        Vector3 meta = dimensioni * 0.5f;
        return Mathf.Abs(locale.x) <= meta.x && Mathf.Abs(locale.z) <= meta.z && locale.y >= -0.5f && locale.y <= dimensioni.y;
    }

    // Abbassa distanza e inclinazione massima se il punto (il personaggio) è dentro una zona.
    public static bool Limiti(Vector3 punto, ref float distanza, ref float inclinazione)
    {
        bool dentro = false;
        foreach (var z in zone)
        {
            if (z == null || !z.Contiene(punto)) continue;
            distanza = Mathf.Min(distanza, z.distanzaMassima);
            inclinazione = Mathf.Min(inclinazione, z.inclinazioneMassima);
            dentro = true;
        }
        return dentro;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.75f, 1f, 0.8f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.up * (dimensioni.y * 0.5f), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, dimensioni);
    }
}
