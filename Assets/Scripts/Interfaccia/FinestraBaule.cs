using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Finestra del baule (richiesta di Lorenzo del 15 ottobre): aprendo un baule non si raccoglie più tutto in automatico,
// ma si vede cosa c'è dentro e lo si sposta nello zaino.
//   - a sinistra il contenuto del baule, a destra lo zaino del giocatore;
//   - si trascina un oggetto con il mouse da una parte all'altra (anche dallo zaino al baule, per lasciarci qualcosa);
//   - doppio clic su un oggetto lo sposta dall'altra parte;
//   - "Prendi tutto" (o R) mette nello zaino tutto il contenuto;
//   - tastiera e pad: frecce o levetta per scegliere (oltre il bordo si passa all'altra parte), E, Invio o A per
//     spostare, R o Y per prendere tutto, Esc, Tab o B per chiudere.
// Le pozioni rispettano il limite dello zaino (Zaino.MassimoConsumabili). Gli oggetti delle missioni non si
// lasciano nel baule, così una quest non si blocca.
// Mentre è aperta InventarioGioco.Aperto è vero: gli altri tasti (E, F, Tab...) non fanno niente e il personaggio
// e la camera sono fermi. Si chiude da sola se il giocatore si allontana o muore.
// Come montarlo: non si monta. La apre il Baule (Assets/Scripts/Livelli/Baule.cs) con FinestraBaule.Apri(...).
// Co-op: la finestra è solo di chi ha aperto il baule.
public class FinestraBaule : MonoBehaviour
{
    public static bool Aperto => corrente != null && corrente.aperta;
    static FinestraBaule corrente;

    const int Colonne = 6, Righe = 4;
    const float Lato = 100f, Spazio = 10f;
    static readonly Rect PannelloBaule = new Rect(170f, 170f, 720f, 610f);
    static readonly Rect PannelloZaino = new Rect(1030f, 170f, 720f, 610f);
    static readonly Rect PulsanteTutto = new Rect(170f, 940f, 380f, 70f);

    Baule baule;
    List<DatiOggetto> contenuto;
    GiocatoreControllo giocatore;
    Zaino zaino;
    bool aperta;
    float tempoApertura;
    bool riattivaAlProssimoFotogramma;
    readonly List<Behaviour> spenti = new List<Behaviour>();
    CursorLockMode cursorePrima;
    bool cursoreVisibilePrima;
    readonly ElencoMenu comandi = new ElencoMenu();   // usato solo per leggere frecce, conferma e indietro

    bool latoBaule = true;           // dove sta la scelta con tastiera e pad
    int scelta;
    int primaRigaBaule, primaRigaZaino;
    DatiOggetto sottoIlMouse;

    // Trascinamento con il mouse.
    DatiOggetto preso;
    bool presoDalBaule;
    Vector2 puntoPresa;
    bool trascinando;
    float ultimoClic;
    DatiOggetto ultimoOggettoClic;

    string avviso;
    float avvisoFino;

    // Apre la finestra su "contenuto" (la lista del baule: la finestra la cambia direttamente).
    public static void Apri(Baule baule, List<DatiOggetto> contenuto, GiocatoreControllo giocatore)
    {
        if (Aperto || baule == null || contenuto == null || giocatore == null) return;
        if (corrente == null)
        {
            var o = new GameObject("Finestra del baule");
            corrente = o.AddComponent<FinestraBaule>();
        }
        corrente.Prepara(baule, contenuto, giocatore);
    }

    void Prepara(Baule b, List<DatiOggetto> lista, GiocatoreControllo g)
    {
        baule = b;
        contenuto = lista;
        giocatore = g;
        zaino = Zaino.Di(g);
        cursorePrima = Cursor.lockState;
        cursoreVisibilePrima = Cursor.visible;
        spenti.Clear();
        foreach (var x in FindObjectsByType<GiocatoreControllo>(FindObjectsSortMode.None)) Spegni(x);
        foreach (var x in FindObjectsByType<CameraTerzaPersona>(FindObjectsSortMode.None)) Spegni(x);
        foreach (var x in FindObjectsByType<AggancioBersaglio>(FindObjectsSortMode.None)) Spegni(x);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        latoBaule = contenuto.Count > 0;
        scelta = 0;
        primaRigaBaule = primaRigaZaino = 0;
        preso = null;
        trascinando = false;
        avviso = null;
        tempoApertura = Time.unscaledTime;
        aperta = true;
    }

    void OnDestroy()
    {
        if (corrente == this) corrente = null;
    }

    void Spegni(Behaviour b)
    {
        if (b == null || !b.enabled) return;
        b.enabled = false;
        spenti.Add(b);
    }

    void Chiudi()
    {
        if (!aperta) return;
        aperta = false;
        preso = null;
        trascinando = false;
        InventarioGioco.FotogrammaChiusura = Time.frameCount;   // lo stesso Esc non deve aprire il menu di pausa
        riattivaAlProssimoFotogramma = true;                    // così lo stesso tasto non arriva anche al personaggio
        if (baule != null) baule.FinestraChiusa();
    }

    void Riattiva()
    {
        foreach (var b in spenti)
            if (b != null) b.enabled = true;
        spenti.Clear();
        Cursor.lockState = cursorePrima;
        Cursor.visible = cursoreVisibilePrima;
    }

    // ---------- spostare gli oggetti ----------

    IReadOnlyList<DatiOggetto> Lista(bool dalBaule) => dalBaule ? (IReadOnlyList<DatiOggetto>)contenuto : zaino.Oggetti;

    // Sposta un oggetto dall'altra parte. Vero se ci è riuscito.
    bool Sposta(DatiOggetto o, bool dalBaule)
    {
        if (o == null || zaino == null) return false;
        if (dalBaule)
        {
            if (!contenuto.Contains(o)) return false;
            if (o is DatiConsumabile && !zaino.PostoPerConsumabili(1))
            {
                Avvisa(string.Format(Lingua.T("neg.troppe_pozioni"), zaino.MassimoConsumabili));
                return false;
            }
            contenuto.Remove(o);
            zaino.Aggiungi(o);
            Suoni.Suona(Suono.Raccolta, giocatore.transform.position + Vector3.up, 0.7f);
        }
        else
        {
            if (!zaino.Contiene(o)) return false;
            if (o is DatiOggettoMissione) { Avvisa(Lingua.T("baule.no_missione")); return false; }
            zaino.Togli(o);
            contenuto.Add(o);
            Suoni.Suona(Suono.CambioArma, giocatore.transform.position + Vector3.up, 0.4f, 0.9f);
        }
        if (baule != null) baule.ContenutoCambiato();
        return true;
    }

    void PrendiTutto()
    {
        var copia = new List<DatiOggetto>(contenuto);
        int presi = 0;
        foreach (var o in copia)
            if (Sposta(o, true)) presi++;
        if (presi == 0 && copia.Count == 0) Avvisa(Lingua.T("baule.vuoto"));
        latoBaule = false;
        scelta = 0;
    }

    void Avvisa(string testo)
    {
        avviso = testo;
        avvisoFino = Time.unscaledTime + 2.5f;
        Suoni.Suona(Suono.Parata, giocatore.transform.position + Vector3.up, 0.3f, 1.4f);
    }

    // ---------- comandi ----------

    void Update()
    {
        if (riattivaAlProssimoFotogramma)
        {
            riattivaAlProssimoFotogramma = false;
            Riattiva();
        }
        if (!aperta) return;
        if (giocatore == null || zaino == null || baule == null || giocatore.StatoAttuale == GiocatoreControllo.Stato.Morto || !baule.Vicino(giocatore))
        {
            Chiudi();
            return;
        }
        if (MenuPausa.InPausa) return;
        // appena aperta non legge i tasti: lo stesso E che ha aperto il baule non deve spostare subito un oggetto
        if (Time.unscaledTime - tempoApertura < 0.25f) return;

        var tastiera = Keyboard.current;
        var pad = Gamepad.current;
        var c = comandi.LeggiComandi();
        if (c.indietro || Comandi.PremutoOra(Azione.Inventario) || (pad != null && pad.selectButton.wasPressedThisFrame)) { Chiudi(); return; }
        if ((tastiera != null && tastiera.rKey.wasPressedThisFrame) || (pad != null && pad.buttonNorth.wasPressedThisFrame)) PrendiTutto();

        int numero = Lista(latoBaule).Count;
        if (c.orizzontale != 0 || c.verticale != 0)
        {
            int colonna = scelta % Colonne, riga = scelta / Colonne;
            colonna += c.orizzontale;
            riga += c.verticale;
            if (colonna < 0 && !latoBaule) { latoBaule = true; colonna = Colonne - 1; }
            else if (colonna >= Colonne && latoBaule) { latoBaule = false; colonna = 0; }
            colonna = Mathf.Clamp(colonna, 0, Colonne - 1);
            numero = Lista(latoBaule).Count;
            scelta = Mathf.Clamp(Mathf.Max(0, riga) * Colonne + colonna, 0, Mathf.Max(0, numero - 1));
            TieniVisibile();
        }
        bool conferma = c.conferma || (tastiera != null && tastiera.eKey.wasPressedThisFrame);
        if (conferma && scelta < numero)
        {
            Sposta(Lista(latoBaule)[scelta], latoBaule);
            scelta = Mathf.Clamp(scelta, 0, Mathf.Max(0, Lista(latoBaule).Count - 1));
        }
        // pulsante lasciato fuori dalla finestra: il trascinamento si annulla
        if (preso != null && Mouse.current != null && !Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasReleasedThisFrame) { preso = null; trascinando = false; }
    }

    void TieniVisibile()
    {
        int riga = scelta / Colonne;
        if (latoBaule)
        {
            if (riga < primaRigaBaule) primaRigaBaule = riga;
            if (riga >= primaRigaBaule + Righe) primaRigaBaule = riga - Righe + 1;
        }
        else
        {
            if (riga < primaRigaZaino) primaRigaZaino = riga;
            if (riga >= primaRigaZaino + Righe) primaRigaZaino = riga - Righe + 1;
        }
    }

    // ---------- disegno ----------

    static Rect Cella(Rect pannello, int i, int primaRiga)
    {
        int riga = i / Colonne - primaRiga, colonna = i % Colonne;
        float larghezzaGriglia = Colonne * Lato + (Colonne - 1) * Spazio;
        float x0 = pannello.x + (pannello.width - larghezzaGriglia) * 0.5f;
        return new Rect(x0 + colonna * (Lato + Spazio), pannello.y + 90f + riga * (Lato + Spazio), Lato, Lato);
    }

    void OnGUI()
    {
        if (!aperta || giocatore == null || zaino == null) return;
        GUI.depth = -50;
        GraficaMenu.PreparaStili();
        float alfa = Mathf.Clamp01((Time.unscaledTime - tempoApertura) / 0.2f);
        GraficaMenu.Atmosfera(0.6f);
        GraficaMenu.FoglioVirtuale();
        var e = Event.current;
        float L = GraficaMenu.Larghezza;

        string titolo = Lingua.T("baule.titolo").ToUpperInvariant();
        if (Lingua.Indice < 6) titolo = GraficaMenu.Spaziato(titolo);
        GraficaMenu.Scritta(new Rect(0f, 40f, L, 80f), titolo, GraficaMenu.Intestazione, GraficaMenu.Testo, alfa);
        GraficaMenu.Divisore(L * 0.5f, 128f, 640f, alfa);

        sottoIlMouse = null;
        DisegnaPannello(PannelloBaule, true, Lingua.T("baule.contenuto"), alfa, e);
        DisegnaPannello(PannelloZaino, false, Lingua.T("inv.zaino"), alfa, e);

        // freccia fra i due pannelli
        float centroX = (PannelloBaule.xMax + PannelloZaino.x) * 0.5f;
        GraficaMenu.Rombo(new Vector2(centroX, PannelloBaule.center.y - 24f), 16f, GraficaMenu.Con(GraficaMenu.Bronzo, alfa));
        GraficaMenu.Rombo(new Vector2(centroX, PannelloBaule.center.y + 24f), 16f, GraficaMenu.Con(GraficaMenu.Bronzo, alfa));

        // dettaglio dell'oggetto sotto il mouse (o scelto con tastiera e pad)
        DatiOggetto dettaglio = sottoIlMouse;
        if (dettaglio == null && scelta < Lista(latoBaule).Count) dettaglio = Lista(latoBaule)[scelta];
        var riquadroDettaglio = new Rect(170f, 800f, 1580f, 120f);
        GraficaMenu.Riempi(riquadroDettaglio, GraficaMenu.Con(GraficaMenu.Riquadro, alfa));
        GraficaMenu.Bordo(riquadroDettaglio, 1f, GraficaMenu.Con(GraficaMenu.Bronzo, 0.6f * alfa));
        if (dettaglio != null)
        {
            GraficaMenu.Scritta(new Rect(200f, 808f, 1520f, 44f), dettaglio.Nome, GraficaMenu.VoceSinistra, GraficaMenu.Selezione, alfa);
            GraficaMenu.Scritta(new Rect(200f, 852f, 1520f, 60f), dettaglio.Descrizione, GraficaMenu.TestoSinistra, GraficaMenu.Testo, alfa);
        }

        // pulsante "Prendi tutto"
        bool sopraTutto = PulsanteTutto.Contains(e.mousePosition);
        GraficaMenu.Cornice(PulsanteTutto, alfa, sopraTutto && contenuto.Count > 0);
        GraficaMenu.Scritta(PulsanteTutto, Lingua.T("baule.prendi_tutto") + "  (R)", GraficaMenu.Voce, contenuto.Count > 0 ? GraficaMenu.Testo : GraficaMenu.Spento, alfa);
        if (sopraTutto && e.type == EventType.MouseDown && e.button == 0) { PrendiTutto(); e.Use(); }

        // istruzioni
        GraficaMenu.Scritta(new Rect(600f, 940f, 1150f, 70f), Lingua.T("baule.istruzioni"), GraficaMenu.Didascalia, GraficaMenu.Spento, alfa);

        if (!string.IsNullOrEmpty(avviso) && Time.unscaledTime < avvisoFino)
            GraficaMenu.Scritta(new Rect(0f, 1015f, L, 50f), avviso, GraficaMenu.Voce, new Color(0.9f, 0.45f, 0.35f), alfa);

        GestisciMouse(e);

        // l'oggetto trascinato segue il mouse
        if (trascinando && preso != null)
        {
            var r = new Rect(e.mousePosition.x - 40f, e.mousePosition.y - 40f, 80f, 80f);
            GraficaMenu.Riempi(r, GraficaMenu.Con(GraficaMenu.Riquadro, 0.8f));
            GraficaMenu.Icona(r, preso, 0.9f);
        }
    }

    void DisegnaPannello(Rect p, bool delBaule, string titolo, float alfa, Event e)
    {
        bool destinazione = trascinando && preso != null && presoDalBaule != delBaule;
        GraficaMenu.Cornice(p, alfa, destinazione && p.Contains(e.mousePosition));
        GraficaMenu.Scritta(new Rect(p.x, p.y + 20f, p.width, 50f), titolo.ToUpperInvariant(), GraficaMenu.Voce, GraficaMenu.Testo, alfa);

        var lista = Lista(delBaule);
        int primaRiga = delBaule ? primaRigaBaule : primaRigaZaino;
        int righeTotali = Mathf.Max(1, (lista.Count + Colonne - 1) / Colonne);
        // rotellina sopra il pannello: scorre le righe
        if (e.type == EventType.ScrollWheel && p.Contains(e.mousePosition))
        {
            primaRiga = Mathf.Clamp(primaRiga + (e.delta.y > 0 ? 1 : -1), 0, Mathf.Max(0, righeTotali - Righe));
            if (delBaule) primaRigaBaule = primaRiga; else primaRigaZaino = primaRiga;
            e.Use();
        }

        for (int i = primaRiga * Colonne; i < (primaRiga + Righe) * Colonne; i++)
        {
            Rect r = Cella(p, i, primaRiga);
            bool pieno = i < lista.Count;
            bool scelto = latoBaule == delBaule && scelta == i;
            bool sopra = r.Contains(e.mousePosition);
            GraficaMenu.Riempi(r, GraficaMenu.Con(new Color(0.06f, 0.055f, 0.06f, 0.9f), alfa));
            GraficaMenu.Bordo(r, scelto || (sopra && pieno) ? 2f : 1f, GraficaMenu.Con(scelto ? GraficaMenu.Selezione : GraficaMenu.Bronzo, (scelto ? 1f : 0.5f) * alfa));
            if (!pieno) continue;
            var o = lista[i];
            bool nascosto = trascinando && preso == o && presoDalBaule == delBaule;
            GraficaMenu.Icona(new Rect(r.x + 10f, r.y + 10f, 80f, 80f), o, nascosto ? alfa * 0.3f : alfa);
            if (sopra) sottoIlMouse = o;

            if (sopra && e.type == EventType.MouseDown && e.button == 0)
            {
                latoBaule = delBaule;
                scelta = i;
                // doppio clic: sposta subito
                if (ultimoOggettoClic == o && Time.unscaledTime - ultimoClic < 0.35f)
                {
                    Sposta(o, delBaule);
                    ultimoOggettoClic = null;
                    preso = null;
                }
                else
                {
                    ultimoOggettoClic = o;
                    ultimoClic = Time.unscaledTime;
                    preso = o;
                    presoDalBaule = delBaule;
                    puntoPresa = e.mousePosition;
                }
                e.Use();
            }
        }

        // barretta di scorrimento se ci sono più righe di quelle visibili
        if (righeTotali > Righe)
        {
            float altezza = Righe * (Lato + Spazio) - Spazio;
            var guida = new Rect(p.xMax - 22f, p.y + 90f, 6f, altezza);
            GraficaMenu.Riempi(guida, GraficaMenu.Con(GraficaMenu.Spento, 0.3f * alfa));
            float parte = (float)Righe / righeTotali;
            float inizio = (float)primaRiga / righeTotali;
            GraficaMenu.Riempi(new Rect(guida.x, guida.y + inizio * altezza, guida.width, parte * altezza), GraficaMenu.Con(GraficaMenu.Bronzo, alfa));
        }

        if (lista.Count == 0)
            GraficaMenu.Scritta(new Rect(p.x, p.y + 260f, p.width, 60f), Lingua.T(delBaule ? "baule.vuoto" : "baule.zaino_vuoto"), GraficaMenu.Didascalia, GraficaMenu.Spento, alfa);
    }

    void GestisciMouse(Event e)
    {
        if (preso == null) return;
        if (e.type == EventType.MouseDrag && !trascinando && (e.mousePosition - puntoPresa).sqrMagnitude > 64f) trascinando = true;
        if (e.type != EventType.MouseUp) return;
        if (trascinando)
        {
            // lasciato sull'altro pannello: si sposta
            Rect altro = presoDalBaule ? PannelloZaino : PannelloBaule;
            if (altro.Contains(e.mousePosition)) Sposta(preso, presoDalBaule);
            scelta = Mathf.Clamp(scelta, 0, Mathf.Max(0, Lista(latoBaule).Count - 1));
        }
        preso = null;
        trascinando = false;
        e.Use();
    }
}
