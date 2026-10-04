using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

// Ciclo del giorno e della notte, con l'atmosfera cupa del gioco.
// A cosa serve: fa girare sole e luna, e in base all'ora cambia luce, colore dell'ambiente,
// nebbia e cielo. Di notte il mondo diventa blu scuro e nebbioso; di giorno resta grigio e smorto.
// Le torce (script Torcia) leggono il valore "Buio" per accendersi di più quando fa notte.
// Un giorno completo dura 45 minuti reali: 17,5 di giorno e 27,5 di notte.
// Per le prove: tieni premuto T per far scorrere il tempo molto più veloce.
// In alto a destra compare l'ora del gioco.
// Come montarlo: su un oggetto vuoto della scena (per esempio "Cielo"), trascinando nei campi
// Sole e Luna due luci di tipo Directional. Il menu "magic-gnl > Crea zona di prova" lo prepara da solo.
public class CicloGiornoNotte : MonoBehaviour
{
    public static CicloGiornoNotte Istanza { get; private set; }

    public Light sole;
    public Light luna;

    [Tooltip("Ora del gioco, da 0 a 24. Il valore impostato qui è l'ora di partenza.")]
    [Range(0f, 24f)] public float ora = 21f;
    [Tooltip("Minuti reali per il giorno (dalle 6 alle 18 del gioco).")]
    [SerializeField] float minutiDiGiorno = 17.5f;
    [Tooltip("Minuti reali per la notte (dalle 18 alle 6 del gioco). Più lunga del giorno per l'atmosfera.")]
    [SerializeField] float minutiDiNotte = 27.5f;
    [Tooltip("Quante volte più veloce scorre il tempo tenendo premuto T.")]
    [SerializeField] float accelerazioneProva = 40f;
    [Tooltip("Direzione da cui sorge il sole, in gradi.")]
    [SerializeField] float orientamento = 170f;

    [Header("Notte")]
    [SerializeField] Color ambienteNotte = new Color(0.06f, 0.08f, 0.17f);
    [SerializeField] Color nebbiaNotte = new Color(0.04f, 0.06f, 0.13f);
    [SerializeField] float densitaNebbiaNotte = 0.035f;
    [SerializeField] Color coloreLuna = new Color(0.55f, 0.65f, 1f);
    [SerializeField] float intensitaLuna = 0.35f;

    [Header("Giorno")]
    [SerializeField] Color ambienteGiorno = new Color(0.42f, 0.43f, 0.47f);
    [SerializeField] Color nebbiaGiorno = new Color(0.52f, 0.55f, 0.6f);
    [SerializeField] float densitaNebbiaGiorno = 0.012f;
    [SerializeField] Color coloreSole = new Color(1f, 0.95f, 0.85f);
    [SerializeField] float intensitaSole = 1.1f;

    [Header("Alba e tramonto")]
    [SerializeField] Color coloreSoleAlba = new Color(1f, 0.55f, 0.3f);

    // 0 = giorno pieno, 1 = notte fonda.
    public float Buio { get; private set; }

    Material cielo;

    void Awake()
    {
        Istanza = this;
    }

    void OnDestroy()
    {
        if (Istanza == this) Istanza = null;
        if (cielo != null) Destroy(cielo);
    }

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.ambientMode = AmbientMode.Flat;
        if (sole != null) RenderSettings.sun = sole;
        if (luna != null) luna.color = coloreLuna;

        // Copia del materiale del cielo, così le modifiche non toccano il file originale.
        if (RenderSettings.skybox != null)
        {
            cielo = new Material(RenderSettings.skybox);
            RenderSettings.skybox = cielo;
        }

        Aggiorna();
    }

    void Update()
    {
        // 12 ore di gioco di giorno e 12 di notte, ma con durate reali diverse.
        bool eGiorno = ora >= 6f && ora < 18f;
        float minutiFase = eGiorno ? minutiDiGiorno : minutiDiNotte;
        float oreAlSecondo = 12f / Mathf.Max(0.1f, minutiFase * 60f);
        if (Keyboard.current != null && Keyboard.current.tKey.isPressed) oreAlSecondo *= accelerazioneProva;
        ora = Mathf.Repeat(ora + oreAlSecondo * Time.deltaTime, 24f);
        Aggiorna();
    }

    void Aggiorna()
    {
        // Alle 6 il sole è all'orizzonte, alle 12 in alto, alle 18 tramonta, a mezzanotte è sotto.
        float angolo = ora / 24f * 360f - 90f;
        if (sole != null) sole.transform.rotation = Quaternion.Euler(angolo, orientamento, 0f);
        if (luna != null) luna.transform.rotation = Quaternion.Euler(angolo + 180f, orientamento, 0f);

        float altezzaSole = Mathf.Sin(angolo * Mathf.Deg2Rad);
        float giorno = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.1f, 0.25f, altezzaSole));
        float vicinoOrizzonte = Mathf.Clamp01(1f - Mathf.Abs(altezzaSole - 0.05f) / 0.25f);
        Buio = 1f - giorno;

        if (sole != null)
        {
            sole.intensity = intensitaSole * giorno;
            sole.color = Color.Lerp(coloreSole, coloreSoleAlba, vicinoOrizzonte);
            sole.enabled = giorno > 0.001f;
        }
        if (luna != null)
        {
            luna.intensity = intensitaLuna * Buio;
            luna.enabled = Buio > 0.001f;
        }

        RenderSettings.ambientLight = Color.Lerp(ambienteNotte, ambienteGiorno, giorno);
        RenderSettings.fogColor = Color.Lerp(nebbiaNotte, nebbiaGiorno, giorno);
        RenderSettings.fogDensity = Mathf.Lerp(densitaNebbiaNotte, densitaNebbiaGiorno, giorno);

        if (cielo != null && cielo.HasProperty("_Exposure"))
            cielo.SetFloat("_Exposure", Mathf.Lerp(0.08f, 1.1f, giorno));
    }

    void OnGUI()
    {
        int ore = Mathf.FloorToInt(ora);
        int minuti = Mathf.FloorToInt((ora - ore) * 60f);
        GUI.Box(new Rect(Screen.width - 250, 10, 240, 44), GUIContent.none);
        GUI.Label(new Rect(Screen.width - 240, 14, 230, 20), "Ora " + ore.ToString("00") + ":" + minuti.ToString("00"));
        GUI.Label(new Rect(Screen.width - 240, 32, 230, 20), "Tieni premuto T per accelerare");
    }
}
