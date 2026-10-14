using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Il portone in cima alla scalinata della gattabuia: con la chiave del Carceriere nello zaino, E lo apre, si esce nel
// bosco morto ed è la fine del capitolo 1 (schermo nero con il titolo, poi si torna al menu).
// Senza la chiave compare "Serve la chiave del portone".
// Come montarlo: su un oggetto vuoto davanti al portone; "Chiave" = l'oggetto chiave (DatiOggettoMissione),
// "Ante" = le due ante da aprire (ruotano sui cardini), "Scena Del Menu" = la scena dove tornare.
// Lo prepara il comando "magic-gnl > Gattabuia: crea la scena della gattabuia".
public class FineCapitolo : MonoBehaviour
{
    [SerializeField] DatiOggetto chiave;
    [SerializeField] Transform[] ante;
    [SerializeField] float raggio = 2.5f;
    [SerializeField] string scenaDelMenu = "Menu";

    GiocatoreControllo giocatore;
    InputAction comando;
    bool fatto;
    float nero;
    bool titolo;

    void Awake()
    {
        comando = new InputAction("Apri il portone", InputActionType.Button);
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
        if (giocatore == null || MenuPausa.InPausa || InventarioGioco.Aperto || Dialogo.InCorso) return;
        Vector3 d = giocatore.transform.position - transform.position;
        d.y = 0f;
        if (d.magnitude > raggio) return;
        var zaino = Zaino.Di(giocatore);
        bool haLaChiave = chiave == null || (zaino != null && zaino.Contiene(chiave));
        HudGioco.MostraAzione("E", Lingua.T(haLaChiave ? "hud.apri_portone" : "hud.serve_chiave"));
        if (haLaChiave && comando.WasPressedThisFrame()) StartCoroutine(Fine());
    }

    IEnumerator Fine()
    {
        fatto = true;
        giocatore.enabled = false;
        Suoni.Suona(Suono.PortaPietra, transform.position + Vector3.up, 1f, 0.6f);
        for (float t = 0f; t < 2f; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / 2f);
            for (int i = 0; ante != null && i < ante.Length; i++)
                if (ante[i] != null) ante[i].localRotation = Quaternion.Euler(0f, (i == 0 ? -1f : 1f) * 100f * k, 0f);
            yield return null;
        }
        for (float t = 0f; t < 2f; t += Time.deltaTime) { nero = t / 2f; yield return null; }
        nero = 1f;
        titolo = true;
        yield return new WaitForSecondsRealtime(6f);
        if (Application.CanStreamedLevelBeLoaded(scenaDelMenu)) SceneManager.LoadScene(scenaDelMenu);
    }

    void OnGUI()
    {
        if (nero <= 0.001f) return;
        GUI.depth = -80;
        GUI.color = new Color(0f, 0f, 0f, nero);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        if (!titolo) return;
        GraficaMenu.PreparaStili();
        float L = GraficaMenu.FoglioIntero();
        GraficaMenu.Scritta(new Rect(0f, 420f, L, 100f), Lingua.T("msg.gb.fine"), GraficaMenu.Titolo, GraficaMenu.Testo, 1f);
        GraficaMenu.Divisore(L * 0.5f, 540f, 520f, 1f);
        GraficaMenu.Scritta(new Rect(0f, 560f, L, 60f), Lingua.T("msg.gb.fine_sotto"), GraficaMenu.Voce, GraficaMenu.Spento, 1f);
    }
}
