using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Strumento dell'editor: crea in un clic una scena per provare il combattimento.
// Come si usa: menu in alto "magic-gnl > Crea scena di prova". Crea e salva
// Assets/Scenes/ScenaProva.unity con pavimento, giocatore (capsula), camera e tre nemici di prova.
// Poi basta premere Play.
// I metodi CreaGiocatore, PreparaCamera e CreaNemico sono usati anche da CreaZonaProva.
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

        var giocatore = CreaGiocatore(new Vector3(0f, 1f, 0f), 0f);
        PreparaCamera(giocatore.transform, false);

        // Tre nemici di prova davanti al giocatore: attacca solo quello al centro,
        // gli altri due servono a provare il cambio di bersaglio con la rotellina.
        CreaNemico("NemicoProva", new Vector3(0f, 1f, 5f), true);
        CreaNemico("NemicoSinistra", new Vector3(-4f, 1f, 7f), false);
        CreaNemico("NemicoDestra", new Vector3(4f, 1f, 7f), false);

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scena, Percorso);
        Debug.Log("Scena di prova creata in " + Percorso + ". Premi Play per provarla.");
    }

    // Giocatore: capsula alta 2 metri, con un blocchetto davanti per vedere dove guarda.
    internal static GameObject CreaGiocatore(Vector3 posizione, float rotazioneY)
    {
        var giocatore = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        giocatore.name = "Giocatore";
        giocatore.transform.SetPositionAndRotation(posizione, Quaternion.Euler(0f, rotazioneY, 0f));
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
        return giocatore;
    }

    // Camera in terza persona sulla camera creata con la scena; con effettoRetro aggiunge anche
    // l'effetto a bassa risoluzione in stile PS2.
    internal static void PreparaCamera(Transform giocatore, bool effettoRetro)
    {
        var camera = Camera.main;
        if (camera == null) return;

        var segui = camera.gameObject.AddComponent<CameraTerzaPersona>();
        segui.bersaglio = giocatore;
        camera.transform.position = giocatore.position + giocatore.rotation * new Vector3(0f, 3f, -6f);
        camera.transform.LookAt(giocatore);
        if (effettoRetro)
        {
            camera.gameObject.AddComponent<EffettoRetro>();
            camera.farClipPlane = 220f;
        }
    }

    internal static void CreaNemico(string nome, Vector3 posizione, bool attacca)
    {
        var nemico = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        nemico.name = nome;
        nemico.transform.position = posizione;
        nemico.transform.rotation = Quaternion.LookRotation(Vector3.back);
        var bersaglio = nemico.AddComponent<Bersaglio>();
        bersaglio.attaccaIlGiocatore = attacca;
    }
}
