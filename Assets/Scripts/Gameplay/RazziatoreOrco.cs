using UnityEngine;

// Un orco della razzia di Villaggio Lago Nero mentre non combatte: distrugge il villaggio (Lorenzo, 13 ottobre).
// Cosa fa, fermo al suo posto davanti a una casa (CasaInRovina):
//   - Colpire la parete: colpi dall'alto con la mazza o l'ascia; la casa si rovina;
//   - Sfondare la porta: colpi di lato finché la porta vola via, poi passa a colpire la parete;
//   - Calciare: calci alla porta e alle assi;
//   - Dare fuoco: con la torcia nella mano sinistra scalda il legno finché la casa prende fuoco.
// Quando la sua casa brucia o è crollata va alla casa vicina (entro "Raggio Prossima Casa") e ricomincia.
// Appena vede il giocatore (campo visivo di InseguimentoNemico) smette, grida e lo attacca; se lo perde torna al
// suo posto e riprende. Mentre spacca è "ignaro": lo si può giustiziare alle spalle con l'esecuzione furtiva.
// Ogni tanto ringhia (Suono UrloOrco).
// "Corazzato": elmo, spallacci e piastra sul petto di ferro scuro (gli orchi della seconda ondata con l'ascia).
// Le pose sono provvisorie (figura a blocchi, AnimazioneUmanoide.FaiGesto): le animazioni vere sono nella lista
// per Nazar (Docs/lista-modelli-nazar.txt).
// Come montarlo: su un orco (Bersaglio + InseguimentoNemico), con "Casa" e "Azione". Lo mette da solo il comando
// "magic-gnl > Prologo: prepara la quest 6 e la razzia".
[RequireComponent(typeof(InseguimentoNemico))]
public class RazziatoreOrco : MonoBehaviour
{
    public enum Azione { ColpireParete, SfondarePorta, Calciare, DareFuoco }

    [SerializeField] Azione azione = Azione.ColpireParete;
    [SerializeField] CasaInRovina casa;
    [Tooltip("Secondi di un gesto (un colpo, un calcio).")]
    [SerializeField, Min(0.4f)] float secondiPerGesto = 1.5f;
    [Tooltip("Danno alla casa a ogni colpo.")]
    [SerializeField, Min(0f)] float dannoAllaCasa = 12f;
    [Tooltip("Secondi di torcia perché una casa prenda fuoco.")]
    [SerializeField, Min(1f)] float secondiPerIncendiare = 7f;
    [Tooltip("Colpi alla porta prima di sfondarla.")]
    [SerializeField, Min(1)] int colpiPerLaPorta = 4;
    [Tooltip("Quando la casa è finita, cerca un'altra casa entro questi metri.")]
    [SerializeField] float raggioProssimaCasa = 30f;
    [SerializeField] bool corazzato;

    Bersaglio bersaglio;
    InseguimentoNemico vista;
    AnimazioneUmanoide animazione;
    float fase;
    bool colpoFatto;
    int colpiPorta;
    float prossimoRinghio;
    bool inseguivaPrima;
    float cercaDa;

    public Azione AzioneAttuale => azione;
    public CasaInRovina Casa => casa;

    void Awake()
    {
        bersaglio = GetComponent<Bersaglio>();
        vista = GetComponent<InseguimentoNemico>();
        prossimoRinghio = Time.time + Random.Range(3f, 10f);
        fase = Random.value;
    }

    void Start()
    {
        animazione = GetComponent<AnimazioneUmanoide>();
        vista.ImpostaSguardo(0f);   // concentrato sul lavoro: non si guarda intorno (ma vede chi gli passa davanti)
        if (azione == Azione.DareFuoco) CreaTorcia();
        if (corazzato) CreaArmatura();
    }

    // Cambia lavoro (lo usa RazziaVillaggio per la seconda ondata): casa e azione, e il posto davanti alla casa.
    public void Assegna(CasaInRovina nuova, Azione nuovaAzione)
    {
        casa = nuova;
        azione = nuovaAzione;
        colpiPorta = 0;
        if (casa != null) vista.CambiaPosto(PostoDavanti(casa), -casa.Fuori);
    }

    void Update()
    {
        if (bersaglio.Morto || vista == null) return;
        // appena si accorge del giocatore: un urlo di guerra
        bool insegue = vista.StaInseguendo;
        if (insegue && !inseguivaPrima) Suoni.Suona(Suono.UrloOrco, transform.position + Vector3.up, 0.9f, Random.Range(0.9f, 1.1f));
        inseguivaPrima = insegue;
        if (!vista.AlPosto || !vista.Ignaro || casa == null) return;

        if (casa.Attuale == CasaInRovina.Stato.Distrutta || (azione == Azione.DareFuoco && casa.Attuale >= CasaInRovina.Stato.InFiamme))
        {
            ProssimaCasa();
            return;
        }
        if (azione == Azione.SfondarePorta && casa.PortaSfondata) azione = Azione.ColpireParete;

        // girato verso la casa
        Vector3 verso = -casa.Fuori;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(verso), 240f * Time.deltaTime);

        fase += Time.deltaTime / secondiPerGesto;
        if (fase >= 1f) { fase -= 1f; colpoFatto = false; }
        var gesto = azione == Azione.ColpireParete ? AnimazioneUmanoide.Gesto.ColpoAlto
                  : azione == Azione.SfondarePorta ? AnimazioneUmanoide.Gesto.ColpoLato
                  : azione == Azione.Calciare ? AnimazioneUmanoide.Gesto.Calcio
                  : AnimazioneUmanoide.Gesto.Torcia;
        if (animazione != null) animazione.FaiGesto(gesto, fase);

        if (azione == Azione.DareFuoco)
        {
            casa.Scalda(Time.deltaTime / secondiPerIncendiare);
        }
        else if (!colpoFatto && fase >= (azione == Azione.Calciare ? 0.56f : 0.66f))
        {
            colpoFatto = true;
            Vector3 punto = transform.position + transform.forward * 1.1f + Vector3.up * (azione == Azione.Calciare ? -0.5f : 0.3f);
            casa.Colpisci(dannoAllaCasa, punto);
            bool legno = azione != Azione.ColpireParete || !casa.PortaSfondata;
            Suoni.Suona(legno ? Suono.LegnoRotto : Suono.ColpoMuro, punto, 0.75f, Random.Range(0.85f, 1.15f));
            if (azione != Azione.ColpireParete && !casa.PortaSfondata && ++colpiPorta >= colpiPerLaPorta) casa.SfondaPorta();
        }

        if (Time.time >= prossimoRinghio)
        {
            prossimoRinghio = Time.time + Random.Range(6f, 14f);
            Suoni.Suona(Suono.UrloOrco, transform.position + Vector3.up, 0.45f, Random.Range(0.8f, 1.05f));
        }
    }

    // La casa vicina ancora da rovinare (non distrutta; per chi dà fuoco, non già in fiamme).
    void ProssimaCasa()
    {
        if (Time.time < cercaDa) return;
        cercaDa = Time.time + 2f;
        CasaInRovina migliore = null;
        float minima = raggioProssimaCasa * raggioProssimaCasa;
        foreach (var c in CasaInRovina.Tutte)
        {
            if (c == null || c == casa || c.Attuale == CasaInRovina.Stato.Distrutta) continue;
            if (azione == Azione.DareFuoco && c.Attuale >= CasaInRovina.Stato.InFiamme) continue;
            float d = (c.Davanti - transform.position).sqrMagnitude;
            if (d < minima) { minima = d; migliore = c; }
        }
        if (migliore == null) return;
        Assegna(migliore, azione == Azione.ColpireParete && !migliore.PortaSfondata ? Azione.SfondarePorta : azione);
        vista.VaiAlPosto(false);
    }

    public static Vector3 PostoDavanti(CasaInRovina c) => c.Davanti + c.Fuori * 1.5f + Vector3.up * 1.1f;

    // ---------- aspetto provvisorio ----------

    // Una torcia nella mano sinistra: bastone, fiamma e una piccola luce.
    void CreaTorcia()
    {
        Transform spalla = Trova(transform, "Spalla sinistra");
        if (spalla == null) return;
        Material base_ = PrimoMateriale();
        var mano = new GameObject("Mano sinistra").transform;
        mano.SetParent(spalla, false);
        mano.localPosition = new Vector3(0f, -0.72f, 0f);
        mano.localRotation = Quaternion.Euler(-35f, 0f, 0f);
        Parte("Manico torcia", mano, new Vector3(0f, -0.25f, 0f), new Vector3(0.06f, 0.6f, 0.06f), Colore(base_, new Color(0.25f, 0.16f, 0.09f), false));
        Parte("Fiamma torcia", mano, new Vector3(0f, -0.62f, 0f), new Vector3(0.16f, 0.24f, 0.16f), Colore(base_, new Color(1f, 0.55f, 0.15f), true));
        var luce = new GameObject("Luce torcia").AddComponent<Light>();
        luce.transform.SetParent(mano, false);
        luce.transform.localPosition = new Vector3(0f, -0.7f, 0f);
        luce.type = LightType.Point;
        luce.color = new Color(1f, 0.55f, 0.2f);
        luce.range = 6f;
        luce.intensity = 1.4f;
    }

    // Elmo con le corna corte, spallacci e piastra sul petto.
    void CreaArmatura()
    {
        Transform figura = transform.Find("Aspetto umanoide");
        if (figura == null) return;
        Material ferro = Colore(PrimoMateriale(), new Color(0.24f, 0.24f, 0.26f), false);
        Parte("Elmo", figura, new Vector3(0f, 0.9f, 0f), new Vector3(0.38f, 0.2f, 0.38f), ferro);
        Parte("Paranaso", figura, new Vector3(0f, 0.82f, 0.17f), new Vector3(0.05f, 0.14f, 0.04f), ferro);
        Parte("Corno sinistro", figura, new Vector3(-0.2f, 1f, 0f), new Vector3(0.05f, 0.18f, 0.05f), ferro, new Vector3(0f, 0f, 30f));
        Parte("Corno destro", figura, new Vector3(0.2f, 1f, 0f), new Vector3(0.05f, 0.18f, 0.05f), ferro, new Vector3(0f, 0f, -30f));
        Parte("Piastra", figura, new Vector3(0f, 0.3f, 0.17f), new Vector3(0.5f, 0.5f, 0.05f), ferro);
        foreach (string nome in new[] { "Spalla sinistra", "Spalla destra" })
        {
            Transform spalla = Trova(figura, nome);
            if (spalla != null) Parte("Spallaccio", spalla, new Vector3(0f, 0.02f, 0f), new Vector3(0.3f, 0.12f, 0.3f), ferro);
        }
    }

    Material PrimoMateriale()
    {
        var r = GetComponentInChildren<Renderer>();
        return r != null ? r.sharedMaterial : null;
    }

    static Material Colore(Material base_, Color c, bool luminoso)
    {
        if (base_ == null) return null;
        var m = new Material(base_) { color = c };
        if (luminoso)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * 2f);
        }
        return m;
    }

    static void Parte(string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale, Vector3 rotazione = default)
    {
        var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
        o.name = nome;
        Destroy(o.GetComponent<Collider>());
        o.transform.SetParent(genitore, false);
        o.transform.localPosition = posizione;
        o.transform.localRotation = Quaternion.Euler(rotazione);
        o.transform.localScale = scala;
        if (materiale != null) o.GetComponent<Renderer>().sharedMaterial = materiale;
    }

    static Transform Trova(Transform t, string nome)
    {
        foreach (var figlio in t.GetComponentsInChildren<Transform>(true))
            if (figlio.name == nome) return figlio;
        return null;
    }
}
