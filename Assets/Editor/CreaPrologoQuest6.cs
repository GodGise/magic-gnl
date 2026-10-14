using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: prepara nella scena VillaggioLagoNero (con le quest 1-5 già preparate) la quest 6 "Una sera in
// taverna" e la razzia degli orchi (richiesta di Lorenzo del 13 ottobre), in un gruppo nuovo "Prologo - quest 6 e razzia".
// La taverna è una scena a parte ("magic-gnl > Taverna: crea la scena della taverna"; se manca la crea questo comando).
//   - quest 6: finite le prime 5 quest (OrarioPrologo, "All'ultima quest", al tramonto) -> "Al tramonto, raggiungi la
//     taverna di Hobb", la bussola indica la porta della taverna, "E  Entra nella taverna" (PassaggioScena);
//     dentro: Hobb, Maren con Tobin, i tamburi (vedi CreaTaverna) -> uscendo (segnale uscita.Taverna) è già notte
//     (OrarioPrologo > NotteSubito) e parte la razzia ("Alla razzia" -> RazziaVillaggio > Inizia). La taverna non si
//     riapre più.
//   - razzia (RazziaVillaggio): prima ondata di 10 orchi sparsi per il villaggio, davanti alle case, che le distruggono
//     (RazziatoreOrco: 3 danno fuoco, 3 sfondano porte, 2 colpiscono le pareti, 2 prendono a calci); al 65% abbattuto
//     ritirata; 1 minuto di tamburi; seconda ondata di 16 orchi (5 corazzati con l'ascia); al 70% abbattuto e a 20 m
//     dalla piazza, terremoto e orco enorme in piazza.
//   - tutte le case (tranne la taverna) diventano CasaInRovina; all'inizio della razzia una dozzina brucia già, altre
//     sono danneggiate, un paio sono già crollate. Nessuno degli orchi della prima ondata è a meno di 25 m dalla porta
//     della taverna, così all'uscita il giocatore ha un attimo per guardarsi intorno.
//   - quando parte la razzia gli abitanti (Maren, Tobin, Brannoc, Edric, Hobb, Ilse) spariscono dalle strade.
// Materiali e immagine delle particelle (fuoco, fumo, polvere, legno annerito) in Assets/Segnaposto/Razzia.
// Non salva la scena: dopo il comando si guarda il risultato e si salva con Ctrl+S.
// Come si usa: aprire Assets/Scenes/VillaggioLagoNero.unity, poi menu "magic-gnl > Prologo: prepara la quest 6 e la
// razzia". Rifacendolo, il gruppo vecchio si cancella e si rifà (con posti e case scelti allo stesso modo).
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class CreaPrologoQuest6
{
    const string NomeGruppo = "Prologo - quest 6 e razzia";
    const string Cartella = "Assets/Segnaposto/Razzia";
    const float S = DatiVillaggioLagoNero.Scala;
    static readonly Vector3 CentroPiazza = new Vector3(96f * S, 0f, 84f * S);

    static readonly string[] Abitanti = { "Maren", "Tobin", "Brannoc", "Edric", "Hobb", "Ilse" };

    [MenuItem("magic-gnl/Prologo: prepara la quest 6 e la razzia")]
    static void Prepara()
    {
        Scene scena = SceneManager.GetActiveScene();
        var missioniOggetto = Trova(scena, "Missioni");
        var villaggio = Trova(scena, "Villaggio");
        var taverna = Trova(scena, "Taverna");
        var orario = Object.FindFirstObjectByType<OrarioPrologo>();
        if (scena.name != "VillaggioLagoNero" || missioniOggetto == null || villaggio == null || taverna == null || orario == null)
        {
            EditorUtility.DisplayDialog("Prologo", "Apri la scena VillaggioLagoNero e prepara prima le quest da 1 a 5.", "Ok");
            return;
        }
        GameObject vecchio = Trova(scena, NomeGruppo);
        if (vecchio != null)
        {
            if (!EditorUtility.DisplayDialog("Prologo", "Il gruppo \"" + NomeGruppo + "\" esiste già. Rifarlo da capo? Le modifiche fatte a mano a quel gruppo andranno perse.", "Rifai", "Annulla")) return;
            Undo.DestroyObjectImmediate(vecchio);
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/" + CreaTaverna.NomeScena + ".unity") == null) CreaTaverna.Crea();

        var missioni = missioniOggetto.GetComponent<Missioni>();
        var gruppo = new GameObject(NomeGruppo).transform;
        Undo.RegisterCreatedObjectUndo(gruppo.gameObject, "Prologo quest 6");
        var materiali = PreparaMateriali();
        var caso = new System.Random(6);

        // ---------- quest 6: la porta della taverna ----------
        Porta(scena, "Taverna", out Vector3 punto, out Vector3 fuori);
        var ingresso = Undo.AddComponent<PassaggioScena>(Vuoto("Ingresso della taverna", gruppo, punto + fuori * 0.8f));
        Imposta(ingresso, "verso", PassaggioScena.Verso.Entra);
        Imposta(ingresso, "scena", CreaTaverna.NomeScena);
        Imposta(ingresso, "chiaveTitolo", "zona.taverna");
        Imposta(ingresso, "chiaveAzione", "hud.entra_taverna");
        Imposta(ingresso, "raggio", 2.2f);
        Imposta(ingresso, "nascondiDentro", new[] { villaggio });
        ingresso.enabled = false;

        var allUltima = Evento(orario, "allUltimaQuest");
        Svuota(allUltima);
        Su(allUltima, Abilita(ingresso), true);
        Su(allUltima, missioni.ImpostaObiettivo, "ob.q6.taverna");
        Meta(allUltima, ingresso.gameObject);
        EditorUtility.SetDirty(orario);

        var uscito = Undo.AddComponent<AscoltaSegnale>(Vuoto("Attesa: uscita dalla taverna", gruppo));
        Imposta(uscito, "parola", "uscita." + CreaTaverna.NomeScena);
        var allUscita = Evento(uscito, "quandoArriva");
        Su(allUscita, Abilita(ingresso), false);
        Su(allUscita, missioni.TogliObiettivo);
        Su(allUscita, orario.NotteSubito);

        // ---------- le case ----------
        var case_ = new List<Transform>();
        foreach (var radice in scena.GetRootGameObjects())
            foreach (var t in radice.GetComponentsInChildren<Transform>(true))
            {
                if (t.parent == null || !t.gameObject.activeInHierarchy) continue;
                bool casa = t.Find("Porta") != null && (t.name.StartsWith("casa-blocco") || (t.parent.name == "Edifici speciali" && t.name != "Taverna"));
                if (casa) case_.Add(t);
            }
        var rovine = new List<CasaInRovina>();
        foreach (var t in case_)
        {
            var c = t.GetComponent<CasaInRovina>();
            if (c == null) c = Undo.AddComponent<CasaInRovina>(t.gameObject);
            Imposta(c, "statoAllaRazzia", CasaInRovina.Stato.Intatta);
            Imposta(c, "materialeFuoco", materiali.fuoco);
            Imposta(c, "materialeFumo", materiali.fumo);
            Imposta(c, "materialeBruciato", materiali.bruciato);
            Imposta(c, "materialeAssi", materiali.assi);
            Imposta(c, "secondiPerBruciare", 80f + (float)caso.NextDouble() * 60f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(c);
            rovine.Add(c);
        }

        // ---------- gli orchi ----------
        Physics.SyncTransforms();
        Vector3 portaTaverna = punto;
        var usate = new HashSet<Transform>();
        var razzia = Undo.AddComponent<RazziaVillaggio>(Vuoto("Razzia", gruppo));
        var prima = Vuoto("Prima ondata", razzia.transform).transform;
        var seconda = Vuoto("Seconda ondata", razzia.transform).transform;

        RazziatoreOrco.Azione[] azioniPrima =
        {
            RazziatoreOrco.Azione.DareFuoco, RazziatoreOrco.Azione.SfondarePorta, RazziatoreOrco.Azione.ColpireParete,
            RazziatoreOrco.Azione.Calciare, RazziatoreOrco.Azione.DareFuoco, RazziatoreOrco.Azione.SfondarePorta,
            RazziatoreOrco.Azione.ColpireParete, RazziatoreOrco.Azione.Calciare, RazziatoreOrco.Azione.DareFuoco,
            RazziatoreOrco.Azione.SfondarePorta
        };
        var postiPrima = ScegliCase(case_, usate, azioniPrima.Length, 18f, 25f, portaTaverna);
        for (int i = 0; i < postiPrima.Count; i++)
            Orco(prima, "Orco della razzia " + (i + 1), postiPrima[i], azioniPrima[i], false);

        // seconda ondata: 16, i corazzati colpiscono le pareti con l'ascia
        RazziatoreOrco.Azione[] azioniSeconda =
        {
            RazziatoreOrco.Azione.ColpireParete, RazziatoreOrco.Azione.DareFuoco, RazziatoreOrco.Azione.SfondarePorta, RazziatoreOrco.Azione.Calciare,
            RazziatoreOrco.Azione.ColpireParete, RazziatoreOrco.Azione.DareFuoco, RazziatoreOrco.Azione.SfondarePorta, RazziatoreOrco.Azione.Calciare,
            RazziatoreOrco.Azione.ColpireParete, RazziatoreOrco.Azione.DareFuoco, RazziatoreOrco.Azione.SfondarePorta, RazziatoreOrco.Azione.Calciare,
            RazziatoreOrco.Azione.ColpireParete, RazziatoreOrco.Azione.DareFuoco, RazziatoreOrco.Azione.SfondarePorta, RazziatoreOrco.Azione.ColpireParete
        };
        var postiSeconda = ScegliCase(case_, usate, azioniSeconda.Length, 12f, 14f, portaTaverna);
        int corazzati = 0;
        for (int i = 0; i < postiSeconda.Count; i++)
        {
            bool corazzato = azioniSeconda[i] == RazziatoreOrco.Azione.ColpireParete && corazzati < 5;
            if (corazzato) corazzati++;
            Orco(seconda, (corazzato ? "Orco corazzato " : "Orco della seconda ondata ") + (i + 1), postiSeconda[i], azioniSeconda[i], corazzato);
        }
        prima.gameObject.SetActive(false);
        seconda.gameObject.SetActive(false);

        // le case già rovinate quando si esce dalla taverna (non quelle degli orchi della prima ondata, che ci lavorano)
        var libere = new List<CasaInRovina>();
        foreach (var c in rovine)
            if (!usate.Contains(c.transform) && Orizzontale(c.transform.position, portaTaverna) > 12f) libere.Add(c);
        Mescola(libere, caso);
        for (int i = 0; i < libere.Count && i < 20; i++)
        {
            var stato = i < 11 ? CasaInRovina.Stato.InFiamme : i < 18 ? CasaInRovina.Stato.Danneggiata : CasaInRovina.Stato.Distrutta;
            Imposta(libere[i], "statoAllaRazzia", stato);
            PrefabUtility.RecordPrefabInstancePropertyModifications(libere[i]);
        }
        // chi colpisce le pareti trova la casa già un po' rovinata
        foreach (var t in postiPrima)
        {
            var c = t.GetComponent<CasaInRovina>();
            var orco = TrovaOrco(prima, t);
            if (c != null && orco != null && orco.AzioneAttuale == RazziatoreOrco.Azione.ColpireParete) { Imposta(c, "statoAllaRazzia", CasaInRovina.Stato.Danneggiata); PrefabUtility.RecordPrefabInstancePropertyModifications(c); }
        }

        // ---------- piazza, ritirata, orco enorme ----------
        var piazza = Vuoto("Centro della piazza", razzia.transform, CentroPiazza).transform;
        float L = 210f * S, P = 170f * S;
        var ritirata = Vuoto("Punti di ritirata", razzia.transform).transform;
        var punti = new[]
        {
            Vuoto("Ritirata ovest (arrivo degli orchi)", ritirata, new Vector3(4f, 0f, 58.5f * S)).transform,
            Vuoto("Ritirata nord-ovest", ritirata, new Vector3(8f, 0f, P * 0.82f)).transform,
            Vuoto("Ritirata nord", ritirata, new Vector3(L * 0.42f, 0f, P - 8f)).transform,
            Vuoto("Ritirata sud", ritirata, new Vector3(L * 0.42f, 0f, 8f)).transform,
        };
        var enorme = CreaOrcoSgherro.MettiEnorme("Orco enorme", CentroPiazza, Vector3.back, razzia.transform);
        enorme.SetActive(false);
        PrefabUtility.RecordPrefabInstancePropertyModifications(enorme);
        PrefabUtility.RecordPrefabInstancePropertyModifications(enorme.transform);

        Imposta(razzia, "primaOndata", prima);
        Imposta(razzia, "secondaOndata", seconda);
        Imposta(razzia, "orcoEnorme", enorme);
        Imposta(razzia, "piazza", piazza);
        Imposta(razzia, "puntiRitirata", punti);
        Imposta(razzia, "materialePolvere", materiali.polvere);
        var abitanti = new List<GameObject>();
        foreach (string nome in Abitanti)
        {
            var o = Trova(scena, nome);
            if (o != null) abitanti.Add(o);
        }
        Imposta(razzia, "nascondiAllaRazzia", abitanti.ToArray());

        var allaRazzia = Evento(orario, "allaRazzia");
        Svuota(allaRazzia);
        Su(allaRazzia, razzia.Inizia);
        EditorUtility.SetDirty(orario);

        EditorSceneManager.MarkSceneDirty(scena);
        Selection.activeGameObject = gruppo.gameObject;
        Debug.Log("Prologo: quest 6 e razzia preparate nel gruppo \"" + NomeGruppo + "\" (" + postiPrima.Count + " + " + postiSeconda.Count
            + " orchi, " + rovine.Count + " case che si possono rovinare). Salva la scena (Ctrl+S) e Play.");
    }

    // ---------- orchi ----------

    static void Orco(Transform ondata, string nome, Transform casa, RazziatoreOrco.Azione azione, bool corazzato)
    {
        var c = casa.GetComponent<CasaInRovina>();
        Davanti(casa, out Vector3 porta, out Vector3 fuori);
        Vector3 piedi = porta + fuori * 1.5f;
        GameObject orco = corazzato
            ? CreaOrcoSgherro.MettiCorazzato(nome, piedi, -fuori, ondata)
            : CreaOrcoSgherro.Metti(nome, piedi, -fuori, ondata);
        Undo.RegisterCreatedObjectUndo(orco, "Prologo quest 6");
        PrefabUtility.RecordPrefabInstancePropertyModifications(orco.transform);
        var r = orco.GetComponent<RazziatoreOrco>();
        if (r == null) r = Undo.AddComponent<RazziatoreOrco>(orco);
        Imposta(r, "casa", c);
        Imposta(r, "azione", azione);
        if (!corazzato && orco.GetComponent<NomeTradotto>() == null) Imposta(Undo.AddComponent<NomeTradotto>(orco), "chiave", "nem.orco");
        // spalle alla strada: vede chi gli passa accanto ma non chi arriva da dietro (si può giustiziare)
        var v = orco.GetComponent<InseguimentoNemico>();
        Imposta(v, "guardaIntorno", 0f);
        Imposta(v, "distanzaVista", 16f);
        Imposta(v, "angoloVisivo", 150f);
        // sono copie di un prefab: le modifiche vanno segnate, se no Unity le perde salvando
        foreach (var comp in orco.GetComponents<Component>()) PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
    }

    static RazziatoreOrco TrovaOrco(Transform ondata, Transform casa)
    {
        foreach (var r in ondata.GetComponentsInChildren<RazziatoreOrco>(true))
            if (r.Casa != null && r.Casa.transform == casa) return r;
        return null;
    }

    // Case sparse per il villaggio, ognuna con un posto libero davanti alla porta: si parte dalla più vicina alla
    // piazza e ogni volta si prende la più lontana da quelle già scelte (almeno "spazio" metri fra due), lontano dalla
    // porta della taverna.
    static List<Transform> ScegliCase(List<Transform> case_, HashSet<Transform> usate, int quante, float spazio, float lontanoTaverna, Vector3 taverna)
    {
        var candidate = new List<Transform>();
        foreach (var t in case_)
        {
            if (usate.Contains(t) || t.GetComponent<CasaInRovina>() == null) continue;
            Davanti(t, out Vector3 porta, out Vector3 fuori);
            Vector3 p = porta + fuori * 1.5f;
            if (Orizzontale(p, taverna) < lontanoTaverna) continue;
            if (Physics.CheckCapsule(p + Vector3.up * 0.5f, p + Vector3.up * 1.7f, 0.45f, ~0, QueryTriggerInteraction.Ignore)) continue;
            candidate.Add(t);
        }
        var scelte = new List<Transform>();
        while (scelte.Count < quante && candidate.Count > 0)
        {
            Transform migliore = null;
            float punteggio = float.MinValue;
            foreach (var t in candidate)
            {
                float vicina = float.MaxValue;
                foreach (var s in scelte) vicina = Mathf.Min(vicina, Orizzontale(s.position, t.position));
                foreach (var u in usate) vicina = Mathf.Min(vicina, Orizzontale(u.position, t.position) * 1.6f);
                // la prima: la più vicina alla piazza; le altre: le più lontane dalle già scelte, senza allontanarsi troppo dalla piazza
                float s1 = scelte.Count == 0 && usate.Count == 0 ? -Orizzontale(t.position, CentroPiazza) : Mathf.Min(vicina, 60f) - Orizzontale(t.position, CentroPiazza) * 0.15f;
                if (scelte.Count > 0 && vicina < spazio) continue;
                if (s1 > punteggio) { punteggio = s1; migliore = t; }
            }
            if (migliore == null) break;
            scelte.Add(migliore);
            candidate.Remove(migliore);
        }
        foreach (var s in scelte) usate.Add(s);
        return scelte;
    }

    // Davanti alla porta della casa (a terra) e verso fuori: come CasaInRovina, ma in modifica.
    static void Davanti(Transform casa, out Vector3 punto, out Vector3 fuori)
    {
        Transform porta = casa.Find("Porta");
        Vector3 centro = casa.position;
        centro.y = 0f;
        if (porta != null && porta.gameObject.activeSelf)
        {
            punto = porta.position;
            punto.y = 0f;
            fuori = (punto - centro);
            fuori = fuori.sqrMagnitude > 0.01f ? fuori.normalized : -casa.forward;
        }
        else
        {
            fuori = -casa.forward;
            fuori.y = 0f;
            fuori.Normalize();
            punto = centro + fuori * casa.lossyScale.z * 0.5f;
        }
    }

    static float Orizzontale(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    static void Mescola<T>(List<T> elenco, System.Random caso)
    {
        for (int i = elenco.Count - 1; i > 0; i--)
        {
            int j = caso.Next(i + 1);
            (elenco[i], elenco[j]) = (elenco[j], elenco[i]);
        }
    }

    static void Svuota(UnityEvent e)
    {
        for (int i = e.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(e, i);
    }

    // ---------- materiali ----------

    struct Materiali { public Material fuoco, fumo, polvere, bruciato, assi; }

    static Materiali PreparaMateriali()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Segnaposto")) AssetDatabase.CreateFolder("Assets", "Segnaposto");
        if (!AssetDatabase.IsValidFolder(Cartella)) AssetDatabase.CreateFolder("Assets/Segnaposto", "Razzia");
        Texture2D macchia = Macchia();
        return new Materiali
        {
            fuoco = Particelle("fuoco-particelle", "Legacy Shaders/Particles/Additive", macchia, new Color(0.75f, 0.55f, 0.4f, 0.6f)),
            fumo = Particelle("fumo-particelle", "Legacy Shaders/Particles/Alpha Blended", macchia, new Color(0.5f, 0.5f, 0.5f, 0.5f)),
            polvere = Particelle("polvere-particelle", "Legacy Shaders/Particles/Alpha Blended", macchia, new Color(0.55f, 0.5f, 0.45f, 0.5f)),
            bruciato = Solido("legno-annerito", new Color(0.09f, 0.075f, 0.065f)),
            assi = Solido("assi-rotte", new Color(0.33f, 0.23f, 0.14f)),
        };
    }

    // Un'immagine tonda e sfumata da 64 px per fuoco, fumo e polvere (fatta qui, niente di copiato).
    static Texture2D Macchia()
    {
        string percorso = Cartella + "/particella.png";
        var esistente = AssetDatabase.LoadAssetAtPath<Texture2D>(percorso);
        if (esistente != null) return esistente;
        const int n = 64;
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                a = a * a * (3f - 2f * a);
                t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        t.Apply();
        File.WriteAllBytes(percorso, t.EncodeToPNG());
        Object.DestroyImmediate(t);
        AssetDatabase.ImportAsset(percorso);
        var importatore = (TextureImporter)AssetImporter.GetAtPath(percorso);
        importatore.alphaIsTransparency = true;
        importatore.wrapMode = TextureWrapMode.Clamp;
        importatore.mipmapEnabled = false;
        importatore.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(percorso);
    }

    static Material Particelle(string nome, string shader, Texture2D immagine, Color tinta)
    {
        string percorso = Cartella + "/" + nome + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (m != null) return m;
        var s = Shader.Find(shader);
        if (s == null) s = Shader.Find("Particles/Standard Unlit");
        m = new Material(s) { mainTexture = immagine };
        if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", tinta);
        AssetDatabase.CreateAsset(m, percorso);
        return m;
    }

    static Material Solido(string nome, Color colore)
    {
        string percorso = Cartella + "/" + nome + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (m != null) return m;
        m = new Material(Shader.Find("Standard")) { color = colore };
        m.SetFloat("_Glossiness", 0.02f);
        AssetDatabase.CreateAsset(m, percorso);
        return m;
    }
}
