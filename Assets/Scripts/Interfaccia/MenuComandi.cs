using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Schermata "Comandi" (Opzioni > Comandi, dal menu iniziale e dal menu di pausa).
// In alto una tastiera e un mouse disegnati: i tasti usati dal gioco sono accesi e hanno scritto sotto cosa fanno;
// passando sopra un tasto con il mouse compare cosa fa. Sotto, l'elenco delle azioni che si possono cambiare:
// si sceglie un'azione (frecce e Invio, oppure clic), si preme il tasto nuovo (Esc annulla). Se il tasto era già
// di un'altra azione, le due si scambiano. "Ripristina" rimette tutti i tasti di partenza.
// I tasti scelti sono salvati da Comandi.cs e valgono subito, anche a partita in corso.
// Si crea da solo all'avvio del gioco e si apre con MenuComandi.Apri(): non va messo nelle scene.
// Mentre è aperta, il menu che l'ha aperta (iniziale o pausa) non legge i tasti e non si disegna.
public class MenuComandi : MonoBehaviour
{
    public static bool Aperto { get; private set; }
    // Fotogramma in cui si è chiusa: chi l'ha aperta non deve leggere lo stesso Esc.
    public static int FotogrammaChiusura { get; private set; } = -10;

    static MenuComandi istanza;

    const int Azioni = 12, Ripristina = 12, Indietro = 13, PerColonna = 6;

    readonly ElencoMenu lettore = new ElencoMenu();   // solo per leggere frecce, conferma e indietro
    int selezione;
    bool attesa;                 // si aspetta il tasto nuovo per l'azione scelta
    int fotogrammaAttesa, fotogrammaApertura, fotogrammaAssegnato = -10;
    GUIStyle stileNomeTasto, stileBreve;   // creati una volta sola (si disegnano tanti tasti per fotogramma)
    float tempoApertura;
    string avviso;
    float avvisoFino;
    Vector2 ultimoMouse = new Vector2(-1f, -1f);
    readonly Dictionary<string, string> nomiTasti = new Dictionary<string, string>();

    // Tastiera disegnata: nome del tasto per l'Input System e larghezza (1 = tasto normale).
    struct Tasto
    {
        public string nome;
        public float largo;
        public Tasto(string n, float l = 1f) { nome = n; largo = l; }
    }

    static readonly Tasto[][] righe =
    {
        new[] { new Tasto("escape"), new Tasto(null), new Tasto("f1"), new Tasto("f2") },
        new[] { new Tasto("backquote"), new Tasto("1"), new Tasto("2"), new Tasto("3"), new Tasto("4"), new Tasto("5"), new Tasto("6"),
                new Tasto("7"), new Tasto("8"), new Tasto("9"), new Tasto("0"), new Tasto("minus"), new Tasto("equals"), new Tasto("backspace", 2f) },
        new[] { new Tasto("tab", 1.5f), new Tasto("q"), new Tasto("w"), new Tasto("e"), new Tasto("r"), new Tasto("t"), new Tasto("y"),
                new Tasto("u"), new Tasto("i"), new Tasto("o"), new Tasto("p"), new Tasto("leftBracket"), new Tasto("rightBracket"), new Tasto("backslash", 1.5f) },
        new[] { new Tasto("capsLock", 1.75f), new Tasto("a"), new Tasto("s"), new Tasto("d"), new Tasto("f"), new Tasto("g"), new Tasto("h"),
                new Tasto("j"), new Tasto("k"), new Tasto("l"), new Tasto("semicolon"), new Tasto("quote"), new Tasto("enter", 2.25f) },
        new[] { new Tasto("leftShift", 2.25f), new Tasto("z"), new Tasto("x"), new Tasto("c"), new Tasto("v"), new Tasto("b"), new Tasto("n"),
                new Tasto("m"), new Tasto("comma"), new Tasto("period"), new Tasto("slash"), new Tasto("rightShift", 2.75f) },
        new[] { new Tasto("leftCtrl", 1.25f), new Tasto("leftMeta", 1.25f), new Tasto("leftAlt", 1.25f), new Tasto("space", 6.25f),
                new Tasto("rightAlt", 1.25f), new Tasto("rightMeta", 1.25f), new Tasto("contextMenu", 1.25f), new Tasto("rightCtrl", 1.25f) },
    };

    const float Unita = 72f, Spazio = 6f, AltezzaTasto = 60f;
    const float LarghezzaTastiera = 15f * Unita, LarghezzaMouse = 150f, DistanzaMouse = 60f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreaAllAvvio()
    {
        if (FindFirstObjectByType<MenuComandi>() != null) return;
        var oggetto = new GameObject("Menu dei comandi");
        DontDestroyOnLoad(oggetto);
        istanza = oggetto.AddComponent<MenuComandi>();
    }

    public static void Apri()
    {
        if (istanza == null) istanza = FindFirstObjectByType<MenuComandi>();
        if (istanza == null) CreaAllAvvio();
        if (istanza == null) return;
        istanza.selezione = 0;
        istanza.attesa = false;
        istanza.avviso = null;
        istanza.fotogrammaApertura = Time.frameCount;
        istanza.tempoApertura = Time.unscaledTime;
        istanza.nomiTasti.Clear();
        Aperto = true;
    }

    void Chiudi()
    {
        Aperto = false;
        attesa = false;
        FotogrammaChiusura = Time.frameCount;
    }

    void Avvisa(string testo)
    {
        avviso = testo;
        avvisoFino = Time.unscaledTime + 3f;
    }

    // ---------- tasti ----------

    void Update()
    {
        if (!Aperto || Time.frameCount == fotogrammaApertura) return;

        if (attesa)
        {
            if (Time.frameCount <= fotogrammaAttesa) return;
            string preso = TastoPremuto(out bool annulla);
            if (annulla) { attesa = false; return; }
            if (preso != null) Assegna(preso);
            return;
        }

        var c = lettore.LeggiComandi();
        if (c.indietro) { Chiudi(); return; }
        if (c.verticale != 0 || c.orizzontale != 0) Muovi(c.orizzontale, c.verticale);
        if (c.conferma) Conferma(selezione);
    }

    void Muovi(int dx, int dy)
    {
        if (selezione < Azioni)
        {
            int colonna = selezione / PerColonna, riga = selezione % PerColonna;
            if (dy > 0) selezione = riga < PerColonna - 1 ? selezione + 1 : (colonna == 0 ? Ripristina : Indietro);
            else if (dy < 0 && riga > 0) selezione--;
            if (dx != 0) selezione = Mathf.Clamp(colonna + dx, 0, 1) * PerColonna + riga;
            return;
        }
        if (dy < 0) selezione = selezione == Ripristina ? PerColonna - 1 : Azioni - 1;
        if (dx != 0) selezione = dx < 0 ? Ripristina : Indietro;
    }

    void Conferma(int voce)
    {
        selezione = voce;
        if (voce == Indietro) { Chiudi(); return; }
        if (voce == Ripristina)
        {
            Comandi.RipristinaTutti();
            Avvisa(Lingua.T("comandi.ripristinati"));
            return;
        }
        attesa = true;
        fotogrammaAttesa = Time.frameCount;
    }

    // Il primo tasto (o tasto del mouse) premuto in questo fotogramma. Esc annulla.
    static string TastoPremuto(out bool annulla)
    {
        annulla = false;
        var tastiera = Keyboard.current;
        if (tastiera != null)
        {
            if (tastiera.escapeKey.wasPressedThisFrame) { annulla = true; return null; }
            foreach (var tasto in tastiera.allKeys)
                if (tasto != null && !tasto.synthetic && tasto.wasPressedThisFrame) return "<Keyboard>/" + tasto.name;
        }
        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame) return "<Mouse>/leftButton";
            if (mouse.rightButton.wasPressedThisFrame) return "<Mouse>/rightButton";
            if (mouse.middleButton.wasPressedThisFrame) return "<Mouse>/middleButton";
            if (mouse.backButton.wasPressedThisFrame) return "<Mouse>/backButton";
            if (mouse.forwardButton.wasPressedThisFrame) return "<Mouse>/forwardButton";
        }
        return null;
    }

    void Assegna(string percorso)
    {
        if (Comandi.Riservato(percorso))
        {
            Avvisa(Lingua.T("comandi.riservato"));
            return;   // si resta in attesa di un altro tasto
        }
        attesa = false;
        fotogrammaAssegnato = Time.frameCount;   // il clic usato come tasto nuovo non deve anche scegliere una voce
        int scambiata = Comandi.Imposta((Azione)selezione, percorso);
        if (scambiata >= 0)
            Avvisa(Lingua.T("comandi.scambiato") + ": " + Comandi.NomeAzione((Azione)scambiata) + " = " + Comandi.NomeTasto((Azione)scambiata));
    }

    // ---------- disegno ----------

    void OnGUI()
    {
        if (!Aperto) return;
        GUI.depth = -150;   // sopra al menu di pausa e al menu iniziale
        GraficaMenu.PreparaStili();
        float alfa = Mathf.Clamp01((Time.unscaledTime - tempoApertura) / 0.2f);

        GUI.matrix = Matrix4x4.identity;
        GraficaMenu.Riempi(new Rect(0, 0, Screen.width, Screen.height), new Color(0.02f, 0.02f, 0.03f, 0.92f));
        GraficaMenu.Atmosfera(0.2f);
        GraficaMenu.FoglioVirtuale();

        var e = Event.current;
        bool mouseMosso = false;
        if (e.type == EventType.Repaint)
        {
            mouseMosso = ultimoMouse.x >= 0f && (e.mousePosition - ultimoMouse).sqrMagnitude > 1f;
            ultimoMouse = e.mousePosition;
        }

        float L = GraficaMenu.Larghezza;
        GraficaMenu.Scritta(new Rect(0, 60, L, 80), Lingua.T("menu.comandi").ToUpperInvariant(), GraficaMenu.Intestazione, GraficaMenu.Testo, alfa);
        GraficaMenu.Divisore(L * 0.5f, 150f, 520f, alfa);

        string sopra = DisegnaTastiera(alfa, e);
        string sopraMouse = DisegnaMouse(alfa, e);
        if (sopraMouse != null) sopra = sopraMouse;

        // riga con cosa fa il tasto sotto il mouse, oppure l'avviso
        string riga = Time.unscaledTime < avvisoFino && !string.IsNullOrEmpty(avviso) ? avviso : sopra;
        if (!string.IsNullOrEmpty(riga))
            GraficaMenu.Scritta(new Rect(0, 598, L, 36), riga, GraficaMenu.Didascalia,
                riga == avviso ? GraficaMenu.Selezione : GraficaMenu.Testo, alfa);

        if (DisegnaElenco(alfa, e, mouseMosso)) return;

        string aiuto = attesa ? Lingua.T("comandi.premi_tasto") : Lingua.T("comandi.aiuto");
        GraficaMenu.Scritta(new Rect(0, 1035, L, 30), aiuto, GraficaMenu.Piccolo, attesa ? GraficaMenu.Selezione : GraficaMenu.Spento, 0.85f * alfa);
    }

    float InizioX => (GraficaMenu.Larghezza - (LarghezzaTastiera + DistanzaMouse + LarghezzaMouse)) * 0.5f;

    // Disegna la tastiera; restituisce la descrizione del tasto sotto il mouse (o null).
    string DisegnaTastiera(float alfa, Event e)
    {
        string sotto = null;
        float y = 180f;
        string sceltoPercorso = selezione < Azioni ? Comandi.Percorso((Azione)selezione) : null;
        for (int r = 0; r < righe.Length; r++)
        {
            float x = InizioX;
            foreach (var t in righe[r])
            {
                float w = t.largo * Unita - Spazio;
                if (t.nome != null)
                {
                    var rett = new Rect(x, y, w, AltezzaTasto);
                    string percorso = "<Keyboard>/" + t.nome;
                    string descrizione = DisegnaTasto(rett, percorso, NomeSulTasto(t.nome), sceltoPercorso, alfa);
                    if (rett.Contains(e.mousePosition) && descrizione != null) sotto = descrizione;
                }
                x += t.largo * Unita;
            }
            y += r == 0 ? AltezzaTasto + 16f : AltezzaTasto + Spazio;
        }
        return sotto;
    }

    // Un tasto: acceso se lo usa un'azione (color fiamma se è quella scelta nell'elenco), con sotto cosa fa.
    string DisegnaTasto(Rect r, string percorso, string etichetta, string sceltoPercorso, float alfa)
    {
        int azione = Comandi.AzioneDelTasto(percorso);
        string fisso = CompitoFisso(percorso);
        bool scelto = sceltoPercorso != null && Comandi.Combacia(sceltoPercorso, percorso);
        bool usato = azione >= 0 || fisso != null;

        if (scelto)
        {
            float pulsa = attesa ? 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f) : 1f;
            GraficaMenu.Alone(new Rect(r.x - 30f, r.y - 30f, r.width + 60f, r.height + 60f), new Color(1f, 0.5f, 0.2f, 0.3f * pulsa * alfa));
        }
        Color fondo = azione >= 0 ? new Color(0.16f, 0.11f, 0.07f, 0.95f) : new Color(0.06f, 0.06f, 0.075f, 0.92f);
        GraficaMenu.Riempi(r, GraficaMenu.Con(fondo, alfa));
        Color bordo = scelto ? GraficaMenu.Selezione : azione >= 0 ? GraficaMenu.Bronzo : GraficaMenu.Spento;
        GraficaMenu.Bordo(r, scelto ? 3f : 1f, GraficaMenu.Con(bordo, (usato ? 1f : 0.35f) * alfa));

        if (stileNomeTasto == null || stileNomeTasto.font != GraficaMenu.Etichetta.font)
        {
            stileNomeTasto = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.UpperLeft, fontSize = 17, clipping = TextClipping.Clip };
            stileBreve = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.LowerCenter, fontSize = 13, clipping = TextClipping.Clip };
        }
        GraficaMenu.Scritta(new Rect(r.x + 6f, r.y + 4f, r.width - 10f, 24f), etichetta, stileNomeTasto,
            usato ? GraficaMenu.Testo : GraficaMenu.Spento, (usato ? 1f : 0.55f) * alfa);

        string breve = azione >= 0 ? Lingua.T("comando_breve." + ((Azione)azione).ToString().ToLowerInvariant()) : fisso;
        if (breve != null)
        {
            GraficaMenu.Scritta(new Rect(r.x + 2f, r.y + 28f, r.width - 4f, 28f), breve, stileBreve,
                scelto ? GraficaMenu.Selezione : azione >= 0 ? GraficaMenu.Bronzo : GraficaMenu.Spento, alfa);
        }

        if (azione >= 0) return Comandi.NomeTasto(percorso) + GraficaMenu.Separatore + Comandi.NomeAzione((Azione)azione);
        if (fisso != null) return Comandi.NomeTasto(percorso) + GraficaMenu.Separatore + DescrizioneFissa(percorso);
        return null;
    }

    // Tasti con un compito che non si cambia: Esc pausa, 1-6 armi e incantesimi.
    static string CompitoFisso(string percorso)
    {
        if (percorso == "<Keyboard>/escape") return Lingua.T("comando_breve.pausa");
        for (int i = 1; i <= 6; i++)
            if (percorso == "<Keyboard>/" + i) return i.ToString();
        return null;
    }

    static string DescrizioneFissa(string percorso) =>
        percorso == "<Keyboard>/escape" ? Lingua.T("comandi.fisso_pausa") : Lingua.T("comandi.fisso_numeri");

    // Il nome scritto sul tasto: quello della tastiera del giocatore (AZERTY, QWERTZ...) per lettere e simboli.
    string NomeSulTasto(string nome)
    {
        if (nomiTasti.TryGetValue(nome, out var salvato)) return salvato;
        string testo;
        switch (nome)
        {
            case "escape": testo = "Esc"; break;
            case "f1": testo = "F1"; break;
            case "f2": testo = "F2"; break;
            case "backspace": testo = "Backspace"; break;
            case "tab": testo = "Tab"; break;
            case "capsLock": testo = "Caps"; break;
            case "enter": testo = Lingua.T("tasto.invio"); break;
            case "leftShift": case "rightShift": testo = "Shift"; break;
            case "leftCtrl": case "rightCtrl": testo = "Ctrl"; break;
            case "leftAlt": testo = "Alt"; break;
            case "rightAlt": testo = "Alt Gr"; break;
            case "leftMeta": case "rightMeta": testo = "Win"; break;
            case "contextMenu": testo = "Menu"; break;
            case "space": testo = Lingua.T("tasto.spazio"); break;
            default:
                var controllo = InputSystem.FindControl("<Keyboard>/" + nome);
                testo = controllo != null && !string.IsNullOrEmpty(controllo.displayName) ? controllo.displayName.ToUpperInvariant() : nome.ToUpperInvariant();
                break;
        }
        nomiTasti[nome] = testo;
        return testo;
    }

    // Il mouse a destra della tastiera: tasto sinistro, destro e rotellina, con sotto i compiti fissi del mouse.
    string DisegnaMouse(float alfa, Event e)
    {
        float x = InizioX + LarghezzaTastiera + DistanzaMouse, y = 230f;
        var corpo = new Rect(x, y, LarghezzaMouse, 250f);
        GraficaMenu.Riempi(corpo, GraficaMenu.Con(new Color(0.06f, 0.06f, 0.075f, 0.92f), alfa));
        GraficaMenu.Bordo(corpo, 1f, GraficaMenu.Con(GraficaMenu.Spento, 0.5f * alfa));

        string sceltoPercorso = selezione < Azioni ? Comandi.Percorso((Azione)selezione) : null;
        float meta = LarghezzaMouse * 0.5f;
        var sinistro = new Rect(x + 4f, y + 4f, meta - 22f, 104f);
        var destro = new Rect(x + meta + 18f, y + 4f, meta - 22f, 104f);
        var centro = new Rect(x + meta - 14f, y + 18f, 28f, 70f);
        string sotto = null;
        string d;
        d = DisegnaTasto(sinistro, "<Mouse>/leftButton", "L", sceltoPercorso, alfa);
        if (sinistro.Contains(e.mousePosition)) sotto = d;
        d = DisegnaTasto(destro, "<Mouse>/rightButton", "R", sceltoPercorso, alfa);
        if (destro.Contains(e.mousePosition)) sotto = d;
        d = DisegnaTasto(centro, "<Mouse>/middleButton", "", sceltoPercorso, alfa);
        if (centro.Contains(e.mousePosition)) sotto = d ?? Lingua.T("comandi.rotellina");

        var stile = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.UpperCenter, fontSize = 15, wordWrap = true };
        GraficaMenu.Scritta(new Rect(x - 40f, y + 262f, LarghezzaMouse + 80f, 80f),
            Lingua.T("comandi.rotellina") + "\n" + Lingua.T("comandi.mouse_movimento"), stile, GraficaMenu.Spento, alfa);
        return sotto;
    }

    // Elenco delle azioni in due colonne, poi Ripristina e Indietro. Restituisce true se un clic ha chiuso la schermata.
    bool DisegnaElenco(float alfa, Event e, bool mouseMosso)
    {
        float L = GraficaMenu.Larghezza;
        const float alto = 650f, passo = 54f, larghezzaColonna = 760f, distanza = 80f;
        float x0 = (L - (2f * larghezzaColonna + distanza)) * 0.5f;
        for (int i = 0; i < Azioni; i++)
        {
            int colonna = i / PerColonna, riga = i % PerColonna;
            var r = new Rect(x0 + colonna * (larghezzaColonna + distanza), alto + riga * passo, larghezzaColonna, passo - 8f);
            if (Clic(r, i, e, mouseMosso)) return false;
            bool scelta = selezione == i;
            if (scelta) GraficaMenu.FasciaLuce(r, GraficaMenu.Con(GraficaMenu.Selezione, 0.2f * alfa));
            var nome = new GUIStyle(GraficaMenu.VoceSinistra) { fontSize = 28 };
            GraficaMenu.Scritta(new Rect(r.x + 24f, r.y, r.width - 260f, r.height), Comandi.NomeAzione((Azione)i), nome,
                scelta ? GraficaMenu.Selezione : GraficaMenu.Testo, alfa);

            // il tasto, dentro un riquadro a forma di tasto
            var tasto = new Rect(r.xMax - 230f, r.y + 4f, 210f, r.height - 8f);
            GraficaMenu.Riempi(tasto, GraficaMenu.Con(new Color(0.1f, 0.08f, 0.06f, 0.95f), alfa));
            GraficaMenu.Bordo(tasto, scelta ? 2f : 1f, GraficaMenu.Con(scelta ? GraficaMenu.Selezione : GraficaMenu.Bronzo, alfa));
            bool aspetta = attesa && scelta;
            float pulsa = aspetta ? 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)) : 1f;
            var stileTasto = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter, fontSize = 20, clipping = TextClipping.Clip };
            GraficaMenu.Scritta(tasto, aspetta ? "…" : Comandi.NomeTasto((Azione)i), stileTasto,
                aspetta ? GraficaMenu.Selezione : GraficaMenu.Testo, pulsa * alfa);
        }

        string[] testi = { Lingua.T("comandi.ripristina"), Lingua.T("menu.indietro") };
        for (int k = 0; k < 2; k++)
        {
            int voce = Ripristina + k;
            var r = new Rect(L * 0.5f - 420f + k * 440f, 985f, 400f, 44f);
            if (Clic(r, voce, e, mouseMosso)) return true;
            bool scelta = selezione == voce;
            if (scelta) GraficaMenu.FasciaLuce(r, GraficaMenu.Con(GraficaMenu.Selezione, 0.2f * alfa));
            var stile = new GUIStyle(GraficaMenu.Voce) { fontSize = 28 };
            GraficaMenu.Scritta(r, testi[k], stile, scelta ? GraficaMenu.Selezione : GraficaMenu.Spento, alfa);
        }
        return false;
    }

    // Passando sopra si sceglie, il clic conferma (mentre si aspetta il tasto nuovo il mouse non sceglie altro).
    bool Clic(Rect area, int voce, Event e, bool mouseMosso)
    {
        if (Time.frameCount == fotogrammaAssegnato && e.type == EventType.MouseDown) { e.Use(); return false; }
        if (attesa || !area.Contains(e.mousePosition)) return false;
        if (mouseMosso) selezione = voce;
        if (e.type != EventType.MouseDown || e.button != 0) return false;
        e.Use();
        Conferma(voce);
        return !Aperto;
    }
}
