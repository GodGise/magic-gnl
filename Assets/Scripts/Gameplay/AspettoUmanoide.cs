using UnityEngine;

// Aspetto provvisorio da figura umana per il giocatore e i nemici di prova.
// A cosa serve: finché non arrivano i modelli veri di Nazar, sostituisce la capsula (giocatore)
// e il cilindro (nemici) con una figura fatta di blocchi in stile PS2: gambe, busto, braccia,
// collo, testa e una fascia sul viso che mostra dove guarda. Mentre si muove, gambe e braccia
// oscillano (vedi AnimazioneUmanoide). È solo l'aspetto: urti, colpi e collider restano quelli di prima.
// Come montarlo: non serve montarlo su niente. GiocatoreControllo e Bersaglio lo chiamano da soli
// quando parte il gioco (Play). Agisce solo su capsule e cilindri di Unity: quando un personaggio
// avrà un modello vero, la figura provvisoria non viene più creata. A gioco fermo, nella vista
// Scene, si vedono ancora capsule e cilindri.
public static class AspettoUmanoide
{
    const string NomeFigura = "Aspetto umanoide";

    // Arma provvisoria tenuta nella mano destra: fa capire meglio i movimenti d'attacco.
    public enum Arma { Nessuna, Spada, Mazza }

    // Crea la figura come figlia di "chi" e nasconde la forma originale.
    // coloreViso: colore della fascia sul viso (scura per il giocatore, rosso cupo per i nemici).
    public static void Prepara(GameObject chi, Color coloreViso, Arma arma = Arma.Nessuna)
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
        // Gambe e braccia stanno sotto un "perno" (anca o spalla), così l'animazione le fa ruotare
        // dall'attaccatura come vere gambe e braccia, invece che attorno al loro centro.
        Transform ancaSinistra = Perno(figura, "Anca sinistra", new Vector3(-0.13f, -0.1f, 0f));
        Transform ancaDestra = Perno(figura, "Anca destra", new Vector3(0.13f, -0.1f, 0f));
        Transform spallaSinistra = Perno(figura, "Spalla sinistra", new Vector3(-0.38f, 0.58f, 0f));
        Transform spallaDestra = Perno(figura, "Spalla destra", new Vector3(0.38f, 0.58f, 0f));

        Parte(ancaSinistra, "Gamba sinistra", PrimitiveType.Cube, new Vector3(0f, -0.45f, 0f), new Vector3(0.22f, 0.9f, 0.25f), corpo);
        Parte(ancaDestra, "Gamba destra", PrimitiveType.Cube, new Vector3(0f, -0.45f, 0f), new Vector3(0.22f, 0.9f, 0.25f), corpo);
        Parte(figura, "Busto", PrimitiveType.Cube, new Vector3(0f, 0.25f, 0f), new Vector3(0.56f, 0.7f, 0.32f), corpo);
        Parte(spallaSinistra, "Braccio sinistro", PrimitiveType.Cube, new Vector3(0f, -0.36f, 0f), new Vector3(0.17f, 0.72f, 0.2f), corpo);
        Parte(spallaDestra, "Braccio destro", PrimitiveType.Cube, new Vector3(0f, -0.36f, 0f), new Vector3(0.17f, 0.72f, 0.2f), corpo);
        Parte(figura, "Collo", PrimitiveType.Cube, new Vector3(0f, 0.65f, 0f), new Vector3(0.14f, 0.1f, 0.14f), corpo);
        Parte(figura, "Testa", PrimitiveType.Sphere, new Vector3(0f, 0.82f, 0f), new Vector3(0.32f, 0.34f, 0.32f), corpo);
        Parte(figura, "Viso", PrimitiveType.Cube, new Vector3(0f, 0.84f, 0.14f), new Vector3(0.24f, 0.07f, 0.06f), viso);

        if (arma != Arma.Nessuna) CreaArma(spallaDestra, arma, corpo);

        // La forma originale e il blocchetto "Direzione" del giocatore non servono più: il viso mostra dove guarda.
        originale.enabled = false;
        Transform direzione = chi.transform.Find("Direzione");
        if (direzione != null && direzione.TryGetComponent(out Renderer blocchetto)) blocchetto.enabled = false;

        // Camminata: gambe e braccia oscillano mentre il personaggio si muove (vedi AnimazioneUmanoide).
        chi.AddComponent<AnimazioneUmanoide>().Imposta(figura, ancaSinistra, ancaDestra, spallaSinistra, spallaDestra);
    }

    // L'arma prosegue la linea del braccio oltre la mano, un po' inclinata in avanti:
    // a braccio giù punta verso terra davanti ai piedi, a braccio alzato punta in avanti.
    static void CreaArma(Transform spalla, Arma arma, Material materialeBase)
    {
        Transform mano = Perno(spalla, "Mano destra", new Vector3(0f, -0.72f, 0f));
        mano.localRotation = Quaternion.Euler(-35f, 0f, 0f);

        if (arma == Arma.Spada)
        {
            Material ferro = new Material(materialeBase) { color = new Color(0.6f, 0.62f, 0.66f) };
            Material cuoio = new Material(materialeBase) { color = new Color(0.25f, 0.15f, 0.08f) };
            Parte(mano, "Elsa", PrimitiveType.Cube, new Vector3(0f, -0.04f, 0f), new Vector3(0.22f, 0.04f, 0.06f), cuoio);
            Parte(mano, "Lama", PrimitiveType.Cube, new Vector3(0f, -0.46f, 0f), new Vector3(0.06f, 0.8f, 0.03f), ferro);
        }
        else
        {
            Material legno = new Material(materialeBase) { color = new Color(0.3f, 0.2f, 0.12f) };
            Parte(mano, "Manico mazza", PrimitiveType.Cube, new Vector3(0f, -0.3f, 0f), new Vector3(0.07f, 0.6f, 0.07f), legno);
            Parte(mano, "Testa mazza", PrimitiveType.Cube, new Vector3(0f, -0.64f, 0f), new Vector3(0.2f, 0.22f, 0.2f), legno);
        }
    }

    static Transform Perno(Transform genitore, string nome, Vector3 posizione)
    {
        Transform perno = new GameObject(nome).transform;
        perno.SetParent(genitore, false);
        perno.localPosition = posizione;
        return perno;
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
