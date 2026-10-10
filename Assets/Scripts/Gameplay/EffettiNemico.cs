using System.Collections;
using UnityEngine;

// Gli effetti degli incantesimi su un nemico: rallentamento, blocco, stordimento e spinta.
// A cosa serve: InseguimentoNemico chiede qui quanto può muoversi (MoltiplicatoreVelocita, Fermo) e Bersaglio
// se può attaccare (Stordito). Le regole (Docs/incantesimi-stregone.md):
//   - Rallentato: va più piano. Dopo un rallentamento, per 4 s il nemico non può essere rallentato di nuovo
//     (e intanto un nuovo rallentamento non rinnova quello in corso).
//   - Bloccato: non si muove (ma se è abbastanza vicino può attaccare).
//   - Stordito: non si muove e non attacca.
//   - Spinto: indietreggia di qualche metro; se dietro non c'è più terreno (un dirupo) cade e muore.
//   - Boss e miniboss (Bersaglio > Boss): niente blocchi, stordimenti e spinte, e i rallentamenti valgono la metà.
//   - Co-op: per ogni Stregone nel gruppo rallentamenti, blocchi e stordimenti durano il 30% in meno
//     (DifficoltaCoop.MoltiplicatoreControlli; da soli niente cambia).
// Sotto i piedi del nemico compare un anello colorato: azzurro rallentato, viola bloccato, giallo stordito.
// Co-op: gli effetti decidono come si muove il nemico, quindi valgono sul PC di chi ospita. Chi non ospita li
// chiede all'host (MondoRete.ChiediEffetto) e intanto li vede da sé con l'anello.
// Come montarlo: non si monta. Si aggiunge da solo al nemico la prima volta che subisce un effetto (Applica).
public class EffettiNemico : MonoBehaviour
{
    public enum Tipo : byte { Rallenta, Blocca, Stordisci, Spingi }

    const float ImmunitaRallentamento = 4f;   // secondi di immunità dopo la fine di un rallentamento

    Bersaglio bersaglio;
    float rallentatoFino, rallentamento, immuneFino, bloccatoFino, storditoFino;
    Transform anello;
    Renderer aspettoAnello;

    public bool Stordito => Time.time < storditoFino;
    public bool Bloccato => Time.time < bloccatoFino;
    public bool Fermo => Bloccato || Stordito;
    public bool Rallentato => Time.time < rallentatoFino;
    public float MoltiplicatoreVelocita => Fermo ? 0f : Rallentato ? Mathf.Clamp01(1f - rallentamento / 100f) : 1f;

    // L'effetto di un nemico, se ce l'ha (null se non ha mai subito effetti).
    public static EffettiNemico Di(Component nemico) => nemico != null ? nemico.GetComponent<EffettiNemico>() : null;

    // Da usare per ogni effetto: da soli o sull'host lo applica subito, chi non ospita lo chiede all'host.
    // valore: percentuale per il rallentamento, metri per la spinta. punto: da dove arriva (per la spinta).
    public static void Applica(Bersaglio b, Tipo tipo, float valore, float durata, Vector3 punto)
    {
        if (b == null || b.Morto) return;
        var effetti = b.GetComponent<EffettiNemico>();
        if (effetti == null) effetti = b.gameObject.AddComponent<EffettiNemico>();
        if (Rete.Ospite)
        {
            MondoRete.ChiediEffetto(b, (byte)tipo, valore, durata, punto);
            if (tipo != Tipo.Spingi) effetti.ApplicaQui(tipo, valore, durata, punto);   // solo per vederlo subito
            return;
        }
        effetti.ApplicaQui(tipo, valore, durata, punto);
    }

    void Awake()
    {
        bersaglio = GetComponent<Bersaglio>();
    }

    public void ApplicaQui(Tipo tipo, float valore, float durata, Vector3 punto)
    {
        if (bersaglio == null || bersaglio.Morto || durata <= 0f && tipo != Tipo.Spingi) return;
        bool boss = bersaglio.Boss;
        float ora = Time.time;
        if (tipo != Tipo.Spingi) durata *= DifficoltaCoop.MoltiplicatoreControlli;
        switch (tipo)
        {
            case Tipo.Rallenta:
                if (Rallentato || ora < immuneFino) return;   // niente rallentamenti a catena
                rallentamento = boss ? valore * 0.5f : valore;
                rallentatoFino = ora + durata;
                immuneFino = rallentatoFino + ImmunitaRallentamento;
                break;
            case Tipo.Blocca:
                if (boss) return;
                bloccatoFino = Mathf.Max(bloccatoFino, ora + durata);
                break;
            case Tipo.Stordisci:
                if (boss) return;
                storditoFino = Mathf.Max(storditoFino, ora + durata);
                break;
            case Tipo.Spingi:
                if (boss || Rete.Ospite) return;
                StartCoroutine(Spinta(punto, valore));
                break;
        }
    }

    // Spinta: scivola indietro di "metri" in un attimo, fermandosi contro i muri. Se finisce dove non c'è
    // terreno sotto (un dirupo), cade e muore.
    IEnumerator Spinta(Vector3 daDove, float metri)
    {
        Vector3 verso = transform.position - daDove;
        verso.y = 0f;
        if (verso.sqrMagnitude < 0.0001f) verso = -transform.forward;
        verso.Normalize();
        const float durata = 0.35f;
        float fatti = 0f;
        Collider corpo = GetComponent<Collider>();
        while (fatti < metri && bersaglio != null && !bersaglio.Morto)
        {
            float passo = Mathf.Min(metri - fatti, metri / durata * Time.deltaTime);
            // Un muro davanti ferma la spinta.
            if (Physics.Raycast(transform.position + Vector3.up * 0.3f, verso, out RaycastHit muro, passo + 0.4f, ~0, QueryTriggerInteraction.Ignore)
                && muro.collider != corpo && !muro.collider.transform.IsChildOf(transform) && !ObiettiviNemici.EGiocatore(muro.collider))
                break;
            transform.position += verso * passo;
            fatti += passo;
            yield return null;
        }
        if (bersaglio == null || bersaglio.Morto) yield break;
        // C'è ancora terreno sotto i piedi? Se no, è caduto da un dirupo.
        Vector3 piedi = corpo != null ? new Vector3(transform.position.x, corpo.bounds.min.y + 0.1f, transform.position.z) : transform.position;
        if (!Physics.Raycast(piedi, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore))
            StartCoroutine(Caduta());
    }

    IEnumerator Caduta()
    {
        for (float t = 0f; t < 0.8f && bersaglio != null && !bersaglio.Morto; t += Time.deltaTime)
        {
            transform.position += Vector3.down * (4f + 20f * t) * Time.deltaTime;
            yield return null;
        }
        if (bersaglio != null && !bersaglio.Morto)
            bersaglio.RiceviColpoDa(Rete.MioId, bersaglio.VitaMassima * 10f, transform.position + Vector3.up, false);
    }

    void Update()
    {
        Color? colore = Stordito ? new Color(1f, 0.85f, 0.2f) : Bloccato ? new Color(0.45f, 0.2f, 0.65f)
            : Rallentato ? new Color(0.45f, 0.75f, 1f) : (Color?)null;
        if (colore == null || bersaglio == null || bersaglio.Morto)
        {
            if (anello != null && anello.gameObject.activeSelf) anello.gameObject.SetActive(false);
            return;
        }
        if (anello == null) CreaAnello();
        if (!anello.gameObject.activeSelf) anello.gameObject.SetActive(true);
        aspettoAnello.sharedMaterial.color = colore.Value;
        anello.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
    }

    void CreaAnello()
    {
        var oggetto = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        oggetto.name = "Effetto magico";
        Destroy(oggetto.GetComponent<Collider>());
        anello = oggetto.transform;
        anello.SetParent(transform, false);
        Collider corpo = GetComponent<Collider>();
        float sotto = corpo != null ? corpo.bounds.min.y - transform.position.y + 0.03f : -0.95f;
        anello.localPosition = new Vector3(0f, sotto, 0f);
        anello.localScale = new Vector3(1.4f, 0.02f, 1.4f);
        aspettoAnello = oggetto.GetComponent<Renderer>();
        aspettoAnello.sharedMaterial = new Material(aspettoAnello.sharedMaterial);
    }
}
