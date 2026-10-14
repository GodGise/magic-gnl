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
//
// Per la razzia (Lorenzo, 13 ottobre) ci sono altri due prefab, creati da soli quando servono:
// - orco corazzato (orco-corazzato.prefab): elmo e piastre (RazziatoreOrco "Corazzato") e ascia orchesca.
//   Vita 240, armatura 45, danno 34, un colpo ogni 3 s con 0,9 s di rosso, un po' più lento. Seconda ondata.
// - orco enorme (orco-enorme.prefab): il boss della piazza, alto 4,6 metri, scritto per essere perso.
//   Vita 4000, armatura 60, danno 55, un colpo ogni 3,2 s con 1,2 s di rosso, portata 4,5 m. Boss: niente esecuzioni.
// Menu: "magic-gnl > Crea prefab Orco corazzato" e "Crea prefab Orco enorme" (rifarli rimette questi numeri).
public static class CreaOrcoSgherro
{
    public const string Percorso = "Assets/Segnaposto/Nemici/orco-sgherro.prefab";
    public const string PercorsoCorazzato = "Assets/Segnaposto/Nemici/orco-corazzato.prefab";
    public const string PercorsoEnorme = "Assets/Segnaposto/Nemici/orco-enorme.prefab";
    const float GrandezzaCorazzato = 1.15f, GrandezzaEnorme = 2.3f;
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

    [MenuItem("magic-gnl/Crea prefab Orco corazzato")]
    static void MenuCorazzato()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PercorsoCorazzato) != null &&
            !EditorUtility.DisplayDialog("Orco corazzato", "Il prefab esiste già. Rifarlo rimette i valori di partenza. Rifarlo?", "Rifallo", "Annulla"))
            return;
        Selection.activeObject = CreaCorazzato();
    }

    [MenuItem("magic-gnl/Crea prefab Orco enorme")]
    static void MenuEnorme()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PercorsoEnorme) != null &&
            !EditorUtility.DisplayDialog("Orco enorme", "Il prefab esiste già. Rifarlo rimette i valori di partenza. Rifarlo?", "Rifallo", "Annulla"))
            return;
        Selection.activeObject = CreaEnorme();
    }

    // Mette un orco corazzato (crea il prefab se manca).
    public static GameObject MettiCorazzato(string nome, Vector3 piedi, Vector3 avanti, Transform genitore = null)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PercorsoCorazzato);
        if (prefab == null) prefab = CreaCorazzato();
        return MettiPrefab(prefab, nome, piedi, avanti, genitore, GrandezzaCorazzato);
    }

    // Mette l'orco enorme (crea il prefab se manca).
    public static GameObject MettiEnorme(string nome, Vector3 piedi, Vector3 avanti, Transform genitore = null)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PercorsoEnorme);
        if (prefab == null) prefab = CreaEnorme();
        return MettiPrefab(prefab, nome, piedi, avanti, genitore, GrandezzaEnorme);
    }

    static GameObject MettiPrefab(GameObject prefab, string nome, Vector3 piedi, Vector3 avanti, Transform genitore, float grandezza)
    {
        var orco = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        orco.name = nome;
        if (genitore != null) orco.transform.SetParent(genitore, true);
        orco.transform.SetPositionAndRotation(piedi + Vector3.up * grandezza, Quaternion.LookRotation(avanti));
        return orco;
    }

    static GameObject CreaCorazzato()
    {
        Cartelle();
        var orco = Base("orco-corazzato", GrandezzaCorazzato, Pelle("pelle-orco-corazzato", new Color(0.24f, 0.31f, 0.18f)),
            240f, 45f, 34f, 3f, 0.9f, 2.8f, AspettoUmanoide.Arma.Ascia, false);
        var v = new SerializedObject(orco.AddComponent<InseguimentoNemico>());
        v.FindProperty("velocitaInseguimento").floatValue = 3f;
        v.ApplyModifiedPropertiesWithoutUndo();
        var r = orco.AddComponent<RazziatoreOrco>();
        var so = new SerializedObject(r);
        so.FindProperty("corazzato").boolValue = true;
        so.FindProperty("dannoAllaCasa").floatValue = 18f;
        so.ApplyModifiedPropertiesWithoutUndo();
        Nome(orco, "nem.orco_corazzato");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(orco, PercorsoCorazzato);
        Object.DestroyImmediate(orco);
        return prefab;
    }

    static GameObject CreaEnorme()
    {
        Cartelle();
        var orco = Base("orco-enorme", GrandezzaEnorme, Pelle("pelle-orco-enorme", new Color(0.22f, 0.27f, 0.16f)),
            4000f, 60f, 55f, 3.2f, 1.2f, 4.5f, AspettoUmanoide.Arma.Mazza, true);
        var v = new SerializedObject(orco.AddComponent<InseguimentoNemico>());
        v.FindProperty("distanzaVista").floatValue = 30f;
        v.FindProperty("angoloVisivo").floatValue = 220f;
        v.FindProperty("guardaIntorno").floatValue = 70f;
        v.FindProperty("velocitaInseguimento").floatValue = 3.2f;
        v.FindProperty("distanzaAttacco").floatValue = 3.4f;
        v.FindProperty("secondiPerPerderlo").floatValue = 10f;
        v.FindProperty("distanzaMassimaDalPosto").floatValue = 60f;
        v.ApplyModifiedPropertiesWithoutUndo();
        Nome(orco, "nem.orco_enorme");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(orco, PercorsoEnorme);
        Object.DestroyImmediate(orco);
        return prefab;
    }

    static GameObject Base(string nome, float grandezza, Material pelle, float vita, float armatura, float danno,
        float intervallo, float preavviso, float portata, AspettoUmanoide.Arma arma, bool boss)
    {
        var orco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        orco.name = nome;
        orco.transform.localScale = Vector3.one * grandezza;
        orco.GetComponent<Renderer>().sharedMaterial = pelle;
        var statistiche = orco.AddComponent<Statistiche>();
        Imposta(statistiche, "armatura", armatura);
        Imposta(statistiche, "bonusDanno", 0f);
        Imposta(statistiche, "probabilitaCritico", 10f);
        Imposta(statistiche, "moltiplicatoreCritico", 1.5f);
        var bersaglio = orco.AddComponent<Bersaglio>();
        bersaglio.attaccaIlGiocatore = false;
        var b = new SerializedObject(bersaglio);
        b.FindProperty("vitaMassima").floatValue = vita;
        b.FindProperty("rinasce").boolValue = false;
        b.FindProperty("intervalloAttacchi").floatValue = intervallo;
        b.FindProperty("preavviso").floatValue = preavviso;
        b.FindProperty("portataAttacco").floatValue = portata;
        b.FindProperty("dannoAttacco").floatValue = danno;
        b.FindProperty("boss").boolValue = boss;
        b.FindProperty("armaFigura").enumValueIndex = (int)arma;
        if (boss) b.FindProperty("spintaQuandoColpito").floatValue = 0f;
        b.ApplyModifiedPropertiesWithoutUndo();
        return orco;
    }

    static void Nome(GameObject o, string chiave)
    {
        var so = new SerializedObject(o.AddComponent<NomeTradotto>());
        so.FindProperty("chiave").stringValue = chiave;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Cartelle()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Segnaposto")) AssetDatabase.CreateFolder("Assets", "Segnaposto");
        if (!AssetDatabase.IsValidFolder(Cartella)) AssetDatabase.CreateFolder("Assets/Segnaposto", "Nemici");
    }

    static Material Pelle(string file, Color colore)
    {
        string percorso = Cartella + "/" + file + ".mat";
        var materiale = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (materiale != null) return materiale;
        materiale = new Material(Shader.Find("Standard")) { color = colore };
        materiale.SetFloat("_Glossiness", 0.05f);
        AssetDatabase.CreateAsset(materiale, percorso);
        return materiale;
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
