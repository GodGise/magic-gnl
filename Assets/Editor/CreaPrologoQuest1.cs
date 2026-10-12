using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: prepara nella scena VillaggioLagoNero (aperta) l'inizio del prologo e la quest 1
// "Un'altra mattina" (Docs/quest-capitolo-1.md, Docs/dialoghi-capitolo-1.md), tutto con forme provvisorie.
// Cosa fa, in un gruppo nuovo "Prologo - quest 1" (tutto si sposta e si regola a mano dopo):
//   - spegne il blocco pieno "Casa dell'eroe" e al suo posto costruisce una casa vuota dentro, con la porta dallo
//     stesso lato e il tetto a due falde: finestre, stipiti, letto grande e lettino di Tobin, baule, camino acceso con
//     il cesto della legna, tavolo con sgabelli, mensole, tappeto, luci calde;
//   - fuori: soglia, sentiero di terra dalla porta al capanno, panca, botti e casse;
//   - il capanno della legna con la catasta, il ceppo e l'ascia;
//   - Maren vicino al camino, Tobin accanto al suo lettino, il giocatore accanto al letto;
//   - Missioni (obiettivo e suggerimenti), OrarioPrologo (tempo a tappe) e la dissolvenza dal nero;
//   - la quest 1 già collegata con gli eventi (si vedono e si cambiano nell'Inspector):
//       risveglio (Tobin, Maren, "L'eroe si alza") -> suggerimenti per muoversi e parlare -> parli con Maren
//       -> obiettivo "Porta la legna", Tobin ti segue -> E alla catasta: la legna in braccio, Tobin parla
//       -> E al camino: la legna nel cesto, il fuoco si ravviva, Maren parla -> quest 1 finita (il sole avanza),
//       obiettivo "Vai dal fabbro" -> arrivi alla fucina (lì parte la quest 2, CreaPrologoQuest2);
//   - la bussola in alto indica Maren, la catasta, il camino e la fucina (MetaObiettivo);
//   - spegne l'orco di prova della piazza e i segnalini (di giorno l'orco attaccherebbe).
// Per una scena già preparata con la versione vecchia c'è "magic-gnl > Prologo: aggiungi la bussola alla quest 1".
// Non salva la scena: dopo il comando si guarda il risultato e si salva con Ctrl+S.
// Se il gruppo esiste già, chiede se rifarlo da capo (le modifiche fatte a mano a quel gruppo si perdono).
// Come si usa: aprire Assets/Scenes/VillaggioLagoNero.unity, poi menu "magic-gnl > Prologo: prepara la quest 1".
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
[InitializeOnLoad]
public static class CreaPrologoQuest1
{
    internal const string NomeGruppo = "Prologo - quest 1";

    // Prima di ogni Play: se la scena ha la quest 1 ma non ancora le mete della bussola, le aggiunge da sola.
    static CreaPrologoQuest1()
    {
        EditorApplication.playModeStateChanged += stato =>
        {
            if (stato != PlayModeStateChange.ExitingEditMode) return;
            Scene scena = SceneManager.GetActiveScene();
            var gruppo = Trova(scena, NomeGruppo);
            if (gruppo == null || gruppo.GetComponentInChildren<MetaObiettivo>(true) != null) return;
            int nuovi = AggiungiBussola(scena);
            if (nuovi == 0) return;
            EditorSceneManager.MarkSceneDirty(scena);
            Debug.Log("Prologo: aggiunte da sole le mete della bussola alla quest 1 (" + nuovi + "). Dopo il Play salva la scena (Ctrl+S).");
        };
    }

    [MenuItem("magic-gnl/Prologo: prepara la quest 1")]
    static void Prepara()
    {
        Scene scena = SceneManager.GetActiveScene();
        if (scena.name != "VillaggioLagoNero")
        {
            EditorUtility.DisplayDialog("Prologo", "Apri prima la scena Assets/Scenes/VillaggioLagoNero.unity.", "Ok");
            return;
        }
        GameObject casa = Trova(scena, "Casa dell'eroe");
        if (casa == null)
        {
            EditorUtility.DisplayDialog("Prologo", "Nella scena non trovo l'oggetto \"Casa dell'eroe\".", "Ok");
            return;
        }
        GameObject vecchio = Trova(scena, NomeGruppo);
        if (vecchio != null)
        {
            if (!EditorUtility.DisplayDialog("Prologo", "Il gruppo \"" + NomeGruppo + "\" esiste già. Rifarlo da capo? Le modifiche fatte a mano a quel gruppo andranno perse.", "Rifai", "Annulla")) return;
            Undo.DestroyObjectImmediate(vecchio);
        }

        var gruppo = new GameObject(NomeGruppo).transform;
        Undo.RegisterCreatedObjectUndo(gruppo.gameObject, "Prologo quest 1");

        // ---------- misure della casa ----------
        Transform tc = casa.transform;
        Vector3 scala = tc.lossyScale;
        float W = Mathf.Max(scala.x, 8f), D = Mathf.Max(scala.z, 7f), H = Mathf.Clamp(scala.y, 3.6f, 4.4f);   // spazio per la camera
        float s = 1f, portaX = 0f;
        Transform porta = tc.Find("Porta");
        if (porta != null && porta.gameObject.activeSelf)
        {
            s = Mathf.Sign(porta.localPosition.z);
            portaX = porta.localPosition.x * scala.x;
        }
        portaX = Mathf.Clamp(portaX, -W / 2f + 1.8f, W / 2f - 1.8f);
        Undo.RecordObject(casa, "Prologo quest 1");
        casa.SetActive(false);

        var interno = Vuoto("Casa dell'eroe (vuota dentro)", gruppo).transform;
        interno.SetPositionAndRotation(new Vector3(tc.position.x, tc.position.y - scala.y / 2f, tc.position.z), tc.rotation);
        const float t = 0.3f;

        // ---------- muri, finestre, tetto ----------
        Blocco("Pavimento", interno, new Vector3(0f, 0.05f, 0f), new Vector3(W, 0.1f, D), LegnoScuro);
        float zFronte = s * (D / 2f - t / 2f), zFondo = -s * (D / 2f - t / 2f);
        MuroX("Muro della porta", interno, zFronte, W, H, t, portaX, 1.8f, 0f, 2.4f);
        MuroX("Muro di fondo", interno, zFondo, W, H, t, W / 2f - 3.2f, 1.2f, 1.1f, 2.1f);
        MuroZ("Muro sinistro", interno, -(W / 2f - t / 2f), D, H, t, s * 0.6f, 1.2f, 1.1f, 2.1f);
        MuroZ("Muro destro", interno, W / 2f - t / 2f, D, H, t, s * 0.4f, 1.2f, 1.1f, 2.1f);
        Blocco("Soffitto", interno, new Vector3(0f, H + 0.1f, 0f), new Vector3(W, 0.2f, D), LegnoScuro);
        float pendenza = 32f, falda = (D / 2f + 0.5f) / Mathf.Cos(pendenza * Mathf.Deg2Rad);
        float colmo = H + 0.2f + (D / 4f) * Mathf.Tan(pendenza * Mathf.Deg2Rad);
        Decoro("Falda davanti", interno, new Vector3(0f, colmo, D / 4f), new Vector3(W + 0.8f, 0.22f, falda), Paglia, new Vector3(pendenza, 0f, 0f));
        Decoro("Falda dietro", interno, new Vector3(0f, colmo, -D / 4f), new Vector3(W + 0.8f, 0.22f, falda), Paglia, new Vector3(-pendenza, 0f, 0f));
        Decoro("Timpano sinistro", interno, new Vector3(-(W / 2f - t / 2f), H + 0.2f + D * 0.15f, 0f), new Vector3(t, D * 0.3f, D * 0.55f), Pietra);
        Decoro("Timpano destro", interno, new Vector3(W / 2f - t / 2f, H + 0.2f + D * 0.15f, 0f), new Vector3(t, D * 0.3f, D * 0.55f), Pietra);
        // stipiti e soglia della porta
        Decoro("Stipite sinistro", interno, new Vector3(portaX - 0.95f, 1.2f, zFronte + s * 0.18f), new Vector3(0.15f, 2.4f, 0.12f), Legno);
        Decoro("Stipite destro", interno, new Vector3(portaX + 0.95f, 1.2f, zFronte + s * 0.18f), new Vector3(0.15f, 2.4f, 0.12f), Legno);
        Decoro("Architrave", interno, new Vector3(portaX, 2.45f, zFronte + s * 0.18f), new Vector3(2.05f, 0.15f, 0.12f), Legno);
        Blocco("Soglia", interno, new Vector3(portaX, 0.06f, s * (D / 2f + 0.5f)), new Vector3(2.2f, 0.12f, 1f), PietraScura);

        // ---------- arredi ----------
        float xLetto = W / 2f - t - 0.75f, zLetto = -s * (D / 2f - t - 1.15f);
        Blocco("Letto", interno, new Vector3(xLetto, 0.3f, zLetto), new Vector3(1.4f, 0.5f, 2.2f), Legno);
        Decoro("Coperta", interno, new Vector3(xLetto, 0.58f, zLetto + s * 0.25f), new Vector3(1.35f, 0.08f, 1.5f), Tappeto);
        Decoro("Cuscino", interno, new Vector3(xLetto, 0.62f, zLetto - s * 0.75f), new Vector3(0.9f, 0.12f, 0.4f), Stoffa);
        float xLettino = xLetto - 1.8f, zLettino = -s * (D / 2f - t - 0.9f);
        Blocco("Lettino di Tobin", interno, new Vector3(xLettino, 0.22f, zLettino), new Vector3(0.9f, 0.36f, 1.6f), Legno);
        Decoro("Coperta di Tobin", interno, new Vector3(xLettino, 0.42f, zLettino + s * 0.2f), new Vector3(0.86f, 0.06f, 1.1f), Materiale("prologo-tobin-coperta", new Color(0.25f, 0.35f, 0.22f)));
        Blocco("Baule", interno, new Vector3(xLetto, 0.3f, zLetto + s * 1.6f), new Vector3(1.1f, 0.6f, 0.6f), LegnoScuro);

        float xCamino = -(W / 2f - t - 0.45f), zCamino = -s * (D / 2f - 2.4f);
        Blocco("Camino", interno, new Vector3(xCamino, 1.1f, zCamino), new Vector3(0.9f, 2.2f, 1.8f), Pietra);
        Blocco("Canna fumaria", interno, new Vector3(xCamino, H + 0.9f, zCamino), new Vector3(0.7f, 2f, 0.9f), Pietra);
        Decoro("Fuoco del camino", interno, new Vector3(xCamino + 0.5f, 0.3f, zCamino), new Vector3(0.3f, 0.25f, 0.9f), Fuoco);
        Luce("Luce del camino", interno, new Vector3(xCamino + 0.9f, 0.8f, zCamino), new Color(1f, 0.55f, 0.25f), 7f, 1.4f);
        var ravvivato = Vuoto("Fuoco ravvivato", interno).transform;
        Decoro("Fiamme alte", ravvivato, new Vector3(xCamino + 0.5f, 0.55f, zCamino), new Vector3(0.25f, 0.45f, 0.7f), Fuoco);
        Luce("Luce del fuoco ravvivato", ravvivato, new Vector3(xCamino + 1f, 1f, zCamino), new Color(1f, 0.6f, 0.3f), 10f, 2.2f);
        ravvivato.gameObject.SetActive(false);
        Vector3 cesto = new Vector3(xCamino + 0.9f, 0f, zCamino + s * 1.3f);
        Blocco("Cesto della legna", interno, cesto + Vector3.up * 0.2f, new Vector3(0.8f, 0.4f, 0.6f), Paglia);
        var legnaNelCesto = Vuoto("Legna nel cesto", interno, cesto + Vector3.up * 0.45f).transform;
        Ceppi(legnaNelCesto, 3, 0.25f);
        legnaNelCesto.gameObject.SetActive(false);

        Blocco("Tavolo", interno, new Vector3(-0.4f, 0.4f, s * 0.7f), new Vector3(1.6f, 0.8f, 0.9f), Legno);
        Blocco("Sgabello 1", interno, new Vector3(-0.4f - 1.1f, 0.25f, s * 0.7f), new Vector3(0.45f, 0.5f, 0.45f), LegnoScuro);
        Blocco("Sgabello 2", interno, new Vector3(-0.4f + 1.1f, 0.25f, s * 0.7f), new Vector3(0.45f, 0.5f, 0.45f), LegnoScuro);
        Forma(PrimitiveType.Cylinder, "Ciotola", interno, new Vector3(-0.6f, 0.84f, s * 0.7f), new Vector3(0.3f, 0.04f, 0.3f), Stoffa);
        Decoro("Tappeto", interno, new Vector3(0f, 0.11f, -s * 0.3f), new Vector3(2.6f, 0.02f, 1.8f), Tappeto);
        for (int i = 0; i < 2; i++)
            Decoro("Mensola " + (i + 1), interno, new Vector3(-W / 2f + 2.6f, 1.4f + i * 0.6f, -s * (D / 2f - t - 0.2f)), new Vector3(1.6f, 0.08f, 0.35f), Legno);
        Luce("Luce della stanza", interno, new Vector3(0f, H - 0.6f, 0f), new Color(1f, 0.85f, 0.65f), 9f, 0.6f);
        // Dentro casa la camera sta vicina e bassa (ZonaCamera), se no finisce contro il soffitto e guarda dall'alto.
        var zonaCasa = Undo.AddComponent<ZonaCamera>(Vuoto("Zona camera (dentro casa)", interno));
        Imposta(zonaCasa, "dimensioni", new Vector3(W - 0.4f, H, D - 0.4f));
        Imposta(zonaCasa, "distanzaMassima", Mathf.Min(W, D) * 0.45f);
        Imposta(zonaCasa, "inclinazioneMassima", 28f);

        // ---------- fuori: sentiero, panca, botti ----------
        Vector3 fuoriPorta = new Vector3(portaX, 0f, s * (D / 2f + 1f));
        Vector3 capannoLocale = new Vector3(W / 2f + 4f, 0f, s * (D / 2f - 1.5f));
        Sentiero(interno, fuoriPorta, fuoriPorta + new Vector3(0f, 0f, s * 5f));
        Sentiero(interno, fuoriPorta + new Vector3(0f, 0f, s * 2f), capannoLocale + new Vector3(0f, 0f, s * 2.2f));
        Blocco("Panca", interno, new Vector3(portaX - 2.4f, 0.25f, s * (D / 2f + 0.6f)), new Vector3(1.6f, 0.5f, 0.45f), Legno);
        Forma(PrimitiveType.Cylinder, "Botte", interno, new Vector3(-W / 2f + 0.8f, 0.5f, s * (D / 2f + 0.7f)), new Vector3(0.8f, 0.5f, 0.8f), Legno, true);
        Blocco("Cassa", interno, new Vector3(-W / 2f + 1.9f, 0.35f, s * (D / 2f + 0.6f)), new Vector3(0.7f, 0.7f, 0.7f), LegnoScuro);

        // ---------- il capanno della legna ----------
        var capanno = Vuoto("Capanno della legna", gruppo).transform;
        capanno.SetPositionAndRotation(interno.TransformPoint(capannoLocale), interno.rotation);
        // Più alto e con il tetto solido: la camera non ci passa attraverso e il giocatore ci sta in piedi comodo.
        Blocco("Fondo", capanno, new Vector3(0f, 1.5f, -s * 1.3f), new Vector3(3.6f, 3f, 0.2f), Legno);
        Blocco("Lato sinistro", capanno, new Vector3(-1.7f, 1.5f, 0f), new Vector3(0.2f, 3f, 2.6f), Legno);
        Blocco("Lato destro", capanno, new Vector3(1.7f, 1.5f, 0f), new Vector3(0.2f, 3f, 2.6f), Legno);
        Blocco("Tettoia", capanno, new Vector3(0f, 3.15f, s * 0.2f), new Vector3(4f, 0.18f, 3.4f), Paglia, 0f);
        Blocco("Palo sinistro", capanno, new Vector3(-1.7f, 1.5f, s * 1.75f), new Vector3(0.18f, 3f, 0.18f), LegnoScuro);
        Blocco("Palo destro", capanno, new Vector3(1.7f, 1.5f, s * 1.75f), new Vector3(0.18f, 3f, 0.18f), LegnoScuro);
        var zonaCapanno = Undo.AddComponent<ZonaCamera>(Vuoto("Zona camera (capanno)", capanno));
        Imposta(zonaCapanno, "dimensioni", new Vector3(3.4f, 3f, 2.6f));
        Imposta(zonaCapanno, "distanzaMassima", 2.4f);
        Imposta(zonaCapanno, "inclinazioneMassima", 25f);
        var catasta = Vuoto("Catasta", capanno, new Vector3(0f, 0f, -s * 0.7f)).transform;
        for (int fila = 0; fila < 4; fila++)
            for (int i = 0; i < 4; i++)
                Forma(PrimitiveType.Cylinder, "Ceppo", catasta, new Vector3(-1.0f + i * 0.66f, 0.18f + fila * 0.33f, 0f), new Vector3(0.32f, 0.85f, 0.32f), Legno)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Vector3 ceppo = new Vector3(0.9f, 0f, s * 2.4f);
        Forma(PrimitiveType.Cylinder, "Ceppo per spaccare", capanno, ceppo + Vector3.up * 0.3f, new Vector3(0.7f, 0.3f, 0.7f), LegnoScuro, true);
        Decoro("Manico dell'ascia", capanno, ceppo + new Vector3(0.05f, 0.8f, 0f), new Vector3(0.06f, 0.8f, 0.06f), Legno, new Vector3(0f, 0f, 20f));
        Decoro("Lama dell'ascia", capanno, ceppo + new Vector3(-0.08f, 0.62f, 0f), new Vector3(0.28f, 0.2f, 0.04f), Ferro, new Vector3(0f, 0f, 20f));
        for (int i = 0; i < 3; i++)
            Forma(PrimitiveType.Cylinder, "Ceppo a terra", capanno, ceppo + new Vector3(-0.7f - i * 0.35f, 0.15f, 0.4f * (i - 1)), new Vector3(0.3f, 0.4f, 0.3f), Legno)
                .transform.localRotation = Quaternion.Euler(90f, 30f * i, 0f);

        // ---------- registro delle missioni, orario, dissolvenza ----------
        var missioni = Undo.AddComponent<Missioni>(Vuoto("Missioni", gruppo));
        if (Object.FindFirstObjectByType<OrarioPrologo>() == null) Undo.AddComponent<OrarioPrologo>(Vuoto("Orario del prologo", gruppo));
        Undo.AddComponent<DissolvenzaIniziale>(Vuoto("Dissolvenza dal nero", gruppo));

        // ---------- Maren e Tobin ----------
        var maren = Personaggio("Maren", interno, new Vector3(xCamino + 1.6f, 0.1f, zCamino + s * 0.3f), 1.75f, new Color(0.45f, 0.18f, 0.16f), true);
        maren.transform.rotation = Quaternion.LookRotation(interno.TransformDirection(new Vector3(1f, 0f, s * 0.6f)));
        var tobin = Personaggio("Tobin", interno, new Vector3(xLettino, 0.1f, zLettino + s * 1.3f), 1.2f, new Color(0.25f, 0.35f, 0.22f), false);
        tobin.transform.rotation = Quaternion.LookRotation(interno.TransformDirection(new Vector3(0f, 0f, s)));
        var segui = Undo.AddComponent<SeguiGiocatore>(tobin);
        Imposta(segui, "altezzaDaTerra", 0.6f);
        segui.enabled = false;

        // ---------- la legna in braccio ----------
        var legnaInBraccio = Vuoto("Legna in braccio", capanno, new Vector3(0f, 1f, 0f)).transform;
        Ceppi(legnaInBraccio, 3, 0.22f);
        var trasporta = Undo.AddComponent<TrasportaOggetto>(legnaInBraccio.gameObject);
        legnaInBraccio.gameObject.SetActive(false);

        // ---------- la quest 1, collegata con gli eventi ----------
        var marenRitorno = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: Maren al ritorno", maren.transform));
        Imposta(marenRitorno, "righe", new[] { Riga("pg.maren", "dlg.q1.09") });
        var tobinCapanno = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: Tobin al capanno", tobin.transform));
        Imposta(tobinCapanno, "righe", new[] { Riga("pg.tobin", "dlg.q1.08") });

        // Arrivo alla fucina (si accende alla fine della quest 1; la quest 2 aggiunge qui i suoi collegamenti).
        var fucina = Vuoto("Arrivo dal fabbro", gruppo);
        if (Porta(scena, "Fabbro", out Vector3 pFabbro, out Vector3 fFabbro)) fucina.transform.position = pFabbro + fFabbro * 2f;
        var zona = Undo.AddComponent<ZonaArrivo>(fucina);
        Imposta(zona, "raggio", 5f);
        Su(Evento(zona, "allArrivo"), missioni.TogliObiettivo);
        fucina.SetActive(false);

        // Camino: E per mettere la legna (si accende quando hai preso la legna).
        var camino = Vuoto("Punto: metti la legna nel fuoco", interno, new Vector3(xCamino + 0.9f, 0.5f, zCamino + s * 0.6f));
        var puntoCamino = Undo.AddComponent<PuntoInterazione>(camino);
        Imposta(puntoCamino, "chiaveAzione", "hud.metti_legna");
        Imposta(puntoCamino, "raggio", 1.8f);
        var mettiLegna = Evento(puntoCamino, "alPremere");
        Su(mettiLegna, trasporta.Posa);
        Su(mettiLegna, legnaNelCesto.gameObject.SetActive, true);
        Su(mettiLegna, ravvivato.gameObject.SetActive, true);
        Su(mettiLegna, marenRitorno.Avvia);
        camino.SetActive(false);

        // Fine della quest 1, dopo le parole di Maren.
        var fineMaren = Evento(marenRitorno, "alTermine");
        Su(fineMaren, missioni.ImpostaObiettivo, "ob.q1.fabbro");
        Su(fineMaren, missioni.QuestFinita, 1);
        Su(fineMaren, Abilita(segui), false);
        Su(fineMaren, fucina.SetActive, true);
        Meta(fineMaren, fucina);

        // Catasta: E per prendere la legna (si accende dopo aver parlato con Maren).
        var puntoLegna = Vuoto("Punto: prendi la legna", capanno, new Vector3(0f, 0.5f, -s * 0.1f));
        var prendi = Undo.AddComponent<PuntoInterazione>(puntoLegna);
        Imposta(prendi, "chiaveAzione", "hud.prendi_legna");
        Imposta(prendi, "raggio", 2.2f);
        var prendiLegna = Evento(prendi, "alPremere");
        Su(prendiLegna, trasporta.Prendi);
        Su(prendiLegna, tobinCapanno.Avvia);
        Su(prendiLegna, camino.SetActive, true);
        Meta(prendiLegna, camino);
        puntoLegna.SetActive(false);

        // Maren: il dialogo in casa; alla fine parte la quest della legna e Tobin ti segue.
        var parlaMaren = Undo.AddComponent<Interlocutore>(maren);
        Imposta(parlaMaren, "nomeChiave", "pg.maren");
        Imposta(parlaMaren, "righe", new[]
        {
            Riga("pg.maren", "dlg.q1.04"), Riga("pg.tobin", "dlg.q1.05"), Riga("pg.maren", "dlg.q1.06"), Riga("pg.maren", "dlg.q1.07"),
        });
        Imposta(parlaMaren, "righeRipetute", new[] { Riga("pg.maren", "dlg.q1.07") });
        var fineParla = Evento(parlaMaren, "alTermine");
        Su(fineParla, missioni.ImpostaObiettivo, "ob.q1.legna");
        Su(fineParla, Abilita(segui), true);
        Su(fineParla, puntoLegna.SetActive, true);
        Meta(fineParla, puntoLegna);
        // Presa la legna, Maren non risponde più al tasto (al camino, vicino a lei, il tasto mette la legna nel fuoco).
        Su(prendiLegna, Abilita(parlaMaren), false);

        // Risveglio: parte da solo dopo la dissolvenza; poi i suggerimenti per muoversi e per parlare.
        var risveglio = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: risveglio", gruppo));
        Imposta(risveglio, "righe", new[] { Riga("pg.tobin", "dlg.q1.01"), Riga("pg.maren", "dlg.q1.02"), Riga("", "dlg.q1.03") });
        Imposta(risveglio, "allAvvio", true);
        Imposta(risveglio, "ritardo", 3.5f);
        var fineRisveglio = Evento(risveglio, "alTermine");
        Su(fineRisveglio, missioni.Suggerisci, "tip.q1.muovi");
        Su(fineRisveglio, missioni.Suggerisci, "tip.q1.parla");
        Meta(fineRisveglio, maren);

        // ---------- il giocatore accanto al letto ----------
        var giocatore = Object.FindFirstObjectByType<GiocatoreControllo>(FindObjectsInactive.Include);
        if (giocatore != null)
        {
            Undo.RecordObject(giocatore.transform, "Prologo quest 1");
            giocatore.transform.position = interno.TransformPoint(new Vector3(xLetto - 0.2f, 1.1f, zLetto + s * 2.4f));
            giocatore.transform.rotation = Quaternion.LookRotation(interno.TransformDirection(new Vector3(-1f, 0f, s * 0.3f)));
        }

        // ---------- di giorno l'orco di prova e i segnalini non servono ----------
        foreach (string nome in new[] { "Orco della piazza", "Segnalini (proposte)" })
        {
            var o = Trova(scena, nome);
            if (o == null) continue;
            Undo.RecordObject(o, "Prologo quest 1");
            o.SetActive(false);
        }

        EditorSceneManager.MarkSceneDirty(scena);
        Selection.activeGameObject = gruppo.gameObject;
        Debug.Log("Prologo: quest 1 preparata nel gruppo \"" + NomeGruppo + "\". Controlla e salva la scena (Ctrl+S), poi Play. Per la fucina: \"Prologo: prepara la quest 2\".");
    }

    // ---------- pezzi della casa ----------

    // Muro lungo X (davanti o dietro) con un'apertura (porta o finestra) centrata in "c", larga "l", da "basso" a "alto".
    static void MuroX(string nome, Transform genitore, float z, float W, float H, float t, float c, float l, float basso, float alto)
    {
        float a = c - l / 2f, b = c + l / 2f;
        Blocco(nome + " (sinistra)", genitore, new Vector3((-W / 2f + a) / 2f, H / 2f, z), new Vector3(a + W / 2f, H, t), Pietra);
        Blocco(nome + " (destra)", genitore, new Vector3((b + W / 2f) / 2f, H / 2f, z), new Vector3(W / 2f - b, H, t), Pietra);
        Blocco(nome + " (sopra)", genitore, new Vector3(c, (alto + H) / 2f, z), new Vector3(l, H - alto, t), Pietra);
        if (basso > 0f) Blocco(nome + " (sotto)", genitore, new Vector3(c, basso / 2f, z), new Vector3(l, basso, t), Pietra);
    }

    // Muro lungo Z (di lato) con una finestra centrata in "c".
    static void MuroZ(string nome, Transform genitore, float x, float D, float H, float t, float c, float l, float basso, float alto)
    {
        float a = c - l / 2f, b = c + l / 2f;
        Blocco(nome + " (dietro)", genitore, new Vector3(x, H / 2f, (-D / 2f + a) / 2f), new Vector3(t, H, a + D / 2f), Pietra);
        Blocco(nome + " (davanti)", genitore, new Vector3(x, H / 2f, (b + D / 2f) / 2f), new Vector3(t, H, D / 2f - b), Pietra);
        Blocco(nome + " (sopra)", genitore, new Vector3(x, (alto + H) / 2f, c), new Vector3(t, H - alto, l), Pietra);
        Blocco(nome + " (sotto)", genitore, new Vector3(x, basso / 2f, c), new Vector3(t, basso, l), Pietra);
    }

    // Una striscia di terra battuta da un punto all'altro (a filo del terreno, senza collider).
    static void Sentiero(Transform genitore, Vector3 da, Vector3 a)
    {
        Vector3 d = a - da;
        d.y = 0f;
        var o = Decoro("Sentiero", genitore, (da + a) / 2f + Vector3.up * 0.015f, new Vector3(1.4f, 0.03f, d.magnitude + 1.4f), Terra);
        if (d.sqrMagnitude > 0.01f) o.transform.localRotation = Quaternion.LookRotation(d.normalized);
    }

    // Qualche ceppo coricato, uno accanto all'altro.
    static void Ceppi(Transform genitore, int quanti, float diametro)
    {
        for (int i = 0; i < quanti; i++)
            Forma(PrimitiveType.Cylinder, "Ceppo", genitore, new Vector3((i - (quanti - 1) / 2f) * diametro * 1.05f, i == 1 ? diametro * 0.6f : 0f, 0f),
                new Vector3(diametro, 0.35f, diametro), Legno).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // ---------- bussola per le scene preparate con la versione vecchia ----------

    [MenuItem("magic-gnl/Prologo: aggiungi la bussola alla quest 1")]
    static void MenuBussola()
    {
        Scene scena = SceneManager.GetActiveScene();
        if (Trova(scena, NomeGruppo) == null)
        {
            EditorUtility.DisplayDialog("Prologo", "Apri la scena VillaggioLagoNero già preparata con \"Prologo: prepara la quest 1\".", "Ok");
            return;
        }
        int nuovi = AggiungiBussola(scena);
        EditorSceneManager.MarkSceneDirty(scena);
        Debug.Log("Prologo: bussola aggiunta alla quest 1 (" + nuovi + " collegamenti nuovi). Salva la scena (Ctrl+S), poi Play.");
    }

    static int AggiungiBussola(Scene scena)
    {
        var maren = Trova(scena, "Maren");
        var legna = Trova(scena, "Punto: prendi la legna");
        var camino = Trova(scena, "Punto: metti la legna nel fuoco");
        var fucina = Trova(scena, "Arrivo dal fabbro");
        var risveglio = Trova(scena, "Dialogo: risveglio");
        var ritorno = Trova(scena, "Dialogo: Maren al ritorno");
        int nuovi = 0;
        if (risveglio != null) nuovi += Meta(Evento(risveglio.GetComponent<DialogoAutomatico>(), "alTermine"), maren);
        if (maren != null) nuovi += Meta(Evento(maren.GetComponent<Interlocutore>(), "alTermine"), legna);
        if (legna != null) nuovi += Meta(Evento(legna.GetComponent<PuntoInterazione>(), "alPremere"), camino);
        if (ritorno != null) nuovi += Meta(Evento(ritorno.GetComponent<DialogoAutomatico>(), "alTermine"), fucina);
        if (maren == null || legna == null || camino == null || fucina == null || risveglio == null || ritorno == null)
            Debug.LogWarning("Prologo: un oggetto della quest 1 non si trova (rinominato?): bussola incompleta.");
        return nuovi;
    }
}
