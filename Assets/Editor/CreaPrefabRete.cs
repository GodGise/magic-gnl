using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using UnityEngine;

// Crea da soli i due prefab di rete usati dal co-op (vedi ReteCoop):
//   - Assets/Resources/Rete/GiocatoreRete.prefab: la figura di ogni giocatore vista dagli altri (GiocatoreRete);
//   - Assets/Resources/Rete/MondoRete.prefab: nemici, ora del giorno e sfere condivisi (MondoRete).
// Come funziona: all'apertura di Unity controlla se ci sono; se mancano li crea. Non serve fare niente.
// Per rifarli: menu "magic-gnl > Rete: ricrea i prefab di rete".
// I prefab (con i loro file .meta) vanno poi committati, così tutti i PC hanno gli stessi identici.
[InitializeOnLoad]
public static class CreaPrefabRete
{
    const string Cartella = "Assets/Resources/Rete";
    const string Giocatore = Cartella + "/GiocatoreRete.prefab";
    const string Mondo = Cartella + "/MondoRete.prefab";

    static CreaPrefabRete()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Giocatore) == null) Crea<GiocatoreRete>(Giocatore, "GiocatoreRete");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Mondo) == null) Crea<MondoRete>(Mondo, "MondoRete");
        };
    }

    [MenuItem("magic-gnl/Rete: ricrea i prefab di rete")]
    static void Ricrea()
    {
        AssetDatabase.DeleteAsset(Giocatore);
        AssetDatabase.DeleteAsset(Mondo);
        Crea<GiocatoreRete>(Giocatore, "GiocatoreRete");
        Crea<MondoRete>(Mondo, "MondoRete");
    }

    static void Crea<T>(string percorso, string nome) where T : MonoBehaviour
    {
        if (!Directory.Exists(Cartella)) Directory.CreateDirectory(Cartella);

        // Si costruisce in una scena "di anteprima" vuota: così non si tocca la scena aperta (per esempio ZonaProva
        // di Lorenzo) e Netcode non scambia il prefab per un oggetto messo in una scena.
        var anteprima = EditorSceneManager.NewPreviewScene();
        var radice = new GameObject(nome);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(radice, anteprima);
        radice.AddComponent<NetworkObject>();
        radice.AddComponent<T>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(radice, percorso);
        Object.DestroyImmediate(radice);
        EditorSceneManager.ClosePreviewScene(anteprima);

        // Netcode riconosce il prefab dal suo "numero di identità" (GlobalObjectIdHash), che assegna da solo
        // quando il prefab viene salvato. Lo si fa salvare di nuovo per sicurezza; se resta zero lo scriviamo noi.
        AssetDatabase.ForceReserializeAssets(new[] { percorso });
        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(percorso);
        var serializzato = new SerializedObject(prefab.GetComponent<NetworkObject>());
        var campo = serializzato.FindProperty("GlobalObjectIdHash");
        if (campo != null && campo.uintValue == 0)
        {
            uint numero = 2166136261;
            foreach (char c in AssetDatabase.AssetPathToGUID(percorso)) { numero ^= c; numero *= 16777619; }
            campo.uintValue = numero | 1;
            serializzato.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
        }
        AssetDatabase.Refresh();
        Debug.Log("[Rete] Creato il prefab " + percorso + ". Ricordati di committarlo insieme al suo .meta.");
    }
}
