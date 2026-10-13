using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: a quest finita, chi l'ha data (o chi c'entra) non chiede più di rifare il lavoro ma ringrazia
// (richiesta di Lorenzo, 13 ottobre: Hobb, a ratti morti, chiedeva ancora di ucciderli).
// Per ogni personaggio mette le "Righe Dopo La Quest" nel suo Interlocutore e collega Interlocutore > QuestCompiuta
// al dialogo che chiude la quest:
//   - Maren   (quest 1, dopo "Dialogo: Maren al ritorno"): ringrazia per la legna; torna a parlare.
//   - Brannoc (quest 2, dopo l'allenamento alla fucina o lo studio con Edric): Guerriero e Ladro / Stregone.
//   - Edric   (quest 2, idem): lo Stregone / le altre classi.
//   - Hobb    (quest 3, dopo "Dialogo: Hobb dopo i ratti"): il granaio è libero.
// Ilse (quest 4) non serve: a quest finita vicino a lei c'è solo "Commercia" (la bottega).
// Si può rifare quante volte si vuole: toglie i collegamenti vecchi e li rimette. Lo rifanno da soli anche i comandi
// "Prologo: prepara la quest N", così rifacendo una quest i ringraziamenti restano.
// Come si usa: aprire Assets/Scenes/VillaggioLagoNero.unity, poi menu "magic-gnl > Prologo: ringraziamenti dopo le
// quest", poi salvare (Ctrl+S). Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class CreaRingraziamenti
{
    [MenuItem("magic-gnl/Prologo: ringraziamenti dopo le quest")]
    static void Menu()
    {
        Scene scena = SceneManager.GetActiveScene();
        if (scena.name != "VillaggioLagoNero")
        {
            EditorUtility.DisplayDialog("Prologo", "Apri la scena VillaggioLagoNero.", "Ok");
            return;
        }
        int fatti = Applica(scena);
        EditorSceneManager.MarkSceneDirty(scena);
        EditorUtility.DisplayDialog("Prologo", "Ringraziamenti collegati per " + fatti + " personaggi. Salva la scena (Ctrl+S).", "Ok");
    }

    // Restituisce quanti personaggi ha sistemato (quelli delle quest non ancora preparate vengono saltati).
    internal static int Applica(Scene scena)
    {
        int fatti = 0;
        string[] fineQuest2 = { "Dialogo: Brannoc dopo l'allenamento", "Dialogo: Edric dopo lo studio" };

        if (Collega(scena, "Maren", new[] { "Dialogo: Maren al ritorno" }, new[] { Riga("pg.maren", "dlg.dopo.maren") })) fatti++;
        if (Collega(scena, "Brannoc", fineQuest2, new[]
            {
                Riga("pg.brannoc", "dlg.dopo.brannoc", ClasseRiga.Guerriero), Riga("pg.brannoc", "dlg.dopo.brannoc", ClasseRiga.Ladro),
                Riga("pg.brannoc", "dlg.dopo.brannoc_s", ClasseRiga.Stregone),
            })) fatti++;
        if (Collega(scena, "Edric", fineQuest2, new[]
            {
                Riga("pg.edric", "dlg.dopo.edric", ClasseRiga.Stregone),
                Riga("pg.edric", "dlg.dopo.edric_altri", ClasseRiga.Guerriero), Riga("pg.edric", "dlg.dopo.edric_altri", ClasseRiga.Ladro),
            })) fatti++;
        if (Collega(scena, "Hobb", new[] { "Dialogo: Hobb dopo i ratti" }, new[] { Riga("pg.hobb", "dlg.dopo.hobb") })) fatti++;
        return fatti;
    }

    static bool Collega(Scene scena, string personaggio, string[] dialoghiDiFine, RigaDialogo[] righe)
    {
        // il personaggio con quel nome che ha un Interlocutore (nel villaggio ci possono essere altri oggetti con lo stesso nome)
        Interlocutore parla = null;
        foreach (var radice in scena.GetRootGameObjects())
            foreach (var i in radice.GetComponentsInChildren<Interlocutore>(true))
                if (parla == null && i.gameObject.name == personaggio) parla = i;
        if (parla == null) return false;
        bool collegato = false;
        foreach (string nome in dialoghiDiFine)
        {
            var d = Trova(scena, nome);
            var dialogo = d != null ? d.GetComponent<DialogoAutomatico>() : null;
            if (dialogo == null) continue;
            UnityEvent e = Evento(dialogo, "alTermine");
            for (int i = e.GetPersistentEventCount() - 1; i >= 0; i--)   // via i collegamenti vecchi (anche quelli rotti)
                if (e.GetPersistentMethodName(i) == nameof(Interlocutore.QuestCompiuta)
                    && (e.GetPersistentTarget(i) == null || e.GetPersistentTarget(i) == parla))
                    UnityEventTools.RemovePersistentListener(e, i);
            Su(e, parla.QuestCompiuta);
            EditorUtility.SetDirty(dialogo);
            collegato = true;
        }
        if (!collegato) return false;
        Undo.RecordObject(parla, "Ringraziamenti");
        Imposta(parla, "righeDopoLaQuest", righe);
        return true;
    }
}
