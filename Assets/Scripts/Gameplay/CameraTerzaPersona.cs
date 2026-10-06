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
// Strettoie (vedi PassaggioStretto): fra due pareti vicine la camera si porta piano piano dietro al
// personaggio, lungo il passaggio, e si abbassa un po', così non resta schiacciata contro i muri.
// Il mouse la può spostare, ma finché si è nella strettoia torna sempre dietro.
// Negli spazi stretti la camera "si assottiglia": il suo spessore nei controlli si riduce a quello che
// entra fra i muri, e il piano di taglio vicino (near clip) si accorcia, così non si vede dentro i muri.
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
    [Tooltip("Di quanto si alza al massimo la camera quando è schiacciata contro un muro (in metri).")]
    [SerializeField] float sollevamentoMassimo = 0.8f;
    [Tooltip("Vicino a un muro la camera guarda un punto davanti al personaggio, a questa distanza, invece della sua testa: così si vede sempre dove si va.")]
    [SerializeField] float distanzaSguardoAvanti = 3f;
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
    [Header("Strettoie")]
    [Tooltip("Quanto in fretta la camera si porta dietro al personaggio in una strettoia.")]
    [SerializeField] float velocitaAllineamentoStrettoia = 4f;
    [Tooltip("Inclinazione della camera nella strettoia, in gradi (più bassa = più dietro e meno dall'alto).")]
    [SerializeField] float inclinazioneStrettoia = 12f;
    [Tooltip("Spessore minimo della camera negli spazi stretti (metri). Sotto questo valore non si riduce.")]
    [SerializeField] float raggioMinimo = 0.06f;
    [Tooltip("Piano di taglio vicino più corto che la camera può usare negli spazi stretti (metri).")]
    [SerializeField] float tagliaVicinoMinimo = 0.03f;

    InputAction guardaMouse, guardaPad;
    float rotazioneOrizzontale;
    GiocatoreControllo giocatore;
    float inclinazione = 20f;
    float distanzaAttuale;
    AggancioBersaglio aggancio;
    Collider corpoPersonaggio;
    readonly List<Renderer> partiNascoste = new List<Renderer>();
    bool personaggioNascosto;
    Camera obiettivo;
    float tagliaVicinoNormale = 0.3f;
    readonly Collider[] toccati = new Collider[16];

    void Awake()
    {
        distanzaAttuale = distanza;
        obiettivo = GetComponent<Camera>();
        if (obiettivo != null) tagliaVicinoNormale = obiettivo.nearClipPlane;

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

        AllineaNellaStrettoia();

        Quaternion rotazione = Quaternion.Euler(inclinazione, rotazioneOrizzontale, 0f);
        Vector3 fuoco = bersaglio.position + Vector3.up * altezzaFuoco;
        Vector3 indietro = rotazione * Vector3.back;

        // Due controlli: quello "duro" dice dove si trova davvero il muro (limite assoluto),
        // quello "di anticipo" è più largo e vede il muro prima, così l'avvicinamento parte in anticipo.
        // Il controllo largo non conta i muri che tocca già alla partenza (per esempio un muro di lato
        // quando si cammina rasente): conta solo quelli che incontra andando davvero verso la camera.
        // Negli spazi stretti la sfera normale toccherebbe già i muri ai lati e la camera finirebbe
        // schiacciata dentro il personaggio o dentro i muri: si usa lo spessore che ci entra davvero.
        float raggio = RaggioCheEntra(fuoco, raggioCollisione);
        float pesoStretto = giocatore != null ? Mathf.InverseLerp(0.1f, 0.5f, giocatore.Strettoia) : 0f;
        float anticipo = Mathf.Lerp(raggioAnticipo, raggio, pesoStretto);
        AggiornaTaglioVicino(raggio);

        // Il limite duro non ha distanza minima: se il muro è più vicino, la camera resta prima del muro
        // (al massimo nasconde il personaggio), invece di finirci dentro.
        float distanzaDura = Mathf.Max(PrimoOstacolo(fuoco, indietro, distanza, raggio, true), 0.05f);
        float distanzaObiettivo = Mathf.Min(distanzaDura, DistanzaLibera(fuoco, indietro, anticipo, false));

        // La camera si avvicina (o si allontana) in modo graduale verso la distanza obiettivo.
        float velocita = distanzaObiettivo < distanzaAttuale ? velocitaAvvicinamento : velocitaRitorno;
        float fattoreDistanza = 1f - Mathf.Exp(-velocita * Time.deltaTime);
        distanzaAttuale = Mathf.Lerp(distanzaAttuale, distanzaObiettivo, fattoreDistanza);

        // Se l'avvicinamento è troppo lento per un ostacolo improvviso, il limite duro vince:
        // la camera non attraversa mai il muro.
        distanzaAttuale = Mathf.Min(distanzaAttuale, distanzaDura);

        Vector3 posizione = fuoco + indietro * distanzaAttuale;

        // Più la camera è vicina al personaggio (muro alle spalle), più si alza un poco sopra la sua testa,
        // invece di finirgli dentro. Prima controlla che sopra non ci sia un soffitto.
        float vicinanza = 1f - Mathf.InverseLerp(distanzaMinima, distanzaSollevamento, distanzaAttuale);
        if (vicinanza > 0f)
        {
            posizione += Vector3.up * PrimoOstacolo(posizione, Vector3.up, sollevamentoMassimo * vicinanza, raggio, true);
        }

        // Lontano dai muri la camera guarda la testa del personaggio (come prima). Più si avvicina a un
        // muro, più guarda un punto davanti a lui, così non punta mai dritta in giù e si vede dove si va.
        Vector3 avanti = rotazione * Vector3.forward;
        avanti.y = 0f;
        Vector3 puntoGuardato = fuoco + avanti.normalized * (distanzaSguardoAvanti * Mathf.Max(0f, vicinanza));
        transform.SetPositionAndRotation(posizione, Quaternion.LookRotation(puntoGuardato - posizione));

        // Se nonostante tutto la camera finisce addosso al personaggio, lo nasconde invece di
        // mostrare l'interno del suo corpo.
        NascondiPersonaggio(DentroIlPersonaggio(posizione));
    }

    // Quanto può allontanarsi la camera dal personaggio, in linea retta all'indietro, senza toccare ostacoli.
    // "raggio" è lo spessore della camera nel controllo.
    float DistanzaLibera(Vector3 fuoco, Vector3 indietro, float raggio, bool contaGiaToccati)
    {
        return Mathf.Max(PrimoOstacolo(fuoco, indietro, distanza, raggio, contaGiaToccati), distanzaMinima);
    }

    // Distanza dal primo ostacolo partendo da "da" verso "direzione" (o distanzaMassima se non c'è niente).
    // Muri, pavimento, strutture e nemici fermano la camera; il personaggio stesso no.
    // contaGiaToccati: se falso, ignora gli oggetti che la sfera tocca già alla partenza
    // (Unity li segnala a distanza zero, anche se non stanno nella direzione del controllo).
    float PrimoOstacolo(Vector3 da, Vector3 direzione, float distanzaMassima, float raggio, bool contaGiaToccati)
    {
        if (distanzaMassima <= 0f) return 0f;

        float libera = distanzaMassima;
        RaycastHit[] colpi = Physics.SphereCastAll(da, raggio, direzione, distanzaMassima, stratiOstacoli, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit colpo in colpi)
        {
            if (colpo.collider.transform.IsChildOf(bersaglio)) continue;
            if (!contaGiaToccati && colpo.distance <= 0f) continue;
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

    // Lo spessore più grande (fino a "massimo") che la camera può avere in quel punto senza toccare
    // nessun ostacolo. Lontano dai muri è quello normale; fra due pareti vicine si riduce.
    float RaggioCheEntra(Vector3 punto, float massimo)
    {
        float raggio = massimo;
        while (raggio > raggioMinimo && TroppoVicino(punto, raggio)) raggio *= 0.75f;
        return Mathf.Max(raggio, raggioMinimo);
    }

    bool TroppoVicino(Vector3 punto, float raggio)
    {
        int n = Physics.OverlapSphereNonAlloc(punto, raggio, toccati, stratiOstacoli, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            if (!toccati[i].transform.IsChildOf(bersaglio)) return true;
        }
        return false;
    }

    // Il piano di taglio vicino deve stare dentro lo spessore della camera, altrimenti i suoi angoli
    // entrano nei muri vicini e si vede attraverso. Torna piano al valore normale quando c'è spazio.
    void AggiornaTaglioVicino(float raggio)
    {
        if (obiettivo == null) return;
        float voluto = Mathf.Clamp(raggio * 0.6f, tagliaVicinoMinimo, tagliaVicinoNormale);
        obiettivo.nearClipPlane = voluto < obiettivo.nearClipPlane
            ? voluto
            : Mathf.MoveTowards(obiettivo.nearClipPlane, voluto, Time.deltaTime);
    }

    // In una strettoia la camera si mette dietro al personaggio, guardando lungo il passaggio.
    void AllineaNellaStrettoia()
    {
        if (giocatore == null) giocatore = bersaglio.GetComponent<GiocatoreControllo>();
        if (giocatore == null || (aggancio != null && aggancio.Agganciato)) return;

        // Pesa quanto è stretto: niente nei passaggi larghi, pieno nella strettoia vera.
        float peso = Mathf.InverseLerp(0.2f, 0.6f, giocatore.Strettoia);
        if (peso <= 0f) return;

        float morbidezza = (1f - Mathf.Exp(-velocitaAllineamentoStrettoia * Time.deltaTime)) * peso;
        rotazioneOrizzontale = Mathf.LerpAngle(rotazioneOrizzontale, bersaglio.eulerAngles.y, morbidezza);
        inclinazione = Mathf.Lerp(inclinazione, inclinazioneStrettoia, morbidezza);
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
