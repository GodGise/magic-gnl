using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Strumento dell'editor: controlla che le 6 quest del prologo siano collegate in sequenza, senza intoppi
// (richiesta di Lorenzo del 14 ottobre: "finita una si passa alla prossima").
// Cosa controlla nella scena VillaggioLagoNero (e nelle zone Cripta e Taverna):
//   - nessun evento ha un collegamento rotto (oggetto cancellato, metodo vuoto);
//   - nessun personaggio ha due dialoghi con lo stesso nome (il guaio di Edric nella quest 5);
//   - ogni quest 1-5 ha la sua fine (QuestFinita) e quella fine dà l'obiettivo della quest dopo, senza toglierlo
//     subito dopo con TogliObiettivo;
//   - la fine della quest 4 accende il dialogo di Edric che apre davvero la cripta;
//   - alla fine della quest 5 si apre la taverna (OrarioPrologo, "All'ultima quest") e all'uscita parte la razzia;
//   - le scene delle zone sono nelle Build Settings.
// Alla fine una finestra dice "tutto a posto" oppure l'elenco dei problemi (anche nella Console).
// Come si usa: aprire Assets/Scenes/VillaggioLagoNero.unity, poi menu "magic-gnl > Prologo: controlla la sequenza
// delle quest". Non cambia niente nella scena. Non va montato su niente.
public static class ControlloPrologo
{
    static readonly List<string> problemi = new List<string>();
    static readonly List<string> ok = new List<string>();

    [MenuItem("magic-gnl/Prologo: controlla la sequenza delle quest")]
    static void Controlla()
    {
        problemi.Clear();
        ok.Clear();
        Scene scena = SceneManager.GetActiveScene();
        if (scena.name != "VillaggioLagoNero")
        {
            EditorUtility.DisplayDialog("Controllo prologo", "Apri prima la scena VillaggioLagoNero.", "Ok");
            return;
        }

        var eventi = TuttiGliEventi(scena);
        CollegamentiRotti(eventi, scena.name);
        DialoghiDoppi(scena);
        FiniDelleQuest(eventi);
        Quest5(scena);
        Quest6(scena, eventi);
        Zone();

        var testo = new StringBuilder();
        if (problemi.Count == 0) testo.AppendLine("Tutto a posto: le quest 1-6 sono collegate in sequenza.");
        else
        {
            testo.AppendLine(problemi.Count + " problemi:");
            foreach (var p in problemi) testo.AppendLine("- " + p);
        }
        testo.AppendLine();
        testo.AppendLine("Controllato:");
        foreach (var o in ok) testo.AppendLine("+ " + o);
        if (problemi.Count == 0) Debug.Log("Controllo prologo\n" + testo);
        else Debug.LogWarning("Controllo prologo\n" + testo);
        EditorUtility.DisplayDialog("Controllo prologo", testo.ToString(), "Ok");
    }

    // ---------- eventi ----------

    struct Evento
    {
        public Component dove;
        public string campo;
        public UnityEvent e;
    }

    static List<Evento> TuttiGliEventi(Scene scena)
    {
        var elenco = new List<Evento>();
        foreach (var radice in scena.GetRootGameObjects())
            foreach (var c in radice.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (c == null) continue;
                foreach (var f in c.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    if (f.FieldType == typeof(UnityEvent) && f.GetValue(c) is UnityEvent e)
                        elenco.Add(new Evento { dove = c, campo = f.Name, e = e });
            }
        return elenco;
    }

    static void CollegamentiRotti(List<Evento> eventi, string nomeScena)
    {
        int rotti = 0, totali = 0;
        foreach (var ev in eventi)
            for (int i = 0; i < ev.e.GetPersistentEventCount(); i++)
            {
                totali++;
                if (ev.e.GetPersistentTarget(i) != null && !string.IsNullOrEmpty(ev.e.GetPersistentMethodName(i))) continue;
                rotti++;
                problemi.Add(nomeScena + ": collegamento rotto in \"" + Percorso(ev.dove.transform) + "\" > " + ev.campo
                    + " (metodo " + ev.e.GetPersistentMethodName(i) + ")");
            }
        if (rotti == 0) ok.Add(nomeScena + ": " + totali + " collegamenti negli eventi, nessuno rotto");
    }

    static void DialoghiDoppi(Scene scena)
    {
        var visti = new Dictionary<string, int>();
        foreach (var radice in scena.GetRootGameObjects())
        {
            foreach (var i in radice.GetComponentsInChildren<Interlocutore>(true)) Conta(visti, Percorso(i.transform));
            foreach (var d in radice.GetComponentsInChildren<DialogoAutomatico>(true)) Conta(visti, Percorso(d.transform));
        }
        int doppi = 0;
        foreach (var kv in visti)
            if (kv.Value > 1) { doppi++; problemi.Add("dialogo doppio: \"" + kv.Key + "\" c'è " + kv.Value + " volte (rifai la quest che lo crea)"); }
        if (doppi == 0) ok.Add(visti.Count + " dialoghi, nessuno doppio");
    }

    static void Conta(Dictionary<string, int> visti, string chiave) => visti[chiave] = visti.TryGetValue(chiave, out int n) ? n + 1 : 1;

    // Ogni quest 1-5: c'è la fine, e la fine dà l'obiettivo dopo senza toglierlo subito.
    static void FiniDelleQuest(List<Evento> eventi)
    {
        for (int q = 1; q <= 5; q++)
        {
            int trovate = 0;
            foreach (var ev in eventi)
            {
                int finita = -1;
                for (int i = 0; i < ev.e.GetPersistentEventCount(); i++)
                    if (ev.e.GetPersistentMethodName(i) == "QuestFinita" && Intero(ev.e, i) == q) finita = i;
                if (finita < 0) continue;
                trovate++;
                string dove = "fine della quest " + q + " (\"" + Percorso(ev.dove.transform) + "\")";
                // l'ultimo cambio di obiettivo deve essere un ImpostaObiettivo (non un TogliObiettivo)
                string ultimo = null, chiave = null;
                for (int i = 0; i < ev.e.GetPersistentEventCount(); i++)
                {
                    string m = ev.e.GetPersistentMethodName(i);
                    if (m == "ImpostaObiettivo") { ultimo = m; chiave = Stringa(ev.e, i); }
                    else if (m == "TogliObiettivo") ultimo = m;
                }
                if (q == 5)
                {
                    ok.Add(dove + ": quest 5 finita (il passo alla quest 6 lo fa OrarioPrologo)");
                    continue;
                }
                if (ultimo != "ImpostaObiettivo") problemi.Add(dove + ": dopo la fine non c'è l'obiettivo della quest " + (q + 1));
                else ok.Add(dove + " -> obiettivo \"" + chiave + "\"");
            }
            if (trovate == 0) problemi.Add("quest " + q + ": nessun evento la segna finita (manca QuestFinita " + q + ")");
        }
    }

    // La fine della quest 4 deve accendere la leggenda di Edric che apre l'ingresso della cripta.
    static void Quest5(Scene scena)
    {
        var fineIlse = Trova(scena, "Dialogo: Ilse dopo la pozione");
        var ingresso = Trova(scena, "Ingresso della cripta");
        if (fineIlse == null || ingresso == null) { problemi.Add("quest 5: manca la fine della quest 4 o l'ingresso della cripta"); return; }
        var passaggio = ingresso.GetComponent<PassaggioScena>();
        var e = Campo(fineIlse.GetComponent<DialogoAutomatico>(), "alTermine");
        bool accende = false;
        for (int i = 0; e != null && i < e.GetPersistentEventCount(); i++)
        {
            if (e.GetPersistentMethodName(i) != "set_enabled" || !(e.GetPersistentTarget(i) is Interlocutore leggenda)) continue;
            var dopo = Campo(leggenda, "alTermine");
            for (int k = 0; dopo != null && k < dopo.GetPersistentEventCount(); k++)
                if (dopo.GetPersistentTarget(k) == passaggio && dopo.GetPersistentMethodName(k) == "set_enabled") accende = true;
        }
        if (accende) ok.Add("quest 4 -> Edric racconta la leggenda -> si apre l'ingresso della cripta");
        else problemi.Add("quest 5: il dialogo di Edric acceso alla fine della quest 4 non apre l'ingresso della cripta (rifai la quest 5)");

        var medaglione = Trova(scena, "Attesa: medaglione preso");
        var uscita = Trova(scena, "Attesa: uscita dalla cripta");
        if (medaglione == null || uscita == null) problemi.Add("quest 5: mancano le attese del medaglione o dell'uscita dalla cripta");
        else ok.Add("quest 5: attese del medaglione e dell'uscita dalla cripta presenti");
    }

    // Fine della quest 5 -> taverna; uscita dalla taverna -> notte e razzia.
    static void Quest6(Scene scena, List<Evento> eventi)
    {
        var orario = Object.FindFirstObjectByType<OrarioPrologo>();
        if (orario == null) { problemi.Add("manca OrarioPrologo (prepara la quest 1)"); return; }
        var ultima = Campo(orario, "allUltimaQuest");
        bool obiettivo = false, apre = false;
        for (int i = 0; ultima != null && i < ultima.GetPersistentEventCount(); i++)
        {
            if (ultima.GetPersistentMethodName(i) == "ImpostaObiettivo" && Stringa(ultima, i) == "ob.q6.taverna") obiettivo = true;
            if (ultima.GetPersistentMethodName(i) == "set_enabled" && ultima.GetPersistentTarget(i) is PassaggioScena) apre = true;
        }
        if (obiettivo && apre) ok.Add("quest 5 finita -> \"Raggiungi la taverna\" e la porta della taverna si apre");
        else problemi.Add("quest 6: alla fine della quest 5 non si apre la taverna (prepara la quest 6 e la razzia)");

        var uscita = Trova(scena, "Attesa: uscita dalla taverna");
        var notte = uscita != null ? Campo(uscita.GetComponent<AscoltaSegnale>(), "quandoArriva") : null;
        bool notteSubito = false;
        for (int i = 0; notte != null && i < notte.GetPersistentEventCount(); i++)
            if (notte.GetPersistentMethodName(i) == "NotteSubito") notteSubito = true;
        if (notteSubito) ok.Add("uscita dalla taverna -> notte");
        else problemi.Add("quest 6: uscendo dalla taverna non cala la notte");

        var razzia = Campo(orario, "allaRazzia");
        bool parte = false;
        for (int i = 0; razzia != null && i < razzia.GetPersistentEventCount(); i++)
            if (razzia.GetPersistentMethodName(i) == "Inizia" && razzia.GetPersistentTarget(i) is RazziaVillaggio) parte = true;
        if (parte) ok.Add("notte -> parte la razzia");
        else problemi.Add("razzia: \"Alla razzia\" di OrarioPrologo non fa partire RazziaVillaggio");
    }

    // Le zone: nelle Build Settings e senza collegamenti rotti.
    static void Zone()
    {
        foreach (string nome in new[] { "Cripta", "Taverna" })
        {
            string percorso = "Assets/Scenes/" + nome + ".unity";
            bool inBuild = false;
            foreach (var s in EditorBuildSettings.scenes) if (s.path == percorso && s.enabled) inBuild = true;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(percorso) == null) { problemi.Add("manca la scena " + percorso); continue; }
            if (!inBuild) problemi.Add(percorso + " non è nelle Build Settings (non si potrebbe entrare)");
            var aperta = SceneManager.GetSceneByPath(percorso);
            bool daChiudere = !aperta.IsValid() || !aperta.isLoaded;
            if (daChiudere) aperta = EditorSceneManager.OpenScene(percorso, OpenSceneMode.Additive);
            CollegamentiRotti(TuttiGliEventi(aperta), nome);
            if (daChiudere) EditorSceneManager.CloseScene(aperta, true);
        }
    }

    // ---------- attrezzi ----------

    static UnityEvent Campo(Component c, string campo)
    {
        if (c == null) return null;
        var f = c.GetType().GetField(campo, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return f?.GetValue(c) as UnityEvent;
    }

    // Gli argomenti salvati di un collegamento (int e string) letti dal file: Unity non li espone direttamente.
    static int Intero(UnityEvent e, int i) => Argomenti(e, i)?.FindPropertyRelative("m_IntArgument").intValue ?? -1;
    static string Stringa(UnityEvent e, int i) => Argomenti(e, i)?.FindPropertyRelative("m_StringArgument").stringValue;


    static SerializedProperty Argomenti(UnityEvent e, int i)
    {
        foreach (var radice in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var c in radice.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (c == null) continue;
                foreach (var f in c.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (f.FieldType != typeof(UnityEvent) || f.GetValue(c) != e) continue;
                    var so = new SerializedObject(c);
                    var lista = so.FindProperty(f.Name + ".m_PersistentCalls.m_Calls");
                    if (lista == null || i >= lista.arraySize) return null;
                    return lista.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments");
                }
            }
        return null;
    }

    static GameObject Trova(Scene scena, string nome)
    {
        foreach (var radice in scena.GetRootGameObjects())
            foreach (var t in radice.GetComponentsInChildren<Transform>(true))
                if (t.name == nome) return t.gameObject;
        return null;
    }

    static string Percorso(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
}
