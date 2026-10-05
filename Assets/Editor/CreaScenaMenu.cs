using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Strumento dell'editor: costruisce la scena del menu iniziale.
// Come si usa: menu in alto "magic-gnl > Crea scena menu".
// Crea e salva Assets/Scenes/Menu.unity: la riva di un lago nero di notte, con nebbia, alberi secchi,
// una torcia, un pontile e le sagome di un villaggio sull'altra riva, una camera che ondeggia piano
// e l'oggetto "Menu" con lo script MenuPrincipale (scritte, scelte e musica).
// Mette anche la scena Menu al primo posto nelle Build Settings (è la scena da cui parte il gioco)
// e aggiunge ZonaProva, così "Nuova partita" ha una scena da caricare.
// Non tocca nessun'altra scena. I materiali vanno in Assets/Segnaposto/Menu.
// Per mettere la musica: selezionare l'oggetto "Menu" e trascinare il file audio nel campo "Musica".
public static class CreaScenaMenu
{
    const string PercorsoScena = "Assets/Scenes/Menu.unity";
    const string CartellaMateriali = "Assets/Segnaposto/Menu";
    static readonly Color coloreNebbia = new Color(0.03f, 0.04f, 0.07f);

    [MenuItem("magic-gnl/Crea scena menu")]
    static void Crea()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (System.IO.File.Exists(PercorsoScena) &&
            !EditorUtility.DisplayDialog("Scena menu",
                "La scena " + PercorsoScena + " esiste già. Ricrearla da capo? Le modifiche fatte a mano (per esempio la musica assegnata) andranno perse.",
                "Ricrea", "Annulla"))
            return;

        var caso = new System.Random(11);
        float Caso(float min, float max) => min + (float)caso.NextDouble() * (max - min);

        var terra = Materiale("menu-terra", new Color(0.09f, 0.1f, 0.08f), 0.05f);
        var acqua = Materiale("menu-acqua", new Color(0.01f, 0.012f, 0.02f), 0.92f);
        var legno = Materiale("menu-legno", new Color(0.13f, 0.09f, 0.06f), 0.1f);
        var sagoma = Materiale("menu-sagoma", new Color(0.035f, 0.037f, 0.045f), 0.0f);
        var finestra = Materiale("menu-finestra", new Color(1f, 0.6f, 0.25f), 0f, true);

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // atmosfera: notte, nebbia fitta, niente cielo
        RenderSettings.skybox = null;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = coloreNebbia;
        RenderSettings.fogDensity = 0.018f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.06f, 0.07f, 0.11f);

        var paesaggio = new GameObject("Paesaggio").transform;

        // luna
        var luna = new GameObject("Luna").AddComponent<Light>();
        luna.type = LightType.Directional;
        luna.color = new Color(0.55f, 0.65f, 1f);
        luna.intensity = 0.25f;
        luna.shadows = LightShadows.Soft;
        luna.transform.rotation = Quaternion.Euler(22f, 200f, 0f);
        luna.transform.SetParent(paesaggio);

        // riva, lago e riva lontana
        Forma(PrimitiveType.Plane, "Riva", new Vector3(0f, 0f, 0f), new Vector3(6f, 1f, 2.2f), Vector3.zero, terra, paesaggio);
        Forma(PrimitiveType.Plane, "Lago", new Vector3(0f, -0.05f, 40f), new Vector3(12f, 1f, 6f), Vector3.zero, acqua, paesaggio);
        Forma(PrimitiveType.Plane, "Riva lontana", new Vector3(0f, 0.1f, 75f), new Vector3(12f, 1f, 2f), Vector3.zero, terra, paesaggio);

        // pontile che entra nell'acqua, con i pali
        Forma(PrimitiveType.Cube, "Pontile", new Vector3(3.2f, 0.25f, 18f), new Vector3(1.8f, 0.15f, 16f), Vector3.zero, legno, paesaggio);
        for (int i = 0; i < 6; i++)
            for (int lato = -1; lato <= 1; lato += 2)
                Forma(PrimitiveType.Cylinder, "Palo", new Vector3(3.2f + lato * 0.9f, -0.2f, 11f + i * 3f),
                    new Vector3(0.18f, 0.6f, 0.18f), Vector3.zero, legno, paesaggio);
        Forma(PrimitiveType.Cube, "Barca", new Vector3(5.1f, 0.05f, 21f), new Vector3(1.1f, 0.35f, 3.2f), new Vector3(0f, 8f, 0f), legno, paesaggio);

        // villaggio sull'altra riva: solo sagome scure e qualche finestra accesa
        var villaggio = new GameObject("Villaggio lontano").transform;
        villaggio.SetParent(paesaggio);
        for (int i = 0; i < 9; i++)
        {
            float x = -22f + i * 5.5f + Caso(-1f, 1f);
            float z = 70f + Caso(-2f, 3f);
            float alt = Caso(3.5f, 5.5f);
            float larg = Caso(3.5f, 5f);
            var casa = Forma(PrimitiveType.Cube, "Casa", new Vector3(x, alt / 2f, z), new Vector3(larg, alt, 4f), new Vector3(0f, Caso(-8f, 8f), 0f), sagoma, villaggio);
            Forma(PrimitiveType.Cube, "Tetto", new Vector3(x, alt + 0.9f, z), new Vector3(larg * 0.72f, larg * 0.72f, 4.1f), new Vector3(0f, casa.transform.eulerAngles.y, 45f), sagoma, villaggio);
            if (i % 3 == 1)
            {
                Forma(PrimitiveType.Cube, "Finestra", new Vector3(x, alt * 0.5f, z - 2.05f), new Vector3(0.6f, 0.7f, 0.05f), Vector3.zero, finestra, villaggio);
                var luce = new GameObject("Luce finestra").AddComponent<Light>();
                luce.type = LightType.Point;
                luce.color = new Color(1f, 0.55f, 0.25f);
                luce.intensity = 1.2f;
                luce.range = 7f;
                luce.transform.position = new Vector3(x, alt * 0.5f, z - 3f);
                luce.transform.SetParent(villaggio);
            }
        }
        // campanile del tempio
        Forma(PrimitiveType.Cube, "Campanile", new Vector3(-9f, 6f, 76f), new Vector3(3f, 12f, 3f), Vector3.zero, sagoma, villaggio);

        // alberi secchi ai lati della riva
        var bosco = new GameObject("Alberi").transform;
        bosco.SetParent(paesaggio);
        for (int i = 0; i < 26; i++)
        {
            float lato = i % 2 == 0 ? -1f : 1f;
            float x = lato * Caso(6f, 26f);
            float z = Caso(-6f, 9f);
            if (lato > 0 && x < 7.5f && z > 4f) continue;   // lascia libero il pontile
            float alt = Caso(5f, 9f);
            Forma(PrimitiveType.Cylinder, "Albero secco", new Vector3(x, alt / 2f, z), new Vector3(0.35f, alt / 2f, 0.35f),
                new Vector3(Caso(-6f, 6f), 0f, Caso(-6f, 6f)), sagoma, bosco);
            for (int r = 0; r < 3; r++)
            {
                float h = Caso(alt * 0.45f, alt * 0.9f);
                float lung = Caso(1.2f, 2.4f);
                float giro = Caso(0f, 360f);
                var ramo = Forma(PrimitiveType.Cylinder, "Ramo", Vector3.zero, new Vector3(0.12f, lung / 2f, 0.12f), Vector3.zero, sagoma, bosco);
                ramo.transform.position = new Vector3(x, h, z) + Quaternion.Euler(0f, giro, 0f) * new Vector3(lung * 0.45f, lung * 0.35f, 0f);
                ramo.transform.rotation = Quaternion.Euler(0f, giro, -55f);
            }
        }

        // torcia in primo piano
        Forma(PrimitiveType.Cylinder, "Palo torcia", new Vector3(-2.6f, 0.9f, 3.5f), new Vector3(0.12f, 0.9f, 0.12f), Vector3.zero, legno, paesaggio);
        Forma(PrimitiveType.Sphere, "Fiamma", new Vector3(-2.6f, 1.95f, 3.5f), Vector3.one * 0.22f, Vector3.zero, finestra, paesaggio);
        var fuoco = new GameObject("Torcia").AddComponent<Light>();
        fuoco.type = LightType.Point;
        fuoco.color = new Color(1f, 0.55f, 0.25f);
        fuoco.range = 10f;
        fuoco.shadows = LightShadows.Soft;
        fuoco.transform.position = new Vector3(-2.6f, 2.1f, 3.5f);
        fuoco.transform.SetParent(paesaggio);
        fuoco.gameObject.AddComponent<Torcia>();

        // camera
        var camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        var cam = camera.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = coloreNebbia;
        cam.farClipPlane = 150f;
        cam.fieldOfView = 55f;
        camera.AddComponent<AudioListener>();
        camera.AddComponent<EffettoRetro>();
        camera.AddComponent<CameraMenu>();
        camera.transform.position = new Vector3(0f, 1.8f, -4f);
        camera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 2f, 30f) - camera.transform.position);

        // menu
        var menu = new GameObject("Menu");
        menu.AddComponent<MenuPrincipale>();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), PercorsoScena);
        AggiornaBuildSettings();
        Selection.activeGameObject = menu;
        Debug.Log("[magic-gnl] Scena menu creata in " + PercorsoScena + ". Premi Play per provarla.");
    }

    // Menu al primo posto; ZonaProva (se esiste) subito dopo; le altre scene già presenti restano.
    static void AggiornaBuildSettings()
    {
        var scene = new List<EditorBuildSettingsScene>();
        scene.Add(new EditorBuildSettingsScene(PercorsoScena, true));
        foreach (var s in EditorBuildSettings.scenes)
            if (s.path != PercorsoScena) scene.Add(s);
        const string zona = "Assets/Scenes/ZonaProva.unity";
        if (System.IO.File.Exists(zona) && !scene.Exists(s => s.path == zona))
            scene.Add(new EditorBuildSettingsScene(zona, true));
        EditorBuildSettings.scenes = scene.ToArray();
    }

    static GameObject Forma(PrimitiveType tipo, string nome, Vector3 posizione, Vector3 scala, Vector3 rotazione, Material materiale, Transform genitore)
    {
        var o = GameObject.CreatePrimitive(tipo);
        o.name = nome;
        o.transform.SetParent(genitore, false);
        o.transform.position = posizione;
        o.transform.rotation = Quaternion.Euler(rotazione);
        o.transform.localScale = scala;
        o.GetComponent<Renderer>().sharedMaterial = materiale;
        var collisore = o.GetComponent<Collider>();
        if (collisore != null) Object.DestroyImmediate(collisore);   // nel menu non serve camminare
        return o;
    }

    static Material Materiale(string nome, Color colore, float lucidita, bool emette = false)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Segnaposto")) AssetDatabase.CreateFolder("Assets", "Segnaposto");
        if (!AssetDatabase.IsValidFolder(CartellaMateriali)) AssetDatabase.CreateFolder("Assets/Segnaposto", "Menu");
        string percorso = CartellaMateriali + "/" + nome + ".mat";
        var materiale = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (materiale == null)
        {
            materiale = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(materiale, percorso);
        }
        materiale.color = colore;
        materiale.SetFloat("_Glossiness", lucidita);
        if (emette)
        {
            materiale.EnableKeyword("_EMISSION");
            materiale.SetColor("_EmissionColor", colore * 2f);
        }
        EditorUtility.SetDirty(materiale);
        return materiale;
    }
}
