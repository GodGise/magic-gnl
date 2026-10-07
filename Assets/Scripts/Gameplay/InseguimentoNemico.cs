using UnityEngine;

// Vista e inseguimento per un nemico (per ora l'orco di prova in piazza a Villaggio Lago Nero).
// A cosa serve: il nemico sta fermo al suo posto e si guarda intorno piano piano. Si accorge del giocatore
// solo quando questo entra nel suo campo visivo: davanti a lui (dentro "Angolo Visivo"), abbastanza vicino
// ("Distanza Vista") e senza muri in mezzo. Allora gli compare un "!" sopra la testa, insegue il giocatore
// e lo attacca (gli attacchi sono quelli di Bersaglio). Se lo perde di vista per qualche secondo, o se
// il giocatore scappa troppo lontano, torna al suo posto. Se viene colpito alle spalle si accorge lo stesso.
// Finché è ignaro (non ha visto il giocatore) può subire l'esecuzione furtiva alle spalle (vedi GiocatoreControllo):
// resta immobile, si inarca all'indietro e muore al taglio.
// Come montarlo: sullo stesso oggetto di un Bersaglio (per esempio un cilindro con Bersaglio).
// Selezionando il nemico, nella vista Scene si vede il cono giallo del suo campo visivo.
[RequireComponent(typeof(Bersaglio))]
public class InseguimentoNemico : MonoBehaviour
{
    [Header("Vista")]
    [Tooltip("Ampiezza del campo visivo, in gradi (davanti al nemico).")]
    [SerializeField] float angoloVisivo = 110f;
    [Tooltip("Fin dove vede il giocatore, in metri.")]
    [SerializeField] float distanzaVista = 18f;
    [Tooltip("Mentre è fermo si guarda intorno: di quanti gradi gira a destra e a sinistra (0 = guarda sempre dritto).")]
    [SerializeField] float guardaIntorno = 50f;
    [Tooltip("Quanti secondi ci mette a girare la testa da un lato all'altro.")]
    [SerializeField] float durataSguardo = 4f;

    [Header("Inseguimento")]
    [SerializeField] float velocitaInseguimento = 3.5f;
    [Tooltip("Si ferma a questa distanza dal giocatore, abbastanza vicino per colpirlo.")]
    [SerializeField] float distanzaAttacco = 1.8f;
    [Tooltip("Se non vede il giocatore per questi secondi, rinuncia e torna al suo posto.")]
    [SerializeField] float secondiPerPerderlo = 4f;
    [Tooltip("Oltre questa distanza dal suo posto rinuncia comunque.")]
    [SerializeField] float distanzaMassimaDalPosto = 40f;
    [SerializeField] float velocitaRitorno = 2.5f;
    [Tooltip("Gradi al secondo con cui si gira.")]
    [SerializeField] float velocitaRotazione = 360f;

    enum Stato { Fermo, Insegue, Torna }
    Stato stato = Stato.Fermo;

    Bersaglio bersaglio;
    GiocatoreControllo giocatore;
    Collider corpo;
    Vector3 posto;
    Quaternion sguardoIniziale;
    float ultimaVolta;      // ultima volta che ha visto il giocatore
    float tempoSguardo;
    TextMesh esclamativo;
    float esclamativoFino;
    bool inEsecuzione;

    // Ignaro: vivo, non sta inseguendo il giocatore e non sta già subendo un'esecuzione. Solo così si può giustiziare.
    public bool Ignaro => !bersaglio.Morto && stato != Stato.Insegue && !inEsecuzione;
    // Letto da AnimazioneUmanoide: durante l'esecuzione il nemico si inarca all'indietro.
    public bool InEsecuzione => inEsecuzione;

    void Awake()
    {
        bersaglio = GetComponent<Bersaglio>();
        corpo = GetComponent<Collider>();
        posto = transform.position;
        sguardoIniziale = transform.rotation;
        bersaglio.attaccaIlGiocatore = false;   // attacca solo dopo averlo visto
        bersaglio.Colpito += SiAccorge;
        CreaEsclamativo();
    }

    void Start()
    {
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
    }

    void OnDestroy()
    {
        if (bersaglio != null) bersaglio.Colpito -= SiAccorge;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        AggiornaEsclamativo();
        if (inEsecuzione) return;   // immobile mentre viene giustiziato

        // Da morto non fa niente; quando rinasce torna tranquillo al suo posto.
        if (bersaglio.Morto)
        {
            stato = Stato.Torna;   // quando rinasce torna al suo posto
            bersaglio.attaccaIlGiocatore = false;
            return;
        }
        // Giocatore morto o svanito nell'ombra: chi lo inseguiva lo perde subito e torna al suo posto.
        if (giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto || giocatore.Invisibile)
        {
            if (stato == Stato.Insegue) Rinuncia();
        }

        switch (stato)
        {
            case Stato.Fermo:
                GuardaIntorno(dt);
                if (VedeGiocatore()) SiAccorge();
                break;

            case Stato.Insegue:
                Insegui(dt);
                break;

            case Stato.Torna:
                if (VedeGiocatore()) { SiAccorge(); break; }
                if (CamminaVerso(posto, velocitaRitorno, 0.3f, dt))
                {
                    stato = Stato.Fermo;
                    tempoSguardo = 0f;
                }
                break;
        }
    }

    // ---------- Vista ----------

    bool VedeGiocatore()
    {
        if (giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        if (giocatore.Invisibile) return false;   // svanito nell'ombra (amuleto Ultimo respiro)

        Vector3 occhi = transform.position + Vector3.up * 0.8f;
        Vector3 bersaglioVista = giocatore.transform.position + Vector3.up * 0.5f;
        Vector3 verso = bersaglioVista - occhi;
        // La furtività del giocatore (armature e amuleti del Ladro, vedi Statistiche) accorcia la vista.
        float vista = distanzaVista;
        if (giocatore.Statistiche != null) vista *= 1f - giocatore.Statistiche.Furtivita / 100f;
        if (verso.magnitude > vista) return false;

        Vector3 orizzontale = new Vector3(verso.x, 0f, verso.z);
        if (Vector3.Angle(transform.forward, orizzontale) > angoloVisivo * 0.5f) return false;

        // Niente muri in mezzo: il primo oggetto incontrato (a parte sé stesso) deve essere il giocatore.
        foreach (RaycastHit colpo in Physics.RaycastAll(occhi, verso.normalized, verso.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            if (colpo.collider == corpo || colpo.collider.transform.IsChildOf(transform)) continue;
            if (colpo.collider.transform.IsChildOf(giocatore.transform)) continue;
            if (colpo.distance < verso.magnitude - 0.6f) return false;
        }
        return true;
    }

    void GuardaIntorno(float dt)
    {
        if (guardaIntorno <= 0f || bersaglio.StaAttaccando) return;
        tempoSguardo += dt;
        float angolo = Mathf.Sin(tempoSguardo / Mathf.Max(0.1f, durataSguardo) * Mathf.PI) * guardaIntorno;
        Quaternion obiettivo = sguardoIniziale * Quaternion.Euler(0f, angolo, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, obiettivo, velocitaRotazione * 0.25f * dt);
    }

    // Si accorge del giocatore: "!" sopra la testa, inizia a inseguire e può attaccare.
    void SiAccorge()
    {
        if (bersaglio.Morto || inEsecuzione) return;
        if (stato != Stato.Insegue)
        {
            esclamativoFino = Time.time + 1.2f;
            Suoni.Suona(Suono.Negato, transform.position + Vector3.up, 0.6f, 0.6f);
        }
        stato = Stato.Insegue;
        ultimaVolta = Time.time;
        bersaglio.attaccaIlGiocatore = true;
    }

    // ---------- Esecuzione furtiva ----------

    // Chiamato da GiocatoreControllo quando inizia l'esecuzione: il nemico resta fermo e non attacca.
    public void IniziaEsecuzione()
    {
        inEsecuzione = true;
        bersaglio.attaccaIlGiocatore = false;
    }

    // Il taglio: il nemico muore sul colpo, poi rinasce tranquillo al suo posto come dopo una morte normale.
    public void Giustizia(Vector3 daDove)
    {
        bersaglio.RiceviColpo(bersaglio.VitaMassima * 10f, daDove);
        inEsecuzione = false;
        stato = Stato.Torna;
    }

    void Rinuncia()
    {
        stato = Stato.Torna;
        bersaglio.attaccaIlGiocatore = false;
    }

    // ---------- Movimento ----------

    void Insegui(float dt)
    {
        if (VedeGiocatore()) ultimaVolta = Time.time;
        bool troppoLontano = Vector3.Distance(transform.position, posto) > distanzaMassimaDalPosto;
        if (Time.time - ultimaVolta > secondiPerPerderlo || troppoLontano)
        {
            Rinuncia();
            return;
        }

        // Mentre carica o sferra un colpo resta fermo (il preavviso rosso di Bersaglio).
        if (bersaglio.StaAttaccando) return;
        CamminaVerso(giocatore.transform.position, velocitaInseguimento, distanzaAttacco, dt);
    }

    // Cammina verso un punto, girando attorno agli ostacoli semplici. Restituisce true quando è arrivato.
    bool CamminaVerso(Vector3 meta, float velocita, float distanzaArrivo, float dt)
    {
        Vector3 verso = meta - transform.position;
        verso.y = 0f;
        float distanza = verso.magnitude;
        if (distanza <= distanzaArrivo)
        {
            // Tornato al posto: si rigira come all'inizio. Vicino al giocatore: resta girato verso di lui.
            if (stato == Stato.Torna)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, sguardoIniziale, velocitaRotazione * dt);
                return Quaternion.Angle(transform.rotation, sguardoIniziale) < 2f;
            }
            Ruota(verso, dt);
            return true;
        }

        Vector3 direzione = verso / distanza;
        float passo = Mathf.Min(velocita * dt, distanza - distanzaArrivo);

        // Se davanti c'è un ostacolo prova a scansarlo di lato.
        foreach (float deviazione in new[] { 0f, 35f, -35f, 70f, -70f })
        {
            Vector3 prova = Quaternion.Euler(0f, deviazione, 0f) * direzione;
            if (Libero(prova, passo + 0.3f))
            {
                Ruota(prova, dt);
                transform.position += prova * passo;
                return false;
            }
        }
        Ruota(direzione, dt);   // bloccato: almeno resta girato verso la meta
        return false;
    }

    bool Libero(Vector3 direzione, float distanza)
    {
        Vector3 basso = transform.position + Vector3.up * -0.5f;
        Vector3 alto = transform.position + Vector3.up * 0.5f;
        foreach (RaycastHit colpo in Physics.CapsuleCastAll(basso, alto, 0.4f, direzione, distanza, ~0, QueryTriggerInteraction.Ignore))
        {
            if (colpo.collider == corpo || colpo.collider.transform.IsChildOf(transform)) continue;
            if (giocatore != null && colpo.collider.transform.IsChildOf(giocatore.transform)) continue;
            if (colpo.distance <= 0f) continue;   // già a contatto (per esempio il pavimento): non blocca
            return false;
        }
        return true;
    }

    void Ruota(Vector3 direzione, float dt)
    {
        direzione.y = 0f;
        if (direzione.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direzione), velocitaRotazione * dt);
    }

    // ---------- "!" sopra la testa ----------

    void CreaEsclamativo()
    {
        var oggetto = new GameObject("Allarme");
        oggetto.transform.SetParent(transform, false);
        oggetto.transform.localPosition = Vector3.up * 1.5f;
        esclamativo = oggetto.AddComponent<TextMesh>();
        // Il carattere di base di Unity: senza, il testo non si vedrebbe.
        Font carattere = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        esclamativo.font = carattere;
        oggetto.GetComponent<MeshRenderer>().sharedMaterial = carattere.material;
        esclamativo.text = "!";
        esclamativo.fontSize = 64;
        esclamativo.characterSize = 0.05f;
        esclamativo.anchor = TextAnchor.MiddleCenter;
        esclamativo.color = new Color(1f, 0.2f, 0.1f);
        oggetto.SetActive(false);
    }

    void AggiornaEsclamativo()
    {
        bool mostra = Time.time < esclamativoFino && !bersaglio.Morto;
        if (esclamativo.gameObject.activeSelf != mostra) esclamativo.gameObject.SetActive(mostra);
        if (mostra && Camera.main != null)
            esclamativo.transform.rotation = Quaternion.LookRotation(esclamativo.transform.position - Camera.main.transform.position);
    }

    // Il cono del campo visivo, visibile nella vista Scene selezionando il nemico.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.8f);
        Vector3 occhi = transform.position + Vector3.up * 0.8f;
        Vector3 sinistra = Quaternion.Euler(0f, -angoloVisivo * 0.5f, 0f) * transform.forward * distanzaVista;
        Vector3 destra = Quaternion.Euler(0f, angoloVisivo * 0.5f, 0f) * transform.forward * distanzaVista;
        Gizmos.DrawLine(occhi, occhi + sinistra);
        Gizmos.DrawLine(occhi, occhi + destra);
        Vector3 precedente = occhi + sinistra;
        for (int i = 1; i <= 12; i++)
        {
            Vector3 punto = occhi + Quaternion.Euler(0f, -angoloVisivo * 0.5f + angoloVisivo * i / 12f, 0f) * transform.forward * distanzaVista;
            Gizmos.DrawLine(precedente, punto);
            precedente = punto;
        }
    }
}
