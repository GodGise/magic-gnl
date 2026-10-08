using UnityEngine;
using UnityEngine.InputSystem;

// Serratura per una Porta che si apre con una chiave.
// A cosa serve: vicino alla porta compare "E  Apri con la chiave" se il giocatore ha la Chiave con lo
// stesso codice, oppure "Chiusa a chiave" se non ce l'ha. Con la chiave, premendo E la porta si apre;
// senza, compare la scritta "Serve: ..." con un suono di porta bloccata.
// Come montarlo: sullo stesso oggetto della Porta (Add Component > Serratura) e scrivi nel campo
// Codice lo stesso codice della Chiave. Non serve una Leva.
// Co-op: la chiave raccolta da un giocatore ce l'hanno tutti (vedi Chiave), e la porta aperta è aperta per tutti (vedi Porta).
[RequireComponent(typeof(Porta))]
public class Serratura : MonoBehaviour
{
    [Tooltip("Codice della chiave che apre questa porta (uguale al Codice della Chiave).")]
    [SerializeField] string codice = "chiesa";
    [Tooltip("Nome della chiave, per la scritta \"Serve: ...\".")]
    [SerializeField] string nomeChiave = "Chiave della chiesa";
    [Tooltip("Distanza (in orizzontale) dal centro della porta entro cui si può usare.")]
    [SerializeField] float raggioInterazione = 2.5f;

    Porta porta;
    GiocatoreControllo giocatore;
    InputAction comandoInteragisci;
    float prossimoTentativo;   // co-op: mentre si aspetta l'host, la porta non si riprova a ogni pressione

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
        porta = GetComponent<Porta>();
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
    }

    void Update()
    {
        if (!Vicino()) return;
        HudGioco.MostraAzione("E", Lingua.T(HaLaChiave() ? "hud.apri_chiave" : "hud.chiusa_chiave"));
        if (!comandoInteragisci.WasPressedThisFrame() || MenuPausa.InPausa || InventarioGioco.Aperto) return;
        if (Time.time < prossimoTentativo) return;
        prossimoTentativo = Time.time + 1f;

        if (HaLaChiave())
        {
            Suoni.Suona(Suono.Serratura, transform.position);
            porta.Apri();
        }
        else
        {
            Suoni.Suona(Suono.Negato, transform.position, 0.8f);
            MessaggiSchermo.Mostra(Lingua.T("hud.serve") + ": " + nomeChiave, 2.5f);
        }
    }

    // Vero se la porta è ancora chiusa e il giocatore, vivo, è abbastanza vicino.
    bool Vicino()
    {
        if (porta == null || porta.Aperta || giocatore == null) return false;
        if (giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        Vector3 distanza = giocatore.transform.position - transform.position;
        distanza.y = 0f;
        return distanza.magnitude <= raggioInterazione;
    }

    bool HaLaChiave()
    {
        Inventario inventario = Inventario.Di(giocatore);
        return inventario != null && inventario.HaChiave(codice);
    }
}
