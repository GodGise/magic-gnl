using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Inventario in partita (disegno approvato in Docs/interfaccia.md). Tab (o I, o Select sul pad) apre e chiude.
//   - a sinistra: la sagoma del personaggio con le 4 caselle (Arma, Scudo, Armatura, Amuleto) e le statistiche;
//   - a destra: lo zaino, una griglia con gli oggetti raccolti;
//   - sotto lo zaino: il dettaglio dell'oggetto scelto, con il confronto con quello che hai addosso
//     (verde = meglio, rosso = peggio).
// Comandi: frecce, WASD o levetta per scegliere; E, Invio o A per equipaggiare (dallo zaino) o togliere
// (da una casella); Tab, Esc o B per chiudere. Mouse: clic per scegliere, doppio clic per equipaggiare o togliere.
// Equipaggiando un oggetto, quello che c'era nella stessa casella torna nello zaino (con lo spadone a due mani
// anche lo scudo). Gli oggetti stanno in Zaino ed Equipaggiamento, sul Giocatore.
// Il gioco non si ferma (in co-op non potrebbe), ma i comandi del personaggio e della camera si spengono.
// Si crea da solo all'avvio del gioco e funziona in ogni scena con un giocatore: non va messo nelle scene.
public class InventarioGioco : MonoBehaviour
{
    public static bool Aperto { get; private set; }
    // Fotogramma in cui l'inventario si è chiuso: il menu di pausa non deve aprirsi con lo stesso Esc.
    public static int FotogrammaChiusura { get; private set; } = -10;

    const int Colonne = 6, RigheVisibili = 3;

    enum Zona { Caselle, Zaino }

    readonly ElencoMenu comandi = new ElencoMenu();   // usato solo per leggere frecce, conferma e indietro
    readonly List<Behaviour> spenti = new List<Behaviour>();
    GiocatoreControllo giocatore;
    Equipaggiamento equipaggiamento;
    Zaino zaino;
    Statistiche statistiche;

    Zona zona = Zona.Zaino;
    int casella;          // 0 Arma, 1 Scudo, 2 Armatura, 3 Amuleto
    int indiceZaino;
    int rigaIniziale;
    float tempoApertura;
    bool riattivaAlProssimoFotogramma;
    CursorLockMode cursorePrima;
    bool cursoreVisibilePrima;
    float ultimoClic;
    int ultimoIndiceClic = -1;
    Vector2 ultimoMouse = new Vector2(-1f, -1f);

    static readonly string[] chiaviCaselle = { "inv.arma", "inv.scudo", "inv.armatura", "inv.amuleto" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreaAllAvvio()
    {
        if (FindFirstObjectByType<InventarioGioco>() != null) return;
        var oggetto = new GameObject("Inventario in partita");
        DontDestroyOnLoad(oggetto);
        oggetto.AddComponent<InventarioGioco>();
    }

    void OnEnable() => SceneManager.sceneLoaded += SceneCaricata;
    void OnDisable() => SceneManager.sceneLoaded -= SceneCaricata;

    void SceneCaricata(Scene s, LoadSceneMode m)
    {
        if (Aperto) Chiudi(false);
        giocatore = null;
    }

    // ---------- apri e chiudi ----------

    void Apri()
    {
        equipaggiamento = giocatore.GetComponent<Equipaggiamento>();
        if (equipaggiamento == null) equipaggiamento = giocatore.gameObject.AddComponent<Equipaggiamento>();
        zaino = Zaino.Di(giocatore);
        statistiche = Statistiche.Di(giocatore);

        cursorePrima = Cursor.lockState;
        cursoreVisibilePrima = Cursor.visible;
        spenti.Clear();
        foreach (var b in FindObjectsByType<GiocatoreControllo>(FindObjectsSortMode.None)) Spegni(b);
        foreach (var b in FindObjectsByType<CameraTerzaPersona>(FindObjectsSortMode.None)) Spegni(b);
        foreach (var b in FindObjectsByType<AggancioBersaglio>(FindObjectsSortMode.None)) Spegni(b);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        zona = zaino.Numero > 0 ? Zona.Zaino : Zona.Caselle;
        indiceZaino = Mathf.Clamp(indiceZaino, 0, Mathf.Max(0, zaino.Numero - 1));
        tempoApertura = Time.unscaledTime;
        Aperto = true;
    }

    void Spegni(Behaviour b)
    {
        if (b == null || !b.enabled) return;
        b.enabled = false;
        spenti.Add(b);
    }

    void Chiudi(bool riattiva)
    {
        Aperto = false;
        FotogrammaChiusura = Time.frameCount;
        if (riattiva) riattivaAlProssimoFotogramma = true;   // così lo stesso tasto non arriva anche alla camera
        else spenti.Clear();
    }

    void Riattiva()
    {
        foreach (var b in spenti)
            if (b != null) b.enabled = true;
        spenti.Clear();
        Cursor.lockState = cursorePrima;
        Cursor.visible = cursoreVisibilePrima;
    }

    // ---------- equipaggiare ----------

    DatiOggetto OggettoInCasella(int i)
    {
        if (equipaggiamento == null) return null;
        switch (i)
        {
            case 0: return equipaggiamento.Arma;
            case 1: return equipaggiamento.Scudo;
            case 2: return equipaggiamento.Armatura;
            default: return equipaggiamento.Amuleto;
        }
    }

    static int CasellaDi(DatiOggetto o) => o is DatiArma ? 0 : o is DatiScudo ? 1 : o is DatiArmatura ? 2 : 3;

    void Equipaggia(DatiOggetto oggetto)
    {
        if (oggetto == null || !zaino.Contiene(oggetto)) return;
        var prima = new DatiOggetto[4];
        for (int i = 0; i < 4; i++) prima[i] = OggettoInCasella(i);

        zaino.Togli(oggetto);
        equipaggiamento.Equipaggia(oggetto);

        // quello che è uscito dalle caselle torna nello zaino (anche lo scudo tolto dallo spadone)
        for (int i = 0; i < 4; i++)
            if (prima[i] != null && prima[i] != OggettoInCasella(i)) zaino.Aggiungi(prima[i]);
        indiceZaino = Mathf.Clamp(indiceZaino, 0, Mathf.Max(0, zaino.Numero - 1));
    }

    void Togli(int i)
    {
        var oggetto = OggettoInCasella(i);
        if (oggetto == null) return;
        switch (i)
        {
            case 0: equipaggiamento.TogliArma(); break;
            case 1: equipaggiamento.TogliScudo(); break;
            case 2: equipaggiamento.TogliArmatura(); break;
            default: equipaggiamento.TogliAmuleto(); break;
        }
        zaino.Aggiungi(oggetto);
    }

    void Conferma()
    {
        if (zona == Zona.Zaino)
        {
            if (indiceZaino >= 0 && indiceZaino < zaino.Numero) Equipaggia(zaino.Oggetti[indiceZaino]);
        }
        else Togli(casella);
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

        var tastiera = Keyboard.current;
        var pad = Gamepad.current;
        bool tasto = (tastiera != null && (tastiera.tabKey.wasPressedThisFrame || tastiera.iKey.wasPressedThisFrame))
                     || (pad != null && pad.selectButton.wasPressedThisFrame);

        if (!Aperto)
        {
            // si apre solo da fermi o camminando: non a metà di un attacco, una schivata o una parata
            if (tasto && giocatore.StatoAttuale == GiocatoreControllo.Stato.Libero && MenuPausa.FotogrammaApertura != Time.frameCount) Apri();
            return;
        }

        if (giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) { Chiudi(true); return; }

        var c = comandi.LeggiComandi();
        if (tasto || c.indietro) { Chiudi(true); return; }
        bool conferma = c.conferma || (tastiera != null && tastiera.eKey.wasPressedThisFrame);

        if (c.orizzontale != 0 || c.verticale != 0) Muovi(c.orizzontale, c.verticale);
        if (conferma) Conferma();
    }

    void Muovi(int dx, int dy)
    {
        if (zona == Zona.Caselle)
        {
            int col = casella % 2, riga = casella / 2;
            if (dx > 0 && col == 1 && zaino.Numero > 0)
            {
                zona = Zona.Zaino;
                indiceZaino = Mathf.Min(zaino.Numero - 1, (rigaIniziale + riga) * Colonne);
                return;
            }
            if (dx != 0) col = Mathf.Clamp(col + dx, 0, 1);
            if (dy != 0) riga = Mathf.Clamp(riga + dy, 0, 1);
            casella = riga * 2 + col;
            return;
        }

        int n = zaino.Numero;
        if (n == 0) { zona = Zona.Caselle; return; }
        int colZ = indiceZaino % Colonne;
        if (dx < 0 && colZ == 0)
        {
            zona = Zona.Caselle;
            casella = (indiceZaino / Colonne - rigaIniziale) <= 0 ? 1 : 3;
            return;
        }
        if (dx > 0 && colZ == Colonne - 1) return;
        int nuovo = indiceZaino + dx + dy * Colonne;
        if (nuovo >= 0 && nuovo < n) indiceZaino = nuovo;
        else if (dy > 0) indiceZaino = n - 1;
    }

    // ---------- disegno ----------

    void OnGUI()
    {
        if (!Aperto || giocatore == null) return;
        GUI.depth = -50;
        GraficaMenu.PreparaStili();
        float comparsa = Mathf.Clamp01((Time.unscaledTime - tempoApertura) / 0.2f);
        GraficaMenu.Atmosfera(0.6f);
        GraficaMenu.FoglioVirtuale();

        var e = Event.current;
        bool mouseMosso = false;
        if (e.type == EventType.Repaint)
        {
            mouseMosso = ultimoMouse.x >= 0f && (e.mousePosition - ultimoMouse).sqrMagnitude > 1f;
            ultimoMouse = e.mousePosition;
        }

        float L = GraficaMenu.Larghezza;
        string titolo = Lingua.T("inv.titolo").ToUpperInvariant();
        if (Lingua.Indice < 6) titolo = GraficaMenu.Spaziato(titolo);
        GraficaMenu.Scritta(new Rect(0, 50, L, 80), titolo, GraficaMenu.Intestazione, GraficaMenu.Testo, comparsa);
        GraficaMenu.Divisore(L * 0.5f, 148f, 640f, comparsa);

        if (DisegnaCaselle(comparsa, e, mouseMosso)) return;
        if (DisegnaZaino(comparsa, e, mouseMosso)) return;
        DisegnaDettaglio(comparsa);

        GraficaMenu.Scritta(new Rect(0, 1035, L, 30), Lingua.T("inv.aiuto"), GraficaMenu.Piccolo, GraficaMenu.Spento, 0.8f * comparsa);
    }

    // Clic singolo sceglie, doppio clic conferma. Restituisce true se ha confermato.
    bool Clic(Rect area, Zona z, int indice, Event e, bool mouseMosso)
    {
        if (!area.Contains(e.mousePosition)) return false;
        if (mouseMosso) { zona = z; if (z == Zona.Caselle) casella = indice; else indiceZaino = indice; }
        if (e.type != EventType.MouseDown || e.button != 0) return false;
        e.Use();
        zona = z;
        if (z == Zona.Caselle) casella = indice; else indiceZaino = indice;
        int chiave = (z == Zona.Caselle ? 0 : 1000) + indice;
        bool doppio = chiave == ultimoIndiceClic && Time.unscaledTime - ultimoClic < 0.35f;
        ultimoIndiceClic = chiave;
        ultimoClic = Time.unscaledTime;
        if (!doppio) return false;
        Conferma();
        ultimoIndiceClic = -1;
        return true;
    }

    bool DisegnaCaselle(float alfa, Event e, bool mouseMosso)
    {
        var P = new Rect(120f, 200f, 640f, 800f);
        GraficaMenu.Cornice(P, alfa, false);
        float cx = P.center.x;

        // sagoma del personaggio
        Color sagoma = new Color(0.16f, 0.15f, 0.18f, 0.95f * alfa);
        GraficaMenu.Alone(new Rect(cx - 50f, 360f, 100f, 100f), sagoma);
        GraficaMenu.Riempi(new Rect(cx - 40f, 375f, 80f, 75f), sagoma);
        GraficaMenu.Riempi(new Rect(cx - 75f, 460f, 150f, 240f), sagoma);
        GraficaMenu.Riempi(new Rect(cx - 60f, 700f, 45f, 120f), sagoma);
        GraficaMenu.Riempi(new Rect(cx + 15f, 700f, 45f, 120f), sagoma);

        Vector2[] centri = { new Vector2(cx - 210f, 430f), new Vector2(cx + 210f, 430f), new Vector2(cx - 210f, 690f), new Vector2(cx + 210f, 690f) };
        for (int i = 0; i < 4; i++)
        {
            var r = new Rect(centri[i].x - 70f, centri[i].y - 70f, 140f, 140f);
            if (Clic(r, Zona.Caselle, i, e, mouseMosso)) return true;
            bool scelta = zona == Zona.Caselle && casella == i;
            if (scelta) GraficaMenu.Alone(new Rect(r.x - 50f, r.y - 50f, r.width + 100f, r.height + 100f), new Color(1f, 0.5f, 0.2f, 0.18f * alfa));
            GraficaMenu.Cornice(r, alfa, scelta);
            var oggetto = OggettoInCasella(i);
            GraficaMenu.Scritta(new Rect(r.x - 40f, r.y - 40f, r.width + 80f, 30f), Lingua.T(chiaviCaselle[i]).ToUpperInvariant(), GraficaMenu.Etichetta,
                scelta ? GraficaMenu.Selezione : GraficaMenu.Bronzo, alfa);
            if (oggetto != null) GraficaMenu.Icona(new Rect(r.x + 30f, r.y + 30f, 80f, 80f), oggetto, alfa);
            else GraficaMenu.Scritta(r, "—", GraficaMenu.Voce, GraficaMenu.Spento, alfa);
            GraficaMenu.Scritta(new Rect(r.x - 70f, r.yMax + 6f, r.width + 140f, 30f), oggetto != null ? oggetto.Nome : Lingua.T("inv.vuoto"),
                GraficaMenu.Didascalia, oggetto != null ? GraficaMenu.Testo : GraficaMenu.Spento, alfa);
        }

        // statistiche totali
        var arma = equipaggiamento != null ? equipaggiamento.Arma : null;
        string[,] righe =
        {
            { Lingua.T("stat.vita"), Mathf.CeilToInt(giocatore.VitaMassima).ToString() },
            { Lingua.T("stat.armatura"), statistiche != null ? Mathf.RoundToInt(statistiche.Armatura).ToString() : "—" },
            { Lingua.T("stat.danno"), arma != null ? Mathf.RoundToInt(arma.danno).ToString() : "—" },
            { Lingua.T("stat.critico"), statistiche != null ? Mathf.RoundToInt(statistiche.ProbabilitaCritico) + "%" : "—" },
        };
        for (int i = 0; i < 4; i++)
        {
            float x = P.x + 50f + i * 145f;
            var et = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleLeft };
            GraficaMenu.Scritta(new Rect(x, 925f, 140f, 26f), righe[i, 0], et, GraficaMenu.Spento, alfa);
            GraficaMenu.Scritta(new Rect(x, 952f, 140f, 30f), righe[i, 1], GraficaMenu.Valore, GraficaMenu.Testo, alfa);
        }
        return false;
    }

    bool DisegnaZaino(float alfa, Event e, bool mouseMosso)
    {
        var Z = new Rect(820f, 200f, 980f, 490f);
        GraficaMenu.Cornice(Z, alfa, false);
        var et = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft, fontSize = 24 };
        GraficaMenu.Scritta(new Rect(Z.x + 40f, Z.y + 16f, 400f, 34f), Lingua.T("inv.zaino").ToUpperInvariant(), et, GraficaMenu.Bronzo, alfa);

        int n = zaino.Numero;
        int righeTotali = Mathf.Max(1, Mathf.CeilToInt(n / (float)Colonne));
        int rigaScelta = indiceZaino / Colonne;
        if (zona == Zona.Zaino)
        {
            if (rigaScelta < rigaIniziale) rigaIniziale = rigaScelta;
            if (rigaScelta >= rigaIniziale + RigheVisibili) rigaIniziale = rigaScelta - RigheVisibili + 1;
        }
        rigaIniziale = Mathf.Clamp(rigaIniziale, 0, Mathf.Max(0, righeTotali - RigheVisibili));
        if (righeTotali > RigheVisibili)
        {
            var pag = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleRight };
            GraficaMenu.Scritta(new Rect(Z.xMax - 240f, Z.y + 16f, 200f, 34f), (rigaIniziale + 1) + "–" + (rigaIniziale + RigheVisibili) + " / " + righeTotali, pag, GraficaMenu.Spento, alfa);
        }

        if (n == 0)
            GraficaMenu.Scritta(new Rect(Z.x, Z.y + 60f, Z.width, Z.height - 60f), Lingua.T("inv.zaino_vuoto"), GraficaMenu.Didascalia, GraficaMenu.Spento, alfa);

        for (int riga = 0; riga < RigheVisibili; riga++)
            for (int col = 0; col < Colonne; col++)
            {
                int i = (rigaIniziale + riga) * Colonne + col;
                var r = new Rect(Z.x + 20f + col * 150f, Z.y + 65f + riga * 140f, 110f, 110f);
                bool pieno = i < n;
                bool scelta = pieno && zona == Zona.Zaino && i == indiceZaino;
                if (pieno && Clic(r, Zona.Zaino, i, e, mouseMosso)) return true;
                if (scelta) GraficaMenu.Alone(new Rect(r.x - 40f, r.y - 40f, r.width + 80f, r.height + 80f), new Color(1f, 0.5f, 0.2f, 0.22f * alfa));
                GraficaMenu.Riempi(r, new Color(0.055f, 0.055f, 0.07f, 0.9f * alfa));
                GraficaMenu.Bordo(r, scelta ? 3f : 1f, GraficaMenu.Con(scelta ? GraficaMenu.Selezione : GraficaMenu.Bronzo, (scelta ? 1f : 0.55f) * alfa));
                if (pieno) GraficaMenu.Icona(new Rect(r.x + 15f, r.y + 15f, 80f, 80f), zaino.Oggetti[i], alfa);
            }
        return false;
    }

    // ---------- dettaglio e confronto ----------

    struct Riga
    {
        public string etichetta, valore;
        public float numero;
        public bool confrontabile, piuAltoMeglio;
    }

    void DisegnaDettaglio(float alfa)
    {
        DatiOggetto oggetto = zona == Zona.Zaino
            ? (indiceZaino >= 0 && indiceZaino < zaino.Numero ? zaino.Oggetti[indiceZaino] : null)
            : OggettoInCasella(casella);
        var D = new Rect(820f, 720f, 980f, 280f);
        GraficaMenu.Cornice(D, alfa, oggetto != null);
        if (oggetto == null) return;

        var nome = new GUIStyle(GraficaMenu.NomeClasse) { alignment = TextAnchor.MiddleLeft };
        GraficaMenu.Scritta(new Rect(D.x + 40f, D.y + 22f, D.width * 0.6f, 44f), oggetto.Nome.ToUpperInvariant(), nome, GraficaMenu.Selezione, alfa);
        var tipo = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleRight };
        GraficaMenu.Scritta(new Rect(D.x + D.width * 0.45f, D.y + 22f, D.width * 0.55f - 40f, 44f), Tipo(oggetto) + GraficaMenu.Separatore + NomeClasse(oggetto.classe), tipo, GraficaMenu.Spento, alfa);
        GraficaMenu.Scritta(new Rect(D.x + 40f, D.y + 70f, D.width - 80f, 64f), oggetto.Descrizione, GraficaMenu.TestoSinistra, GraficaMenu.Testo, alfa);

        // confronto con quello che c'è addosso nella stessa casella (solo per gli oggetti dello zaino)
        DatiOggetto addosso = zona == Zona.Zaino ? OggettoInCasella(CasellaDi(oggetto)) : null;
        var righe = Righe(oggetto);
        var righeAddosso = addosso != null && addosso.GetType() == oggetto.GetType() ? Righe(addosso) : null;

        var et = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleLeft };
        for (int i = 0; i < righe.Count && i < 4; i++)
        {
            float x = D.x + 40f + i * 235f;
            var riga = righe[i];
            GraficaMenu.Scritta(new Rect(x, D.y + 150f, 230f, 28f), riga.etichetta, et, GraficaMenu.Spento, alfa);
            GraficaMenu.Scritta(new Rect(x, D.y + 186f, 230f, 34f), riga.valore, GraficaMenu.Valore, GraficaMenu.Testo, alfa);

            if (righeAddosso == null || !riga.confrontabile || i >= righeAddosso.Count) continue;
            float diff = riga.numero - righeAddosso[i].numero;
            if (Mathf.Abs(diff) < 0.05f) continue;
            bool meglio = (diff > 0f) == riga.piuAltoMeglio;
            float wValore = GraficaMenu.Valore.CalcSize(new GUIContent(riga.valore)).x;
            string testo = (diff > 0f ? "+" : "-") + Formatta(Mathf.Abs(diff));
            var d = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft };
            GraficaMenu.Scritta(new Rect(x + wValore + 12f, D.y + 186f, 120f, 34f), testo, d,
                meglio ? new Color(0.47f, 0.67f, 0.35f) : new Color(0.75f, 0.27f, 0.24f), alfa);
        }
    }

    static string Formatta(float v) => Mathf.Approximately(v, Mathf.Round(v)) ? Mathf.RoundToInt(v).ToString() : v.ToString("0.#");

    List<Riga> Righe(DatiOggetto o)
    {
        var righe = new List<Riga>();
        void Aggiungi(string chiave, string valore, float numero, bool confronta, bool piuAlto) =>
            righe.Add(new Riga { etichetta = Lingua.T(chiave), valore = valore, numero = numero, confrontabile = confronta, piuAltoMeglio = piuAlto });

        switch (o)
        {
            case DatiArma a:
                Aggiungi("stat.danno", Formatta(a.danno), a.danno, true, true);
                Aggiungi("stat.critico", a.probabilitaCritico > 0f ? "+" + Formatta(a.probabilitaCritico) + "%" : "—", a.probabilitaCritico, true, true);
                Aggiungi("stat.costo", Formatta(a.costoAttacco), a.costoAttacco, true, false);
                float tempo = a.preparazione + a.recupero;
                Aggiungi("stat.velocita", Lingua.T(tempo <= 0.62f ? "stat.rapida" : tempo <= 0.85f ? "stat.media" : "stat.lenta"), -tempo, false, true);
                break;
            case DatiScudo s:
                Aggiungi("stat.parata", Mathf.RoundToInt(s.dannoAssorbito * 100f) + "%", s.dannoAssorbito * 100f, true, true);
                Aggiungi("stat.costo_parata", Formatta(s.costoColpoParato), s.costoColpoParato, true, false);
                Aggiungi("stat.schivata", "+" + Formatta(s.costoSchivataExtra), s.costoSchivataExtra, true, false);
                Aggiungi("stat.parata_perfetta", Impostazioni.SiNo(s.finestraParataPerfetta > 0f), 0f, false, true);
                break;
            case DatiArmatura b:
                Aggiungi("stat.armatura", Formatta(b.armatura), b.armatura, true, true);
                Aggiungi("stat.peso", Lingua.T(b.peso == DatiArmatura.Peso.Leggera ? "stat.leggera" : b.peso == DatiArmatura.Peso.Media ? "stat.media" : "stat.pesante"), 0f, false, true);
                Aggiungi("stat.schivata", "+" + Formatta(b.costoSchivataExtra), b.costoSchivataExtra, true, false);
                Aggiungi("stat.movimento", "-" + Mathf.RoundToInt((1f - b.moltiplicatoreVelocita) * 100f) + "%", (1f - b.moltiplicatoreVelocita) * 100f, true, false);
                break;
            case DatiAmuleto c:
                Aggiungi("stat.tipo", Lingua.T(c.tipo == DatiAmuleto.Tipo.Magico ? "stat.magico" : "stat.arcano"), 0f, false, true);
                break;
        }
        return righe;
    }

    static string Tipo(DatiOggetto o)
    {
        switch (o)
        {
            case DatiArma a:
                return Lingua.T(a.tipo == DatiArma.Tipo.Spada ? "tipo.spada" : a.tipo == DatiArma.Tipo.Spadone ? "tipo.spadone" : a.tipo == DatiArma.Tipo.Ascia ? "tipo.ascia" : "tipo.mazza");
            case DatiScudo s:
                return Lingua.T(s.taglia == DatiScudo.Taglia.Grande ? "tipo.scudo_grande" : s.taglia == DatiScudo.Taglia.Medio ? "tipo.scudo_medio" : "tipo.scudo_piccolo");
            case DatiArmatura b:
                return Lingua.T(b.peso == DatiArmatura.Peso.Leggera ? "tipo.armatura_leggera" : b.peso == DatiArmatura.Peso.Media ? "tipo.armatura_media" : "tipo.armatura_pesante");
            case DatiAmuleto c:
                return Lingua.T(c.tipo == DatiAmuleto.Tipo.Magico ? "tipo.amuleto_magico" : "tipo.amuleto_arcano");
        }
        return "";
    }

    static string NomeClasse(ClasseGiocatore classe) =>
        Lingua.T(classe == ClasseGiocatore.Guerriero ? "classe.guerriero" : classe == ClasseGiocatore.Ladro ? "classe.ladro" : "classe.stregone");
}
