using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Strumento dell'editor: collega le armi del Guerriero fatte da Nazar (Assets/Art/World/Item/Warrior/Warrior.fbx,
// sette armi in un file solo) agli oggetti del gioco, così in mano e a terra si vedono le armi vere al posto dei
// blocchi provvisori (Lorenzo, 14 ottobre).
// Cosa fa, per ogni arma del file:
//   1. crea un prefab con solo quell'arma in Assets/Dati/ModelliArmi/<nome-oggetto>.prefab;
//   2. lo sistema come vuole il gioco: impugnatura nel punto zero e lama (o testa) verso l'alto. L'impugnatura la
//      trova da solo: è l'estremità più stretta dell'arma, lungo il lato più lungo;
//   3. lo mette nel campo "Modello" dell'oggetto, con il materiale di Nazar dello stesso nome.
// Nomi nel file -> oggetti del gioco:
//   Sword_OldLife -> spada-della-vecchia-vita     Sword_foil -> spada-del-capitano
//   Sword_knights -> spadone-del-cavaliere        Axe_Wood -> ascia-da-legna
//   Axe_Executioner -> ascia-del-boia             Mace -> mazza-ferrata
//   HammerBones -> martello-di-ossa
// Se Nazar cambia il file, si rilancia il comando (i prefab si rifanno). Se un'arma è storta in mano si corregge
// dall'Inspector dell'oggetto (Rotazione, Posizione e Scala Modello), senza toccare il prefab.
// Come si usa: menu "magic-gnl > Armi: collega i modelli del Guerriero di Nazar". Non va montato su niente.
public static class CollegaArmiGuerriero
{
    const string File = "Assets/Art/World/Item/Warrior/Warrior.fbx";
    const string Materiali = "Assets/Art/World/Item/Warrior/";
    const string Cartella = "Assets/Dati/ModelliArmi";

    static readonly (string modello, string oggetto, string materiale)[] Armi =
    {
        ("Sword_OldLife", "spada-della-vecchia-vita", "Sword_OldLife"),
        ("Sword_foil", "spada-del-capitano", "Sword_foil"),
        ("Sword_knights", "spadone-del-cavaliere", "Sword_knights"),
        ("Axe_Wood", "ascia-da-legna", "Axe_wood"),
        ("Axe_Executioner", "ascia-del-boia", "Axe_Executioner"),
        ("Mace", "mazza-ferrata", "Mace"),
        ("HammerBones", "martello-di-ossa", "Hammer_bones"),
    };

    [MenuItem("magic-gnl/Armi: collega i modelli del Guerriero di Nazar")]
    static void Collega()
    {
        var radice = AssetDatabase.LoadAssetAtPath<GameObject>(File);
        if (radice == null) { EditorUtility.DisplayDialog("Armi", "Non trovo " + File + ".", "Ok"); return; }
        if (!AssetDatabase.IsValidFolder(Cartella)) AssetDatabase.CreateFolder("Assets/Dati", "ModelliArmi");

        var fatti = new List<string>();
        var mancanti = new List<string>();
        foreach (var (nomeModello, nomeOggetto, nomeMateriale) in Armi)
        {
            Transform parte = Cerca(radice.transform, nomeModello);
            var oggetto = Oggetto(nomeOggetto);
            if (parte == null || oggetto == null) { mancanti.Add(nomeModello + (parte == null ? " (non è nel file)" : " (manca " + nomeOggetto + ".asset)")); continue; }

            var prefab = CreaPrefab(parte, nomeOggetto);
            var materiale = AssetDatabase.LoadAssetAtPath<Material>(Materiali + nomeMateriale + ".mat");
            Undo.RecordObject(oggetto, "Collega armi");
            oggetto.modello = prefab;
            oggetto.materialeModello = materiale;
            oggetto.rotazioneModello = Vector3.zero;
            oggetto.posizioneModello = Vector3.zero;
            oggetto.scalaModello = 1f;
            EditorUtility.SetDirty(oggetto);
            fatti.Add(nomeOggetto);
        }
        AssetDatabase.SaveAssets();
        string testo = "Collegate " + fatti.Count + " armi: " + string.Join(", ", fatti) + ".";
        if (mancanti.Count > 0) testo += "\nNon collegate: " + string.Join(", ", mancanti) + ".";
        testo += "\nPer provarle: Play, menu \"magic-gnl > Prova: metti tutti gli oggetti nello zaino\", Tab e mettile in mano.";
        Debug.Log("Armi del Guerriero: " + testo);
        EditorUtility.DisplayDialog("Armi del Guerriero", testo, "Ok");
    }

    // Un prefab con la sola arma, girata e spostata: impugnatura nel punto zero, testa verso l'alto (+Y).
    static GameObject CreaPrefab(Transform parte, string nome)
    {
        var contenitore = new GameObject(nome);
        var copia = Object.Instantiate(parte.gameObject, contenitore.transform, false);
        copia.name = parte.name;
        // la sola rotazione e scala dell'arma dentro il file, senza la sua posizione (le armi sono in fila)
        copia.transform.localPosition = Vector3.zero;
        copia.transform.localRotation = parte.localRotation;
        copia.transform.localScale = parte.lossyScale;

        // tutti i vertici, nello spazio del contenitore
        var punti = new List<Vector3>();
        foreach (var filtro in copia.GetComponentsInChildren<MeshFilter>())
        {
            if (filtro.sharedMesh == null) continue;
            var verso = contenitore.transform.worldToLocalMatrix * filtro.transform.localToWorldMatrix;
            foreach (var v in filtro.sharedMesh.vertices) punti.Add(verso.MultiplyPoint3x4(v));
        }
        if (punti.Count > 0)
        {
            Bounds b = new Bounds(punti[0], Vector3.zero);
            foreach (var p in punti) b.Encapsulate(p);
            // il lato più lungo è la direzione dell'arma
            int asse = b.size.x >= b.size.y && b.size.x >= b.size.z ? 0 : b.size.y >= b.size.z ? 1 : 2;
            float lunghezza = b.size[asse];
            float inizio = b.min[asse];
            // larghezza di ogni estremità (il 15% di arma da ogni parte): la più stretta è il manico
            float larghezzaBassa = Larghezza(punti, asse, inizio, inizio + lunghezza * 0.15f);
            float larghezzaAlta = Larghezza(punti, asse, inizio + lunghezza * 0.85f, inizio + lunghezza);
            bool manicoInBasso = larghezzaBassa <= larghezzaAlta;
            Vector3 direzione = Vector3.zero;
            direzione[asse] = manicoInBasso ? 1f : -1f;   // dal manico verso la testa
            // la mano stringe a un decimo della lunghezza dalla fine del manico
            Vector3 presa = b.center;
            presa[asse] = manicoInBasso ? inizio + lunghezza * 0.1f : inizio + lunghezza * 0.9f;
            // gira tutto perché la testa vada in alto, poi porta la presa nel punto zero
            var giro = Quaternion.FromToRotation(direzione, Vector3.up);
            copia.transform.localRotation = giro * copia.transform.localRotation;
            copia.transform.localPosition = -(giro * presa);
        }

        string percorso = Cartella + "/" + nome + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(contenitore, percorso);
        Object.DestroyImmediate(contenitore);
        return prefab;
    }

    // Quanto è larga l'arma fra due punti del suo lato lungo (la diagonale delle altre due misure).
    static float Larghezza(List<Vector3> punti, int asse, float da, float a)
    {
        bool trovato = false;
        Vector2 min = Vector2.zero, max = Vector2.zero;
        int u = (asse + 1) % 3, w = (asse + 2) % 3;
        foreach (var p in punti)
        {
            if (p[asse] < da || p[asse] > a) continue;
            var q = new Vector2(p[u], p[w]);
            if (!trovato) { min = max = q; trovato = true; continue; }
            min = Vector2.Min(min, q);
            max = Vector2.Max(max, q);
        }
        return trovato ? (max - min).magnitude : float.MaxValue;
    }

    static Transform Cerca(Transform t, string nome)
    {
        foreach (var figlio in t.GetComponentsInChildren<Transform>(true))
            if (figlio.name == nome) return figlio;
        return null;
    }

    static DatiOggetto Oggetto(string file)
    {
        foreach (string guid in AssetDatabase.FindAssets(file + " t:DatiOggetto", new[] { "Assets/Dati" }))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(p) == file) return AssetDatabase.LoadAssetAtPath<DatiOggetto>(p);
        }
        return null;
    }
}
