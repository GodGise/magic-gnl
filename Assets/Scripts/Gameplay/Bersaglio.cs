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
// Più giocatori (co-op): attacca il giocatore più vicino (vedi ObiettiviNemici), oppure quello scelto da InseguimentoNemico.
// In rete il nemico "pensa" solo sul PC di chi ospita: lì si decidono attacchi, vita e morte, e MondoRete li manda
// agli altri. Sugli altri PC è una figura che segue le posizioni ricevute; i colpi dei loro giocatori vanno all'host.
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

    // Numero uguale su tutti i PC (vedi RegistroNemici), per i messaggi di rete.
    public int NumeroRete { get; private set; }
    // In co-op: chi ha dato l'ultimo colpo (per dargli il mana o la vita dell'uccisione).
    ulong ultimoColpitore;
    // Ospite in co-op: posizione e direzione ricevute dall'host, raggiunte in modo morbido.
    Vector3 posizioneRete;
    float direzioneRete;
    bool haPosizioneRete;
    // Il giocatore preso di mira (da InseguimentoNemico); se nessuno lo sceglie, il più vicino.
    public IObiettivoNemico Obiettivo { get; set; }
    public float DannoAttacco => dannoAttacco;

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
    float dannoDaSbilanciato = 1f; // e intanto i colpi che riceve fanno questo multiplo del danno

    public bool Sbilanciato => Time.time < sbilanciatoFino;
    // Tutte le parti visibili del nemico, ognuna con il suo colore di partenza.
    Renderer[] aspetto;
    Color[] coloriBase;
    Collider corpo;
    IObiettivoNemico giocatore;   // quello che sta attaccando adesso
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
        NumeroRete = RegistroNemici.Iscrivi(this);
    }

    void OnDestroy() => RegistroNemici.Togli(this, NumeroRete);

    void Start()
    {
        prossimoAttacco = Time.time + intervalloAttacchi;
    }

    void Update()
    {
        // Ospite in co-op: niente decisioni, solo la figura che segue l'host.
        if (Rete.Ospite)
        {
            SeguiPosizioneRete();
            return;
        }

        if (morto || staAttaccando || !attaccaIlGiocatore) return;
        giocatore = ObiettiviNemici.Valido(Obiettivo) ? Obiettivo : ObiettiviNemici.PiuVicino(transform.position);
        if (giocatore == null) return;
        if (Time.time < prossimoAttacco || Time.time < sbilanciatoFino) return;

        // Attacca solo se il giocatore è abbastanza vicino da vedere il preavviso.
        if (Vector3.Distance(transform.position, giocatore.Corpo.position) <= portataAttacco * 2f)
            StartCoroutine(Attacca());
        else
            prossimoAttacco = Time.time + 0.5f;
    }

    IEnumerator Attacca()
    {
        staAttaccando = true;
        inizioAttacco = Time.time;
        NumeroAttacco++;
        var preso = giocatore;
        MondoRete.InviaAttacco(this);

        Vector3 verso = preso.Corpo.position - transform.position;
        verso.y = 0f;
        if (verso.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(verso);

        ImpostaColore(Color.red);
        yield return new WaitForSeconds(preavviso);

        if (!morto)
        {
            RipristinaColori();
            Suoni.Suona(Suono.Fendente, transform.position + Vector3.up, 0.8f, 0.75f);
            // Il danno lo calcola chi è colpito (vedi GiocatoreControllo.ColpitoDaNemico), con la sua armatura.
            if (ObiettiviNemici.Esiste(preso) && !preso.Invisibile && Vector3.Distance(transform.position, preso.Corpo.position) <= portataAttacco)
                preso.ColpitoDaNemico(this);
        }

        prossimoAttacco = Time.time + intervalloAttacchi;
        staAttaccando = false;
    }

    // Chiamato dal giocatore quando un suo colpo va a segno.
    // "danno" è già calcolato (armatura e critico compresi, vedi CalcoloDanno).
    // In co-op, per chi non ospita, il colpo viene mandato all'host che lo applica e lo rimanda a tutti.
    public void RiceviColpo(float danno, Vector3 origineColpo, bool critico = false)
    {
        if (morto) return;
        if (Rete.Ospite)
        {
            MondoRete.ChiediColpo(this, danno, origineColpo, critico);
            return;
        }
        RiceviColpoDa(Rete.MioId, danno, origineColpo, critico);
    }

    // Il colpo vero (da soli, o sul PC dell'host). "chi" è il numero di rete del giocatore che ha colpito.
    public void RiceviColpoDa(ulong chi, float danno, Vector3 origineColpo, bool critico)
    {
        if (morto) return;
        ultimoColpitore = chi;

        if (Sbilanciato) danno *= dannoDaSbilanciato;
        vita -= danno;
        Debug.Log(name + (critico ? " colpito con un CRITICO: -" : " colpito: -") + danno.ToString("0.#") + ", vita " + Mathf.Max(0f, vita).ToString("0.#"));
        Colpito?.Invoke();

        Vector3 spinta = transform.position - origineColpo;
        spinta.y = 0f;
        if (spinta.sqrMagnitude > 0.0001f) transform.position += spinta.normalized * spintaQuandoColpito * (critico ? 2f : 1f);

        MondoRete.InviaColpito(this, critico);
        if (vita <= 0f)
        {
            Muori();
            return;
        }
        if (!staAttaccando) StartCoroutine(Lampeggia(critico));
    }

    // Chiamato dal giocatore dopo una parata perfetta: il nemico indietreggia, lampeggia di azzurro
    // e non attacca per "durata" secondi; intanto i colpi che riceve fanno "moltiplicatoreDanno" volte il danno.
    public void Sbilancia(float durata, Vector3 daDove, float moltiplicatoreDanno = 1f)
    {
        if (morto) return;
        if (Rete.Ospite)
        {
            MondoRete.ChiediSbilancia(this, durata, daDove, moltiplicatoreDanno);
            return;
        }
        sbilanciatoFino = Time.time + durata;
        dannoDaSbilanciato = moltiplicatoreDanno;
        Vector3 spinta = transform.position - daDove;
        spinta.y = 0f;
        if (spinta.sqrMagnitude > 0.0001f) transform.position += spinta.normalized * spintaQuandoColpito * 2f;
        StartCoroutine(LampeggiaColore(new Color(0.4f, 0.7f, 1f), 0.25f));
        MondoRete.InviaSbilanciato(this, durata, moltiplicatoreDanno);
    }

    // ---------- co-op: cosa fa la figura del nemico sui PC di chi non ospita (chiamati da MondoRete) ----------

    public float Vita => vita;
    public float DirezioneAttuale => transform.eulerAngles.y;

    public void ImpostaPosizioneRete(Vector3 posizione, float direzione)
    {
        posizioneRete = posizione;
        direzioneRete = direzione;
        if (!haPosizioneRete || (transform.position - posizione).sqrMagnitude > 25f)
            transform.SetPositionAndRotation(posizione, Quaternion.Euler(0f, direzione, 0f));
        haPosizioneRete = true;
    }

    void SeguiPosizioneRete()
    {
        if (!haPosizioneRete) return;
        float t = 1f - Mathf.Exp(-12f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, posizioneRete, t);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, direzioneRete, 0f), t);
    }

    public void AttaccoDaRete(int numeroAttacco)
    {
        if (morto) return;
        NumeroAttacco = numeroAttacco;
        StopCoroutine(nameof(AttaccoSoloAspetto));
        StartCoroutine(nameof(AttaccoSoloAspetto));
    }

    // Come Attacca, ma senza danno: il danno lo decide l'host.
    IEnumerator AttaccoSoloAspetto()
    {
        staAttaccando = true;
        inizioAttacco = Time.time;
        ImpostaColore(Color.red);
        yield return new WaitForSeconds(preavviso);
        if (!morto)
        {
            RipristinaColori();
            Suoni.Suona(Suono.Fendente, transform.position + Vector3.up, 0.8f, 0.75f);
        }
        staAttaccando = false;
    }

    public void ColpitoDaRete(float vitaRimasta, bool critico)
    {
        if (morto) return;
        vita = vitaRimasta;
        Colpito?.Invoke();
        if (!staAttaccando) StartCoroutine(Lampeggia(critico));
    }

    public void SbilanciatoDaRete(float durata, float moltiplicatoreDanno)
    {
        if (morto) return;
        sbilanciatoFino = Time.time + durata;
        dannoDaSbilanciato = moltiplicatoreDanno;
        StartCoroutine(LampeggiaColore(new Color(0.4f, 0.7f, 1f), 0.25f));
    }

    public void MortoDaRete()
    {
        if (morto) return;
        vita = 0f;
        MostraMorte();
    }

    public void RinatoDaRete()
    {
        if (!morto) return;
        Rinasci();
    }

    // Chi entra a partita iniziata riceve vita e morte di tutti i nemici.
    public void StatoDaRete(float vitaAttuale, bool eMorto)
    {
        vita = vitaAttuale;
        if (eMorto && !morto) MostraMorte();
        else if (!eMorto && morto) Rinasci();
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
        MostraMorte();
        // Il premio dell'uccisione (con il bastone ridà un po' di mana) va a chi ha dato l'ultimo colpo.
        if (!Rete.Attiva || ultimoColpitore == Rete.MioId)
        {
            if (ObiettiviNemici.Locale != null) ObiettiviNemici.Locale.NemicoSconfitto();
        }
        else MondoRete.InviaSconfitto(ultimoColpitore);
        MondoRete.InviaMorto(this);
        if (rinasce) Invoke(nameof(Rinasci), secondiPerRinascere);
    }

    void MostraMorte()
    {
        morto = true;
        Suoni.Suona(Suono.MorteNemico, transform.position);
        StopAllCoroutines();
        staAttaccando = false;
        MostraAspetto(false);
        if (corpo != null) corpo.enabled = false;
    }

    void Rinasci()
    {
        if (!Rete.Ospite) MondoRete.InviaRinato(this);
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
