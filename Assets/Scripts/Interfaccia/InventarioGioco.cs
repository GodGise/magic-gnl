using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Inventario in partita (disegno approvato in Docs/interfaccia.md). Tab (o il tasto scelto in Opzioni > Comandi, o Select sul pad) apre e chiude.
//   - a sinistra: la sagoma del personaggio con le 4 caselle (Arma, Scudo, Armatura, Amuleto) e le statistiche;
//   - a destra: lo zaino, una griglia con gli oggetti raccolti;
//   - sotto lo zaino: il dettaglio dell'oggetto scelto, con il confronto con quello che hai addosso
//     (verde = meglio, rosso = peggio).
// Lo zaino non ha limite ed è diviso in schede, come in Elden Ring: Tutto, Armi (anche archi e balestre), Scudi,
// Armature, Amuleti. Si cambia scheda con Q e R (LB e RB sul pad) o con un clic sul nome della scheda.
// Comandi: frecce, WASD o levetta per scegliere; E, Invio o A per equipaggiare (dallo zaino) o togliere
// (da una casella); Tab, Esc o B per chiudere. Mouse: clic per scegliere, doppio clic per equipaggiare o togliere.
// Trascinare con il mouse: un oggetto dello zaino su una casella lo equipaggia lì (quello che c'era torna nello
// zaino); un oggetto di una casella sullo zaino lo toglie; un incantesimo su un'altra casella degli incantesimi
// li scambia. Sostituire con due clic: clic su una casella (resta accesa), poi clic su un oggetto dello zaino.
// Equipaggiando un oggetto, quello che c'era nella stessa casella torna nello zaino (con lo spadone a due mani
// anche lo scudo). Gli oggetti stanno in Zaino ed Equipaggiamento, sul Giocatore.
// Il gioco non si ferma (in co-op non potrebbe), ma i comandi del personaggio e della camera si spengono.
// Ladro: la seconda casella è l'Arma a distanza (arco o balestra) al posto dello Scudo, così le caselle restano 4
// (proposta di Lorenzo in Docs/oggetti-ladro.md). Il Ladro non usa scudi e le altre classi non usano archi:
// provando a equipaggiarli compare un avviso in basso.
// Stregone (Docs/incantesimi-stregone.md): la prima casella è il Bastone, la seconda il Libro (al posto dello Scudo);
// la scheda Scudi diventa Libri e c'è una scheda in più, Incantesimi. Sotto le 4 caselle c'è la fila delle caselle
// degli incantesimi (4, fino a 6 con certi libri, tasti 1-6 in partita): un incantesimo dallo zaino va nella prima
// casella libera, oppure, se sono tutte piene, al posto di quello nella casella scelta per ultima. Al massimo 2
// incantesimi della stessa scuola. Per bastoni, libri, vesti e amuleti il dettaglio mostra i pro (verdi) e i contro
// (rossi) al posto dei numeri.
// Oggetti di altre classi (decisione di Lorenzo del 9 ottobre): armi, scudi, armature, archi e libri si equipaggiano
// solo con la propria classe; gli amuleti sono in comune fra tutte le classi, tranne Ultimo respiro (solo Ladro).
// Si crea da solo all'avvio del gioco e funziona in ogni scena con un giocatore: non va messo nelle scene.
public class InventarioGioco : MonoBehaviour
{
    public static bool Aperto { get; private set; }
    // Fotogramma in cui l'inventario si è chiuso: il menu di pausa non deve aprirsi con lo stesso Esc.
    public static int FotogrammaChiusura { get; private set; } = -10;

    const int Colonne = 9, RigheVisibili = 4;   // 36 caselle visibili; lo zaino non ha limite, il resto si scorre con la rotellina o le frecce

    enum Zona { Caselle, Zaino }

    readonly ElencoMenu comandi = new ElencoMenu();   // usato solo per leggere frecce, conferma e indietro
    readonly List<Behaviour> spenti = new List<Behaviour>();
    GiocatoreControllo giocatore;
    Equipaggiamento equipaggiamento;
    Zaino zaino;
    Statistiche statistiche;

    Zona zona = Zona.Zaino;
    int casella;          // 0 Arma, 1 Scudo, 2 Armatura, 3 Amuleto; Stregone: da 4 in su le caselle degli incantesimi
    int ultimaCasellaIncantesimo;   // l'ultima casella degli incantesimi scelta (lì va un incantesimo se sono tutte piene)
    int indiceZaino;          // posizione nella scheda scelta (non in tutto lo zaino)
    int scheda;               // 0 Tutto, 1 Armi, 2 Scudi, 3 Armature, 4 Amuleti
    static readonly string[] chiaviSchede = { "inv.tutto", "inv.armi", "inv.scudi", "inv.armature", "inv.amuleti", "inv.incantesimi" };
    // Lo Stregone ha la scheda Incantesimi in più.
    static int NumeroSchede => Stregone ? 6 : 5;
    // Gli oggetti dello zaino che si vedono nella scheda scelta.
    readonly List<DatiOggetto> visibili = new List<DatiOggetto>();
    int rigaIniziale;
    float tempoApertura;
    bool riattivaAlProssimoFotogramma;
    CursorLockMode cursorePrima;
    bool cursoreVisibilePrima;
    float ultimoClic;
    int ultimoIndiceClic = -1;
    Vector2 ultimoMouse = new Vector2(-1f, -1f);

    // Trascinamento con il mouse: l'oggetto preso, da dove (casella o zaino) e se il mouse si è già mosso abbastanza.
    DatiOggetto preso;
    bool presoDaCasella;
    int presoIndice;
    Vector2 puntoPresa;
    bool trascinando;
    // Casella "accesa" con un clic: il prossimo clic su un oggetto adatto dello zaino lo mette lì. -1 = nessuna.
    int casellaAccesa = -1;
    // Posizioni delle caselle e dello zaino (dall'ultimo disegno), per capire dove si lascia l'oggetto.
    readonly Rect[] rettCaselle = new Rect[10];
    Rect rettZaino;

    static readonly string[] chiaviCaselle = { "inv.arma", "inv.scudo", "inv.armatura", "inv.amuleto" };

    // Il Ladro ha l'arma a distanza nella seconda casella.
    static bool Ladro => SceltaPartita.Classe == ClasseGiocatore.Ladro;
    // Lo Stregone ha il libro nella seconda casella.
    static bool Stregone => SceltaPartita.Classe == ClasseGiocatore.Stregone;
    static string ChiaveCasella(int i) => i == 0 && Stregone ? "inv.bastone" : i == 1 && Ladro ? "inv.distanza"
        : i == 1 && Stregone ? "inv.libro" : i >= 4 ? "inv.incantesimi" : chiaviCaselle[i];
    int CaselleIncantesimi => Stregone && equipaggiamento != null ? equipaggiamento.NumeroCaselle : 0;
    static string ChiaveScheda(int i) => i == 2 && Stregone ? "inv.libri" : chiaviSchede[i];

    // Avviso in basso (per esempio "Il Ladro non usa scudi"), per qualche secondo.
    string avviso;
    float avvisoFino;

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

        AggiornaVisibili();
        zona = visibili.Count > 0 ? Zona.Zaino : Zona.Caselle;
        indiceZaino = Mathf.Clamp(indiceZaino, 0, Mathf.Max(0, visibili.Count - 1));
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
        preso = null;
        trascinando = false;
        casellaAccesa = -1;
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
        if (i >= 4) return equipaggiamento.Incantesimo(i - 4);
        switch (i)
        {
            case 0: return Stregone ? equipaggiamento.Bastone : (DatiOggetto)equipaggiamento.Arma;
            case 1: return Ladro ? equipaggiamento.ArmaDistanza : Stregone ? equipaggiamento.Libro : (DatiOggetto)equipaggiamento.Scudo;
            case 2: return equipaggiamento.Armatura;
            default: return equipaggiamento.Amuleto;
        }
    }

    static bool NellaScheda(DatiOggetto o, int s) => s == 0
        || (s == 1 && (o is DatiArma || o is DatiArmaDistanza || o is DatiBastone)) || (s == 2 && (o is DatiScudo || o is DatiLibro))
        || (s == 3 && o is DatiArmatura) || (s == 4 && o is DatiAmuleto) || (s == 5 && o is DatiIncantesimo);

    void AggiornaVisibili()
    {
        visibili.Clear();
        if (zaino == null) return;
        foreach (var o in zaino.Oggetti)
            if (o != null && NellaScheda(o, scheda)) visibili.Add(o);
    }

    int QuantiNellaScheda(int s)
    {
        int n = 0;
        if (zaino != null) foreach (var o in zaino.Oggetti) if (o != null && NellaScheda(o, s)) n++;
        return n;
    }

    void CambiaScheda(int nuova)
    {
        scheda = (nuova + NumeroSchede) % NumeroSchede;
        indiceZaino = 0;
        rigaIniziale = 0;
        AggiornaVisibili();
        if (visibili.Count == 0 && zona == Zona.Zaino) zona = Zona.Caselle;
    }

    int CasellaDi(DatiOggetto o) => o is DatiIncantesimo ? 4 + ultimaCasellaIncantesimo
        : o is DatiArma || o is DatiBastone ? 0 : o is DatiScudo || o is DatiArmaDistanza || o is DatiLibro ? 1 : o is DatiArmatura ? 2 : 3;

    void Equipaggia(DatiOggetto oggetto)
    {
        if (oggetto == null || !zaino.Contiene(oggetto)) return;
        string negato = Negato(oggetto);
        if (negato != null)
        {
            avviso = Lingua.T(negato);
            avvisoFino = Time.unscaledTime + 3f;
            Suoni.Suona(Suono.Negato, giocatore.transform.position + Vector3.up, 0.6f);
            return;
        }
        if (oggetto is DatiIncantesimo incantesimo)
        {
            MettiIncantesimo(incantesimo);
            return;
        }
        var prima = new DatiOggetto[4];
        for (int i = 0; i < 4; i++) prima[i] = OggettoInCasella(i);

        zaino.Togli(oggetto);
        equipaggiamento.Equipaggia(oggetto);

        // quello che è uscito dalle caselle torna nello zaino (anche lo scudo tolto dallo spadone)
        for (int i = 0; i < 4; i++)
            if (prima[i] != null && prima[i] != OggettoInCasella(i)) zaino.Aggiungi(prima[i]);
        indiceZaino = Mathf.Clamp(indiceZaino, 0, Mathf.Max(0, visibili.Count - 1));
    }

    // Ladro e Stregone non usano scudi; archi e balestre sono solo del Ladro, i libri solo dello Stregone;
    // poi la regola degli oggetti delle altre classi. null = si può equipaggiare.
    static string Negato(DatiOggetto oggetto) =>
        oggetto is DatiScudo && Ladro ? "inv.no_scudo_ladro"
        : oggetto is DatiScudo && Stregone ? "inv.no_scudo_stregone"
        : oggetto is DatiArmaDistanza && !Ladro ? "inv.no_distanza"
        : oggetto is DatiLibro && !Stregone ? "inv.no_libro"
        : ChiaveAltraClasse(oggetto);

    // La casella i può ricevere questo oggetto? (le caselle da 4 in su sono quelle degli incantesimi)
    bool Accetta(int i, DatiOggetto o)
    {
        if (o == null) return false;
        if (i >= 4) return o is DatiIncantesimo && i - 4 < CaselleIncantesimi;
        return !(o is DatiIncantesimo) && CasellaDi(o) == i;
    }

    // Mette un oggetto dello zaino proprio nella casella i (trascinato lì, o dopo aver acceso la casella).
    void MettiInCasella(int i, DatiOggetto o)
    {
        if (!Accetta(i, o) || !zaino.Contiene(o)) return;
        if (i < 4) { Equipaggia(o); return; }
        string negato = Negato(o);
        if (negato != null) { Avvisa(negato); return; }
        var incantesimo = (DatiIncantesimo)o;
        int k = i - 4;
        if (!equipaggiamento.RispettaScuole(k, incantesimo)) { Avvisa("inv.massimo_scuola"); return; }
        zaino.Togli(incantesimo);
        var vecchio = equipaggiamento.MettiIncantesimo(k, incantesimo);
        if (vecchio != null) zaino.Aggiungi(vecchio);
        ultimaCasellaIncantesimo = k;
        zona = Zona.Caselle;
        casella = i;
    }

    // Scambia gli incantesimi di due caselle (le scuole non cambiano, quindi la regola resta rispettata).
    void ScambiaIncantesimi(int a, int b)
    {
        if (a == b) return;
        var primo = equipaggiamento.Incantesimo(a);
        var secondo = equipaggiamento.Incantesimo(b);
        equipaggiamento.MettiIncantesimo(b, primo);
        equipaggiamento.MettiIncantesimo(a, secondo);
        zona = Zona.Caselle;
        casella = 4 + b;
        ultimaCasellaIncantesimo = b;
    }

    // Il mouse lascia l'oggetto trascinato nel punto p.
    void Lascia(Vector2 p)
    {
        int sopra = -1;
        for (int i = 0; i < 4 + CaselleIncantesimi; i++)
            if (rettCaselle[i].Contains(p)) { sopra = i; break; }

        if (!presoDaCasella)
        {
            if (sopra < 0) return;
            if (Accetta(sopra, preso)) MettiInCasella(sopra, preso);
            else Avvisa("inv.non_va_qui");
            return;
        }
        if (sopra < 0)
        {
            if (rettZaino.Contains(p)) Togli(presoIndice);   // dalla casella allo zaino: si toglie
            return;
        }
        if (presoIndice >= 4 && sopra >= 4) ScambiaIncantesimi(presoIndice - 4, sopra - 4);
    }

    // Un incantesimo va nella prima casella libera; se sono tutte piene, al posto di quello nell'ultima casella
    // scelta (che torna nello zaino). Al massimo 2 della stessa scuola.
    void MettiIncantesimo(DatiIncantesimo incantesimo)
    {
        int dove = equipaggiamento.CasellaLibera();
        if (dove < 0) dove = Mathf.Clamp(ultimaCasellaIncantesimo, 0, equipaggiamento.NumeroCaselle - 1);
        if (!equipaggiamento.RispettaScuole(dove, incantesimo))
        {
            Avvisa("inv.massimo_scuola");
            return;
        }
        zaino.Togli(incantesimo);
        var vecchio = equipaggiamento.MettiIncantesimo(dove, incantesimo);
        if (vecchio != null) zaino.Aggiungi(vecchio);
        ultimaCasellaIncantesimo = dove;
        indiceZaino = Mathf.Clamp(indiceZaino, 0, Mathf.Max(0, visibili.Count - 1));
    }

    void Avvisa(string chiave)
    {
        avviso = Lingua.T(chiave);
        avvisoFino = Time.unscaledTime + 3f;
        Suoni.Suona(Suono.Negato, giocatore.transform.position + Vector3.up, 0.6f);
    }

    // Avviso per un oggetto di un'altra classe: armi e armature solo della propria classe, amuleti in comune
    // (Ultimo respiro, l'invisibilità col tasto Q, resta solo del Ladro). null = si può equipaggiare.
    static string ChiaveAltraClasse(DatiOggetto oggetto)
    {
        var mia = SceltaPartita.Classe;
        if (oggetto is DatiAmuleto amuleto)
            return amuleto.effetto == DatiAmuleto.Effetto.SvanireNellOmbra && mia != ClasseGiocatore.Ladro ? "inv.amuleto_solo_ladro" : null;
        if (oggetto.classe == mia) return null;
        return oggetto.classe == ClasseGiocatore.Guerriero ? "inv.solo_guerriero"
            : oggetto.classe == ClasseGiocatore.Ladro ? "inv.solo_ladro" : "inv.solo_stregone";
    }

    void Togli(int i)
    {
        var oggetto = OggettoInCasella(i);
        if (oggetto == null) return;
        if (i >= 4)
        {
            equipaggiamento.TogliIncantesimo(i - 4);
            zaino.Aggiungi(oggetto);
            return;
        }
        switch (i)
        {
            case 0: if (Stregone) equipaggiamento.TogliBastone(); else equipaggiamento.TogliArma(); break;
            case 1:
                if (Ladro) equipaggiamento.TogliArmaDistanza();
                else if (Stregone) equipaggiamento.TogliLibro();
                else equipaggiamento.TogliScudo();
                break;
            case 2: equipaggiamento.TogliArmatura(); break;
            default: equipaggiamento.TogliAmuleto(); break;
        }
        zaino.Aggiungi(oggetto);
    }

    void Conferma()
    {
        if (zona == Zona.Zaino)
        {
            if (indiceZaino >= 0 && indiceZaino < visibili.Count) Equipaggia(visibili[indiceZaino]);
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
        // il tasto dell'inventario si sceglie in Opzioni > Comandi (Tab all'inizio)
        bool tasto = Comandi.PremutoOra(Azione.Inventario) || (pad != null && pad.selectButton.wasPressedThisFrame);

        if (!Aperto)
        {
            // si apre solo da fermi o camminando: non a metà di un attacco, una schivata o una parata
            if (tasto && giocatore.StatoAttuale == GiocatoreControllo.Stato.Libero && MenuPausa.FotogrammaApertura != Time.frameCount) Apri();
            return;
        }

        if (giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto) { Chiudi(true); return; }

        AggiornaVisibili();
        if (casella >= 4 + CaselleIncantesimi) casella = 3;   // meno caselle di prima (libro tolto)
        if (zona == Zona.Caselle && casella >= 4) ultimaCasellaIncantesimo = casella - 4;
        var c = comandi.LeggiComandi();
        if (tasto || c.indietro) { Chiudi(true); return; }
        // schede dello zaino: Q indietro, R avanti (LB e RB sul pad)
        if ((tastiera != null && tastiera.qKey.wasPressedThisFrame) || (pad != null && pad.leftShoulder.wasPressedThisFrame)) CambiaScheda(scheda - 1);
        if ((tastiera != null && tastiera.rKey.wasPressedThisFrame) || (pad != null && pad.rightShoulder.wasPressedThisFrame)) CambiaScheda(scheda + 1);
        bool conferma = c.conferma || (tastiera != null && tastiera.eKey.wasPressedThisFrame);

        if (c.orizzontale != 0 || c.verticale != 0) { casellaAccesa = -1; Muovi(c.orizzontale, c.verticale); }
        if (conferma) { casellaAccesa = -1; Conferma(); }
        // pulsante lasciato fuori dalla finestra: il trascinamento si annulla
        if (preso != null && Mouse.current != null && !Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasReleasedThisFrame) { preso = null; trascinando = false; }
    }

    void Muovi(int dx, int dy)
    {
        if (zona == Zona.Caselle && casella >= 4)
        {
            // fila delle caselle degli incantesimi (Stregone)
            int k = casella - 4, quante = CaselleIncantesimi;
            if (dy < 0) { casella = k < quante / 2 ? 2 : 3; return; }
            if (dx > 0 && k == quante - 1 && visibili.Count > 0)
            {
                zona = Zona.Zaino;
                indiceZaino = Mathf.Min(visibili.Count - 1, (rigaIniziale + RigheVisibili - 1) * Colonne);
                return;
            }
            if (dx != 0) casella = 4 + Mathf.Clamp(k + dx, 0, quante - 1);
            return;
        }
        if (zona == Zona.Caselle)
        {
            int col = casella % 2, riga = casella / 2;
            if (dy > 0 && riga == 1 && CaselleIncantesimi > 0)
            {
                casella = 4 + (col == 0 ? 0 : CaselleIncantesimi - 1);
                return;
            }
            if (dx > 0 && col == 1 && visibili.Count > 0)
            {
                zona = Zona.Zaino;
                indiceZaino = Mathf.Min(visibili.Count - 1, (rigaIniziale + riga) * Colonne);
                return;
            }
            if (dx != 0) col = Mathf.Clamp(col + dx, 0, 1);
            if (dy != 0) riga = Mathf.Clamp(riga + dy, 0, 1);
            casella = riga * 2 + col;
            return;
        }

        int n = visibili.Count;
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
        // trascinamento: parte dopo qualche pixel di movimento con il tasto premuto, finisce quando si lascia
        if (preso != null && e.type == EventType.MouseDrag && !trascinando && (e.mousePosition - puntoPresa).sqrMagnitude > 64f)
        {
            trascinando = true;
            casellaAccesa = -1;
        }
        if (preso != null && e.type == EventType.MouseUp && e.button == 0)
        {
            if (trascinando) Lascia(e.mousePosition);
            preso = null;
            trascinando = false;
            e.Use();
        }
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
        AggiornaVisibili();
        if (DisegnaSchede(comparsa, e)) return;
        if (DisegnaZaino(comparsa, e, mouseMosso)) return;
        DisegnaDettaglio(comparsa);

        // l'oggetto trascinato segue il mouse
        if (trascinando && preso != null)
        {
            var m = e.mousePosition;
            GraficaMenu.Alone(new Rect(m.x - 70f, m.y - 70f, 140f, 140f), new Color(1f, 0.5f, 0.2f, 0.25f));
            GraficaMenu.Icona(new Rect(m.x - 40f, m.y - 40f, 80f, 80f), preso, 0.9f);
        }
        GraficaMenu.Scritta(new Rect(0, 1004, L, 28), Lingua.T("inv.aiuto_mouse"), GraficaMenu.Piccolo, GraficaMenu.Spento, 0.7f * comparsa);

        // Avviso (oggetto che questa classe non usa) al posto della riga dei comandi, per qualche secondo.
        if (Time.unscaledTime < avvisoFino && !string.IsNullOrEmpty(avviso))
            GraficaMenu.Scritta(new Rect(0, 1036, L, 34), avviso, GraficaMenu.Piccolo, GraficaMenu.Selezione, Mathf.Clamp01(avvisoFino - Time.unscaledTime));
        else
            GraficaMenu.Scritta(new Rect(0, 1040, L, 30), Lingua.T("inv.aiuto"), GraficaMenu.Piccolo, GraficaMenu.Spento, 0.8f * comparsa);
    }

    // Clic singolo sceglie, doppio clic conferma. Restituisce true se ha confermato.
    // Il clic su una casella la accende: il clic dopo su un oggetto adatto dello zaino lo mette lì.
    // Tenendo premuto e muovendo il mouse parte il trascinamento.
    bool Clic(Rect area, Zona z, int indice, Event e, bool mouseMosso)
    {
        if (!area.Contains(e.mousePosition)) return false;
        if (mouseMosso && !trascinando) { zona = z; if (z == Zona.Caselle) casella = indice; else indiceZaino = indice; }
        if (e.type != EventType.MouseDown || e.button != 0) return false;
        e.Use();
        zona = z;
        if (z == Zona.Caselle) casella = indice; else indiceZaino = indice;

        var oggetto = z == Zona.Caselle ? OggettoInCasella(indice) : (indice < visibili.Count ? visibili[indice] : null);
        if (z == Zona.Zaino && casellaAccesa >= 0 && Accetta(casellaAccesa, oggetto))
        {
            MettiInCasella(casellaAccesa, oggetto);
            casellaAccesa = -1;
            ultimoIndiceClic = -1;
            return true;
        }
        if (z == Zona.Caselle) casellaAccesa = indice;
        else casellaAccesa = -1;
        if (oggetto != null)
        {
            preso = oggetto;
            presoDaCasella = z == Zona.Caselle;
            presoIndice = indice;
            puntoPresa = e.mousePosition;
            trascinando = false;
        }
        int chiave = (z == Zona.Caselle ? 0 : 1000) + indice;
        bool doppio = chiave == ultimoIndiceClic && Time.unscaledTime - ultimoClic < 0.35f;
        ultimoIndiceClic = chiave;
        ultimoClic = Time.unscaledTime;
        if (!doppio) return false;
        Conferma();
        ultimoIndiceClic = -1;
        casellaAccesa = -1;
        preso = null;
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
            rettCaselle[i] = r;
            if (Clic(r, Zona.Caselle, i, e, mouseMosso)) return true;
            bool scelta = (zona == Zona.Caselle && casella == i) || casellaAccesa == i;
            AloneDestinazione(r, i, alfa);
            if (scelta) GraficaMenu.Alone(new Rect(r.x - 50f, r.y - 50f, r.width + 100f, r.height + 100f), new Color(1f, 0.5f, 0.2f, 0.18f * alfa));
            GraficaMenu.Cornice(r, alfa, scelta);
            var oggetto = OggettoInCasella(i);
            GraficaMenu.Scritta(new Rect(r.x - 40f, r.y - 40f, r.width + 80f, 30f), Lingua.T(ChiaveCasella(i)).ToUpperInvariant(), GraficaMenu.Etichetta,
                scelta ? GraficaMenu.Selezione : GraficaMenu.Bronzo, alfa);
            if (oggetto != null) GraficaMenu.Icona(new Rect(r.x + 30f, r.y + 30f, 80f, 80f), oggetto, alfa);
            else GraficaMenu.Scritta(r, "—", GraficaMenu.Voce, GraficaMenu.Spento, alfa);
            GraficaMenu.Scritta(new Rect(r.x - 70f, r.yMax + 6f, r.width + 140f, 30f), oggetto != null ? oggetto.Nome : Lingua.T("inv.vuoto"),
                GraficaMenu.Didascalia, oggetto != null ? GraficaMenu.Testo : GraficaMenu.Spento, alfa);
        }

        // Stregone: la fila delle caselle degli incantesimi (tasti 1-6 in partita), sotto le 4 caselle.
        int quante = CaselleIncantesimi;
        for (int k = 0; k < quante; k++)
        {
            int indice = 4 + k;
            var r = new Rect(cx - quante * 42f + k * 84f + 7f, 826f, 70f, 70f);
            rettCaselle[indice] = r;
            if (Clic(r, Zona.Caselle, indice, e, mouseMosso)) return true;
            bool scelta = (zona == Zona.Caselle && casella == indice) || casellaAccesa == indice;
            AloneDestinazione(r, indice, alfa);
            if (scelta) GraficaMenu.Alone(new Rect(r.x - 30f, r.y - 30f, r.width + 60f, r.height + 60f), new Color(1f, 0.5f, 0.2f, 0.2f * alfa));
            GraficaMenu.Riempi(r, new Color(0.055f, 0.055f, 0.07f, 0.92f * alfa));
            GraficaMenu.Bordo(r, scelta ? 3f : 1f, GraficaMenu.Con(scelta ? GraficaMenu.Selezione : GraficaMenu.Bronzo, (scelta ? 1f : 0.6f) * alfa));
            var inc = equipaggiamento.Incantesimo(k);
            if (inc != null) GraficaMenu.Icona(new Rect(r.x + 10f, r.y + 10f, 50f, 50f), inc, alfa);
            var numero = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.UpperLeft, fontSize = 18 };
            GraficaMenu.Scritta(new Rect(r.x + 5f, r.y + 2f, 30f, 22f), (k + 1).ToString(), numero, scelta ? GraficaMenu.Selezione : GraficaMenu.Spento, alfa);
        }
        if (quante > 0)
            GraficaMenu.Scritta(new Rect(cx - 200f, 798f, 400f, 26f), Lingua.T("inv.incantesimi").ToUpperInvariant(), GraficaMenu.Etichetta,
                zona == Zona.Caselle && casella >= 4 ? GraficaMenu.Selezione : GraficaMenu.Bronzo, alfa);

        // statistiche totali
        var arma = equipaggiamento != null ? equipaggiamento.Arma : null;
        // lo Stregone vede il mana al posto del danno dell'arma (non ha armi)
        string secondaEtichetta = Lingua.T(Stregone ? "stat.mana" : "stat.armatura");
        string secondoValore = Stregone ? Mathf.RoundToInt(giocatore.ManaMassimo).ToString()
            : statistiche != null ? Mathf.RoundToInt(statistiche.Armatura).ToString() : "—";
        string terzaEtichetta = Lingua.T(Stregone ? "stat.armatura" : "stat.danno");
        string terzoValore = Stregone ? (statistiche != null ? Mathf.RoundToInt(statistiche.Armatura).ToString() : "—")
            : arma != null ? Mathf.RoundToInt(arma.danno).ToString() : "—";
        string[,] righe =
        {
            { Lingua.T("stat.vita"), Mathf.CeilToInt(giocatore.VitaMassima).ToString() },
            { secondaEtichetta, secondoValore },
            { terzaEtichetta, terzoValore },
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

    // Mentre si trascina, le caselle dove l'oggetto può andare pulsano; la casella accesa pulsa anche lei.
    void AloneDestinazione(Rect r, int i, float alfa)
    {
        bool destinazione = trascinando && preso != null
            && (presoDaCasella ? presoIndice >= 4 && i >= 4 && i != presoIndice : Accetta(i, preso));
        if (!destinazione && casellaAccesa != i) return;
        float pulsa = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f);
        GraficaMenu.Alone(new Rect(r.x - 40f, r.y - 40f, r.width + 80f, r.height + 80f), new Color(1f, 0.55f, 0.2f, (0.12f + 0.18f * pulsa) * alfa));
    }

    // Le schede sopra lo zaino: nome e quanti oggetti contiene; la scelta è color fiamma. Q a sinistra, R a destra.
    bool DisegnaSchede(float alfa, Event e)
    {
        const float x0 = 860f, y = 160f, h = 34f, larghezza = 900f;
        float w = larghezza / NumeroSchede;
        for (int i = 0; i < NumeroSchede; i++)
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
            string testo = Lingua.T(ChiaveScheda(i)) + "  " + QuantiNellaScheda(i);
            var stile = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
            GraficaMenu.Scritta(r, testo, stile, scelta ? GraficaMenu.Selezione : GraficaMenu.Spento, alfa);
        }
        // i tasti per cambiare scheda, dentro due rombi ai lati
        var tq = new Vector2(x0 - 22f, y + h * 0.5f);
        var tr = new Vector2(x0 + larghezza + 22f, y + h * 0.5f);
        GraficaMenu.Rombo(tq, 30f, GraficaMenu.Con(GraficaMenu.Bronzo, alfa));
        GraficaMenu.Rombo(tr, 30f, GraficaMenu.Con(GraficaMenu.Bronzo, alfa));
        var tasto = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleCenter };
        GraficaMenu.Scritta(new Rect(tq.x - 20f, tq.y - 14f, 40f, 28f), "Q", tasto, new Color(0.05f, 0.05f, 0.06f), alfa);
        GraficaMenu.Scritta(new Rect(tr.x - 20f, tr.y - 14f, 40f, 28f), "R", tasto, new Color(0.05f, 0.05f, 0.06f), alfa);
        return false;
    }

    bool DisegnaZaino(float alfa, Event e, bool mouseMosso)
    {
        var Z = new Rect(820f, 200f, 980f, 490f);
        rettZaino = Z;
        // un oggetto preso da una casella si può lasciare nello zaino per toglierlo
        bool lasciaQui = trascinando && preso != null && presoDaCasella;
        GraficaMenu.Cornice(Z, alfa, lasciaQui && Z.Contains(e.mousePosition));
        var et = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft, fontSize = 24 };
        GraficaMenu.Scritta(new Rect(Z.x + 40f, Z.y + 16f, 400f, 34f), Lingua.T("inv.zaino").ToUpperInvariant(), et, GraficaMenu.Bronzo, alfa);

        int n = visibili.Count;
        int righeTotali = Mathf.Max(1, Mathf.CeilToInt(n / (float)Colonne));
        int rigaScelta = indiceZaino / Colonne;
        // Rotellina del mouse sopra lo zaino: scorre di una riga alla volta.
        if (e.type == EventType.ScrollWheel && Z.Contains(e.mousePosition) && righeTotali > RigheVisibili)
        {
            int passo = e.delta.y > 0f ? 1 : -1;
            int nuovaRiga = Mathf.Clamp(rigaIniziale + passo, 0, righeTotali - RigheVisibili);
            int spostamento = nuovaRiga - rigaIniziale;
            rigaIniziale = nuovaRiga;
            if (zona == Zona.Zaino && spostamento != 0)
            {
                indiceZaino = Mathf.Clamp(indiceZaino + spostamento * Colonne, 0, n - 1);
                rigaScelta = indiceZaino / Colonne;
            }
            e.Use();
        }
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
                var r = new Rect(Z.x + 30f + col * 102f, Z.y + 70f + riga * 100f, 90f, 90f);
                bool pieno = i < n;
                bool scelta = pieno && zona == Zona.Zaino && i == indiceZaino;
                if (pieno && Clic(r, Zona.Zaino, i, e, mouseMosso)) return true;
                if (scelta) GraficaMenu.Alone(new Rect(r.x - 30f, r.y - 30f, r.width + 60f, r.height + 60f), new Color(1f, 0.5f, 0.2f, 0.22f * alfa));
                GraficaMenu.Riempi(r, new Color(0.055f, 0.055f, 0.07f, 0.9f * alfa));
                GraficaMenu.Bordo(r, scelta ? 3f : 1f, GraficaMenu.Con(scelta ? GraficaMenu.Selezione : GraficaMenu.Bronzo, (scelta ? 1f : 0.55f) * alfa));
                if (pieno) GraficaMenu.Icona(new Rect(r.x + 10f, r.y + 10f, 70f, 70f), visibili[i], trascinando && !presoDaCasella && visibili[i] == preso ? alfa * 0.3f : alfa);
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
            ? (indiceZaino >= 0 && indiceZaino < visibili.Count ? visibili[indiceZaino] : null)
            : OggettoInCasella(casella);
        var D = new Rect(820f, 720f, 980f, 280f);
        GraficaMenu.Cornice(D, alfa, oggetto != null);
        if (oggetto == null) return;

        var nome = new GUIStyle(GraficaMenu.NomeClasse) { alignment = TextAnchor.MiddleLeft };
        GraficaMenu.Scritta(new Rect(D.x + 40f, D.y + 22f, D.width * 0.6f, 44f), oggetto.Nome.ToUpperInvariant(), nome, GraficaMenu.Selezione, alfa);
        var tipo = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleRight };
        GraficaMenu.Scritta(new Rect(D.x + D.width * 0.45f, D.y + 22f, D.width * 0.55f - 40f, 44f), Tipo(oggetto) + GraficaMenu.Separatore + NomeClasse(oggetto.classe), tipo, GraficaMenu.Spento, alfa);
        GraficaMenu.Scritta(new Rect(D.x + 40f, D.y + 70f, D.width - 80f, 64f), oggetto.Descrizione, GraficaMenu.TestoSinistra, GraficaMenu.Testo, alfa);

        // Bastoni, libri, vesti dello Stregone e amuleti: pro (verdi) e contro (rossi) al posto dei numeri.
        var voci = Voci(oggetto);
        if (voci != null)
        {
            DisegnaVoci(voci, new Rect(D.x + 40f, D.y + 146f, D.width - 80f, 120f), alfa);
            return;
        }

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

            // si confrontano solo righe uguali (un bastone e una spada stanno nella stessa casella ma hanno righe diverse)
            if (righeAddosso == null || !riga.confrontabile || i >= righeAddosso.Count || righeAddosso[i].etichetta != riga.etichetta) continue;
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
            case DatiIncantesimo inc:
                // incantesimi dello Stregone: danno, mana, carica e attesa
                Aggiungi("stat.danno", inc.danno > 0f ? Formatta(inc.danno) : inc.dannoAlSecondo > 0f ? Formatta(inc.dannoAlSecondo) + "/s"
                    : inc.dannoEvocazione > 0f ? Formatta(inc.dannoEvocazione) : "—", inc.danno, inc.danno > 0f, true);
                Aggiungi("stat.costo_mana", Formatta(inc.costoMana), inc.costoMana, true, false);
                Aggiungi("stat.carica", Formatta(inc.carica) + " s", inc.carica, true, false);
                Aggiungi("stat.attesa", Formatta(inc.attesa) + " s", inc.attesa, true, false);
                break;
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
            case DatiArmaDistanza d:
                Aggiungi("stat.danno", Formatta(d.danno), d.danno, true, true);
                Aggiungi("stat.portata", Formatta(d.portata) + " m", d.portata, true, true);
                Aggiungi("stat.ricarica", Formatta(d.ricarica) + " s", d.ricarica, true, false);
                Aggiungi("stat.costo", Formatta(d.costoTiro), d.costoTiro, true, false);
                break;
            case DatiArmatura b:
                Aggiungi("stat.armatura", Formatta(b.armatura), b.armatura, true, true);
                if (b.furtivita > 0f) Aggiungi("stat.furtivita", "+" + Formatta(b.furtivita) + "%", b.furtivita, true, true);
                else Aggiungi("stat.peso", Lingua.T(ChiavePeso(b.peso, "stat.")), 0f, false, true);
                Aggiungi("stat.schivata", "+" + Formatta(b.costoSchivataExtra), b.costoSchivataExtra, true, false);
                Aggiungi("stat.movimento", "-" + Mathf.RoundToInt((1f - b.moltiplicatoreVelocita) * 100f) + "%", (1f - b.moltiplicatoreVelocita) * 100f, true, false);
                break;
            case DatiAmuleto c:
                Aggiungi("stat.tipo", Lingua.T(c.tipo == DatiAmuleto.Tipo.Magico ? "stat.magico" : "stat.arcano"), 0f, false, true);
                break;
        }
        return righe;
    }

    // Le voci "pro e contro" di un oggetto, oppure null se l'oggetto si mostra con i numeri (armi, scudi, armature
    // di Guerriero e Ladro, incantesimi).
    static List<VoceEffetto> Voci(DatiOggetto o)
    {
        var voci = new List<VoceEffetto>();
        switch (o)
        {
            case DatiBastone st:
                if (st.dueMani) voci.Add(new VoceEffetto { chiave = "mod.due_mani", testoValore = "", positivo = false });
                st.magia.Descrivi(voci);
                st.bonus.Descrivi(voci);
                if (voci.Count == 0) voci.Add(new VoceEffetto { chiave = "mod.nessuno", testoValore = "", positivo = true });
                return voci;
            case DatiLibro l:
                new Statistiche.Modificatore { manaMassimo = l.manaMassimo, recuperoMana = l.recuperoMana }.Descrivi(voci);
                l.magia.Descrivi(voci);
                Peso(voci, l.costoSchivataExtra, l.moltiplicatoreVelocita);
                return voci;
            case DatiArmatura v when v.classe == ClasseGiocatore.Stregone:
                voci.Add(new VoceEffetto { chiave = "stat.armatura", testoValore = Formatta(v.armatura), positivo = true });
                v.Modificatore().Descrivi(voci);
                voci.RemoveAll(x => x.chiave == "mod.armatura");   // già scritta qui sopra
                v.magia.Descrivi(voci);
                if (v.manaPersoPerDannoPercento > 0f)
                    voci.Add(new VoceEffetto { chiave = "mod.mana_per_danno", testoValore = "-" + Formatta(v.manaPersoPerDannoPercento) + "%", positivo = false });
                if (v.manaPerSchivata > 0f)
                    voci.Add(new VoceEffetto { chiave = "mod.mana_per_schivata", testoValore = "-" + Formatta(v.manaPerSchivata), positivo = false });
                Peso(voci, v.costoSchivataExtra, v.moltiplicatoreVelocita);
                return voci;
            case DatiAmuleto c:
                voci.Add(new VoceEffetto { chiave = c.tipo == DatiAmuleto.Tipo.Magico ? "stat.magico" : "stat.arcano", testoValore = "", positivo = true, neutro = true });
                if (c.effetto != DatiAmuleto.Effetto.Nessuno) voci.Add(EffettoArcano(c));
                c.Modificatore().Descrivi(voci);
                c.magia.Descrivi(voci);
                return voci;
        }
        return null;
    }

    static void Peso(List<VoceEffetto> voci, float schivata, float corsa)
    {
        if (Mathf.Abs(schivata) > 0.001f)
            voci.Add(new VoceEffetto { chiave = "stat.schivata", testoValore = (schivata > 0f ? "+" : "") + Formatta(schivata), positivo = schivata < 0f });
        if (corsa < 0.999f)
            voci.Add(new VoceEffetto { chiave = "stat.movimento", testoValore = "-" + Mathf.RoundToInt((1f - corsa) * 100f) + "%", positivo = false });
    }

    static VoceEffetto EffettoArcano(DatiAmuleto c)
    {
        string v = Formatta(c.valore);
        switch (c.effetto)
        {
            case DatiAmuleto.Effetto.VitaPerUccisione: return new VoceEffetto { chiave = "mod.vita_per_uccisione", testoValore = "+" + v, positivo = true };
            case DatiAmuleto.Effetto.RecuperoResistenza: return new VoceEffetto { chiave = "mod.recupero_resistenza", testoValore = "+" + v + "%", positivo = true };
            case DatiAmuleto.Effetto.RubaVita: return new VoceEffetto { chiave = "mod.ruba_vita", testoValore = v + "%", positivo = true };
            case DatiAmuleto.Effetto.SvanireNellOmbra: return new VoceEffetto { chiave = "mod.svanire", testoValore = v + " s", positivo = true };
            case DatiAmuleto.Effetto.ManaPerUccisione: return new VoceEffetto { chiave = "mod.mana_per_uccisione", testoValore = "+" + v, positivo = true };
            default: return new VoceEffetto { chiave = "mod.ruba_mana", testoValore = v + "%", positivo = true };
        }
    }

    // Le voci una dopo l'altra, andando a capo quando la riga è piena (al massimo 4 righe).
    static void DisegnaVoci(List<VoceEffetto> voci, Rect area, float alfa)
    {
        var stile = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleLeft, wordWrap = false };
        var verde = new Color(0.47f, 0.67f, 0.35f);
        var rosso = new Color(0.75f, 0.27f, 0.24f);
        float x = area.x, y = area.y, riga = 30f;
        foreach (var voce in voci)
        {
            string testo = (voce.scuola.HasValue ? NomeScuola(voce.scuola.Value) + GraficaMenu.Separatore : "") + Lingua.T(voce.chiave)
                + (string.IsNullOrEmpty(voce.testoValore) ? "" : " " + voce.testoValore);
            float w = stile.CalcSize(new GUIContent(testo)).x + 34f;
            if (x + w > area.xMax && x > area.x)
            {
                x = area.x;
                y += riga;
                if (y + riga > area.yMax + 4f) break;
            }
            Color colore = voce.neutro ? GraficaMenu.Bronzo : voce.positivo ? verde : rosso;
            GraficaMenu.Scritta(new Rect(x, y, w, riga), testo, stile, colore, alfa);
            x += w;
        }
    }

    static string NomeScuola(ScuolaMagia scuola)
    {
        switch (scuola)
        {
            case ScuolaMagia.Brace: return Lingua.T("scuola.brace");
            case ScuolaMagia.LagoNero: return Lingua.T("scuola.lago_nero");
            case ScuolaMagia.Ombra: return Lingua.T("scuola.ombra");
            default: return Lingua.T("scuola.evocazione");
        }
    }

    static string Tipo(DatiOggetto o)
    {
        switch (o)
        {
            case DatiArma a:
                switch (a.tipo)
                {
                    case DatiArma.Tipo.Spada: return Lingua.T("tipo.spada");
                    case DatiArma.Tipo.Spadone: return Lingua.T("tipo.spadone");
                    case DatiArma.Tipo.Ascia: return Lingua.T("tipo.ascia");
                    case DatiArma.Tipo.Pugnale: return Lingua.T("tipo.pugnale");
                    case DatiArma.Tipo.Stiletto: return Lingua.T("tipo.stiletto");
                    case DatiArma.Tipo.DoppiPugnali: return Lingua.T("tipo.doppi_pugnali");
                    default: return Lingua.T("tipo.mazza");
                }
            case DatiArmaDistanza d:
                return Lingua.T(d.tipo == DatiArmaDistanza.Tipo.ArcoCorto ? "tipo.arco_corto" : d.tipo == DatiArmaDistanza.Tipo.ArcoLungo ? "tipo.arco_lungo" : "tipo.balestra");
            case DatiScudo s:
                return Lingua.T(s.taglia == DatiScudo.Taglia.Grande ? "tipo.scudo_grande" : s.taglia == DatiScudo.Taglia.Medio ? "tipo.scudo_medio" : "tipo.scudo_piccolo");
            case DatiLibro _:
                return Lingua.T("tipo.libro");
            case DatiBastone st:
                return Lingua.T(st.tipo == DatiBastone.Tipo.Verga ? "tipo.verga" : st.dueMani ? "tipo.bastone_due_mani" : "tipo.bastone");
            case DatiIncantesimo inc:
                return Lingua.T("tipo.incantesimo") + GraficaMenu.Separatore + NomeScuola(inc.scuola);
            case DatiArmatura b:
                return Lingua.T(ChiavePeso(b.peso, b.classe == ClasseGiocatore.Stregone ? "tipo.veste_" : "tipo.armatura_"));
            case DatiAmuleto c:
                return Lingua.T(c.tipo == DatiAmuleto.Tipo.Magico ? "tipo.amuleto_magico" : "tipo.amuleto_arcano");
        }
        return "";
    }

    // Chiave del peso dell'armatura: prefisso "stat." (Leggera, Media...) o "tipo.armatura_" (Armatura leggera...).
    static string ChiavePeso(DatiArmatura.Peso peso, string prefisso)
    {
        switch (peso)
        {
            case DatiArmatura.Peso.Leggera: return prefisso + "leggera";
            case DatiArmatura.Peso.Media: return prefisso + "media";
            case DatiArmatura.Peso.Cuoio: return prefisso + "cuoio";
            case DatiArmatura.Peso.Ombra: return prefisso + "ombra";
            default: return prefisso + "pesante";
        }
    }

    static string NomeClasse(ClasseGiocatore classe) =>
        Lingua.T(classe == ClasseGiocatore.Guerriero ? "classe.guerriero" : classe == ClasseGiocatore.Ladro ? "classe.ladro" : "classe.stregone");
}
