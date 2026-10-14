using UnityEngine;
using UnityEngine.InputSystem;

// Checkpoint (per esempio un altare o un falò): il punto in cui si rinasce dopo la morte.
// A cosa serve: quando il giocatore è vicino compare la scritta "E  Accendi il checkpoint";
// premendo E (oppure A / Croce sul pad) il checkpoint si accende (cambia colore e fa luce) e
// diventa il nuovo punto di rinascita. Se il giocatore muore, rinasce davanti all'ultimo
// checkpoint acceso. Accendendone un altro, quello di prima si spegne.
// Come montarlo:
//   1. Crea l'oggetto che fa da altare, per esempio GameObject > 3D Object > Cylinder, schiacciato
//      (Scale Y piccola), e mettilo sul terreno.
//   2. Nell'Inspector clicca Add Component e scegli Checkpoint.
//   3. Nella vista Scene una sfera verde a fil di ferro mostra da quanto vicino si può accendere, e una pallina azzurra mostra
//      dove ricompare il giocatore: tienila sul terreno libero, non dentro un muro.
// Co-op: ogni giocatore ha il suo checkpoint (rinasce dove l'ha acceso lui): non è condiviso, di proposito.
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Distanza (in orizzontale) dal centro del checkpoint entro cui il giocatore può accenderlo con E.")]
    [SerializeField] float raggioAttivazione = 2f;
    [Tooltip("Dove ricompare il giocatore rispetto al checkpoint: di base 1,5 m davanti e 1 m più in alto (il centro del personaggio).")]
    [SerializeField] Vector3 spostamentoRinascita = new Vector3(0f, 1f, 1.5f);
    [Tooltip("Colore del checkpoint acceso.")]
    [SerializeField] Color coloreAcceso = new Color(1f, 0.55f, 0.15f);
    [SerializeField] float intensitaLuce = 2.5f;
    [SerializeField] float raggioLuce = 7f;

    GiocatoreControllo giocatore;
    InputAction comandoInteragisci;
    Renderer aspetto;
    Color coloreSpento;
    Light luce;

    // Stesso tasto della leva: E sulla tastiera, A (Xbox) o Croce (PS) sul pad.
    void Awake()
    {
        comandoInteragisci = new InputAction("Interagisci", InputActionType.Button);
        comandoInteragisci.AddBinding("<Keyboard>/e");
        comandoInteragisci.AddBinding("<Gamepad>/buttonSouth");
        Comandi.Collega(comandoInteragisci, Azione.Interagisci, this);   // tasto scelto in Opzioni > Comandi
    }

    void OnEnable() => comandoInteragisci.Enable();
    void OnDisable() => comandoInteragisci.Disable();
    void OnDestroy() => comandoInteragisci.Dispose();

    void Start()
    {
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
        aspetto = GetComponentInChildren<Renderer>();
        if (aspetto != null) coloreSpento = aspetto.material.color;

        // Luce del checkpoint acceso: creata qui, spenta finché il giocatore non arriva.
        var oggettoLuce = new GameObject("Luce checkpoint");
        oggettoLuce.transform.SetParent(transform, false);
        oggettoLuce.transform.position = transform.position + Vector3.up * 1.5f;
        luce = oggettoLuce.AddComponent<Light>();
        luce.type = LightType.Point;
        luce.color = coloreAcceso;
        luce.intensity = intensitaLuce;
        luce.range = raggioLuce;
        luce.enabled = false;
    }

    void Update()
    {
        if (!PuoAccendere()) return;
        HudGioco.MostraAzione("E", Lingua.T("hud.checkpoint"));
        if (comandoInteragisci.WasPressedThisFrame() && !MenuPausa.InPausa && !InventarioGioco.Aperto) Accendi();
    }

    // Vero se il giocatore è vivo, abbastanza vicino e questo checkpoint non è già quello acceso.
    bool PuoAccendere()
    {
        if (giocatore == null || giocatore.UltimoCheckpoint == this) return false;
        if (giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;

        Vector3 distanza = giocatore.transform.position - transform.position;
        distanza.y = 0f;
        return distanza.magnitude <= raggioAttivazione;
    }

    void Accendi()
    {
        giocatore.RaggiungiCheckpoint(this, PuntoRinascita(), RotazioneRinascita());

        if (aspetto != null)
        {
            aspetto.material.color = coloreAcceso;
            aspetto.material.EnableKeyword("_EMISSION");
            aspetto.material.SetColor("_EmissionColor", coloreAcceso * 0.8f);
        }
        if (luce != null) luce.enabled = true;
        Suoni.Suona(Suono.FuocoAcceso, transform.position + Vector3.up);
        Debug.Log("Checkpoint raggiunto: " + name);
    }

    // Chiamato dal giocatore quando accende un altro checkpoint.
    public void Spegni()
    {
        if (aspetto != null)
        {
            aspetto.material.color = coloreSpento;
            aspetto.material.SetColor("_EmissionColor", Color.black);
        }
        if (luce != null) luce.enabled = false;
    }

    // Il giocatore ricompare davanti al checkpoint, girato nella stessa direzione.
    // Si usa solo la rotazione orizzontale e non la scala, così un altare schiacciato non sposta il punto.
    Quaternion RotazioneRinascita() => Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    Vector3 PuntoRinascita() => transform.position + RotazioneRinascita() * spostamentoRinascita;

    // Disegni visibili solo nella vista Scene, per sistemare il checkpoint.
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, raggioAttivazione);
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.9f);
        Gizmos.DrawSphere(PuntoRinascita(), 0.25f);
    }
}
