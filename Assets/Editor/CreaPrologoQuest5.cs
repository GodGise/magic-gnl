using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: prepara nella scena VillaggioLagoNero (con le quest 1-4 già preparate) la parte nel villaggio
// della quest 5 "La leggenda del lago" (Docs/dialoghi-capitolo-1.md, Docs/quest-capitolo-1.md), in un gruppo nuovo
// "Prologo - quest 5". La cripta vera è una scena a parte: "magic-gnl > Cripta: crea la scena della cripta" (se manca,
// questo comando la crea).
//   - nel cimitero un piccolo mausoleo di pietra con la scala che scende: "E  Entra nella cripta" (PassaggioScena),
//     usabile solo dopo che Edric ha raccontato la leggenda; entrando il villaggio si nasconde;
//   - collegamenti:
//       fine della quest 4 (Ilse racconta del lago) -> "Parla con Edric al tempio"
//       -> Edric racconta la leggenda (dlg.q5.01-03) -> "Scendi nella cripta sotto il cimitero", la bussola indica il
//          mausoleo, l'ingresso si apre
//       -> nella cripta (vedi CreaCripta): scheletri, cancello, leva, baule con il Medaglione della prima guardia
//       -> segnale q5.medaglione (dalla cripta) -> Edric ha le battute del ritorno
//       -> uscendo dalla cripta (segnale uscita.Cripta) -> "Porta il medaglione a Edric", la bussola indica Edric
//       -> Edric (dlg.q5.04-05) -> quest 5 finita (il sole avanza); da lì Edric ringrazia (dlg.dopo.edric5).
//   - mentre la quest è in corso, l'Interlocutore di Edric della quest 2 (i ringraziamenti) si spegne, così non ci sono
//     due "Parla: Edric" insieme.
// Gli alberi e le lapidi che toccherebbero il mausoleo si spengono (non si cancellano).
// Rifacendo la quest 4 il collegamento con la fine della quest 4 si rifà da solo (CreaPrologoQuest4 chiama Collega).
// Non salva la scena: dopo il comando si guarda il risultato e si salva con Ctrl+S.
// Come si usa: aprire Assets/Scenes/VillaggioLagoNero.unity, poi menu "magic-gnl > Prologo: prepara la quest 5".
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class CreaPrologoQuest5
{
    const string NomeGruppo = "Prologo - quest 5";

    [MenuItem("magic-gnl/Prologo: prepara la quest 5")]
    static void Prepara()
    {
        Scene scena = SceneManager.GetActiveScene();
        var missioniOggetto = Trova(scena, "Missioni");
        var fineIlse = Trova(scena, "Dialogo: Ilse dopo la pozione");
        var cimitero = Trova(scena, "Cimitero");
        var villaggio = Trova(scena, "Villaggio");
        var edric = TrovaInterlocutore(scena, "Edric");
        if (scena.name != "VillaggioLagoNero" || missioniOggetto == null || fineIlse == null || cimitero == null || edric == null)
        {
            EditorUtility.DisplayDialog("Prologo", "Apri la scena VillaggioLagoNero e prepara prima le quest da 1 a 4.", "Ok");
            return;
        }
        GameObject vecchio = Trova(scena, NomeGruppo);
        if (vecchio != null)
        {
            if (!EditorUtility.DisplayDialog("Prologo", "Il gruppo \"" + NomeGruppo + "\" esiste già. Rifarlo da capo? Le modifiche fatte a mano a quel gruppo andranno perse.", "Rifai", "Annulla")) return;
            Undo.DestroyObjectImmediate(vecchio);
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/" + CreaCripta.NomeScena + ".unity") == null) CreaCripta.Crea();

        var missioni = missioniOggetto.GetComponent<Missioni>();
        var gruppo = new GameObject(NomeGruppo).transform;
        Undo.RegisterCreatedObjectUndo(gruppo.gameObject, "Prologo quest 5");

        // ---------- il mausoleo nel cimitero ----------
        Bounds recinto = Limiti(cimitero);
        // contro il recinto ovest, a metà, con l'ingresso verso est (verso il cancello del cimitero e il tempio)
        var mausoleo = Vuoto("Mausoleo della cripta", gruppo).transform;
        mausoleo.position = new Vector3(recinto.min.x + 3.2f, 0f, recinto.center.z);
        mausoleo.rotation = Quaternion.LookRotation(Vector3.right);
        Material pietra = Pietra, scura = PietraScura;
        Blocco("Muro di fondo", mausoleo, new Vector3(0f, 1.6f, -1.8f), new Vector3(4f, 3.2f, 0.4f), pietra);
        Blocco("Muro sinistro", mausoleo, new Vector3(-1.8f, 1.6f, 0f), new Vector3(0.4f, 3.2f, 4f), pietra);
        Blocco("Muro destro", mausoleo, new Vector3(1.8f, 1.6f, 0f), new Vector3(0.4f, 3.2f, 4f), pietra);
        Blocco("Facciata sinistra", mausoleo, new Vector3(-1.2f, 1.6f, 1.8f), new Vector3(1.6f, 3.2f, 0.4f), pietra);
        Blocco("Facciata destra", mausoleo, new Vector3(1.2f, 1.6f, 1.8f), new Vector3(1.6f, 3.2f, 0.4f), pietra);
        Blocco("Architrave", mausoleo, new Vector3(0f, 2.85f, 1.8f), new Vector3(0.8f, 0.7f, 0.4f), pietra);
        Blocco("Tetto", mausoleo, new Vector3(0f, 3.35f, 0f), new Vector3(4.4f, 0.3f, 4.4f), scura);
        Decoro("Timpano", mausoleo, new Vector3(0f, 3.75f, 1.9f), new Vector3(3.2f, 0.5f, 0.3f), scura);
        // dentro: buio e la scala che scende
        Decoro("Buio", mausoleo, new Vector3(0f, 1.3f, 1.65f), new Vector3(0.8f, 2.5f, 0.05f), Materiale("cripta-buio", new Color(0.01f, 0.01f, 0.015f)));
        for (int i = 0; i < 3; i++)
            Decoro("Gradino", mausoleo, new Vector3(0f, 0.05f - i * 0.12f, 1f - i * 0.5f), new Vector3(1.6f, 0.1f, 0.5f), scura);
        Decoro("Occhio inciso", mausoleo, new Vector3(0f, 2.85f, 2.02f), new Vector3(0.35f, 0.2f, 0.02f), Materiale("cripta-bronzo", new Color(0.45f, 0.33f, 0.18f)));
        Luce("Lumino del mausoleo", mausoleo, new Vector3(0f, 1.2f, 2.6f), new Color(0.6f, 0.75f, 1f), 4f, 0.7f);
        SgomberaAlberi(scena, mausoleo, Vector3.zero, new Vector3(6f, 0f, 6f));
        SpegniLapidi(cimitero.transform, mausoleo, new Vector3(5.5f, 0f, 6f));

        var ingresso = Undo.AddComponent<PassaggioScena>(Vuoto("Ingresso della cripta", mausoleo, new Vector3(0f, 0f, 2.6f)));
        Imposta(ingresso, "verso", PassaggioScena.Verso.Entra);
        Imposta(ingresso, "scena", CreaCripta.NomeScena);
        Imposta(ingresso, "chiaveTitolo", "zona.cripta");
        Imposta(ingresso, "chiaveAzione", "hud.entra_cripta");
        Imposta(ingresso, "raggio", 2.2f);
        Imposta(ingresso, "nascondiDentro", villaggio != null ? new[] { villaggio } : new GameObject[0]);
        ingresso.enabled = false;

        // ---------- Edric ----------
        var leggenda = Undo.AddComponent<Interlocutore>(Vuoto("Edric (la leggenda)", edric.transform));
        Imposta(leggenda, "nomeChiave", "pg.edric");
        Imposta(leggenda, "raggio", 3f);
        Imposta(leggenda, "unaVoltaSola", false);
        Imposta(leggenda, "righe", new[] { Riga("pg.edric", "dlg.q5.01"), Riga("pg.edric", "dlg.q5.02"), Riga("pg.edric", "dlg.q5.03") });
        Imposta(leggenda, "righeRipetute", new[] { Riga("pg.edric", "dlg.q5.03") });
        var dopoLeggenda = Evento(leggenda, "alTermine");
        Su(dopoLeggenda, Abilita(ingresso), true);
        Su(dopoLeggenda, missioni.ImpostaObiettivo, "ob.q5.cripta");
        Meta(dopoLeggenda, ingresso.gameObject);
        leggenda.enabled = false;

        var ritorno = Undo.AddComponent<Interlocutore>(Vuoto("Edric (il medaglione)", edric.transform));
        Imposta(ritorno, "nomeChiave", "pg.edric");
        Imposta(ritorno, "raggio", 3f);
        Imposta(ritorno, "unaVoltaSola", true);
        Imposta(ritorno, "righe", new[] { Riga("pg.edric", "dlg.q5.04"), Riga("pg.edric", "dlg.q5.05") });
        Imposta(ritorno, "righeDopoLaQuest", new[] { Riga("pg.edric", "dlg.dopo.edric5") });
        var fine = Evento(ritorno, "alTermine");
        Su(fine, missioni.QuestFinita, 5);
        Su(fine, missioni.TogliObiettivo);
        Su(fine, ritorno.QuestCompiuta);   // da qui Edric ringrazia
        ritorno.enabled = false;

        // ---------- segnali dalla cripta ----------
        var uscito = Undo.AddComponent<AscoltaSegnale>(Vuoto("Attesa: uscita dalla cripta", gruppo));
        Imposta(uscito, "parola", "uscita." + CreaCripta.NomeScena);
        var allUscita = Evento(uscito, "quandoArriva");
        Su(allUscita, missioni.ImpostaObiettivo, "ob.q5.torna");
        Meta(allUscita, edric.gameObject);
        uscito.gameObject.SetActive(false);

        var medaglione = Undo.AddComponent<AscoltaSegnale>(Vuoto("Attesa: medaglione preso", gruppo));
        Imposta(medaglione, "parola", "q5.medaglione");
        var preso = Evento(medaglione, "quandoArriva");
        Su(preso, Abilita(leggenda), false);
        Su(preso, Abilita(ritorno), true);
        Su(preso, uscito.gameObject.SetActive, true);

        Collega(scena);
        EditorSceneManager.MarkSceneDirty(scena);
        Selection.activeGameObject = gruppo.gameObject;
        Debug.Log("Prologo: quest 5 preparata nel gruppo \"" + NomeGruppo + "\" (mausoleo nel cimitero, Edric). "
            + "Rifai \"Villaggio: chiudi i vicoli ciechi\", poi salva la scena (Ctrl+S) e Play.");
    }

    // Collega la fine della quest 4 all'inizio della quest 5 (lo richiama anche CreaPrologoQuest4 quando si rifà).
    internal static void Collega(Scene scena)
    {
        var gruppo = Trova(scena, NomeGruppo);
        var fineIlse = Trova(scena, "Dialogo: Ilse dopo la pozione");
        var missioniOggetto = Trova(scena, "Missioni");
        var edric = TrovaInterlocutore(scena, "Edric");
        var leggendaOggetto = Trova(scena, "Edric (la leggenda)");
        if (gruppo == null || fineIlse == null || missioniOggetto == null || edric == null || leggendaOggetto == null) return;
        var missioni = missioniOggetto.GetComponent<Missioni>();
        var leggenda = leggendaOggetto.GetComponent<Interlocutore>();
        var dialogo = fineIlse.GetComponent<DialogoAutomatico>();
        var e = Evento(dialogo, "alTermine");
        // via i collegamenti vecchi della quest 5 (rifacendo)
        for (int i = e.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            var bersaglio = e.GetPersistentTarget(i);
            string metodo = e.GetPersistentMethodName(i);
            if (bersaglio == null || (metodo == "ImpostaObiettivo" && bersaglio == missioni) || bersaglio == leggenda || bersaglio == edric
                || (metodo == "Attiva" && bersaglio is MetaObiettivo))
                UnityEventTools.RemovePersistentListener(e, i);
        }
        Su(e, missioni.ImpostaObiettivo, "ob.q5.edric");
        Su(e, Abilita(edric), false);
        Su(e, Abilita(leggenda), true);
        Meta(e, edric.gameObject);
        EditorUtility.SetDirty(dialogo);
    }

    // L'Interlocutore di un personaggio (l'oggetto con quel nome che ce l'ha).
    static Interlocutore TrovaInterlocutore(Scene scena, string nome)
    {
        foreach (var radice in scena.GetRootGameObjects())
            foreach (var i in radice.GetComponentsInChildren<Interlocutore>(true))
                if (i.gameObject.name == nome) return i;
        return null;
    }

    static Bounds Limiti(GameObject o)
    {
        var r = o.GetComponentsInChildren<Renderer>();
        if (r.Length == 0) return new Bounds(o.transform.position, Vector3.one * 10f);
        Bounds b = r[0].bounds;
        foreach (var x in r) if (x.name.StartsWith("Recinto")) b.Encapsulate(x.bounds);
        return b;
    }

    // Spegne lapidi e tombe che finirebbero dentro il mausoleo.
    static void SpegniLapidi(Transform cimitero, Transform area, Vector3 dimensioni)
    {
        foreach (Transform t in cimitero.GetComponentsInChildren<Transform>(true))
        {
            if (t.name != "Lapide" && t.name != "Tomba") continue;
            Vector3 locale = area.InverseTransformPoint(t.position);
            if (Mathf.Abs(locale.x) > dimensioni.x * 0.5f + 0.8f || Mathf.Abs(locale.z) > dimensioni.z * 0.5f + 1.2f) continue;
            Undo.RecordObject(t.gameObject, "Prologo quest 5");
            t.gameObject.SetActive(false);
        }
    }
}
