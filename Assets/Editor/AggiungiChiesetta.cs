using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Strumento dell'editor: aggiunge alla scena aperta una chiesetta in rovina con la porta chiusa a chiave,
// un baule con il bastone magico dentro, e la chiave in fondo alla cripta.
// Come si usa: con la scena aperta (per esempio ZonaProva), menu in alto
// "magic-gnl > Aggiungi chiesetta (alla scena aperta)", poi salvare la scena (Ctrl+S).
// A differenza di "Crea zona di prova" NON ricrea la scena: aggiunge solo oggetti nuovi, senza toccare il resto.
// Si annulla tutto con Ctrl+Z. La chiesetta è un unico oggetto ("Chiesetta"): spostandolo si sposta tutta.
// La chiave va dentro l'oggetto "Cripta" se c'è; altrimenti viene messa davanti alla chiesetta.
public static class AggiungiChiesetta
{
    const string CartellaMateriali = "Assets/Segnaposto/Materiali/";

    // A est della piazza, con la porta rivolta a ovest (verso la piazza). Zona libera da alberi e case.
    static readonly Vector3 Posizione = new Vector3(28f, 0f, 10f);
    const float RotazioneY = 90f;

    [MenuItem("magic-gnl/Aggiungi chiesetta (alla scena aperta)")]
    static void Aggiungi()
    {
        if (GameObject.Find("Chiesetta") != null &&
            !EditorUtility.DisplayDialog("Chiesetta", "Nella scena c'è già una Chiesetta. Aggiungerne un'altra?", "Aggiungi", "Annulla"))
            return;

        Undo.IncrementCurrentGroup();
        int gruppoAnnulla = Undo.GetCurrentGroup();

        Material pietra = Carica("Pietra");
        Material scura = Carica("PietraScura");
        Material legno = Carica("Legno");
        Material lapide = Carica("Lapide");

        GameObject zona = GameObject.Find("Zona");
        var chiesa = new GameObject("Chiesetta");
        Undo.RegisterCreatedObjectUndo(chiesa, "Aggiungi chiesetta");
        if (zona != null) chiesa.transform.SetParent(zona.transform, false);
        chiesa.transform.SetPositionAndRotation(Posizione, Quaternion.Euler(0f, RotazioneY, 0f));
        Transform c = chiesa.transform;

        // Misure in metri: larga 8, profonda 12, muri alti 5. Il davanti (con la porta) è verso -Z.
        Blocco("Pavimento", c, new Vector3(0f, 0.05f, 0f), new Vector3(8f, 0.1f, 12f), pietra);
        Blocco("Muro retro", c, new Vector3(0f, 2.5f, 6f), new Vector3(8f, 5f, 0.6f), pietra);
        Blocco("Muro sinistro", c, new Vector3(-4f, 2.5f, 0f), new Vector3(0.6f, 5f, 12.6f), pietra);
        Blocco("Muro destro", c, new Vector3(4f, 2.5f, 0f), new Vector3(0.6f, 5f, 12.6f), pietra);
        Blocco("Facciata sinistra", c, new Vector3(-2.5f, 2.5f, -6f), new Vector3(3f, 5f, 0.6f), pietra);
        Blocco("Facciata destra", c, new Vector3(2.5f, 2.5f, -6f), new Vector3(3f, 5f, 0.6f), pietra);
        Blocco("Architrave", c, new Vector3(0f, 4f, -6f), new Vector3(2f, 2f, 0.6f), pietra);

        // Tetto a due falde, frontoni a triangolo (un cubo girato di 45 gradi, metà nascosta nel muro) e croce.
        Blocco("Falda sinistra", c, new Vector3(-2.1f, 6.25f, 0f), new Vector3(5f, 0.4f, 13f), scura, Quaternion.Euler(0f, 0f, 30f));
        Blocco("Falda destra", c, new Vector3(2.1f, 6.25f, 0f), new Vector3(5f, 0.4f, 13f), scura, Quaternion.Euler(0f, 0f, -30f));
        Blocco("Frontone davanti", c, new Vector3(0f, 5f, -6f), new Vector3(3f, 3f, 0.6f), pietra, Quaternion.Euler(0f, 0f, 45f));
        Blocco("Frontone dietro", c, new Vector3(0f, 5f, 6f), new Vector3(3f, 3f, 0.6f), pietra, Quaternion.Euler(0f, 0f, 45f));
        Blocco("Croce verticale", c, new Vector3(0f, 8f, -6.2f), new Vector3(0.25f, 1.4f, 0.25f), scura);
        Blocco("Croce orizzontale", c, new Vector3(0f, 8.25f, -6.2f), new Vector3(0.9f, 0.25f, 0.25f), scura);

        // Interno: altare in fondo e quattro panche.
        Blocco("Altare", c, new Vector3(0f, 0.6f, 4.6f), new Vector3(2.2f, 1f, 1f), lapide);
        foreach (float z in new[] { -3f, -1.2f })
        {
            Blocco("Panca", c, new Vector3(-1.9f, 0.35f, z), new Vector3(2.4f, 0.5f, 0.5f), legno);
            Blocco("Panca", c, new Vector3(1.9f, 0.35f, z), new Vector3(2.4f, 0.5f, 0.5f), legno);
        }

        // Luce calda all'interno, così di notte si vede.
        var oggettoLuce = new GameObject("Luce chiesetta");
        oggettoLuce.transform.SetParent(c, false);
        oggettoLuce.transform.localPosition = new Vector3(0f, 3.5f, 2f);
        Light luce = oggettoLuce.AddComponent<Light>();
        luce.type = LightType.Point;
        luce.color = new Color(1f, 0.75f, 0.45f);
        luce.range = 12f;
        luce.intensity = 1.4f;

        // Porta di legno chiusa a chiave: si apre salendo dentro l'architrave.
        GameObject porta = Blocco("Porta chiesetta", c, new Vector3(0f, 1.5f, -6f), new Vector3(2f, 3f, 0.25f), legno);
        Porta componentePorta = porta.AddComponent<Porta>();
        var impostazioni = new SerializedObject(componentePorta);
        impostazioni.FindProperty("verso").enumValueIndex = (int)Porta.Verso.Su;
        impostazioni.ApplyModifiedPropertiesWithoutUndo();
        porta.AddComponent<Serratura>(); // codice "chiesa", uguale a quello della chiave

        // Baule davanti all'altare, con il davanti verso la porta.
        var baule = new GameObject("Baule chiesetta");
        baule.transform.SetParent(c, false);
        baule.transform.localPosition = new Vector3(0f, 0.1f, 3f);
        baule.AddComponent<Baule>();

        // Chiave in fondo alla cripta, oltre la porta con la leva e la trappola.
        var chiave = new GameObject("Chiave della chiesa");
        Undo.RegisterCreatedObjectUndo(chiave, "Aggiungi chiave");
        GameObject cripta = GameObject.Find("Cripta");
        if (cripta != null)
        {
            chiave.transform.SetParent(cripta.transform, false);
            chiave.transform.localPosition = new Vector3(0f, 1f, 2f);
        }
        else
        {
            chiave.transform.position = Posizione + new Vector3(-8f, 1f, 0f);
            Debug.LogWarning("Cripta non trovata: la chiave è stata messa davanti alla chiesetta.");
        }
        chiave.AddComponent<Chiave>();

        Undo.CollapseUndoOperations(gruppoAnnulla);
        EditorSceneManager.MarkSceneDirty(chiesa.scene);
        Selection.activeGameObject = chiesa;
        Debug.Log("Chiesetta aggiunta. La chiave è nella cripta. Salva la scena con Ctrl+S.");
    }

    static Material Carica(string nome)
    {
        Material materiale = AssetDatabase.LoadAssetAtPath<Material>(CartellaMateriali + nome + ".mat");
        if (materiale == null) Debug.LogWarning("Materiale non trovato: " + CartellaMateriali + nome + ".mat");
        return materiale;
    }

    static GameObject Blocco(string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale)
    {
        return Blocco(nome, genitore, posizione, scala, materiale, Quaternion.identity);
    }

    static GameObject Blocco(string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale, Quaternion rotazione)
    {
        GameObject oggetto = GameObject.CreatePrimitive(PrimitiveType.Cube);
        oggetto.name = nome;
        oggetto.transform.SetParent(genitore, false);
        oggetto.transform.localPosition = posizione;
        oggetto.transform.localRotation = rotazione;
        oggetto.transform.localScale = scala;
        if (materiale != null) oggetto.GetComponent<Renderer>().sharedMaterial = materiale;
        return oggetto;
    }
}
