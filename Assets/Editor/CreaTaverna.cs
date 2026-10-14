using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: crea la scena della taverna di Hobb per la quest 6 "Una sera in taverna" (Lorenzo, 13 ottobre:
// la taverna è una zona a parte come la cripta, così all'uscita la razzia è già cominciata e non si vedono gli orchi
// comparire dal nulla). Si entra dalla porta della taverna del villaggio (CaricamentoZona).
// La sala: 16 x 12 metri, bancone a nord con Hobb dietro, camino a ovest, quattro tavoli con qualche cliente, botti,
// lampade calde. La porta è a sud.
// Cosa succede (Docs/dialoghi-capitolo-1.md, quest 6):
//   arrivo -> "Siediti al bancone con Hobb", la bussola indica Hobb;
//   Hobb (dlg.q6.01-03) -> SeraInTaverna: entra Maren con Tobin in braccio (dlg.q6.04-05), i tamburi lontani, Hobb
//   (dlg.q6.06), Maren (dlg.q6.07) -> "Esci dalla taverna", la porta si apre, segnale q6.fine.
//   Uscendo (segnale uscita.Taverna) nel villaggio è già notte e parte la razzia (vedi CreaPrologoQuest6).
// La taverna sta lontano dal villaggio e dalla cripta (X 4000).
// Salva la scena in Assets/Scenes/Taverna.unity e la aggiunge alle Build Settings. Se esiste già chiede se rifarla.
// Come si usa: menu "magic-gnl > Taverna: crea la scena della taverna" (lo fa da solo anche "Prologo: prepara la
// quest 6 e la razzia" se la scena manca). Non va montato su niente.
public static class CreaTaverna
{
    public const string NomeScena = "Taverna";
    const string Percorso = "Assets/Scenes/Taverna.unity";
    const float Largo = 16f, Profondo = 12f, Alto = 4f;
    static readonly Vector3 Origine = new Vector3(4000f, 0f, 0f);

    static Material Assi => Materiale("taverna-assi", new Color(0.3f, 0.2f, 0.12f));
    static Material Muri => Materiale("taverna-muri", new Color(0.36f, 0.3f, 0.24f));
    static Material Travi => Materiale("taverna-travi", new Color(0.18f, 0.12f, 0.08f));
    static Material Botte => Materiale("taverna-botte", new Color(0.33f, 0.22f, 0.12f));

    [MenuItem("magic-gnl/Taverna: crea la scena della taverna")]
    static void Menu()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Percorso) != null &&
            !EditorUtility.DisplayDialog("Taverna", "La scena della taverna esiste già. Rifarla da capo? Le modifiche fatte a mano andranno perse.", "Rifai", "Annulla"))
            return;
        var aperta = SceneManager.GetSceneByPath(Percorso);
        if (aperta.IsValid() && aperta.isLoaded) EditorSceneManager.CloseScene(aperta, true);
        Crea();
        EditorUtility.DisplayDialog("Taverna", "Scena della taverna creata in " + Percorso + " e aggiunta alle Build Settings.\n"
            + "Ora, nella scena VillaggioLagoNero, usa \"magic-gnl > Prologo: prepara la quest 6 e la razzia\".", "Ok");
    }

    internal static void Crea()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        var prima = SceneManager.GetActiveScene();
        var scena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scena);

        var radice = new GameObject("Taverna").transform;
        radice.position = Origine;

        var ambiente = Vuoto("Ambiente della taverna", radice).AddComponent<AmbienteZona>();
        ambiente.luceAmbiente = new Color(0.34f, 0.27f, 0.2f);   // calda, si vede bene anche lontano dalle lampade
        ambiente.nebbia = new Color(0.06f, 0.04f, 0.025f);
        ambiente.densitaNebbia = 0.015f;
        var ponte = Vuoto("Ponte verso le missioni", radice).AddComponent<PonteMissioni>();

        Stanza(radice);
        var arredi = Vuoto("Arredi", radice).transform;
        Bancone(arredi);
        Camino(arredi);
        Tavoli(arredi);
        Lampade(arredi);

        // ---------- persone ----------
        var gente = Vuoto("Persone", radice).transform;
        var hobb = Personaggio("Hobb", gente, new Vector3(Largo / 2f, 0f, Profondo - 1.4f), 1.75f, new Color(0.42f, 0.32f, 0.2f), true);
        hobb.transform.localRotation = Quaternion.LookRotation(Vector3.back);
        Personaggio("Cliente al camino", gente, new Vector3(2.6f, 0f, 5.2f), 1.7f, new Color(0.28f, 0.3f, 0.34f), true).transform.localRotation = Quaternion.LookRotation(Vector3.left);
        Personaggio("Cliente al tavolo", gente, new Vector3(11.4f, 0f, 3.9f), 1.72f, new Color(0.4f, 0.26f, 0.22f), true).transform.localRotation = Quaternion.LookRotation(Vector3.right);
        Personaggio("Vecchio pescatore", gente, new Vector3(12.6f, 0f, 7.4f), 1.65f, new Color(0.25f, 0.3f, 0.26f), true).transform.localRotation = Quaternion.LookRotation(Vector3.left);

        // Maren, spenta finché non entra; Tobin addormentato in braccio
        var maren = Personaggio("Maren", gente, new Vector3(Largo / 2f, 0f, 0.8f), 1.75f, new Color(0.45f, 0.18f, 0.16f), true);
        var tobin = Forma(PrimitiveType.Capsule, "Tobin in braccio", maren.transform, new Vector3(0f, 0.12f, 0.3f), new Vector3(0.28f, 0.42f, 0.28f),
            Materiale("prologo-tobin", new Color(0.25f, 0.35f, 0.22f)));
        tobin.transform.localRotation = Quaternion.Euler(0f, 0f, 80f);
        Forma(PrimitiveType.Sphere, "Testa di Tobin", maren.transform, new Vector3(-0.32f, 0.2f, 0.3f), Vector3.one * 0.2f, Pelle);
        maren.SetActive(false);
        var ingressoMaren = Vuoto("Ingresso di Maren", gente, new Vector3(Largo / 2f, 0f, 0.6f) + Vector3.up * 0.875f).transform;
        ingressoMaren.localRotation = Quaternion.LookRotation(Vector3.forward);
        var postoMaren = Vuoto("Posto di Maren", gente, new Vector3(Largo / 2f + 1.6f, 0f, Profondo - 4.2f) + Vector3.up * 0.875f).transform;
        postoMaren.localRotation = Quaternion.LookRotation(Vector3.left);

        // ---------- arrivo e uscita ----------
        var arrivo = Vuoto("Arrivo", radice, new Vector3(Largo / 2f, 0f, 1.6f)).transform;
        arrivo.localRotation = Quaternion.LookRotation(Vector3.forward);
        var porta = Vuoto("Porta della taverna", radice, new Vector3(Largo / 2f, 0f, 0.7f));
        var uscita = porta.AddComponent<PassaggioScena>();
        Imposta(uscita, "verso", PassaggioScena.Verso.Esci);
        Imposta(uscita, "scena", NomeScena);
        Imposta(uscita, "chiaveAzione", "hud.esci_taverna");
        Imposta(uscita, "raggio", 2.6f);
        uscita.enabled = false;
        // una lanterna sopra la porta, così l'uscita si trova subito
        Luce("Lanterna della porta", porta.transform, new Vector3(0f, 2.8f, 0.6f), new Color(1f, 0.75f, 0.45f), 6f, 1.4f);

        var zona = Vuoto("Zona camera (taverna)", radice, new Vector3(Largo / 2f, 0f, Profondo / 2f)).AddComponent<ZonaCamera>();
        Imposta(zona, "dimensioni", new Vector3(Largo, Alto, Profondo));
        Imposta(zona, "distanzaMassima", 4.5f);
        Imposta(zona, "inclinazioneMassima", 38f);

        // ---------- la quest ----------
        var sera = Vuoto("Sera in taverna", radice).AddComponent<SeraInTaverna>();
        Imposta(sera, "maren", maren);
        Imposta(sera, "ingressoMaren", ingressoMaren);
        Imposta(sera, "postoMaren", postoMaren);
        Imposta(sera, "righeMaren", new[] { Riga("pg.maren", "dlg.q6.04"), Riga("pg.maren", "dlg.q6.05") });
        Imposta(sera, "righeDopoITamburi", new[] { Riga("pg.hobb", "dlg.q6.06"), Riga("pg.maren", "dlg.q6.07") });
        Imposta(sera, "uscita", uscita);
        var fine = Evento(sera, "alTermine");
        Su(fine, ponte.ImpostaObiettivo, "ob.q6.esci");
        UnityEventTools.AddObjectPersistentListener<Transform>(fine, ponte.ImpostaMeta, porta.transform);
        Su(fine, ponte.Segnale, "q6.fine");

        var parla = Vuoto("Hobb (la sera)", hobb.transform).AddComponent<Interlocutore>();
        Imposta(parla, "nomeChiave", "pg.hobb");
        Imposta(parla, "raggio", 3.2f);
        Imposta(parla, "unaVoltaSola", true);
        Imposta(parla, "righe", new[] { Riga("pg.hobb", "dlg.q6.01"), Riga("pg.hobb", "dlg.q6.02"), Riga("pg.hobb", "dlg.q6.03") });
        var dopoHobb = Evento(parla, "alTermine");
        Su(dopoHobb, sera.Avvia);
        Su(dopoHobb, ponte.TogliObiettivo);

        var allArrivo = Vuoto("All'arrivo", radice, arrivo.localPosition).AddComponent<ZonaArrivo>();
        Imposta(allArrivo, "raggio", 3f);
        var arrivato = Evento(allArrivo, "allArrivo");
        Su(arrivato, ponte.ImpostaObiettivo, "ob.q6.hobb");
        UnityEventTools.AddObjectPersistentListener<Transform>(arrivato, ponte.ImpostaMeta, hobb.transform);

        EditorSceneManager.SaveScene(scena, Percorso);
        AggiungiAlleBuildSettings();
        if (prima.IsValid()) SceneManager.SetActiveScene(prima);
        EditorSceneManager.CloseScene(scena, true);
        AssetDatabase.SaveAssets();
        Debug.Log("Taverna: scena creata in " + Percorso + ".");
    }

    static void AggiungiAlleBuildSettings()
    {
        var elenco = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in elenco) if (s.path == Percorso) return;
        elenco.Add(new EditorBuildSettingsScene(Percorso, true));
        EditorBuildSettings.scenes = elenco.ToArray();
    }

    // ---------- la sala ----------

    static void Stanza(Transform radice)
    {
        var s = Vuoto("Sala", radice).transform;
        Blocco("Pavimento", s, new Vector3(Largo / 2f, -0.1f, Profondo / 2f), new Vector3(Largo, 0.2f, Profondo), Assi);
        Blocco("Soffitto", s, new Vector3(Largo / 2f, Alto + 0.15f, Profondo / 2f), new Vector3(Largo, 0.3f, Profondo), Travi);
        Blocco("Muro nord", s, new Vector3(Largo / 2f, Alto / 2f, Profondo + 0.2f), new Vector3(Largo + 0.8f, Alto, 0.4f), Muri);
        Blocco("Muro ovest", s, new Vector3(-0.2f, Alto / 2f, Profondo / 2f), new Vector3(0.4f, Alto, Profondo), Muri);
        Blocco("Muro est", s, new Vector3(Largo + 0.2f, Alto / 2f, Profondo / 2f), new Vector3(0.4f, Alto, Profondo), Muri);
        // muro sud con la porta in mezzo (1,6 m)
        float lato = (Largo - 1.6f) / 2f;
        Blocco("Muro sud", s, new Vector3(lato / 2f, Alto / 2f, -0.2f), new Vector3(lato, Alto, 0.4f), Muri);
        Blocco("Muro sud", s, new Vector3(Largo - lato / 2f, Alto / 2f, -0.2f), new Vector3(lato, Alto, 0.4f), Muri);
        Blocco("Sopra la porta", s, new Vector3(Largo / 2f, Alto - 0.8f, -0.2f), new Vector3(1.6f, 1.6f, 0.4f), Muri);
        Blocco("Porta chiusa", s, new Vector3(Largo / 2f, 1.2f, -0.35f), new Vector3(1.6f, 2.4f, 0.1f), Travi);
        // travi del soffitto
        for (int i = 1; i < 5; i++)
            Decoro("Trave", s, new Vector3(i * Largo / 5f, Alto - 0.2f, Profondo / 2f), new Vector3(0.3f, 0.3f, Profondo), Travi);
    }

    static void Bancone(Transform g)
    {
        var b = Vuoto("Bancone", g, new Vector3(Largo / 2f, 0f, Profondo - 2.6f)).transform;
        Blocco("Banco", b, new Vector3(0f, 0.55f, 0f), new Vector3(6f, 1.1f, 0.8f), Travi);
        Decoro("Piano del banco", b, new Vector3(0f, 1.12f, 0f), new Vector3(6.3f, 0.06f, 1f), Assi);
        for (int i = -2; i <= 2; i++)
            Forma(PrimitiveType.Cylinder, "Sgabello", b, new Vector3(i * 1.2f, 0.35f, -0.9f), new Vector3(0.45f, 0.35f, 0.45f), Botte, true);
        // boccali sul banco e botti dietro
        for (int i = 0; i < 4; i++)
            Forma(PrimitiveType.Cylinder, "Boccale", b, new Vector3(-2.2f + i * 1.3f, 1.25f, 0.1f), new Vector3(0.14f, 0.1f, 0.14f), Materiale("taverna-peltro", new Color(0.5f, 0.5f, 0.52f)));
        for (int i = 0; i < 3; i++)
            Forma(PrimitiveType.Cylinder, "Botte", b, new Vector3(-2f + i * 2f, 0.6f, 2.2f), new Vector3(1f, 0.6f, 1f), Botte, true);
        Decoro("Mensola", b, new Vector3(0f, 2.1f, 2.45f), new Vector3(5f, 0.08f, 0.35f), Assi);
    }

    static void Camino(Transform g)
    {
        var c = Vuoto("Camino", g, new Vector3(0.6f, 0f, 5.5f)).transform;
        Blocco("Pietre", c, new Vector3(0f, 1.2f, 0f), new Vector3(1.2f, 2.4f, 2.6f), PietraScura);
        Decoro("Bocca", c, new Vector3(0.5f, 0.6f, 0f), new Vector3(0.3f, 1f, 1.5f), Materiale("taverna-nero", new Color(0.03f, 0.02f, 0.02f)));
        Forma(PrimitiveType.Sphere, "Fuoco", c, new Vector3(0.6f, 0.35f, 0f), new Vector3(0.7f, 0.5f, 0.9f), Fuoco);
        var l = Vuoto("Luce del camino", c, new Vector3(1.4f, 1f, 0f)).AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.55f, 0.25f);
        l.range = 9f;
        l.intensity = 1.8f;
        var t = l.gameObject.AddComponent<global::Torcia>();
        Imposta(t, "intensitaMassima", 1.8f);
    }

    static void Tavoli(Transform g)
    {
        Vector3[] posti = { new Vector3(4f, 0f, 3.6f), new Vector3(10.6f, 0f, 3.6f), new Vector3(4f, 0f, 7.4f), new Vector3(11.8f, 0f, 7.4f) };
        foreach (var p in posti)
        {
            var t = Vuoto("Tavolo", g, p).transform;
            Blocco("Piano", t, new Vector3(0f, 0.75f, 0f), new Vector3(2.2f, 0.1f, 1.1f), Assi);
            Decoro("Gamba", t, new Vector3(-0.9f, 0.35f, 0f), new Vector3(0.12f, 0.7f, 0.9f), Travi);
            Decoro("Gamba", t, new Vector3(0.9f, 0.35f, 0f), new Vector3(0.12f, 0.7f, 0.9f), Travi);
            Blocco("Panca", t, new Vector3(0f, 0.25f, 0.95f), new Vector3(2f, 0.5f, 0.35f), Travi);
            Blocco("Panca", t, new Vector3(0f, 0.25f, -0.95f), new Vector3(2f, 0.5f, 0.35f), Travi);
            Forma(PrimitiveType.Cylinder, "Candela", t, new Vector3(0.3f, 0.88f, 0f), new Vector3(0.08f, 0.08f, 0.08f), Materiale("taverna-cera", new Color(0.85f, 0.8f, 0.65f)));
        }
    }

    static void Lampade(Transform g)
    {
        foreach (float x in new[] { 4f, 8f, 12f })
        {
            var l = Vuoto("Lampada", g, new Vector3(x, Alto - 0.9f, Profondo / 2f));
            Decoro("Catena", l.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.03f, 0.7f, 0.03f), Ferro);
            Forma(PrimitiveType.Sphere, "Lume", l.transform, Vector3.zero, Vector3.one * 0.3f, Fuoco);
            var luce = l.AddComponent<Light>();
            luce.type = LightType.Point;
            luce.color = new Color(1f, 0.7f, 0.4f);
            luce.range = 8f;
            luce.intensity = 1.3f;
        }
    }
}
