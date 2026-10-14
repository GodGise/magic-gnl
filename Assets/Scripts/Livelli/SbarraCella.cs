using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// La sbarra arrugginita della cella nella gattabuia (Docs/gattabuia.md, zona 1): si tiene premuto Interagisci per
// qualche secondo, la sbarra cede, la porta della cella si apre e la sbarra resta in mano come prima arma.
// Mentre si tiene premuto compare una barra che si riempie, con scricchiolii di ferro; lasciando il tasto si ricomincia.
// Come montarlo: su un oggetto vuoto davanti alla porta della cella (dentro). "Porta" = la Porta della cella (sbarre),
// "Sbarra" = l'oggetto DatiArma da dare (sbarra-arrugginita), "Dopo" = cosa succede (per esempio un nuovo obiettivo).
// Lo prepara il comando "magic-gnl > Gattabuia: crea la scena della gattabuia".
// Co-op (da fare quando la gattabuia sarà in rete): solo l'host forza la sua sbarra, poi apre le celle degli altri.
public class SbarraCella : MonoBehaviour
{
    [SerializeField] Porta porta;
    [SerializeField] DatiOggetto sbarra;
    [SerializeField, Min(0.2f)] float secondiDaTenere = 2.5f;
    [SerializeField] float raggio = 1.8f;
    [SerializeField] UnityEvent dopo;

    GiocatoreControllo giocatore;
    InputAction comando;
    float tenuto;
    bool fatto;
    float prossimoScricchiolio;

    void Awake()
    {
        comando = new InputAction("Forza la sbarra", InputActionType.Button);
        comando.AddBinding("<Keyboard>/e");
        comando.AddBinding("<Gamepad>/buttonSouth");
        Comandi.Collega(comando, Azione.Interagisci, this);
    }

    void OnEnable() => comando.Enable();
    void OnDisable() => comando.Disable();
    void OnDestroy() => comando.Dispose();

    void Update()
    {
        if (fatto) return;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null || MenuPausa.InPausa || InventarioGioco.Aperto || Dialogo.InCorso) { tenuto = 0f; return; }
        Vector3 d = giocatore.transform.position - transform.position;
        d.y = 0f;
        if (d.magnitude > raggio) { tenuto = 0f; return; }
        HudGioco.MostraAzione("E", Lingua.T("hud.forza_sbarra"));
        if (!comando.IsPressed()) { tenuto = Mathf.Max(0f, tenuto - Time.deltaTime * 2f); return; }
        tenuto += Time.deltaTime;
        if (Time.time >= prossimoScricchiolio)
        {
            prossimoScricchiolio = Time.time + 0.45f;
            Suoni.Suona(Suono.Serratura, transform.position + Vector3.up, 0.6f, Random.Range(0.6f, 0.8f));
            CameraTerzaPersona.Scuoti(0.01f, 0.2f);
        }
        if (tenuto >= secondiDaTenere) Cede();
    }

    void Cede()
    {
        fatto = true;
        Suoni.Suona(Suono.GuardiaRotta, transform.position + Vector3.up, 1f, 0.8f);
        if (porta != null) porta.Apri();
        var equip = giocatore.GetComponent<Equipaggiamento>();
        if (sbarra != null && equip != null)
        {
            var zaino = Zaino.Di(giocatore);
            if (equip.Arma != null && zaino != null) zaino.Aggiungi(equip.Arma);
            equip.Equipaggia(sbarra);
            MessaggiSchermo.Mostra(Lingua.T("hud.raccolto") + ": " + sbarra.Nome, 3f);
        }
        dopo.Invoke();
    }

    void OnGUI()
    {
        if (fatto || tenuto <= 0.01f || !HudGioco.Attivo) return;
        GraficaMenu.PreparaStili();
        float L = GraficaMenu.FoglioIntero();
        var r = new Rect(L * 0.5f - 160f, GraficaMenu.Altezza - 150f, 320f, 12f);
        GraficaMenu.Riempi(r, new Color(0f, 0f, 0f, 0.7f));
        GraficaMenu.Riempi(new Rect(r.x + 2f, r.y + 2f, (r.width - 4f) * Mathf.Clamp01(tenuto / secondiDaTenere), r.height - 4f), GraficaMenu.Selezione);
        GraficaMenu.Bordo(r, 2f, GraficaMenu.Bronzo);
    }
}
