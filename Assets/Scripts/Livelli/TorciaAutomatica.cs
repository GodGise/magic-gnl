using UnityEngine;

// Torcia che si accende da sola quando fa buio e si spegne quando torna il giorno.
// A cosa serve: le torce lungo le strade del villaggio restano spente di giorno (niente fiamma, niente luce)
// e si accendono al tramonto, una dopo l'altra con un piccolo ritardo diverso per ognuna, come se qualcuno
// le accendesse; all'alba si spengono. Legge il "Buio" del CicloGiornoNotte (0 = giorno pieno, 1 = notte).
// Se nella scena non c'è il ciclo giorno e notte, la torcia resta sempre accesa.
// La luce che tremola è quella del componente Torcia (Assets/Scripts/Ambiente): questo script la accende e la spegne soltanto.
// La luce viene sempre calcolata "per pixel": con tante torce, Unity illumina bene solo le poche più importanti
// e quali siano cambia mentre tremolano, così le luci sembravano guaste. Ogni torcia illumina solo il suo raggio,
// quindi il costo resta basso.
// Come montarlo: sull'oggetto della torcia (il "palo"), che deve avere fra i figli la luce (Light, con Torcia)
// e la fiamma (un oggetto che si chiama "Brace"). Il menu "magic-gnl > Crea Villaggio Lago Nero" lo mette da solo.
public class TorciaAutomatica : MonoBehaviour
{
    [Tooltip("Si accende quando il buio supera questo valore (0-1). Con 0,5 si accende verso le 17:40.")]
    [Range(0f, 1f)] [SerializeField] float sogliaAccensione = 0.5f;
    [Tooltip("Si spegne quando il buio scende sotto questo valore (0-1). Più basso della soglia di accensione, così non lampeggia.")]
    [Range(0f, 1f)] [SerializeField] float sogliaSpegnimento = 0.35f;
    [Tooltip("Ritardo massimo, in secondi, fra quando fa buio e quando questa torcia si accende (ognuna ne sceglie uno a caso).")]
    [SerializeField] float ritardoMassimo = 3f;
    [Tooltip("Quanto ci mette la fiamma a crescere quando si accende, in secondi.")]
    [SerializeField] float durataAccensione = 0.4f;

    Light luce;
    Transform fiamma;
    Vector3 grandezzaFiamma;
    bool accesa;
    bool inAttesa;
    float cambioAlle;
    float accesaDa;

    public bool Accesa => accesa;

    void Awake()
    {
        luce = GetComponentInChildren<Light>(true);
        if (luce != null) luce.renderMode = LightRenderMode.ForcePixel;
        fiamma = TrovaFiglio(transform, "Brace");
        if (fiamma != null) grandezzaFiamma = fiamma.localScale;
    }

    void Start()
    {
        // All'inizio niente ritardo: si parte già accesi di notte o spenti di giorno.
        Imposta(Buio() > sogliaAccensione, false);
    }

    void Update()
    {
        float buio = Buio();
        bool deveEssereAccesa = accesa ? buio > sogliaSpegnimento : buio > sogliaAccensione;

        if (deveEssereAccesa == accesa)
        {
            inAttesa = false;
        }
        else if (!inAttesa)
        {
            inAttesa = true;
            cambioAlle = Time.time + Random.Range(0f, ritardoMassimo);
        }
        else if (Time.time >= cambioAlle)
        {
            inAttesa = false;
            Imposta(deveEssereAccesa, true);
        }

        // La fiamma cresce piano quando si accende.
        if (accesa && fiamma != null && Time.time - accesaDa < durataAccensione)
            fiamma.localScale = grandezzaFiamma * Mathf.SmoothStep(0.1f, 1f, (Time.time - accesaDa) / durataAccensione);
    }

    void Imposta(bool accendi, bool conEffetto)
    {
        accesa = accendi;
        if (luce != null) luce.enabled = accendi;
        if (fiamma != null)
        {
            fiamma.gameObject.SetActive(accendi);
            fiamma.localScale = grandezzaFiamma * (accendi && conEffetto ? 0.1f : 1f);
        }
        accesaDa = Time.time;
    }

    static float Buio() => CicloGiornoNotte.Istanza != null ? CicloGiornoNotte.Istanza.Buio : 1f;

    static Transform TrovaFiglio(Transform da, string nome)
    {
        foreach (Transform figlio in da)
        {
            if (figlio.name == nome) return figlio;
            Transform trovato = TrovaFiglio(figlio, nome);
            if (trovato != null) return trovato;
        }
        return null;
    }
}
