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
// Co-op: la leva e le sue porte sono uguali per tutti; se la tira chi non ospita, la richiesta va all'host.
public class Leva : MonoBehaviour, IOggettoCondiviso
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

    public int NumeroRete { get; private set; }

    void Awake()
    {
        NumeroRete = RegistroCondivisi.Iscrivi(this);
        comandoInteragisci = new InputAction("Interagisci", InputActionType.Button);
        comandoInteragisci.AddBinding("<Keyboard>/e");
        comandoInteragisci.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable() => comandoInteragisci.Enable();
    void OnDisable() => comandoInteragisci.Disable();
    void OnDestroy()
    {
        comandoInteragisci.Dispose();
        RegistroCondivisi.Togli(this, NumeroRete);
    }

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

        if (!PuoUsare()) return;
        HudGioco.MostraAzione("E", Lingua.T("hud.leva"));
        if (comandoInteragisci.WasPressedThisFrame() && !MenuPausa.InPausa && !InventarioGioco.Aperto) Usa();
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
        bool giu = !abbassata;
        if (MondoRete.ChiediUso(this, giu ? 1 : 0)) return;   // co-op: la tira l'host per tutti
        abbassata = giu;
        Suoni.Suona(Suono.Leva, transform.position + Vector3.up * 0.6f);
        foreach (Porta porta in porte)
        {
            if (porta == null) continue;
            if (abbassata) porta.Apri();
            else porta.Chiudi();
        }
        MondoRete.InviaEvento(this, Rete.MioId, abbassata ? 1 : 0);
        Debug.Log(name + (abbassata ? ": leva abbassata" : ": leva alzata"));
    }

    // ---------- co-op (IOggettoCondiviso) ----------
    public void UsaDaRete(ulong chi, int valore, Vector3 punto)
    {
        if (unaVoltaSola && abbassata) return;
        if ((valore == 1) != abbassata) Usa();
    }

    // Le porte arrivano con i loro messaggi: qui si muove solo il manico.
    public void EventoDaRete(ulong chi, int valore, Vector3 punto)
    {
        abbassata = valore == 1;
        Suoni.Suona(Suono.Leva, transform.position + Vector3.up * 0.6f);
    }

    public int StatoRete => abbassata ? 1 : 0;
    public void StatoDaRete(int stato)
    {
        abbassata = stato == 1;
        progresso = abbassata ? 1f : 0f;
        AggiornaManico();
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
