using System.Collections.Generic;
using UnityEngine;

// Una casa del villaggio che la razzia degli orchi può danneggiare, incendiare e distruggere (Lorenzo, 13 ottobre).
// Stati, in ordine: Intatta -> Danneggiata (assi rotte, macchie scure, porta sfondata) -> In fiamme (fuoco e fumo
// dal tetto, luce che tremola, crepitio) -> Distrutta (il tetto crolla: la casa si abbassa, resta annerita, braci e
// fumo). Non si torna indietro.
// Chi la rovina: gli orchi della razzia (RazziatoreOrco) la colpiscono, ne sfondano la porta, ci danno fuoco.
// All'inizio della razzia (RazziaVillaggio > Inizia) ogni casa passa allo stato "Alla Razzia": così quando il giocatore
// esce dalla taverna il villaggio brucia già in più punti.
// È tutto provvisorio, fatto di cubi e particelle: le texture e i modelli veri delle case rovinate e in fiamme sono
// nella lista per Nazar (Docs/lista-modelli-nazar.txt). La pianta della casa non cambia: si cambia solo l'aspetto.
// Come montarlo: sul cubo di una casa del villaggio (le copie di "casa-blocco" e gli edifici speciali). Lo mette da
// solo il comando "magic-gnl > Prologo: prepara la quest 6 e la razzia", con i materiali di fuoco, fumo e annerito.
public class CasaInRovina : MonoBehaviour
{
    public enum Stato { Intatta, Danneggiata, InFiamme, Distrutta }

    [Tooltip("Com'è la casa quando parte la razzia (gli orchi hanno già cominciato).")]
    [SerializeField] Stato statoAllaRazzia = Stato.Intatta;
    [Tooltip("Danno dei colpi degli orchi per passare da intatta a danneggiata.")]
    [SerializeField, Min(1f)] float resistenza = 60f;
    [Tooltip("Danno dei colpi, dopo danneggiata, per prendere fuoco da sola (0 = solo con la torcia).")]
    [SerializeField, Min(0f)] float dannoPerIncendio = 0f;
    [Tooltip("Secondi di fuoco prima che il tetto crolli.")]
    [SerializeField, Min(5f)] float secondiPerBruciare = 100f;
    [Tooltip("Secondi perché il fuoco arrivi al massimo dopo l'innesco.")]
    [SerializeField, Min(0.5f)] float secondiPerDivampare = 10f;

    [Header("Materiali (li mette il comando che prepara la razzia)")]
    [SerializeField] Material materialeFuoco;
    [SerializeField] Material materialeFumo;
    [SerializeField] Material materialeBruciato;
    [SerializeField] Material materialeAssi;

    public Stato Attuale { get; private set; } = Stato.Intatta;
    public bool PortaSfondata { get; private set; }
    // Il punto davanti alla porta (o al centro della facciata), a terra, e la direzione verso fuori.
    public Vector3 Davanti => davanti;
    public Vector3 Fuori => fuori;

    public static readonly List<CasaInRovina> Tutte = new List<CasaInRovina>();

    float danni;
    float innesco;           // da 0 a 1 mentre la torcia scalda il legno
    float accesaDa = -1f;
    float crolloDa = -1f;
    Vector3 davanti, fuori, dimensioni, scalaIntatta;
    Transform porta;
    Renderer aspetto;
    Color coloreIntatto;
    ParticleSystem fuoco, fumo;
    Light luceFuoco;
    float prossimoCrepitio;
    readonly List<GameObject> segni = new List<GameObject>();

    void Awake()
    {
        aspetto = GetComponent<Renderer>();
        if (aspetto != null) coloreIntatto = aspetto.material.color;   // copia del materiale: si annerisce solo questa casa
        scalaIntatta = transform.localScale;
        dimensioni = transform.lossyScale;
        porta = transform.Find("Porta");
        Vector3 centro = transform.position;
        centro.y = 0f;
        if (porta != null && porta.gameObject.activeSelf)
        {
            davanti = porta.position;
            davanti.y = 0f;
            fuori = davanti - centro;
            fuori.y = 0f;
            fuori = fuori.sqrMagnitude > 0.01f ? fuori.normalized : -transform.forward;
        }
        else
        {
            fuori = -transform.forward;
            davanti = centro + fuori * dimensioni.z * 0.5f;
        }
    }

    void OnEnable() { if (!Tutte.Contains(this)) Tutte.Add(this); }
    void OnDisable() => Tutte.Remove(this);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Azzera() => Tutte.Clear();

    // ---------- comandi ----------

    // La razzia è cominciata: la casa diventa com'è scritto in "Alla Razzia".
    public void Razzia()
    {
        if (statoAllaRazzia >= Stato.Danneggiata) Danneggia();
        if (statoAllaRazzia == Stato.Danneggiata && Random.value < 0.5f) SfondaPorta();
        if (statoAllaRazzia >= Stato.InFiamme)
        {
            Incendia();
            // già accesa da un po': il fuoco è alto da subito, e chi doveva crollare crolla presto
            accesaDa = Time.time - secondiPerDivampare - Random.Range(0f, secondiPerBruciare * 0.5f);
        }
        if (statoAllaRazzia == Stato.Distrutta) accesaDa = Time.time - secondiPerBruciare + Random.Range(1f, 6f);
    }

    // Un colpo d'orco (o di qualsiasi cosa) sulla casa, nel punto "dove".
    public void Colpisci(float danno, Vector3 dove)
    {
        if (Attuale == Stato.Distrutta) return;
        danni += danno;
        Schegge(dove, 5);
        if (Attuale == Stato.Intatta && danni >= resistenza) Danneggia();
        if (Attuale == Stato.Danneggiata && dannoPerIncendio > 0f && danni >= resistenza + dannoPerIncendio) Incendia();
    }

    // La porta vola via a terra (resta lì, storta) e la casa conta danneggiata.
    public void SfondaPorta()
    {
        if (PortaSfondata) return;
        PortaSfondata = true;
        if (porta != null && porta.gameObject.activeSelf)
        {
            // la porta finisce stesa a terra davanti alla soglia, un po' storta
            Vector3 dove = davanti + fuori * 0.9f + Vector3.up * 0.06f;
            Pezzo("Porta sfondata", dove, new Vector3(1.5f, 0.1f, 2.1f), materialeAssi,
                Quaternion.LookRotation(fuori) * Quaternion.Euler(0f, Random.Range(-25f, 25f), 0f));
            porta.gameObject.SetActive(false);
            // il vano resta buio
            Pezzo("Vano della porta", davanti + Vector3.up * 1.1f - fuori * 0.02f, new Vector3(1.5f, 2.15f, 0.05f), materialeBruciato,
                Quaternion.LookRotation(fuori));
        }
        Suoni.Suona(Suono.LegnoRotto, davanti + Vector3.up, 1f, 0.85f);
        Schegge(davanti + Vector3.up, 12);
        if (Attuale == Stato.Intatta) Danneggia();
    }

    // La torcia scalda il legno: "quanto" si somma, a 1 la casa prende fuoco.
    public void Scalda(float quanto)
    {
        if (Attuale >= Stato.InFiamme) return;
        innesco += quanto;
        if (innesco >= 1f) Incendia();
    }

    // ---------- stati ----------

    void Danneggia()
    {
        if (Attuale >= Stato.Danneggiata) return;
        Attuale = Stato.Danneggiata;
        if (aspetto != null) aspetto.material.color = Color.Lerp(coloreIntatto, new Color(0.18f, 0.15f, 0.12f), 0.3f);
        // assi rotte e macchie scure sulla facciata, un mucchio di legno a terra
        float largo = Larghezza(), alto = dimensioni.y;
        Vector3 lato = Vector3.Cross(Vector3.up, fuori);
        for (int i = 0; i < 3; i++)
        {
            Vector3 p = davanti + lato * Random.Range(-largo * 0.42f, largo * 0.42f) + Vector3.up * Random.Range(0.6f, alto * 0.85f) + fuori * 0.03f;
            Pezzo("Macchia", p, new Vector3(Random.Range(0.6f, 1.4f), Random.Range(0.4f, 1f), 0.04f), materialeBruciato, Quaternion.LookRotation(fuori) * Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f)));
        }
        for (int i = 0; i < 4; i++)
        {
            Vector3 p = davanti + lato * Random.Range(-largo * 0.4f, largo * 0.4f) + fuori * Random.Range(0.4f, 1.6f) + Vector3.up * 0.05f;
            Pezzo("Asse rotta", p, new Vector3(0.18f, 0.06f, Random.Range(0.8f, 1.6f)), materialeAssi, Quaternion.Euler(0f, Random.Range(0f, 180f), Random.Range(-8f, 8f)));
        }
    }

    void Incendia()
    {
        if (Attuale >= Stato.InFiamme) return;
        Danneggia();
        Attuale = Stato.InFiamme;
        accesaDa = Time.time;
        CreaFuoco();
        Suoni.Suona(Suono.FuocoAcceso, transform.position + Vector3.up * dimensioni.y * 0.5f, 1f, 0.7f);
    }

    void Crolla()
    {
        Attuale = Stato.Distrutta;
        crolloDa = Time.time;
        Suoni.Suona(Suono.CrolloMuro, transform.position, 1f, 0.8f);
        Schegge(transform.position + Vector3.up * dimensioni.y * 0.5f, 25);
    }

    // ---------- tempo ----------

    void Update()
    {
        if (Attuale < Stato.InFiamme) return;
        float acceso = Time.time - accesaDa;
        float forza = Mathf.Clamp01(acceso / secondiPerDivampare);

        if (Attuale == Stato.InFiamme && acceso >= secondiPerBruciare) Crolla();
        if (Attuale == Stato.Distrutta)
        {
            // il tetto cede in 2 secondi: la casa si abbassa a meno di metà e diventa nera; restano braci e fumo
            float c = Mathf.Clamp01((Time.time - crolloDa) / 2f);
            float altezza = Mathf.Lerp(1f, 0.42f, c * c);
            Vector3 scala = scalaIntatta;
            scala.y *= altezza;
            transform.localScale = scala;
            Vector3 p = transform.position;
            p.y = transform.lossyScale.y * 0.5f;
            transform.position = p;
            if (aspetto != null) aspetto.material.color = Color.Lerp(aspetto.material.color, new Color(0.07f, 0.06f, 0.055f), c);
            forza = Mathf.Lerp(1f, 0.3f, c);
        }

        if (fuoco != null)
        {
            var emissione = fuoco.emission;
            emissione.rateOverTimeMultiplier = Mathf.Lerp(4f, 26f, forza) * Mathf.Clamp(Larghezza() / 6f, 0.6f, 2f);
            var forma = fuoco.shape;
            forma.position = new Vector3(0f, AltezzaFuoco(), 0f);
        }
        if (fumo != null)
        {
            var forma = fumo.shape;
            forma.position = new Vector3(0f, AltezzaFuoco() + 1f, 0f);
        }
        if (luceFuoco != null)
        {
            luceFuoco.transform.position = new Vector3(transform.position.x, AltezzaFuoco() + 1.5f, transform.position.z) + fuori * 1.5f;
            luceFuoco.intensity = forza * (2.2f + 0.8f * Mathf.PerlinNoise(Time.time * 7f, transform.position.x));
        }

        // crepitio ogni tanto, solo se il giocatore è abbastanza vicino da sentirlo
        if (Time.time >= prossimoCrepitio)
        {
            prossimoCrepitio = Time.time + Random.Range(1.2f, 2.6f);
            var cam = Camera.main;
            if (cam != null && (cam.transform.position - transform.position).sqrMagnitude < 35f * 35f)
                Suoni.Suona(Suono.Crepitio, transform.position + Vector3.up * AltezzaFuoco(), 0.6f * forza, Random.Range(0.85f, 1.1f));
        }
    }

    float AltezzaFuoco() => transform.position.y + transform.lossyScale.y * 0.5f;
    float Larghezza() => Mathf.Abs(Vector3.Dot(Vector3.Cross(Vector3.up, fuori), transform.right)) > 0.7f ? dimensioni.x : dimensioni.z;

    // ---------- pezzi provvisori ----------

    // Un cubo senza collider, figlio del mondo (non della casa, che è scalata e si abbassa crollando).
    GameObject Pezzo(string nome, Vector3 dove, Vector3 scala, Material materiale, Quaternion rotazione)
    {
        var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
        o.name = nome;
        Destroy(o.GetComponent<Collider>());
        o.transform.SetParent(transform.parent, true);
        o.transform.SetPositionAndRotation(dove, rotazione);
        o.transform.localScale = scala;
        var r = o.GetComponent<Renderer>();
        if (materiale != null) r.sharedMaterial = materiale;
        else r.material.color = new Color(0.12f, 0.1f, 0.08f);
        segni.Add(o);
        return o;
    }

    // Una manciata di schegge che saltano via dal punto colpito.
    void Schegge(Vector3 dove, int quante)
    {
        if (materialeAssi == null) return;
        var o = new GameObject("Schegge");
        o.transform.position = dove;
        var ps = o.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
        main.gravityModifier = 1.6f;
        main.startColor = new Color(0.3f, 0.21f, 0.13f);
        main.stopAction = ParticleSystemStopAction.Destroy;
        var emissione = ps.emission;
        emissione.rateOverTime = 0f;
        emissione.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)quante) });
        var forma = ps.shape;
        forma.shapeType = ParticleSystemShapeType.Hemisphere;
        forma.radius = 0.3f;
        o.transform.rotation = Quaternion.LookRotation(Vector3.up);
        var r = o.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Mesh;
        r.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        r.sharedMaterial = materialeAssi;
        ps.Play();
    }

    // Se manca il materiale (casa preparata a mano): uno semplice, così le particelle non sono rosa.
    static Material Riserva(string shader)
    {
        var s = Shader.Find(shader);
        return s != null ? new Material(s) : null;
    }

    void CreaFuoco()
    {
        float largo = dimensioni.x, profondo = dimensioni.z;
        // fiamme dal tetto: un rettangolo grande quanto la casa
        var oFuoco = new GameObject("Fuoco della casa");
        oFuoco.transform.SetParent(transform.parent, false);
        oFuoco.transform.SetPositionAndRotation(new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
        fuoco = oFuoco.AddComponent<ParticleSystem>();
        fuoco.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = fuoco.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.7f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.75f, 0.3f), new Color(1f, 0.35f, 0.08f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;
        var forma = fuoco.shape;
        forma.shapeType = ParticleSystemShapeType.Box;
        forma.rotation = new Vector3(-90f, 0f, 0f);   // la scatola sdraiata: le fiamme vanno verso l'alto
        forma.scale = new Vector3(largo * 0.85f, profondo * 0.85f, 0.4f);
        forma.position = new Vector3(0f, AltezzaFuoco(), 0f);
        var colore = fuoco.colorOverLifetime;
        colore.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.85f, 0.4f), 0f), new GradientColorKey(new Color(1f, 0.35f, 0.05f), 0.5f), new GradientColorKey(new Color(0.4f, 0.08f, 0.02f), 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.15f), new GradientAlphaKey(0f, 1f) });
        colore.color = g;
        var grandezza = fuoco.sizeOverLifetime;
        grandezza.enabled = true;
        grandezza.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));
        var rf = oFuoco.GetComponent<ParticleSystemRenderer>();
        rf.sharedMaterial = materialeFuoco != null ? materialeFuoco : Riserva("Legacy Shaders/Particles/Additive");
        fuoco.Play();

        // fumo nero che sale e si allarga
        var oFumo = new GameObject("Fumo della casa");
        oFumo.transform.SetParent(oFuoco.transform, false);
        fumo = oFumo.AddComponent<ParticleSystem>();
        fumo.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var mf = fumo.main;
        mf.loop = true;
        mf.startLifetime = new ParticleSystem.MinMaxCurve(4f, 6.5f);
        mf.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        mf.startSize = new ParticleSystem.MinMaxCurve(1.6f, 3f);
        mf.startColor = new Color(0.08f, 0.07f, 0.07f, 0.55f);
        mf.simulationSpace = ParticleSystemSimulationSpace.World;
        mf.maxParticles = 80;
        var ef = fumo.emission;
        ef.rateOverTime = 7f;
        var sf = fumo.shape;
        sf.shapeType = ParticleSystemShapeType.Box;
        sf.rotation = new Vector3(-90f, 0f, 0f);
        sf.scale = new Vector3(largo * 0.6f, profondo * 0.6f, 0.4f);
        sf.position = new Vector3(0f, AltezzaFuoco() + 1f, 0f);
        var gf = fumo.sizeOverLifetime;
        gf.enabled = true;
        gf.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 2.2f));
        var cf = fumo.colorOverLifetime;
        cf.enabled = true;
        var g2 = new Gradient();
        g2.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                   new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
        cf.color = g2;
        var vf = fumo.velocityOverLifetime;   // il vento lo piega un poco
        vf.enabled = true;
        vf.space = ParticleSystemSimulationSpace.World;
        vf.x = new ParticleSystem.MinMaxCurve(0.6f, 0.6f);
        vf.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        vf.z = new ParticleSystem.MinMaxCurve(0.2f, 0.2f);
        var rfu = oFumo.GetComponent<ParticleSystemRenderer>();
        rfu.sharedMaterial = materialeFumo != null ? materialeFumo : Riserva("Legacy Shaders/Particles/Alpha Blended");
        fumo.Play();

        // luce arancione che tremola, davanti alla facciata
        var oLuce = new GameObject("Luce del fuoco");
        oLuce.transform.SetParent(oFuoco.transform, false);
        luceFuoco = oLuce.AddComponent<Light>();
        luceFuoco.type = LightType.Point;
        luceFuoco.color = new Color(1f, 0.5f, 0.18f);
        luceFuoco.range = Mathf.Clamp(Mathf.Max(largo, profondo) * 2f, 10f, 18f);
        luceFuoco.intensity = 0f;
    }
}
