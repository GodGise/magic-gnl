using System.Collections.Generic;
using UnityEngine;

// Zona di acqua bassa: chi ci cammina dentro va più piano.
// A cosa serve: nel Lago Nero, dove l'acqua arriva alle ginocchia, il giocatore rallenta (vedi GiocatoreControllo).
// Il suono dei passi nell'acqua lo dà la Superficie Sonora di tipo Acqua messa sullo stesso oggetto.
// Come montarlo: su un Cube senza Mesh Renderer, con il Box Collider in "Is Trigger", grande quanto la zona
// d'acqua (dal fondale fino a poco sotto la superficie). Va aggiunta anche una Superficie Sonora con Tipo = Acqua.
// Il menu "magic-gnl > Crea Villaggio Lago Nero" mette già queste zone lungo tutta la riva.
[RequireComponent(typeof(BoxCollider))]
public class AcquaBassa : MonoBehaviour
{
    [Tooltip("Velocità nell'acqua rispetto a quella normale (0,6 = 60%).")]
    [Range(0.1f, 1f)] [SerializeField] float velocitaInAcqua = 0.6f;

    static readonly List<AcquaBassa> zone = new List<AcquaBassa>();
    BoxCollider area;

    void Awake()
    {
        area = GetComponent<BoxCollider>();
        area.isTrigger = true;
    }

    void OnEnable() => zone.Add(this);
    void OnDisable() => zone.Remove(this);

    // Velocità da usare per chi ha i piedi in quel punto: 1 fuori dall'acqua, meno di 1 dentro.
    public static float FattoreVelocita(Vector3 piedi)
    {
        float fattore = 1f;
        foreach (AcquaBassa zona in zone)
        {
            if (zona.area != null && zona.area.bounds.Contains(piedi)) fattore = Mathf.Min(fattore, zona.velocitaInAcqua);
        }
        return fattore;
    }

    // Riquadro azzurro nella vista Scene, per vedere dove sono le zone d'acqua.
    void OnDrawGizmos()
    {
        BoxCollider zona = GetComponent<BoxCollider>();
        if (zona == null) return;
        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(zona.center, zona.size);
    }
}
