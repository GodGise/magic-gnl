using UnityEngine;
using UnityEngine.InputSystem;

// Rialzare un alleato a terra, in co-op (regola di Lorenzo, 10 ottobre; vedi GiocatoreControllo, "a terra").
// A cosa serve: vicino a un giocatore a terra compare "E  Rialza: Giocatore 2"; tenendo premuto E (A sul pad) per
// "Secondi Per Rialzare" secondi lo si rimette in piedi con una parte della vita. Se si lascia il tasto, ci si
// allontana o si viene colpiti (si esce dallo stato Libero), si ricomincia da zero.
// Il tasto è quello di "Interagisci" scelto in Opzioni > Comandi. Non vale con l'inventario o la pausa aperti.
// Come montarlo: non si monta. GiocatoreControllo lo aggiunge da solo al giocatore.
[RequireComponent(typeof(GiocatoreControllo))]
public class RianimaAlleato : MonoBehaviour
{
    [Tooltip("Secondi da tenere premuto il tasto per rialzare un alleato.")]
    [SerializeField] float secondiPerRialzare = 3f;
    [Tooltip("Distanza massima dall'alleato a terra, in metri.")]
    [SerializeField] float distanza = 2.2f;

    GiocatoreControllo giocatore;
    InputAction comandoRialza;
    float tenuto;
    GiocatoreRete scelto;

    void Awake()
    {
        giocatore = GetComponent<GiocatoreControllo>();
        comandoRialza = new InputAction("Rialza", InputActionType.Button);
        comandoRialza.AddBinding("<Keyboard>/e");
        comandoRialza.AddBinding("<Gamepad>/buttonSouth");
        Comandi.Collega(comandoRialza, Azione.Interagisci, this);   // tasto scelto in Opzioni > Comandi
    }

    void OnEnable() => comandoRialza.Enable();
    void OnDisable() => comandoRialza.Disable();
    void OnDestroy() => comandoRialza.Dispose();

    void Update()
    {
        var vicino = giocatore.StatoAttuale == GiocatoreControllo.Stato.Libero && !InventarioGioco.Aperto && !MenuPausa.InPausa
            ? AlleatoATerraVicino() : null;
        if (vicino != scelto) tenuto = 0f;
        scelto = vicino;
        if (scelto == null) { tenuto = 0f; return; }

        string testo = Lingua.T("hud.rialza") + ": " + Lingua.T("rete.giocatore") + " " + scelto.Numero;
        if (tenuto > 0f) testo += "  " + Mathf.FloorToInt(100f * tenuto / Mathf.Max(0.1f, secondiPerRialzare)) + "%";
        HudGioco.MostraAzione("E", testo);

        if (!comandoRialza.IsPressed()) { tenuto = 0f; return; }
        tenuto += Time.deltaTime;
        if (tenuto < secondiPerRialzare) return;
        tenuto = 0f;
        scelto.ChiediRialza();
    }

    GiocatoreRete AlleatoATerraVicino()
    {
        GiocatoreRete migliore = null;
        float minima = distanza;
        foreach (var altro in GiocatoreRete.Altri)
        {
            if (altro == null || !altro.IsSpawned || !altro.ATerra) continue;
            Vector3 d = altro.transform.position - transform.position;
            if (Mathf.Abs(d.y) > 2f) continue;
            d.y = 0f;
            if (d.magnitude > minima) continue;
            minima = d.magnitude;
            migliore = altro;
        }
        return migliore;
    }
}
