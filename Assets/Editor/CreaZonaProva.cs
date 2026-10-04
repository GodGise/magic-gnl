using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Strumento dell'editor: costruisce la prima zona giocabile con forme semplici (segnaposto).
// Come si usa: menu in alto "magic-gnl > Crea zona di prova (villaggio in rovina)".
// Crea e salva Assets/Scenes/ZonaProva.unity: un villaggio in rovina di notte, con
//   - un sentiero con le torce dal punto di partenza (sud) alla piazza con il pozzo;
//   - case distrutte attorno alla piazza;
//   - un cimitero con lapidi e una cripta a ovest;
//   - un bosco di alberi vivi e secchi tutto attorno, rocce e confini invisibili;
//   - ciclo giorno e notte (si parte alle 21), effetto retro PS2, giocatore e tre nemici di prova.
// Tutte le forme sono segnaposto: Nazar le sostituirà con i modelli veri.
// La disposizione è sempre la stessa (numeri casuali con seme fisso), così tutti vedono la stessa zona.
// I materiali segnaposto vengono salvati in Assets/Segnaposto/Materiali.
public static class CreaZonaProva
{
    const string PercorsoScena = "Assets/Scenes/ZonaProva.unity";
    const string CartellaMateriali = "Assets/Segnaposto/Materiali";

    static Material terreno, pietra, pietraScura, legno, foglie, sentiero, lapide, fuoco, buio;
    static System.Random caso;

    [MenuItem("magic-gnl/Crea zona di prova (villaggio in rovina)")]
    static void Crea()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (System.IO.File.Exists(PercorsoScena) &&
            !EditorUtility.DisplayDialog("Zona di prova",
                "La scena " + PercorsoScena + " esiste già. Ricrearla da capo? Le modifiche fatte a mano andranno perse.",
                "Ricrea", "Annulla"))
            return;

        caso = new System.Random(7);
        PreparaMateriali();

        var scena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var zona = new GameObject("Zona").transform;

        var suolo = Forma(PrimitiveType.Plane, "Suolo", zona, Vector3.zero, new Vector3(20f, 1f, 20f), terreno);
        suolo.isStatic = true;

        CreaSentiero(zona);
        CreaPiazza(zona);
        CreaCimiteroECripta(zona);
        CreaBosco(zona);
        CreaRocce(zona);
        CreaConfini(zona);
        PreparaLuciECielo();

        var giocatore = CreaScenaProva.CreaGiocatore(new Vector3(0f, 1f, -75f), 0f);
        CreaScenaProva.PreparaCamera(giocatore.transform, true);

        CreaScenaProva.CreaNemico("Guardiano della piazza", new Vector3(3f, 1f, 7f), true);
        CreaScenaProva.CreaNemico("Spettro del cimitero", new Vector3(-31f, 1f, 22f), false);
        CreaScenaProva.CreaNemico("Custode della cripta", new Vector3(-35f, 1f, 33f), true);

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scena, PercorsoScena);
        Debug.Log("Zona di prova creata in " + PercorsoScena + ". Premi Play: si parte a sud, la piazza è dritta davanti.");
    }

    // ---------- Parti della zona ----------

    static void CreaSentiero(Transform zona)
    {
        var gruppo = Gruppo("Sentiero", zona, Vector3.zero);
        for (float z = -82f; z <= -6f; z += 4f)
        {
            float x = SentieroX(z);
            Forma(PrimitiveType.Cube, "Lastra", gruppo, new Vector3(x, 0.02f, z), new Vector3(3.2f, 0.04f, 4.4f),
                sentiero, Tra(-6f, 6f), false);
        }

        // Torce ai lati del sentiero, alternate.
        bool sinistra = true;
        for (float z = -70f; z <= -15f; z += 14f)
        {
            float x = SentieroX(z) + (sinistra ? -3.2f : 3.2f);
            Torcia(gruppo, new Vector3(x, 0f, z));
            sinistra = !sinistra;
        }
    }

    static float SentieroX(float z) => 2f * Mathf.Sin(z * 0.05f);

    static void CreaPiazza(Transform zona)
    {
        var piazza = Gruppo("Piazza", zona, Vector3.zero);

        Forma(PrimitiveType.Cylinder, "Pozzo", piazza, new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 0.5f, 2.4f), pietra);
        Forma(PrimitiveType.Cylinder, "Acqua scura", piazza, new Vector3(0f, 1.01f, 0f), new Vector3(1.8f, 0.01f, 1.8f), buio, 0f, false);

        float[] angoliTorce = { 45f, 135f, 225f, 315f };
        foreach (float a in angoliTorce) Torcia(piazza, Direzione(a) * 6f);

        // Case in rovina attorno alla piazza, con la porta rivolta al centro.
        float[] angoliCase = { 40f, 90f, 140f, 215f, 320f };
        foreach (float a in angoliCase) Casa(piazza, Direzione(a) * Tra(16f, 20f), a);
    }

    static void CreaCimiteroECripta(Transform zona)
    {
        var cimitero = Gruppo("Cimitero", zona, new Vector3(-35f, 0f, 20f));
        for (int riga = 0; riga < 4; riga++)
        {
            for (int colonna = 0; colonna < 5; colonna++)
            {
                if (caso.NextDouble() < 0.15) continue;
                float altezza = Tra(0.7f, 1.3f);
                var l = Forma(PrimitiveType.Cube, "Lapide", cimitero,
                    new Vector3(-6f + colonna * 3f + Tra(-0.4f, 0.4f), altezza / 2f, -4f + riga * 3f + Tra(-0.4f, 0.4f)),
                    new Vector3(0.7f, altezza, 0.15f), lapide);
                l.transform.localRotation = Quaternion.Euler(Tra(-8f, 8f), Tra(-15f, 15f), Tra(-10f, 10f));
            }
        }
        Torcia(cimitero, new Vector3(-8f, 0f, -7f));
        Torcia(cimitero, new Vector3(8f, 0f, -7f));

        Cripta(zona, new Vector3(-35f, 0f, 40f));
    }

    static void CreaBosco(Transform zona)
    {
        var bosco = Gruppo("Bosco", zona, Vector3.zero);
        int piantati = 0;
        for (int tentativo = 0; tentativo < 600 && piantati < 170; tentativo++)
        {
            var p = new Vector3(Tra(-95f, 95f), 0f, Tra(-95f, 95f));
            if (p.magnitude < 30f) continue;                                         // piazza e case
            if (p.z < -2f && Mathf.Abs(p.x - SentieroX(p.z)) < 7f) continue;          // sentiero
            if (Vector3.Distance(p, new Vector3(-35f, 0f, 30f)) < 20f) continue;      // cimitero e cripta
            Albero(bosco, p);
            piantati++;
        }
    }

    static void CreaRocce(Transform zona)
    {
        var rocce = Gruppo("Rocce", zona, Vector3.zero);
        for (int i = 0; i < 30; i++)
        {
            var p = new Vector3(Tra(-90f, 90f), 0f, Tra(-90f, 90f));
            if (p.magnitude < 10f) continue;
            if (p.z < -2f && Mathf.Abs(p.x - SentieroX(p.z)) < 4f) continue;
            float d = Tra(0.8f, 2.5f);
            var r = Forma(PrimitiveType.Sphere, "Roccia", rocce, p + Vector3.up * d * 0.2f,
                new Vector3(d, d * Tra(0.4f, 0.8f), d * Tra(0.7f, 1.2f)), pietraScura);
            r.transform.localRotation = Quaternion.Euler(0f, Tra(0f, 360f), 0f);
        }
    }

    // Muri invisibili sul bordo del suolo, per non cadere nel vuoto.
    static void CreaConfini(Transform zona)
    {
        var confini = Gruppo("Confini", zona, Vector3.zero);
        Muro(confini, new Vector3(0f, 5f, 100f), new Vector3(200f, 10f, 1f));
        Muro(confini, new Vector3(0f, 5f, -100f), new Vector3(200f, 10f, 1f));
        Muro(confini, new Vector3(100f, 5f, 0f), new Vector3(1f, 10f, 200f));
        Muro(confini, new Vector3(-100f, 5f, 0f), new Vector3(1f, 10f, 200f));
    }

    static void PreparaLuciECielo()
    {
        // La luce direzionale creata con la scena diventa il sole (le torce sono luci Point e vanno escluse).
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

        var cielo = new GameObject("Cielo").AddComponent<CicloGiornoNotte>();
        cielo.sole = sole;
        cielo.luna = luna;
        cielo.ora = 21f;

        // Anteprima notturna anche nell'editor; in gioco ci pensa il CicloGiornoNotte.
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.13f);
        RenderSettings.fogDensity = 0.035f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.06f, 0.08f, 0.17f);
        sole.transform.rotation = Quaternion.Euler(-45f, 170f, 0f);
        sole.intensity = 0f;
        luna.transform.rotation = Quaternion.Euler(135f, 170f, 0f);
        luna.color = new Color(0.55f, 0.65f, 1f);
        luna.intensity = 0.35f;
    }

    // ---------- Pezzi riutilizzabili ----------

    static void Casa(Transform genitore, Vector3 centro, float rotazioneY)
    {
        var casa = Gruppo("Casa in rovina", genitore, centro, rotazioneY);
        float larghezza = Tra(5f, 7f);
        float profondita = Tra(5f, 7f);

        float hRetro = Tra(2.5f, 4f);
        Forma(PrimitiveType.Cube, "Muro retro", casa, new Vector3(0f, hRetro / 2f, profondita / 2f), new Vector3(larghezza, hRetro, 0.5f), pietra);

        float hSinistra = Tra(1.2f, 3.8f);
        Forma(PrimitiveType.Cube, "Muro sinistro", casa, new Vector3(-larghezza / 2f, hSinistra / 2f, 0f), new Vector3(0.5f, hSinistra, profondita), pietra);

        // Il muro destro a volte è crollato: ne resta solo un pezzo.
        float hDestra = Tra(1.2f, 3.8f);
        float lunghezzaDestra = caso.NextDouble() < 0.5 ? profondita : profondita * 0.5f;
        Forma(PrimitiveType.Cube, "Muro destro", casa, new Vector3(larghezza / 2f, hDestra / 2f, (profondita - lunghezzaDestra) / 2f),
            new Vector3(0.5f, hDestra, lunghezzaDestra), pietra);

        // Facciata con la porta al centro.
        float porta = 1.6f;
        float pezzo = (larghezza - porta) / 2f;
        float hFronte = Tra(2f, 3.5f);
        Forma(PrimitiveType.Cube, "Facciata sinistra", casa, new Vector3(-(porta + pezzo) / 2f, hFronte / 2f, -profondita / 2f), new Vector3(pezzo, hFronte, 0.5f), pietra);
        Forma(PrimitiveType.Cube, "Facciata destra", casa, new Vector3((porta + pezzo) / 2f, hFronte * 0.4f, -profondita / 2f), new Vector3(pezzo, hFronte * 0.8f, 0.5f), pietra);

        for (int i = 0; i < 3; i++)
        {
            Forma(PrimitiveType.Cube, "Macerie", casa,
                new Vector3(Tra(-larghezza / 2f, larghezza / 2f), 0.25f, Tra(-profondita / 2f, profondita / 2f)),
                new Vector3(Tra(0.4f, 1f), 0.5f, Tra(0.4f, 1f)), pietraScura, Tra(0f, 360f));
        }
    }

    static void Cripta(Transform genitore, Vector3 posizione)
    {
        var c = Gruppo("Cripta", genitore, posizione);
        Forma(PrimitiveType.Cube, "Retro", c, new Vector3(0f, 2f, 3f), new Vector3(7f, 4f, 0.6f), pietraScura);
        Forma(PrimitiveType.Cube, "Lato sinistro", c, new Vector3(-3.5f, 2f, 0f), new Vector3(0.6f, 4f, 6.6f), pietraScura);
        Forma(PrimitiveType.Cube, "Lato destro", c, new Vector3(3.5f, 2f, 0f), new Vector3(0.6f, 4f, 6.6f), pietraScura);
        Forma(PrimitiveType.Cube, "Tetto", c, new Vector3(0f, 4.3f, 0f), new Vector3(7.6f, 0.6f, 7.2f), pietraScura);
        Forma(PrimitiveType.Cube, "Fronte sinistro", c, new Vector3(-2.25f, 2f, -3f), new Vector3(2.5f, 4f, 0.6f), pietraScura);
        Forma(PrimitiveType.Cube, "Fronte destro", c, new Vector3(2.25f, 2f, -3f), new Vector3(2.5f, 4f, 0.6f), pietraScura);
        Forma(PrimitiveType.Cube, "Architrave", c, new Vector3(0f, 3.4f, -3f), new Vector3(2f, 1.2f, 0.6f), pietraScura);
        Forma(PrimitiveType.Cube, "Interno buio", c, new Vector3(0f, 0.02f, 0f), new Vector3(6.4f, 0.04f, 6f), buio, 0f, false);
        Torcia(c, new Vector3(-1.9f, 0f, -3.8f));
        Torcia(c, new Vector3(1.9f, 0f, -3.8f));
    }

    static void Albero(Transform genitore, Vector3 posizione)
    {
        var albero = Gruppo("Albero", genitore, posizione, Tra(0f, 360f));
        float altezza = Tra(4f, 7f);
        Forma(PrimitiveType.Cylinder, "Tronco", albero, new Vector3(0f, altezza / 2f, 0f), new Vector3(0.45f, altezza / 2f, 0.45f), legno);

        if (caso.NextDouble() < 0.65)
        {
            Forma(PrimitiveType.Sphere, "Chioma", albero, new Vector3(0f, altezza + 0.5f, 0f), Vector3.one * Tra(2.5f, 4f), foglie, 0f, false);
        }
        else
        {
            // Albero secco: solo un ramo storto.
            var ramo = Forma(PrimitiveType.Cylinder, "Ramo secco", albero, new Vector3(0.6f, altezza * 0.75f, 0f), new Vector3(0.15f, 1f, 0.15f), legno, 0f, false);
            ramo.transform.localRotation = Quaternion.Euler(0f, 0f, -50f);
        }
    }

    static void Torcia(Transform genitore, Vector3 posizione)
    {
        var torcia = Gruppo("Torcia", genitore, posizione);
        Forma(PrimitiveType.Cylinder, "Palo", torcia, new Vector3(0f, 0.9f, 0f), new Vector3(0.12f, 0.9f, 0.12f), legno);
        Forma(PrimitiveType.Cube, "Brace", torcia, new Vector3(0f, 1.9f, 0f), Vector3.one * 0.3f, fuoco, 0f, false);

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

    static void Muro(Transform genitore, Vector3 posizione, Vector3 dimensioni)
    {
        var muro = GameObject.CreatePrimitive(PrimitiveType.Cube);
        muro.name = "Confine invisibile";
        muro.transform.SetParent(genitore, false);
        muro.transform.localPosition = posizione;
        muro.transform.localScale = dimensioni;
        Object.DestroyImmediate(muro.GetComponent<MeshRenderer>());
    }

    // ---------- Utilità ----------

    static GameObject Forma(PrimitiveType tipo, string nome, Transform genitore, Vector3 posizione, Vector3 scala,
        Material materiale, float rotazioneY = 0f, bool conCollider = true)
    {
        var oggetto = GameObject.CreatePrimitive(tipo);
        oggetto.name = nome;
        oggetto.transform.SetParent(genitore, false);
        oggetto.transform.localPosition = posizione;
        oggetto.transform.localRotation = Quaternion.Euler(0f, rotazioneY, 0f);
        oggetto.transform.localScale = scala;
        oggetto.GetComponent<Renderer>().sharedMaterial = materiale;
        if (!conCollider) Object.DestroyImmediate(oggetto.GetComponent<Collider>());
        return oggetto;
    }

    static Transform Gruppo(string nome, Transform genitore, Vector3 posizione, float rotazioneY = 0f)
    {
        var gruppo = new GameObject(nome).transform;
        gruppo.SetParent(genitore, false);
        gruppo.localPosition = posizione;
        gruppo.localRotation = Quaternion.Euler(0f, rotazioneY, 0f);
        return gruppo;
    }

    // Direzione orizzontale per un angolo in gradi: 0 = nord (+Z), 90 = est (+X).
    static Vector3 Direzione(float gradi)
    {
        float r = gradi * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(r), 0f, Mathf.Cos(r));
    }

    static float Tra(float minimo, float massimo) => minimo + (float)caso.NextDouble() * (massimo - minimo);

    static void PreparaMateriali()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Segnaposto")) AssetDatabase.CreateFolder("Assets", "Segnaposto");
        if (!AssetDatabase.IsValidFolder(CartellaMateriali)) AssetDatabase.CreateFolder("Assets/Segnaposto", "Materiali");

        terreno = Materiale("Terreno", new Color(0.17f, 0.19f, 0.13f));
        pietra = Materiale("Pietra", new Color(0.42f, 0.41f, 0.4f));
        pietraScura = Materiale("PietraScura", new Color(0.24f, 0.24f, 0.26f));
        legno = Materiale("Legno", new Color(0.26f, 0.18f, 0.12f));
        foglie = Materiale("Foglie", new Color(0.1f, 0.18f, 0.12f));
        sentiero = Materiale("Sentiero", new Color(0.3f, 0.27f, 0.22f));
        lapide = Materiale("Lapide", new Color(0.55f, 0.55f, 0.57f));
        buio = Materiale("Buio", new Color(0.02f, 0.02f, 0.03f));
        fuoco = Materiale("Fuoco", new Color(1f, 0.5f, 0.15f), true);
        AssetDatabase.SaveAssets();
    }

    static Material Materiale(string nome, Color colore, bool emette = false)
    {
        string percorso = CartellaMateriali + "/" + nome + ".mat";
        var materiale = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (materiale == null)
        {
            materiale = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(materiale, percorso);
        }
        materiale.color = colore;
        materiale.SetFloat("_Glossiness", 0.05f);
        if (emette)
        {
            materiale.EnableKeyword("_EMISSION");
            materiale.SetColor("_EmissionColor", colore * 2.5f);
            materiale.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        EditorUtility.SetDirty(materiale);
        return materiale;
    }
}
