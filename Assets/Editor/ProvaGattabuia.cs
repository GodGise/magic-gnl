using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Strumento dell'editor per provare la gattabuia in fretta: durante il Play porta il giocatore davanti ai punti
// principali (baule delle armi, entrata delle grotte, piazzola del Carceriere, portone) e ferisce il Carceriere,
// senza dover attraversare tutta la mappa. Gli eventi restano quelli veri (zone, baule, sconfitta, portone).
// Come si usa: aprire Assets/Scenes/Gattabuia.unity, Play, menu "magic-gnl > Prova gattabuia > ...".
// Non va montato su niente.
public static class ProvaGattabuia
{
    const string Cartella = "magic-gnl/Prova gattabuia/";

    [MenuItem(Cartella + "Vai al baule delle armi")] static void Baule() => Vai("Baule delle armi");
    [MenuItem(Cartella + "Vai all'entrata delle grotte")] static void Grotte() => Vai("Entrata nelle grotte");
    [MenuItem(Cartella + "Vai davanti alla piazzola del Carceriere")] static void Piazzola() => Vai("Davanti alla piazzola");
    [MenuItem(Cartella + "Vai al portone")] static void Portone() => Vai("Apri il portone");
    [MenuItem(Cartella + "Ferisci il Carceriere (metà vita)")] static void Ferisci() => Carceriere(0.5f);
    [MenuItem(Cartella + "Sconfiggi il Carceriere")] static void Sconfiggi() => Carceriere(1.1f);

    static void Carceriere(float parte)
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova gattabuia: funziona solo durante il Play."); return; }
        var c = Object.FindFirstObjectByType<Carceriere>();
        var b = c != null ? c.GetComponent<Bersaglio>() : null;
        if (b == null || b.Morto) { Debug.LogWarning("Prova gattabuia: non trovo il Carceriere vivo."); return; }
        b.RiceviColpo(b.VitaMassima * parte, b.transform.position + Vector3.up);
        Debug.Log("Prova gattabuia: Carceriere ferito, vita " + b.Vita + ".");
    }

    static void Vai(string nome)
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova gattabuia: funziona solo durante il Play."); return; }
        GameObject meta = null;
        for (int k = 0; k < SceneManager.sceneCount && meta == null; k++)
            foreach (var radice in SceneManager.GetSceneAt(k).GetRootGameObjects())
                foreach (var t in radice.GetComponentsInChildren<Transform>(true))
                    if (t.name == nome) { meta = t.gameObject; break; }
        var giocatore = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (meta == null || giocatore == null)
        {
            Debug.LogWarning("Prova gattabuia: non trovo " + (meta == null ? "\"" + nome + "\"" : "il giocatore") + ".");
            return;
        }
        Vector3 punto = meta.transform.position;
        punto.y = 1.2f;
        var cc = giocatore.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        giocatore.transform.position = punto;
        if (cc != null) cc.enabled = true;
        Debug.Log("Prova gattabuia: giocatore portato a " + nome + ".");
    }
}
