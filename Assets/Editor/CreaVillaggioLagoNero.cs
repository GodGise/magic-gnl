using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Strumento dell'editor: costruisce il blockout di Villaggio Lago Nero (versione intatta) in una scena nuova.
// Come si usa: menu in alto "magic-gnl > Crea Villaggio Lago Nero (blockout)". Crea e salva
// Assets/Scenes/VillaggioLagoNero.unity. Si usa UNA volta: dopo, la scena si modifica a mano come ZonaProva.
// Se la scena esiste già chiede conferma, perché ricrearla cancella le modifiche fatte a mano.
//
// Cosa costruisce, seguendo la mappa vista dall'alto di Giuseppe allargata del 50% (DatiVillaggioLagoNero.Scala):
//   - Terreno di 315 x 255 metri, con il Lago Nero a est (più basso di 30 cm), la riva e il molo con tre barche;
//   - Strade (principali 4,4 m, vicoli 2,9 m) e la piazzetta di 48 m con il pozzo al centro;
//   - Edifici speciali (Mercante, Taverna, Fabbro, Erborista, Tempio, Casa dell'eroe) e 71 case, molte più grandi
//     della mappa e alcune come sulla mappa, tutte copie del prefab "casa-blocco" con un cubetto scuro dove sta la porta;
//   - Cimitero con recinto e lapidi, orti, bosco ai bordi (cilindri e sfere), torce, confini invisibili;
//   - MappaGuida: l'immagine della mappa stesa sul terreno, spenta. Accendendola (casella accanto al nome
//     nell'Inspector) si confronta la scena con la mappa;
//   - Giocatore davanti alla porta della casa dell'eroe, un orco di prova in piazzetta che insegue solo dopo averti visto,
//     due segnalini colorati (proposte da confermare con Giuseppe): rosso dove arrivano gli orchi, viola per lo scontro.
// I dati delle posizioni sono in DatiVillaggioLagoNero.cs. Materiali, prefab e mappa stanno in Assets/Segnaposto/Villaggio.
// Tutto è segnaposto: i modelli di Nazar sostituiranno i cubi senza cambiare la pianta.
public static class CreaVillaggioLagoNero
{
    const string PercorsoScena = "Assets/Scenes/VillaggioLagoNero.unity";
    const string Cartella = "Assets/Segnaposto/Villaggio";
    const string PercorsoPrefab = Cartella + "/casa-blocco.prefab";
    const string PercorsoMappa = Cartella + "/villaggio-lago-nero.png";
    // Le misure fisse prese dalla mappa (cimitero, abside, segnalini...) vanno moltiplicate per la stessa scala dei dati.
    const float S = DatiVillaggioLagoNero.Scala;
    const float Larghezza = 210f * S;  // est-ovest
    const float Profondita = 170f * S; // nord-sud
    static readonly Vector3 CentroPiazza = new Vector3(96f * S, 0f, 84f * S);

    static Material terreno, strada, piazza, pietra, pietraScura, casa, speciale, porta, legno, foglie, acqua, sabbia, orto, lapide, fuoco;
    static System.Random caso;

    [MenuItem("magic-gnl/Crea Villaggio Lago Nero (blockout)")]
    static void Crea()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (System.IO.File.Exists(PercorsoScena) &&
            !EditorUtility.DisplayDialog("Villaggio Lago Nero",
                "La scena " + PercorsoScena + " esiste già. Ricrearla da capo? Le modifiche fatte a mano andranno perse.",
                "Ricrea", "Annulla"))
            return;

        caso = new System.Random(5);
        PreparaCartelle();
        PreparaMateriali();
        GameObject prefabCasa = PreparaPrefabCasa();

        var scena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var villaggio = new GameObject("Villaggio").transform;

        CreaTerrenoELago(Gruppo("Terreno", villaggio), Gruppo("Lago", villaggio));
        CreaStrade(Gruppo("Strade", villaggio));
        CreaPiazza(Gruppo("Piazza", villaggio));
        CreaEdifici(Gruppo("Edifici speciali", villaggio), Gruppo("Case", villaggio), prefabCasa);
        CreaCimitero(Gruppo("Cimitero", villaggio));
        CreaOrti(Gruppo("Orti", villaggio));
        CreaBosco(Gruppo("Bosco", villaggio));
        CreaConfini(Gruppo("Confini", villaggio));
        CreaMappaGuida(villaggio);
        PreparaLuciECielo();

        // Giocatore davanti alla porta della casa dell'eroe, girato verso la strada.
        var partenza = new Vector3(DatiVillaggioLagoNero.Partenza[0], 1.05f, DatiVillaggioLagoNero.Partenza[1]);
        var casaEroe = TrovaEdificio("Casa dell'eroe");
        Vector3 verso = partenza - casaEroe; verso.y = 0f;
        var giocatore = CreaScenaProva.CreaGiocatore(partenza, Quaternion.LookRotation(verso).eulerAngles.y);
        CreaScenaProva.PreparaCamera(giocatore.transform, true);
        if (Camera.main != null) Camera.main.farClipPlane = 330f; // il villaggio è largo più di 300 metri

        // Orco di prova in piazza: sta fermo e si guarda intorno; insegue e attacca solo dopo aver visto il giocatore.
        CreaOrco("Orco della piazza", CentroPiazza + new Vector3(0f, 0f, -9f));

        CreaSegnalini(Gruppo("Segnalini (proposte)", null));

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scena, PercorsoScena);
        Debug.Log("Villaggio Lago Nero creato in " + PercorsoScena + ". Premi Play: si parte davanti alla casa dell'eroe, la piazzetta è a nord-ovest.");
    }

    // ---------- Parti del villaggio ----------

    static void CreaTerrenoELago(Transform gruppoTerreno, Transform gruppoLago)
    {
        float[] riva = DatiVillaggioLagoNero.Riva;
        float rivaMinima = float.MaxValue;
        for (int i = 0; i < riva.Length; i += 2) rivaMinima = Mathf.Min(rivaMinima, riva[i]);
        float inizioStrisce = rivaMinima - 4f;

        // Il grosso del terreno è un unico blocco; vicino al lago si va a strisce che seguono la riva ondulata.
        Blocco("Terreno", gruppoTerreno, new Vector3(inizioStrisce / 2f, -0.5f, Profondita / 2f), new Vector3(inizioStrisce, 1f, Profondita), terreno);

        for (int i = 0; i + 3 < riva.Length; i += 2)
        {
            float xRiva = (riva[i] + riva[i + 2]) / 2f;
            float z0 = riva[i + 3], z1 = riva[i + 1];   // la riva va da nord a sud: z scende
            float zCentro = (z0 + z1) / 2f, zLungo = Mathf.Abs(z1 - z0) + 0.05f;

            float xSabbia = xRiva - 3.5f;
            Blocco("Terreno riva", gruppoTerreno, new Vector3((inizioStrisce + xSabbia) / 2f, -0.5f, zCentro),
                new Vector3(xSabbia - inizioStrisce, 1f, zLungo), terreno);
            Blocco("Riva", gruppoLago, new Vector3((xSabbia + xRiva) / 2f, -0.55f, zCentro),
                new Vector3(xRiva - xSabbia, 1f, zLungo), sabbia);
            Blocco("Acqua", gruppoLago, new Vector3((xRiva + Larghezza) / 2f, -0.8f, zCentro),
                new Vector3(Larghezza - xRiva, 1f, zLungo), acqua);
        }

        // Molo: assi di legno sull'acqua, con i pali ai lati, e tre barche.
        float[] m = DatiVillaggioLagoNero.Molo;
        var molo = Gruppo("Molo", gruppoLago);
        Blocco("Assi del molo", molo, new Vector3(m[0] + m[2] / 2f, 0.05f, m[1]), new Vector3(m[2], 0.2f, m[3]), legno);
        for (float k = 0f; k <= m[2]; k += 6f)
        {
            foreach (float lato in new[] { -1f, 1f })
                Cilindro("Palo", molo, new Vector3(m[0] + k, -0.2f, m[1] + lato * (m[3] / 2f + 0.3f)), 0.35f, 1.4f, legno, false);
        }
        Barca(molo, m[0] + 10f, m[1] - 4.2f, 3f, 5f);
        Barca(molo, m[0] + 20f, m[1] + 4.4f, -4f, 5f);
        Barca(molo, m[0] + 27f, m[1] - 4.6f, 8f, 4.4f);
    }

    static void Barca(Transform genitore, float x, float z, float rotazione, float lunghezza)
    {
        var barca = Gruppo("Barca", genitore);
        barca.localPosition = new Vector3(x, -0.35f, z);
        barca.localRotation = Quaternion.Euler(0f, rotazione, 0f);
        Blocco("Scafo", barca, new Vector3(0f, 0f, 0f), new Vector3(lunghezza, 0.5f, 1.6f), legno);
        Blocco("Prua", barca, new Vector3(lunghezza / 2f, 0f, 0f), new Vector3(0.9f, 0.5f, 0.9f), legno, 45f);
    }

    static void CreaStrade(Transform gruppo)
    {
        for (int s = 0; s < DatiVillaggioLagoNero.Strade.Length; s++)
        {
            float[] punti = DatiVillaggioLagoNero.Strade[s];
            float larghezza = DatiVillaggioLagoNero.LarghezzeStrade[s];
            bool principale = larghezza > 3f;
            float altezza = principale ? 0.03f : 0.025f;   // un filo di differenza: niente sfarfallio dove si incrociano
            var tratto = Gruppo(principale ? "Strada principale" : "Vicolo", gruppo);

            for (int i = 0; i + 3 < punti.Length; i += 2)
            {
                var a = new Vector3(punti[i], 0f, punti[i + 1]);
                var b = new Vector3(punti[i + 2], 0f, punti[i + 3]);
                Vector3 dir = b - a;
                float lunghezza = dir.magnitude;
                if (lunghezza < 0.01f) continue;
                var pezzo = Blocco("Tratto", tratto, (a + b) / 2f + Vector3.up * (altezza - 0.03f),
                    new Vector3(larghezza, 0.06f, lunghezza), strada);
                pezzo.transform.localRotation = Quaternion.LookRotation(dir);
            }
            // Un disco su ogni punto, così le curve non hanno buchi.
            for (int i = 0; i + 1 < punti.Length; i += 2)
                Cilindro("Curva", tratto, new Vector3(punti[i], altezza - 0.03f, punti[i + 1]), larghezza, 0.06f, strada, false);
        }
    }

    static void CreaPiazza(Transform gruppo)
    {
        // La piazzetta della mappa è un cerchio di circa 32 m, un po' schiacciato da nord a sud.
        var lastricato = Cilindro("Lastricato", gruppo, CentroPiazza, 32f * S, 0.08f, piazza, true);
        lastricato.transform.localScale = new Vector3(32f * S, 0.04f, 27.2f * S);

        Cilindro("Pozzo", gruppo, CentroPiazza + Vector3.up * 0.5f, 3.6f, 1f, pietra, true);
        Cilindro("Acqua del pozzo", gruppo, CentroPiazza + Vector3.up * 1.01f, 2f, 0.02f, acqua, false);

        foreach (float angolo in new[] { 45f, 135f, 225f, 315f })
        {
            float r = angolo * Mathf.Deg2Rad;
            Torcia(gruppo, CentroPiazza + new Vector3(Mathf.Sin(r), 0f, Mathf.Cos(r)) * 9f * S);
        }
    }

    static void CreaEdifici(Transform speciali, Transform case_, GameObject prefabCasa)
    {
        float[] e = DatiVillaggioLagoNero.Edifici;
        string[] nomi = DatiVillaggioLagoNero.NomiEdifici;
        int numeroCasa = 0;
        for (int i = 0; i < nomi.Length; i++)
        {
            int k = i * DatiVillaggioLagoNero.CampiEdificio;
            string nome = nomi[i];
            bool eSpeciale = nome.Length > 0;
            float altezza = e[k + 8];

            var edificio = (GameObject)PrefabUtility.InstantiatePrefab(prefabCasa);
            edificio.name = eSpeciale ? nome : "casa-blocco " + (++numeroCasa);
            edificio.transform.SetParent(eSpeciale ? speciali : case_, false);
            edificio.transform.localPosition = new Vector3(e[k], altezza / 2f, e[k + 1]);
            edificio.transform.localRotation = Quaternion.Euler(0f, e[k + 2], 0f);
            var dimensioni = new Vector3(e[k + 3], altezza, e[k + 4]);
            edificio.transform.localScale = dimensioni;
            if (eSpeciale) edificio.GetComponent<Renderer>().sharedMaterial = speciale;

            // Il cubetto della porta: le misure sono divise per quelle della casa, perché è figlio del cubo scalato.
            Transform cubettoPorta = edificio.transform.Find("Porta");
            float px = e[k + 5], pz = e[k + 6];
            if (float.IsNaN(px))
            {
                cubettoPorta.gameObject.SetActive(false);
            }
            else
            {
                float segno = Mathf.Sign(pz);
                cubettoPorta.localPosition = new Vector3(px / dimensioni.x, 1.1f / altezza - 0.5f, segno * (0.5f + 0.06f / dimensioni.z));
                cubettoPorta.localScale = new Vector3(1.6f / dimensioni.x, 2.2f / altezza, 0.12f / dimensioni.z);
            }
        }

        // Il tempio ha un'abside rotonda sul lato ovest (il cerchio accanto al rettangolo sulla mappa).
        Cilindro("Abside del tempio", speciali, new Vector3(51.75f * S, 4.5f, 140.5f * S), 8f, 9f, speciale, true)
            .transform.localScale = new Vector3(8f, 4.5f, 8.75f);

        // Torce davanti alla casa dell'eroe e al tempio.
        Vector3 eroe = TrovaEdificio("Casa dell'eroe");
        var partenza = new Vector3(DatiVillaggioLagoNero.Partenza[0], 0f, DatiVillaggioLagoNero.Partenza[1]);
        Vector3 lato = Vector3.Cross(Vector3.up, (partenza - eroe).normalized);
        Torcia(speciali, partenza + lato * 2.5f);
        Torcia(speciali, new Vector3(68f * S, 0f, 136f * S));
    }

    static void CreaCimitero(Transform gruppo)
    {
        // Recinto basso da X 20 a 42 e da Z 138 a 158, con il cancello a est, verso il tempio.
        const float x0 = 20f * S, x1 = 42f * S, z0 = 138f * S, z1 = 158f * S, h = 1.2f;
        Blocco("Recinto nord", gruppo, new Vector3((x0 + x1) / 2f, h / 2f, z1), new Vector3(x1 - x0, h, 0.4f), pietraScura);
        Blocco("Recinto sud", gruppo, new Vector3((x0 + x1) / 2f, h / 2f, z0), new Vector3(x1 - x0, h, 0.4f), pietraScura);
        Blocco("Recinto ovest", gruppo, new Vector3(x0, h / 2f, (z0 + z1) / 2f), new Vector3(0.4f, h, z1 - z0), pietraScura);
        float meta = (z0 + z1) / 2f, varco = 1.6f;
        Blocco("Recinto est", gruppo, new Vector3(x1, h / 2f, (z0 + meta - varco) / 2f), new Vector3(0.4f, h, meta - varco - z0), pietraScura);
        Blocco("Recinto est", gruppo, new Vector3(x1, h / 2f, (meta + varco + z1) / 2f), new Vector3(0.4f, h, z1 - meta - varco), pietraScura);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                float x = (23.5f + i * 5f + 0.7f) * S;
                float z = Profondita - (16f + j * 6f) * S - 0.3f;   // la lapide in testa alla tomba
                var l = Blocco("Lapide", gruppo, new Vector3(x, 0.55f, z), new Vector3(0.9f, 1.1f, 0.2f), lapide);
                l.transform.localRotation = Quaternion.Euler(Tra(-6f, 6f), Tra(-10f, 10f), Tra(-6f, 6f));
                Blocco("Tomba", gruppo, new Vector3(x, 0.05f, z - 0.9f), new Vector3(1.2f, 0.1f, 2.2f), pietraScura).GetComponent<Collider>().enabled = false;
            }
        }
    }

    static void CreaOrti(Transform gruppo)
    {
        float[] o = DatiVillaggioLagoNero.Orti;
        for (int i = 0; i + 3 < o.Length; i += 4)
        {
            Blocco("Orto", gruppo, new Vector3(o[i] + o[i + 2] / 2f, 0.02f, o[i + 1] + o[i + 3] / 2f),
                new Vector3(o[i + 2], 0.08f, o[i + 3]), orto);
        }
    }

    static void CreaBosco(Transform gruppo)
    {
        float[] a = DatiVillaggioLagoNero.Alberi;
        for (int i = 0; i + 2 < a.Length; i += 3)
        {
            float raggio = a[i + 2];
            float altezza = raggio > 2.3f ? Tra(6f, 8f) : Tra(4.5f, 6f);   // nel bosco alti, nel borgo più bassi
            var albero = Gruppo("Albero", gruppo);
            albero.localPosition = new Vector3(a[i], 0f, a[i + 1]);
            Cilindro("Tronco", albero, new Vector3(0f, altezza * 0.35f, 0f), 0.5f, altezza * 0.7f, legno, true);
            var chioma = Forma(PrimitiveType.Sphere, "Chioma", albero, new Vector3(0f, altezza * 0.7f + raggio * 0.4f, 0f),
                new Vector3(raggio * 2f, raggio * 1.6f, raggio * 2f), foglie);
            Object.DestroyImmediate(chioma.GetComponent<Collider>());
        }
    }

    // Muri invisibili sul bordo, per non uscire dalla mappa.
    static void CreaConfini(Transform gruppo)
    {
        Confine(gruppo, new Vector3(Larghezza / 2f, 5f, Profondita + 0.5f), new Vector3(Larghezza + 2f, 10f, 1f));
        Confine(gruppo, new Vector3(Larghezza / 2f, 5f, -0.5f), new Vector3(Larghezza + 2f, 10f, 1f));
        Confine(gruppo, new Vector3(-0.5f, 5f, Profondita / 2f), new Vector3(1f, 10f, Profondita + 2f));
        Confine(gruppo, new Vector3(Larghezza + 0.5f, 5f, Profondita / 2f), new Vector3(1f, 10f, Profondita + 2f));
    }

    static void Confine(Transform gruppo, Vector3 posizione, Vector3 dimensioni)
    {
        var muro = GameObject.CreatePrimitive(PrimitiveType.Cube);
        muro.name = "Confine invisibile";
        muro.transform.SetParent(gruppo, false);
        muro.transform.localPosition = posizione;
        muro.transform.localScale = dimensioni;
        Object.DestroyImmediate(muro.GetComponent<MeshRenderer>());
    }

    // L'immagine della mappa stesa sul terreno (spenta): accendendola si vede se la scena coincide con la mappa.
    static void CreaMappaGuida(Transform genitore)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(PercorsoMappa);
        if (texture == null)
        {
            Debug.LogWarning("Mappa non trovata in " + PercorsoMappa + ": MappaGuida non creata.");
            return;
        }
        string percorsoMateriale = Cartella + "/mappa-guida.mat";
        var materiale = AssetDatabase.LoadAssetAtPath<Material>(percorsoMateriale);
        if (materiale == null)
        {
            materiale = new Material(Shader.Find("Unlit/Texture"));
            AssetDatabase.CreateAsset(materiale, percorsoMateriale);
        }
        materiale.mainTexture = texture;
        // L'immagine ha titolo e margini intorno: si prende solo il riquadro della mappa.
        materiale.mainTextureScale = new Vector2(0.9608f, 0.8815f);
        materiale.mainTextureOffset = new Vector2(0.0196f, 0.0630f);
        EditorUtility.SetDirty(materiale);

        var quad = Forma(PrimitiveType.Quad, "MappaGuida", genitore, new Vector3(Larghezza / 2f, 0.1f, Profondita / 2f),
            new Vector3(Larghezza, Profondita, 1f), materiale);
        quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.SetActive(false);
    }

    // Un nemico di prova (Bersaglio) un po' più grosso e verdastro, con vista e inseguimento (InseguimentoNemico).
    // Guarda verso sud, cioè verso la strada da cui arriva il giocatore partendo dalla casa dell'eroe.
    static void CreaOrco(string nome, Vector3 piedi)
    {
        const float grandezza = 1.15f;
        var orco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        orco.name = nome;
        orco.transform.SetPositionAndRotation(piedi + Vector3.up * grandezza, Quaternion.LookRotation(Vector3.back));
        orco.transform.localScale = Vector3.one * grandezza;
        orco.GetComponent<Renderer>().sharedMaterial = Materiale("PelleOrco", new Color(0.28f, 0.36f, 0.2f));
        var bersaglio = orco.AddComponent<Bersaglio>();
        bersaglio.attaccaIlGiocatore = false;
        orco.AddComponent<InseguimentoNemico>();
    }

    static void CreaSegnalini(Transform gruppo)
    {
        Segnalino(gruppo, "Segnalino: arrivo degli orchi (proposta)", new Vector3(4f, 1.5f, 58.5f * S), new Color(0.9f, 0.1f, 0.1f));
        Segnalino(gruppo, "Segnalino: scontro con l'orco enorme (proposta)", CentroPiazza + new Vector3(0f, 1.5f, 8f), new Color(0.6f, 0.15f, 0.9f));
    }

    static void Segnalino(Transform gruppo, string nome, Vector3 posizione, Color colore)
    {
        var materiale = Materiale(nome.Contains("orchi") ? "SegnalinoRosso" : "SegnalinoViola", colore, true);
        var cubo = Forma(PrimitiveType.Cube, nome, gruppo, posizione, Vector3.one * 1.2f, materiale);
        Object.DestroyImmediate(cubo.GetComponent<Collider>());
    }

    static void PreparaLuciECielo()
    {
        Light sole = null;
        foreach (var luce in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (luce.type == LightType.Directional) { sole = luce; break; }
        }
        if (sole == null)
        {
            sole = new GameObject("Sole").AddComponent<Light>();
            sole.type = LightType.Directional;
        }
        sole.name = "Sole";
        sole.shadows = LightShadows.Soft;

        var luna = new GameObject("Luna").AddComponent<Light>();
        luna.type = LightType.Directional;
        luna.shadows = LightShadows.Soft;
        luna.color = new Color(0.55f, 0.65f, 1f);

        // Si parte di pomeriggio, così si vedono bene gli spazi del blockout. Con T si accelera il tempo fino alla notte.
        var cielo = new GameObject("Cielo").AddComponent<CicloGiornoNotte>();
        cielo.sole = sole;
        cielo.luna = luna;
        cielo.ora = 16f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.55f, 0.58f, 0.62f);
        RenderSettings.fogDensity = 0.008f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.5f);
        sole.transform.rotation = Quaternion.Euler(35f, 210f, 0f);
        sole.intensity = 1f;
        luna.transform.rotation = Quaternion.Euler(135f, 170f, 0f);
        luna.intensity = 0f;
    }

    // ---------- Materiali, prefab e mappa ----------

    static void PreparaCartelle()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Segnaposto")) AssetDatabase.CreateFolder("Assets", "Segnaposto");
        if (!AssetDatabase.IsValidFolder(Cartella)) AssetDatabase.CreateFolder("Assets/Segnaposto", "Villaggio");

        // La mappa è grande (3060 x 2700): la si importa senza ridurla troppo, così si leggono le case.
        if (AssetImporter.GetAtPath(PercorsoMappa) is TextureImporter importatore && importatore.maxTextureSize < 4096)
        {
            importatore.maxTextureSize = 4096;
            importatore.SaveAndReimport();
        }
    }

    // I nomi contengono "Terreno", "Sentiero", "Pietra", "Legno": PassiSonori li usa per scegliere il suono dei passi.
    static void PreparaMateriali()
    {
        terreno = Materiale("TerrenoVillaggio", new Color(0.33f, 0.38f, 0.26f));
        strada = Materiale("SentieroVillaggio", new Color(0.45f, 0.39f, 0.3f));
        piazza = Materiale("PietraPiazza", new Color(0.5f, 0.47f, 0.42f));
        pietra = Materiale("PietraVillaggio", new Color(0.45f, 0.44f, 0.42f));
        pietraScura = Materiale("PietraScuraVillaggio", new Color(0.27f, 0.27f, 0.29f));
        casa = Materiale("CasaVillaggio", new Color(0.55f, 0.53f, 0.5f));
        speciale = Materiale("EdificioSpeciale", new Color(0.42f, 0.33f, 0.26f));
        porta = Materiale("PortaVillaggio", new Color(0.12f, 0.08f, 0.05f));
        legno = Materiale("LegnoVillaggio", new Color(0.32f, 0.22f, 0.14f));
        foglie = Materiale("FoglieVillaggio", new Color(0.18f, 0.27f, 0.17f));
        acqua = Materiale("AcquaLagoNero", new Color(0.04f, 0.05f, 0.07f));
        sabbia = Materiale("SentieroRiva", new Color(0.52f, 0.47f, 0.37f));
        orto = Materiale("SentieroOrto", new Color(0.36f, 0.31f, 0.2f));
        lapide = Materiale("PietraLapide", new Color(0.6f, 0.6f, 0.62f));
        fuoco = Materiale("FuocoVillaggio", new Color(1f, 0.5f, 0.15f), true);
        AssetDatabase.SaveAssets();
    }

    static Material Materiale(string nome, Color colore, bool emette = false)
    {
        string percorso = Cartella + "/" + nome + ".mat";
        var materiale = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (materiale == null)
        {
            materiale = new Material(Shader.Find("Standard"));
            materiale.color = colore;
            materiale.SetFloat("_Glossiness", 0.05f);
            if (emette)
            {
                materiale.EnableKeyword("_EMISSION");
                materiale.SetColor("_EmissionColor", colore * 2.5f);
                materiale.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            AssetDatabase.CreateAsset(materiale, percorso);
        }
        return materiale;
    }

    // Prefab "casa-blocco": un cubo 5 x 5 x 6 m con un cubetto scuro "Porta" sul lato sud.
    // Se esiste già non viene toccato: così, quando Nazar lo cambierà, ricreare la scena userà la versione nuova.
    static GameObject PreparaPrefabCasa()
    {
        var esistente = AssetDatabase.LoadAssetAtPath<GameObject>(PercorsoPrefab);
        if (esistente != null) return esistente;

        var cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubo.name = "casa-blocco";
        cubo.transform.localScale = new Vector3(5f, 5f, 6f);
        cubo.GetComponent<Renderer>().sharedMaterial = casa;

        var cubetto = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubetto.name = "Porta";
        Object.DestroyImmediate(cubetto.GetComponent<Collider>());
        cubetto.transform.SetParent(cubo.transform, false);
        cubetto.transform.localPosition = new Vector3(0f, 1.1f / 5f - 0.5f, -0.51f);
        cubetto.transform.localScale = new Vector3(1.6f / 5f, 2.2f / 5f, 0.12f / 6f);
        cubetto.GetComponent<Renderer>().sharedMaterial = porta;

        var prefab = PrefabUtility.SaveAsPrefabAsset(cubo, PercorsoPrefab);
        Object.DestroyImmediate(cubo);
        return prefab;
    }

    // ---------- Pezzi e utilità ----------

    static void Torcia(Transform genitore, Vector3 posizione)
    {
        var torcia = Gruppo("Torcia", genitore);
        torcia.localPosition = posizione;
        Cilindro("Palo", torcia, new Vector3(0f, 0.9f, 0f), 0.24f, 1.8f, legno, true);
        var brace = Forma(PrimitiveType.Cube, "Brace", torcia, new Vector3(0f, 1.9f, 0f), Vector3.one * 0.3f, fuoco);
        Object.DestroyImmediate(brace.GetComponent<Collider>());

        var oggettoLuce = new GameObject("Luce");
        oggettoLuce.transform.SetParent(torcia, false);
        oggettoLuce.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        var luce = oggettoLuce.AddComponent<Light>();
        luce.type = LightType.Point;
        luce.range = 10f;
        luce.color = new Color(1f, 0.55f, 0.2f);
        luce.intensity = 1.8f;
        luce.shadows = LightShadows.None;
        oggettoLuce.AddComponent<global::Torcia>();
    }

    static Vector3 TrovaEdificio(string nome)
    {
        string[] nomi = DatiVillaggioLagoNero.NomiEdifici;
        for (int i = 0; i < nomi.Length; i++)
        {
            int k = i * DatiVillaggioLagoNero.CampiEdificio;
            if (nomi[i] == nome) return new Vector3(DatiVillaggioLagoNero.Edifici[k], 0f, DatiVillaggioLagoNero.Edifici[k + 1]);
        }
        return CentroPiazza;
    }

    static GameObject Blocco(string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale, float rotazioneY = 0f)
    {
        var oggetto = Forma(PrimitiveType.Cube, nome, genitore, posizione, scala, materiale);
        oggetto.transform.localRotation = Quaternion.Euler(0f, rotazioneY, 0f);
        oggetto.isStatic = true;
        return oggetto;
    }

    // Cilindro con diametro e altezza in metri. Il collider a capsula di Unity non va bene per i cilindri schiacciati,
    // quindi se serve un collider si usa la forma vera (MeshCollider).
    static GameObject Cilindro(string nome, Transform genitore, Vector3 posizione, float diametro, float altezza, Material materiale, bool conCollider)
    {
        var oggetto = Forma(PrimitiveType.Cylinder, nome, genitore, posizione, new Vector3(diametro, altezza / 2f, diametro), materiale);
        Object.DestroyImmediate(oggetto.GetComponent<Collider>());
        if (conCollider) oggetto.AddComponent<MeshCollider>();
        oggetto.isStatic = true;
        return oggetto;
    }

    static GameObject Forma(PrimitiveType tipo, string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale)
    {
        var oggetto = GameObject.CreatePrimitive(tipo);
        oggetto.name = nome;
        if (genitore != null) oggetto.transform.SetParent(genitore, false);
        oggetto.transform.localPosition = posizione;
        oggetto.transform.localScale = scala;
        oggetto.GetComponent<Renderer>().sharedMaterial = materiale;
        return oggetto;
    }

    static Transform Gruppo(string nome, Transform genitore)
    {
        var gruppo = new GameObject(nome).transform;
        if (genitore != null) gruppo.SetParent(genitore, false);
        return gruppo;
    }

    static float Tra(float minimo, float massimo) => minimo + (float)caso.NextDouble() * (massimo - minimo);
}
