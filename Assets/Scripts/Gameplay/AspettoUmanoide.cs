using UnityEngine;

// Aspetto provvisorio da figura umana per il giocatore e i nemici di prova.
// A cosa serve: finché non arrivano i modelli veri di Nazar, sostituisce la capsula (giocatore)
// e il cilindro (nemici) con una figura fatta di blocchi in stile PS2: gambe, busto, braccia,
// collo, testa e una fascia sul viso che mostra dove guarda. È solo l'aspetto: urti, colpi e
// collider restano quelli di prima.
// Come montarlo: non serve montarlo su niente. GiocatoreControllo e Bersaglio lo chiamano da soli
// quando parte il gioco (Play). Agisce solo su capsule e cilindri di Unity: quando un personaggio
// avrà un modello vero, la figura provvisoria non viene più creata. A gioco fermo, nella vista
// Scene, si vedono ancora capsule e cilindri.
public static class AspettoUmanoide
{
    const string NomeFigura = "Aspetto umanoide";

    // Crea la figura come figlia di "chi" e nasconde la forma originale.
    // coloreViso: colore della fascia sul viso (scura per il giocatore, rosso cupo per i nemici).
    public static void Prepara(GameObject chi, Color coloreViso)
    {
        if (chi.transform.Find(NomeFigura) != null) return; // già fatta

        MeshFilter forma = chi.GetComponent<MeshFilter>();
        MeshRenderer originale = chi.GetComponent<MeshRenderer>();
        if (forma == null || originale == null || forma.sharedMesh == null) return;

        string nomeForma = forma.sharedMesh.name;
        if (nomeForma != "Capsule" && nomeForma != "Cylinder") return; // ha già un modello vero

        Material corpo = originale.sharedMaterial;
        if (corpo == null) return;
        Material viso = new Material(corpo) { color = coloreViso };

        Transform figura = new GameObject(NomeFigura).transform;
        figura.SetParent(chi.transform, false);

        // Misure per una figura alta 2 metri: la capsula e il cilindro vanno da -1 (piedi) a +1 (testa).
        Parte(figura, "Gamba sinistra", PrimitiveType.Cube, new Vector3(-0.13f, -0.55f, 0f), new Vector3(0.22f, 0.9f, 0.25f), corpo);
        Parte(figura, "Gamba destra", PrimitiveType.Cube, new Vector3(0.13f, -0.55f, 0f), new Vector3(0.22f, 0.9f, 0.25f), corpo);
        Parte(figura, "Busto", PrimitiveType.Cube, new Vector3(0f, 0.25f, 0f), new Vector3(0.56f, 0.7f, 0.32f), corpo);
        Parte(figura, "Braccio sinistro", PrimitiveType.Cube, new Vector3(-0.38f, 0.22f, 0f), new Vector3(0.17f, 0.72f, 0.2f), corpo);
        Parte(figura, "Braccio destro", PrimitiveType.Cube, new Vector3(0.38f, 0.22f, 0f), new Vector3(0.17f, 0.72f, 0.2f), corpo);
        Parte(figura, "Collo", PrimitiveType.Cube, new Vector3(0f, 0.65f, 0f), new Vector3(0.14f, 0.1f, 0.14f), corpo);
        Parte(figura, "Testa", PrimitiveType.Sphere, new Vector3(0f, 0.82f, 0f), new Vector3(0.32f, 0.34f, 0.32f), corpo);
        Parte(figura, "Viso", PrimitiveType.Cube, new Vector3(0f, 0.84f, 0.14f), new Vector3(0.24f, 0.07f, 0.06f), viso);

        // La forma originale e il blocchetto "Direzione" del giocatore non servono più: il viso mostra dove guarda.
        originale.enabled = false;
        Transform direzione = chi.transform.Find("Direzione");
        if (direzione != null && direzione.TryGetComponent(out Renderer blocchetto)) blocchetto.enabled = false;
    }

    static void Parte(Transform genitore, string nome, PrimitiveType tipo, Vector3 posizione, Vector3 scala, Material materiale)
    {
        GameObject parte = GameObject.CreatePrimitive(tipo);
        parte.name = nome;
        // Tolto subito: le parti sono solo da vedere e non devono urtare il personaggio stesso.
        Object.DestroyImmediate(parte.GetComponent<Collider>());
        parte.transform.SetParent(genitore, false);
        parte.transform.localPosition = posizione;
        parte.transform.localScale = scala;
        parte.GetComponent<Renderer>().sharedMaterial = materiale;
    }
}
