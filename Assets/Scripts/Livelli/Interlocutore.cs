using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Un personaggio con cui si parla (Maren, Brannoc, Edric...).
// A cosa serve: quando il giocatore è vicino compare "E  Parla: Maren"; premendo il tasto parte il dialogo.
// Come montarlo:
//   1. Metti il componente sul personaggio (o su un oggetto vuoto accanto a lui, nel punto da cui si parla).
//   2. "Nome Chiave": la chiave del nome in Lingua (per esempio pg.maren).
//   3. "Righe": le righe del primo dialogo. Ogni riga ha due chiavi di Lingua (chi parla e il testo); il testo
//      può contenere {Interagisci}, {Attacca}... al posto dei tasti. "Solo Classe" fa sentire la riga a una classe sola.
//   4. "Righe Ripetute": cosa dice se gli si parla ancora (se vuoto, ripete le righe di prima).
//   5. "Al Termine": cosa succede a fine dialogo (per esempio avviare una missione); si trascina un oggetto e si sceglie la funzione.
// In co-op ognuno parla per conto suo: il dialogo lo vede solo chi ha premuto il tasto.
public class Interlocutore : MonoBehaviour
{
    [Tooltip("Chiave del nome in Lingua (per esempio pg.maren).")]
    [SerializeField] string nomeChiave;
    [SerializeField] RigaDialogo[] righe;
    [Tooltip("Cosa dice quando gli si parla di nuovo. Se vuoto, ripete le righe di prima.")]
    [SerializeField] RigaDialogo[] righeRipetute;
    [Tooltip("Distanza (in orizzontale) entro cui si può parlargli, in metri.")]
    [SerializeField] float raggio = 2.5f;
    [Tooltip("Se attivo, il dialogo parte una volta sola.")]
    [SerializeField] bool unaVoltaSola;
    [Tooltip("Cosa succede quando il dialogo finisce (solo la prima volta che lo si completa).")]
    [SerializeField] UnityEvent alTermine;

    GiocatoreControllo giocatore;
    InputAction comandoParla;
    bool parlatoUnaVolta;

    void Awake()
    {
        comandoParla = new InputAction("Parla", InputActionType.Button);
        comandoParla.AddBinding("<Keyboard>/e");
        comandoParla.AddBinding("<Gamepad>/buttonSouth");
        Comandi.Collega(comandoParla, Azione.Interagisci, this);   // tasto scelto in Opzioni > Comandi
    }

    void OnEnable() => comandoParla.Enable();
    void OnDisable() => comandoParla.Disable();
    void OnDestroy() => comandoParla.Dispose();

    void Start() => giocatore = FindFirstObjectByType<GiocatoreControllo>();

    void Update()
    {
        if (Dialogo.InCorso || Dialogo.ApenaFinito || !PuoParlare()) return;
        string nome = string.IsNullOrEmpty(nomeChiave) ? "" : ": " + Lingua.T(nomeChiave);
        HudGioco.MostraAzione("E", Lingua.T("hud.parla") + nome);
        if (comandoParla.WasPressedThisFrame() && !MenuPausa.InPausa && !InventarioGioco.Aperto) Parla();
    }

    bool PuoParlare()
    {
        if (giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        if (unaVoltaSola && parlatoUnaVolta) return false;
        Vector3 distanza = giocatore.transform.position - transform.position;
        distanza.y = 0f;
        return distanza.magnitude <= raggio;
    }

    void Parla()
    {
        bool primaVolta = !parlatoUnaVolta;
        var dette = primaVolta || righeRipetute == null || righeRipetute.Length == 0 ? righe : righeRipetute;
        Dialogo.Avvia(dette, () =>
        {
            if (!primaVolta) return;
            parlatoUnaVolta = true;
            alTermine?.Invoke();
        });
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, raggio);
    }
}
