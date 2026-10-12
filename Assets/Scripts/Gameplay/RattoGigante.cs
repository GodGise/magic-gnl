using UnityEngine;

// Il ratto gigante (quest 3, il granaio di Hobb): un nemico piccolo e veloce che attacca a balzi.
// Come combatte: si avvicina fino a qualche metro e gira intorno al giocatore; quando è il suo momento si acquatta
// (si schiaccia e diventa rosso: è il preavviso), balza addosso al giocatore mordendo e graffiando, poi indietreggia
// e ricomincia. Un attacco ogni "Intervallo Attacchi" secondi (4 da soli). Il danno è quello di Bersaglio
// ("Danno Attacco", 11 per il ratto) e passa da GiocatoreControllo: si para e si schiva come gli altri colpi.
// Vita, morte, aggancio e difficoltà co-op sono quelli di Bersaglio (il ratto ha "Attacca Il Giocatore" spento:
// gli attacchi li fa questo componente). Stordito o bloccato dagli incantesimi non si muove; rallentato va più piano.
// Come montarlo: sullo stesso oggetto del Bersaglio (un oggetto con un Collider, senza forma umana). Il comando
// "magic-gnl > Prologo: prepara la quest 3" crea tre ratti pronti nel granaio.
// Co-op: pensa solo sul PC di chi ospita (come gli altri nemici); il prologo per ora si gioca da soli.
[RequireComponent(typeof(Bersaglio))]
public class RattoGigante : MonoBehaviour
{
    [Tooltip("Secondi fra un attacco e il successivo (accorciati dalla difficoltà co-op).")]
    [SerializeField] float intervalloAttacchi = 4f;
    [Tooltip("Ritardo del primo attacco: diverso per ogni ratto, così non balzano tutti insieme.")]
    [SerializeField] float ritardoPrimoAttacco = 1.5f;
    [Tooltip("Secondi di preavviso: il ratto si acquatta e diventa rosso prima di balzare.")]
    [SerializeField] float preavviso = 0.55f;
    [Tooltip("Durata del balzo, in secondi.")]
    [SerializeField] float durataBalzo = 0.35f;
    [Tooltip("Altezza del balzo, in metri.")]
    [SerializeField] float altezzaBalzo = 0.7f;
    [Tooltip("Da quanto lontano può balzare, in metri.")]
    [SerializeField] float portataBalzo = 5f;
    [Tooltip("Distanza entro cui il morso colpisce, alla fine del balzo.")]
    [SerializeField] float portataMorso = 1.5f;
    [Tooltip("Di quanti metri indietreggia dopo il morso.")]
    [SerializeField] float indietreggia = 3f;
    [Tooltip("Distanza a cui aspetta il suo turno, girando intorno al giocatore.")]
    [SerializeField] float distanzaAttesa = 3.5f;
    [SerializeField] float velocita = 4.5f;
    [Tooltip("Altezza da terra del centro del ratto.")]
    [SerializeField] float altezzaDaTerra = 0.3f;

    enum Fase { Avvicina, Acquatta, Balza, Indietreggia }

    Bersaglio bersaglio;
    Renderer[] parti;
    Color[] colori;
    Vector3 scalaNormale;
    Fase fase = Fase.Avvicina;
    float inizioFase, prossimoAttacco;
    Vector3 partenzaBalzo, arrivoBalzo, versoIndietro;
    IObiettivoNemico preso;
    bool morsoDato;
    float girare = 1f;

    void Awake()
    {
        bersaglio = GetComponent<Bersaglio>();
        bersaglio.attaccaIlGiocatore = false;
        scalaNormale = transform.localScale;
        girare = Random.value < 0.5f ? -1f : 1f;
    }

    void OnEnable()
    {
        prossimoAttacco = Time.time + ritardoPrimoAttacco;
        Cambia(Fase.Avvicina);
    }

    void Start()
    {
        parti = GetComponentsInChildren<Renderer>();
        colori = new Color[parti.Length];
        for (int i = 0; i < parti.Length; i++) colori[i] = parti[i].material.color;
    }

    float Intervallo => intervalloAttacchi / DifficoltaCoop.FrequenzaAttacchi;

    void Update()
    {
        if (Rete.Ospite) return;
        if (bersaglio.Morto)
        {
            transform.localScale = scalaNormale;
            return;
        }
        var effetti = EffettiNemico.Di(bersaglio);
        float lento = effetti != null ? effetti.MoltiplicatoreVelocita : 1f;
        if (lento <= 0f && fase != Fase.Balza) return;   // stordito o bloccato

        if (preso == null || !ObiettiviNemici.Valido(preso)) preso = ObiettiviNemici.PiuVicino(transform.position);
        if (preso == null) return;
        Vector3 bersaglioPos = preso.Corpo.position;
        Vector3 verso = Piatto(bersaglioPos - transform.position);
        float distanza = verso.magnitude;
        float t = Time.time - inizioFase;

        switch (fase)
        {
            case Fase.Avvicina:
                Guarda(verso);
                // Si avvicina fino alla distanza di attesa e gira intorno al giocatore.
                Vector3 passo = Vector3.zero;
                if (distanza > distanzaAttesa + 0.5f) passo = verso.normalized;
                else if (distanza < distanzaAttesa - 1f) passo = -verso.normalized;
                passo += Vector3.Cross(Vector3.up, verso.normalized) * girare * 0.6f;
                Muovi(passo.normalized * velocita * lento * Time.deltaTime);
                if (Time.time >= prossimoAttacco && distanza <= portataBalzo) Cambia(Fase.Acquatta);
                break;

            case Fase.Acquatta:
                Guarda(verso);
                float schiaccia = Mathf.Clamp01(t / preavviso);
                transform.localScale = new Vector3(scalaNormale.x * (1f + 0.15f * schiaccia), scalaNormale.y * (1f - 0.35f * schiaccia), scalaNormale.z);
                Colora(Color.Lerp(Color.white, Color.red, schiaccia));
                if (t >= preavviso)
                {
                    transform.localScale = scalaNormale;
                    Colora(Color.red);
                    partenzaBalzo = transform.position;
                    // Atterra poco prima del giocatore, dove si trovava all'inizio del balzo (si può schivare).
                    Vector3 arrivo = bersaglioPos - verso.normalized * 0.7f;
                    arrivoBalzo = new Vector3(arrivo.x, partenzaBalzo.y, arrivo.z);
                    morsoDato = false;
                    Suoni.Suona(Suono.Fendente, transform.position, 0.6f, 1.6f);
                    Cambia(Fase.Balza);
                }
                break;

            case Fase.Balza:
                float k = Mathf.Clamp01(t / durataBalzo);
                Vector3 p = Vector3.Lerp(partenzaBalzo, arrivoBalzo, k);
                p.y = Appoggia(p).y + Mathf.Sin(k * Mathf.PI) * altezzaBalzo;
                if (!Bloccato(transform.position, p)) transform.position = p;
                if (!morsoDato && k >= 0.75f)
                {
                    morsoDato = true;
                    if (ObiettiviNemici.Esiste(preso) && !preso.Invisibile && Vector3.Distance(transform.position, preso.Corpo.position) <= portataMorso + 0.6f)
                        preso.ColpitoDaNemico(bersaglio);
                }
                if (k >= 1f)
                {
                    Colora(Color.white);
                    versoIndietro = -verso.normalized;
                    prossimoAttacco = Time.time + Intervallo - durataBalzo - preavviso;
                    Cambia(Fase.Indietreggia);
                }
                break;

            case Fase.Indietreggia:
                Guarda(verso);
                float durata = indietreggia / Mathf.Max(0.1f, velocita * 1.2f);
                Muovi(versoIndietro * velocita * 1.2f * lento * Time.deltaTime);
                if (t >= durata) { girare = -girare; Cambia(Fase.Avvicina); }
                break;
        }
    }

    void Cambia(Fase nuova)
    {
        fase = nuova;
        inizioFase = Time.time;
        if (nuova != Fase.Acquatta && nuova != Fase.Balza && parti != null) Colora(Color.white);
    }

    void Guarda(Vector3 verso)
    {
        if (verso.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(verso.normalized), 12f * Time.deltaTime);
    }

    void Muovi(Vector3 spostamento)
    {
        if (spostamento.sqrMagnitude < 1e-8f) return;
        Vector3 nuova = Appoggia(transform.position + spostamento);
        if (!Bloccato(transform.position, nuova)) transform.position = nuova;
    }

    // Vero se fra due punti c'è un muro o un oggetto solido (il ratto non lo attraversa).
    bool Bloccato(Vector3 da, Vector3 a)
    {
        Vector3 d = a - da;
        if (d.sqrMagnitude < 1e-6f) return false;
        foreach (var colpo in Physics.SphereCastAll(da, 0.25f, d.normalized, d.magnitude + 0.2f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (colpo.transform.IsChildOf(transform) || colpo.distance <= 0f) continue;
            if (colpo.collider.GetComponentInParent<GiocatoreControllo>() != null) continue;
            if (colpo.collider.GetComponentInParent<Bersaglio>() != null) continue;   // gli altri ratti
            if (colpo.normal.y > 0.6f) continue;   // pavimento
            return true;
        }
        return false;
    }

    // Tiene il ratto appoggiato al pavimento.
    Vector3 Appoggia(Vector3 punto)
    {
        Vector3 inizio = new Vector3(punto.x, transform.position.y + 1f, punto.z);
        float y = punto.y, migliore = float.MaxValue;
        foreach (var c in Physics.RaycastAll(inizio, Vector3.down, 3f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c.transform.IsChildOf(transform) || c.collider.GetComponentInParent<GiocatoreControllo>() != null) continue;
            if (c.collider.GetComponentInParent<Bersaglio>() != null) continue;
            if (c.distance < migliore) { migliore = c.distance; y = c.point.y + altezzaDaTerra; }
        }
        punto.y = y;
        return punto;
    }

    // Tinge il ratto (rosso del preavviso); bianco = colori normali.
    void Colora(Color tinta)
    {
        if (parti == null) return;
        for (int i = 0; i < parti.Length; i++)
            if (parti[i] != null) parti[i].material.color = tinta == Color.white ? colori[i] : Color.Lerp(colori[i], tinta, 0.7f);
    }

    static Vector3 Piatto(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
