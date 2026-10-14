using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Le missioni del prologo: l'obiettivo a schermo ("Porta la legna dal capanno a casa") e i suggerimenti del tutorial.
// A cosa serve: è il "registro" delle quest. Gli altri pezzi della scena (Interlocutore, PuntoInterazione,
// ZonaArrivo, DialogoAutomatico) lo chiamano dai loro eventi per cambiare l'obiettivo, mostrare un suggerimento o
// dire che una quest è finita. Quando una quest finisce lo dice anche a OrarioPrologo (il sole avanza).
// L'obiettivo si vede in alto a destra, sotto l'ora; i suggerimenti in alto al centro, uno dopo l'altro.
// Dove andare lo indica la Bussola in alto (la meta si sceglie con il componente MetaObiettivo).
// Come montarlo: su un oggetto vuoto della scena (per esempio "Missioni"); lo crea da solo il menu
// "magic-gnl > Prologo: prepara la quest 1". Poi, negli eventi degli altri componenti, si trascina questo oggetto e
// si sceglie Missioni > ImpostaObiettivo (con la chiave di Lingua, per esempio ob.q1.legna), Suggerisci (per esempio
// tip.q1.muovi), TogliObiettivo oppure QuestFinita (con il numero della quest).
// Tutti i testi sono chiavi di Lingua: nei suggerimenti {Interagisci}, {Attacca}... diventano i tasti scelti.
public class Missioni : MonoBehaviour
{
    [Tooltip("Obiettivo mostrato all'avvio (chiave di Lingua). Vuoto = nessuno.")]
    [SerializeField] string obiettivoIniziale;
    [Tooltip("Per quanti secondi resta a schermo ogni suggerimento.")]
    [SerializeField, Min(1f)] float secondiSuggerimento = 6f;

    public static Missioni Istanza { get; private set; }
    public string Obiettivo => obiettivo;
    // Dove si trova l'obiettivo (lo indica la Bussola). Si imposta con MetaObiettivo > Attiva.
    public Transform Meta => meta;

    string obiettivo;
    int numeroObiettivo = -1;
    Transform meta;
    float obiettivoDa;
    readonly Queue<string> suggerimenti = new Queue<string>();
    Coroutine giroSuggerimenti;

    void Awake()
    {
        Istanza = this;
        obiettivo = obiettivoIniziale;
        obiettivoDa = Time.time;
    }

    void OnDestroy()
    {
        if (Istanza == this) Istanza = null;
    }

    public void ImpostaObiettivo(string chiave)
    {
        numeroObiettivo = -1;
        obiettivo = chiave;
        obiettivoDa = Time.time;
    }

    // Obiettivo con un numero dentro (la chiave di Lingua ha {0}), per esempio "Ratti rimasti: 2". Non riparte
    // l'animazione di "obiettivo nuovo" se cambia solo il numero.
    public void ImpostaObiettivoConNumero(string chiave, int numero)
    {
        if (obiettivo != chiave) obiettivoDa = Time.time;
        obiettivo = chiave;
        numeroObiettivo = numero;
    }

    // Toglie l'obiettivo e anche la meta della bussola.
    public void TogliObiettivo()
    {
        obiettivo = null;
        meta = null;
    }

    public void ImpostaMeta(Transform punto) => meta = punto;
    public void TogliMeta() => meta = null;

    // I suggerimenti si mettono in fila: se ne arrivano due insieme si vedono uno dopo l'altro, mai durante un dialogo.
    public void Suggerisci(string chiave)
    {
        if (string.IsNullOrEmpty(chiave)) return;
        suggerimenti.Enqueue(chiave);
        if (giroSuggerimenti == null) giroSuggerimenti = StartCoroutine(MostraSuggerimenti());
    }

    IEnumerator MostraSuggerimenti()
    {
        while (suggerimenti.Count > 0)
        {
            while (Dialogo.InCorso) yield return null;
            MessaggiSchermo.Mostra(Dialogo.Testo(suggerimenti.Dequeue()), secondiSuggerimento);
            yield return new WaitForSeconds(secondiSuggerimento + 0.3f);
        }
        giroSuggerimenti = null;
    }

    public void QuestFinita(int numero)
    {
        if (OrarioPrologo.Istanza != null) OrarioPrologo.Istanza.QuestFinita(numero);
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(obiettivo) || !HudGioco.Attivo || MenuPausa.InPausa || InventarioGioco.Aperto || Dialogo.InCorso) return;
        GUI.depth = 10;
        GraficaMenu.PreparaStili();
        float larghezza = GraficaMenu.FoglioIntero();
        float nuovo = Mathf.Clamp01((Time.time - obiettivoDa) / 0.6f);   // un obiettivo nuovo appare piano e si accende
        string testo = Dialogo.Testo(obiettivo);
        if (numeroObiettivo >= 0) testo = testo.Replace("{0}", numeroObiettivo.ToString());
        var stile = new GUIStyle(GraficaMenu.EtichettaGioco) { alignment = TextAnchor.MiddleLeft, wordWrap = false };
        float w = Mathf.Clamp(stile.CalcSize(new GUIContent(testo)).x + 70f, 260f, 760f);
        var r = new Rect(larghezza - 80f - w, 132f, w, 54f);
        GraficaMenu.Cornice(r, 0.8f * nuovo, nuovo < 1f);
        GraficaMenu.Rombo(new Vector2(r.x + 24f, r.center.y), 12f, GraficaMenu.Con(GraficaMenu.Selezione, nuovo));
        GraficaMenu.Scritta(new Rect(r.x + 44f, r.y, w - 54f, r.height), testo, stile, GraficaMenu.Testo, nuovo);
    }
}
