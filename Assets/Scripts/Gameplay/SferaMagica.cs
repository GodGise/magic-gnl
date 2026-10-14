using System.Collections;
using UnityEngine;

// Sfera luminosa lanciata dal bastone magico, e dai proiettili degli incantesimi dello Stregone (Scintilla,
// Palla di fuoco, Scheggia di ghiaccio, Dardo d'ombra, scintille dei Fuochi fatui) con il loro colore e grandezza.
// A cosa serve: vola verso il bersaglio scelto al momento del lancio, correggendo un po' la rotta se
// il nemico si sposta. Quando tocca un nemico gli fa danno (e lo spinge indietro come un colpo di spada);
// tocca anche i muri crepati. Contro un muro qualsiasi, o dopo 3 secondi, si spegne con un piccolo scoppio.
// Con danno 0 è solo da vedere (co-op: la sfera lanciata da un altro giocatore; il danno lo manda il suo PC).
// Incantesimi: con LanciaIncantesimo il danno e gli effetti non li decide la sfera ma chi la lancia (MagiaStregone),
// con "alColpo": viene chiamato quando tocca un nemico (oppure, con nemico vuoto, un muro o allo scadere del tempo,
// per esempio per far esplodere la Palla di fuoco dove si trova).
// Come montarlo: non serve montarlo. La creano GiocatoreControllo e MagiaStregone con SferaMagica.Lancia(...).
public class SferaMagica : MonoBehaviour
{
    static readonly Color Colore = new Color(0.4f, 0.8f, 1f);
    const float Raggio = 0.22f;
    const float Sterzata = 6f;   // quanto in fretta corregge la rotta verso il bersaglio

    float durataMassima = 3f;
    float grandezza = 1f;
    System.Action<Bersaglio, Vector3> alColpo;   // incantesimi: chi lancia decide danno ed effetti

    Bersaglio obiettivo;
    Transform lanciatore;
    Vector3 direzione;
    float velocita;
    float danno;
    float penetrazione;   // quota di armatura del nemico ignorata (bastoni dello Stregone)
    float tempo;
    bool esplosa;
    Light luce;

    // penetrazione: quota dell'armatura del nemico che la sfera ignora (0,25 = un quarto), dal bastone equipaggiato.
    public static void Lancia(Vector3 partenza, Vector3 direzione, Bersaglio obiettivo, float velocita, float danno, Transform lanciatore, float penetrazione = 0f)
    {
        SferaMagica sfera = Crea(partenza, Colore, 1f);
        sfera.obiettivo = obiettivo;
        sfera.lanciatore = lanciatore;
        sfera.direzione = direzione.normalized;
        sfera.velocita = velocita;
        sfera.danno = danno;
        sfera.penetrazione = penetrazione;
    }

    // Proiettile di un incantesimo: colore, grandezza (1 = normale) e cosa succede quando colpisce.
    public static SferaMagica LanciaIncantesimo(Vector3 partenza, Vector3 direzione, Bersaglio obiettivo, float velocita, Transform lanciatore,
        Color colore, float grandezza, float durataMassima, System.Action<Bersaglio, Vector3> alColpo)
    {
        SferaMagica sfera = Crea(partenza, colore, grandezza);
        sfera.obiettivo = obiettivo;
        sfera.lanciatore = lanciatore;
        sfera.direzione = direzione.normalized;
        sfera.velocita = velocita;
        sfera.durataMassima = Mathf.Max(0.1f, durataMassima);
        sfera.alColpo = alColpo;
        sfera.danno = 1f;   // non è "solo aspetto"
        return sfera;
    }

    static SferaMagica Crea(Vector3 partenza, Color colore, float grandezza)
    {
        GameObject oggetto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        oggetto.name = "Sfera magica";
        DestroyImmediate(oggetto.GetComponent<Collider>());
        oggetto.transform.position = partenza;
        oggetto.transform.localScale = Vector3.one * 0.35f * grandezza;

        Renderer aspetto = oggetto.GetComponent<Renderer>();
        Material materiale = new Material(aspetto.sharedMaterial) { color = colore };
        materiale.EnableKeyword("_EMISSION");
        materiale.SetColor("_EmissionColor", colore * 2.5f);
        aspetto.sharedMaterial = materiale;

        SferaMagica sfera = oggetto.AddComponent<SferaMagica>();
        sfera.grandezza = grandezza;
        sfera.luce = oggetto.AddComponent<Light>();
        sfera.luce.type = LightType.Point;
        sfera.luce.color = colore;
        sfera.luce.range = 5f;
        sfera.luce.intensity = 2.5f;
        return sfera;
    }

    void Update()
    {
        if (esplosa) return;

        float dt = Time.deltaTime;
        tempo += dt;
        if (tempo >= durataMassima)
        {
            alColpo?.Invoke(null, transform.position);   // per esempio la Palla di fuoco esplode dove si trova
            Esplodi();
            return;
        }

        // Corregge un poco la rotta verso il bersaglio, se è ancora vivo.
        if (obiettivo != null && !obiettivo.Morto)
        {
            Vector3 verso = (obiettivo.transform.position + Vector3.up * 0.3f - transform.position).normalized;
            direzione = Vector3.RotateTowards(direzione, verso, Sterzata * dt, 0f).normalized;
        }

        // Controlla se in questo tratto tocca qualcosa (il giocatore stesso non conta).
        float passo = velocita * dt;
        Collider toccato = null;
        float distanzaToccato = float.MaxValue;
        foreach (RaycastHit colpo in Physics.SphereCastAll(transform.position, Raggio * grandezza, direzione, passo, ~0, QueryTriggerInteraction.Ignore))
        {
            if (lanciatore != null && colpo.collider.transform.IsChildOf(lanciatore)) continue;
            if (colpo.distance < distanzaToccato)
            {
                distanzaToccato = colpo.distance;
                toccato = colpo.collider;
            }
        }

        if (toccato != null)
        {
            transform.position += direzione * distanzaToccato;
            Colpisci(toccato);
            return;
        }

        transform.position += direzione * passo;
        transform.localScale = Vector3.one * (0.35f + 0.05f * Mathf.Sin(tempo * 25f)) * grandezza; // pulsa un poco
    }

    void Colpisci(Collider toccato)
    {
        if (danno <= 0f) { Esplodi(); return; }
        if (alColpo != null)
        {
            // Incantesimo: danno ed effetti li decide chi l'ha lanciato. I muri crepati si rompono lo stesso.
            Bersaglio colpito = toccato.GetComponentInParent<Bersaglio>();
            if (colpito == null)
            {
                MuroFragile crepato = toccato.GetComponentInParent<MuroFragile>();
                if (crepato != null) crepato.RiceviColpo(transform.position - direzione * 2f);
            }
            alColpo(colpito, transform.position);
            Esplodi();
            return;
        }
        Bersaglio nemico = toccato.GetComponentInParent<Bersaglio>();
        if (nemico != null)
        {
            Statistiche diChiLancia = lanciatore != null ? Statistiche.Di(lanciatore) : null;
            float dannoFinale = CalcoloDanno.Calcola(danno, diChiLancia, nemico.Statistiche, out bool critico, penetrazione);
            nemico.RiceviColpo(dannoFinale, transform.position - direzione, critico);
            // Amuleto Cuore del lago nero: parte del danno torna come mana a chi ha lanciato.
            GiocatoreControllo chiLancia = lanciatore != null ? lanciatore.GetComponent<GiocatoreControllo>() : null;
            if (chiLancia != null) chiLancia.IncantesimoASegno(dannoFinale);
        }
        else
        {
            MuroFragile muro = toccato.GetComponentInParent<MuroFragile>();
            if (muro != null) muro.RiceviColpo(transform.position - direzione * 2f);
        }
        Esplodi();
    }

    // Piccolo scoppio: la sfera sparisce, la luce fa un lampo e si spegne.
    void Esplodi()
    {
        esplosa = true;
        GetComponent<Renderer>().enabled = false;
        Suoni.Suona(Suono.SferaImpatto, transform.position, 0.9f);
        StartCoroutine(Spegni());
    }

    IEnumerator Spegni()
    {
        const float durata = 0.3f;
        for (float t = 0f; t < durata; t += Time.deltaTime)
        {
            luce.intensity = Mathf.Lerp(6f, 0f, t / durata);
            luce.range = Mathf.Lerp(7f, 3f, t / durata);
            yield return null;
        }
        Destroy(gameObject);
    }
}
