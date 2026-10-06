using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Strumento dell'editor: aggiunge alla scena aperta una strettoia di prova, per provare come il personaggio
// si adatta agli spazi stretti (vedi PassaggioStretto).
// Come si usa: con la scena aperta, menu in alto "magic-gnl > Aggiungi strettoia di prova (alla scena aperta)",
// poi salvare la scena (Ctrl+S). Non ricrea la scena: aggiunge solo l'oggetto "Strettoia di prova".
// La strettoia compare 6 metri davanti al Giocatore (o al centro della vista Scene se il Giocatore non c'è),
// e si restringe a gradini: 1,6 m, poi 1,2 m, poi 0,75 m, e alla fine si riapre.
// Si annulla con Ctrl+Z; per toglierla basta cancellare l'oggetto. Spostandola si sposta tutta.
public static class AggiungiStrettoia
{
    // Tratti della strettoia: larghezza del passaggio e lunghezza del tratto, in metri.
    static readonly float[] Larghezze = { 1.6f, 1.2f, 0.75f };
    static readonly float[] Lunghezze = { 2f, 2f, 4f };
    const float AltezzaMuri = 3f;
    const float SpessoreMuri = 0.6f;

    [MenuItem("magic-gnl/Aggiungi strettoia di prova (alla scena aperta)")]
    static void Aggiungi()
    {
        Material pietra = AssetDatabase.LoadAssetAtPath<Material>("Assets/Segnaposto/Materiali/PietraScura.mat");

        // Davanti al giocatore, rivolta come lui: si entra camminando dritti.
        Vector3 posizione;
        Quaternion rotazione;
        GameObject giocatore = GameObject.Find("Giocatore");
        if (giocatore != null)
        {
            Vector3 avanti = giocatore.transform.forward;
            avanti.y = 0f;
            if (avanti.sqrMagnitude < 0.0001f) avanti = Vector3.forward;
            avanti.Normalize();
            posizione = giocatore.transform.position + avanti * 6f;
            posizione.y = giocatore.transform.position.y - 1f; // ai piedi del giocatore
            rotazione = Quaternion.LookRotation(avanti);
        }
        else
        {
            posizione = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : Vector3.zero;
            rotazione = Quaternion.identity;
        }

        var strettoia = new GameObject("Strettoia di prova");
        Undo.RegisterCreatedObjectUndo(strettoia, "Aggiungi strettoia di prova");
        strettoia.transform.SetPositionAndRotation(posizione, rotazione);

        float z = 0f;
        for (int i = 0; i < Larghezze.Length; i++)
        {
            float meta = Larghezze[i] / 2f + SpessoreMuri / 2f;
            float centroZ = z + Lunghezze[i] / 2f;
            Muro("Muro sinistro " + (i + 1), strettoia.transform, new Vector3(-meta, AltezzaMuri / 2f, centroZ), Lunghezze[i], pietra);
            Muro("Muro destro " + (i + 1), strettoia.transform, new Vector3(meta, AltezzaMuri / 2f, centroZ), Lunghezze[i], pietra);
            z += Lunghezze[i];
        }

        EditorSceneManager.MarkSceneDirty(strettoia.scene);
        Selection.activeGameObject = strettoia;
        Debug.Log("Strettoia di prova aggiunta davanti al giocatore. Salva la scena con Ctrl+S.");
    }

    static void Muro(string nome, Transform genitore, Vector3 posizione, float lunghezza, Material materiale)
    {
        GameObject muro = GameObject.CreatePrimitive(PrimitiveType.Cube);
        muro.name = nome;
        muro.transform.SetParent(genitore, false);
        muro.transform.localPosition = posizione;
        muro.transform.localScale = new Vector3(SpessoreMuri, AltezzaMuri, lunghezza);
        if (materiale != null) muro.GetComponent<Renderer>().sharedMaterial = materiale;
    }
}
