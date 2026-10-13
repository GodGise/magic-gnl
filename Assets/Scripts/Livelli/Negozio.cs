using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Un negozio di baratto: la bottega di Ilse e, in futuro, fabbro, oste e altri mercanti (decisione di Lorenzo, 13 ottobre).
// A cosa serve: vicino al negoziante compare "E  Commercia: Ilse"; premendo il tasto si apre la finestra della bottega.
//   - a sinistra l'elenco delle ricette: cosa ricevi (per esempio "Pozione di Ilse x2") e cosa serve
//     ("Fiore di riva 3/3"); quelle che puoi fare sono accese, le altre spente;
//   - a destra il dettaglio della ricetta scelta: descrizione, effetti, ingredienti (verde = li hai, rosso = mancano)
//     e il pulsante "Scambia".
// Ogni ricetta (DatiRicetta) toglie gli ingredienti dallo zaino e mette il risultato. Per ora si baratta solo
// (fiori per pozioni); le monete, quando ci saranno, saranno un oggetto come gli altri (vedi DatiRicetta).
// Comandi: frecce, WASD o levetta per scegliere; E, Invio o A per scambiare; Esc, Tab o B per chiudere.
// Mouse: clic per scegliere, doppio clic o clic su "Scambia" per scambiare.
// Il gioco non si ferma (in co-op non potrebbe), ma comandi del personaggio e camera si spengono, come nell'inventario.
// Mentre la bottega è aperta InventarioGioco.Aperto è vero, così gli altri tasti (E, F, Tab...) non fanno niente.
// Come montarlo:
//   1. Sul negoziante (o su un oggetto vuoto accanto, nel punto da cui si parla).
//   2. "Nome Chiave" (pg.ilse) e "Titolo Chiave" (neg.bottega_ilse): chiavi di Lingua.
//   3. "Ricette": le DatiRicetta in vendita (menu "magic-gnl > Erbe: crea fiori, pozioni e ricette" crea quelle di Ilse).
//   4. "Al Primo Scambio": cosa succede la prima volta che si scambia (la quest 4 passa a "Bevi una pozione").
// Se il componente è spento il negoziante non commercia (si accende da un evento, per esempio a metà di una quest);
// da un evento si può anche aprire subito la bottega con Negozio > Apri.
public class Negozio : MonoBehaviour
{
    // La bottega aperta adesso (null = nessuna).
    public static Negozio Corrente { get; private set; }
    public static bool Aperto => Corrente != null;

    [Tooltip("Chiave del nome del negoziante in Lingua (per esempio pg.ilse).")]
    [SerializeField] string nomeChiave;
    [Tooltip("Chiave del titolo della finestra in Lingua (per esempio neg.bottega_ilse).")]
    [SerializeField] string titoloChiave;
    [SerializeField] DatiRicetta[] ricette;
    [Tooltip("Distanza (in orizzontale) entro cui si può commerciare, in metri.")]
    [SerializeField] float raggio = 3f;
    [Tooltip("Cosa succede la prima volta che si fa uno scambio.")]
    [SerializeField] UnityEvent alPrimoScambio;

    const float AltezzaRiga = 118f;
    const int RigheVisibili = 6;
    static readonly Color Verde = new Color(0.55f, 0.82f, 0.45f);
    static readonly Color Rosso = new Color(0.9f, 0.38f, 0.32f);

    readonly ElencoMenu comandi = new ElencoMenu();   // usato solo per leggere frecce, conferma e indietro
    readonly List<Behaviour> spenti = new List<Behaviour>();
    GiocatoreControllo giocatore;
    Zaino zaino;
    InputAction comandoApri;
    int scelta, rigaIniziale;
    float tempoApertura;
    bool scambiatoUnaVolta, riattivaAlProssimoFotogramma;
    CursorLockMode cursorePrima;
    bool cursoreVisibilePrima;
    float ultimoClic;
    int ultimoIndiceClic = -1;
    string messaggio;
    Color coloreMessaggio;
    float messaggioFino;

    void Awake()
    {
        comandoApri = new InputAction("Commercia", InputActionType.Button);
        comandoApri.AddBinding("<Keyboard>/e");
        comandoApri.AddBinding("<Gamepad>/buttonSouth");
        Comandi.Collega(comandoApri, Azione.Interagisci, this);   // tasto scelto in Opzioni > Comandi
    }

    void OnEnable() => comandoApri.Enable();
    void OnDisable()
    {
        comandoApri.Disable();
        if (Corrente == this) { Chiudi(); riattivaAlProssimoFotogramma = false; Riattiva(); }
    }
    void OnDestroy() => comandoApri.Dispose();

    void Start() => giocatore = FindFirstObjectByType<GiocatoreControllo>();

    bool Vicino()
    {
        if (giocatore == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) return false;
        Vector3 distanza = giocatore.transform.position - transform.position;
        distanza.y = 0f;
        return distanza.sqrMagnitude <= raggio * raggio;
    }

    // ---------- apri e chiudi ----------

    // Apre la bottega (dal tasto, oppure da un evento: per esempio alla fine del dialogo con Ilse).
    public void Apri()
    {
        if (Aperto || !isActiveAndEnabled) return;
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) return;
        zaino = Zaino.Di(giocatore);
        cursorePrima = Cursor.lockState;
        cursoreVisibilePrima = Cursor.visible;
        spenti.Clear();
        foreach (var b in FindObjectsByType<GiocatoreControllo>(FindObjectsSortMode.None)) Spegni(b);
        foreach (var b in FindObjectsByType<CameraTerzaPersona>(FindObjectsSortMode.None)) Spegni(b);
        foreach (var b in FindObjectsByType<AggancioBersaglio>(FindObjectsSortMode.None)) Spegni(b);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // la prima ricetta che si può fare è quella scelta all'apertura
        scelta = 0;
        for (int i = 0; i < Numero; i++) if (ricette[i] != null && ricette[i].Possibile(zaino)) { scelta = i; break; }
        rigaIniziale = 0;
        tempoApertura = Time.unscaledTime;
        Corrente = this;
        Suoni.Suona(Suono.CambioArma, giocatore.transform.position + Vector3.up, 0.4f, 0.9f);
    }

    void Spegni(Behaviour b)
    {
        if (b == null || !b.enabled) return;
        b.enabled = false;
        spenti.Add(b);
    }

    void Chiudi()
    {
        if (Corrente != this) return;
        Corrente = null;
        InventarioGioco.FotogrammaChiusura = Time.frameCount;   // lo stesso Esc non deve aprire il menu di pausa
        riattivaAlProssimoFotogramma = true;                    // così lo stesso tasto non arriva anche al personaggio
    }

    void Riattiva()
    {
        foreach (var b in spenti)
            if (b != null) b.enabled = true;
        spenti.Clear();
        Cursor.lockState = cursorePrima;
        Cursor.visible = cursoreVisibilePrima;
    }

    // ---------- scambiare ----------

    int Numero => ricette != null ? ricette.Length : 0;
    DatiRicetta Scelta => scelta >= 0 && scelta < Numero ? ricette[scelta] : null;

    void Scambia()
    {
        var r = Scelta;
        if (r == null || !r.Valida) return;
        Vector3 suono = giocatore.transform.position + Vector3.up;
        if (!r.Esegui(zaino))
        {
            Avvisa(Lingua.T("neg.mancano"), Rosso);
            Suoni.Suona(Suono.Negato, suono, 0.6f);
            return;
        }
        if (r.da is DatiConsumabile c) UsoConsumabili.Seleziona(c);   // la pozione appena fatta è pronta per il tasto F
        Avvisa(Lingua.T("hud.raccolto") + ": " + NomeConQuantita(r), Verde);
        Suoni.Suona(Suono.Raccolta, suono, 0.8f);
        if (scambiatoUnaVolta) return;
        scambiatoUnaVolta = true;
        alPrimoScambio?.Invoke();
    }

    void Avvisa(string testo, Color colore)
    {
        messaggio = testo;
        coloreMessaggio = colore;
        messaggioFino = Time.unscaledTime + 2.5f;
    }

    static string NomeConQuantita(DatiRicetta r) => r.da.Nome + (r.quantiDa > 1 ? " x" + r.quantiDa : "");

    // ---------- comandi ----------

    void Update()
    {
        if (riattivaAlProssimoFotogramma)
        {
            riattivaAlProssimoFotogramma = false;
            Riattiva();
        }
        if (giocatore == null) giocatore = FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null || MenuPausa.InPausa) return;

        if (Corrente != this)
        {
            if (Aperto || InventarioGioco.Aperto || Dialogo.InCorso || Dialogo.ApenaFinito || !Vicino()) return;
            string nome = string.IsNullOrEmpty(nomeChiave) ? "" : ": " + Lingua.T(nomeChiave);
            HudGioco.MostraAzione("E", Lingua.T("neg.commercia") + nome);
            if (comandoApri.WasPressedThisFrame() && giocatore.StatoAttuale == GiocatoreControllo.Stato.Libero) Apri();
            return;
        }

        if (giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto || !Vicino()) { Chiudi(); return; }
        // appena aperta non legge i tasti: lo stesso E che ha chiuso il dialogo non deve fare subito uno scambio
        if (Time.unscaledTime - tempoApertura < 0.25f) return;

        var tastiera = Keyboard.current;
        var pad = Gamepad.current;
        var c = comandi.LeggiComandi();
        bool chiudi = c.indietro || Comandi.PremutoOra(Azione.Inventario) || (pad != null && pad.selectButton.wasPressedThisFrame);
        if (chiudi) { Chiudi(); return; }
        if (c.verticale != 0 && Numero > 0) scelta = Mathf.Clamp(scelta + c.verticale, 0, Numero - 1);
        if (c.conferma || (tastiera != null && tastiera.eKey.wasPressedThisFrame)) Scambia();
    }

    // ---------- disegno ----------

    void OnGUI()
    {
        if (Corrente != this || giocatore == null) return;
        GUI.depth = -50;
        GraficaMenu.PreparaStili();
        float alfa = Mathf.Clamp01((Time.unscaledTime - tempoApertura) / 0.2f);
        GraficaMenu.Atmosfera(0.6f);
        GraficaMenu.FoglioVirtuale();
        var e = Event.current;
        float L = GraficaMenu.Larghezza;

        string titolo = Lingua.T(string.IsNullOrEmpty(titoloChiave) ? "neg.bottega" : titoloChiave).ToUpperInvariant();
        if (Lingua.Indice < 6) titolo = GraficaMenu.Spaziato(titolo);
        GraficaMenu.Scritta(new Rect(0, 50, L, 80), titolo, GraficaMenu.Intestazione, GraficaMenu.Testo, alfa);
        GraficaMenu.Divisore(L * 0.5f, 148f, 640f, alfa);

        if (DisegnaRicette(alfa, e)) return;
        if (DisegnaDettaglio(alfa, e)) return;

        if (Time.unscaledTime < messaggioFino && !string.IsNullOrEmpty(messaggio))
            GraficaMenu.Scritta(new Rect(0, 1036, L, 34), messaggio, GraficaMenu.Piccolo, coloreMessaggio, Mathf.Clamp01(messaggioFino - Time.unscaledTime));
        else
            GraficaMenu.Scritta(new Rect(0, 1040, L, 30), Lingua.T("neg.aiuto"), GraficaMenu.Piccolo, GraficaMenu.Spento, 0.8f * alfa);
    }

    // L'elenco delle ricette a sinistra. Restituisce true se un clic ha fatto uno scambio (il disegno si ferma lì).
    bool DisegnaRicette(float alfa, Event e)
    {
        var P = new Rect(120f, 200f, 820f, 800f);
        GraficaMenu.Cornice(P, alfa, false);
        var et = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft, fontSize = 24 };
        GraficaMenu.Scritta(new Rect(P.x + 40f, P.y + 16f, 400f, 34f), Lingua.T("neg.ricette").ToUpperInvariant(), et, GraficaMenu.Bronzo, alfa);
        if (Numero == 0)
        {
            GraficaMenu.Scritta(new Rect(P.x, P.y + 60f, P.width, 200f), Lingua.T("neg.niente"), GraficaMenu.Didascalia, GraficaMenu.Spento, alfa);
            return false;
        }

        // rotellina sopra l'elenco: scorre di una riga
        if (e.type == EventType.ScrollWheel && P.Contains(e.mousePosition) && Numero > RigheVisibili)
        {
            scelta = Mathf.Clamp(scelta + (e.delta.y > 0f ? 1 : -1), 0, Numero - 1);
            e.Use();
        }
        if (scelta < rigaIniziale) rigaIniziale = scelta;
        if (scelta >= rigaIniziale + RigheVisibili) rigaIniziale = scelta - RigheVisibili + 1;

        var stileNome = new GUIStyle(GraficaMenu.VoceSinistra) { fontSize = 30 };
        var stileIngredienti = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleLeft, wordWrap = false };
        for (int k = 0; k < RigheVisibili && rigaIniziale + k < Numero; k++)
        {
            int i = rigaIniziale + k;
            var r = ricette[i];
            var riga = new Rect(P.x + 24f, P.y + 64f + k * AltezzaRiga, P.width - 48f, AltezzaRiga - 10f);
            if (r == null || !r.Valida) continue;
            bool possibile = r.Possibile(zaino);
            bool selezionata = i == scelta;

            if (riga.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                e.Use();
                scelta = i;
                bool doppio = i == ultimoIndiceClic && Time.unscaledTime - ultimoClic < 0.35f;
                ultimoIndiceClic = i;
                ultimoClic = Time.unscaledTime;
                if (doppio) { ultimoIndiceClic = -1; Scambia(); return true; }
            }

            if (selezionata) GraficaMenu.FasciaLuce(riga, GraficaMenu.Con(GraficaMenu.Selezione, 0.18f * alfa));
            var casella = new Rect(riga.x + 10f, riga.y + 9f, 90f, 90f);
            GraficaMenu.Riempi(casella, new Color(0.055f, 0.055f, 0.07f, 0.92f * alfa));
            GraficaMenu.Bordo(casella, selezionata ? 3f : 1f, GraficaMenu.Con(selezionata ? GraficaMenu.Selezione : GraficaMenu.Bronzo, (selezionata ? 1f : 0.6f) * alfa));
            DisegnaOggetto(new Rect(casella.x + 15f, casella.y + 15f, 60f, 60f), r.da, possibile ? alfa : 0.45f * alfa);

            Color colore = selezionata ? GraficaMenu.Selezione : possibile ? GraficaMenu.Testo : GraficaMenu.Spento;
            GraficaMenu.Scritta(new Rect(casella.xMax + 24f, riga.y + 8f, riga.width - 140f, 44f), NomeConQuantita(r), stileNome, colore, alfa);
            GraficaMenu.Scritta(new Rect(casella.xMax + 24f, riga.y + 56f, riga.width - 140f, 36f), Ingredienti(r), stileIngredienti,
                possibile ? GraficaMenu.Testo : GraficaMenu.Spento, alfa);
        }
        if (Numero > RigheVisibili)
        {
            var pag = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleRight };
            GraficaMenu.Scritta(new Rect(P.xMax - 240f, P.y + 16f, 200f, 34f), (scelta + 1) + " / " + Numero, pag, GraficaMenu.Spento, alfa);
        }
        return false;
    }

    // "Fiore di riva 3/3  ·  Cardo nero 0/1"
    string Ingredienti(DatiRicetta r)
    {
        var parti = new List<string>();
        foreach (var i in r.chiede)
            if (i.oggetto != null) parti.Add(i.oggetto.Nome + " " + DatiRicetta.Hai(zaino, i.oggetto) + "/" + Mathf.Max(1, i.quanti));
        return string.Join(GraficaMenu.Separatore, parti);
    }

    // Il dettaglio della ricetta scelta, a destra, con il pulsante "Scambia".
    bool DisegnaDettaglio(float alfa, Event e)
    {
        var D = new Rect(1000f, 200f, 800f, 800f);
        GraficaMenu.Cornice(D, alfa, false);
        var r = Scelta;
        if (r == null || !r.Valida) return false;
        bool possibile = r.Possibile(zaino);
        float cx = D.center.x, y = D.y + 40f;

        var grande = new Rect(cx - 70f, y, 140f, 140f);
        GraficaMenu.Alone(new Rect(grande.x - 50f, grande.y - 50f, grande.width + 100f, grande.height + 100f), new Color(1f, 0.5f, 0.2f, 0.12f * alfa));
        GraficaMenu.Cornice(grande, alfa, possibile);
        DisegnaOggetto(new Rect(grande.x + 30f, grande.y + 30f, 80f, 80f), r.da, alfa);
        y = grande.yMax + 14f;
        GraficaMenu.Scritta(new Rect(D.x + 30f, y, D.width - 60f, 50f), NomeConQuantita(r), GraficaMenu.NomeGioco, GraficaMenu.Selezione, alfa);
        y += 54f;
        var descrizione = new GUIStyle(GraficaMenu.Didascalia) { wordWrap = true, alignment = TextAnchor.UpperCenter };
        GraficaMenu.Scritta(new Rect(D.x + 60f, y, D.width - 120f, 80f), r.da.Descrizione, descrizione, GraficaMenu.Testo, alfa);
        y += 86f;
        if (r.da is DatiConsumabile c)
        {
            string effetti = c.Effetti();
            if (effetti.Length > 0) GraficaMenu.Scritta(new Rect(D.x + 30f, y, D.width - 60f, 34f), effetti, GraficaMenu.Didascalia, Verde, alfa);
        }
        y += 50f;

        GraficaMenu.Divisore(cx, y, 520f, alfa);
        y += 22f;
        var et = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
        GraficaMenu.Scritta(new Rect(D.x, y, D.width, 30f), Lingua.T("neg.ti_serve").ToUpperInvariant(), et, GraficaMenu.Bronzo, alfa);
        y += 40f;
        var sinistra = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleLeft };
        var destra = new GUIStyle(GraficaMenu.Valore) { alignment = TextAnchor.MiddleRight };
        foreach (var i in r.chiede)
        {
            if (i.oggetto == null) continue;
            int hai = DatiRicetta.Hai(zaino, i.oggetto), serve = Mathf.Max(1, i.quanti);
            var riga = new Rect(D.x + 140f, y, D.width - 280f, 46f);
            DisegnaOggetto(new Rect(riga.x, riga.y + 3f, 40f, 40f), i.oggetto, alfa);
            GraficaMenu.Scritta(new Rect(riga.x + 56f, riga.y, riga.width - 160f, riga.height), i.oggetto.Nome, sinistra, GraficaMenu.Testo, alfa);
            GraficaMenu.Scritta(new Rect(riga.xMax - 120f, riga.y, 120f, riga.height), hai + " / " + serve, destra, hai >= serve ? Verde : Rosso, alfa);
            y += 50f;
        }

        // il pulsante "Scambia": acceso se si può fare
        var pulsante = new Rect(cx - 170f, D.yMax - 110f, 340f, 70f);
        bool sopra = pulsante.Contains(e.mousePosition);
        if (sopra && e.type == EventType.MouseDown && e.button == 0)
        {
            e.Use();
            Scambia();
            return true;
        }
        if (possibile) GraficaMenu.Alone(new Rect(pulsante.x - 40f, pulsante.y - 40f, pulsante.width + 80f, pulsante.height + 80f),
            new Color(1f, 0.5f, 0.2f, (sopra ? 0.25f : 0.14f) * alfa));
        GraficaMenu.Cornice(pulsante, alfa, possibile);
        string testo = Lingua.T("neg.scambia").ToUpperInvariant();
        GraficaMenu.Scritta(pulsante, testo, GraficaMenu.Voce, possibile ? GraficaMenu.Selezione : GraficaMenu.Spento, alfa);
        return false;
    }

    // Icona dell'oggetto; per le pozioni senza icona disegna la boccetta del loro colore, per i fiori un fiore.
    static void DisegnaOggetto(Rect r, DatiOggetto o, float alfa)
    {
        if (o == null) return;
        if (o.icona != null) { GraficaMenu.Icona(r, o, alfa); return; }
        Vector2 c = r.center;
        float s = r.width / 60f;
        if (o is DatiConsumabile pozione)
        {
            GraficaMenu.Rombo(c + new Vector2(0f, 6f * s), 34f * s, GraficaMenu.Con(pozione.colore, alfa));
            GraficaMenu.Riempi(new Rect(c.x - 4f * s, c.y - 24f * s, 8f * s, 14f * s), new Color(0.75f, 0.8f, 0.82f, 0.9f * alfa));
            GraficaMenu.Riempi(new Rect(c.x - 6f * s, c.y - 28f * s, 12f * s, 5f * s), new Color(0.45f, 0.32f, 0.2f, alfa));
            return;
        }
        if (o is DatiOggettoMissione fiore && fiore.forma == DatiOggettoMissione.FormaProvvisoria.Fiore)
        {
            GraficaMenu.Riempi(new Rect(c.x - 2f * s, c.y, 4f * s, 26f * s), new Color(0.3f, 0.45f, 0.22f, alfa));
            for (int k = 0; k < 4; k++)
            {
                float a = k * Mathf.PI * 0.5f;
                GraficaMenu.Rombo(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 9f * s + new Vector2(0f, -6f * s), 14f * s, GraficaMenu.Con(fiore.colore, alfa));
            }
            GraficaMenu.Rombo(c + new Vector2(0f, -6f * s), 8f * s, new Color(1f, 0.9f, 0.6f, alfa));
            return;
        }
        GraficaMenu.Icona(r, o, alfa);
    }
}
