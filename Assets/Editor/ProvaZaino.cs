using UnityEditor;
using UnityEngine;

// Strumento di prova per l'inventario: durante il Play mette nello zaino del giocatore
// una copia di ogni oggetto del progetto (armi, scudi, armature, amuleti), così si può provare
// l'inventario (Tab) senza piazzare oggetti nella scena.
// Come si usa: premere Play, poi menu "magic-gnl > Prova: metti tutti gli oggetti nello zaino".
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class ProvaZaino
{
    const string Voce = "magic-gnl/Prova: metti tutti gli oggetti nello zaino";

    [MenuItem(Voce)]
    static void MettiTutto()
    {
        var giocatore = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null)
        {
            Debug.LogWarning("Nessun giocatore nella scena.");
            return;
        }

        var zaino = Zaino.Di(giocatore);
        int messi = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:DatiOggetto"))
        {
            var oggetto = AssetDatabase.LoadAssetAtPath<DatiOggetto>(AssetDatabase.GUIDToAssetPath(guid));
            if (oggetto == null || zaino.Contiene(oggetto)) continue;
            zaino.Aggiungi(oggetto);
            messi++;
        }
        Debug.Log("Messi nello zaino " + messi + " oggetti. Premere Tab per aprire l'inventario.");
    }

    // La voce è attiva solo durante il Play.
    [MenuItem(Voce, true)]
    static bool SoloInPlay() => Application.isPlaying;
}
