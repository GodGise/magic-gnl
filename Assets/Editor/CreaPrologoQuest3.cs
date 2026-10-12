using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: prepara nella scena VillaggioLagoNero (aperta, con le quest 1 e 2 già preparate) la quest 3
// "Ratti nel granaio" (Docs/quest-capitolo-1.md), con forme provvisorie, in un gruppo nuovo "Prologo - quest 3":
//   - un granaio grande sulla riva del lago, vicino al molo: 12 x 16 m, alto 5,5 m, portone aperto verso il
//     villaggio, soppalco in fondo, balle di fieno, sacchi, botti, un carro, travi e lanterne; al centro uno spazio
//     libero di circa 8 x 9 m per lo scontro; dentro la camera sta più vicina (ZonaCamera);
//   - Hobb davanti al portone;
//   - tre ratti giganti (RattoGigante + Bersaglio): 37 di vita, morso da 11, un balzo ogni 4 secondi a testa;
//   - tutto collegato con gli eventi:
//       fine della quest 2 (Brannoc o Edric) -> obiettivo "Vai al granaio di Hobb" (la bussola indica Hobb)
//       -> parli con Hobb -> i ratti si svegliano, obiettivo "Ratti rimasti: 3" (scende a ogni ratto ucciso),
//          suggerimenti su resistenza e balzi
//       -> morti tutti e tre: Hobb ringrazia, quest 3 finita (il sole avanza).
// Gli alberi e le case del villaggio che toccherebbero il granaio si spengono (non si cancellano).
// La vita persa contro i ratti si recupera nella quest 4 (la pozione di Ilse).
// Non salva la scena: dopo il comando si guarda il risultato e si salva con Ctrl+S.
// Se il gruppo esiste già, chiede se rifarlo da capo (le modifiche fatte a mano a quel gruppo si perdono).
// Rifacendo la quest 2 vanno rifatti anche i collegamenti della quest 3: si rilancia questo comando.
// Come si usa: aprire Assets/Scenes/VillaggioLagoNero.unity, poi menu "magic-gnl > Prologo: prepara la quest 3".
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class CreaPrologoQuest3
{
    const string NomeGruppo = "Prologo - quest 3";
    const float W = 12f, D = 16f, H = 5.5f, T = 0.35f, Porta = 4.2f;

    [MenuItem("magic-gnl/Prologo: prepara la quest 3")]
    static void Prepara()
    {
        Scene scena = SceneManager.GetActiveScene();
        var missioniOggetto = Trova(scena, "Missioni");
        var fineBrannoc = Trova(scena, "Dialogo: Brannoc dopo l'allenamento");
        var fineEdric = Trova(scena, "Dialogo: Edric dopo lo studio");
        if (scena.name != "VillaggioLagoNero" || missioniOggetto == null || fineBrannoc == null || fineEdric == null)
        {
            EditorUtility.DisplayDialog("Prologo", "Apri la scena VillaggioLagoNero e prepara prima le quest 1 e 2.", "Ok");
            return;
        }
        GameObject vecchio = Trova(scena, NomeGruppo);
        if (vecchio != null)
        {
            if (!EditorUtility.DisplayDialog("Prologo", "Il gruppo \"" + NomeGruppo + "\" esiste già. Rifarlo da capo? Le modifiche fatte a mano a quel gruppo andranno perse.", "Rifai", "Annulla")) return;
            Undo.DestroyObjectImmediate(vecchio);
        }
        var missioni = missioniOggetto.GetComponent<Missioni>();
        var gruppo = new GameObject(NomeGruppo).transform;
        Undo.RegisterCreatedObjectUndo(gruppo.gameObject, "Prologo quest 3");

        // Sulla riva, poco a sud-ovest dell'inizio del molo; il portone guarda verso il villaggio (ovest).
        float[] molo = DatiVillaggioLagoNero.Molo;
        var granaio = Vuoto("Granaio di Hobb", gruppo).transform;
        granaio.SetPositionAndRotation(new Vector3(molo[0] - 17f, 0f, molo[1] - 1.5f), Quaternion.LookRotation(Vector3.left));
        SgomberaAlberi(scena, granaio, new Vector3(0f, 0f, 2f), new Vector3(W + 3f, 0f, D + 8f));
        SgomberaCase(scena, granaio, new Vector3(0f, 0f, 2f), new Vector3(W + 3f, 0f, D + 8f));

        // ---------- muri, portone, tetto ----------
        Material legnoGranaio = Materiale("prologo-granaio", new Color(0.4f, 0.27f, 0.17f));
        Material pelliccia = Materiale("prologo-ratto", new Color(0.24f, 0.21f, 0.19f));
        Material occhi = Materiale("prologo-occhi-ratto", new Color(0.9f, 0.15f, 0.1f), true);
        Material lanterna = Materiale("prologo-lanterna", new Color(1f, 0.75f, 0.35f), true);

        Blocco("Pavimento", granaio, new Vector3(0f, 0.05f, 0f), new Vector3(W, 0.1f, D), Terra);
        Blocco("Muro di fondo", granaio, new Vector3(0f, H / 2f, -D / 2f + T / 2f), new Vector3(W, H, T), legnoGranaio);
        Blocco("Muro sinistro", granaio, new Vector3(-W / 2f + T / 2f, H / 2f, 0f), new Vector3(T, H, D), legnoGranaio);
        Blocco("Muro destro", granaio, new Vector3(W / 2f - T / 2f, H / 2f, 0f), new Vector3(T, H, D), legnoGranaio);
        float lato = (W - Porta) / 2f;
        Blocco("Facciata (sinistra)", granaio, new Vector3(-W / 2f + lato / 2f, H / 2f, D / 2f - T / 2f), new Vector3(lato, H, T), legnoGranaio);
        Blocco("Facciata (destra)", granaio, new Vector3(W / 2f - lato / 2f, H / 2f, D / 2f - T / 2f), new Vector3(lato, H, T), legnoGranaio);
        Blocco("Sopra il portone", granaio, new Vector3(0f, (3.6f + H) / 2f, D / 2f - T / 2f), new Vector3(Porta, H - 3.6f, T), legnoGranaio);
        // ante del portone aperte verso fuori
        Decoro("Anta sinistra", granaio, new Vector3(-Porta / 2f - 0.9f, 1.8f, D / 2f + 1f), new Vector3(0.12f, 3.5f, 2.1f), LegnoScuro, new Vector3(0f, -15f, 0f));
        Decoro("Anta destra", granaio, new Vector3(Porta / 2f + 0.9f, 1.8f, D / 2f + 1f), new Vector3(0.12f, 3.5f, 2.1f), LegnoScuro, new Vector3(0f, 15f, 0f));
        Blocco("Soffitto", granaio, new Vector3(0f, H + 0.1f, 0f), new Vector3(W, 0.2f, D), LegnoScuro);
        float pendenza = 35f, falda = (W / 2f + 0.6f) / Mathf.Cos(pendenza * Mathf.Deg2Rad);
        float colmo = H + 0.2f + (W / 4f) * Mathf.Tan(pendenza * Mathf.Deg2Rad);
        Decoro("Falda sinistra", granaio, new Vector3(-W / 4f, colmo, 0f), new Vector3(falda, 0.25f, D + 0.8f), Paglia, new Vector3(0f, 0f, pendenza));
        Decoro("Falda destra", granaio, new Vector3(W / 4f, colmo, 0f), new Vector3(falda, 0.25f, D + 0.8f), Paglia, new Vector3(0f, 0f, -pendenza));
        Decoro("Timpano davanti", granaio, new Vector3(0f, H + 0.2f + W * 0.17f, D / 2f - T / 2f), new Vector3(W * 0.6f, W * 0.34f, T), legnoGranaio);
        Decoro("Timpano dietro", granaio, new Vector3(0f, H + 0.2f + W * 0.17f, -D / 2f + T / 2f), new Vector3(W * 0.6f, W * 0.34f, T), legnoGranaio);

        // ---------- dentro: travi, soppalco, fieno, sacchi, botti, carro ----------
        foreach (float x in new[] { -W / 2f + 0.9f, W / 2f - 0.9f })
            foreach (float z in new[] { -4f, 0f, 4f })
                Blocco("Trave", granaio, new Vector3(x, H / 2f, z), new Vector3(0.35f, H, 0.35f), LegnoScuro);
        for (int i = -1; i <= 1; i++)
            Decoro("Trave del tetto", granaio, new Vector3(0f, H - 0.3f, i * 4f), new Vector3(W - 0.4f, 0.3f, 0.3f), LegnoScuro);
        // soppalco in fondo, a 3 m, con i pali e una scala
        Blocco("Soppalco", granaio, new Vector3(0f, 3f, -D / 2f + 2f), new Vector3(W - 0.8f, 0.2f, 3.6f), Legno);
        Blocco("Palo del soppalco 1", granaio, new Vector3(-2.5f, 1.45f, -D / 2f + 3.7f), new Vector3(0.25f, 2.9f, 0.25f), LegnoScuro);
        Blocco("Palo del soppalco 2", granaio, new Vector3(2.5f, 1.45f, -D / 2f + 3.7f), new Vector3(0.25f, 2.9f, 0.25f), LegnoScuro);
        Decoro("Scala del soppalco", granaio, new Vector3(4.2f, 1.5f, -D / 2f + 4.1f), new Vector3(0.7f, 3.2f, 0.08f), Legno, new Vector3(-15f, 0f, 0f));
        for (int i = 0; i < 4; i++)
            Blocco("Fieno sul soppalco", granaio, new Vector3(-4f + i * 2.2f, 3.55f, -D / 2f + 1.2f), new Vector3(1.6f, 0.9f, 1f), Paglia);
        // balle di fieno lungo i muri, il centro resta libero
        Vector3[] balle =
        {
            new Vector3(-4.6f, 0.45f, -2.2f), new Vector3(-4.6f, 0.45f, -0.9f), new Vector3(-4.6f, 1.35f, -1.5f),
            new Vector3(4.6f, 0.45f, 2.2f), new Vector3(4.6f, 0.45f, 3.5f), new Vector3(4.6f, 1.35f, 2.8f),
            new Vector3(-3f, 0.45f, -5.2f), new Vector3(-1.6f, 0.45f, -5.2f),
        };
        foreach (var b in balle) Blocco("Balla di fieno", granaio, b, new Vector3(1.3f, 0.9f, 1.1f), Paglia);
        foreach (var p in new[] { new Vector3(4.7f, 0f, -1.5f), new Vector3(4.3f, 0f, -2.4f), new Vector3(-4.7f, 0f, 3f) })
            Forma(PrimitiveType.Sphere, "Sacco di grano", granaio, p + Vector3.up * 0.35f, new Vector3(0.7f, 0.75f, 0.6f), Stoffa, true);
        foreach (var p in new[] { new Vector3(-4.8f, 0f, 5.6f), new Vector3(-4f, 0f, 6.2f) })
            Forma(PrimitiveType.Cylinder, "Botte", granaio, p + Vector3.up * 0.5f, new Vector3(0.8f, 0.5f, 0.8f), Legno, true);
        // il carro vicino al portone
        Blocco("Carro", granaio, new Vector3(3.9f, 0.8f, 5.9f), new Vector3(1.6f, 0.5f, 2.6f), Legno);
        foreach (float z in new[] { 5f, 6.8f })
            foreach (float x in new[] { 3f, 4.8f })
                Forma(PrimitiveType.Cylinder, "Ruota", granaio, new Vector3(x, 0.5f, z), new Vector3(1f, 0.06f, 1f), LegnoScuro).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        // lanterne
        foreach (var p in new[] { new Vector3(-W / 2f + 1.2f, 2.6f, -4f), new Vector3(W / 2f - 1.2f, 2.6f, 0f), new Vector3(-W / 2f + 1.2f, 2.6f, 4f), new Vector3(0f, 2.4f, -D / 2f + 3.9f) })
        {
            Decoro("Lanterna", granaio, p, new Vector3(0.22f, 0.3f, 0.22f), lanterna);
            Luce("Luce della lanterna", granaio, p, new Color(1f, 0.72f, 0.4f), 8f, 1.3f);
        }
        Luce("Luce dal portone", granaio, new Vector3(0f, 3f, D / 2f - 1.5f), new Color(0.9f, 0.88f, 0.8f), 10f, 0.8f);

        var zona = Undo.AddComponent<ZonaCamera>(Vuoto("Zona camera (granaio)", granaio));
        Imposta(zona, "dimensioni", new Vector3(W - 0.4f, H, D - 0.4f));
        Imposta(zona, "distanzaMassima", 4.5f);
        Imposta(zona, "inclinazioneMassima", 35f);
        var centro = Vuoto("Centro del granaio", granaio, new Vector3(0f, 0f, 0f));

        // ---------- Hobb ----------
        var hobb = Personaggio("Hobb", granaio, new Vector3(-Porta / 2f - 1.6f, 0f, D / 2f + 2.4f), 1.75f, new Color(0.42f, 0.32f, 0.2f), true);
        hobb.transform.localRotation = Quaternion.LookRotation(new Vector3(0.6f, 0f, 1f));

        // ---------- i ratti ----------
        var ratti = Vuoto("Ratti del granaio", granaio).transform;
        var gruppoRatti = Undo.AddComponent<GruppoNemici>(ratti.gameObject);
        Imposta(gruppoRatti, "chiaveRimasti", "ob.q3.ratti");
        Vector3[] posti = { new Vector3(2.4f, 0f, -2.8f), new Vector3(-2.2f, 0f, -3.6f), new Vector3(0.3f, 0f, -1f) };
        float[] ritardi = { 1.2f, 2.6f, 4f };
        for (int i = 0; i < posti.Length; i++) Ratto("Ratto gigante " + (i + 1), ratti, posti[i], ritardi[i], pelliccia, occhi);
        ratti.gameObject.SetActive(false);

        // ---------- collegamenti ----------
        var fine = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: Hobb dopo i ratti", hobb.transform));
        Imposta(fine, "righe", new[] { Riga("pg.hobb", "dlg.q3.04") });
        var dopo = Evento(fine, "alTermine");
        Su(dopo, missioni.QuestFinita, 3);
        Su(dopo, missioni.TogliObiettivo);
        Su(Evento(gruppoRatti, "quandoSconfitti"), fine.Avvia);

        var parla = Undo.AddComponent<Interlocutore>(hobb);
        Imposta(parla, "nomeChiave", "pg.hobb");
        Imposta(parla, "raggio", 3f);
        Imposta(parla, "unaVoltaSola", false);
        Imposta(parla, "righe", new[] { Riga("pg.hobb", "dlg.q3.01"), Riga("pg.hobb", "dlg.q3.02"), Riga("pg.hobb", "dlg.q3.03") });
        Imposta(parla, "righeRipetute", new[] { Riga("pg.hobb", "dlg.q3.03") });
        var inizio = Evento(parla, "alTermine");
        Su(inizio, ratti.gameObject.SetActive, true);
        Meta(inizio, centro);
        Su(inizio, missioni.Suggerisci, "tip.q3.resistenza");
        Su(inizio, missioni.Suggerisci, "tip.q3.schiva");

        // Fine della quest 2 (per tutte le classi) -> "Vai al granaio di Hobb".
        foreach (var d in new[] { fineBrannoc, fineEdric })
        {
            var e = Evento(d.GetComponent<DialogoAutomatico>(), "alTermine");
            for (int i = e.GetPersistentEventCount() - 1; i >= 0; i--)   // rifacendo la quest 3: via i collegamenti vecchi
                if (e.GetPersistentTarget(i) == null || (e.GetPersistentMethodName(i) == "ImpostaObiettivo" && e.GetPersistentTarget(i) == missioni))
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(e, i);
            Su(e, missioni.ImpostaObiettivo, "ob.q3.granaio");
            Meta(e, hobb);
            EditorUtility.SetDirty(d.GetComponent<DialogoAutomatico>());
        }

        EditorSceneManager.MarkSceneDirty(scena);
        Selection.activeGameObject = gruppo.gameObject;
        Debug.Log("Prologo: quest 3 preparata nel gruppo \"" + NomeGruppo + "\" (granaio di Hobb sul lago, 3 ratti). Salva la scena (Ctrl+S), poi Play.");
    }

    // Un ratto gigante: corpo, testa, orecchie, occhi rossi e coda; Bersaglio (37 di vita, morso da 11) e RattoGigante.
    static void Ratto(string nome, Transform genitore, Vector3 piedi, float ritardo, Material pelliccia, Material occhi)
    {
        var r = new GameObject(nome);
        r.transform.SetParent(genitore, false);
        r.transform.localPosition = piedi + Vector3.up * 0.3f;
        r.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        var box = r.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.05f, 0f);
        box.size = new Vector3(0.7f, 0.6f, 1.4f);
        Forma(PrimitiveType.Capsule, "Corpo", r.transform, new Vector3(0f, 0.05f, 0f), new Vector3(0.6f, 0.65f, 0.6f), pelliccia).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Forma(PrimitiveType.Sphere, "Testa", r.transform, new Vector3(0f, 0.12f, 0.7f), new Vector3(0.42f, 0.38f, 0.5f), pelliccia);
        Forma(PrimitiveType.Sphere, "Muso", r.transform, new Vector3(0f, 0.08f, 0.95f), new Vector3(0.18f, 0.16f, 0.2f), Pelle);
        Forma(PrimitiveType.Sphere, "Orecchio sinistro", r.transform, new Vector3(-0.15f, 0.33f, 0.62f), new Vector3(0.14f, 0.16f, 0.06f), Pelle);
        Forma(PrimitiveType.Sphere, "Orecchio destro", r.transform, new Vector3(0.15f, 0.33f, 0.62f), new Vector3(0.14f, 0.16f, 0.06f), Pelle);
        Forma(PrimitiveType.Sphere, "Occhio sinistro", r.transform, new Vector3(-0.1f, 0.2f, 0.88f), Vector3.one * 0.06f, occhi);
        Forma(PrimitiveType.Sphere, "Occhio destro", r.transform, new Vector3(0.1f, 0.2f, 0.88f), Vector3.one * 0.06f, occhi);
        Forma(PrimitiveType.Cylinder, "Coda", r.transform, new Vector3(0f, -0.02f, -1.05f), new Vector3(0.06f, 0.5f, 0.06f), Pelle).transform.localRotation = Quaternion.Euler(80f, 0f, 0f);

        var b = r.AddComponent<Bersaglio>();
        b.attaccaIlGiocatore = false;
        Imposta(b, "vitaMassima", 37f);
        Imposta(b, "dannoAttacco", 11f);
        Imposta(b, "rinasce", false);
        Imposta(b, "spintaQuandoColpito", 0.5f);
        var ratto = r.AddComponent<RattoGigante>();
        Imposta(ratto, "intervalloAttacchi", 4f);
        Imposta(ratto, "ritardoPrimoAttacco", ritardo);
        Imposta(r.AddComponent<NomeTradotto>(), "chiave", "nem.ratto");
    }
}
