using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Strumento dell'editor: pezzi in comune ai comandi che preparano le quest del prologo (CreaPrologoQuest1,
// CreaPrologoQuest2): blocchi, personaggi provvisori, materiali, collegamenti degli eventi, ricerca degli oggetti.
// Non va montato su niente: è usato solo dai comandi dell'editor e non finisce nel gioco.
internal static class AiutiPrologo
{
    const string Cartella = "Assets/Segnaposto/Prologo";

    // ---------- materiali provvisori (creati una volta in Assets/Segnaposto/Prologo) ----------

    internal static Material Legno => Materiale("prologo-legno", new Color(0.36f, 0.25f, 0.16f));
    internal static Material LegnoScuro => Materiale("prologo-pavimento", new Color(0.22f, 0.16f, 0.11f));
    internal static Material Pietra => Materiale("prologo-pietra", new Color(0.42f, 0.4f, 0.37f));
    internal static Material PietraScura => Materiale("prologo-pietra-scura", new Color(0.27f, 0.26f, 0.25f));
    internal static Material Paglia => Materiale("prologo-paglia", new Color(0.5f, 0.42f, 0.25f));
    internal static Material Fuoco => Materiale("prologo-fuoco", new Color(1f, 0.5f, 0.15f), true);
    internal static Material Ferro => Materiale("prologo-ferro", new Color(0.3f, 0.31f, 0.33f));
    internal static Material Terra => Materiale("prologo-terra", new Color(0.33f, 0.27f, 0.19f));
    internal static Material Stoffa => Materiale("prologo-stoffa", new Color(0.55f, 0.5f, 0.4f));
    internal static Material Tappeto => Materiale("prologo-tappeto", new Color(0.4f, 0.17f, 0.12f));
    internal static Material Bersaglio => Materiale("prologo-bersaglio", new Color(0.72f, 0.62f, 0.42f));
    internal static Material Pelle => Materiale("prologo-pelle", new Color(0.78f, 0.62f, 0.5f));

    internal static Material Materiale(string nome, Color colore, bool luminoso = false)
    {
        string[] parti = Cartella.Split('/');
        string attuale = parti[0];
        for (int i = 1; i < parti.Length; i++)
        {
            string prossima = attuale + "/" + parti[i];
            if (!AssetDatabase.IsValidFolder(prossima)) AssetDatabase.CreateFolder(attuale, parti[i]);
            attuale = prossima;
        }
        string percorso = Cartella + "/" + nome + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (m != null) return m;
        m = new Material(Shader.Find("Standard")) { color = colore };
        m.SetFloat("_Glossiness", 0.05f);
        if (luminoso)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", colore * 2f);
        }
        AssetDatabase.CreateAsset(m, percorso);
        return m;
    }

    // ---------- forme ----------

    // Un cubo che fa da muro (con collider), statico.
    internal static GameObject Blocco(string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale, float rotazioneY = 0f)
    {
        var o = Forma(PrimitiveType.Cube, nome, genitore, posizione, scala, materiale, true);
        o.transform.localRotation = Quaternion.Euler(0f, rotazioneY, 0f);
        o.isStatic = true;
        return o;
    }

    // Un cubo di sola decorazione (senza collider): tappeti, assi, cose piccole che non devono bloccare.
    internal static GameObject Decoro(string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale, Vector3 rotazione = default)
    {
        var o = Forma(PrimitiveType.Cube, nome, genitore, posizione, scala, materiale, false);
        o.transform.localRotation = Quaternion.Euler(rotazione);
        return o;
    }

    internal static GameObject Forma(PrimitiveType tipo, string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale, bool solido = false)
    {
        var o = GameObject.CreatePrimitive(tipo);
        o.name = nome;
        o.transform.SetParent(genitore, false);
        o.transform.localPosition = posizione;
        o.transform.localScale = scala;
        o.GetComponent<Renderer>().sharedMaterial = materiale;
        if (!solido) UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());
        return o;
    }

    internal static GameObject Vuoto(string nome, Transform genitore, Vector3 posizione = default)
    {
        var o = new GameObject(nome);
        o.transform.SetParent(genitore, false);
        o.transform.localPosition = posizione;
        return o;
    }

    internal static void Luce(string nome, Transform genitore, Vector3 posizione, Color colore, float portata, float intensita)
    {
        var o = Vuoto(nome, genitore, posizione);
        var l = o.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = colore;
        l.range = portata;
        l.intensity = intensita;
    }

    // Un personaggio provvisorio: capsula del vestito, testa e un cubetto davanti alla testa per vedere dove guarda.
    // "piedi" è il punto a terra; la radice sta a metà altezza.
    internal static GameObject Personaggio(string nome, Transform genitore, Vector3 piedi, float altezza, Color vestito, bool solido)
    {
        var radice = new GameObject(nome);
        radice.transform.SetParent(genitore, false);
        radice.transform.localPosition = piedi + Vector3.up * (altezza / 2f);
        var stoffa = Materiale("prologo-" + nome.ToLowerInvariant().Replace(' ', '-'), vestito);
        var corpo = Forma(PrimitiveType.Capsule, "Corpo", radice.transform, Vector3.zero, new Vector3(altezza * 0.28f, altezza * 0.42f, altezza * 0.28f), stoffa);
        corpo.transform.localPosition = Vector3.down * altezza * 0.08f;
        Forma(PrimitiveType.Sphere, "Testa", radice.transform, Vector3.up * altezza * 0.4f, Vector3.one * altezza * 0.17f, Pelle);
        Forma(PrimitiveType.Cube, "Naso (dove guarda)", radice.transform, new Vector3(0f, altezza * 0.4f, altezza * 0.09f), Vector3.one * altezza * 0.04f, Pelle);
        if (solido)
        {
            var c = radice.AddComponent<CapsuleCollider>();
            c.height = altezza;
            c.radius = 0.3f;
        }
        return radice;
    }

    // Un manichino o una sagoma da colpire: un Bersaglio che non attacca e rinasce.
    internal static GameObject Manichino(string nome, Transform genitore, Vector3 piedi, float vita, bool sagoma)
    {
        var o = GameObject.CreatePrimitive(sagoma ? PrimitiveType.Cube : PrimitiveType.Cylinder);
        o.name = nome;
        o.transform.SetParent(genitore, false);
        o.transform.localScale = sagoma ? new Vector3(1.1f, 1.1f, 0.25f) : new Vector3(0.6f, 0.9f, 0.6f);
        o.transform.localPosition = piedi + Vector3.up * (sagoma ? 1.25f : 0.9f);
        o.GetComponent<Renderer>().sharedMaterial = Bersaglio;
        var b = o.AddComponent<global::Bersaglio>();
        b.attaccaIlGiocatore = false;
        Imposta(b, "vitaMassima", vita);
        Imposta(b, "rinasce", true);
        if (sagoma)
        {
            // il centro rosso e il palo
            Forma(PrimitiveType.Cylinder, "Centro", o.transform, new Vector3(0f, 0f, -0.52f), new Vector3(0.35f, 0.04f, 0.35f), Materiale("prologo-rosso", new Color(0.6f, 0.1f, 0.08f)))
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Decoro("Palo", genitore, piedi + Vector3.up * 0.35f, new Vector3(0.12f, 0.7f, 0.12f), Legno);
        }
        else
        {
            Decoro("Palo", genitore, piedi + Vector3.up * 0.1f, new Vector3(0.5f, 0.2f, 0.5f), LegnoScuro);
            Forma(PrimitiveType.Sphere, "Testa di paglia", o.transform, new Vector3(0f, 0.75f, 0f), new Vector3(0.7f, 0.45f, 0.7f), Paglia);
            Decoro("Braccia", o.transform, new Vector3(0f, 0.35f, 0f), new Vector3(2.2f, 0.12f, 0.2f), Legno);
        }
        return o;
    }

    // ---------- collegamenti ----------

    internal static RigaDialogo Riga(string parlante, string testo, ClasseRiga classe = ClasseRiga.Tutte) =>
        new RigaDialogo { parlante = parlante, testo = testo, soloClasse = classe };

    // Il campo (anche privato) di un componente, scritto per nome.
    internal static void Imposta(Component c, string campo, object valore)
    {
        var f = c.GetType().GetField(campo, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (f == null) { Debug.LogWarning("Prologo: campo " + campo + " non trovato in " + c.GetType().Name); return; }
        f.SetValue(c, valore);
        EditorUtility.SetDirty(c);
    }

    internal static UnityEvent Evento(Component c, string campo)
    {
        if (c == null) return null;
        var f = c.GetType().GetField(campo, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var e = (UnityEvent)f.GetValue(c);
        if (e == null) { e = new UnityEvent(); f.SetValue(c, e); }
        EditorUtility.SetDirty(c);
        return e;
    }

    // "enabled = vero/falso" di un componente, da usare in un evento.
    internal static UnityAction<bool> Abilita(Behaviour b) =>
        (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), b, typeof(Behaviour).GetProperty("enabled").GetSetMethod());

    internal static void Su(UnityEvent e, UnityAction a) => UnityEventTools.AddPersistentListener(e, a);
    internal static void Su(UnityEvent e, UnityAction<string> a, string v) => UnityEventTools.AddStringPersistentListener(e, a, v);
    internal static void Su(UnityEvent e, UnityAction<int> a, int v) => UnityEventTools.AddIntPersistentListener(e, a, v);
    internal static void Su(UnityEvent e, UnityAction<bool> a, bool v) => UnityEventTools.AddBoolPersistentListener(e, a, v);

    // Un collegamento a MetaObiettivo.Attiva, senza ripeterlo se c'è già.
    internal static int Meta(UnityEvent evento, GameObject dove)
    {
        if (evento == null || dove == null) return 0;
        var meta = dove.GetComponent<MetaObiettivo>();
        if (meta == null) meta = Undo.AddComponent<MetaObiettivo>(dove);
        for (int i = 0; i < evento.GetPersistentEventCount(); i++)
            if (evento.GetPersistentTarget(i) == meta && evento.GetPersistentMethodName(i) == "Attiva") return 0;
        UnityEventTools.AddPersistentListener(evento, meta.Attiva);
        return 1;
    }

    // ---------- ricerca ----------

    internal static GameObject Trova(Scene scena, string nome)
    {
        foreach (var radice in scena.GetRootGameObjects())
        {
            if (radice.name == nome) return radice;
            foreach (var t in radice.GetComponentsInChildren<Transform>(true))
                if (t.name == nome) return t.gameObject;
        }
        return null;
    }

    // Un oggetto creato dai menu "Crea oggetti del Guerriero/Ladro/Stregone", cercato per nome del file.
    internal static DatiOggetto Oggetto(string file)
    {
        foreach (string guid in AssetDatabase.FindAssets(file + " t:DatiOggetto", new[] { "Assets/Dati" }))
        {
            string percorso = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(percorso) == file) return AssetDatabase.LoadAssetAtPath<DatiOggetto>(percorso);
        }
        Debug.LogWarning("Prologo: non trovo l'oggetto " + file + ".asset in Assets/Dati.");
        return null;
    }

    // La porta di un edificio del villaggio: punto sulla facciata e direzione verso fuori.
    internal static bool Porta(Scene scena, string edificio, out Vector3 punto, out Vector3 fuori)
    {
        punto = Vector3.zero;
        fuori = Vector3.forward;
        var e = Trova(scena, edificio);
        if (e == null) return false;
        Transform porta = e.transform.Find("Porta");
        Vector3 centro = e.transform.position;
        centro.y = 0f;
        if (porta != null && porta.gameObject.activeSelf)
        {
            punto = porta.position;
            punto.y = 0f;
            fuori = (punto - centro).normalized;
        }
        else
        {
            fuori = -e.transform.forward;
            punto = centro + fuori * e.transform.lossyScale.z * 0.5f;
        }
        return true;
    }
}
