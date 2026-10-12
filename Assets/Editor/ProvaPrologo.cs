using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Strumento dell'editor per provare il prologo in fretta: durante il Play porta il giocatore davanti a un punto
// della quest (Maren, la catasta, il camino, Brannoc, il manichino, Edric), senza doverci camminare.
// Le quest restano quelle vere: si salta solo la strada, non i passi (dialoghi e tasti vanno fatti lo stesso).
// Come si usa: in Play, menu "magic-gnl > Prova prologo > Vai da ...". Non va montato su niente.
// "Ferisci i ratti del granaio" toglie 20 di vita a ogni ratto (per provare la fine della quest 3 in fretta);
// "Togli 50 di vita al giocatore" serve a provare le pozioni (tasto F).
public static class ProvaPrologo
{
    const string Cartella = "magic-gnl/Prova prologo/";

    [MenuItem(Cartella + "Vai da Maren")] static void Maren() => Vai("Maren");
    [MenuItem(Cartella + "Vai alla catasta del capanno")] static void Catasta() => Vai("Punto: prendi la legna");
    [MenuItem(Cartella + "Vai al camino")] static void Camino() => Vai("Punto: metti la legna nel fuoco");
    [MenuItem(Cartella + "Vai alla fucina (arrivo)")] static void Fucina() => Vai("Arrivo dal fabbro");
    [MenuItem(Cartella + "Vai da Brannoc")] static void Brannoc() => Vai("Brannoc");
    [MenuItem(Cartella + "Vai al manichino")] static void Manichino() => Vai("Manichino di Brannoc");
    [MenuItem(Cartella + "Vai da Edric (Stregone)")] static void Edric() => Vai("Edric");
    [MenuItem(Cartella + "Vai da Hobb (granaio)")] static void Hobb() => Vai("Hobb");
    [MenuItem(Cartella + "Vai al centro del granaio")] static void Granaio() => Vai("Centro del granaio");
    [MenuItem(Cartella + "Vai da Ilse (erborista)")] static void Ilse() => Vai("Ilse");
    [MenuItem(Cartella + "Vai al primo fiore di riva")] static void Fiore() => Vai("Fiore di riva 1");

    // Toglie 50 di vita al giocatore (senza scendere sotto 1): serve a provare la Pozione di Ilse (tasto F).
    [MenuItem(Cartella + "Togli 50 di vita al giocatore")]
    static void TogliVita()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        var g = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (g == null) return;
        float danno = Mathf.Min(50f, g.Vita - 1f);
        if (danno > 0f) g.RiceviColpo(danno, g.transform.position - g.transform.forward);
        Debug.Log("Prova prologo: vita del giocatore " + Mathf.CeilToInt(g.Vita) + ".");
    }

    // Toglie 20 di vita a ogni ratto del granaio ancora vivo (due volte = tutti morti): serve a provare il contatore
    // "Ratti rimasti" e la fine della quest 3 senza dover vincere lo scontro.
    [MenuItem(Cartella + "Ferisci i ratti del granaio (-20)")]
    static void FerisciRatti()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        int quanti = 0;
        foreach (var ratto in Object.FindObjectsByType<RattoGigante>(FindObjectsSortMode.None))
        {
            var b = ratto.GetComponent<Bersaglio>();
            if (b == null || b.Morto) continue;
            b.RiceviColpo(20f, b.transform.position + Vector3.up);
            quanti++;
        }
        Debug.Log(quanti > 0 ? "Prova prologo: feriti " + quanti + " ratti." : "Prova prologo: nessun ratto vivo (parla prima con Hobb).");
    }

    static void Vai(string nome)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Prova prologo: funziona solo durante il Play.");
            return;
        }
        GameObject meta = null;
        foreach (var radice in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var t in radice.GetComponentsInChildren<Transform>(true))
                if (t.name == nome) { meta = t.gameObject; break; }
        var giocatore = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (meta == null || giocatore == null)
        {
            Debug.LogWarning("Prova prologo: non trovo " + (meta == null ? "\"" + nome + "\"" : "il giocatore") + ".");
            return;
        }
        // Un passo e mezzo davanti al punto (dove guarda), girati verso di lui.
        Vector3 davanti = meta.transform.forward;
        davanti.y = 0f;
        if (davanti.sqrMagnitude < 0.01f) davanti = Vector3.forward;
        Vector3 punto = meta.transform.position + davanti.normalized * 1.6f;
        punto.y = meta.transform.position.y + 1.2f;
        var cc = giocatore.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        giocatore.transform.SetPositionAndRotation(punto, Quaternion.LookRotation(-davanti.normalized));
        if (cc != null) cc.enabled = true;
        Debug.Log("Prova prologo: giocatore portato da " + nome + ".");
    }
}
