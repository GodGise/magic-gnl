using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Aggancio del bersaglio (lock-on), come in Elden Ring.
// A cosa serve: tiene camera e personaggio puntati su un nemico.
//   - Agganciare / sganciare: clic della rotellina del mouse, oppure premere la levetta destra del pad.
//   - Cambiare nemico: girare la rotellina (in avanti = nemico a destra, indietro = a sinistra),
//     oppure spostare di scatto la levetta destra del pad a destra o a sinistra.
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
    [SerializeField] float pausaCambioBersaglio = 0.2f;

    public Bersaglio Attuale { get; private set; }
    public bool Agganciato => Attuale != null;

    InputAction comandoAggancia, comandoRotellina, comandoLevettaPad;
    float prossimoCambioPossibile;
    bool levettaANeutro = true;

    void Awake()
    {
        comandoAggancia = new InputAction("Aggancia", InputActionType.Button);
        comandoAggancia.AddBinding("<Mouse>/middleButton");
        comandoAggancia.AddBinding("<Gamepad>/rightStickPress");

        comandoRotellina = new InputAction("CambiaBersaglioRotellina", InputActionType.Value);
        comandoRotellina.AddBinding("<Mouse>/scroll");

        comandoLevettaPad = new InputAction("CambiaBersaglioPad", InputActionType.Value);
        comandoLevettaPad.AddBinding("<Gamepad>/rightStick");
    }

    void OnEnable()
    {
        comandoAggancia.Enable();
        comandoRotellina.Enable();
        comandoLevettaPad.Enable();
    }

    void OnDisable()
    {
        comandoAggancia.Disable();
        comandoRotellina.Disable();
        comandoLevettaPad.Disable();
    }

    void OnDestroy()
    {
        comandoAggancia.Dispose();
        comandoRotellina.Dispose();
        comandoLevettaPad.Dispose();
    }

    void Update()
    {
        if (Attuale != null && !Valido(Attuale, distanzaSgancio)) Attuale = null;

        if (comandoAggancia.WasPressedThisFrame())
        {
            Attuale = Attuale != null ? null : ScegliPiuCentrale();
        }

        if (Attuale == null) return;

        float rotellina = comandoRotellina.ReadValue<Vector2>().y;
        if (Mathf.Abs(rotellina) > 0.01f && Time.time >= prossimoCambioPossibile)
            CambiaBersaglio(rotellina > 0f ? 1 : -1);

        float levetta = comandoLevettaPad.ReadValue<Vector2>().x;
        if (Mathf.Abs(levetta) < 0.3f)
        {
            levettaANeutro = true;
        }
        else if (levettaANeutro && Mathf.Abs(levetta) > 0.7f)
        {
            levettaANeutro = false;
            CambiaBersaglio(levetta > 0f ? 1 : -1);
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

    // verso = +1 passa al nemico più vicino sulla destra, -1 sulla sinistra.
    void CambiaBersaglio(int verso)
    {
        prossimoCambioPossibile = Time.time + pausaCambioBersaglio;

        Vector3 versoAttuale = Attuale.transform.position - transform.position;
        versoAttuale.y = 0f;

        Bersaglio scelto = null;
        float angoloScelto = float.MaxValue;

        foreach (Bersaglio b in FindObjectsByType<Bersaglio>(FindObjectsSortMode.None))
        {
            if (b == Attuale || !Valido(b, distanzaMassimaAggancio)) continue;

            Vector3 versoCandidato = b.transform.position - transform.position;
            versoCandidato.y = 0f;

            // Positivo = a destra del nemico attuale, visto dall'alto.
            float angolo = Vector3.SignedAngle(versoAttuale, versoCandidato, Vector3.up) * verso;
            if (angolo > 0f && angolo < angoloScelto)
            {
                angoloScelto = angolo;
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
