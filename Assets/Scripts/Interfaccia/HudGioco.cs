using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Interfaccia in partita, nello stile dei menu (disegno approvato in Docs/interfaccia.md):
//   - in alto a sinistra: Vita (con la scia chiara del danno appena subito), Resistenza e, se serve, Mana;
//   - in basso a sinistra: arma (tasto 1) e bastone (tasto 2, se lo hai) dentro due rombi, scudo e amuleto più piccoli;
//     per il Ladro, al posto dello scudo, l'arma a distanza (tasto 2); se l'amuleto ha un'abilità (Ultimo respiro),
//     un rombo con Q che si riempie mentre si ricarica, acceso quando è pronta e viola mentre si è invisibili;
//   - al centro, da morti: la scritta "Sei morto";
//   - in basso al centro: nome e vita del nemico agganciato, e le azioni possibili ("E  Raccogli: ...");
//   - sopra il nemico agganciato: un rombo color fiamma;
//   - in alto a destra: momento del giorno e ora del gioco.
// F1 accende e spegne il vecchio pannello di prova (stato del personaggio, comandi), utile solo per lo sviluppo.
// Si nasconde con il menu di pausa e con l'inventario aperti.
// Si crea da solo all'avvio del gioco e funziona in ogni scena che ha un giocatore: non va messo nelle scene.
// Gli altri script possono mostrare un'azione in basso con HudGioco.MostraAzione("E", "Apri la porta"),
// da chiamare in ogni fotogramma finché l'azione è possibile.
public class HudGioco : MonoBehaviour
{
    // C'è un giocatore e l'interfaccia lo sta mostrando (gli script vecchi nascondono le loro scritte di prova).
    public static bool Attivo { get; private set; }
    // Pannello di prova acceso con F1.
    public static bool PannelloProva { get; private set; }

    static string azioneTasto, azioneTesto;
    static int azioneFotogramma = -10;

    public static void MostraAzione(string tasto, string testo)
    {
        // "E" è il tasto per interagire: si mostra quello scelto dal giocatore in Opzioni > Comandi.
        if (tasto == "E") tasto = Comandi.NomeTasto(Azione.Interagisci);
        azioneTasto = tasto;
        azioneTesto = testo;
        azioneFotogramma = Time.frameCount;
    }

    static readonly Color ColoreVita = new Color(0.59f, 0.11f, 0.09f);
    static readonly Color ColoreResistenza = new Color(0.59f, 0.5f, 0.24f);
    static readonly Color ColoreMana = new Color(0.2f, 0.32f, 0.59f);

    // La vita dei nemici non è ancora pubblica in Bersaglio: la si legge così finché non c'è una proprietà apposta.
    static readonly FieldInfo campoVitaNemico = typeof(Bersaglio).GetField("vita", BindingFlags.Instance | BindingFlags.NonPublic);

    GiocatoreControllo giocatore;
    Resistenza resistenza;
    Equipaggiamento equipaggiamento;
    AggancioBersaglio aggancio;

    float scia, vitaPrima, sciaFerma;
    Bersaglio nemicoPrima;
    float sciaNemico, vitaNemicoPrima, sciaNemicoFerma;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreaAllAvvio()
    {
        if (FindFirstObjectByType<HudGioco>() != null) return;
        var oggetto = new GameObject("Interfaccia in partita");
        DontDestroyOnLoad(oggetto);
        oggetto.AddComponent<HudGioco>();
    }

    void OnEnable() => SceneManager.sceneLoaded += SceneCaricata;
    void OnDisable() => SceneManager.sceneLoaded -= SceneCaricata;
    void SceneCaricata(Scene s, LoadSceneMode m) => giocatore = null;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame) PannelloProva = !PannelloProva;

        if (giocatore == null)
        {
            giocatore = FindFirstObjectByType<GiocatoreControllo>();
            if (giocatore != null)
            {
                resistenza = giocatore.GetComponent<Resistenza>();
                equipaggiamento = giocatore.GetComponent<Equipaggiamento>();
                aggancio = giocatore.GetComponent<AggancioBersaglio>();
                scia = vitaPrima = giocatore.Vita;
            }
        }
        Attivo = giocatore != null;
        if (!Attivo) return;
        if (equipaggiamento == null) equipaggiamento = giocatore.GetComponent<Equipaggiamento>();

        // scia del danno: resta ferma un attimo, poi scende fino alla vita attuale
        float vita = giocatore.Vita;
        if (vita < vitaPrima - 0.01f) sciaFerma = Time.time + 0.6f;
        if (vita > scia) scia = vita;
        if (Time.time > sciaFerma) scia = Mathf.MoveTowards(scia, vita, Mathf.Max(1f, giocatore.VitaMassima) * 0.8f * Time.deltaTime);
        vitaPrima = vita;

        Bersaglio nemico = aggancio != null ? aggancio.Attuale : null;
        if (nemico != nemicoPrima)
        {
            nemicoPrima = nemico;
            if (nemico != null) sciaNemico = vitaNemicoPrima = VitaNemico(nemico);
        }
        if (nemico != null)
        {
            float v = VitaNemico(nemico);
            if (v < vitaNemicoPrima - 0.01f) sciaNemicoFerma = Time.time + 0.6f;
            if (v > sciaNemico) sciaNemico = v;
            if (Time.time > sciaNemicoFerma) sciaNemico = Mathf.MoveTowards(sciaNemico, v, Mathf.Max(1f, nemico.VitaMassima) * 0.8f * Time.deltaTime);
            vitaNemicoPrima = v;
        }
    }

    static float VitaNemico(Bersaglio nemico)
    {
        if (nemico == null) return 0f;
        if (campoVitaNemico != null && campoVitaNemico.GetValue(nemico) is float v) return v;
        return nemico.Morto ? 0f : nemico.VitaMassima;
    }

    // ---------- disegno ----------

    void OnGUI()
    {
        if (!Attivo || giocatore == null || MenuPausa.InPausa || InventarioGioco.Aperto) return;
        GUI.depth = 10;
        GraficaMenu.PreparaStili();
        float larghezza = GraficaMenu.FoglioIntero();
        float altezza = GraficaMenu.Altezza;

        // barre in alto a sinistra (la vita si allunga se cresce il massimo)
        float vitaMax = Mathf.Max(1f, giocatore.VitaMassima);
        Barra(80f, 70f, Mathf.Clamp(520f * vitaMax / 100f, 320f, 760f), 22f, giocatore.Vita / vitaMax, scia / vitaMax, ColoreVita,
            Lingua.T("stat.vita"), Mathf.CeilToInt(giocatore.Vita) + " / " + Mathf.CeilToInt(vitaMax));
        if (resistenza != null)
            Barra(80f, 128f, Mathf.Clamp(420f * resistenza.Massimo / 100f, 260f, 640f), 16f, resistenza.Attuale / Mathf.Max(1f, resistenza.Massimo), 0f,
                ColoreResistenza, Lingua.T("stat.resistenza"), null);
        if (giocatore.HaBastone || SceltaPartita.Classe == ClasseGiocatore.Stregone)
            Barra(80f, 178f, 340f, 16f, giocatore.Mana / Mathf.Max(1f, giocatore.ManaMassimo), 0f, ColoreMana, Lingua.T("stat.mana"), null);

        DisegnaArmi(altezza);
        DisegnaMorte(larghezza, altezza);
        DisegnaNemico(larghezza, altezza);
        DisegnaAzione(larghezza, altezza);
        DisegnaOra(larghezza);
    }

    void Barra(float x, float y, float w, float h, float parte, float partescia, Color colore, string etichetta, string valore)
    {
        parte = Mathf.Clamp01(parte);
        partescia = Mathf.Clamp01(partescia);
        GraficaMenu.Riempi(new Rect(x, y, w, h), new Color(0.03f, 0.03f, 0.04f, 0.86f));
        if (partescia > parte) GraficaMenu.Riempi(new Rect(x + 2f, y + 2f, (w - 4f) * partescia, h - 4f), new Color(0.86f, 0.78f, 0.67f, 0.5f));
        GraficaMenu.Riempi(new Rect(x + 2f, y + 2f, (w - 4f) * parte, h - 4f), colore);
        GraficaMenu.Riempi(new Rect(x + 2f, y + 2f, (w - 4f) * parte, (h - 4f) * 0.35f), new Color(1f, 1f, 1f, 0.12f));
        GraficaMenu.Bordo(new Rect(x, y, w, h), 2f, GraficaMenu.Con(GraficaMenu.Bronzo, 0.9f));
        GraficaMenu.Rombo(new Vector2(x, y + h * 0.5f), h + 6f, GraficaMenu.Bronzo);
        GraficaMenu.Rombo(new Vector2(x, y + h * 0.5f), h - 4f, GraficaMenu.Riquadro);
        GraficaMenu.Rombo(new Vector2(x + w, y + h * 0.5f), 10f, GraficaMenu.Bronzo);
        if (!string.IsNullOrEmpty(etichetta))
        {
            var e = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleLeft };
            GraficaMenu.Scritta(new Rect(x + 18f, y - 26f, w * 0.6f, 24f), etichetta, e, GraficaMenu.Testo, 1f);
        }
        if (!string.IsNullOrEmpty(valore))
        {
            var v = new GUIStyle(GraficaMenu.Etichetta) { alignment = TextAnchor.MiddleRight };
            GraficaMenu.Scritta(new Rect(x + w * 0.4f, y - 26f, w * 0.6f - 6f, 24f), valore, v, GraficaMenu.Spento, 1f);
        }
    }

    // Rombo grande con l'icona dentro; acceso = color fiamma (è quello in mano).
    void RomboOggetto(Vector2 centro, float lato, bool acceso, DatiOggetto oggetto, string tasto)
    {
        Color bordo = acceso ? GraficaMenu.Selezione : GraficaMenu.Bronzo;
        GraficaMenu.Rombo(centro, lato, bordo);
        GraficaMenu.Rombo(centro, lato - 8f, new Color(0.025f, 0.025f, 0.035f, 0.9f));
        GraficaMenu.Rombo(centro, lato - 22f, GraficaMenu.Con(bordo, 0.35f));
        GraficaMenu.Rombo(centro, lato - 24f, new Color(0.025f, 0.025f, 0.035f, 0.95f));
        float icona = lato * 0.5f;
        if (oggetto != null) GraficaMenu.Icona(new Rect(centro.x - icona * 0.5f, centro.y - icona * 0.5f, icona, icona), oggetto, 1f);
        if (!string.IsNullOrEmpty(tasto))
            GraficaMenu.Scritta(new Rect(centro.x - 20f, centro.y + lato * 0.42f, 40f, 24f), tasto, GraficaMenu.Etichetta,
                acceso ? GraficaMenu.Selezione : GraficaMenu.Spento, 1f);
    }

    void DisegnaArmi(float altezza)
    {
        float y = altezza - 150f;
        if (giocatore.Stregone && giocatore.Magia != null)
        {
            DisegnaIncantesimi(y);
            return;
        }
        bool spada = giocatore.Arma == GiocatoreControllo.ArmaImpugnata.Spada;
        var e = equipaggiamento;
        RomboOggetto(new Vector2(110f, y), 118f, spada, e != null ? e.Arma : null, "1");

        float x = 240f;
        if (giocatore.HaBastone)
        {
            Vector2 c = new Vector2(x, y);
            RomboOggetto(c, 118f, !spada, null, "2");
            // bastone disegnato: asta inclinata con una luce azzurra in cima
            Matrix4x4 prima = GUI.matrix;
            GUI.matrix = prima * Matrix4x4.TRS(c, Quaternion.Euler(0f, 0f, 40f), Vector3.one) * Matrix4x4.TRS(-c, Quaternion.identity, Vector3.one);
            GraficaMenu.Riempi(new Rect(c.x - 3f, c.y - 30f, 6f, 60f), new Color(0.55f, 0.42f, 0.28f));
            GUI.matrix = prima;
            GraficaMenu.Alone(new Rect(c.x + 8f, c.y - 40f, 30f, 30f), new Color(0.5f, 0.7f, 1f, 0.9f));
            x += 130f;
        }
        bool ladro = SceltaPartita.Classe == ClasseGiocatore.Ladro;
        if (ladro && e != null && e.ArmaDistanza != null)
        {
            // il tiro con l'arco non c'è ancora: l'arco si vede, ma non si accende
            RomboOggetto(new Vector2(x, y), 118f, false, e.ArmaDistanza, null);   // il tasto arriverà con il tiro
            x += 130f;
        }
        else if (SceltaPartita.Classe == ClasseGiocatore.Stregone && e != null && e.Libro != null)
        {
            // Stregone: il libro sta dove gli altri tengono lo scudo
            RomboOggetto(new Vector2(x + 10f, y + 10f), 86f, false, e.Libro, null);
            x += 100f;
        }
        else if (!ladro && e != null && e.Scudo != null)
        {
            RomboOggetto(new Vector2(x + 10f, y + 10f), 86f, false, e.Scudo, null);
            x += 100f;
        }
        if (e != null && e.Amuleto != null)
        {
            RomboOggetto(new Vector2(x + 10f, y + 16f), 70f, false, e.Amuleto, null);
            x += 84f;
        }
        if (giocatore.HaAbilitaOmbra) RomboAbilita(new Vector2(x + 10f, y + 16f), 70f);
    }

    // Stregone: i rombi delle caselle degli incantesimi, con il tasto (1-6). Quello scelto è più grande e acceso;
    // sotto ogni rombo una barretta si riempie durante l'attesa e diventa color fiamma quando è pronto.
    // Sopra la fila: il nome dell'incantesimo scelto e il suo costo in mana.
    void DisegnaIncantesimi(float y)
    {
        var magia = giocatore.Magia;
        var e = equipaggiamento;
        if (e == null) return;
        int n = e.NumeroCaselle;
        float x = 110f;
        for (int i = 0; i < n; i++)
        {
            var inc = e.Incantesimo(i);
            bool scelto = i == magia.Scelta;
            float lato = scelto ? 112f : 84f;
            var c = new Vector2(x, y + (scelto ? 0f : 14f));
            RomboOggetto(c, lato, scelto, inc, (i + 1).ToString());
            if (inc != null)
            {
                float pronto = magia.Pronto(inc);
                var barra = new Rect(c.x - lato * 0.3f, c.y + lato * 0.62f, lato * 0.6f, 5f);
                GraficaMenu.Riempi(barra, new Color(0f, 0f, 0f, 0.6f));
                GraficaMenu.Riempi(new Rect(barra.x, barra.y, barra.width * pronto, barra.height),
                    pronto >= 1f ? GraficaMenu.Selezione : new Color(0.35f, 0.45f, 0.75f));
            }
            x += scelto ? 118f : 96f;
        }
        var attuale = magia.Scelto;
        if (attuale != null)
        {
            var stile = new GUIStyle(GraficaMenu.Didascalia) { alignment = TextAnchor.MiddleLeft };
            string testo = attuale.Nome + GraficaMenu.Separatore + Lingua.T("stat.mana") + " " + Mathf.CeilToInt(magia.Costo(attuale));
            GraficaMenu.Scritta(new Rect(60f, y - 100f, 700f, 30f), testo, stile, giocatore.Mana >= magia.Costo(attuale) ? GraficaMenu.Testo : GraficaMenu.Spento, 1f);
        }
    }

    // Abilità dell'amuleto (tasto Q): il rombo interno cresce mentre si ricarica; pronto = color fiamma,
    // in uso (invisibile) = viola che pulsa.
    void RomboAbilita(Vector2 centro, float lato)
    {
        bool invisibile = giocatore.Invisibile;
        float pronta = invisibile ? 1f : Mathf.Clamp01(giocatore.OmbraPronta);
        bool accesa = !invisibile && pronta >= 1f;
        Color viola = new Color(0.62f, 0.4f, 0.95f);
        Color bordo = invisibile ? viola : accesa ? GraficaMenu.Selezione : GraficaMenu.Bronzo;
        GraficaMenu.Rombo(centro, lato, bordo);
        GraficaMenu.Rombo(centro, lato - 8f, new Color(0.025f, 0.025f, 0.035f, 0.9f));
        float pulsa = invisibile ? 0.6f + 0.4f * Mathf.Sin(Time.time * 6f) : 1f;
        Color pieno = invisibile ? GraficaMenu.Con(viola, 0.5f * pulsa) : GraficaMenu.Con(accesa ? GraficaMenu.Selezione : GraficaMenu.Bronzo, accesa ? 0.45f : 0.3f);
        GraficaMenu.Rombo(centro, (lato - 14f) * pronta, pieno);
        GraficaMenu.Scritta(new Rect(centro.x - 20f, centro.y - 14f, 40f, 28f), "Q", GraficaMenu.Etichetta,
            accesa || invisibile ? GraficaMenu.Testo : GraficaMenu.Spento, 1f);
    }

    // Da morti: "Sei morto" grande al centro, rosso cupo, finché si rinasce.
    // In co-op: "A terra" (aspetta un alleato) oppure, da spettatore, solo una riga in alto.
    void DisegnaMorte(float larghezza, float altezza)
    {
        if (giocatore.StatoAttuale != GiocatoreControllo.Stato.Morto) return;
        if (giocatore.StatoCaduta == GiocatoreControllo.Caduta.Spettatore)
        {
            GraficaMenu.Riempi(new Rect(0f, 40f, larghezza, 60f), new Color(0f, 0f, 0f, 0.5f));
            GraficaMenu.Scritta(new Rect(0f, 40f, larghezza, 60f), Lingua.T("hud.spettatore"), GraficaMenu.Voce, GraficaMenu.Testo, 1f);
            return;
        }
        bool aTerra = giocatore.StatoCaduta == GiocatoreControllo.Caduta.ATerra;
        GraficaMenu.Riempi(new Rect(0f, altezza * 0.5f - 90f, larghezza, aTerra ? 240f : 180f), new Color(0f, 0f, 0f, 0.55f));
        if (aTerra)
            GraficaMenu.Scritta(new Rect(0f, altezza * 0.5f + 80f, larghezza, 50f), Lingua.T("hud.a_terra_aiuto"), GraficaMenu.Voce, GraficaMenu.Testo, 1f);
        string testo = Lingua.T(aTerra ? "hud.a_terra" : "hud.morto").ToUpperInvariant();
        if (Lingua.Indice < 6) testo = GraficaMenu.Spaziato(testo);
        GraficaMenu.Scritta(new Rect(0f, altezza * 0.5f - 70f, larghezza, 140f), testo, GraficaMenu.Titolo, new Color(0.62f, 0.1f, 0.08f), 1f);
    }

    void DisegnaNemico(float larghezza, float altezza)
    {
        Bersaglio nemico = aggancio != null ? aggancio.Attuale : null;
        if (nemico == null || nemico.Morto) return;

        string nome = nemico.gameObject.name.Replace("(Clone)", "").Trim();
        GraficaMenu.Scritta(new Rect(0f, altezza - 214f, larghezza, 40f), nome, GraficaMenu.NomeClasse, GraficaMenu.Testo, 1f);
        float max = Mathf.Max(1f, nemico.VitaMassima);
        Barra(larghezza * 0.5f - 400f, altezza - 175f, 800f, 16f, VitaNemico(nemico) / max, sciaNemico / max, ColoreVita, null, null);

        // rombo color fiamma sopra il nemico agganciato
        var camera = Camera.main;
        if (camera == null) return;
        Vector3 schermo = camera.WorldToScreenPoint(nemico.transform.position + Vector3.up * 1.4f);
        if (schermo.z <= 0f) return;
        float s = Screen.height / altezza;
        var punto = new Vector2(schermo.x / s, (Screen.height - schermo.y) / s);
        float pulsa = 0.75f + 0.25f * Mathf.Sin(Time.time * 4f);
        GraficaMenu.Rombo(punto, 26f, GraficaMenu.Con(GraficaMenu.Selezione, 0.35f * pulsa));
        GraficaMenu.Rombo(punto, 18f, GraficaMenu.Con(GraficaMenu.Selezione, pulsa));
        GraficaMenu.Rombo(punto, 10f, new Color(0.03f, 0.03f, 0.04f, 0.9f));
    }

    void DisegnaAzione(float larghezza, float altezza)
    {
        if (Time.frameCount - azioneFotogramma > 1 || string.IsNullOrEmpty(azioneTesto)) return;
        // il tasto: una lettera sta nel rombo; un nome lungo (per esempio "Mouse sinistro") in un riquadro
        float kw = GraficaMenu.Etichetta.CalcSize(new GUIContent(azioneTasto)).x;
        bool lungo = kw > 30f;
        float spazioTasto = lungo ? kw + 44f : 72f;
        float w = Mathf.Clamp(GraficaMenu.TestoSinistra.CalcSize(new GUIContent(azioneTesto)).x + spazioTasto + 38f, 360f, 1100f);
        var r = new Rect(larghezza * 0.5f - w * 0.5f, altezza - 100f, w, 56f);
        GraficaMenu.Cornice(r, 0.95f, false);
        if (lungo)
        {
            var k = new Rect(r.x + 14f, r.y + 12f, kw + 16f, r.height - 24f);
            GraficaMenu.Riempi(k, GraficaMenu.Bronzo);
            GraficaMenu.Scritta(k, azioneTasto, GraficaMenu.Etichetta, new Color(0.05f, 0.05f, 0.06f), 1f);
        }
        else
        {
            var c = new Vector2(r.x + 40f, r.center.y);
            GraficaMenu.Rombo(c, 30f, GraficaMenu.Bronzo);
            GraficaMenu.Scritta(new Rect(c.x - 20f, c.y - 14f, 40f, 28f), azioneTasto, GraficaMenu.Etichetta, new Color(0.05f, 0.05f, 0.06f), 1f);
        }
        var testo = new GUIStyle(GraficaMenu.TestoSinistra) { wordWrap = false };
        GraficaMenu.Scritta(new Rect(r.x + spazioTasto, r.y, r.width - spazioTasto - 18f, r.height), azioneTesto, testo, GraficaMenu.Testo, 1f);
    }

    void DisegnaOra(float larghezza)
    {
        var ciclo = CicloGiornoNotte.Istanza;
        if (ciclo == null) return;
        float ora = ciclo.ora;
        string momento = ora >= 5f && ora < 7f ? "hud.alba" : ora >= 7f && ora < 17f ? "hud.giorno" : ora >= 17f && ora < 19f ? "hud.tramonto" : "hud.notte";
        int ore = Mathf.FloorToInt(ora), minuti = Mathf.FloorToInt((ora - ore) * 60f);
        var r = new Rect(larghezza - 300f, 60f, 220f, 52f);
        GraficaMenu.Cornice(r, 0.8f, false);
        GraficaMenu.Scritta(r, Lingua.T(momento) + GraficaMenu.Separatore + ore.ToString("00") + ":" + minuti.ToString("00"), GraficaMenu.Etichetta, GraficaMenu.Testo, 1f);
    }
}
