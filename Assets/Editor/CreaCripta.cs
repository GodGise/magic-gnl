using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: crea la scena della cripta della prima guardia (quest 5, "La leggenda del lago"), una scena a
// parte che si carica entrando dal mausoleo del cimitero (CaricamentoZona). Richiesta di Lorenzo del 13 ottobre.
// La pianta è disegnata qui sotto a caselle di 3 metri (Mappa): si cambia la cripta cambiando i caratteri.
//   #  roccia (muro)              .  pavimento
//   A  arrivo (scendendo le scale) U  scala per uscire (E  Esci dalla cripta)
//   S  scheletro vivente           T  torcia sul muro più vicino
//   G  cancello (Porta)            L  leva che apre il cancello
//   Z  avviso "il cancello è chiuso" (davanti al cancello)
//   B  baule con il Medaglione della prima guardia
//   C  sarcofago                   O  mucchio di ossa
//   F  lunaria da raccogliere
// Il percorso: ingresso vuoto e silenzioso -> corridoio centrale con 4 scheletri sparsi (a ovest, a est, nell'ossario
// a sud-ovest e nel corridoio nord-est) -> cancello chiuso della sala della prima guardia -> leva nella stanza a
// nord-ovest -> sala con il sarcofago della guardia, la sua statua e il baule. Nella camera a nord-est due lunarie.
// La cripta sta lontano dal villaggio (X 3000), così le due scene caricate insieme non si toccano.
// Collegamenti della quest dentro la cripta (passano da PonteMissioni, che parla con Missioni del villaggio):
//   arrivo -> "Trova la tomba della prima guardia", suggerimento sugli scheletri;
//   davanti al cancello chiuso -> avviso, "Trova il modo di aprire il cancello", la bussola indica la leva;
//   leva -> "Apri il baule della prima guardia", la bussola indica il baule;
//   baule -> medaglione nello zaino, "Esci dalla cripta", la bussola indica la scala, poi il segnale q5.medaglione
//   (il villaggio lo riceve: vedi CreaPrologoQuest5).
// Salva la scena in Assets/Scenes/Cripta.unity e la aggiunge alle Build Settings. Se esiste già chiede se rifarla.
// Le scene aperte restano aperte (per esempio VillaggioLagoNero).
// Come si usa: menu "magic-gnl > Cripta: crea la scena della cripta". Non va montato su niente.
public static class CreaCripta
{
    public const string NomeScena = "Cripta";
    const string Percorso = "Assets/Scenes/Cripta.unity";
    const string PercorsoMedaglione = "Assets/Dati/Oggetti/Comuni/medaglione-della-prima-guardia.asset";
    const string PercorsoLunaria = "Assets/Dati/Oggetti/Missioni/lunaria.asset";
    const float Casella = 3f, Altezza = 4f;
    static readonly Vector3 Origine = new Vector3(3000f, 0f, 0f);

    static readonly string[] Mappa =
    {
        "###########################",
        "#.....###T.......T###....T#",
        "#TL...###.C..C..C.###.C.C.#",
        "#.....###....B....###....F#",
        "#.....###.........###F....#",
        "###.#####T.......T#####.###",
        "###.#####.........#####.###",
        "###.#########G#########S###",
        "###T#########.#########T###",
        "###.#########Z#########.###",
        "###.#########.#########.###",
        "###.T...S....T....S...T.###",
        "######.######.######.######",
        "####T...O####.####O...T####",
        "####.OS..####.####.....####",
        "####.....####.####...O.####",
        "##########.......##########",
        "##########T.....T##########",
        "##########...A...##########",
        "##########...U...##########",
        "###########################",
    };

    static int Righe => Mappa.Length;
    static int Colonne => Mappa[0].Length;
    static char Cella(int r, int c) => r < 0 || c < 0 || r >= Righe || c >= Colonne ? '#' : Mappa[r][c];
    static bool Pieno(int r, int c) => Cella(r, c) == '#';
    // Centro della casella (in coordinate della cripta: la riga 0 è a nord).
    static Vector3 Centro(int r, int c) => new Vector3(c * Casella, 0f, (Righe - 1 - r) * Casella);

    static Material Roccia => Materiale("cripta-roccia", new Color(0.24f, 0.23f, 0.22f));
    static Material Lastre => Materiale("cripta-lastre", new Color(0.2f, 0.19f, 0.18f));
    static Material PietraChiara => Materiale("cripta-pietra-chiara", new Color(0.42f, 0.4f, 0.36f));
    static Material Osso => Materiale("cripta-osso", new Color(0.82f, 0.78f, 0.66f));
    static Material FerroNero => Materiale("cripta-ferro", new Color(0.16f, 0.16f, 0.18f));
    static Material Bronzo => Materiale("cripta-bronzo", new Color(0.45f, 0.33f, 0.18f));

    [MenuItem("magic-gnl/Cripta: crea la scena della cripta")]
    static void Menu()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Percorso) != null &&
            !EditorUtility.DisplayDialog("Cripta", "La scena della cripta esiste già. Rifarla da capo? Le modifiche fatte a mano andranno perse.", "Rifai", "Annulla"))
            return;
        var aperta = SceneManager.GetSceneByPath(Percorso);
        if (aperta.IsValid() && aperta.isLoaded) EditorSceneManager.CloseScene(aperta, true);
        Crea();
        EditorUtility.DisplayDialog("Cripta", "Scena della cripta creata in " + Percorso + " e aggiunta alle Build Settings.\n"
            + "Ora, nella scena VillaggioLagoNero, usa \"magic-gnl > Prologo: prepara la quest 5\".", "Ok");
    }

    internal static void Crea()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        var prima = SceneManager.GetActiveScene();
        var scena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scena);

        var radice = new GameObject("Cripta").transform;
        radice.position = Origine;

        var ambiente = new GameObject("Ambiente della cripta").AddComponent<AmbienteZona>();
        ambiente.transform.SetParent(radice, false);
        ambiente.luceAmbiente = new Color(0.11f, 0.11f, 0.14f);
        ambiente.nebbia = new Color(0.012f, 0.012f, 0.02f);
        ambiente.densitaNebbia = 0.055f;
        Vuoto("Memoria della cripta", radice).AddComponent<MemoriaZona>();
        var ponte = Vuoto("Ponte verso le missioni", radice).AddComponent<PonteMissioni>();
        Imposta(ponte, "finitoSe", "q5.medaglione");

        Struttura(radice);
        var oggetti = Vuoto("Oggetti", radice).transform;
        var nemici = Vuoto("Scheletri", radice).transform;
        Porta cancello = null;
        global::Leva leva = null;
        global::Baule baule = null;
        Transform uscita = null, arrivo = null, zonaCancello = null;

        for (int r = 0; r < Righe; r++)
            for (int c = 0; c < Colonne; c++)
            {
                Vector3 p = Centro(r, c);
                switch (Cella(r, c))
                {
                    case 'A':
                        arrivo = Vuoto("Arrivo", radice, p).transform;
                        arrivo.localRotation = Quaternion.LookRotation(Vector3.forward);
                        break;
                    case 'U': uscita = Scala(oggetti, p).transform; break;
                    case 'T': Torcia(oggetti, r, c); break;
                    case 'S': Scheletro(nemici, p, r, c); break;
                    case 'G': cancello = Cancello(oggetti, p); break;
                    case 'Z': zonaCancello = Vuoto("Davanti al cancello", oggetti, p).transform; break;
                    case 'L': leva = Leva(oggetti, r, c); break;
                    case 'B': baule = Baule(oggetti, p); break;
                    case 'C': Sarcofago(oggetti, p, r == 2 && c == 13); break;
                    case 'O': Ossa(oggetti, p, r * 31 + c); break;
                    case 'F': Lunaria(oggetti, p); break;
                }
            }
        SalaDellaGuardia(oggetti);

        // camera più vicina in tutta la cripta (corridoi larghi 3 m, soffitto a 4 m)
        var zona = Vuoto("Zona camera (cripta)", radice, new Vector3((Colonne - 1) * Casella / 2f, 0f, (Righe - 1) * Casella / 2f)).AddComponent<ZonaCamera>();
        Imposta(zona, "dimensioni", new Vector3(Colonne * Casella, Altezza, Righe * Casella));
        Imposta(zona, "distanzaMassima", 4.2f);
        Imposta(zona, "inclinazioneMassima", 35f);

        // ---------- la quest dentro la cripta ----------
        var arrivoZona = Vuoto("All'arrivo", radice, arrivo != null ? arrivo.localPosition : Vector3.zero).AddComponent<ZonaArrivo>();
        Imposta(arrivoZona, "raggio", 3f);
        var allArrivo = Evento(arrivoZona, "allArrivo");
        Su(allArrivo, ponte.ImpostaObiettivo, "ob.q5.tomba");
        Su(allArrivo, ponte.Suggerisci, "tip.q5.scheletri");
        if (cancello != null) UnityEventTools.AddObjectPersistentListener<Transform>(allArrivo, ponte.ImpostaMeta, cancello.transform);

        if (zonaCancello != null && cancello != null)
        {
            var avviso = zonaCancello.gameObject.AddComponent<ZonaPortaChiusa>();
            Imposta(avviso, "porta", cancello);
            Imposta(avviso, "raggio", 3f);
            var chiuso = Evento(avviso, "davantiAllaPortaChiusa");
            Su(chiuso, ponte.Messaggio, "hud.cancello_chiuso");
            Su(chiuso, ponte.ImpostaObiettivo, "ob.q5.cancello");
            Su(chiuso, ponte.Suggerisci, "tip.q5.leva");
            if (leva != null) UnityEventTools.AddObjectPersistentListener<Transform>(chiuso, ponte.ImpostaMeta, leva.transform);
        }
        if (leva != null)
        {
            if (cancello != null) Imposta(leva, "porte", new[] { cancello });
            var tirata = Evento(leva, "quandoAbbassata");
            Su(tirata, ponte.ImpostaObiettivo, "ob.q5.baule");
            if (baule != null) UnityEventTools.AddObjectPersistentListener<Transform>(tirata, ponte.ImpostaMeta, baule.transform);
        }
        if (baule != null)
        {
            var aperto = Evento(baule, "quandoAperto");
            Su(aperto, ponte.ImpostaObiettivo, "ob.q5.esci");
            Su(aperto, ponte.Suggerisci, "tip.q5.medaglione");
            if (uscita != null) UnityEventTools.AddObjectPersistentListener<Transform>(aperto, ponte.ImpostaMeta, uscita);
            Su(aperto, ponte.Segnale, "q5.medaglione");   // per ultimo: dopo, il ponte non cambia più gli obiettivi
        }

        EditorSceneManager.SaveScene(scena, Percorso);
        AggiungiAlleBuildSettings();
        if (prima.IsValid()) SceneManager.SetActiveScene(prima);
        EditorSceneManager.CloseScene(scena, true);
        AssetDatabase.SaveAssets();
        Debug.Log("Cripta: scena creata in " + Percorso + " (" + Righe + "x" + Colonne + " caselle da " + Casella + " m).");
    }

    static void AggiungiAlleBuildSettings()
    {
        var elenco = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in elenco) if (s.path == Percorso) return;
        elenco.Add(new EditorBuildSettingsScene(Percorso, true));
        EditorBuildSettings.scenes = elenco.ToArray();
    }

    // ---------- pavimento, soffitto e muri ----------

    static void Struttura(Transform radice)
    {
        var struttura = Vuoto("Struttura", radice).transform;
        Vector3 meta = new Vector3((Colonne - 1) * Casella / 2f, 0f, (Righe - 1) * Casella / 2f);
        Blocco("Pavimento", struttura, meta + Vector3.down * 0.1f, new Vector3(Colonne * Casella, 0.2f, Righe * Casella), Lastre);
        Blocco("Soffitto", struttura, meta + Vector3.up * (Altezza + 0.15f), new Vector3(Colonne * Casella, 0.3f, Righe * Casella), Roccia);
        // muri: le caselle di roccia che toccano il pavimento, unite riga per riga in blocchi lunghi
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
        // archi di pietra chiara sopra i passaggi stretti fra stanze e corridoi (solo decorazione)
        for (int r = 0; r < Righe; r++)
            for (int c = 0; c < Colonne; c++)
            {
                if (Pieno(r, c)) continue;
                bool verticale = Pieno(r, c - 1) && Pieno(r, c + 1) && (!Pieno(r - 1, c) && !Pieno(r + 1, c));
                bool orizzontale = Pieno(r - 1, c) && Pieno(r + 1, c) && (!Pieno(r, c - 1) && !Pieno(r, c + 1));
                bool imbocco = (verticale && (Larga(r - 1, c) || Larga(r + 1, c))) || (orizzontale && (Larga(r, c - 1) || Larga(r, c + 1)));
                if (!imbocco) continue;
                Vector3 p = Centro(r, c);
                Decoro("Arco", struttura, p + Vector3.up * (Altezza - 0.35f), verticale ? new Vector3(Casella, 0.7f, 0.5f) : new Vector3(0.5f, 0.7f, Casella), PietraChiara);
            }
    }

    // Casella di roccia accanto al pavimento (anche in diagonale): lì serve un muro.
    static bool Muro(int r, int c)
    {
        if (!Pieno(r, c)) return false;
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
                if (!Pieno(r + dr, c + dc)) return true;
        return false;
    }

    // Casella che fa parte di una stanza (pavimento con almeno 3 vicini di pavimento).
    static bool Larga(int r, int c)
    {
        if (Pieno(r, c)) return false;
        int liberi = 0;
        if (!Pieno(r - 1, c)) liberi++;
        if (!Pieno(r + 1, c)) liberi++;
        if (!Pieno(r, c - 1)) liberi++;
        if (!Pieno(r, c + 1)) liberi++;
        return liberi >= 3;
    }

    // ---------- pezzi ----------

    static GameObject Scala(Transform genitore, Vector3 p)
    {
        // gradini che salgono verso sud, contro il muro, e l'uscita
        var scala = Vuoto("Scala dell'uscita", genitore, p);
        for (int i = 0; i < 5; i++)
            Blocco("Gradino", scala.transform, new Vector3(0f, 0.2f + i * 0.4f, 0.2f - i * 0.35f) + Vector3.forward * -0.1f, new Vector3(2.4f, 0.4f, 0.5f), PietraChiara);
        Luce("Luce dall'alto", scala.transform, new Vector3(0f, 3.5f, -1.2f), new Color(0.55f, 0.6f, 0.75f), 6f, 0.9f);
        var uscita = scala.AddComponent<PassaggioScena>();
        Imposta(uscita, "verso", PassaggioScena.Verso.Esci);
        Imposta(uscita, "scena", NomeScena);
        Imposta(uscita, "chiaveAzione", "hud.esci_cripta");
        Imposta(uscita, "raggio", 2.2f);
        return scala;
    }

    static void Torcia(Transform genitore, int r, int c)
    {
        // sul muro vicino: prima nord, poi ovest, est, sud
        Vector3 verso = Pieno(r - 1, c) ? Vector3.forward : Pieno(r, c - 1) ? Vector3.left : Pieno(r, c + 1) ? Vector3.right : Vector3.back;
        Vector3 p = Centro(r, c) + verso * (Casella / 2f - 0.15f) + Vector3.up * 2.4f;
        var torcia = Vuoto("Torcia", genitore, p);
        Decoro("Supporto", torcia.transform, -verso * 0.12f + Vector3.down * 0.15f, new Vector3(0.12f, 0.35f, 0.12f), FerroNero);
        Forma(PrimitiveType.Sphere, "Fiamma", torcia.transform, -verso * 0.15f + Vector3.up * 0.1f, new Vector3(0.18f, 0.28f, 0.18f), Fuoco);
        var luce = Vuoto("Luce", torcia.transform, -verso * 0.6f);
        var l = luce.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.62f, 0.32f);
        l.range = 10f;
        l.intensity = 1.6f;
        var t = luce.AddComponent<global::Torcia>();
        Imposta(t, "intensitaMassima", 1.6f);
    }

    static void Scheletro(Transform genitore, Vector3 p, int r, int c)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        s.name = "Scheletro vivente";
        s.transform.SetParent(genitore, false);
        s.transform.localPosition = p + Vector3.up;
        // guarda verso l'ingresso della cripta (a sud)
        s.transform.localRotation = Quaternion.LookRotation(Vector3.back);
        s.GetComponent<Renderer>().sharedMaterial = Osso;
        var b = s.AddComponent<global::Bersaglio>();
        b.attaccaIlGiocatore = false;
        Imposta(b, "vitaMassima", 45f);
        Imposta(b, "rinasce", false);
        Imposta(b, "dannoAttacco", 12f);
        Imposta(b, "intervalloAttacchi", 1.3f);
        Imposta(b, "preavviso", 0.45f);
        Imposta(b, "portataAttacco", 2f);
        Imposta(b, "spintaQuandoColpito", 0.45f);
        Imposta(b, "armaFigura", AspettoUmanoide.Arma.Spada);
        s.AddComponent<ScheletroVivente>();
        Imposta(s.AddComponent<NomeTradotto>(), "chiave", "nem.scheletro");
    }

    static Porta Cancello(Transform genitore, Vector3 p)
    {
        // grata di ferro nel vano (sul lato nord della casella, verso la sala)
        var g = Forma(PrimitiveType.Cube, "Cancello", genitore, p + new Vector3(0f, Altezza / 2f, Casella / 2f - 0.2f), new Vector3(Casella, Altezza, 0.25f), FerroNero, true);
        for (int i = -3; i <= 3; i++)
            Decoro("Sbarra", g.transform, new Vector3(i * 0.13f, 0f, -0.8f), new Vector3(0.025f, 1f, 0.6f), Bronzo);
        Decoro("Traversa", g.transform, new Vector3(0f, 0.25f, -0.8f), new Vector3(1f, 0.03f, 0.6f), Bronzo);
        var porta = g.AddComponent<Porta>();
        Imposta(porta, "durataApertura", 2.2f);
        return porta;
    }

    static global::Leva Leva(Transform genitore, int r, int c)
    {
        // sul muro a ovest, rivolta verso est (verso la stanza)
        var o = Vuoto("Leva del cancello", genitore, Centro(r, c) + Vector3.left * 0.4f);
        o.transform.localRotation = Quaternion.LookRotation(Vector3.right);
        Decoro("Piastra", o.transform, new Vector3(0f, 1.3f, -0.6f), new Vector3(0.9f, 1.4f, 0.12f), Bronzo);
        var leva = o.AddComponent<global::Leva>();
        Imposta(leva, "raggioInterazione", 2.2f);
        return leva;
    }

    static global::Baule Baule(Transform genitore, Vector3 p)
    {
        var o = Vuoto("Baule della prima guardia", genitore, p + Vector3.back * 0.3f);
        var b = o.AddComponent<global::Baule>();
        Imposta(b, "contenuto", global::Baule.Contenuto.Oggetto);
        var medaglione = AssetDatabase.LoadAssetAtPath<DatiOggetto>(PercorsoMedaglione);
        if (medaglione == null) Debug.LogWarning("Cripta: manca " + PercorsoMedaglione + ".");
        Imposta(b, "oggetto", medaglione);
        Imposta(b, "raggioInterazione", 2.2f);
        Luce("Luce sul baule", o.transform, new Vector3(0f, 3f, 0f), new Color(1f, 0.82f, 0.5f), 5f, 0.8f);
        return b;
    }

    static void Sarcofago(Transform genitore, Vector3 p, bool guardia)
    {
        var s = Vuoto(guardia ? "Sarcofago della prima guardia" : "Sarcofago", genitore, p);
        float l = guardia ? 2.8f : 2.3f;
        Blocco("Cassa", s.transform, new Vector3(0f, 0.5f, 0f), new Vector3(1.3f, 1f, l), PietraChiara);
        Decoro("Coperchio", s.transform, new Vector3(0f, 1.06f, 0f), new Vector3(1.45f, 0.14f, l + 0.15f), Roccia);
        if (!guardia) return;
        // la figura scolpita della guardia distesa sul coperchio, con la spada sul petto
        Decoro("Figura", s.transform, new Vector3(0f, 1.25f, 0.1f), new Vector3(0.6f, 0.25f, 2.1f), PietraChiara);
        Decoro("Spada di pietra", s.transform, new Vector3(0f, 1.42f, 0.2f), new Vector3(0.12f, 0.08f, 1.5f), Bronzo);
    }

    static void Ossa(Transform genitore, Vector3 p, int seme)
    {
        var o = Vuoto("Ossa", genitore, p);
        var caso = new System.Random(seme);
        for (int i = 0; i < 9; i++)
        {
            float x = (float)caso.NextDouble() * 1.6f - 0.8f, z = (float)caso.NextDouble() * 1.6f - 0.8f;
            if (i % 3 == 0) Forma(PrimitiveType.Sphere, "Teschio", o.transform, new Vector3(x, 0.15f, z), Vector3.one * 0.3f, Osso);
            else Decoro("Osso", o.transform, new Vector3(x, 0.06f, z), new Vector3(0.08f, 0.08f, 0.6f), Osso, new Vector3(0f, (float)caso.NextDouble() * 180f, 0f));
        }
    }

    static void Lunaria(Transform genitore, Vector3 p)
    {
        var o = Vuoto("Lunaria", genitore, p);
        var r = o.AddComponent<OggettoRaccoglibile>();
        Imposta(r, "oggetto", AssetDatabase.LoadAssetAtPath<DatiOggetto>(PercorsoLunaria));
        Imposta(r, "distanza", 2f);
        Luce("Bagliore della lunaria", o.transform, new Vector3(0f, 0.6f, 0f), new Color(0.75f, 0.8f, 1f), 3f, 0.6f);
    }

    // La sala: due colonne per lato e la statua della guardia contro il muro nord, sopra il sarcofago.
    static void SalaDellaGuardia(Transform genitore)
    {
        var sala = Vuoto("Sala della prima guardia", genitore).transform;
        foreach (int c in new[] { 10, 16 })
            foreach (int r in new[] { 4, 6 })
                Blocco("Colonna", sala, Centro(r, c) + Vector3.up * Altezza / 2f, new Vector3(0.8f, Altezza, 0.8f), PietraChiara);
        Vector3 statua = Centro(1, 13) + Vector3.forward * 0.6f;
        Blocco("Basamento", sala, statua + Vector3.up * 0.3f, new Vector3(1.6f, 0.6f, 1f), Roccia);
        Blocco("Statua della prima guardia", sala, statua + Vector3.up * 1.7f, new Vector3(0.8f, 2.2f, 0.6f), PietraChiara);
        Forma(PrimitiveType.Sphere, "Testa della statua", sala, statua + Vector3.up * 3.05f, Vector3.one * 0.45f, PietraChiara);
        Decoro("Lancia della statua", sala, statua + new Vector3(0.55f, 1.9f, -0.2f), new Vector3(0.08f, 3.2f, 0.08f), Bronzo);
        Luce("Luce sulla statua", sala, statua + new Vector3(0f, 3.4f, -1.5f), new Color(0.6f, 0.7f, 1f), 7f, 1.1f);
    }
}
