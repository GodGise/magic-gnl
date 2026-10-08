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
// Co-op: vede e insegue il giocatore più vicino fra quelli che vede (vedi ObiettiviNemici). Pensa solo sul PC
// di chi ospita; sugli altri PC riceve da MondoRete solo se sta inseguendo, se è sotto esecuzione e il "!".
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
    IObiettivoNemico giocatore;   // il giocatore che sta inseguendo (o l'ultimo visto)
    Collider corpo;
    Vector3 posto;
    Quaternion sguardoIniziale;
    float ultimaVolta;      // ultima volta che ha visto il giocatore
    float tempoSguardo;
    TextMesh esclamativo;
    float esclamativoFino;
    bool inEsecuzione;
    float fineEsecuzione;          // sicurezza: oltre questo momento l'esecuzione finisce comunque (co-op: chi la faceva è uscito)
    float esecuzioneLocaleFino;    // co-op, chi non ospita: la sta facendo il giocatore di questo PC
    const float DurataMassimaEsecuzione = 5f;

    // Ignaro: vivo, non sta inseguendo il giocatore e non sta già subendo un'esecuzione. Solo così si può giustiziare.
    public bool Ignaro => !bersaglio.Morto && stato != Stato.Insegue && !inEsecuzione;
    // Letto da AnimazioneUmanoide: durante l'esecuzione il nemico si inarca all'indietro.
    public bool InEsecuzione => inEsecuzione;
    // Per MondoRete: vero mentre insegue qualcuno.
    public bool StaInseguendo => stato == Stato.Insegue;

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

    void OnDestroy()
    {
        if (bersaglio != null) bersaglio.Colpito -= SiAccorge;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        AggiornaEsclamativo();
        if (Rete.Ospite) return;    // in co-op, per chi non ospita, decide tutto l'host
        if (inEsecuzione && Time.time > fineEsecuzione)
        {
            // L'esecuzione non è mai finita (in co-op chi la faceva è uscito): il nemico torna libero.
            inEsecuzione = false;
            stato = Stato.Torna;
        }
        if (inEsecuzione) return;   // immobile mentre viene giustiziato

        // Da morto non fa niente; quando rinasce torna tranquillo al suo posto.
        if (bersaglio.Morto)
        {
            stato = Stato.Torna;   // quando rinasce torna al suo posto
            bersaglio.attaccaIlGiocatore = false;
            return;
        }
        // Giocatore morto o svanito nell'ombra: chi lo inseguiva lo perde subito (se ne vede un altro, insegue quello).
        if (!ObiettiviNemici.Valido(giocatore))
        {
            if (stato == Stato.Insegue && !CercaChiVede()) Rinuncia();
        }

        switch (stato)
        {
            case Stato.Fermo:
                GuardaIntorno(dt);
                if (CercaChiVede()) SiAccorge();
                break;

            case Stato.Insegue:
                Insegui(dt);
                break;

            case Stato.Torna:
                if (CercaChiVede()) { SiAccorge(); break; }
                if (CamminaVerso(posto, velocitaRitorno, 0.3f, dt))
                {
                    stato = Stato.Fermo;
                    tempoSguardo = 0f;
                }
                break;
        }
    }

    // ---------- Vista ----------

    // Fra tutti i giocatori, il più vicino che vede adesso: diventa quello da inseguire. Vero se ne ha trovato uno.
    bool CercaChiVede()
    {
        IObiettivoNemico migliore = null;
        float minima = float.MaxValue;
        foreach (var chi in ObiettiviNemici.Tutti)
        {
            if (!Vede(chi)) continue;
            float d = (chi.Corpo.position - transform.position).sqrMagnitude;
            if (d < minima) { minima = d; migliore = chi; }
        }
        if (migliore == null) return false;
        giocatore = migliore;
        bersaglio.Obiettivo = migliore;
        return true;
    }

    bool VedeGiocatore() => Vede(giocatore);

    bool Vede(IObiettivoNemico chi)
    {
        // Morto, oppure svanito nell'ombra (amuleto Ultimo respiro): non si vede.
        if (!ObiettiviNemici.Valido(chi)) return false;

        Vector3 occhi = transform.position + Vector3.up * 0.8f;
        Vector3 bersaglioVista = chi.Corpo.position + Vector3.up * 0.5f;
        Vector3 verso = bersaglioVista - occhi;
        // La furtività del giocatore (armature e amuleti del Ladro, vedi Statistiche) accorcia la vista.
        float vista = distanzaVista * (1f - chi.Furtivita / 100f);
        if (verso.magnitude > vista) return false;

        Vector3 orizzontale = new Vector3(verso.x, 0f, verso.z);
        if (Vector3.Angle(transform.forward, orizzontale) > angoloVisivo * 0.5f) return false;

        // Niente muri in mezzo: il primo oggetto incontrato (a parte sé stesso e i giocatori) deve essere il giocatore.
        foreach (RaycastHit colpo in Physics.RaycastAll(occhi, verso.normalized, verso.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            if (colpo.collider == corpo || colpo.collider.transform.IsChildOf(transform)) continue;
            if (colpo.collider.transform.IsChildOf(chi.Corpo) || ObiettiviNemici.EGiocatore(colpo.collider)) continue;
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
        if (Rete.Ospite) return;   // lo decide l'host (che manda il "!" con AllarmeDaRete)
        if (bersaglio.Morto || inEsecuzione) return;
        // Colpito alle spalle senza aver visto nessuno: insegue il giocatore più vicino.
        if (!ObiettiviNemici.Valido(giocatore) && !CercaChiVede())
        {
            giocatore = ObiettiviNemici.PiuVicino(transform.position);
            if (giocatore == null) return;
            bersaglio.Obiettivo = giocatore;
        }
        if (stato != Stato.Insegue)
        {
            MostraAllarme();
            MondoRete.InviaAllarme(bersaglio);
        }
        stato = Stato.Insegue;
        ultimaVolta = Time.time;
        bersaglio.attaccaIlGiocatore = true;
    }

    void MostraAllarme()
    {
        esclamativoFino = Time.time + 1.2f;
        Suoni.Suona(Suono.Negato, transform.position + Vector3.up, 0.6f, 0.6f);
    }

    // ---------- co-op: per chi non ospita (chiamati da MondoRete) ----------

    public void AllarmeDaRete() => MostraAllarme();

    public void StatoDaRete(bool insegue, bool sottoEsecuzione)
    {
        stato = insegue ? Stato.Insegue : Stato.Fermo;
        // Mentre il giocatore di questo PC lo sta giustiziando, la posa resta quella anche se l'host non lo sa ancora.
        inEsecuzione = sottoEsecuzione || Time.time < esecuzioneLocaleFino;
    }

    // ---------- Esecuzione furtiva ----------

    // Chiamato da GiocatoreControllo quando inizia l'esecuzione: il nemico resta fermo e non attacca.
    public void IniziaEsecuzione()
    {
        inEsecuzione = true;
        fineEsecuzione = Time.time + DurataMassimaEsecuzione;
        bersaglio.attaccaIlGiocatore = false;
        if (Rete.Ospite)
        {
            esecuzioneLocaleFino = Time.time + DurataMassimaEsecuzione;
            MondoRete.ChiediEsecuzione(bersaglio);
        }
    }

    // Il taglio: il nemico muore sul colpo, poi rinasce tranquillo al suo posto come dopo una morte normale.
    public void Giustizia(Vector3 daDove)
    {
        if (Rete.Ospite)
        {
            esecuzioneLocaleFino = 0f;
            MondoRete.ChiediGiustizia(bersaglio, daDove);
            return;
        }
        GiustiziaDa(Rete.MioId, daDove);
    }

    public void GiustiziaDa(ulong chi, Vector3 daDove)
    {
        bersaglio.RiceviColpoDa(chi, bersaglio.VitaMassima * 10f, daDove, false);
        inEsecuzione = false;
        stato = Stato.Torna;
    }

    void Rinuncia()
    {
        stato = Stato.Torna;
        bersaglio.attaccaIlGiocatore = false;
        bersaglio.Obiettivo = null;
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
        if (bersaglio.StaAttaccando || !ObiettiviNemici.Esiste(giocatore)) return;
        CamminaVerso(giocatore.Corpo.position, velocitaInseguimento, distanzaAttacco, dt);
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
            if (ObiettiviNemici.EGiocatore(colpo.collider)) continue;
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
