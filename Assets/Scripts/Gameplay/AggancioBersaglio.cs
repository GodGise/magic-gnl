using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Aggancio del bersaglio (lock-on), come in Elden Ring.
// A cosa serve: tiene camera e personaggio puntati su un nemico.
//   - Agganciare / sganciare: clic della rotellina del mouse, oppure premere la levetta destra del pad.
//   - Cambiare nemico: muovere il mouse di scatto verso il nemico che si vuole (a destra, a sinistra, in alto per
//     uno più lontano, in basso per uno più vicino), oppure spostare di scatto la levetta destra del pad.
//     Viene scelto il nemico che sullo schermo sta più nella direzione del gesto, partendo da quello agganciato.
//     Un movimento lento del mouse non cambia niente: serve uno scatto più lungo di "Scatto Mouse" pixel.
//   - La rotellina non cambia più nemico: con lo Stregone sceglie l'incantesimo (vedi MagiaStregone).
// Si sgancia da solo se il nemico muore o si allontana troppo.
// Sopra il nemico agganciato compare un quadratino rosso.
// Come montarlo: sullo stesso oggetto del GiocatoreControllo (lo richiede da solo).
// La CameraTerzaPersona lo trova da sola sul suo bersaglio.
public class AggancioBersaglio : MonoBehaviour
{
    [SerializeField] float distanzaMassimaAggancio = 15f;
    [Tooltip("Oltre questa distanza l'aggancio si toglie da solo.")]
    [SerializeField] float distanzaSgancio = 20f;
    [Tooltip("Secondi minimi tra un cambio di bersaglio e il successivo.")]
    [SerializeField] float pausaCambioBersaglio = 0.3f;
    [Tooltip("Quanti pixel deve muoversi il mouse, di scatto, per passare a un altro nemico. Più alto = serve un gesto più deciso.")]
    [SerializeField] float scattoMouse = 140f;
    [Tooltip("Quanto in fretta si \"dimentica\" il movimento del mouse: più alto = il gesto deve essere più rapido.")]
    [SerializeField] float dimenticaMouse = 6f;
    [Tooltip("Il nemico scelto deve stare entro questo angolo (in gradi) dalla direzione del gesto.")]
    [SerializeField] float angoloGesto = 60f;

    public Bersaglio Attuale { get; private set; }
    public bool Agganciato => Attuale != null;

    InputAction comandoAggancia, comandoMouse, comandoLevettaPad;
    float prossimoCambioPossibile;
    bool levettaANeutro = true;
    Vector2 gestoMouse;   // movimento del mouse accumulato negli ultimi istanti

    void Awake()
    {
        comandoAggancia = new InputAction("Aggancia", InputActionType.Button);
        comandoAggancia.AddBinding("<Mouse>/middleButton");
        comandoAggancia.AddBinding("<Gamepad>/rightStickPress");
        Comandi.Collega(comandoAggancia, Azione.Aggancia, this);   // tasto scelto in Opzioni > Comandi

        comandoMouse = new InputAction("CambiaBersaglioMouse", InputActionType.Value);
        comandoMouse.AddBinding("<Mouse>/delta");

        comandoLevettaPad = new InputAction("CambiaBersaglioPad", InputActionType.Value);
        comandoLevettaPad.AddBinding("<Gamepad>/rightStick");
    }

    void OnEnable()
    {
        comandoAggancia.Enable();
        comandoMouse.Enable();
        comandoLevettaPad.Enable();
    }

    void OnDisable()
    {
        comandoAggancia.Disable();
        comandoMouse.Disable();
        comandoLevettaPad.Disable();
    }

    void OnDestroy()
    {
        comandoAggancia.Dispose();
        comandoMouse.Dispose();
        comandoLevettaPad.Dispose();
    }

    void Update()
    {
        if (Attuale != null && !Valido(Attuale, distanzaSgancio)) Attuale = null;

        if (comandoAggancia.WasPressedThisFrame())
        {
            Attuale = Attuale != null ? null : ScegliPiuCentrale();
        }

        if (Attuale == null) { gestoMouse = Vector2.zero; return; }
        if (InventarioGioco.Aperto || MenuPausa.InPausa) { gestoMouse = Vector2.zero; return; }

        // Mouse: il movimento si accumula e svanisce in fretta, così conta solo uno scatto deciso.
        // Subito dopo un cambio il movimento non conta, così uno scatto solo non salta due nemici.
        if (Time.time < prossimoCambioPossibile) gestoMouse = Vector2.zero;
        else gestoMouse += comandoMouse.ReadValue<Vector2>();
        gestoMouse *= Mathf.Exp(-dimenticaMouse * Time.deltaTime);
        if (gestoMouse.magnitude >= scattoMouse)
        {
            CambiaBersaglio(gestoMouse.normalized);
            gestoMouse = Vector2.zero;
        }

        // Pad: scatto della levetta destra in una direzione, poi va riportata al centro.
        Vector2 levetta = comandoLevettaPad.ReadValue<Vector2>();
        if (levetta.magnitude < 0.3f)
        {
            levettaANeutro = true;
        }
        else if (levettaANeutro && levetta.magnitude > 0.7f && Time.time >= prossimoCambioPossibile)
        {
            levettaANeutro = false;
            CambiaBersaglio(levetta.normalized);
        }
    }

    // Sceglie il nemico più vicino al centro della visuale; se nessuno è davanti, il più vicino.
    Bersaglio ScegliPiuCentrale()
    {
        Vector3 avanti = DirezioneDiRiferimento();
        Bersaglio migliore = null;
        float migliorAngolo = float.MaxValue;
        Bersaglio piuVicino = null;
        float minimaDistanza = float.MaxValue;

        foreach (Bersaglio b in FindObjectsByType<Bersaglio>(FindObjectsSortMode.None))
        {
            if (!Valido(b, distanzaMassimaAggancio)) continue;

            Vector3 verso = b.transform.position - transform.position;
            verso.y = 0f;
            float distanza = verso.magnitude;
            if (distanza < minimaDistanza)
            {
                minimaDistanza = distanza;
                piuVicino = b;
            }

            float angolo = Vector3.Angle(avanti, verso);
            if (angolo <= 90f && angolo < migliorAngolo)
            {
                migliorAngolo = angolo;
                migliore = b;
            }
        }
        return migliore != null ? migliore : piuVicino;
    }

    // Passa al nemico che sullo schermo sta nella direzione del gesto (x = destra, y = in alto), partendo da quello
    // agganciato. Fra quelli nella direzione giusta vince il più allineato e vicino sullo schermo.
    void CambiaBersaglio(Vector2 direzione)
    {
        prossimoCambioPossibile = Time.time + pausaCambioBersaglio;
        var camera = Camera.main;
        if (camera == null) return;
        Vector3 daQui = camera.WorldToScreenPoint(Attuale.transform.position);
        if (daQui.z <= 0f) return;

        Bersaglio scelto = null;
        float migliorPunteggio = float.MaxValue;
        foreach (Bersaglio b in FindObjectsByType<Bersaglio>(FindObjectsSortMode.None))
        {
            if (b == Attuale || !Valido(b, distanzaMassimaAggancio)) continue;
            Vector3 la = camera.WorldToScreenPoint(b.transform.position);
            if (la.z <= 0f) continue;   // dietro la camera
            Vector2 verso = new Vector2(la.x - daQui.x, la.y - daQui.y);
            if (verso.sqrMagnitude < 1f) continue;
            float angolo = Vector2.Angle(direzione, verso);
            if (angolo > angoloGesto) continue;
            // Più conta essere allineati che essere vicini: ogni grado pesa come 8 pixel.
            float punteggio = verso.magnitude + angolo * 8f;
            if (punteggio < migliorPunteggio)
            {
                migliorPunteggio = punteggio;
                scelto = b;
            }
        }

        if (scelto != null) Attuale = scelto;
    }

    bool Valido(Bersaglio b, float distanzaMassima)
    {
        if (b == null || !b.isActiveAndEnabled || b.Morto) return false;
        return Vector3.Distance(transform.position, b.transform.position) <= distanzaMassima;
    }

    Vector3 DirezioneDiRiferimento()
    {
        Vector3 avanti = Camera.main != null ? Camera.main.transform.forward : transform.forward;
        avanti.y = 0f;
        return avanti.sqrMagnitude > 0.0001f ? avanti.normalized : transform.forward;
    }

    // Quadratino rosso sopra il nemico agganciato.
    void OnGUI()
    {
        if (HudGioco.Attivo) return; // in partita il segno lo disegna HudGioco
        if (Attuale == null || Camera.main == null) return;

        Vector3 schermo = Camera.main.WorldToScreenPoint(Attuale.transform.position + Vector3.up * 1.4f);
        if (schermo.z <= 0f) return;

        Color prima = GUI.color;
        GUI.color = new Color(0.9f, 0.15f, 0.15f);
        GUI.DrawTexture(new Rect(schermo.x - 6f, Screen.height - schermo.y - 6f, 12f, 12f), Texture2D.whiteTexture);
        GUI.color = prima;
    }
}
