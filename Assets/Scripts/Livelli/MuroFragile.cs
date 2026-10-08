using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Muro crepato che crolla dopo un certo numero di colpi, come i muri segreti di Dark Souls.
// A cosa serve: nasconde un passaggio. Si riconosce dalle crepe scure sulle due facce. Ogni colpo
// del giocatore lo fa tremare e allarga le crepe; al colpo giusto il muro crolla in tanti blocchi
// che cadono, rotolano e dopo qualche secondo spariscono, lasciando il passaggio aperto.
// Come montarlo:
//   1. GameObject > 3D Object > Cube. Con Scale dagli la misura del muro (per esempio X 0.6, Y 2.8,
//      Z 2: lo spessore è il lato più sottile tra X e Z) e mettilo nel varco da chiudere.
//   2. Dagli lo stesso materiale dei muri vicini, così sembra un muro come gli altri.
//   3. Nell'Inspector clicca Add Component e scegli Muro Fragile. Le crepe si disegnano da sole
//      quando parte il gioco; nella vista Scene il muro ha un contorno arancione.
// Il giocatore lo colpisce con il normale attacco (tasto sinistro): non serve altro.
// Co-op: i colpi di tutti i giocatori contano insieme (li conta l'host) e il muro crolla per tutti.
public class MuroFragile : MonoBehaviour, IOggettoCondiviso
{
    [Tooltip("Quanti colpi servono per farlo crollare.")]
    [SerializeField] int colpiNecessari = 4;

    [Header("Crepe")]
    [Tooltip("Quante crepe disegnare su ogni faccia.")]
    [SerializeField] int numeroCrepe = 4;
    [SerializeField] Color coloreCrepe = new Color(0.04f, 0.035f, 0.03f);
    [Tooltip("Di quanto si allargano le crepe a ogni colpo (0.5 = la metà in più).")]
    [SerializeField] float allargamentoPerColpo = 0.5f;

    [Header("Crollo")]
    [Tooltip("In quanti blocchi si rompe: in larghezza (X) e in altezza (Y).")]
    [SerializeField] Vector2Int frammenti = new Vector2Int(4, 5);
    [Tooltip("Forza con cui i blocchi vengono spinti via, dalla parte opposta al giocatore.")]
    [SerializeField] float spinta = 3f;
    [Tooltip("Secondi dopo il crollo prima che i blocchi spariscano.")]
    [SerializeField] float durataMacerie = 4f;

    int colpiRicevuti;
    bool crollato;
    Vector3 posizioneBase;
    Coroutine tremore;

    // Misure del muro (dalla Scale del cubo) e asse dello spessore: 0 = X, 2 = Z.
    Vector3 misure;
    int asseSpessore;

    Transform crepe;
    Material materialeCrepe;
    readonly List<Transform> segmentiCrepe = new List<Transform>();
    readonly List<Vector3> scaleCrepe = new List<Vector3>();

    public int NumeroRete { get; private set; }
    int statoDaRete = int.MinValue;   // co-op: stato ricevuto prima che il muro fosse pronto

    void Awake() => NumeroRete = RegistroCondivisi.Iscrivi(this);
    void OnDestroy() => RegistroCondivisi.Togli(this, NumeroRete);

    void Start()
    {
        posizioneBase = transform.position;
        Vector3 scala = transform.lossyScale;
        misure = new Vector3(Mathf.Abs(scala.x), Mathf.Abs(scala.y), Mathf.Abs(scala.z));
        asseSpessore = misure.x <= misure.z ? 0 : 2;
        CreaCrepe();
        if (statoDaRete != int.MinValue) StatoDaRete(statoDaRete);
    }

    // Chiamato dal giocatore quando un suo colpo prende il muro. In co-op, per chi non ospita, il colpo va all'host.
    public void RiceviColpo(Vector3 origineColpo)
    {
        if (crollato) return;
        if (MondoRete.ChiediUso(this, 1, origineColpo)) return;
        Colpo(origineColpo);
        MondoRete.InviaEvento(this, Rete.MioId, colpiRicevuti, origineColpo);
    }

    // ---------- co-op (IOggettoCondiviso) ----------
    public void UsaDaRete(ulong chi, int valore, Vector3 punto) => RiceviColpo(punto);

    // L'host ha contato un colpo: "valore" è il numero di colpi ricevuti finora.
    public void EventoDaRete(ulong chi, int valore, Vector3 punto)
    {
        if (crollato || crepe == null) return;
        colpiRicevuti = valore - 1;
        Colpo(punto);
    }

    public int StatoRete => crollato ? -1 : colpiRicevuti;
    public void StatoDaRete(int stato)
    {
        if (crepe == null) { statoDaRete = stato; return; }   // non ancora pronto: lo applica Start
        if (crollato) return;
        if (stato < 0) { Crolla(posizioneBase - transform.forward); return; }
        colpiRicevuti = stato;
        AllargaCrepe();
    }

    void Colpo(Vector3 origineColpo)
    {
        if (crollato) return;
        colpiRicevuti++;
        Debug.Log(name + " colpito: " + colpiRicevuti + " / " + colpiNecessari);
        Suoni.Suona(Suono.ColpoMuro, posizioneBase);

        if (colpiRicevuti >= colpiNecessari)
        {
            Crolla(origineColpo);
            return;
        }

        AllargaCrepe();
        if (tremore != null) StopCoroutine(tremore);
        tremore = StartCoroutine(Trema());
    }

    // ---------- Crepe ----------

    // Disegna su ogni faccia alcune crepe a zig-zag fatte di bastoncini scuri sottilissimi.
    // Le crepe stanno in un oggetto a parte, senza la Scale del muro, così non si deformano.
    void CreaCrepe()
    {
        crepe = new GameObject("Crepe di " + name).transform;
        crepe.SetPositionAndRotation(transform.position, transform.rotation);

        Vector3 normale = asseSpessore == 0 ? Vector3.right : Vector3.forward;
        Vector3 larghezzaAsse = asseSpessore == 0 ? Vector3.forward : Vector3.right;
        float mezzaLarghezza = (asseSpessore == 0 ? misure.z : misure.x) * 0.5f;
        float mezzaAltezza = misure.y * 0.5f;
        float mezzoSpessore = (asseSpessore == 0 ? misure.x : misure.z) * 0.5f;

        // Stesso disegno a ogni partita per lo stesso muro.
        var caso = new System.Random(Mathf.RoundToInt(transform.position.x * 73f + transform.position.z * 31f));

        for (int faccia = -1; faccia <= 1; faccia += 2)
        {
            for (int c = 0; c < numeroCrepe; c++)
            {
                // Ogni crepa parte da un punto nella parte centrale del muro e serpeggia.
                Vector2 punto = new Vector2(Tra(caso, -0.6f, 0.6f) * mezzaLarghezza, Tra(caso, -0.6f, 0.6f) * mezzaAltezza);
                float angolo = Tra(caso, 0f, 360f);
                int pezzi = caso.Next(4, 8);

                for (int p = 0; p < pezzi; p++)
                {
                    angolo += Tra(caso, -45f, 45f);
                    Vector2 direzione = new Vector2(Mathf.Cos(angolo * Mathf.Deg2Rad), Mathf.Sin(angolo * Mathf.Deg2Rad));
                    Vector2 fine = punto + direzione * Tra(caso, 0.18f, 0.4f);
                    fine.x = Mathf.Clamp(fine.x, -mezzaLarghezza + 0.05f, mezzaLarghezza - 0.05f);
                    fine.y = Mathf.Clamp(fine.y, -mezzaAltezza + 0.05f, mezzaAltezza - 0.05f);

                    Segmento(punto, fine, normale * faccia, larghezzaAsse, mezzoSpessore);
                    punto = fine;
                }
            }
        }
    }

    // Un pezzetto di crepa da "da" ad "a" (coordinate sulla faccia: x = larghezza, y = altezza).
    void Segmento(Vector2 da, Vector2 a, Vector3 versoFaccia, Vector3 larghezzaAsse, float mezzoSpessore)
    {
        Vector2 tratto = a - da;
        if (tratto.magnitude < 0.02f) return;

        Vector2 centro = (da + a) * 0.5f;
        Vector3 direzione = (larghezzaAsse * tratto.x + Vector3.up * tratto.y).normalized;

        GameObject segmento = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segmento.name = "Crepa";
        DestroyImmediate(segmento.GetComponent<Collider>());
        segmento.transform.SetParent(crepe, false);
        // Appoggiato sulla faccia: sporge di pochi millimetri, così si vede senza sfarfallare.
        segmento.transform.localPosition = larghezzaAsse * centro.x + Vector3.up * centro.y + versoFaccia * (mezzoSpessore + 0.006f);
        segmento.transform.localRotation = Quaternion.LookRotation(direzione, versoFaccia);
        segmento.transform.localScale = new Vector3(0.035f, 0.012f, tratto.magnitude);

        Renderer aspetto = segmento.GetComponent<Renderer>();
        if (materialeCrepe == null) materialeCrepe = new Material(aspetto.sharedMaterial) { color = coloreCrepe };
        aspetto.sharedMaterial = materialeCrepe;

        segmentiCrepe.Add(segmento.transform);
        scaleCrepe.Add(segmento.transform.localScale);
    }

    void AllargaCrepe()
    {
        float fattore = 1f + allargamentoPerColpo * colpiRicevuti;
        for (int i = 0; i < segmentiCrepe.Count; i++)
        {
            Vector3 scala = scaleCrepe[i];
            segmentiCrepe[i].localScale = new Vector3(scala.x * fattore, scala.y, scala.z);
        }
    }

    // Breve tremolio a ogni colpo, che si smorza.
    IEnumerator Trema()
    {
        const float durata = 0.15f;
        const float ampiezza = 0.04f;
        for (float t = 0f; t < durata; t += Time.deltaTime)
        {
            Vector3 scossa = Random.insideUnitSphere * ampiezza * (1f - t / durata);
            transform.position = posizioneBase + scossa;
            crepe.position = posizioneBase + scossa;
            yield return null;
        }
        transform.position = posizioneBase;
        crepe.position = posizioneBase;
        tremore = null;
    }

    // ---------- Crollo ----------

    void Crolla(Vector3 origineColpo)
    {
        crollato = true;
        Suoni.Suona(Suono.CrolloMuro, posizioneBase);
        if (tremore != null) StopCoroutine(tremore);
        transform.position = posizioneBase;
        Destroy(crepe.gameObject);

        // Il muro intero sparisce (aspetto e collider): al suo posto cadono i blocchi.
        Renderer aspetto = GetComponent<Renderer>();
        Material materiale = aspetto != null ? aspetto.sharedMaterial : null;
        if (aspetto != null) aspetto.enabled = false;
        Collider corpo = GetComponent<Collider>();
        if (corpo != null) corpo.enabled = false;

        // I blocchi vengono spinti dalla parte opposta al giocatore.
        Vector3 normale = asseSpessore == 0 ? transform.right : transform.forward;
        if (Vector3.Dot(normale, posizioneBase - origineColpo) < 0f) normale = -normale;

        Vector3 larghezzaAsse = asseSpessore == 0 ? transform.forward : transform.right;
        float larghezza = asseSpessore == 0 ? misure.z : misure.x;
        float spessore = asseSpessore == 0 ? misure.x : misure.z;
        int colonne = Mathf.Max(1, frammenti.x);
        int righe = Mathf.Max(1, frammenti.y);
        float larghezzaBlocco = larghezza / colonne;
        float altezzaBlocco = misure.y / righe;

        var macerie = new List<GameObject>();
        for (int i = 0; i < colonne; i++)
        {
            for (int j = 0; j < righe; j++)
            {
                Vector3 centro = posizioneBase
                    + larghezzaAsse * ((i + 0.5f) * larghezzaBlocco - larghezza * 0.5f)
                    + transform.up * ((j + 0.5f) * altezzaBlocco - misure.y * 0.5f);

                GameObject blocco = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blocco.name = "Maceria";
                blocco.transform.SetPositionAndRotation(centro, transform.rotation);
                // Un po' più piccoli dello spazio che occupano, così non si incastrano tra loro all'inizio.
                blocco.transform.localScale = asseSpessore == 0
                    ? new Vector3(spessore * 0.95f, altezzaBlocco * 0.95f, larghezzaBlocco * 0.95f)
                    : new Vector3(larghezzaBlocco * 0.95f, altezzaBlocco * 0.95f, spessore * 0.95f);
                if (materiale != null) blocco.GetComponent<Renderer>().sharedMaterial = materiale;

                Rigidbody fisica = blocco.AddComponent<Rigidbody>();
                fisica.mass = 20f;
                // I blocchi in alto partono più forte: il muro crolla verso l'esterno, un po' a caso.
                float altezzaRelativa = (j + 0.5f) / righe;
                fisica.AddForce(normale * spinta * (0.5f + altezzaRelativa) + Random.insideUnitSphere * spinta * 0.3f, ForceMode.VelocityChange);
                fisica.AddTorque(Random.insideUnitSphere * 3f, ForceMode.VelocityChange);
                macerie.Add(blocco);
            }
        }

        StartCoroutine(SparisciMacerie(macerie));
        Debug.Log(name + " è crollato: passaggio aperto!");
    }

    // Dopo qualche secondo i blocchi rimpiccioliscono e spariscono, così non bloccano il passaggio.
    IEnumerator SparisciMacerie(List<GameObject> macerie)
    {
        yield return new WaitForSeconds(durataMacerie);

        const float durata = 0.5f;
        List<Vector3> scale = macerie.ConvertAll(m => m.transform.localScale);
        for (float t = 0f; t < durata; t += Time.deltaTime)
        {
            for (int i = 0; i < macerie.Count; i++)
            {
                if (macerie[i] != null) macerie[i].transform.localScale = scale[i] * (1f - t / durata);
            }
            yield return null;
        }
        foreach (GameObject blocco in macerie)
        {
            if (blocco != null) Destroy(blocco);
        }
    }

    static float Tra(System.Random caso, float minimo, float massimo)
    {
        return minimo + (float)caso.NextDouble() * (massimo - minimo);
    }

    // Contorno arancione nella vista Scene, per ricordare che questo muro si può rompere.
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 1.02f);
    }
}
