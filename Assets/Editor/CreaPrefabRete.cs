using System.IO;
using UnityEditor;
using Unity.Netcode;
using UnityEngine;

// Crea da solo il prefab di rete del giocatore (Assets/Resources/Rete/GiocatoreRete.prefab), usato da ReteCoop.
// Come funziona: all'apertura di Unity controlla se il prefab esiste; se manca lo crea. Non serve fare niente.
// Se vuoi rifarlo: cancellare il prefab e riaprire Unity, oppure menu "magic-gnl > Rete: ricrea il prefab del giocatore".
// Il prefab (e il suo file .meta) vanno poi committati, così tutti i PC hanno lo stesso identico.
[InitializeOnLoad]
public static class CreaPrefabRete
{
    const string Cartella = "Assets/Resources/Rete";
    const string Percorso = Cartella + "/GiocatoreRete.prefab";

    static CreaPrefabRete()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && AssetDatabase.LoadAssetAtPath<GameObject>(Percorso) == null)
                Crea();
        };
    }

    [MenuItem("magic-gnl/Rete: ricrea il prefab del giocatore")]
    static void Ricrea()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Percorso) != null) AssetDatabase.DeleteAsset(Percorso);
        Crea();
    }

    static void Crea()
    {
        if (!Directory.Exists(Cartella)) Directory.CreateDirectory(Cartella);

        var radice = new GameObject("GiocatoreRete");
        radice.AddComponent<NetworkObject>();
        radice.AddComponent<GiocatoreRete>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(radice, Percorso);
        Object.DestroyImmediate(radice);

        // Netcode riconosce il prefab dal suo "numero di identità" (GlobalObjectIdHash). Di solito lo assegna da solo;
        // se è ancora zero lo scriviamo noi, ricavandolo dal codice univoco (GUID) del file.
        var oggettoRete = prefab.GetComponent<NetworkObject>();
        var serializzato = new SerializedObject(oggettoRete);
        var campo = serializzato.FindProperty("GlobalObjectIdHash");
        if (campo != null && campo.uintValue == 0)
        {
            string guid = AssetDatabase.AssetPathToGUID(Percorso);
            campo.uintValue = (uint)(guid.GetHashCode() | 1);
            serializzato.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
        }
        AssetDatabase.Refresh();
        Debug.Log("Creato il prefab di rete del giocatore: " + Percorso + ". Ricordati di committarlo insieme al suo .meta.");
    }
}
