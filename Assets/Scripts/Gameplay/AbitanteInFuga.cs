using UnityEngine;

// Un abitante di Villaggio Lago Nero che scappa durante la razzia (richiesta di Giuseppe, 14 ottobre).
// Cosa fa: corre da una casa all'altra lontano dagli orchi, gridando; dopo una corsa lunga ha il fiatone e rallenta
// per un attimo, e a volte inciampa: è lì che gli orchi lo prendono. Gli orchi vicini lo inseguono anche senza
// vederlo (è un'"esca", come la Bambola di ossa: vedi ObiettiviNemici) e lo attaccano con i loro colpi normali.
// Se lo uccidono cade a terra e resta lì per un po', poi sparisce. Se l'orco che lo inseguiva muore (lo uccide il
// giocatore), l'abitante è salvo: scappa verso il bordo del villaggio e sparisce.
// Non conta per la razzia (le ondate contano solo gli orchi) e nessuna quest dipende da lui.
// Come montarlo: non si monta a mano. Lo crea RazziaVillaggio durante la razzia (pochi alla volta, con ricambio).
// Aspetto provvisorio: la figura a blocchi (AspettoUmanoide) con un vestito a caso; il modello vero è nella lista
// per Nazar (abitanti).
// Il prologo si gioca da soli: l'abitante lo comanda solo chi decide il mondo.
public class AbitanteInFuga : MonoBehaviour, IEscaNemici
{
    [SerializeField] float vita = 40f;
    [SerializeField] float velocitaCorsa = 4.4f;
    [SerializeField] float velocitaFiatone = 2.2f;
    [Tooltip("Secondi di corsa prima del fiatone.")]
    [SerializeField] float secondiDiCorsa = 6f;
    [SerializeField] float secondiFiatone = 2.5f;
    [Tooltip("Gli orchi entro questi metri lo inseguono anche senza vederlo.")]
    [SerializeField] float raggioAttrazione = 9f;

    public Transform Corpo => transform;
    public bool Abbattuto => morto;
    public bool Invisibile => false;
    public float Furtivita => 0f;
    public float RaggioAttrazione => salvo ? 0f : raggioAttrazione;
    public bool Morto => morto;
    // Sparito (salvo e fuori dal villaggio, o morto da un po'): RazziaVillaggio ne fa comparire un altro.
    public bool Finito { get; private set; }

    Vector3 meta;
    Vector3 uscita;
    bool morto, salvo, eraInseguito;
    float corsaDa, fiatoneFino, prossimoGrido, mortoDa, inciampoFino, prossimaMeta;
    Collider corpo;

    // Lo prepara RazziaVillaggio: dove scappa quando è salvo.
    public void Prepara(Vector3 uscitaDalVillaggio)
    {
        uscita = uscitaDalVillaggio;
    }

    void Awake()
    {
        corpo = GetComponent<Collider>();
        corsaDa = Time.time;
        prossimoGrido = Time.time + Random.Range(0.5f, 3f);
        // un vestito a caso fra colori smorti da villaggio
        var r = GetComponent<Renderer>();
        if (r != null)
        {
            Color[] vestiti = { new Color(0.42f, 0.3f, 0.2f), new Color(0.3f, 0.34f, 0.26f), new Color(0.38f, 0.22f, 0.2f), new Color(0.28f, 0.28f, 0.34f), new Color(0.46f, 0.42f, 0.32f) };
            r.material.color = vestiti[Random.Range(0, vestiti.Length)];
        }
        AspettoUmanoide.Prepara(gameObject, new Color(0.75f, 0.6f, 0.5f));
    }

    void OnEnable() => ObiettiviNemici.Iscrivi(this);
    void OnDisable() => ObiettiviNemici.Togli(this);

    void Update()
    {
        if (Rete.Ospite || Finito) return;
        if (morto)
        {
            // steso a terra; dopo un po' sparisce
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(-88f, transform.eulerAngles.y, 0f), 240f * Time.deltaTime);
            if (Time.time - mortoDa > 25f) Sparisci();
            return;
        }

        // chi lo insegue adesso: se prima c'era un orco e ora è morto, è salvo
        Bersaglio inseguitore = Inseguitore();
        if (inseguitore != null) eraInseguito = true;
        else if (eraInseguito && !salvo)
        {
            salvo = true;
            meta = uscita;
        }

        float velocita = velocitaCorsa;
        if (Time.time < fiatoneFino) velocita = velocitaFiatone;
        else if (Time.time - corsaDa > secondiDiCorsa)
        {
            fiatoneFino = Time.time + secondiFiatone;   // fiatone, poi riparte
            corsaDa = fiatoneFino;
            if (Random.value < 0.35f) inciampoFino = Time.time + 1.2f;   // a volte inciampa
        }
        if (Time.time < inciampoFino) velocita = 0f;

        if (salvo)
        {
            if (Cammina(uscita, velocita) || Lontano(40f)) Sparisci();
            return;
        }
        // scappa: ogni tanto (o arrivato) sceglie un'altra casa, lontana dall'orco più vicino
        if (Time.time >= prossimaMeta || Orizzontale(transform.position, meta) < 1.2f) NuovaMeta();
        if (Cammina(meta, velocita)) NuovaMeta();

        if (Time.time >= prossimoGrido)
        {
            prossimoGrido = Time.time + Random.Range(2.5f, 6f);
            Suoni.Suona(Suono.Grido, transform.position + Vector3.up, 0.55f, Random.Range(0.85f, 1.2f));
        }
    }

    // L'orco più vicino che sta inseguendo proprio lui.
    Bersaglio Inseguitore()
    {
        foreach (var b in RegistroNemici.Tutti)
            if (b != null && !b.Morto && ReferenceEquals(b.Obiettivo, this)) return b;
        return null;
    }

    void NuovaMeta()
    {
        prossimaMeta = Time.time + Random.Range(4f, 8f);
        Vector3 pericolo = transform.position;
        float minima = float.MaxValue;
        foreach (var b in RegistroNemici.Tutti)
        {
            if (b == null || b.Morto || !b.gameObject.activeInHierarchy) continue;
            float d = (b.transform.position - transform.position).sqrMagnitude;
            if (d < minima) { minima = d; pericolo = b.transform.position; }
        }
        // fra alcune case a caso, la più lontana dal pericolo ma non troppo lontana da qui
        Vector3 migliore = transform.position + Random.insideUnitSphere * 10f;
        float punteggio = float.MinValue;
        int prove = Mathf.Min(8, CasaInRovina.Tutte.Count);
        for (int i = 0; i < prove; i++)
        {
            var c = CasaInRovina.Tutte[Random.Range(0, CasaInRovina.Tutte.Count)];
            if (c == null) continue;
            Vector3 p = c.Davanti + c.Fuori * 2f;
            float lontanoDaQui = Orizzontale(p, transform.position);
            if (lontanoDaQui < 6f || lontanoDaQui > 35f) continue;
            float s = Orizzontale(p, pericolo) - lontanoDaQui * 0.3f;
            if (s > punteggio) { punteggio = s; migliore = p; }
        }
        migliore.y = transform.position.y;
        meta = migliore;
    }

    // Cammina verso un punto girando attorno agli ostacoli semplici. Vero quando è arrivato.
    bool Cammina(Vector3 punto, float velocita)
    {
        Vector3 verso = punto - transform.position;
        verso.y = 0f;
        float distanza = verso.magnitude;
        if (distanza < 0.8f) return true;
        if (velocita <= 0f) return false;
        Vector3 direzione = verso / distanza;
        float passo = Mathf.Min(velocita * Time.deltaTime, distanza);
        foreach (float deviazione in new[] { 0f, 35f, -35f, 70f, -70f, 110f, -110f })
        {
            Vector3 prova = Quaternion.Euler(0f, deviazione, 0f) * direzione;
            if (!Libero(prova, passo + 0.3f)) continue;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(prova), 540f * Time.deltaTime);
            transform.position += prova * passo;
            return false;
        }
        prossimaMeta = 0f;   // bloccato: cambia meta
        return false;
    }

    bool Libero(Vector3 direzione, float distanza)
    {
        Vector3 basso = transform.position + Vector3.up * -0.45f, alto = transform.position + Vector3.up * 0.45f;
        foreach (var colpo in Physics.CapsuleCastAll(basso, alto, 0.35f, direzione, distanza, ~0, QueryTriggerInteraction.Ignore))
        {
            if (colpo.collider == corpo || colpo.distance <= 0f) continue;
            if (colpo.collider.GetComponentInParent<Bersaglio>() != null) continue;   // gli orchi non lo fermano: li scansa correndo
            return false;
        }
        return true;
    }

    bool Lontano(float metri)
    {
        var g = ObiettiviNemici.Locale;
        return g != null && Orizzontale(g.transform.position, transform.position) > metri;
    }

    static float Orizzontale(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // ---------- IObiettivoNemico ----------

    public void ColpitoDaNemico(Bersaglio nemico)
    {
        if (morto) return;
        vita -= nemico != null ? nemico.DannoAttacco : 20f;
        Suoni.Suona(Suono.Colpito, transform.position + Vector3.up, 0.8f, 1.4f);
        if (vita > 0f)
        {
            inciampoFino = Time.time + 0.4f;
            return;
        }
        morto = true;
        mortoDa = Time.time;
        Suoni.Suona(Suono.Grido, transform.position + Vector3.up, 0.9f, 0.8f);
        if (corpo != null) corpo.enabled = false;
        ObiettiviNemici.Togli(this);   // l'orco che lo inseguiva lo perde e torna a distruggere
    }

    public void NemicoSconfitto() { }

    void Sparisci()
    {
        Finito = true;
        gameObject.SetActive(false);
    }
}
