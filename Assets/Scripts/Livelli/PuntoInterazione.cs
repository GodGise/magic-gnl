using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Un punto con cui si interagisce con il tasto Interagisci: "E  Prendi la legna", "E  Metti la legna nel fuoco".
// A cosa serve: le azioni semplici delle quest, senza scrivere uno script per ognuna.
// Come montarlo: su un oggetto della scena (la catasta, il camino...). "Chiave Azione" è il testo che compare in basso
// (chiave di Lingua, per esempio hud.prendi_legna). "Al Premere" è cosa succede (si trascinano oggetti e si sceglie
// la funzione: cambiare l'obiettivo in Missioni, accendere un altro punto, far partire un DialogoAutomatico...).
// "Una Volta Sola": dopo l'uso il punto si spegne. Per farlo comparire solo più avanti nella quest, si lascia
// l'oggetto spento e lo si accende da un evento (GameObject > SetActive).
// Ignora il tasto con l'inventario aperto, in pausa o durante un dialogo.
public class PuntoInterazione : MonoBehaviour
{
    [Tooltip("Testo dell'azione in basso (chiave di Lingua).")]
    [SerializeField] string chiaveAzione = "hud.parla";
    [Tooltip("Distanza (in orizzontale) entro cui si può usare, in metri.")]
    [SerializeField] float raggio = 2f;
    [SerializeField] bool unaVoltaSola = true;
    [SerializeField] UnityEvent alPremere;

    GiocatoreControllo giocatore;
    InputAction comando;
    bool usato;

    void Awake()
    {
        comando = new InputAction("Interagisci punto", InputActionType.Button);
        comando.AddBinding("<Keyboard>/e");
        comando.AddBinding("<Gamepad>/buttonSouth");
        Comandi.Collega(comando, Azione.Interagisci, this);   // tasto scelto in Opzioni > Comandi
    }

    void OnEnable() => comando.Enable();
    void OnDisable() => comando.Disable();
    void OnDestroy() => comando.Dispose();

    void Update()
    {
        if (usato && unaVoltaSola) return;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return;
        if (Dialogo.InCorso || Dialogo.ApenaFinito || MenuPausa.InPausa || InventarioGioco.Aperto) return;
        Vector3 d = giocatore.transform.position - transform.position;
        d.y = 0f;
        if (d.magnitude > raggio) return;
        HudGioco.MostraAzione("E", Lingua.T(chiaveAzione));
        if (!comando.WasPressedThisFrame()) return;
        usato = true;
        alPremere.Invoke();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.9f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, raggio);
    }
}
