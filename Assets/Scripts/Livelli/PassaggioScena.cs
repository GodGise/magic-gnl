using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Un ingresso o un'uscita fra il mondo e una zona in una scena a parte (per esempio la scala della cripta).
// A cosa serve: vicino compare "E  Entra nella cripta" (o "E  Esci dalla cripta"); premendo il tasto parte il passaggio
// con la schermata di caricamento (CaricamentoZona).
// Come montarlo:
//   - Ingresso (nel villaggio): "Verso" = Entra, "Scena" = il nome della scena della zona (deve essere nelle Build
//     Settings), "Chiave Titolo" = il nome della zona in Lingua (zona.cripta), "Nascondi Dentro" = gli oggetti del
//     mondo da spegnere mentre si è dentro (per esempio "Villaggio").
//   - Uscita (nella zona): "Verso" = Esci. Si torna dove si era entrati.
//   - "Chiave Azione": il testo in basso (hud.entra_cripta, hud.esci_cripta).
//   - "Dopo Il Passaggio": cosa succede una volta arrivati (per esempio cambiare l'obiettivo).
// Se il componente è spento il passaggio non si può usare (si accende da un evento, per esempio quando Edric racconta
// della cripta). Ignora il tasto con l'inventario aperto, in pausa o durante un dialogo.
public class PassaggioScena : MonoBehaviour
{
    public enum Verso { Entra, Esci }

    [SerializeField] Verso verso = Verso.Entra;
    [SerializeField] string scena = "Cripta";
    [SerializeField] string chiaveTitolo = "zona.cripta";
    [SerializeField] string chiaveAzione = "hud.entra_cripta";
    [SerializeField] float raggio = 2.5f;
    [SerializeField] GameObject[] nascondiDentro;
    [SerializeField] UnityEvent dopoIlPassaggio;

    GiocatoreControllo giocatore;
    InputAction comando;

    void Awake()
    {
        comando = new InputAction("Passaggio", InputActionType.Button);
        comando.AddBinding("<Keyboard>/e");
        comando.AddBinding("<Gamepad>/buttonSouth");
        Comandi.Collega(comando, Azione.Interagisci, this);   // tasto scelto in Opzioni > Comandi
    }

    void OnEnable() => comando.Enable();
    void OnDisable() => comando.Disable();
    void OnDestroy() => comando.Dispose();

    void Update()
    {
        if (CaricamentoZona.InPassaggio) return;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null || !giocatore.enabled || giocatore.StatoAttuale != GiocatoreControllo.Stato.Libero) return;
        if (Dialogo.InCorso || Dialogo.ApenaFinito || MenuPausa.InPausa || InventarioGioco.Aperto) return;
        Vector3 d = giocatore.transform.position - transform.position;
        d.y = 0f;
        if (d.magnitude > raggio) return;
        HudGioco.MostraAzione("E", Lingua.T(chiaveAzione));
        if (!comando.WasPressedThisFrame()) return;
        if (verso == Verso.Entra) CaricamentoZona.Entra(scena, chiaveTitolo, nascondiDentro, () => dopoIlPassaggio?.Invoke());
        else
        {
            var evento = dopoIlPassaggio;   // questo oggetto sparisce con la zona: l'evento parte prima di scaricarla
            evento?.Invoke();
            string uscita = "uscita." + gameObject.scene.name;   // la scena sparisce: il nome si prende adesso
            CaricamentoZona.Esci(() => SegnaliMissioni.Invia(uscita));
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = verso == Verso.Entra ? new Color(0.4f, 0.9f, 0.5f) : new Color(0.9f, 0.6f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, raggio);
    }
}
