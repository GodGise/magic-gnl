using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Strumento dell'editor per provare il prologo in fretta: durante il Play porta il giocatore davanti a un punto
// della quest (Maren, la catasta, il camino, Brannoc, il manichino, Edric), senza doverci camminare.
// Le quest restano quelle vere: si salta solo la strada, non i passi (dialoghi e tasti vanno fatti lo stesso).
// Come si usa: in Play, menu "magic-gnl > Prova prologo > Vai da ...". Non va montato su niente.
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
