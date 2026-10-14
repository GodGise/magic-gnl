using UnityEngine;

// Mostra addosso al personaggio gli oggetti equipaggiati, con le forme provvisorie di FormeOggetti
// (o con il modello di Nazar, quando un oggetto ce l'ha).
// A cosa serve: si vede cosa si ha addosso:
//   - mano destra: l'arma (Guerriero e Ladro) o il bastone (Stregone), al posto della spada e del bastone provvisori;
//   - mano sinistra: lo scudo (Guerriero, non con le armi a due mani), il secondo pugnale (Pugnali gemelli),
//     il libro (Stregone, non con i bastoni a due mani);
//   - schiena: arco o balestra (Ladro);
//   - busto: armatura o veste, sopra la figura;
//   - petto: l'amuleto.
// Ogni volta che l'equipaggiamento cambia, le forme si rifanno da sole.
// Funziona sulla figura provvisoria di AspettoUmanoide; con un modello vero del personaggio non fa niente.
// Co-op: per ora si vede solo sul proprio PC (gli altri giocatori vedono la figura di base).
// Come montarlo: non si monta. Lo aggiunge da solo Equipaggiamento, sul giocatore.
[RequireComponent(typeof(Equipaggiamento))]
public class AspettoEquipaggiamento : MonoBehaviour
{
    const string NomeFigura = "Aspetto umanoide";

    Equipaggiamento equipaggiamento;
    int firma = int.MinValue;   // cambia quando cambia qualcosa da mostrare

    void Awake() => equipaggiamento = GetComponent<Equipaggiamento>();

    void LateUpdate()
    {
        int nuova = Firma();
        if (nuova == firma) return;
        firma = nuova;
        Rifai();
    }

    // Un numero che riassume oggetti addosso, classe e mani presenti: se cambia, si rifà tutto.
    int Firma()
    {
        var e = equipaggiamento;
        unchecked
        {
            int h = 17;
            h = h * 31 + Id(e.Arma);
            h = h * 31 + Id(e.Scudo);
            h = h * 31 + Id(e.Armatura);
            h = h * 31 + Id(e.Amuleto);
            h = h * 31 + Id(e.ArmaDistanza);
            h = h * 31 + Id(e.Bastone);
            h = h * 31 + Id(e.LibroUsato);
            h = h * 31 + (int)SceltaPartita.Classe;
            Transform mano = Trova(transform, "Mano destra");
            h = h * 31 + (mano != null ? mano.childCount : -1);
            return h;
        }
    }

    static int Id(Object o) => o != null ? o.GetInstanceID() : 0;

    void Rifai()
    {
        Transform figura = Trova(transform, NomeFigura);
        Transform manoDestra = Trova(transform, "Mano destra");
        if (figura == null || manoDestra == null) return;   // nessuna figura provvisoria: c'è un modello vero

        var e = equipaggiamento;
        var classe = SceltaPartita.Classe;

        // Mano destra: i gruppi "Spada" e "Bastone" li accende e spegne GiocatoreControllo (vedi AspettoUmanoide.MostraArma).
        Transform spada = manoDestra.Find("Spada");
        if (spada != null)
        {
            Svuota(spada);
            if (e.Arma != null) FormeOggetti.Crea(e.Arma, spada);
            else FormeOggetti.SpadaSenzaArma(spada);
        }
        Transform bastone = manoDestra.Find("Bastone");
        if (bastone != null)
        {
            Svuota(bastone);
            if (e.Bastone != null) FormeOggetti.Crea(e.Bastone, bastone);
            else FormeOggetti.BastoneSenzaBastone(bastone);
        }

        // Mano sinistra.
        Transform sinistra = ManoSinistra(figura);
        if (sinistra != null)
        {
            Svuota(sinistra);
            if (classe == ClasseGiocatore.Stregone)
            {
                if (e.LibroUsato != null) FormeOggetti.Crea(e.LibroUsato, sinistra);
            }
            else if (e.Arma != null && e.Arma.tipo == DatiArma.Tipo.DoppiPugnali)
            {
                FormeOggetti.Crea(e.Arma, sinistra);
            }
            else if (e.Scudo != null && (e.Arma == null || !e.Arma.dueMani))
            {
                var scudo = FormeOggetti.Crea(e.Scudo, sinistra);
                scudo.localPosition = new Vector3(-0.05f, 0.05f, 0.05f);
                scudo.localRotation = Quaternion.Euler(35f, 0f, 0f);   // raddrizzato: la mano è inclinata in avanti
            }
        }

        // Schiena: arco o balestra, in diagonale.
        Transform schiena = Gruppo(figura, "Schiena (oggetti)", new Vector3(0f, 0.3f, -0.24f), Quaternion.Euler(0f, 0f, 30f));
        Svuota(schiena);
        if (e.ArmaDistanza != null) FormeOggetti.Crea(e.ArmaDistanza, schiena);

        // Busto: armatura o veste (il busto della figura è a 0,25 m sopra il centro).
        Transform busto = Gruppo(figura, "Armatura (oggetti)", new Vector3(0f, 0.25f, 0f), Quaternion.identity);
        Svuota(busto);
        if (e.Armatura != null) FormeOggetti.Crea(e.Armatura, busto);

        // Petto: amuleto, davanti all'armatura.
        float davanti = e.Armatura != null ? 0.22f : 0.18f;
        Transform petto = Gruppo(figura, "Amuleto (oggetti)", new Vector3(0f, 0.42f, davanti), Quaternion.identity);
        Svuota(petto);
        if (e.Amuleto != null) FormeOggetti.Crea(e.Amuleto, petto);
    }

    // La mano sinistra non c'è nella figura di base: la si crea come la destra, in fondo al braccio sinistro.
    static Transform ManoSinistra(Transform figura)
    {
        Transform mano = Trova(figura, "Mano sinistra");
        if (mano != null) return mano;
        Transform spalla = Trova(figura, "Spalla sinistra");
        if (spalla == null) return null;
        mano = new GameObject("Mano sinistra").transform;
        mano.SetParent(spalla, false);
        mano.localPosition = new Vector3(0f, -0.72f, 0f);
        mano.localRotation = Quaternion.Euler(-35f, 0f, 0f);
        return mano;
    }

    static Transform Gruppo(Transform figura, string nome, Vector3 posizione, Quaternion rotazione)
    {
        Transform g = figura.Find(nome);
        if (g != null) return g;
        g = new GameObject(nome).transform;
        g.SetParent(figura, false);
        g.localPosition = posizione;
        g.localRotation = rotazione;
        return g;
    }

    static void Svuota(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            Transform figlio = t.GetChild(i);
            figlio.SetParent(null, false);
            Destroy(figlio.gameObject);
        }
    }

    // Cerca un figlio con quel nome a qualsiasi profondità.
    static Transform Trova(Transform da, string nome)
    {
        foreach (Transform figlio in da)
        {
            if (figlio.name == nome) return figlio;
            Transform trovato = Trova(figlio, nome);
            if (trovato != null) return trovato;
        }
        return null;
    }
}
