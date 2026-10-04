using UnityEngine;

// Torcia: luce calda che tremola come una fiamma.
// A cosa serve: di notte illumina strade e piazze; di giorno resta accesa ma debole.
// Legge il valore "Buio" del CicloGiornoNotte; se nella scena non c'è, resta sempre al massimo.
// Come montarlo: su un oggetto con una luce di tipo Point (la richiede da solo).
// Il menu "magic-gnl > Crea zona di prova" mette già diverse torce.
[RequireComponent(typeof(Light))]
public class Torcia : MonoBehaviour
{
    [SerializeField] float intensitaMassima = 1.8f;
    [Tooltip("Quanto varia la luce quando tremola (0 = fissa).")]
    [Range(0f, 0.5f)] [SerializeField] float tremolio = 0.25f;
    [SerializeField] float velocitaTremolio = 8f;
    [Tooltip("Intensità di giorno rispetto alla notte.")]
    [Range(0f, 1f)] [SerializeField] float quotaDiGiorno = 0.2f;

    Light luce;
    float seme;

    void Awake()
    {
        luce = GetComponent<Light>();
        seme = Random.value * 100f;
    }

    void Update()
    {
        float buio = CicloGiornoNotte.Istanza != null ? CicloGiornoNotte.Istanza.Buio : 1f;
        float fiamma = 1f - tremolio + tremolio * 2f * Mathf.PerlinNoise(seme, Time.time * velocitaTremolio);
        luce.intensity = intensitaMassima * Mathf.Lerp(quotaDiGiorno, 1f, buio) * fiamma;
    }
}
