using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: crea la scena "in grigio" della gattabuia, la prigione degli orchi dove ci si risveglia dopo
// lo scontro perso con l'orco enorme (Docs/gattabuia.md, Lorenzo 10-11 ottobre; prima versione del 14 ottobre).
// È una scena a sé (non una zona del villaggio): RazziaVillaggio la carica quando si perde contro l'orco enorme.
// La pianta è disegnata qui sotto a caselle di 3 metri (Mappa). Le 8 zone, in ordine:
//   1. le celle (in basso a sinistra): risveglio in cella (A), si forza la sbarra tenendo premuto E (b), la sbarra è
//      la prima arma; le altre celle (B, c) hanno cadaveri dietro le sbarre;
//   2. il corridoio delle celle, con un osso lungo da raccogliere (o) e un cadavere (c);
//   3. i macellai: due orchi distratti (m) che lavorano ai tavoli (t, con la mannaia sopra): ignari, si possono
//      giustiziare alle spalle; li sente solo chi corre; un pugnale arrugginito a terra (p);
//   4. l'armeria: rastrelliere vuote (r) e il baule delle armi (Q) con le armi base della propria classe; subito
//      dopo un checkpoint (h);
//   5. i sotterranei: fosse comuni (F), un catenaccio (k), un orco di pattuglia (G) che va avanti e indietro fino a (w);
//   6. un buco nel muro (H) porta alle grotte buie (d) con funghi che brillano appena (L); entrando (Z) escono i
//      parassiti (x, per ora ratti giganti pallidi: il nemico vero è da fare);
//   7. la piazzola del Carceriere (K), con un checkpoint prima di entrare (C);
//   8. la scalinata (S) e il portone (O): con la chiave del Carceriere si apre ed è la fine del capitolo 1.
//   T = torcia sul muro più vicino.
// Obiettivi e bussola (Missioni) per ogni passo. Al risveglio il prologo conta finito (InizioGattabuia), così da qui
// si apre il co-op. Si salva in Assets/Scenes/Gattabuia.unity e si aggiunge alle Build Settings. Se esiste già chiede
// se rifarla. Crea anche, se mancano, la sbarra arrugginita e la chiave del portone.
// Come si usa: menu "magic-gnl > Gattabuia: crea la scena della gattabuia". Non va montato su niente.
public static class CreaGattabuia
{
    public const string NomeScena = "Gattabuia";
    const string Percorso = "Assets/Scenes/Gattabuia.unity";
    const string PercorsoChiave = "Assets/Dati/Oggetti/Missioni/chiave-del-portone.asset";
    const float Casella = 3f, Altezza = 4f;

    static readonly string[] Mappa =
    {
        "##################################",
        "###############O##################",
        "###############S##################",
        "############T.....T..C.....dxdd###",
        "############.......########ddLd###",
        "############...K...##########dx###",
        "############.......##########xd###",
        "############T.....T##########dL###",
        "########rrQrrT######T.....###dx###",
        "########.............F...w###dd###",
        "########..h...######......HdZdL###",
        "###########.########.G..F.#dddd###",
        "#########T..m....T##..k..T########",
        "#########...t.t...################",
        "#########......m..################",
        "#########.p.......################",
        "################.#################",
        "################c#################",
        "################.#################",
        "##T.....o.....T..#################",
        "###b##B##B##B#####################",
        "###A##c##c##c#####################",
        "##################################",
        "##################################",
    };

    static int Righe => Mappa.Length;
    static int Colonne => Mappa[0].Length;
    static char Cella(int r, int c) => r < 0 || c < 0 || r >= Righe || c >= Colonne ? '#' : Mappa[r][c];
    static bool Pieno(int r, int c) { char x = Cella(r, c); return x == '#'; }
    static Vector3 Centro(int r, int c) => new Vector3(c * Casella, 0f, (Righe - 1 - r) * Casella);

    static Material Roccia => Materiale("gattabuia-roccia", new Color(0.22f, 0.21f, 0.2f));
    static Material Lastre => Materiale("gattabuia-lastre", new Color(0.18f, 0.17f, 0.16f));
    static Material Grotta => Materiale("gattabuia-grotta", new Color(0.12f, 0.12f, 0.13f));
    static Material FerroNero => Materiale("cripta-ferro", new Color(0.16f, 0.16f, 0.18f));
    static Material Sangue => Materiale("gattabuia-sangue", new Color(0.22f, 0.03f, 0.03f));
    static Material Stracci => Materiale("gattabuia-stracci", new Color(0.3f, 0.27f, 0.22f));
    static Material Osso => Materiale("cripta-osso", new Color(0.82f, 0.78f, 0.66f));
    static Material Fungo => Materiale("gattabuia-fungo", new Color(0.3f, 0.6f, 0.9f), true);
    static Material PelleCarceriere => Materiale("gattabuia-carceriere", new Color(0.3f, 0.33f, 0.22f));
    static Material Parassita => Materiale("gattabuia-parassita", new Color(0.75f, 0.72f, 0.62f));

    [MenuItem("magic-gnl/Gattabuia: crea la scena della gattabuia")]
    static void Menu()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Percorso) != null &&
            !EditorUtility.DisplayDialog("Gattabuia", "La scena della gattabuia esiste già. Rifarla da capo? Le modifiche fatte a mano andranno perse.", "Rifai", "Annulla"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Crea();
        EditorUtility.DisplayDialog("Gattabuia", "Scena della gattabuia creata in " + Percorso + " e aggiunta alle Build Settings.\n"
            + "Si arriva perdendo contro l'orco enorme della razzia, oppure aprendo la scena e premendo Play.", "Ok");
    }

    internal static void Crea()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        CreaOggettiComuni.Crea();   // le armi improvvisate (sbarra, osso, pugnale, mannaia...) se mancano
        var sbarra = AiutiPrologo.Oggetto("sbarra-arrugginita");
        var chiave = Chiave();
        var scena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) Object.DestroyImmediate(l.gameObject);   // sotto terra: niente sole

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.24f, 0.22f, 0.25f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.025f);
        RenderSettings.fogDensity = 0.035f;
        RenderSettings.skybox = null;

        var radice = new GameObject("Gattabuia").transform;
        Struttura(radice);
        var oggetti = Vuoto("Oggetti", radice).transform;
        var nemici = Vuoto("Nemici", radice).transform;
        var parassiti = Vuoto("Parassiti (escono nelle grotte)", nemici).transform;

        var missioni = Vuoto("Missioni", radice).AddComponent<Missioni>();
        var inizio = Vuoto("Inizio della gattabuia", radice).AddComponent<InizioGattabuia>();

        Transform arrivo = null, portone = null, piazzola = null, uscitaArmeria = null, tavolo = null;
        Porta portaCella = null;
        Vector3 puntoSbarra = Vector3.zero;
        global::Baule baule = null;
        Carceriere carceriere = null;
        GameObject zonaGrotte = null, zonaPiazzola = null;
        var macellai = new List<(GameObject orco, Vector3 p)>();
        Vector3 pattugliaDa = Vector3.zero, pattugliaA = Vector3.zero;
        bool pattuglia = false;

        for (int r = 0; r < Righe; r++)
            for (int c = 0; c < Colonne; c++)
            {
                Vector3 p = Centro(r, c);
                switch (Cella(r, c))
                {
                    case 'A':
                        arrivo = Vuoto("Arrivo", radice, p).transform;
                        Cadavere(oggetti, p + new Vector3(0.7f, 0f, -0.6f), r * 7 + c, false);
                        puntoSbarra = p + Vector3.forward * 0.8f;
                        break;
                    case 'b': portaCella = Sbarre(oggetti, p, true).GetComponent<Porta>(); break;
                    case 'B': Sbarre(oggetti, p, false); break;
                    case 'c': Cadavere(oggetti, p, r * 7 + c, true); break;
                    case 'T': Torcia(oggetti, r, c); break;
                    case 'o': Raccogli(oggetti, p, "osso-lungo"); break;
                    case 'p': Raccogli(oggetti, p, "pugnale-arrugginito"); break;
                    case 'k': Raccogli(oggetti, p, "catenaccio"); break;
                    case 't':
                        var t = Tavolo(oggetti, p, tavolo == null);
                        if (tavolo == null) tavolo = t;
                        break;
                    case 'm':
                        var orco = CreaOrcoSgherro.Metti("Orco macellaio", p, Vector3.forward, nemici);
                        macellai.Add((orco, p));
                        break;
                    case 'r': Rastrelliera(oggetti, p); break;
                    case 'Q': baule = Baule(oggetti, p); break;
                    case 'h': uscitaArmeria = Checkpoint(oggetti, p).transform; break;
                    case 'C': zonaPiazzola = Checkpoint(oggetti, p); break;
                    case 'F': Fossa(oggetti, p, r * 13 + c); break;
                    case 'G': pattugliaDa = p; pattuglia = true; break;
                    case 'w': pattugliaA = p; break;
                    case 'H': Buco(oggetti, p); break;
                    case 'Z': zonaGrotte = Vuoto("Entrata nelle grotte", oggetti, p); break;
                    case 'x': ParassitaProvvisorio(parassiti, p, r * 31 + c); break;
                    case 'L': Funghi(oggetti, p, r * 17 + c); break;
                    case 'K':
                        carceriere = CreaCarceriere(nemici, p);
                        piazzola = Vuoto("Centro della piazzola", radice, p).transform;
                        break;
                    case 'S': Scalinata(oggetti, p); break;
                    case 'O': portone = Portone(oggetti, p, chiave).transform; break;
                }
            }

        // i macellai lavorano al tavolo più vicino
        foreach (var (orco, p) in macellai)
        {
            Transform vicino = null;
            float minima = float.MaxValue;
            foreach (Transform t in oggetti)
                if (t.name == "Tavolo dei macellai" && (t.position - p).sqrMagnitude < minima) { minima = (t.position - p).sqrMagnitude; vicino = t; }
            var lavoro = Undo.AddComponent<RazziatoreOrco>(orco);
            Imposta(lavoro, "puntoDiLavoro", vicino);
            Imposta(lavoro, "secondiPerGesto", 1.8f);
            Imposta(lavoro, "raggioUditoCorsa", 10f);
            Imposta(Undo.AddComponent<NomeTradotto>(orco), "chiave", "nem.macellaio");
            var v = orco.GetComponent<InseguimentoNemico>();
            Imposta(v, "distanzaVista", 12f);
            Imposta(v, "angoloVisivo", 110f);
            if (vicino != null)
            {
                Vector3 verso = vicino.position - orco.transform.position;
                verso.y = 0f;
                if (verso.sqrMagnitude > 0.01f) orco.transform.rotation = Quaternion.LookRotation(verso);
            }
            foreach (var comp in orco.GetComponents<Component>()) PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
        }

        // l'orco di pattuglia, fra G e w
        if (pattuglia)
        {
            var orco = CreaOrcoSgherro.Metti("Orco di pattuglia", pattugliaDa, Vector3.right, nemici);
            var punti = new[] { Vuoto("Pattuglia: punto 1", nemici, pattugliaDa).transform, Vuoto("Pattuglia: punto 2", nemici, pattugliaA).transform };
            var giro = Undo.AddComponent<PattugliaOrco>(orco);
            Imposta(giro, "punti", punti);
            Imposta(Undo.AddComponent<NomeTradotto>(orco), "chiave", "nem.orco");
            foreach (var comp in orco.GetComponents<Component>()) PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
        }

        // ---------- il giocatore ----------
        var giocatore = CreaScenaProva.CreaGiocatore(arrivo != null ? arrivo.position + Vector3.up * 1.05f : Vector3.up, 0f);
        CreaScenaProva.PreparaCamera(giocatore.transform, true);
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = RenderSettings.fogColor;
            Camera.main.farClipPlane = 120f;
        }
        var zona = Vuoto("Zona camera (gattabuia)", radice, new Vector3((Colonne - 1) * Casella / 2f, 0f, (Righe - 1) * Casella / 2f)).AddComponent<ZonaCamera>();
        Imposta(zona, "dimensioni", new Vector3(Colonne * Casella, Altezza, Righe * Casella));
        Imposta(zona, "distanzaMassima", 4.5f);
        Imposta(zona, "inclinazioneMassima", 35f);

        // ---------- i passi ----------
        var avvio = Evento(inizio, "allAvvio");
        Su(avvio, missioni.ImpostaObiettivo, "ob.gb.cella");
        Su(avvio, missioni.Suggerisci, "tip.gb.sbarra");

        if (portaCella != null)
        {
            var cella = Vuoto("Sbarra da forzare", oggetti, puntoSbarra).AddComponent<SbarraCella>();
            Imposta(cella, "porta", portaCella);
            Imposta(cella, "sbarra", sbarra);
            var dopo = Evento(cella, "dopo");
            Su(dopo, missioni.ImpostaObiettivo, "ob.gb.fuga");
            Su(dopo, missioni.Suggerisci, "tip.gb.furtivo");
            if (baule != null) Meta(dopo, baule.gameObject);
        }
        if (baule != null)
        {
            var dona = baule.gameObject.AddComponent<DonaOggetti>();
            Imposta(dona, "perGuerriero", new[] { AiutiPrologo.Oggetto("spada-della-vecchia-vita"), AiutiPrologo.Oggetto("scudo-della-vecchia-vita") });
            Imposta(dona, "perLadro", new[] { AiutiPrologo.Oggetto("pugnale-da-scuoiare"), AiutiPrologo.Oggetto("arco-corto-da-caccia") });
            Imposta(dona, "perStregone", new[] { AiutiPrologo.Oggetto("bastone-della-vecchia-vita"), AiutiPrologo.Oggetto("libro-della-vecchia-vita"),
                AiutiPrologo.Oggetto("scintilla"), AiutiPrologo.Oggetto("scheggia-di-ghiaccio") });
            var aperto = Evento(baule, "quandoAperto");
            Su(aperto, dona.Dona);
            Su(aperto, missioni.ImpostaObiettivo, "ob.gb.uscita");
            if (piazzola != null) Meta(aperto, piazzola.gameObject);
        }
        if (zonaGrotte != null)
        {
            var z = zonaGrotte.AddComponent<ZonaArrivo>();
            Imposta(z, "raggio", 2.5f);
            parassiti.gameObject.SetActive(false);
            var entra = Evento(z, "allArrivo");
            Su(entra, parassiti.gameObject.SetActive, true);
            Su(entra, missioni.Suggerisci, "tip.gb.parassiti");
        }
        if (zonaPiazzola != null && carceriere != null)
        {
            var z = Vuoto("Davanti alla piazzola", zonaPiazzola.transform).AddComponent<ZonaArrivo>();
            Imposta(z, "raggio", 3f);
            var entra = Evento(z, "allArrivo");
            Su(entra, missioni.ImpostaObiettivo, "ob.gb.carceriere");
            Meta(entra, carceriere.gameObject);
            var vinto = Evento(carceriere, "allaSconfitta");
            Su(vinto, missioni.ImpostaObiettivo, "ob.gb.portone");
            if (portone != null) Meta(vinto, portone.gameObject);
            Imposta(carceriere, "bottinoSempre", new DatiOggetto[] { chiave, AiutiPrologo.Oggetto("catena-del-carceriere") });
            Imposta(carceriere, "bottinoACaso", new[] { AiutiPrologo.Oggetto("mazza-chiodata"), AiutiPrologo.Oggetto("mannaia-affilata"),
                AiutiPrologo.Oggetto("dente-d-orco"), AiutiPrologo.Oggetto("lacci-di-cuoio"), AiutiPrologo.Oggetto("pietra-torbida-del-lago") });
        }

        EditorSceneManager.SaveScene(scena, Percorso);
        AggiungiAlleBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("Gattabuia: scena creata in " + Percorso + " (" + Righe + "x" + Colonne + " caselle da " + Casella + " m).");
    }

    static void AggiungiAlleBuildSettings()
    {
        var elenco = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in elenco) if (s.path == Percorso) return;
        elenco.Add(new EditorBuildSettingsScene(Percorso, true));
        EditorBuildSettings.scenes = elenco.ToArray();
    }

    // La chiave del portone (oggetto delle missioni), creata se manca.
    static DatiOggetto Chiave()
    {
        var c = AssetDatabase.LoadAssetAtPath<DatiOggettoMissione>(PercorsoChiave);
        if (c != null) return c;
        if (!AssetDatabase.IsValidFolder("Assets/Dati/Oggetti/Missioni")) AssetDatabase.CreateFolder("Assets/Dati/Oggetti", "Missioni");
        c = ScriptableObject.CreateInstance<DatiOggettoMissione>();
        c.nomeDiLavoro = "Chiave del portone";
        c.chiaveNome = "oggetto.chiave_portone.nome";
        c.chiaveDescrizione = "oggetto.chiave_portone.descrizione";
        c.tutteLeClassi = true;
        c.colore = new Color(0.45f, 0.38f, 0.25f);
        c.forma = DatiOggettoMissione.FormaProvvisoria.Sacchetto;
        AssetDatabase.CreateAsset(c, PercorsoChiave);
        return c;
    }

    // ---------- pavimento, soffitto e muri ----------

    static void Struttura(Transform radice)
    {
        var struttura = Vuoto("Struttura", radice).transform;
        Vector3 meta = new Vector3((Colonne - 1) * Casella / 2f, 0f, (Righe - 1) * Casella / 2f);
        Blocco("Pavimento", struttura, meta + Vector3.down * 0.1f, new Vector3(Colonne * Casella, 0.2f, Righe * Casella), Lastre);
        Blocco("Soffitto", struttura, meta + Vector3.up * (Altezza + 0.15f), new Vector3(Colonne * Casella, 0.3f, Righe * Casella), Roccia);
        for (int r = 0; r < Righe; r++)
        {
            int c = 0;
            while (c < Colonne)
            {
                if (!Muro(r, c)) { c++; continue; }
                int inizio = c;
                while (c < Colonne && Muro(r, c)) c++;
                int n = c - inizio;
                Vector3 centro = (Centro(r, inizio) + Centro(r, c - 1)) / 2f + Vector3.up * Altezza / 2f;
                Blocco("Muro", struttura, centro, new Vector3(n * Casella, Altezza, Casella), Roccia);
            }
        }
        // pavimento delle grotte più scuro e irregolare
        for (int r = 0; r < Righe; r++)
            for (int c = 0; c < Colonne; c++)
            {
                char x = Cella(r, c);
                if (x != 'd' && x != 'x' && x != 'L' && x != 'Z') continue;
                Decoro("Terra della grotta", struttura, Centro(r, c) + Vector3.up * 0.01f, new Vector3(Casella, 0.02f, Casella), Grotta);
                if ((r * 7 + c) % 3 == 0)
                    Blocco("Stalagmite", struttura, Centro(r, c) + new Vector3(Casella * 0.35f, 0.5f, -Casella * 0.3f), new Vector3(0.35f, 1f, 0.35f), Roccia, 25f);
            }
    }

    static bool Muro(int r, int c)
    {
        if (!Pieno(r, c)) return false;
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
                if (!Pieno(r + dr, c + dc)) return true;
        return false;
    }

    // ---------- pezzi ----------

    // Sbarre della cella sulla casella (aperta = la Porta della propria cella, da forzare).
    static GameObject Sbarre(Transform genitore, Vector3 p, bool daAprire)
    {
        var g = Forma(PrimitiveType.Cube, daAprire ? "Sbarre della cella" : "Sbarre", genitore, p + new Vector3(0f, Altezza / 2f, 0f),
            new Vector3(Casella, Altezza, 0.2f), FerroNero, true);
        g.GetComponent<Renderer>().enabled = false;   // si vedono solo le sbarre
        for (int i = -5; i <= 5; i++)
            Decoro("Sbarra", g.transform, new Vector3(i * 0.09f, 0f, 0f), new Vector3(0.025f, 1f, 0.4f), FerroNero);
        Decoro("Traversa", g.transform, new Vector3(0f, 0.3f, 0f), new Vector3(1f, 0.025f, 0.4f), FerroNero);
        if (daAprire)
        {
            var porta = g.AddComponent<Porta>();
            Imposta(porta, "durataApertura", 1.2f);
        }
        return g;
    }

    // Un cadavere coperto di stracci, con una macchia di sangue (dietro le sbarre o in un corridoio).
    static void Cadavere(Transform genitore, Vector3 p, int seme, bool sdraiato)
    {
        var caso = new System.Random(seme);
        var o = Vuoto("Cadavere", genitore, p).transform;
        o.localRotation = Quaternion.Euler(0f, (float)caso.NextDouble() * 360f, 0f);
        Decoro("Macchia di sangue", o, new Vector3(0f, 0.012f, 0f), new Vector3(1.4f, 0.01f, 1.1f), Sangue);
        var corpo = Forma(PrimitiveType.Capsule, "Corpo", o, new Vector3(0f, 0.2f, 0f), new Vector3(0.45f, 0.8f, 0.45f), Stracci);
        corpo.transform.localRotation = Quaternion.Euler(sdraiato ? 90f : 60f, 0f, 0f);
        Forma(PrimitiveType.Sphere, "Teschio", o, new Vector3(0f, 0.2f, 0.85f), Vector3.one * 0.25f, Osso);
    }

    static void Torcia(Transform genitore, int r, int c)
    {
        Vector3 verso = Pieno(r - 1, c) ? Vector3.forward : Pieno(r, c - 1) ? Vector3.left : Pieno(r, c + 1) ? Vector3.right : Vector3.back;
        Vector3 p = Centro(r, c) + verso * (Casella / 2f - 0.15f) + Vector3.up * 2.4f;
        var torcia = Vuoto("Torcia", genitore, p);
        Decoro("Supporto", torcia.transform, -verso * 0.12f + Vector3.down * 0.15f, new Vector3(0.12f, 0.35f, 0.12f), FerroNero);
        Forma(PrimitiveType.Sphere, "Fiamma", torcia.transform, -verso * 0.15f + Vector3.up * 0.1f, new Vector3(0.18f, 0.28f, 0.18f), Fuoco);
        var luce = Vuoto("Luce", torcia.transform, -verso * 0.6f);
        var l = luce.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.6f, 0.3f);
        l.range = 10f;
        l.intensity = 1.5f;
        Imposta(luce.AddComponent<global::Torcia>(), "intensitaMassima", 1.5f);
    }

    static void Raccogli(Transform genitore, Vector3 p, string file)
    {
        var o = Vuoto("Da raccogliere: " + file, genitore, p + Vector3.up * 0.05f);
        var r = o.AddComponent<OggettoRaccoglibile>();
        Imposta(r, "oggetto", AiutiPrologo.Oggetto(file));
        Imposta(r, "distanza", 2f);
    }

    // Il tavolo dei macellai; sul primo c'è la mannaia da prendere.
    static Transform Tavolo(Transform genitore, Vector3 p, bool conMannaia)
    {
        var t = Vuoto("Tavolo dei macellai", genitore, p).transform;
        Blocco("Piano", t, new Vector3(0f, 0.85f, 0f), new Vector3(2f, 0.15f, 1.1f), Materiale("gattabuia-legno", new Color(0.25f, 0.16f, 0.1f)));
        Decoro("Gamba", t, new Vector3(-0.85f, 0.4f, 0f), new Vector3(0.15f, 0.8f, 0.9f), FerroNero);
        Decoro("Gamba", t, new Vector3(0.85f, 0.4f, 0f), new Vector3(0.15f, 0.8f, 0.9f), FerroNero);
        Decoro("Sangue sul tavolo", t, new Vector3(0.2f, 0.935f, 0.1f), new Vector3(1.2f, 0.01f, 0.7f), Sangue);
        Forma(PrimitiveType.Capsule, "Resti coperti", t, new Vector3(-0.2f, 1.05f, 0f), new Vector3(0.4f, 0.6f, 0.4f), Stracci).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        Decoro("Gancio", t, new Vector3(0f, 2.6f, 0.9f), new Vector3(0.05f, 1.2f, 0.05f), FerroNero);
        if (conMannaia)
        {
            var m = Vuoto("Da raccogliere: mannaia-del-macellaio", t, new Vector3(0.7f, 0.95f, -0.2f));
            var r = m.AddComponent<OggettoRaccoglibile>();
            Imposta(r, "oggetto", AiutiPrologo.Oggetto("mannaia-del-macellaio"));
            Imposta(r, "distanza", 2.2f);
        }
        return t;
    }

    static void Rastrelliera(Transform genitore, Vector3 p)
    {
        var t = Vuoto("Rastrelliera vuota", genitore, p + Vector3.forward * (Casella / 2f - 0.4f)).transform;
        Blocco("Asse", t, new Vector3(0f, 1.2f, 0f), new Vector3(2.4f, 0.12f, 0.3f), Materiale("gattabuia-legno", new Color(0.25f, 0.16f, 0.1f)));
        Blocco("Asse bassa", t, new Vector3(0f, 0.4f, 0f), new Vector3(2.4f, 0.12f, 0.3f), Materiale("gattabuia-legno", new Color(0.25f, 0.16f, 0.1f)));
        for (int i = -2; i <= 2; i++) Decoro("Piolo", t, new Vector3(i * 0.5f, 0.9f, 0.1f), new Vector3(0.06f, 1.2f, 0.06f), FerroNero);
    }

    static global::Baule Baule(Transform genitore, Vector3 p)
    {
        var o = Vuoto("Baule delle armi", genitore, p + Vector3.back * 0.3f);
        o.transform.localRotation = Quaternion.LookRotation(Vector3.back);
        var b = o.AddComponent<global::Baule>();
        Imposta(b, "contenuto", global::Baule.Contenuto.Oggetto);
        Imposta(b, "raggioInterazione", 2.2f);
        Luce("Luce sul baule", o.transform, new Vector3(0f, 3f, 0f), new Color(1f, 0.82f, 0.5f), 5f, 0.8f);
        return b;
    }

    static GameObject Checkpoint(Transform genitore, Vector3 p)
    {
        var o = Vuoto("Checkpoint", genitore, p);
        o.AddComponent<global::Checkpoint>();
        return o;
    }

    // Una fossa comune: un rettangolo buio pieno di ossa.
    static void Fossa(Transform genitore, Vector3 p, int seme)
    {
        var t = Vuoto("Fossa comune", genitore, p).transform;
        Decoro("Buio della fossa", t, new Vector3(0f, 0.015f, 0f), new Vector3(2.4f, 0.01f, 2.4f), Materiale("cripta-buio", new Color(0.01f, 0.01f, 0.015f)));
        var caso = new System.Random(seme);
        for (int i = 0; i < 12; i++)
        {
            float x = (float)caso.NextDouble() * 2f - 1f, z = (float)caso.NextDouble() * 2f - 1f;
            if (i % 4 == 0) Forma(PrimitiveType.Sphere, "Teschio", t, new Vector3(x, 0.1f, z), Vector3.one * 0.25f, Osso);
            else Decoro("Osso", t, new Vector3(x, 0.05f, z), new Vector3(0.07f, 0.07f, 0.55f), Osso, new Vector3(0f, (float)caso.NextDouble() * 180f, 0f));
        }
        // bordo basso: non ci si cade dentro, ma si vede che è una buca
        Blocco("Bordo", t, new Vector3(0f, 0.15f, 1.3f), new Vector3(2.8f, 0.3f, 0.2f), Roccia);
        Blocco("Bordo", t, new Vector3(0f, 0.15f, -1.3f), new Vector3(2.8f, 0.3f, 0.2f), Roccia);
    }

    static void Buco(Transform genitore, Vector3 p)
    {
        var t = Vuoto("Buco nel muro", genitore, p).transform;
        var caso = new System.Random(3);
        for (int i = 0; i < 6; i++)
            Decoro("Macerie", t, new Vector3((float)caso.NextDouble() * 2f - 1f, 0.15f, (float)caso.NextDouble() * 2.4f - 1.2f), Vector3.one * (0.25f + (float)caso.NextDouble() * 0.3f), Roccia,
                new Vector3(0f, (float)caso.NextDouble() * 90f, 20f));
    }

    static void Funghi(Transform genitore, Vector3 p, int seme)
    {
        var t = Vuoto("Funghi che brillano", genitore, p + new Vector3(Casella * 0.3f, 0f, Casella * 0.3f)).transform;
        var caso = new System.Random(seme);
        for (int i = 0; i < 5; i++)
        {
            Vector3 q = new Vector3((float)caso.NextDouble() * 0.8f - 0.4f, 0f, (float)caso.NextDouble() * 0.8f - 0.4f);
            Decoro("Gambo", t, q + Vector3.up * 0.08f, new Vector3(0.04f, 0.16f, 0.04f), Osso);
            Forma(PrimitiveType.Sphere, "Cappello", t, q + Vector3.up * 0.18f, new Vector3(0.16f, 0.06f, 0.16f), Fungo);
        }
        Luce("Luce dei funghi", t, new Vector3(0f, 0.5f, 0f), new Color(0.35f, 0.6f, 1f), 4f, 0.7f);
    }

    // Parassita provvisorio: per ora un ratto gigante pallido (il ragno a sei zampe è da fare).
    static void ParassitaProvvisorio(Transform genitore, Vector3 p, int seme)
    {
        var r = new GameObject("Parassita (provvisorio)");
        r.transform.SetParent(genitore, false);
        r.transform.localPosition = p + Vector3.up * 0.3f;
        r.transform.localRotation = Quaternion.Euler(0f, (seme * 47) % 360, 0f);
        var box = r.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.05f, 0f);
        box.size = new Vector3(0.8f, 0.5f, 1f);
        Forma(PrimitiveType.Sphere, "Corpo", r.transform, new Vector3(0f, 0.1f, -0.1f), new Vector3(0.75f, 0.45f, 0.85f), Parassita);
        Forma(PrimitiveType.Sphere, "Testa", r.transform, new Vector3(0f, 0.08f, 0.45f), new Vector3(0.35f, 0.3f, 0.35f), Parassita);
        for (int i = 0; i < 6; i++)
        {
            float lato = i % 2 == 0 ? 1f : -1f;
            float z = -0.3f + (i / 2) * 0.3f;
            Decoro("Zampa", r.transform, new Vector3(lato * 0.45f, -0.05f, z), new Vector3(0.6f, 0.05f, 0.05f), Parassita, new Vector3(0f, lato * (20f - i * 8f), lato * -25f));
        }
        var b = r.AddComponent<global::Bersaglio>();
        b.attaccaIlGiocatore = false;
        Imposta(b, "vitaMassima", 30f);
        Imposta(b, "dannoAttacco", 8f);
        Imposta(b, "rinasce", false);
        Imposta(b, "spintaQuandoColpito", 0.6f);
        var ratto = r.AddComponent<RattoGigante>();
        Imposta(ratto, "intervalloAttacchi", 3f);
        Imposta(ratto, "ritardoPrimoAttacco", 1f + (seme % 5) * 0.6f);
        Imposta(ratto, "portataBalzo", 4f);
        Imposta(r.AddComponent<NomeTradotto>(), "chiave", "nem.parassita");
    }

    // Il Carceriere: orco grosso avvolto di catene, con la mazza (tubo). Numeri da Docs/gattabuia.md (fase 1).
    static Carceriere CreaCarceriere(Transform genitore, Vector3 p)
    {
        var o = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        o.name = "Carceriere";
        o.transform.SetParent(genitore, false);
        o.transform.localScale = Vector3.one * 1.5f;
        o.transform.localPosition = p + Vector3.up * 1.5f;
        o.transform.localRotation = Quaternion.LookRotation(Vector3.back);
        o.GetComponent<Renderer>().sharedMaterial = PelleCarceriere;
        var stat = o.AddComponent<Statistiche>();
        Imposta(stat, "armatura", 30f);
        var b = o.AddComponent<global::Bersaglio>();
        b.attaccaIlGiocatore = false;
        Imposta(b, "vitaMassima", 620f);
        Imposta(b, "dannoAttacco", 30f);
        Imposta(b, "intervalloAttacchi", 3f);
        Imposta(b, "preavviso", 0.9f);
        Imposta(b, "portataAttacco", 3.2f);
        Imposta(b, "rinasce", false);
        Imposta(b, "boss", true);
        Imposta(b, "spintaQuandoColpito", 0f);
        Imposta(b, "armaFigura", AspettoUmanoide.Arma.Mazza);
        var v = o.AddComponent<InseguimentoNemico>();
        Imposta(v, "distanzaVista", 16f);
        Imposta(v, "angoloVisivo", 200f);
        Imposta(v, "velocitaInseguimento", 3f);
        Imposta(v, "distanzaAttacco", 2.6f);
        Imposta(v, "secondiPerPerderlo", 8f);
        var c = o.AddComponent<Carceriere>();
        Imposta(o.AddComponent<NomeTradotto>(), "chiave", "nem.carceriere");
        // le catene avvolte sul petto (provvisorie)
        for (int i = 0; i < 3; i++)
            Decoro("Catena", o.transform, new Vector3(0f, -0.1f + i * 0.18f, 0f), new Vector3(1.05f, 0.05f, 1.05f), FerroNero, new Vector3(0f, i * 30f, 8f));
        return c;
    }

    static void Scalinata(Transform genitore, Vector3 p)
    {
        var t = Vuoto("Scalinata", genitore, p).transform;
        for (int i = 0; i < 6; i++)
            Blocco("Gradino", t, new Vector3(0f, 0.12f + i * 0.25f, -Casella / 2f + 0.25f + i * 0.5f), new Vector3(Casella, 0.25f, 0.5f), Lastre);
    }

    // Il portone in cima alla scalinata: due ante sui cardini e FineCapitolo davanti.
    static GameObject Portone(Transform genitore, Vector3 p, DatiOggetto chiave)
    {
        var t = Vuoto("Portone", genitore, p + Vector3.up * 1.5f).transform;
        var legno = Materiale("gattabuia-portone", new Color(0.2f, 0.13f, 0.08f));
        var ante = new Transform[2];
        for (int i = 0; i < 2; i++)
        {
            float lato = i == 0 ? -1f : 1f;
            var cardine = Vuoto(i == 0 ? "Anta sinistra" : "Anta destra", t, new Vector3(lato * Casella / 2f, 0f, Casella / 2f - 0.2f)).transform;
            Blocco("Legno", cardine, new Vector3(-lato * Casella / 4f, Altezza / 2f - 0.2f, 0f), new Vector3(Casella / 2f, Altezza - 0.4f, 0.25f), legno);
            Decoro("Fascia", cardine, new Vector3(-lato * Casella / 4f, 1f, -0.14f), new Vector3(Casella / 2f, 0.12f, 0.02f), FerroNero);
            Decoro("Fascia", cardine, new Vector3(-lato * Casella / 4f, 2.6f, -0.14f), new Vector3(Casella / 2f, 0.12f, 0.02f), FerroNero);
            ante[i] = cardine;
        }
        var fine = Vuoto("Apri il portone", t, new Vector3(0f, -1.5f, 0f)).AddComponent<FineCapitolo>();
        Imposta(fine, "chiave", chiave);
        Imposta(fine, "ante", ante);
        Luce("Luce da sotto il portone", t, new Vector3(0f, 0f, 1f), new Color(0.6f, 0.65f, 0.8f), 6f, 0.8f);
        return fine.gameObject;
    }
}
