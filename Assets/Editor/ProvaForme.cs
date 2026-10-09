using UnityEditor;
using UnityEngine;

// Strumento di prova per le forme provvisorie degli oggetti (FormeOggetti): durante il Play mette a terra,
// davanti al giocatore, una copia di ogni oggetto del progetto, in file ordinate per classe e per tipo.
// Si possono guardare tutte le forme e raccogliere con E. Spariscono quando si ferma il Play: la scena non cambia.
// Come si usa: premere Play, poi menu "magic-gnl > Prova: metti tutti gli oggetti a terra".
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class ProvaForme
{
    const string Voce = "magic-gnl/Prova: metti tutti gli oggetti a terra";

    [MenuItem(Voce)]
    static void MettiATerra()
    {
        var giocatore = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null)
        {
            Debug.LogWarning("Nessun giocatore nella scena.");
            return;
        }

        // Una fila per ogni cartella (Guerriero/Armi, Ladro/Amuleti, ...), a 4 m davanti al giocatore e poi via via più lontano.
        var file = new System.Collections.Generic.SortedDictionary<string, System.Collections.Generic.List<DatiOggetto>>();
        foreach (string guid in AssetDatabase.FindAssets("t:DatiOggetto"))
        {
            string percorso = AssetDatabase.GUIDToAssetPath(guid);
            var oggetto = AssetDatabase.LoadAssetAtPath<DatiOggetto>(percorso);
            if (oggetto == null) continue;
            string cartella = System.IO.Path.GetDirectoryName(percorso).Replace('\\', '/');
            if (!file.TryGetValue(cartella, out var elenco)) file[cartella] = elenco = new System.Collections.Generic.List<DatiOggetto>();
            elenco.Add(oggetto);
        }

        Transform g = giocatore.transform;
        Vector3 avanti = new Vector3(g.forward.x, 0f, g.forward.z).normalized;
        Vector3 destra = Vector3.Cross(Vector3.up, avanti);
        var gruppo = new GameObject("Prova forme (si cancella col Play)").transform;
        int fila = 0, messi = 0;
        foreach (var coppia in file)
        {
            var elenco = coppia.Value;
            for (int i = 0; i < elenco.Count; i++)
            {
                Vector3 punto = g.position + avanti * (4f + fila * 2.5f) + destra * ((i - (elenco.Count - 1) * 0.5f) * 1.6f);
                if (Physics.Raycast(punto + Vector3.up * 5f, Vector3.down, out RaycastHit terra, 20f, ~0, QueryTriggerInteraction.Ignore))
                    punto = terra.point;
                else punto.y = g.position.y - 1f;
                var posto = new GameObject(elenco[i].name);
                posto.transform.SetParent(gruppo, false);
                posto.transform.position = punto;
                posto.AddComponent<OggettoRaccoglibile>().Imposta(elenco[i]);
                messi++;
            }
            fila++;
        }
        Debug.Log("Messi a terra " + messi + " oggetti in " + fila + " file davanti al giocatore. Con E si raccolgono.");
    }

    [MenuItem(Voce, true)]
    static bool SoloInPlay() => Application.isPlaying;
}
