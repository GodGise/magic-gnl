using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Nemico di prova per allenare le tre mosse.
// A cosa serve: incassa i colpi del giocatore (lampeggia di bianco e indietreggia) e, se
// "Attacca Il Giocatore" è attivo, ogni pochi secondi si gira verso di lui, diventa rosso
// (preavviso) e poi colpisce. Il momento giusto per schivare o parare è la fine del rosso.
// Se è un cilindro di Unity, all'avvio prende un aspetto provvisorio da figura umana (vedi AspettoUmanoide):
// rosso, lampo bianco e scomparsa alla morte valgono per tutta la figura.
// Il danno dei colpi passa da CalcoloDanno (vedi Statistiche): l'armatura del nemico riduce i colpi del giocatore,
// e anche il nemico può fare colpi critici. Un critico ricevuto lo fa lampeggiare di giallo e lo spinge più lontano.
// Con "Rinasce" attivo, dopo la morte torna in vita dopo qualche secondo (per allenarsi); spento, resta morto.
// Come montarlo: su qualunque oggetto con un Collider (per esempio un cilindro).
// Il menu "magic-gnl > Crea scena di prova" ne mette uno già pronto.
public class Bersaglio : MonoBehaviour
{
    [SerializeField] float vitaMassima = 100f;
    [Tooltip("Se attivo, dopo la morte il nemico rinasce (utile per allenarsi). Se spento, resta morto.")]
    [SerializeField] bool rinasce = true;
    [SerializeField] float secondiPerRinascere = 2f;
    [SerializeField] float spintaQuandoColpito = 0.3f;

    [Header("Attacco di prova")]
    public bool attaccaIlGiocatore = true;
    [SerializeField] float intervalloAttacchi = 3f;
    [Tooltip("Secondi in cui resta rosso prima di colpire.")]
    [SerializeField] float preavviso = 0.7f;
    [SerializeField] float portataAttacco = 2.5f;
    [Tooltip("Danno dell'arma del nemico, prima dell'armatura del giocatore e dei critici (vedi Statistiche).")]
    [SerializeField] float dannoAttacco = 20f;

    float vita;
    bool morto;

    public bool Morto => morto;
    public float VitaMassima => vitaMassima;
    // Avvisa chi è interessato (per esempio InseguimentoNemico) che il nemico è stato colpito.
    public event System.Action Colpito;
    bool staAttaccando;
    float inizioAttacco;

    // Letti da AnimazioneUmanoide: durante il preavviso il nemico carica il colpo, poi colpisce.
    public bool StaAttaccando => staAttaccando;
    public float TempoAttacco => Time.time - inizioAttacco;
    public float DurataPreavviso => preavviso;
    public int NumeroAttacco { get; private set; } // cresce a ogni attacco: serve a cambiare movimento
    float prossimoAttacco;
    float sbilanciatoFino; // dopo una parata perfetta del giocatore non attacca fino a questo momento
    // Tutte le parti visibili del nemico, ognuna con il suo colore di partenza.
    Renderer[] aspetto;
    Color[] coloriBase;
    Collider corpo;
    GiocatoreControllo giocatore;
    Statistiche statistiche;

    public Statistiche Statistiche => statistiche;

    void Awake()
    {
        statistiche = Statistiche.Di(this);
        AspettoUmanoide.Prepara(gameObject, new Color(0.35f, 0.04f, 0.04f), AspettoUmanoide.Arma.Mazza);

        // Solo le parti accese: la forma originale nascosta dalla figura umana resta spenta.
        var parti = new List<Renderer>();
        foreach (Renderer parte in GetComponentsInChildren<Renderer>())
        {
            if (parte.enabled) parti.Add(parte);
        }
        aspetto = parti.ToArray();
        coloriBase = new Color[aspetto.Length];
        for (int i = 0; i < aspetto.Length; i++) coloriBase[i] = aspetto[i].material.color;

        corpo = GetComponent<Collider>();
        vita = vitaMassima;
    }

    void Start()
    {
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
        prossimoAttacco = Time.time + intervalloAttacchi;
    }

    void Update()
    {
        if (morto || staAttaccando || !attaccaIlGiocatore || giocatore == null) return;
        if (Time.time < prossimoAttacco || Time.time < sbilanciatoFino) return;

        // Attacca solo se il giocatore è abbastanza vicino da vedere il preavviso.
        if (Vector3.Distance(transform.position, giocatore.transform.position) <= portataAttacco * 2f)
            StartCoroutine(Attacca());
        else
            prossimoAttacco = Time.time + 0.5f;
    }

    IEnumerator Attacca()
    {
        staAttaccando = true;
        inizioAttacco = Time.time;
        NumeroAttacco++;

        Vector3 verso = giocatore.transform.position - transform.position;
        verso.y = 0f;
        if (verso.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(verso);

        ImpostaColore(Color.red);
        yield return new WaitForSeconds(preavviso);

        if (!morto)
        {
            RipristinaColori();
            Suoni.Suona(Suono.Fendente, transform.position + Vector3.up, 0.8f, 0.75f);
            if (Vector3.Distance(transform.position, giocatore.transform.position) <= portataAttacco)
            {
                float danno = CalcoloDanno.Calcola(dannoAttacco, statistiche, giocatore.Statistiche, out bool critico);
                giocatore.RiceviColpo(danno, transform.position, critico, this);
            }
        }

        prossimoAttacco = Time.time + intervalloAttacchi;
        staAttaccando = false;
    }

    // Chiamato dal giocatore quando un suo colpo va a segno.
    // "danno" è già calcolato (armatura e critico compresi, vedi CalcoloDanno).
    public void RiceviColpo(float danno, Vector3 origineColpo, bool critico = false)
    {
        if (morto) return;

        vita -= danno;
        Debug.Log(name + (critico ? " colpito con un CRITICO: -" : " colpito: -") + danno.ToString("0.#") + ", vita " + Mathf.Max(0f, vita).ToString("0.#"));
        Colpito?.Invoke();

        Vector3 spinta = transform.position - origineColpo;
        spinta.y = 0f;
        if (spinta.sqrMagnitude > 0.0001f) transform.position += spinta.normalized * spintaQuandoColpito * (critico ? 2f : 1f);

        if (vita <= 0f)
        {
            Muori();
            return;
        }
        if (!staAttaccando) StartCoroutine(Lampeggia(critico));
    }

    // Chiamato dal giocatore dopo una parata perfetta: il nemico indietreggia, lampeggia di azzurro
    // e non attacca per "durata" secondi.
    public void Sbilancia(float durata, Vector3 daDove)
    {
        if (morto) return;
        sbilanciatoFino = Time.time + durata;
        Vector3 spinta = transform.position - daDove;
        spinta.y = 0f;
        if (spinta.sqrMagnitude > 0.0001f) transform.position += spinta.normalized * spintaQuandoColpito * 2f;
        StartCoroutine(LampeggiaColore(new Color(0.4f, 0.7f, 1f), 0.25f));
    }

    IEnumerator LampeggiaColore(Color colore, float durata)
    {
        ImpostaColore(colore);
        yield return new WaitForSeconds(durata);
        if (!staAttaccando) RipristinaColori();
    }

    IEnumerator Lampeggia(bool critico)
    {
        ImpostaColore(critico ? new Color(1f, 0.85f, 0.2f) : Color.white);
        yield return new WaitForSeconds(critico ? 0.2f : 0.1f);
        if (!staAttaccando) RipristinaColori();
    }

    void Muori()
    {
        morto = true;
        Suoni.Suona(Suono.MorteNemico, transform.position);
        if (giocatore != null) giocatore.NemicoSconfitto(); // con il bastone ridà un po' di mana
        StopAllCoroutines();
        staAttaccando = false;
        MostraAspetto(false);
        if (corpo != null) corpo.enabled = false;
        if (rinasce) Invoke(nameof(Rinasci), secondiPerRinascere);
    }

    void Rinasci()
    {
        vita = vitaMassima;
        morto = false;
        RipristinaColori();
        MostraAspetto(true);
        if (corpo != null) corpo.enabled = true;
        prossimoAttacco = Time.time + intervalloAttacchi;
    }

    // Colora tutte le parti dello stesso colore (rosso del preavviso, bianco quando è colpito).
    void ImpostaColore(Color colore)
    {
        foreach (Renderer parte in aspetto) parte.material.color = colore;
    }

    // Rimette a ogni parte il suo colore di partenza.
    void RipristinaColori()
    {
        for (int i = 0; i < aspetto.Length; i++) aspetto[i].material.color = coloriBase[i];
    }

    void MostraAspetto(bool mostra)
    {
        foreach (Renderer parte in aspetto) parte.enabled = mostra;
    }
}
