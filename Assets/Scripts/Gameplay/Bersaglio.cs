using System.Collections;
using UnityEngine;

// Nemico di prova per allenare le tre mosse.
// A cosa serve: incassa i colpi del giocatore (lampeggia di bianco e indietreggia) e, se
// "Attacca Il Giocatore" è attivo, ogni pochi secondi si gira verso di lui, diventa rosso
// (preavviso) e poi colpisce. Il momento giusto per schivare o parare è la fine del rosso.
// Come montarlo: su qualunque oggetto con un Collider (per esempio un cilindro).
// Il menu "magic-gnl > Crea scena di prova" ne mette uno già pronto.
public class Bersaglio : MonoBehaviour
{
    [SerializeField] float vitaMassima = 100f;
    [SerializeField] float secondiPerRinascere = 2f;
    [SerializeField] float spintaQuandoColpito = 0.3f;

    [Header("Attacco di prova")]
    public bool attaccaIlGiocatore = true;
    [SerializeField] float intervalloAttacchi = 3f;
    [Tooltip("Secondi in cui resta rosso prima di colpire.")]
    [SerializeField] float preavviso = 0.7f;
    [SerializeField] float portataAttacco = 2.5f;
    [SerializeField] float dannoAttacco = 20f;

    float vita;
    bool morto;
    bool staAttaccando;
    float prossimoAttacco;
    Renderer aspetto;
    Collider corpo;
    Color coloreBase;
    GiocatoreControllo giocatore;

    void Awake()
    {
        aspetto = GetComponentInChildren<Renderer>();
        corpo = GetComponent<Collider>();
        if (aspetto != null) coloreBase = aspetto.material.color;
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
        if (Time.time < prossimoAttacco) return;

        // Attacca solo se il giocatore è abbastanza vicino da vedere il preavviso.
        if (Vector3.Distance(transform.position, giocatore.transform.position) <= portataAttacco * 2f)
            StartCoroutine(Attacca());
        else
            prossimoAttacco = Time.time + 0.5f;
    }

    IEnumerator Attacca()
    {
        staAttaccando = true;

        Vector3 verso = giocatore.transform.position - transform.position;
        verso.y = 0f;
        if (verso.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(verso);

        ImpostaColore(Color.red);
        yield return new WaitForSeconds(preavviso);

        if (!morto)
        {
            ImpostaColore(coloreBase);
            if (Vector3.Distance(transform.position, giocatore.transform.position) <= portataAttacco)
                giocatore.RiceviColpo(dannoAttacco, transform.position);
        }

        prossimoAttacco = Time.time + intervalloAttacchi;
        staAttaccando = false;
    }

    // Chiamato dal giocatore quando un suo colpo va a segno.
    public void RiceviColpo(float danno, Vector3 origineColpo)
    {
        if (morto) return;

        vita -= danno;
        Debug.Log(name + " colpito: vita " + Mathf.Max(0f, vita));

        Vector3 spinta = transform.position - origineColpo;
        spinta.y = 0f;
        if (spinta.sqrMagnitude > 0.0001f) transform.position += spinta.normalized * spintaQuandoColpito;

        if (vita <= 0f)
        {
            Muori();
            return;
        }
        if (!staAttaccando) StartCoroutine(Lampeggia());
    }

    IEnumerator Lampeggia()
    {
        ImpostaColore(Color.white);
        yield return new WaitForSeconds(0.1f);
        if (!staAttaccando) ImpostaColore(coloreBase);
    }

    void Muori()
    {
        morto = true;
        StopAllCoroutines();
        staAttaccando = false;
        if (aspetto != null) aspetto.enabled = false;
        if (corpo != null) corpo.enabled = false;
        Invoke(nameof(Rinasci), secondiPerRinascere);
    }

    void Rinasci()
    {
        vita = vitaMassima;
        morto = false;
        ImpostaColore(coloreBase);
        if (aspetto != null) aspetto.enabled = true;
        if (corpo != null) corpo.enabled = true;
        prossimoAttacco = Time.time + intervalloAttacchi;
    }

    void ImpostaColore(Color colore)
    {
        if (aspetto != null) aspetto.material.color = colore;
    }
}
