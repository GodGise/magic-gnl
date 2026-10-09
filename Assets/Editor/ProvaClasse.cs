using UnityEditor;
using UnityEngine;

// Strumento di prova per le classi: sceglie con quale classe si gioca premendo Play in una scena di gioco
// (per esempio la zona di prova), senza passare dal menu principale e senza toccare la scena.
// Appena parte il Play mette nello zaino tutti gli oggetti che quella classe può usare:
// armi, armature, scudi, bastoni, libri e incantesimi della sua classe, più gli amuleti
// (gli amuleti sono in comune fra le classi; "Ultimo respiro" solo per il Ladro).
// Come si usa: menu "magic-gnl > Prova classe > Gioca come Ladro" (o Guerriero, o Stregone), poi Play e Tab.
// La scelta resta salvata anche chiudendo Unity. "Smetti di riempire lo zaino" lascia la classe ma non aggiunge
// più oggetti al Play. Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
[InitializeOnLoad]
public static class ProvaClasse
{
    const string Cartella = "magic-gnl/Prova classe/";
    const string ChiaveRiempi = "magic-gnl.ProvaClasse.Riempi";

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

    [MenuItem(Cartella + "Smetti di riempire lo zaino")]
    static void Smetti()
    {
        EditorPrefs.SetBool(ChiaveRiempi, false);
        Debug.Log("Prova classe: al Play lo zaino non si riempie più. La classe resta " + SceltaPartita.Classe + ".");
    }

    // Spunta accanto alla classe scelta.
    [MenuItem(Cartella + "Gioca come Guerriero", true)]
    static bool SpuntaGuerriero() => Spunta(ClasseGiocatore.Guerriero, "Gioca come Guerriero");
    [MenuItem(Cartella + "Gioca come Ladro", true)]
    static bool SpuntaLadro() => Spunta(ClasseGiocatore.Ladro, "Gioca come Ladro");
    [MenuItem(Cartella + "Gioca come Stregone", true)]
    static bool SpuntaStregone() => Spunta(ClasseGiocatore.Stregone, "Gioca come Stregone");

    static bool Spunta(ClasseGiocatore classe, string voce)
    {
        Menu.SetChecked(Cartella + voce, SceltaPartita.Classe == classe && EditorPrefs.GetBool(ChiaveRiempi, false));
        return true;
    }

    static void Scegli(ClasseGiocatore classe)
    {
        SceltaPartita.Classe = classe;
        EditorPrefs.SetBool(ChiaveRiempi, true);
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
    }

    // Stessa regola dell'inventario: oggetti della propria classe, amuleti per tutti tranne Ultimo respiro (solo Ladro).
    static bool Usabile(DatiOggetto oggetto, ClasseGiocatore classe)
    {
        if (oggetto is DatiAmuleto amuleto)
            return amuleto.effetto != DatiAmuleto.Effetto.SvanireNellOmbra || classe == ClasseGiocatore.Ladro;
        return oggetto.classe == classe;
    }
}
