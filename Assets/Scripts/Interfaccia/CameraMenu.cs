using UnityEngine;

// Movimento lento e leggero della camera dietro al menu iniziale, come se qualcuno respirasse
// guardando il lago di notte. Non risponde ai comandi: è solo atmosfera.
// Come montarlo: sulla Main Camera della scena Menu (il menu "magic-gnl > Crea scena menu" lo fa da solo).
public class CameraMenu : MonoBehaviour
{
    [Tooltip("Punto che la camera guarda.")]
    [SerializeField] Vector3 puntoGuardato = new Vector3(0f, 2f, 30f);
    [Tooltip("Quanto si sposta la camera, in metri.")]
    [SerializeField] float ampiezza = 0.35f;
    [Tooltip("Velocità dell'ondeggiamento (più basso = più lento).")]
    [SerializeField] float velocita = 0.08f;

    Vector3 partenza;

    void Start()
    {
        partenza = transform.position;
    }

    void Update()
    {
        float t = Time.time * velocita;
        Vector3 scarto = new Vector3(
            (Mathf.PerlinNoise(t, 0.3f) - 0.5f) * 2f * ampiezza,
            (Mathf.PerlinNoise(0.7f, t) - 0.5f) * ampiezza,
            0f);
        transform.position = partenza + scarto;
        transform.rotation = Quaternion.LookRotation(puntoGuardato - transform.position);
    }
}
