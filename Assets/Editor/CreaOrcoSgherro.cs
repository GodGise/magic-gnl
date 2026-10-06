using UnityEditor;
using UnityEngine;

// Strumento dell'editor: crea il prefab dell'orco sgherro, il nemico comune che si trova in giro.
// A cosa serve: tutti gli orchi semplici del gioco sono copie di questo prefab. Per bilanciarli si cambia
// il prefab (doppio clic su Assets/Segnaposto/Nemici/orco-sgherro.prefab, poi Inspector) e tutte le copie
// nelle scene si aggiornano insieme.
// Come si usa: menu in alto "magic-gnl > Crea prefab Orco sgherro". Se il prefab esiste già chiede prima
// di rifarlo, perché rifarlo rimette i valori di partenza qui sotto. Poi si trascina il prefab nella scena.
// Il menu "magic-gnl > Crea Villaggio Lago Nero" usa questo prefab per l'orco della piazza (e lo crea se manca).
//
// Valori di partenza (inizio del gioco, arma iniziale da 25, nessuna armatura per il giocatore):
// - vita 160 e armatura 25: un colpo normale del giocatore fa 20, quindi servono 8 colpi (6-7 con dei critici);
// - danno 26: il giocatore (100 di vita) muore al 4° colpo non parato, al 3° se arriva un critico;
// - critico 10% da ×1,5 (39 di danno): chi non sta attento lo paga caro;
// - attacca ogni 2,5 secondi, con 0,7 secondi di rosso prima del colpo.
public static class CreaOrcoSgherro
{
    public const string Percorso = "Assets/Segnaposto/Nemici/orco-sgherro.prefab";
    const string Cartella = "Assets/Segnaposto/Nemici";
    const float Grandezza = 1.1f;

    [MenuItem("magic-gnl/Crea prefab Orco sgherro")]
    static void Menu()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Percorso) != null &&
            !EditorUtility.DisplayDialog("Orco sgherro", "Il prefab esiste già. Rifarlo rimette i valori di partenza e cancella le modifiche fatte al prefab. Rifarlo?", "Rifallo", "Annulla"))
            return;
        Selection.activeObject = Crea();
        Debug.Log("Prefab dell'orco sgherro creato in " + Percorso + ". Trascinalo nella scena per aggiungere un orco.");
    }

    // Il prefab: quello che c'è già, oppure uno nuovo.
    public static GameObject Prefab()
    {
        GameObject esistente = AssetDatabase.LoadAssetAtPath<GameObject>(Percorso);
        return esistente != null ? esistente : Crea();
    }

    // Mette nella scena aperta una copia del prefab con i piedi in quel punto, rivolta in quella direzione.
    public static GameObject Metti(string nome, Vector3 piedi, Vector3 avanti, Transform genitore = null)
    {
        var orco = (GameObject)PrefabUtility.InstantiatePrefab(Prefab());
        orco.name = nome;
        if (genitore != null) orco.transform.SetParent(genitore, true);
        orco.transform.SetPositionAndRotation(piedi + Vector3.up * Grandezza, Quaternion.LookRotation(avanti));
        return orco;
    }

    static GameObject Crea()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Segnaposto")) AssetDatabase.CreateFolder("Assets", "Segnaposto");
        if (!AssetDatabase.IsValidFolder(Cartella)) AssetDatabase.CreateFolder("Assets/Segnaposto", "Nemici");

        var orco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        orco.name = "orco-sgherro";
        orco.transform.localScale = Vector3.one * Grandezza;
        orco.GetComponent<Renderer>().sharedMaterial = Pelle();

        var statistiche = orco.AddComponent<Statistiche>();
        Imposta(statistiche, "armatura", 25f);
        Imposta(statistiche, "bonusDanno", 0f);
        Imposta(statistiche, "probabilitaCritico", 10f);
        Imposta(statistiche, "moltiplicatoreCritico", 1.5f);

        var bersaglio = orco.AddComponent<Bersaglio>();
        bersaglio.attaccaIlGiocatore = false; // lo accende InseguimentoNemico quando vede il giocatore
        var b = new SerializedObject(bersaglio);
        b.FindProperty("vitaMassima").floatValue = 160f;
        b.FindProperty("rinasce").boolValue = false; // nel mondo i nemici uccisi restano morti
        b.FindProperty("intervalloAttacchi").floatValue = 2.5f;
        b.FindProperty("preavviso").floatValue = 0.7f;
        b.FindProperty("portataAttacco").floatValue = 2.5f;
        b.FindProperty("dannoAttacco").floatValue = 26f;
        b.ApplyModifiedPropertiesWithoutUndo();

        orco.AddComponent<InseguimentoNemico>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(orco, Percorso);
        Object.DestroyImmediate(orco);
        return prefab;
    }

    static void Imposta(Object componente, string campo, float valore)
    {
        var so = new SerializedObject(componente);
        so.FindProperty(campo).floatValue = valore;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Material Pelle()
    {
        string percorso = Cartella + "/pelle-orco.mat";
        var materiale = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (materiale != null) return materiale;
        materiale = new Material(Shader.Find("Standard")) { color = new Color(0.28f, 0.36f, 0.2f) };
        materiale.SetFloat("_Glossiness", 0.05f);
        AssetDatabase.CreateAsset(materiale, percorso);
        return materiale;
    }
}
