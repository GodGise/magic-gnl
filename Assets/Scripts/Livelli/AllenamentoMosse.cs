using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// L'allenamento del tutorial: un elenco di mosse da provare almeno una volta (attaccare, parare, schivare,
// agganciare, tirare con l'arco, lanciare e cambiare incantesimo). Ogni mossa provata si spunta; quando sono
// spuntate tutte si attiva "Al Completamento" (per esempio Brannoc che dice "Non male").
// L'elenco si vede a sinistra dello schermo, sotto le barre, con il tasto di ogni mossa (quello scelto dal giocatore).
// Ogni classe ha la sua lista di mosse ("Mosse Guerriero", "Mosse Ladro", "Mosse Stregone").
// Come montarlo: su un oggetto vuoto della scena. Parte spento: lo si avvia da un evento con
// AllenamentoMosse > Avvia (per esempio alla fine del dialogo con Brannoc). Finisce una volta sola.
public class AllenamentoMosse : MonoBehaviour
{
    public enum Mossa { Attacca, Para, Schiva, Aggancia, TiraConLArco, LanciaIncantesimo, CambiaIncantesimo }

    [SerializeField] Mossa[] mosseGuerriero = { Mossa.Attacca, Mossa.Para, Mossa.Schiva, Mossa.Aggancia };
    [SerializeField] Mossa[] mosseLadro = { Mossa.Attacca, Mossa.Para, Mossa.Schiva, Mossa.Aggancia, Mossa.TiraConLArco };
    [SerializeField] Mossa[] mosseStregone = { Mossa.LanciaIncantesimo, Mossa.CambiaIncantesimo, Mossa.Schiva, Mossa.Aggancia };
    [Tooltip("Secondi di attesa dopo l'ultima mossa, prima di \"Al Completamento\" (per vedere la spunta).")]
    [SerializeField, Min(0f)] float attesaFinale = 1.2f;
    [SerializeField] UnityEvent alCompletamento;

    static readonly Dictionary<Mossa, string> Chiavi = new Dictionary<Mossa, string>
    {
        { Mossa.Attacca, "all.attacca" }, { Mossa.Para, "all.para" }, { Mossa.Schiva, "all.schiva" },
        { Mossa.Aggancia, "all.aggancia" }, { Mossa.TiraConLArco, "all.arco" },
        { Mossa.LanciaIncantesimo, "all.lancia" }, { Mossa.CambiaIncantesimo, "all.cambia" },
    };

    GiocatoreControllo giocatore;
    AggancioBersaglio aggancio;
    TiroConLArco arco;
    Mossa[] mosse;
    readonly HashSet<Mossa> fatte = new HashSet<Mossa>();
    bool attivo, finito;
    float finitoAlle = -1f;
    int tiriIniziali, sceltaIniziale = -1;

    public bool Attivo => attivo;

    public void Avvia()
    {
        if (attivo || finito) return;
        attivo = true;
        mosse = SceltaPartita.Classe == ClasseGiocatore.Guerriero ? mosseGuerriero
            : SceltaPartita.Classe == ClasseGiocatore.Ladro ? mosseLadro : mosseStregone;
        giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore != null)
        {
            aggancio = giocatore.GetComponent<AggancioBersaglio>();
            arco = giocatore.GetComponent<TiroConLArco>();
            tiriIniziali = arco != null ? arco.Tiri : 0;
            sceltaIniziale = giocatore.Magia != null ? giocatore.Magia.Scelta : -1;
        }
        if (mosse == null || mosse.Length == 0) Completa();
    }

    void Update()
    {
        if (finito)
        {
            if (finitoAlle >= 0f && Time.time >= finitoAlle)
            {
                finitoAlle = -1f;
                attivo = false;
                alCompletamento.Invoke();
            }
            return;
        }
        if (!attivo || giocatore == null) return;

        var stato = giocatore.StatoAttuale;
        foreach (var m in mosse)
        {
            if (fatte.Contains(m)) continue;
            bool ok = false;
            switch (m)
            {
                case Mossa.Attacca: ok = stato == GiocatoreControllo.Stato.Attacco && !giocatore.Stregone; break;
                case Mossa.LanciaIncantesimo: ok = stato == GiocatoreControllo.Stato.Attacco && giocatore.Stregone; break;
                case Mossa.Para: ok = stato == GiocatoreControllo.Stato.Parata; break;
                case Mossa.Schiva: ok = stato == GiocatoreControllo.Stato.Schivata; break;
                case Mossa.Aggancia:
                    if (aggancio == null) aggancio = giocatore.GetComponent<AggancioBersaglio>();
                    ok = aggancio != null && aggancio.Agganciato;
                    break;
                case Mossa.TiraConLArco:
                    if (arco == null) { arco = giocatore.GetComponent<TiroConLArco>(); if (arco != null) tiriIniziali = arco.Tiri; }
                    ok = arco != null && arco.Tiri > tiriIniziali;
                    break;
                case Mossa.CambiaIncantesimo:
                    var magia = giocatore.Magia;
                    if (magia == null) break;
                    if (sceltaIniziale < 0) sceltaIniziale = magia.Scelta;
                    ok = magia.Scelta != sceltaIniziale;
                    break;
            }
            if (ok) fatte.Add(m);
        }
        if (fatte.Count >= mosse.Length) Completa();
    }

    void Completa()
    {
        finito = true;
        finitoAlle = Time.time + attesaFinale;
    }

    void OnGUI()
    {
        if (!attivo || mosse == null || !HudGioco.Attivo || MenuPausa.InPausa || InventarioGioco.Aperto || Dialogo.InCorso) return;
        GUI.depth = 10;
        GraficaMenu.PreparaStili();
        GraficaMenu.FoglioIntero();
        float x = 60f, y = 250f, riga = 40f;
        float alto = 52f + riga * mosse.Length;
        var riquadro = new Rect(x, y, 520f, alto);
        GraficaMenu.Cornice(riquadro, 0.85f, false);
        var titolo = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft };
        GraficaMenu.Scritta(new Rect(x + 24f, y + 8f, 470f, 32f), Lingua.T("all.titolo"), titolo, GraficaMenu.Selezione, 1f);
        var stile = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft, wordWrap = false };
        for (int i = 0; i < mosse.Length; i++)
        {
            bool fatta = fatte.Contains(mosse[i]);
            float ry = y + 46f + i * riga;
            var centro = new Vector2(x + 34f, ry + riga * 0.5f);
            GraficaMenu.Rombo(centro, 18f, GraficaMenu.Bronzo);
            GraficaMenu.Rombo(centro, 12f, fatta ? GraficaMenu.Selezione : GraficaMenu.Riquadro);
            GraficaMenu.Scritta(new Rect(x + 58f, ry, 450f, riga), Dialogo.Testo(Chiavi[mosse[i]]), stile,
                fatta ? GraficaMenu.Spento : GraficaMenu.Testo, 1f);
        }
    }
}
