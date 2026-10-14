using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Strumento dell'editor: crea in un clic una scena per provare lo Stregone e i suoi incantesimi,
// separata dalla zona di prova di Lorenzo (che non viene toccata).
// Come si usa: menu "magic-gnl > Crea scena di prova dello Stregone". Il comando:
//   1. crea gli oggetti dello Stregone, se mancano (come "Crea oggetti dello Stregone");
//   2. crea e salva Assets/Scenes/Prove/ProvaStregone.unity;
//   3. sceglie "Prova classe > Gioca come Stregone completo": al Play lo Stregone ha già bastone, libro, veste
//      e un incantesimo per scuola addosso, e tutto il resto nello zaino (Tab).
// Poi basta premere Play. Nella scena, partendo dal giocatore (guarda verso nord):
//   - davanti, 5 manichini fermi che non attaccano e rinascono: per provare danno, mira e attese;
//   - a destra (nord-est), un branco di 4 orchi sgherri che inseguono: per Palla di fuoco, Scia, Pozza, evocazioni,
//     Bambola di ossa;
//   - a sinistra (nord-ovest), un dirupo alto 6 m con una rampa e 2 orchi in cima: con l'Onda del lago si buttano giù;
//   - lontano a nord, un orco gigante alto quasi 5 m, più dell'Onda: l'Onda lo rallenta ma non lo spinge;
//   - a sud-est, un boss di prova: niente blocchi, stordimenti e spinte, rallentamenti dimezzati.
// Tutti gli orchi rinascono dopo la morte, così si può riprovare. I numeri si cambiano dall'Inspector.
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class CreaScenaStregone
{
    const string Percorso = "Assets/Scenes/Prove/ProvaStregone.unity";

    [MenuItem("magic-gnl/Crea scena di prova dello Stregone")]
    static void Crea()
    {
        if (System.IO.File.Exists(Percorso) && !EditorUtility.DisplayDialog("Scena di prova dello Stregone",
            "La scena " + Percorso + " esiste già. Ricrearla da capo?", "Ricrea", "Annulla")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        CreaOggettiStregone.Crea();

        var scena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var pavimento = GameObject.CreatePrimitive(PrimitiveType.Plane);
        pavimento.name = "Pavimento";
        pavimento.transform.localScale = new Vector3(12f, 1f, 12f);   // 120 x 120 metri
        pavimento.isStatic = true;

        var giocatore = CreaScenaProva.CreaGiocatore(new Vector3(0f, 1f, 0f), 0f);
        CreaScenaProva.PreparaCamera(giocatore.transform, false);

        // Manichini: fermi, non attaccano, rinascono.
        var manichini = new GameObject("Manichini (non attaccano)").transform;
        for (int i = 0; i < 5; i++)
        {
            CreaScenaProva.CreaNemico("Manichino " + (i + 1), new Vector3(-8f + i * 4f, 1f, 12f), false);
            GameObject.Find("Manichino " + (i + 1)).transform.SetParent(manichini, true);
        }

        // Branco di orchi che inseguono.
        var branco = new GameObject("Branco di orchi").transform;
        Vector3[] posti = { new Vector3(28f, 0f, 30f), new Vector3(32f, 0f, 32f), new Vector3(26f, 0f, 34f), new Vector3(31f, 0f, 36f) };
        for (int i = 0; i < posti.Length; i++)
            Rinasce(CreaOrcoSgherro.Metti("Orco del branco " + (i + 1), posti[i], Vector3.back, branco));

        CreaDirupo();

        // Orco gigante: alto 4,8 m, più dell'Onda del lago (4 m).
        var gigante = CreaOrcoSgherro.Metti("Orco gigante (più alto dell'Onda)", new Vector3(0f, 0f, 48f), Vector3.back);
        gigante.transform.localScale = new Vector3(1.6f, 2.4f, 1.6f);
        gigante.transform.position = new Vector3(0f, 2.4f, 48f);
        Rinasce(gigante, 400f);

        // Boss di prova.
        var boss = CreaOrcoSgherro.Metti("Boss di prova", new Vector3(30f, 0f, -28f), Vector3.left);
        boss.transform.localScale = Vector3.one * 1.6f;
        boss.transform.position = new Vector3(30f, 1.6f, -28f);
        var b = new SerializedObject(boss.GetComponent<Bersaglio>());
        b.FindProperty("boss").boolValue = true;
        b.FindProperty("vitaMassima").floatValue = 800f;
        b.FindProperty("rinasce").boolValue = true;
        b.FindProperty("dannoAttacco").floatValue = 35f;
        b.ApplyModifiedPropertiesWithoutUndo();
        boss.GetComponent<Renderer>().sharedMaterial = Materiale("boss-prova", new Color(0.45f, 0.08f, 0.08f));

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes/Prove")) AssetDatabase.CreateFolder("Assets/Scenes", "Prove");
        EditorSceneManager.SaveScene(scena, Percorso);

        ProvaClasse.StregoneCompleto();
        Debug.Log("Scena di prova dello Stregone creata in " + Percorso + ". Premi Play: Stregone completo, Tab per l'inventario, tasti 1-6 per gli incantesimi.");
    }

    // Dirupo alto 6 m (oltre i 4 m che servono per cadere) con una rampa a sud e due orchi in cima, sul bordo nord.
    static void CreaDirupo()
    {
        var dirupo = new GameObject("Dirupo (Onda del lago)").transform;
        var roccia = Materiale("roccia-prova", new Color(0.32f, 0.31f, 0.3f));

        var piano = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piano.name = "Piano del dirupo";
        piano.transform.SetParent(dirupo, false);
        piano.transform.position = new Vector3(-30f, 3f, 32f);
        piano.transform.localScale = new Vector3(12f, 6f, 8f);      // in cima a 6 m, da z 28 a z 36
        piano.GetComponent<Renderer>().sharedMaterial = roccia;
        piano.isStatic = true;

        // Rampa: sale di 6 m in 12 m, da z 16 a z 28.
        var rampa = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rampa.name = "Rampa";
        rampa.transform.SetParent(dirupo, false);
        rampa.transform.SetPositionAndRotation(new Vector3(-30f, 3f, 22f), Quaternion.Euler(-26.57f, 0f, 0f));
        rampa.transform.localScale = new Vector3(4f, 0.5f, 13.6f);
        rampa.GetComponent<Renderer>().sharedMaterial = roccia;
        rampa.isStatic = true;

        Rinasce(CreaOrcoSgherro.Metti("Orco sul dirupo 1", new Vector3(-32f, 6f, 34.5f), Vector3.back, dirupo));
        Rinasce(CreaOrcoSgherro.Metti("Orco sul dirupo 2", new Vector3(-28f, 6f, 34.5f), Vector3.back, dirupo));
    }

    // In questa scena gli orchi rinascono, così si possono riprovare gli incantesimi.
    static void Rinasce(GameObject orco, float vita = -1f)
    {
        var so = new SerializedObject(orco.GetComponent<Bersaglio>());
        so.FindProperty("rinasce").boolValue = true;
        if (vita > 0f) so.FindProperty("vitaMassima").floatValue = vita;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Material Materiale(string nome, Color colore)
    {
        const string cartella = "Assets/Segnaposto/Materiali";
        if (!AssetDatabase.IsValidFolder("Assets/Segnaposto")) AssetDatabase.CreateFolder("Assets", "Segnaposto");
        if (!AssetDatabase.IsValidFolder(cartella)) AssetDatabase.CreateFolder("Assets/Segnaposto", "Materiali");
        string percorso = cartella + "/" + nome + ".mat";
        var materiale = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (materiale != null) return materiale;
        materiale = new Material(Shader.Find("Standard")) { color = colore };
        materiale.SetFloat("_Glossiness", 0.05f);
        AssetDatabase.CreateAsset(materiale, percorso);
        return materiale;
    }
}
