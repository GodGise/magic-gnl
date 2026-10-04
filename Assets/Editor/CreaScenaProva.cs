using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Strumento dell'editor: crea in un clic una scena per provare il combattimento.
// Come si usa: menu in alto "magic-gnl > Crea scena di prova". Crea e salva
// Assets/Scenes/ScenaProva.unity con pavimento, giocatore (capsula), camera e un nemico di prova.
// Poi basta premere Play.
public static class CreaScenaProva
{
    const string Percorso = "Assets/Scenes/ScenaProva.unity";

    [MenuItem("magic-gnl/Crea scena di prova")]
    static void Crea()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var pavimento = GameObject.CreatePrimitive(PrimitiveType.Plane);
        pavimento.name = "Pavimento";
        pavimento.transform.localScale = new Vector3(5f, 1f, 5f);

        // Giocatore: capsula alta 2 metri, con un blocchetto davanti per vedere dove guarda.
        var giocatore = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        giocatore.name = "Giocatore";
        giocatore.transform.position = new Vector3(0f, 1f, 0f);
        Object.DestroyImmediate(giocatore.GetComponent<CapsuleCollider>());
        var controller = giocatore.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = Vector3.zero;
        giocatore.AddComponent<Resistenza>();
        giocatore.AddComponent<GiocatoreControllo>();

        var muso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        muso.name = "Direzione";
        Object.DestroyImmediate(muso.GetComponent<BoxCollider>());
        muso.transform.SetParent(giocatore.transform, false);
        muso.transform.localPosition = new Vector3(0f, 0.4f, 0.5f);
        muso.transform.localScale = new Vector3(0.3f, 0.2f, 0.4f);

        // Camera in terza persona sulla camera creata con la scena.
        var camera = Camera.main;
        if (camera != null)
        {
            var segui = camera.gameObject.AddComponent<CameraTerzaPersona>();
            segui.bersaglio = giocatore.transform;
            camera.transform.position = new Vector3(0f, 4f, -6f);
            camera.transform.LookAt(giocatore.transform);
        }

        // Nemico di prova davanti al giocatore.
        var nemico = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        nemico.name = "NemicoProva";
        nemico.transform.position = new Vector3(0f, 1f, 5f);
        nemico.transform.rotation = Quaternion.LookRotation(Vector3.back);
        nemico.AddComponent<Bersaglio>();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scena, Percorso);
        Debug.Log("Scena di prova creata in " + Percorso + ". Premi Play per provarla.");
    }
}
