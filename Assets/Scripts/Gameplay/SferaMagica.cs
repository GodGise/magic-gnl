using System.Collections;
using UnityEngine;

// Sfera luminosa lanciata dal bastone magico.
// A cosa serve: vola verso il bersaglio scelto al momento del lancio, correggendo un po' la rotta se
// il nemico si sposta. Quando tocca un nemico gli fa danno (e lo spinge indietro come un colpo di spada);
// tocca anche i muri crepati. Contro un muro qualsiasi, o dopo 3 secondi, si spegne con un piccolo scoppio.
// Come montarlo: non serve montarlo. La crea GiocatoreControllo con SferaMagica.Lancia(...).
public class SferaMagica : MonoBehaviour
{
    static readonly Color Colore = new Color(0.4f, 0.8f, 1f);
    const float Raggio = 0.22f;
    const float Sterzata = 6f;   // quanto in fretta corregge la rotta verso il bersaglio
    const float DurataMassima = 3f;

    Bersaglio obiettivo;
    Transform lanciatore;
    Vector3 direzione;
    float velocita;
    float danno;
    float tempo;
    bool esplosa;
    Light luce;

    public static void Lancia(Vector3 partenza, Vector3 direzione, Bersaglio obiettivo, float velocita, float danno, Transform lanciatore)
    {
        GameObject oggetto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        oggetto.name = "Sfera magica";
        DestroyImmediate(oggetto.GetComponent<Collider>());
        oggetto.transform.position = partenza;
        oggetto.transform.localScale = Vector3.one * 0.35f;

        Renderer aspetto = oggetto.GetComponent<Renderer>();
        Material materiale = new Material(aspetto.sharedMaterial) { color = Colore };
        materiale.EnableKeyword("_EMISSION");
        materiale.SetColor("_EmissionColor", Colore * 2.5f);
        aspetto.sharedMaterial = materiale;

        SferaMagica sfera = oggetto.AddComponent<SferaMagica>();
        sfera.luce = oggetto.AddComponent<Light>();
        sfera.luce.type = LightType.Point;
        sfera.luce.color = Colore;
        sfera.luce.range = 5f;
        sfera.luce.intensity = 2.5f;

        sfera.obiettivo = obiettivo;
        sfera.lanciatore = lanciatore;
        sfera.direzione = direzione.normalized;
        sfera.velocita = velocita;
        sfera.danno = danno;
    }

    void Update()
    {
        if (esplosa) return;

        float dt = Time.deltaTime;
        tempo += dt;
        if (tempo >= DurataMassima)
        {
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
        foreach (RaycastHit colpo in Physics.SphereCastAll(transform.position, Raggio, direzione, passo, ~0, QueryTriggerInteraction.Ignore))
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
        transform.localScale = Vector3.one * (0.35f + 0.05f * Mathf.Sin(tempo * 25f)); // pulsa un poco
    }

    void Colpisci(Collider toccato)
    {
        Bersaglio nemico = toccato.GetComponentInParent<Bersaglio>();
        if (nemico != null)
        {
            Statistiche diChiLancia = lanciatore != null ? Statistiche.Di(lanciatore) : null;
            float dannoFinale = CalcoloDanno.Calcola(danno, diChiLancia, nemico.Statistiche, out bool critico);
            nemico.RiceviColpo(dannoFinale, transform.position - direzione, critico);
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
