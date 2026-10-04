using UnityEngine;
using UnityEngine.InputSystem;

// Camera in terza persona che gira attorno al personaggio.
// A cosa serve: segue il giocatore; si ruota con il mouse o con la levetta destra del pad.
// Il cursore viene bloccato al centro: Esc lo libera, un clic nella finestra di gioco lo riblocca.
// Con l'aggancio del bersaglio attivo la camera si gira da sola verso il nemico agganciato
// e il mouse non la ruota (la rotellina serve a cambiare nemico).
// Collisione: la camera non attraversa muri, strutture e pavimento. Se qualcosa si mette tra il
// personaggio e la camera, questa si avvicina al personaggio; quando lo spazio torna libero
// si allontana piano piano fino alla distanza normale.
// Come montarlo: sulla Main Camera, trascinando il personaggio nel campo "Bersaglio".
public class CameraTerzaPersona : MonoBehaviour
{
    public Transform bersaglio;
    [SerializeField] float distanza = 6f;
    [Header("Collisione")]
    [Tooltip("Spessore della camera quando controlla gli ostacoli. Più è grande, più resta lontana dai muri.")]
    [SerializeField] float raggioCollisione = 0.3f;
    [Tooltip("Distanza minima dal personaggio, anche con un muro subito dietro di lui.")]
    [SerializeField] float distanzaMinima = 0.4f;
    [Tooltip("Quanto velocemente la camera torna indietro quando lo spazio si libera.")]
    [SerializeField] float velocitaRitorno = 6f;
    [Tooltip("Quali strati contano come ostacoli per la camera (di base tutti).")]
    [SerializeField] LayerMask stratiOstacoli = ~0;
    [Header("Vista")]
    [SerializeField] float altezzaFuoco = 0.8f;
    [SerializeField] float sensibilitaMouse = 0.12f;
    [SerializeField] float sensibilitaPad = 150f;
    [SerializeField] float inclinazioneMinima = -20f;
    [SerializeField] float inclinazioneMassima = 60f;
    [Tooltip("Inclinazione della camera mentre si è agganciati a un nemico.")]
    [SerializeField] float inclinazioneAggancio = 15f;
    [Tooltip("Quanto velocemente la camera si gira verso il nemico agganciato.")]
    [SerializeField] float velocitaAggancio = 10f;

    InputAction guardaMouse, guardaPad;
    float rotazioneOrizzontale;
    float inclinazione = 20f;
    float distanzaAttuale;
    AggancioBersaglio aggancio;

    void Awake()
    {
        distanzaAttuale = distanza;

        guardaMouse = new InputAction("GuardaMouse", InputActionType.Value);
        guardaMouse.AddBinding("<Mouse>/delta");
        guardaPad = new InputAction("GuardaPad", InputActionType.Value);
        guardaPad.AddBinding("<Gamepad>/rightStick");

        if (bersaglio != null)
        {
            rotazioneOrizzontale = bersaglio.eulerAngles.y;
            aggancio = bersaglio.GetComponent<AggancioBersaglio>();
        }
    }

    void OnEnable()
    {
        guardaMouse.Enable();
        guardaPad.Enable();
        BloccaCursore(true);
    }

    void OnDisable()
    {
        guardaMouse.Disable();
        guardaPad.Disable();
        BloccaCursore(false);
    }

    void OnDestroy()
    {
        guardaMouse.Dispose();
        guardaPad.Dispose();
    }

    void LateUpdate()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) BloccaCursore(false);
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) BloccaCursore(true);

        if (bersaglio == null) return;

        if (aggancio != null && aggancio.Agganciato)
        {
            // Agganciati: la camera guarda dal giocatore verso il nemico.
            Vector3 verso = aggancio.Attuale.transform.position - bersaglio.position;
            verso.y = 0f;
            if (verso.sqrMagnitude > 0.0001f)
            {
                float morbidezza = 1f - Mathf.Exp(-velocitaAggancio * Time.deltaTime);
                float rotazioneVerso = Quaternion.LookRotation(verso).eulerAngles.y;
                rotazioneOrizzontale = Mathf.LerpAngle(rotazioneOrizzontale, rotazioneVerso, morbidezza);
                inclinazione = Mathf.Lerp(inclinazione, inclinazioneAggancio, morbidezza);
            }
        }
        else
        {
            Vector2 mouse = Cursor.lockState == CursorLockMode.Locked ? guardaMouse.ReadValue<Vector2>() * sensibilitaMouse : Vector2.zero;
            Vector2 pad = guardaPad.ReadValue<Vector2>() * (sensibilitaPad * Time.deltaTime);

            rotazioneOrizzontale += mouse.x + pad.x;
            inclinazione = Mathf.Clamp(inclinazione - (mouse.y + pad.y), inclinazioneMinima, inclinazioneMassima);
        }

        Quaternion rotazione = Quaternion.Euler(inclinazione, rotazioneOrizzontale, 0f);
        Vector3 fuoco = bersaglio.position + Vector3.up * altezzaFuoco;
        Vector3 indietro = rotazione * Vector3.back;

        // Se c'è un ostacolo la camera si avvicina subito (così non lo attraversa mai);
        // se lo spazio è libero torna alla distanza normale con un movimento morbido.
        float distanzaLibera = DistanzaLibera(fuoco, indietro);
        if (distanzaLibera < distanzaAttuale)
        {
            distanzaAttuale = distanzaLibera;
        }
        else
        {
            float morbidezza = 1f - Mathf.Exp(-velocitaRitorno * Time.deltaTime);
            distanzaAttuale = Mathf.Lerp(distanzaAttuale, distanzaLibera, morbidezza);
        }

        transform.SetPositionAndRotation(fuoco + indietro * distanzaAttuale, rotazione);
    }

    // Quanto può allontanarsi la camera dal personaggio, in linea retta all'indietro, senza toccare ostacoli.
    float DistanzaLibera(Vector3 fuoco, Vector3 indietro)
    {
        float libera = distanza;
        RaycastHit[] colpi = Physics.SphereCastAll(fuoco, raggioCollisione, indietro, distanza, stratiOstacoli, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit colpo in colpi)
        {
            // Il personaggio stesso e i nemici non spingono la camera.
            if (colpo.collider.transform.IsChildOf(bersaglio)) continue;
            if (colpo.collider.GetComponentInParent<Bersaglio>() != null) continue;
            libera = Mathf.Min(libera, colpo.distance);
        }
        return Mathf.Max(libera, distanzaMinima);
    }

    static void BloccaCursore(bool blocca)
    {
        Cursor.lockState = blocca ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !blocca;
    }
}
