using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Camera in terza persona che gira attorno al personaggio.
// A cosa serve: segue il giocatore; si ruota con il mouse o con la levetta destra del pad.
// Il cursore viene bloccato al centro: Esc lo libera, un clic nella finestra di gioco lo riblocca.
// Con l'aggancio del bersaglio attivo la camera si gira da sola verso il nemico agganciato
// e il mouse non la ruota (la rotellina serve a cambiare nemico).
// Collisione: la camera non attraversa muri, strutture e pavimento. Se qualcosa si mette tra il
// personaggio e la camera, questa si avvicina al personaggio in modo graduale (parte un po' prima
// del muro); quando lo spazio torna libero si allontana piano piano fino alla distanza normale.
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
    [Tooltip("Controllo più largo che fa partire l'avvicinamento un po' prima del muro, così la camera scivola invece di scattare. Più è grande, prima inizia ad avvicinarsi (ma negli spazi stretti resta più vicina al personaggio).")]
    [SerializeField] float raggioAnticipo = 0.7f;
    [Tooltip("Quanto velocemente la camera si avvicina al personaggio quando trova un ostacolo. Più è basso, più l'avvicinamento è dolce.")]
    [SerializeField] float velocitaAvvicinamento = 8f;
    [Tooltip("Quanto velocemente la camera torna indietro quando lo spazio si libera.")]
    [SerializeField] float velocitaRitorno = 6f;
    [Tooltip("Sotto questa distanza dal personaggio la camera inizia ad alzarsi sopra la sua testa.")]
    [SerializeField] float distanzaSollevamento = 2f;
    [Tooltip("Di quanto si alza la camera quando è schiacciata contro un muro (in metri).")]
    [SerializeField] float altezzaSollevamento = 1.6f;
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
    Collider corpoPersonaggio;
    readonly List<Renderer> partiNascoste = new List<Renderer>();
    bool personaggioNascosto;

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
            corpoPersonaggio = bersaglio.GetComponent<Collider>();
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

        // Due controlli: quello "duro" dice dove si trova davvero il muro (limite assoluto),
        // quello "di anticipo" è più largo e vede il muro prima, così l'avvicinamento parte in anticipo.
        float distanzaDura = DistanzaLibera(fuoco, indietro, raggioCollisione);
        float distanzaObiettivo = Mathf.Min(distanzaDura, DistanzaLibera(fuoco, indietro, raggioAnticipo));

        // La camera si avvicina (o si allontana) in modo graduale verso la distanza obiettivo.
        float velocita = distanzaObiettivo < distanzaAttuale ? velocitaAvvicinamento : velocitaRitorno;
        float fattoreDistanza = 1f - Mathf.Exp(-velocita * Time.deltaTime);
        distanzaAttuale = Mathf.Lerp(distanzaAttuale, distanzaObiettivo, fattoreDistanza);

        // Se l'avvicinamento è troppo lento per un ostacolo improvviso, il limite duro vince:
        // la camera non attraversa mai il muro.
        distanzaAttuale = Mathf.Min(distanzaAttuale, distanzaDura);

        Vector3 posizione = fuoco + indietro * distanzaAttuale;

        // Più la camera è vicina al personaggio (muro alle spalle), più si alza sopra la sua testa,
        // invece di finirgli dentro. Prima controlla che sopra non ci sia un soffitto.
        float vicinanza = 1f - Mathf.InverseLerp(distanzaMinima, distanzaSollevamento, distanzaAttuale);
        if (vicinanza > 0f)
        {
            posizione += Vector3.up * PrimoOstacolo(posizione, Vector3.up, altezzaSollevamento * vicinanza, raggioCollisione);
        }

        // La camera guarda sempre il personaggio (quando non è alzata è la stessa rotazione di prima).
        transform.SetPositionAndRotation(posizione, Quaternion.LookRotation(fuoco - posizione));

        // Se nonostante tutto la camera finisce addosso al personaggio, lo nasconde invece di
        // mostrare l'interno del suo corpo.
        NascondiPersonaggio(DentroIlPersonaggio(posizione));
    }

    // Quanto può allontanarsi la camera dal personaggio, in linea retta all'indietro, senza toccare ostacoli.
    // "raggio" è lo spessore della camera nel controllo.
    float DistanzaLibera(Vector3 fuoco, Vector3 indietro, float raggio)
    {
        return Mathf.Max(PrimoOstacolo(fuoco, indietro, distanza, raggio), distanzaMinima);
    }

    // Distanza dal primo ostacolo partendo da "da" verso "direzione" (o distanzaMassima se non c'è niente).
    // Muri, pavimento, strutture e nemici fermano la camera; il personaggio stesso no.
    float PrimoOstacolo(Vector3 da, Vector3 direzione, float distanzaMassima, float raggio)
    {
        if (distanzaMassima <= 0f) return 0f;

        float libera = distanzaMassima;
        RaycastHit[] colpi = Physics.SphereCastAll(da, raggio, direzione, distanzaMassima, stratiOstacoli, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit colpo in colpi)
        {
            if (colpo.collider.transform.IsChildOf(bersaglio)) continue;
            libera = Mathf.Min(libera, colpo.distance);
        }
        return libera;
    }

    // Vero se la camera è dentro (o quasi) il corpo del personaggio.
    bool DentroIlPersonaggio(Vector3 posizione)
    {
        if (corpoPersonaggio == null) return false;
        Bounds corpo = corpoPersonaggio.bounds;
        corpo.Expand(0.6f); // margine: anche a pochi centimetri dal corpo si vedrebbe l'interno
        return corpo.Contains(posizione);
    }

    // Nasconde le parti visibili del personaggio e, quando la camera si allontana, riaccende solo
    // quelle che aveva spento (le parti già spente, come la capsula sotto la figura umana, restano spente).
    void NascondiPersonaggio(bool nascondi)
    {
        if (nascondi == personaggioNascosto) return;
        personaggioNascosto = nascondi;

        if (nascondi)
        {
            partiNascoste.Clear();
            foreach (Renderer parte in bersaglio.GetComponentsInChildren<Renderer>())
            {
                if (!parte.enabled) continue;
                parte.enabled = false;
                partiNascoste.Add(parte);
            }
        }
        else
        {
            foreach (Renderer parte in partiNascoste)
            {
                if (parte != null) parte.enabled = true;
            }
            partiNascoste.Clear();
        }
    }

    static void BloccaCursore(bool blocca)
    {
        Cursor.lockState = blocca ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !blocca;
    }
}
