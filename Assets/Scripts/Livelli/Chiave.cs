using UnityEngine;
using UnityEngine.InputSystem;

// Chiave da raccogliere con E.
// A cosa serve: una chiave dorata che ruota piano e brilla (si vede anche al buio). Quando il
// giocatore è vicino compare "E  Raccogli: ..."; premendo E la chiave sparisce e finisce
// nell'Inventario. Apre la Serratura che ha lo stesso codice (per esempio "chiesa").
// Come montarlo:
//   1. GameObject > Create Empty, mettilo dove vuoi la chiave, a circa 1 metro da terra.
//   2. Add Component > Chiave, e scrivi nel campo Codice lo stesso codice della Serratura da aprire.
//   L'aspetto della chiave si crea da solo quando parte il gioco; nella vista Scene è segnata in giallo.
public class Chiave : MonoBehaviour
{
    [Tooltip("Codice della chiave: deve essere uguale al Codice della Serratura che apre.")]
    [SerializeField] string codice = "chiesa";
    [Tooltip("Nome mostrato al giocatore.")]
    [SerializeField] string nomeVisibile = "Chiave della chiesa";
    [Tooltip("Distanza (in orizzontale) entro cui si può raccogliere.")]
    [SerializeField] float raggioRaccolta = 1.8f;
    [SerializeField] Color colore = new Color(1f, 0.78f, 0.3f);

    GiocatoreControllo giocatore;
    InputAction comandoInteragisci;
    Transform aspetto;
    bool raccolta;

    // Stesso tasto di leva e checkpoint: E sulla tastiera, A (Xbox) o Croce (PS) sul pad.
    void Awake()
    {
        comandoInteragisci = new InputAction("Interagisci", InputActionType.Button);
        comandoInteragisci.AddBinding("<Keyboard>/e");
        comandoInteragisci.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable() => comandoInteragisci.Enable();
    void OnDisable() => comandoInteragisci.Disable();
    void OnDestroy() => comandoInteragisci.Dispose();

    void Start()
    {
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
        CreaAspetto();
    }

    void Update()
    {
        if (raccolta) return;

        // Ruota e ondeggia piano, per farsi notare.
        aspetto.localRotation = Quaternion.Euler(0f, Time.time * 90f, 0f);
        aspetto.localPosition = Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.08f);

        if (PuoRaccogliere() && comandoInteragisci.WasPressedThisFrame()) Raccogli();
    }

    bool PuoRaccogliere()
    {
        if (raccolta || giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        Vector3 distanza = giocatore.transform.position - transform.position;
        if (Mathf.Abs(distanza.y) > 2.5f) return false;
        distanza.y = 0f;
        return distanza.magnitude <= raggioRaccolta;
    }

    void Raccogli()
    {
        raccolta = true;
        Inventario.Di(giocatore).AggiungiChiave(codice);
        Suoni.Suona(Suono.Raccolta, transform.position);
        MessaggiSchermo.Mostra("Hai raccolto: " + nomeVisibile, 3f);
        gameObject.SetActive(false);
    }

    // Chiave a blocchi: anello, gambo e due denti, dorata e luminosa, con una piccola luce.
    void CreaAspetto()
    {
        aspetto = new GameObject("Aspetto chiave").transform;
        aspetto.SetParent(transform, false);

        Material oro = null;
        oro = Parte("Anello", PrimitiveType.Cylinder, new Vector3(0f, 0.16f, 0f), new Vector3(0.2f, 0.02f, 0.2f), Quaternion.Euler(90f, 0f, 0f), oro);
        Parte("Gambo", PrimitiveType.Cube, new Vector3(0f, -0.06f, 0f), new Vector3(0.04f, 0.34f, 0.04f), Quaternion.identity, oro);
        Parte("Dente basso", PrimitiveType.Cube, new Vector3(0.05f, -0.19f, 0f), new Vector3(0.08f, 0.04f, 0.03f), Quaternion.identity, oro);
        Parte("Dente alto", PrimitiveType.Cube, new Vector3(0.04f, -0.12f, 0f), new Vector3(0.06f, 0.035f, 0.03f), Quaternion.identity, oro);

        Light luce = new GameObject("Luce chiave").AddComponent<Light>();
        luce.transform.SetParent(aspetto, false);
        luce.type = LightType.Point;
        luce.color = colore;
        luce.range = 4f;
        luce.intensity = 1.5f;
    }

    // Crea un pezzo; la prima volta crea anche il materiale dorato e lo restituisce, così gli altri pezzi lo riusano.
    Material Parte(string nome, PrimitiveType tipo, Vector3 posizione, Vector3 scala, Quaternion rotazione, Material materiale)
    {
        GameObject parte = GameObject.CreatePrimitive(tipo);
        parte.name = nome;
        DestroyImmediate(parte.GetComponent<Collider>());
        parte.transform.SetParent(aspetto, false);
        parte.transform.localPosition = posizione;
        parte.transform.localRotation = rotazione;
        parte.transform.localScale = scala;

        Renderer r = parte.GetComponent<Renderer>();
        if (materiale == null)
        {
            materiale = new Material(r.sharedMaterial) { color = colore };
            materiale.EnableKeyword("_EMISSION");
            materiale.SetColor("_EmissionColor", colore * 0.7f);
        }
        r.sharedMaterial = materiale;
        return materiale;
    }

    void OnGUI()
    {
        if (!PuoRaccogliere()) return;
        var stile = new GUIStyle(GUI.skin.box) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        GUI.Box(new Rect(Screen.width * 0.5f - 170f, Screen.height * 0.75f, 340f, 40f), "E   Raccogli: " + nomeVisibile, stile);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(transform.position, new Vector3(0.2f, 0.45f, 0.2f));
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, raggioRaccolta);
    }
}
