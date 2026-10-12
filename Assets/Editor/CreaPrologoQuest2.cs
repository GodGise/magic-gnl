using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: prepara nella scena VillaggioLagoNero (aperta, con la quest 1 già preparata) la quest 2
// "Prova la lama" per Guerriero e Ladro e "Lo studio delle magie" per lo Stregone (Docs/quest-capitolo-1.md,
// Docs/dialoghi-capitolo-1.md), con forme provvisorie, in un gruppo nuovo "Prologo - quest 2":
//   - il cortile della fucina davanti alla porta del Fabbro, verso la piazzetta: forgia accesa, incudine,
//     rastrelliera, botte dell'acqua, Brannoc, un manichino e (solo per il Ladro) tre sagome per il tiro con l'arco;
//   - il cortile di studio accanto al tempio (solo per lo Stregone): pavimento di pietra, muretti, leggio, bracieri,
//     il vecchio Edric e tre bersagli a distanze diverse;
//   - tutto collegato con gli eventi:
//       arrivo alla fucina -> obiettivo "Parla con Brannoc" (la bussola lo indica)
//       Guerriero e Ladro: Brannoc dà le armi (Spada e Scudo della vecchia vita / Pugnale da scuoiare e Arco corto
//         da caccia) -> allenamento: attaccare, parare, schivare, agganciare (il Ladro anche tirare con l'arco),
//         con i suggerimenti -> Brannoc: "Non male..." -> quest 2 finita (il sole avanza);
//       Stregone: Brannoc lo manda al tempio -> Edric gli dà bastone, libro, Scintilla e Scheggia di ghiaccio ->
//         allenamento: lanciare, cambiare incantesimo, schivare, agganciare -> Edric: "Bene. Ricordi tutto..." ->
//         quest 2 finita.
// Gli oggetti donati vengono creati se mancano (menu "Crea oggetti del Guerriero/Ladro/Stregone").
// Non salva la scena: dopo il comando si guarda il risultato e si salva con Ctrl+S.
// Se il gruppo esiste già, chiede se rifarlo da capo (le modifiche fatte a mano a quel gruppo si perdono).
// Come si usa: aprire Assets/Scenes/VillaggioLagoNero.unity, poi menu "magic-gnl > Prologo: prepara la quest 2".
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class CreaPrologoQuest2
{
    const string NomeGruppo = "Prologo - quest 2";

    [MenuItem("magic-gnl/Prologo: prepara la quest 2")]
    static void Prepara()
    {
        Scene scena = SceneManager.GetActiveScene();
        var missioniOggetto = Trova(scena, "Missioni");
        var arrivo = Trova(scena, "Arrivo dal fabbro");
        if (scena.name != "VillaggioLagoNero" || missioniOggetto == null || arrivo == null)
        {
            EditorUtility.DisplayDialog("Prologo", "Apri la scena VillaggioLagoNero e prepara prima la quest 1 (\"Prologo: prepara la quest 1\").", "Ok");
            return;
        }
        if (!Porta(scena, "Fabbro", out Vector3 portaFabbro, out Vector3 fuoriFabbro) || Trova(scena, "Tempio") == null)
        {
            EditorUtility.DisplayDialog("Prologo", "Nella scena non trovo il Fabbro o il Tempio.", "Ok");
            return;
        }
        GameObject vecchio = Trova(scena, NomeGruppo);
        if (vecchio != null)
        {
            if (!EditorUtility.DisplayDialog("Prologo", "Il gruppo \"" + NomeGruppo + "\" esiste già. Rifarlo da capo? Le modifiche fatte a mano a quel gruppo andranno perse.", "Rifai", "Annulla")) return;
            Undo.DestroyObjectImmediate(vecchio);
        }

        // Gli oggetti da donare (creati se mancano).
        CreaOggettiGuerriero.Crea();
        CreaOggettiLadro.Crea();
        CreaOggettiStregone.Crea();
        var missioni = missioniOggetto.GetComponent<Missioni>();

        var gruppo = new GameObject(NomeGruppo).transform;
        Undo.RegisterCreatedObjectUndo(gruppo.gameObject, "Prologo quest 2");

        // ==================== il cortile della fucina ====================
        var cortile = Vuoto("Cortile della fucina", gruppo).transform;
        cortile.SetPositionAndRotation(portaFabbro + fuoriFabbro * 1f, Quaternion.LookRotation(fuoriFabbro));
        Decoro("Terra battuta", cortile, new Vector3(0f, 0.015f, 6f), new Vector3(9f, 0.03f, 13f), Terra);
        Blocco("Forgia", cortile, new Vector3(-3.4f, 0.6f, 0.9f), new Vector3(1.6f, 1.2f, 1.4f), PietraScura);
        Decoro("Braci della forgia", cortile, new Vector3(-3.4f, 1.22f, 0.9f), new Vector3(1.2f, 0.06f, 1f), Fuoco);
        Blocco("Cappa della forgia", cortile, new Vector3(-3.4f, 2.4f, 0.6f), new Vector3(1.2f, 1.4f, 0.9f), PietraScura);
        Luce("Luce della forgia", cortile, new Vector3(-3.4f, 1.8f, 1.6f), new Color(1f, 0.5f, 0.2f), 8f, 1.8f);
        Blocco("Base dell'incudine", cortile, new Vector3(-1.9f, 0.3f, 1.8f), new Vector3(0.5f, 0.6f, 0.5f), LegnoScuro);
        Blocco("Incudine", cortile, new Vector3(-1.9f, 0.72f, 1.8f), new Vector3(0.9f, 0.25f, 0.4f), Ferro);
        Forma(PrimitiveType.Cylinder, "Botte dell'acqua", cortile, new Vector3(-3.9f, 0.45f, 2.6f), new Vector3(0.8f, 0.45f, 0.8f), Legno, true);
        Blocco("Rastrelliera", cortile, new Vector3(3.6f, 0.9f, 0.5f), new Vector3(1.8f, 1.8f, 0.3f), Legno);
        for (int i = 0; i < 4; i++)
            Decoro("Lama sulla rastrelliera", cortile, new Vector3(2.9f + i * 0.45f, 1.1f, 0.75f), new Vector3(0.08f, 1.1f, 0.03f), Ferro);
        Blocco("Ceppo da lavoro", cortile, new Vector3(1.4f, 0.4f, 2.2f), new Vector3(0.9f, 0.8f, 0.9f), LegnoScuro);

        var brannoc = Personaggio("Brannoc", cortile, new Vector3(1.9f, 0f, 1.1f), 1.85f, new Color(0.3f, 0.22f, 0.16f), true);
        brannoc.transform.localRotation = Quaternion.LookRotation(new Vector3(-0.3f, 0f, 1f));
        var manichino = Manichino("Manichino di Brannoc", cortile, new Vector3(0f, 0f, 5.5f), 9999f, false);
        manichino.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        var tiro = Vuoto("Bersagli di tiro (solo Ladro)", cortile).transform;
        var soloLadro = Undo.AddComponent<SoloPerClasse>(tiro.gameObject);
        Imposta(soloLadro, "guerriero", false);
        Imposta(soloLadro, "stregone", false);
        Decoro("Linea di tiro", tiro, new Vector3(0f, 0.04f, 3.5f), new Vector3(5f, 0.05f, 0.2f), Legno);
        Vector3[] sagome = { new Vector3(-2.6f, 0f, 9.5f), new Vector3(0.4f, 0f, 12f), new Vector3(2.8f, 0f, 10f) };
        GameObject primaSagoma = null;
        for (int i = 0; i < sagome.Length; i++)
        {
            var sagoma = Manichino("Sagoma " + (i + 1), tiro, sagome[i], 9999f, true);
            sagoma.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            if (primaSagoma == null) primaSagoma = sagoma;
        }

        // Brannoc: dialogo, armi, allenamento.
        var dona = Undo.AddComponent<DonaOggetti>(brannoc);
        Imposta(dona, "perGuerriero", new[] { Oggetto("spada-della-vecchia-vita"), Oggetto("scudo-della-vecchia-vita") });
        Imposta(dona, "perLadro", new[] { Oggetto("pugnale-da-scuoiare"), Oggetto("arco-corto-da-caccia") });
        Imposta(dona, "perStregone", new DatiOggetto[0]);

        var allenaFucina = Undo.AddComponent<AllenamentoMosse>(Vuoto("Allenamento alla fucina", cortile));
        var dopoAllenamento = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: Brannoc dopo l'allenamento", brannoc.transform));
        Imposta(dopoAllenamento, "righe", new[]
        {
            Riga("pg.brannoc", "dlg.q2.04", ClasseRiga.Guerriero), Riga("pg.brannoc", "dlg.q2.04", ClasseRiga.Ladro),
            Riga("pg.brannoc", "dlg.q2.05", ClasseRiga.Ladro),
            Riga("pg.brannoc", "dlg.q2.06", ClasseRiga.Guerriero), Riga("pg.brannoc", "dlg.q2.06", ClasseRiga.Ladro),
        });
        Su(Evento(allenaFucina, "alCompletamento"), dopoAllenamento.Avvia);
        var fineFucina = Evento(dopoAllenamento, "alTermine");
        Su(fineFucina, missioni.QuestFinita, 2);
        Su(fineFucina, missioni.TogliObiettivo);

        var parlaBrannoc = Undo.AddComponent<Interlocutore>(brannoc);
        Imposta(parlaBrannoc, "nomeChiave", "pg.brannoc");
        Imposta(parlaBrannoc, "raggio", 3f);
        Imposta(parlaBrannoc, "righe", new[]
        {
            Riga("pg.brannoc", "dlg.q2.01g", ClasseRiga.Guerriero), Riga("pg.brannoc", "dlg.q2.01l", ClasseRiga.Ladro),
            Riga("pg.brannoc", "dlg.q2.01s", ClasseRiga.Stregone), Riga("pg.brannoc", "dlg.q2.02"),
            Riga("pg.brannoc", "dlg.q2.03", ClasseRiga.Guerriero), Riga("pg.brannoc", "dlg.q2.03", ClasseRiga.Ladro),
        });
        Imposta(parlaBrannoc, "righeRipetute", new[]
        {
            Riga("pg.brannoc", "dlg.q2.03", ClasseRiga.Guerriero), Riga("pg.brannoc", "dlg.q2.03", ClasseRiga.Ladro),
            Riga("pg.brannoc", "dlg.q2.01s", ClasseRiga.Stregone),
        });

        // ==================== il cortile di studio del tempio (solo Stregone) ====================
        Transform tempio = Trova(scena, "Tempio").transform;
        float profonditaTempio = tempio.lossyScale.z;
        var studio = Vuoto("Cortile di studio del tempio (solo Stregone)", gruppo).transform;
        studio.SetPositionAndRotation(tempio.position + tempio.right * -4f + tempio.forward * -(profonditaTempio / 2f + 5.5f), tempio.rotation);
        studio.position = new Vector3(studio.position.x, 0f, studio.position.z);
        var soloStregone = Undo.AddComponent<SoloPerClasse>(studio.gameObject);
        Imposta(soloStregone, "guerriero", false);
        Imposta(soloStregone, "ladro", false);
        Decoro("Lastricato", studio, new Vector3(-3f, 0.02f, 0f), new Vector3(16f, 0.04f, 9f), PietraScura);
        Blocco("Muretto nord", studio, new Vector3(-3f, 0.4f, 4.3f), new Vector3(16f, 0.8f, 0.4f), Pietra);
        Blocco("Muretto ovest", studio, new Vector3(-10.8f, 0.4f, 0f), new Vector3(0.4f, 0.8f, 9f), Pietra);
        Blocco("Muretto sud (sinistra)", studio, new Vector3(-7f, 0.4f, -4.3f), new Vector3(8f, 0.8f, 0.4f), Pietra);
        Blocco("Leggio", studio, new Vector3(3.6f, 0.55f, 2.6f), new Vector3(0.6f, 1.1f, 0.5f), Legno);
        Decoro("Libro aperto", studio, new Vector3(3.6f, 1.12f, 2.6f), new Vector3(0.55f, 0.05f, 0.4f), Stoffa, new Vector3(-20f, 0f, 0f));
        Blocco("Scaffale", studio, new Vector3(1.4f, 1f, 3.8f), new Vector3(2.2f, 2f, 0.5f), LegnoScuro);
        foreach (var punto in new[] { new Vector3(4.2f, 0f, -3.4f), new Vector3(-2f, 0f, 3.6f), new Vector3(-10f, 0f, -3.4f) })
        {
            Forma(PrimitiveType.Cylinder, "Braciere", studio, punto + Vector3.up * 0.45f, new Vector3(0.6f, 0.45f, 0.6f), Ferro, true);
            Decoro("Fiamma del braciere", studio, punto + Vector3.up * 0.95f, new Vector3(0.35f, 0.15f, 0.35f), Fuoco);
            Luce("Luce del braciere", studio, punto + Vector3.up * 1.3f, new Color(1f, 0.6f, 0.3f), 6f, 1.2f);
        }
        var edric = Personaggio("Edric", studio, new Vector3(2.8f, 0f, 1.6f), 1.7f, new Color(0.35f, 0.33f, 0.42f), true);
        edric.transform.localRotation = Quaternion.LookRotation(new Vector3(-0.5f, 0f, -1f));
        Vector3[] bersagliStudio = { new Vector3(-3.5f, 0f, -0.5f), new Vector3(-6.5f, 0f, 1.8f), new Vector3(-9.5f, 0f, -1.2f) };
        GameObject primoBersaglio = null;
        for (int i = 0; i < bersagliStudio.Length; i++)
        {
            var b = Manichino("Bersaglio di studio " + (i + 1), studio, bersagliStudio[i], 9999f, false);
            b.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            if (primoBersaglio == null) primoBersaglio = b;
        }

        var donaEdric = Undo.AddComponent<DonaOggetti>(edric);
        Imposta(donaEdric, "perGuerriero", new DatiOggetto[0]);
        Imposta(donaEdric, "perLadro", new DatiOggetto[0]);
        Imposta(donaEdric, "perStregone", new[] { Oggetto("bastone-della-vecchia-vita"), Oggetto("libro-della-vecchia-vita"), Oggetto("scintilla"), Oggetto("scheggia-di-ghiaccio") });
        var allenaTempio = Undo.AddComponent<AllenamentoMosse>(Vuoto("Allenamento nel tempio", studio));
        var dopoStudio = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: Edric dopo lo studio", edric.transform));
        Imposta(dopoStudio, "righe", new[] { Riga("pg.edric", "dlg.q2s.06"), Riga("pg.edric", "dlg.q2s.07") });
        Su(Evento(allenaTempio, "alCompletamento"), dopoStudio.Avvia);
        var fineStudio = Evento(dopoStudio, "alTermine");
        Su(fineStudio, missioni.QuestFinita, 2);
        Su(fineStudio, missioni.TogliObiettivo);

        var parlaEdric = Undo.AddComponent<Interlocutore>(edric);
        Imposta(parlaEdric, "nomeChiave", "pg.edric");
        Imposta(parlaEdric, "raggio", 3f);
        Imposta(parlaEdric, "righe", new[]
        {
            Riga("pg.edric", "dlg.q2s.01"), Riga("pg.edric", "dlg.q2s.02"), Riga("pg.edric", "dlg.q2s.03"),
            Riga("pg.edric", "dlg.q2s.04"), Riga("pg.edric", "dlg.q2s.05"),
        });
        Imposta(parlaEdric, "righeRipetute", new[] { Riga("pg.edric", "dlg.q2s.05") });
        var fineEdric = Evento(parlaEdric, "alTermine");
        Su(fineEdric, donaEdric.Dona);
        Su(fineEdric, missioni.ImpostaObiettivo, "ob.q2.studia");
        Su(fineEdric, allenaTempio.Avvia);
        foreach (var tip in new[] { "tip.q2s.lancia", "tip.q2s.cambia", "tip.q2s.mana", "tip.q2.schiva", "tip.q2.aggancia", "tip.q2s.distanza" })
            Su(fineEdric, missioni.Suggerisci, tip);
        Meta(fineEdric, primoBersaglio);

        // ==================== dopo Brannoc: strade diverse per classe ====================
        var ramo = Undo.AddComponent<RamoPerClasse>(Vuoto("Dopo Brannoc (per classe)", brannoc.transform));
        foreach (string campo in new[] { "perGuerriero", "perLadro" })
        {
            var e = Evento(ramo, campo);
            Su(e, dona.Dona);
            Su(e, missioni.ImpostaObiettivo, "ob.q2.allenati");
            Su(e, allenaFucina.Avvia);
            foreach (var tip in new[] { "tip.q2.attacca", "tip.q2.para", "tip.q2.schiva", "tip.q2.aggancia", "tip.q2.cambia_bersaglio" })
                Su(e, missioni.Suggerisci, tip);
            if (campo == "perLadro") Su(e, missioni.Suggerisci, "tip.q2.arco");
            Meta(e, manichino);
        }
        var perStregone = Evento(ramo, "perStregone");
        Su(perStregone, missioni.ImpostaObiettivo, "ob.q2.tempio");
        Meta(perStregone, edric);
        Su(Evento(parlaBrannoc, "alTermine"), ramo.Esegui);

        // ==================== arrivo alla fucina (dalla quest 1) ====================
        var zona = arrivo.GetComponent<ZonaArrivo>();
        var allArrivo = Evento(zona, "allArrivo");
        // Rifacendo la quest 2 si tolgono i collegamenti vecchi (quelli verso oggetti cancellati e l'obiettivo di Brannoc).
        for (int i = allArrivo.GetPersistentEventCount() - 1; i >= 0; i--)
            if (allArrivo.GetPersistentTarget(i) == null || allArrivo.GetPersistentMethodName(i) == "ImpostaObiettivo")
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(allArrivo, i);
        Su(allArrivo, missioni.ImpostaObiettivo, "ob.q2.brannoc");
        Meta(allArrivo, brannoc);

        EditorSceneManager.MarkSceneDirty(scena);
        Selection.activeGameObject = gruppo.gameObject;
        Debug.Log("Prologo: quest 2 preparata nel gruppo \"" + NomeGruppo + "\" (fucina e cortile di studio del tempio). Salva la scena (Ctrl+S), poi Play.");
    }
}
