using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Un negozio del mercato del villaggio: la bottega di Ilse e, in futuro, fabbro, oste e altri mercanti, ognuno con
// le sue cose (decisioni di Lorenzo, 13 ottobre; chi vende cosa in Docs/monete.md).
// A cosa serve: vicino al negoziante compare "E  Commercia: Ilse"; premendo il tasto si apre la finestra della bottega.
//   - in alto le schede: Baratto (fiori per pozioni, il modo più conveniente), Compra (con le Lacrime del lago,
//     costa di più), Vendi (fiori per Lacrime). Si vedono solo le schede che hanno qualcosa. Q e R (LB e RB) o un clic
//     cambiano scheda; in alto a destra le Lacrime che hai (Borsa);
//   - a sinistra l'elenco della scheda: quelle che puoi fare sono accese, le altre spente;
//   - a destra il dettaglio della voce scelta: descrizione, effetti, cosa serve (verde = ce l'hai, rosso = manca),
//     cosa ricevi e il pulsante "Scambia" / "Compra" / "Vendi".
// Ogni voce è una DatiRicetta: toglie ingredienti e monete, dà l'oggetto o le monete.
// Comandi: frecce, WASD o levetta per scegliere; E, Invio o A per confermare; Q e R per le schede; Esc, Tab o B per
// chiudere. Mouse: clic per scegliere, doppio clic o clic sul pulsante per confermare.
// Il gioco non si ferma (in co-op non potrebbe), ma comandi del personaggio e camera si spengono, come nell'inventario.
// Mentre la bottega è aperta InventarioGioco.Aperto è vero, così gli altri tasti (E, F, Tab...) non fanno niente.
// Come montarlo:
//   1. Sul negoziante (o su un oggetto vuoto accanto, nel punto da cui si parla).
//   2. "Nome Chiave" (pg.ilse) e "Titolo Chiave" (neg.bottega_ilse): chiavi di Lingua.
//   3. "Ricette": le voci del negozio (menu "magic-gnl > Erbe: crea fiori, pozioni e ricette" crea quelle di Ilse).
//   4. "Al Primo Scambio": cosa succede la prima volta che si riceve un oggetto, con il baratto o comprando
//      (la quest 4 passa a "Bevi una pozione"). Vendere non conta.
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
    [Tooltip("Cosa succede la prima volta che si riceve un oggetto (baratto o acquisto).")]
    [SerializeField] UnityEvent alPrimoScambio;

    const float AltezzaRiga = 118f;
    const int RigheVisibili = 5;
    static readonly Color Verde = new Color(0.55f, 0.82f, 0.45f);
    static readonly Color Rosso = new Color(0.9f, 0.38f, 0.32f);
    static readonly string[] chiaviSchede = { "neg.baratto", "neg.compra", "neg.vendi" };
    static readonly string[] chiaviPulsante = { "neg.scambia", "neg.compra_ora", "neg.vendi_ora" };

    readonly ElencoMenu comandi = new ElencoMenu();   // usato solo per leggere frecce, conferma e indietro
    readonly List<Behaviour> spenti = new List<Behaviour>();
    readonly List<DatiRicetta> visibili = new List<DatiRicetta>();   // le voci della scheda scelta
    readonly List<DatiRicetta.Modo> schede = new List<DatiRicetta.Modo>();   // le schede che hanno qualcosa
    GiocatoreControllo giocatore;
    Zaino zaino;
    Borsa borsa;
    InputAction comandoApri;
    int scheda, scelta, rigaIniziale;
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
        borsa = Borsa.Di(giocatore);
        cursorePrima = Cursor.lockState;
        cursoreVisibilePrima = Cursor.visible;
        spenti.Clear();
        foreach (var b in FindObjectsByType<GiocatoreControllo>(FindObjectsSortMode.None)) Spegni(b);
        foreach (var b in FindObjectsByType<CameraTerzaPersona>(FindObjectsSortMode.None)) Spegni(b);
        foreach (var b in FindObjectsByType<AggancioBersaglio>(FindObjectsSortMode.None)) Spegni(b);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // si apre sulla prima scheda che ha qualcosa da fare (di solito il baratto, se hai i fiori)
        PreparaSchede();
        CambiaScheda(0);
        for (int s = 0; s < schede.Count; s++)
        {
            CambiaScheda(s);
            if (PrimaPossibile() >= 0) break;
            if (s == schede.Count - 1) CambiaScheda(0);
        }
        int prima = PrimaPossibile();
        scelta = prima >= 0 ? prima : 0;
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

    // ---------- schede ----------

    void PreparaSchede()
    {
        schede.Clear();
        foreach (DatiRicetta.Modo m in System.Enum.GetValues(typeof(DatiRicetta.Modo)))
            if (ricette != null)
                foreach (var r in ricette)
                    if (r != null && r.Valida && r.Tipo == m) { schede.Add(m); break; }
    }

    void CambiaScheda(int nuova)
    {
        if (schede.Count == 0) { visibili.Clear(); return; }
        scheda = (nuova + schede.Count) % schede.Count;
        visibili.Clear();
        foreach (var r in ricette)
            if (r != null && r.Valida && r.Tipo == schede[scheda]) visibili.Add(r);
        scelta = 0;
        rigaIniziale = 0;
    }

    int PrimaPossibile()
    {
        for (int i = 0; i < visibili.Count; i++) if (visibili[i].Possibile(zaino, borsa)) return i;
        return -1;
    }

    DatiRicetta.Modo ModoScheda => schede.Count > 0 ? schede[scheda] : DatiRicetta.Modo.Baratto;
    DatiRicetta Scelta => scelta >= 0 && scelta < visibili.Count ? visibili[scelta] : null;

    // ---------- scambiare ----------

    void Conferma()
    {
        var r = Scelta;
        if (r == null) return;
        Vector3 suono = giocatore.transform.position + Vector3.up;
        if (!r.Esegui(zaino, borsa))
        {
            bool pocheMonete = r.prezzo > 0 && borsa.Quante < r.prezzo;
            Avvisa(Lingua.T(pocheMonete ? "neg.poche_monete" : "neg.mancano"), Rosso);
            Suoni.Suona(Suono.Negato, suono, 0.6f);
            return;
        }
        if (r.Tipo == DatiRicetta.Modo.Vendi)
        {
            Avvisa(Lingua.T("neg.venduto") + ": " + NomeVoce(r) + GraficaMenu.Separatore + "+" + Borsa.Testo(r.ricavo), Verde);
            Suoni.Suona(Suono.Raccolta, suono, 0.7f, 1.25f);
            return;
        }
        if (r.da is DatiConsumabile c) UsoConsumabili.Seleziona(c);   // la pozione appena presa è pronta per il tasto F
        Avvisa(Lingua.T("hud.raccolto") + ": " + NomeVoce(r), Verde);
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

    // L'oggetto che rappresenta la voce: quello che ricevi, oppure (vendendo) quello che dai.
    static DatiOggetto OggettoVoce(DatiRicetta r)
    {
        if (r.da != null) return r.da;
        if (r.chiede != null) foreach (var i in r.chiede) if (i.oggetto != null) return i.oggetto;
        return null;
    }

    // "Pozione di Ilse x2"; vendendo, "Fiore di riva" (o "Fiore di riva x3" se se ne vendono 3 alla volta).
    static string NomeVoce(DatiRicetta r)
    {
        var o = OggettoVoce(r);
        if (o == null) return "";
        int quanti = r.da != null ? r.quantiDa : PrimoQuanti(r);
        return o.Nome + (quanti > 1 ? " x" + quanti : "");
    }

    static int PrimoQuanti(DatiRicetta r)
    {
        if (r.chiede != null) foreach (var i in r.chiede) if (i.oggetto != null) return Mathf.Max(1, i.quanti);
        return 1;
    }

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
        if ((tastiera != null && tastiera.qKey.wasPressedThisFrame) || (pad != null && pad.leftShoulder.wasPressedThisFrame)) CambiaScheda(scheda - 1);
        if ((tastiera != null && tastiera.rKey.wasPressedThisFrame) || (pad != null && pad.rightShoulder.wasPressedThisFrame)) CambiaScheda(scheda + 1);
        if (c.verticale != 0 && visibili.Count > 0) scelta = Mathf.Clamp(scelta + c.verticale, 0, visibili.Count - 1);
        if (c.conferma || (tastiera != null && tastiera.eKey.wasPressedThisFrame)) Conferma();
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
        GraficaMenu.Scritta(new Rect(0, 40, L, 80), titolo, GraficaMenu.Intestazione, GraficaMenu.Testo, alfa);
        GraficaMenu.Divisore(L * 0.5f, 128f, 640f, alfa);

        // le Lacrime che hai, in alto a destra
        var stileBorsa = new GUIStyle(GraficaMenu.Valore) { alignment = TextAnchor.MiddleRight, fontSize = 30 };
        GraficaMenu.Scritta(new Rect(L - 560f, 60f, 400f, 44f), Borsa.Testo(borsa.Quante), stileBorsa, GraficaMenu.Testo, alfa);
        Borsa.Disegna(new Vector2(L - 130f, 82f), 34f, alfa);

        if (DisegnaSchede(alfa, e)) return;
        if (DisegnaElenco(alfa, e)) return;
        if (DisegnaDettaglio(alfa, e)) return;

        if (Time.unscaledTime < messaggioFino && !string.IsNullOrEmpty(messaggio))
            GraficaMenu.Scritta(new Rect(0, 1036, L, 34), messaggio, GraficaMenu.Piccolo, coloreMessaggio, Mathf.Clamp01(messaggioFino - Time.unscaledTime));
        else
            GraficaMenu.Scritta(new Rect(0, 1040, L, 30), Lingua.T("neg.aiuto"), GraficaMenu.Piccolo, GraficaMenu.Spento, 0.8f * alfa);
    }

    // Le schede sopra l'elenco (solo quelle che hanno qualcosa): Baratto, Compra, Vendi.
    bool DisegnaSchede(float alfa, Event e)
    {
        if (schede.Count <= 1) return false;
        const float x0 = 120f, y = 152f, h = 36f, larghezza = 820f;
        float w = larghezza / schede.Count;
        for (int i = 0; i < schede.Count; i++)
        {
            var r = new Rect(x0 + i * w, y, w, h);
            bool scelta = i == scheda;
            if (e.type == EventType.MouseDown && e.button == 0 && r.Contains(e.mousePosition))
            {
                e.Use();
                CambiaScheda(i);
                return true;
            }
            if (scelta) GraficaMenu.FasciaLuce(new Rect(r.x + 6f, r.y, r.width - 12f, r.height), GraficaMenu.Con(GraficaMenu.Selezione, 0.22f * alfa));
            var stile = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
            GraficaMenu.Scritta(r, Lingua.T(chiaviSchede[(int)schede[i]]).ToUpperInvariant(), stile, scelta ? GraficaMenu.Selezione : GraficaMenu.Spento, alfa);
        }
        var tasto = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
        var tq = new Vector2(x0 - 22f, y + h * 0.5f);
        var tr = new Vector2(x0 + larghezza + 22f, y + h * 0.5f);
        GraficaMenu.Rombo(tq, 30f, GraficaMenu.Con(GraficaMenu.Bronzo, alfa));
        GraficaMenu.Rombo(tr, 30f, GraficaMenu.Con(GraficaMenu.Bronzo, alfa));
        GraficaMenu.Scritta(new Rect(tq.x - 20f, tq.y - 14f, 40f, 28f), "Q", tasto, new Color(0.05f, 0.05f, 0.06f), alfa);
        GraficaMenu.Scritta(new Rect(tr.x - 20f, tr.y - 14f, 40f, 28f), "R", tasto, new Color(0.05f, 0.05f, 0.06f), alfa);
        return false;
    }

    // L'elenco delle voci della scheda, a sinistra. Restituisce true se un clic ha fatto uno scambio (il disegno si ferma lì).
    bool DisegnaElenco(float alfa, Event e)
    {
        var P = new Rect(120f, 200f, 820f, 800f);
        GraficaMenu.Cornice(P, alfa, false);
        if (visibili.Count == 0)
        {
            GraficaMenu.Scritta(new Rect(P.x, P.y + 60f, P.width, 200f), Lingua.T("neg.niente"), GraficaMenu.Didascalia, GraficaMenu.Spento, alfa);
            return false;
        }

        // rotellina sopra l'elenco: scorre di una riga
        if (e.type == EventType.ScrollWheel && P.Contains(e.mousePosition) && visibili.Count > RigheVisibili)
        {
            scelta = Mathf.Clamp(scelta + (e.delta.y > 0f ? 1 : -1), 0, visibili.Count - 1);
            e.Use();
        }
        if (scelta < rigaIniziale) rigaIniziale = scelta;
        if (scelta >= rigaIniziale + RigheVisibili) rigaIniziale = scelta - RigheVisibili + 1;

        var stileNome = new GUIStyle(GraficaMenu.VoceSinistra) { fontSize = 30 };
        var stileRiga = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleLeft, wordWrap = false };
        for (int k = 0; k < RigheVisibili && rigaIniziale + k < visibili.Count; k++)
        {
            int i = rigaIniziale + k;
            var r = visibili[i];
            var riga = new Rect(P.x + 24f, P.y + 30f + k * AltezzaRiga, P.width - 48f, AltezzaRiga - 10f);
            bool possibile = r.Possibile(zaino, borsa);
            bool selezionata = i == scelta;

            if (riga.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                e.Use();
                scelta = i;
                bool doppio = i == ultimoIndiceClic && Time.unscaledTime - ultimoClic < 0.35f;
                ultimoIndiceClic = i;
                ultimoClic = Time.unscaledTime;
                if (doppio) { ultimoIndiceClic = -1; Conferma(); return true; }
            }

            if (selezionata) GraficaMenu.FasciaLuce(riga, GraficaMenu.Con(GraficaMenu.Selezione, 0.18f * alfa));
            var casella = new Rect(riga.x + 10f, riga.y + 9f, 90f, 90f);
            GraficaMenu.Riempi(casella, new Color(0.055f, 0.055f, 0.07f, 0.92f * alfa));
            GraficaMenu.Bordo(casella, selezionata ? 3f : 1f, GraficaMenu.Con(selezionata ? GraficaMenu.Selezione : GraficaMenu.Bronzo, (selezionata ? 1f : 0.6f) * alfa));
            DisegnaOggetto(new Rect(casella.x + 15f, casella.y + 15f, 60f, 60f), OggettoVoce(r), possibile ? alfa : 0.45f * alfa);

            Color colore = selezionata ? GraficaMenu.Selezione : possibile ? GraficaMenu.Testo : GraficaMenu.Spento;
            GraficaMenu.Scritta(new Rect(casella.xMax + 24f, riga.y + 8f, riga.width - 140f, 44f), NomeVoce(r), stileNome, colore, alfa);
            GraficaMenu.Scritta(new Rect(casella.xMax + 24f, riga.y + 56f, riga.width - 140f, 36f), RigaCosto(r), stileRiga,
                possibile ? GraficaMenu.Testo : GraficaMenu.Spento, alfa);
        }
        if (visibili.Count > RigheVisibili)
        {
            var pag = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleRight };
            GraficaMenu.Scritta(new Rect(P.xMax - 240f, P.yMax - 44f, 200f, 34f), (scelta + 1) + " / " + visibili.Count, pag, GraficaMenu.Spento, alfa);
        }
        return false;
    }

    // La seconda riga di una voce: "Fiore di riva 3/3", "30 lacrime" o, vendendo, "Ne hai 3  ·  +4 lacrime".
    string RigaCosto(DatiRicetta r)
    {
        var parti = new List<string>();
        if (r.Tipo == DatiRicetta.Modo.Vendi)
        {
            var o = OggettoVoce(r);
            parti.Add(string.Format(Lingua.T("neg.ne_hai"), DatiRicetta.Hai(zaino, o)));
            parti.Add("+" + Borsa.Testo(r.ricavo));
            return string.Join(GraficaMenu.Separatore, parti);
        }
        if (r.chiede != null)
            foreach (var i in r.chiede)
                if (i.oggetto != null) parti.Add(i.oggetto.Nome + " " + DatiRicetta.Hai(zaino, i.oggetto) + "/" + Mathf.Max(1, i.quanti));
        if (r.prezzo > 0) parti.Add(Borsa.Testo(r.prezzo));
        return string.Join(GraficaMenu.Separatore, parti);
    }

    // Il dettaglio della voce scelta, a destra, con il pulsante per confermare.
    bool DisegnaDettaglio(float alfa, Event e)
    {
        var D = new Rect(1000f, 200f, 800f, 800f);
        GraficaMenu.Cornice(D, alfa, false);
        var r = Scelta;
        if (r == null) return false;
        var oggetto = OggettoVoce(r);
        bool possibile = r.Possibile(zaino, borsa);
        float cx = D.center.x, y = D.y + 36f;

        var grande = new Rect(cx - 70f, y, 140f, 140f);
        GraficaMenu.Alone(new Rect(grande.x - 50f, grande.y - 50f, grande.width + 100f, grande.height + 100f), new Color(1f, 0.5f, 0.2f, 0.12f * alfa));
        GraficaMenu.Cornice(grande, alfa, possibile);
        DisegnaOggetto(new Rect(grande.x + 30f, grande.y + 30f, 80f, 80f), oggetto, alfa);
        y = grande.yMax + 12f;
        GraficaMenu.Scritta(new Rect(D.x + 30f, y, D.width - 60f, 50f), NomeVoce(r), GraficaMenu.NomeGioco, GraficaMenu.Selezione, alfa);
        y += 52f;
        var descrizione = new GUIStyle(GraficaMenu.Didascalia) { wordWrap = true, alignment = TextAnchor.UpperCenter };
        if (oggetto != null) GraficaMenu.Scritta(new Rect(D.x + 60f, y, D.width - 120f, 80f), oggetto.Descrizione, descrizione, GraficaMenu.Testo, alfa);
        y += 84f;
        if (r.da is DatiConsumabile c)
        {
            string effetti = c.Effetti();
            if (effetti.Length > 0) GraficaMenu.Scritta(new Rect(D.x + 30f, y, D.width - 60f, 34f), effetti, GraficaMenu.Didascalia, Verde, alfa);
        }
        y += 44f;

        GraficaMenu.Divisore(cx, y, 520f, alfa);
        y += 20f;
        var et = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
        GraficaMenu.Scritta(new Rect(D.x, y, D.width, 30f), Lingua.T(r.Tipo == DatiRicetta.Modo.Vendi ? "neg.dai" : "neg.ti_serve").ToUpperInvariant(), et, GraficaMenu.Bronzo, alfa);
        y += 38f;
        var sinistra = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleLeft };
        var destra = new GUIStyle(GraficaMenu.Valore) { alignment = TextAnchor.MiddleRight };
        var riga = new Rect(D.x + 140f, y, D.width - 280f, 46f);
        if (r.chiede != null)
            foreach (var i in r.chiede)
            {
                if (i.oggetto == null) continue;
                int hai = DatiRicetta.Hai(zaino, i.oggetto), serve = Mathf.Max(1, i.quanti);
                DisegnaOggetto(new Rect(riga.x, riga.y + 3f, 40f, 40f), i.oggetto, alfa);
                GraficaMenu.Scritta(new Rect(riga.x + 56f, riga.y, riga.width - 160f, riga.height), i.oggetto.Nome, sinistra, GraficaMenu.Testo, alfa);
                GraficaMenu.Scritta(new Rect(riga.xMax - 120f, riga.y, 120f, riga.height), hai + " / " + serve, destra, hai >= serve ? Verde : Rosso, alfa);
                riga.y += 50f;
            }
        if (r.prezzo > 0)
        {
            Borsa.Disegna(new Vector2(riga.x + 20f, riga.y + 23f), 30f, alfa);
            GraficaMenu.Scritta(new Rect(riga.x + 56f, riga.y, riga.width - 160f, riga.height), Lingua.T("valuta.nome"), sinistra, GraficaMenu.Testo, alfa);
            GraficaMenu.Scritta(new Rect(riga.xMax - 160f, riga.y, 160f, riga.height), borsa.Quante + " / " + r.prezzo, destra, borsa.Quante >= r.prezzo ? Verde : Rosso, alfa);
            riga.y += 50f;
        }
        if (r.ricavo > 0)
        {
            riga.y += 8f;
            GraficaMenu.Scritta(new Rect(D.x, riga.y, D.width, 30f), Lingua.T("neg.ricevi").ToUpperInvariant(), et, GraficaMenu.Bronzo, alfa);
            riga.y += 38f;
            Borsa.Disegna(new Vector2(riga.x + 20f, riga.y + 23f), 30f, alfa);
            GraficaMenu.Scritta(new Rect(riga.x + 56f, riga.y, riga.width - 160f, riga.height), Lingua.T("valuta.nome"), sinistra, GraficaMenu.Testo, alfa);
            GraficaMenu.Scritta(new Rect(riga.xMax - 120f, riga.y, 120f, riga.height), "+" + r.ricavo, destra, Verde, alfa);
        }

        // il pulsante: acceso se si può fare
        var pulsante = new Rect(cx - 170f, D.yMax - 100f, 340f, 70f);
        bool sopra = pulsante.Contains(e.mousePosition);
        if (sopra && e.type == EventType.MouseDown && e.button == 0)
        {
            e.Use();
            Conferma();
            return true;
        }
        if (possibile) GraficaMenu.Alone(new Rect(pulsante.x - 40f, pulsante.y - 40f, pulsante.width + 80f, pulsante.height + 80f),
            new Color(1f, 0.5f, 0.2f, (sopra ? 0.25f : 0.14f) * alfa));
        GraficaMenu.Cornice(pulsante, alfa, possibile);
        string testo = Lingua.T(chiaviPulsante[(int)r.Tipo]).ToUpperInvariant();
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
