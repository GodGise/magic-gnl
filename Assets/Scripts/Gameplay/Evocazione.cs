using UnityEngine;

// Base comune delle evocazioni dello Stregone: Fuoco fatuo, Spirito del lupo, Bambola di ossa (e il suo scheletro).
// A cosa serve: tiene vita, durata e la fine dell'evocazione. Le evocazioni sono obiettivi per i nemici
// (IObiettivoNemico, vedi ObiettiviNemici): i nemici possono attaccarle e ucciderle. Quando un'evocazione finisce o
// muore avvisa MagiaStregone (Finita), che fa partire l'attesa prima di poterla rievocare.
// Le uccisioni delle evocazioni contano come uccisioni dello Stregone (amuleti che ridanno vita o mana): il colpo
// passa da Bersaglio.RiceviColpo dal PC dello Stregone, quindi il premio arriva a lui.
// Co-op: per ora l'evocazione esiste solo sul PC di chi la lancia. Se lo Stregone ospita la partita i nemici la
// vedono e la attaccano; se non ospita, i suoi colpi arrivano lo stesso ai nemici ma i nemici non la vedono.
// Come montarlo: non si monta. Le crea MagiaStregone (FuocoFatuo.Crea, SpiritoLupo.Crea, BambolaOssa.Crea).
public abstract class Evocazione : MonoBehaviour, IObiettivoNemico
{
    protected MagiaStregone padrone;
    protected DatiIncantesimo dati;
    protected float vita, vitaMassima;
    protected float fineDurata;
    bool finita;

    public event System.Action Finita;

    public Transform Corpo => transform;
    public virtual bool Abbattuto => finita || vita <= 0f;
    public bool Invisibile => false;
    public float Furtivita => 0f;
    // Le evocazioni senza vita (Fuoco fatuo) non si possono colpire: i nemici le ignorano.
    protected virtual bool Bersagliabile => true;

    protected void Prepara(MagiaStregone chi, DatiIncantesimo inc, float vitaBase, float durata)
    {
        padrone = chi;
        dati = inc;
        vitaMassima = vita = chi.VitaEvocazione(Mathf.Max(1f, vitaBase));
        fineDurata = durata > 0f ? Time.time + chi.DurataEvocazione(durata) : float.MaxValue;
        if (Bersagliabile) ObiettiviNemici.Iscrivi(this);
    }

    protected virtual void Update()
    {
        if (finita) return;
        if (padrone == null) { Termina(false); return; }
        if (Time.time >= fineDurata) { Scade(); return; }
        Aggiorna();
    }

    protected abstract void Aggiorna();

    // La durata è finita (la Bambola invece si anima in scheletro).
    protected virtual void Scade() => Termina(false);

    // Un nemico la colpisce: il danno si calcola come per il giocatore, ma l'evocazione non ha armatura.
    public void ColpitoDaNemico(Bersaglio nemico)
    {
        if (finita || nemico == null) return;
        float danno = CalcoloDanno.Calcola(nemico.DannoAttacco, nemico.Statistiche, null, out _);
        PerdiVita(danno);
    }

    protected void PerdiVita(float danno)
    {
        if (finita) return;
        vita -= danno;
        Lampeggia();
        if (vita <= 0f) Muori();
    }

    // Morta per mano dei nemici (la Bambola fa il gas).
    protected virtual void Muori() => Termina(true);

    public void NemicoSconfitto() { }

    public void Termina(bool uccisa)
    {
        if (finita) return;
        finita = true;
        ObiettiviNemici.Togli(this);
        Finita?.Invoke();
        Destroy(gameObject);
    }

    protected virtual void Lampeggia() { }

    protected void OnDestroy() => ObiettiviNemici.Togli(this);

    // Il nemico vivo più vicino entro "raggio" (o null).
    protected Bersaglio NemicoVicino(float raggio)
    {
        Bersaglio migliore = null;
        float minima = raggio * raggio;
        foreach (Bersaglio b in FindObjectsByType<Bersaglio>(FindObjectsSortMode.None))
        {
            if (b == null || b.Morto || !b.isActiveAndEnabled) continue;
            float d = (b.transform.position - transform.position).sqrMagnitude;
            if (d < minima) { minima = d; migliore = b; }
        }
        return migliore;
    }

    // Una forma semplice e luminosa (le evocazioni sono spiriti): primitiva senza collider, così non blocca niente.
    protected static GameObject Forma(PrimitiveType tipo, Transform padre, Vector3 posizione, Vector3 scala, Color colore, float luce = 1.5f)
    {
        var parte = GameObject.CreatePrimitive(tipo);
        Destroy(parte.GetComponent<Collider>());
        parte.transform.SetParent(padre, false);
        parte.transform.localPosition = posizione;
        parte.transform.localScale = scala;
        var aspetto = parte.GetComponent<Renderer>();
        var materiale = new Material(aspetto.sharedMaterial) { color = colore };
        materiale.EnableKeyword("_EMISSION");
        materiale.SetColor("_EmissionColor", colore * luce);
        aspetto.sharedMaterial = materiale;
        return parte;
    }

    // Cammina verso un punto sul terreno, a "velocita" metri al secondo, girandosi verso la meta.
    protected void CamminaVerso(Vector3 meta, float velocita, float fermaA)
    {
        Vector3 verso = meta - transform.position;
        verso.y = 0f;
        float distanza = verso.magnitude;
        if (distanza > 0.01f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(verso), 540f * Time.deltaTime);
        if (distanza <= fermaA) return;
        Vector3 nuovo = transform.position + verso / distanza * Mathf.Min(velocita * Time.deltaTime, distanza - fermaA);
        // resta appoggiato al terreno
        if (Physics.Raycast(nuovo + Vector3.up * 1.5f, Vector3.down, out RaycastHit terra, 4f, ~0, QueryTriggerInteraction.Ignore)
            && terra.collider.GetComponentInParent<Bersaglio>() == null && !ObiettiviNemici.EGiocatore(terra.collider))
            nuovo.y = terra.point.y;
        transform.position = nuovo;
    }
}
