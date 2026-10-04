using UnityEngine;
using UnityEngine.InputSystem;

// Leva che apre (e, se vuoi, richiude) una o più porte.
// A cosa serve: quando il giocatore è vicino compare la scritta "E  Tira la leva". Premendo E
// (oppure A / Croce sul pad) il manico si abbassa e le porte collegate si aprono.
// Come montarlo:
//   1. GameObject > Create Empty, mettilo a terra dove vuoi la leva e giralo (Rotation Y) in modo
//      che guardi verso il punto da cui arriva il giocatore. Non cambiare la sua Scale.
//   2. Nell'Inspector clicca Add Component e scegli Leva. Base e manico si creano da soli quando
//      parte il gioco; nella vista Scene la leva è segnata in giallo, con il raggio in cui si usa.
//   3. Nel campo "Porte" trascina le porte da aprire. Se lo lasci vuoto, la leva apre la Porta
//      più vicina (entro 30 metri). Una linea gialla nella vista Scene mostra le porte collegate.
public class Leva : MonoBehaviour
{
    [Tooltip("Porte che la leva apre. Se vuoto, usa la Porta più vicina.")]
    [SerializeField] Porta[] porte;
    [Tooltip("Distanza (in orizzontale) entro cui il giocatore può usare la leva.")]
    [SerializeField] float raggioInterazione = 2f;
    [Tooltip("Se attivo la leva si usa una volta sola e resta abbassata; se spento apre e richiude a ogni uso.")]
    [SerializeField] bool unaVoltaSola = true;
    [Tooltip("Secondi che impiega il manico ad abbassarsi o alzarsi.")]
    [SerializeField] float durataMovimento = 0.4f;

    const float DistanzaMassimaPortaVicina = 30f;

    GiocatoreControllo giocatore;
    InputAction comandoInteragisci;
    Transform manico;
    bool abbassata;
    float progresso; // 0 = manico su, 1 = manico giù

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

        if (porte == null || porte.Length == 0)
        {
            Porta vicina = PortaPiuVicina();
            porte = vicina != null ? new[] { vicina } : new Porta[0];
            if (vicina == null) Debug.LogWarning(name + ": nessuna Porta collegata né vicina, la leva non apre niente.");
        }

        CreaAspetto();
        AggiornaManico();
    }

    void Update()
    {
        // Movimento morbido del manico verso su o giù.
        float obiettivo = abbassata ? 1f : 0f;
        if (!Mathf.Approximately(progresso, obiettivo))
        {
            progresso = Mathf.MoveTowards(progresso, obiettivo, Time.deltaTime / Mathf.Max(0.01f, durataMovimento));
            AggiornaManico();
        }

        if (PuoUsare() && comandoInteragisci.WasPressedThisFrame()) Usa();
    }

    // Vero se il giocatore è vivo, abbastanza vicino e la leva si può ancora usare.
    bool PuoUsare()
    {
        if (giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        if (unaVoltaSola && abbassata) return false;

        Vector3 distanza = giocatore.transform.position - transform.position;
        distanza.y = 0f;
        return distanza.magnitude <= raggioInterazione;
    }

    void Usa()
    {
        abbassata = !abbassata;
        foreach (Porta porta in porte)
        {
            if (porta == null) continue;
            if (abbassata) porta.Apri();
            else porta.Chiudi();
        }
        Debug.Log(name + (abbassata ? ": leva abbassata" : ": leva alzata"));
    }

    Porta PortaPiuVicina()
    {
        Porta migliore = null;
        float minima = DistanzaMassimaPortaVicina;
        foreach (Porta porta in FindObjectsByType<Porta>(FindObjectsSortMode.None))
        {
            float d = Vector3.Distance(porta.transform.position, transform.position);
            if (d < minima)
            {
                minima = d;
                migliore = porta;
            }
        }
        return migliore;
    }

    // Aspetto provvisorio a blocchi: una base di pietra e un manico di legno con il pomello di ferro.
    void CreaAspetto()
    {
        Transform aspetto = new GameObject("Aspetto leva").transform;
        aspetto.SetParent(transform, false);

        // La base tiene il suo collider: il giocatore non ci passa attraverso.
        Parte(aspetto, "Base", PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(0.5f, 0.4f, 0.35f), new Color(0.24f, 0.24f, 0.26f), true);

        // Il manico ruota attorno a questo perno, in cima alla base.
        manico = new GameObject("Perno manico").transform;
        manico.SetParent(aspetto, false);
        manico.localPosition = new Vector3(0f, 0.4f, 0f);
        Parte(manico, "Manico", PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(0.07f, 0.35f, 0.07f), new Color(0.3f, 0.2f, 0.12f), false);
        Parte(manico, "Pomello", PrimitiveType.Sphere, new Vector3(0f, 0.72f, 0f), Vector3.one * 0.16f, new Color(0.32f, 0.3f, 0.28f), false);
    }

    static void Parte(Transform genitore, string nome, PrimitiveType tipo, Vector3 posizione, Vector3 scala, Color colore, bool tieniCollider)
    {
        GameObject parte = GameObject.CreatePrimitive(tipo);
        parte.name = nome;
        if (!tieniCollider) DestroyImmediate(parte.GetComponent<Collider>());
        parte.transform.SetParent(genitore, false);
        parte.transform.localPosition = posizione;
        parte.transform.localScale = scala;
        parte.GetComponent<Renderer>().material.color = colore;
    }

    // Manico su: inclinato all'indietro. Manico giù: inclinato in avanti, verso il giocatore.
    void AggiornaManico()
    {
        if (manico != null) manico.localRotation = Quaternion.Euler(Mathf.Lerp(-40f, 40f, progresso), 0f, 0f);
    }

    // Scritta in basso al centro quando il giocatore può usare la leva.
    void OnGUI()
    {
        if (!PuoUsare()) return;
        var stile = new GUIStyle(GUI.skin.box) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        GUI.Box(new Rect(Screen.width * 0.5f - 130f, Screen.height * 0.75f, 260f, 40f), "E   Tira la leva", stile);
    }

    // Disegni visibili solo nella vista Scene: la leva, il raggio in cui si usa e le porte collegate.
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 1f);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, new Vector3(0.5f, 1f, 0.5f));
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, raggioInterazione);

        if (porte == null) return;
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.9f);
        foreach (Porta porta in porte)
        {
            if (porta != null) Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, porta.transform.position);
        }
    }
}
