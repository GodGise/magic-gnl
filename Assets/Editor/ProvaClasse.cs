using UnityEditor;
using UnityEngine;

// Strumento di prova per le classi: sceglie con quale classe si gioca premendo Play in una scena di gioco
// (per esempio la zona di prova), senza passare dal menu principale e senza toccare la scena.
// Appena parte il Play mette nello zaino tutti gli oggetti che quella classe può usare:
// armi, armature, scudi, bastoni, libri e incantesimi della sua classe, più gli amuleti
// (gli amuleti sono in comune fra le classi; "Ultimo respiro" solo per il Ladro).
// Come si usa: menu "magic-gnl > Prova classe > Gioca come Ladro" (o Guerriero, o Stregone), poi Play e Tab.
// "Ladro completo" fa lo stesso e in più gli mette addosso un equipaggiamento intero da Ladro furtivo
// (Stiletto d'ombra, Arco lungo di tasso, Manto dell'ombra, Ultimo respiro): si parte già pronti.
// "Stregone completo" mette addosso l'equipaggiamento di partenza (Bastone e Libro della vecchia vita, Tunica
// stracciata) e un incantesimo per scuola: Scintilla, Scheggia di ghiaccio, Dardo d'ombra, Fuoco fatuo.
// La scelta resta salvata anche chiudendo Unity. "Smetti di riempire lo zaino" lascia la classe ma non aggiunge
// più oggetti al Play. Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
[InitializeOnLoad]
public static class ProvaClasse
{
    const string Cartella = "magic-gnl/Prova classe/";
    const string ChiaveRiempi = "magic-gnl.ProvaClasse.Riempi";
    const string ChiaveCompleto = "magic-gnl.ProvaClasse.Completo";

    // Equipaggiamento del "Ladro completo": nomi dei file in Assets/Dati/Oggetti/Ladro/.
    static readonly string[] SetLadro = { "stiletto-d-ombra", "arco-lungo-di-tasso", "manto-dell-ombra", "ultimo-respiro" };
    // Equipaggiamento dello "Stregone completo": nomi dei file in Assets/Dati/Oggetti/Stregone/.
    static readonly string[] SetStregone = { "bastone-della-vecchia-vita", "libro-della-vecchia-vita", "tunica-stracciata",
        "scintilla", "scheggia-di-ghiaccio", "dardo-d-ombra", "fuoco-fatuo" };

    static ProvaClasse()
    {
        EditorApplication.playModeStateChanged += CambioPlay;
    }

    [MenuItem(Cartella + "Gioca come Guerriero")]
    static void Guerriero() => Scegli(ClasseGiocatore.Guerriero);

    [MenuItem(Cartella + "Gioca come Ladro")]
    static void Ladro() => Scegli(ClasseGiocatore.Ladro);

    [MenuItem(Cartella + "Gioca come Stregone")]
    static void Stregone() => Scegli(ClasseGiocatore.Stregone);

    [MenuItem(Cartella + "Gioca come Ladro completo (tutto addosso)")]
    static void LadroCompleto() => Scegli(ClasseGiocatore.Ladro, true);

    [MenuItem(Cartella + "Gioca come Stregone completo (tutto addosso)")]
    internal static void StregoneCompleto() => Scegli(ClasseGiocatore.Stregone, true);

    [MenuItem(Cartella + "Smetti di riempire lo zaino")]
    static void Smetti()
    {
        EditorPrefs.SetBool(ChiaveRiempi, false);
        EditorPrefs.SetBool(ChiaveCompleto, false);
        Debug.Log("Prova classe: al Play lo zaino non si riempie più. La classe resta " + SceltaPartita.Classe + ".");
    }

    // Spunta accanto alla classe scelta.
    [MenuItem(Cartella + "Gioca come Guerriero", true)]
    static bool SpuntaGuerriero() => Spunta(ClasseGiocatore.Guerriero, "Gioca come Guerriero");
    [MenuItem(Cartella + "Gioca come Ladro", true)]
    static bool SpuntaLadro() => Spunta(ClasseGiocatore.Ladro, "Gioca come Ladro");
    [MenuItem(Cartella + "Gioca come Stregone", true)]
    static bool SpuntaStregone() => Spunta(ClasseGiocatore.Stregone, "Gioca come Stregone");
    [MenuItem(Cartella + "Gioca come Ladro completo (tutto addosso)", true)]
    static bool SpuntaLadroCompleto() => Spunta(ClasseGiocatore.Ladro, "Gioca come Ladro completo (tutto addosso)", true);

    [MenuItem(Cartella + "Gioca come Stregone completo (tutto addosso)", true)]
    static bool SpuntaStregoneCompleto() => Spunta(ClasseGiocatore.Stregone, "Gioca come Stregone completo (tutto addosso)", true);

    static bool Spunta(ClasseGiocatore classe, string voce, bool completo = false)
    {
        Menu.SetChecked(Cartella + voce, SceltaPartita.Classe == classe && EditorPrefs.GetBool(ChiaveRiempi, false)
            && EditorPrefs.GetBool(ChiaveCompleto, false) == completo);
        return true;
    }

    static void Scegli(ClasseGiocatore classe, bool completo = false)
    {
        SceltaPartita.Classe = classe;
        EditorPrefs.SetBool(ChiaveRiempi, true);
        EditorPrefs.SetBool(ChiaveCompleto, completo);
        if (Application.isPlaying)
        {
            Debug.Log("Prova classe: " + classe + ". Meglio fermare e rifare Play, così il personaggio parte già con questa classe.");
            RiempiZaino();
        }
        else Debug.Log("Prova classe: " + classe + ". Premi Play: lo zaino si riempie con i suoi oggetti (Tab per aprirlo).");
    }

    static void CambioPlay(PlayModeStateChange stato)
    {
        if (stato != PlayModeStateChange.EnteredPlayMode || !EditorPrefs.GetBool(ChiaveRiempi, false)) return;
        // Il giocatore può comparire qualche istante dopo l'avvio (rete): si aspetta fino a 10 secondi.
        double limite = EditorApplication.timeSinceStartup + 10.0;
        EditorApplication.CallbackFunction attesa = null;
        attesa = () =>
        {
            if (!Application.isPlaying) { EditorApplication.update -= attesa; return; }
            if (Object.FindFirstObjectByType<GiocatoreControllo>() == null && EditorApplication.timeSinceStartup < limite) return;
            EditorApplication.update -= attesa;
            RiempiZaino();
        };
        EditorApplication.update += attesa;
    }

    static void RiempiZaino()
    {
        var giocatore = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null)
        {
            Debug.LogWarning("Prova classe: nessun giocatore nella scena, zaino non riempito.");
            return;
        }
        var classe = SceltaPartita.Classe;
        var zaino = Zaino.Di(giocatore);
        int messi = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:DatiOggetto"))
        {
            var oggetto = AssetDatabase.LoadAssetAtPath<DatiOggetto>(AssetDatabase.GUIDToAssetPath(guid));
            if (oggetto == null || !Usabile(oggetto, classe) || zaino.Contiene(oggetto)) continue;
            zaino.Aggiungi(oggetto);
            messi++;
        }
        Debug.Log("Prova classe: " + classe + ", messi nello zaino " + messi + " oggetti. Premi Tab per aprire l'inventario.");
        if (!EditorPrefs.GetBool(ChiaveCompleto, false)) return;
        if (classe == ClasseGiocatore.Ladro) Vesti(giocatore, zaino, classe, SetLadro, "Assets/Dati/Oggetti/Ladro");
        else if (classe == ClasseGiocatore.Stregone) Vesti(giocatore, zaino, classe, SetStregone, "Assets/Dati/Oggetti/Stregone");
    }

    // Toglie quello che non è della classe (per esempio spada e scudo di partenza) e mette addosso il set completo.
    // Quello che c'era addosso ed è della classe torna nello zaino.
    static void Vesti(GiocatoreControllo giocatore, Zaino zaino, ClasseGiocatore classe, string[] set, string cartella)
    {
        var e = giocatore.GetComponent<Equipaggiamento>();
        if (e == null)
        {
            Debug.LogWarning("Prova classe: il giocatore non ha l'Equipaggiamento, set completo non messo.");
            return;
        }
        DatiOggetto[] prima = { e.Arma, e.Scudo, e.Armatura, e.Amuleto, e.ArmaDistanza, e.Bastone, e.Libro };
        e.TogliArma(); e.TogliScudo(); e.TogliArmatura(); e.TogliAmuleto(); e.TogliArmaDistanza(); e.TogliBastone(); e.TogliLibro();
        for (int i = 0; i < Equipaggiamento.CaselleMassime; i++)
        {
            var inc = e.TogliIncantesimo(i);
            if (inc != null && !zaino.Contiene(inc)) zaino.Aggiungi(inc);
        }
        foreach (var vecchio in prima)
            if (vecchio != null && Usabile(vecchio, classe) && !zaino.Contiene(vecchio)) zaino.Aggiungi(vecchio);

        var nomi = new System.Collections.Generic.List<string>();
        foreach (string nome in set)
        {
            DatiOggetto oggetto = null;
            foreach (string guid in AssetDatabase.FindAssets(nome + " t:DatiOggetto", new[] { cartella }))
            {
                string percorso = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(percorso) == nome) oggetto = AssetDatabase.LoadAssetAtPath<DatiOggetto>(percorso);
            }
            if (oggetto == null) { Debug.LogWarning("Prova classe: non trovo " + nome + ".asset in " + cartella + "."); continue; }
            zaino.Togli(oggetto);
            e.Equipaggia(oggetto);
            nomi.Add(oggetto.name);
        }
        Debug.Log("Prova classe: " + classe + " completo, " + nomi.Count + " oggetti addosso (" + string.Join(", ", nomi) + ").");
    }

    // Stessa regola dell'inventario: oggetti della propria classe, amuleti per tutti tranne Ultimo respiro (solo Ladro).
    static bool Usabile(DatiOggetto oggetto, ClasseGiocatore classe)
    {
        if (oggetto is DatiAmuleto amuleto)
            return amuleto.effetto != DatiAmuleto.Effetto.SvanireNellOmbra || classe == ClasseGiocatore.Ladro;
        return oggetto.classe == classe || oggetto.tutteLeClassi;
    }
}
