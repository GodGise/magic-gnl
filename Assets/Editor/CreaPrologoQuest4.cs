using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static AiutiPrologo;

// Strumento dell'editor: prepara nella scena VillaggioLagoNero (aperta, con le quest 1, 2 e 3 già preparate) la quest 4
// "Le erbe di Ilse" (Docs/quest-capitolo-1.md, Docs/dialoghi-capitolo-1.md), con forme provvisorie, in un gruppo nuovo
// "Prologo - quest 4":
//   - spegne il blocco pieno "Erborista" e al suo posto costruisce la bottega di Ilse, vuota dentro: bancone, mensole
//     con i vasetti, erbe appese, calderone che fuma verde, tavolo da lavoro, ceste; dentro la camera sta più vicina;
//   - Ilse dietro il bancone;
//   - tre Fiori di riva nell'acqua bassa della riva del lago (a nord del molo, più a nord e a sud), spenti finché
//     Ilse non li chiede;
//   - crea, se mancano, fiori, pozioni e ricette della bottega (CreaErbe, Docs/erbe-e-pozioni.md) e mette su Ilse
//     il Negozio con le 5 ricette (3 Fiori di riva -> 2 Pozioni di Ilse, che ridanno 60 di vita in 1 secondo, e le
//     altre, che si potranno fare quando i nuovi fiori saranno nel mondo);
//   - tutto collegato con gli eventi:
//       fine della quest 3 (Hobb ringrazia) -> Ilse diventa parlabile, obiettivo "Vai dall'erborista Ilse"
//       -> parli con Ilse -> si accendono i fiori, obiettivo "Fiori di riva raccolti: 0/3" (la bussola indica il fiore
//          più vicino), suggerimenti su come raccogliere e sull'acqua bassa
//       -> con 3 fiori nello zaino: obiettivo "Porta i fiori a Ilse"
//       -> parli con Ilse -> si apre la bottega, obiettivo "Scambia i fiori con le pozioni da Ilse", suggerimento
//       -> primo scambio nella bottega -> obiettivo "Bevi una pozione: premi F", suggerimento
//          (da qui in poi vicino a Ilse c'è "E  Commercia: Ilse")
//       -> bevi una pozione (tasto Usa oggetto, F) -> Ilse racconta del lago, quest 4 finita (il sole avanza).
// Gli alberi del villaggio che toccherebbero la bottega si spengono (non si cancellano).
// Dopo questo comando conviene rifare "magic-gnl > Villaggio: chiudi i vicoli ciechi" (la bottega è cambiata).
// Non salva la scena: dopo il comando si guarda il risultato e si salva con Ctrl+S.
// Se il gruppo esiste già, chiede se rifarlo da capo (le modifiche fatte a mano a quel gruppo si perdono).
// Rifacendo la quest 3 vanno rifatti anche i collegamenti della quest 4: si rilancia questo comando.
// Come si usa: aprire Assets/Scenes/VillaggioLagoNero.unity, poi menu "magic-gnl > Prologo: prepara la quest 4".
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class CreaPrologoQuest4
{
    const string NomeGruppo = "Prologo - quest 4";

    [MenuItem("magic-gnl/Prologo: prepara la quest 4")]
    static void Prepara()
    {
        Scene scena = SceneManager.GetActiveScene();
        var missioniOggetto = Trova(scena, "Missioni");
        var fineHobb = Trova(scena, "Dialogo: Hobb dopo i ratti");
        var erborista = Trova(scena, "Erborista");
        if (scena.name != "VillaggioLagoNero" || missioniOggetto == null || fineHobb == null || erborista == null)
        {
            EditorUtility.DisplayDialog("Prologo", "Apri la scena VillaggioLagoNero e prepara prima le quest 1, 2 e 3.", "Ok");
            return;
        }
        GameObject vecchio = Trova(scena, NomeGruppo);
        if (vecchio != null)
        {
            if (!EditorUtility.DisplayDialog("Prologo", "Il gruppo \"" + NomeGruppo + "\" esiste già. Rifarlo da capo? Le modifiche fatte a mano a quel gruppo andranno perse.", "Rifai", "Annulla")) return;
            Undo.DestroyObjectImmediate(vecchio);
        }
        var missioni = missioniOggetto.GetComponent<Missioni>();
        var pozione = CreaErbe.PozioneDiIlse;
        var fiore = CreaErbe.FioreDiRiva;
        var ricette = CreaErbe.RicetteIlse();
        var gruppo = new GameObject(NomeGruppo).transform;
        Undo.RegisterCreatedObjectUndo(gruppo.gameObject, "Prologo quest 4");

        // ---------- la bottega di Ilse, al posto del blocco "Erborista" ----------
        Transform te = erborista.transform;
        Vector3 scala = te.lossyScale;
        float W = Mathf.Max(scala.x, 9f), D = Mathf.Max(scala.z, 8f), H = Mathf.Clamp(scala.y, 3.8f, 4.4f);
        float s = -1f, portaX = 0f;
        Transform portaVecchia = te.Find("Porta");
        if (portaVecchia != null)
        {
            s = Mathf.Sign(portaVecchia.localPosition.z);
            portaX = portaVecchia.localPosition.x * scala.x;
        }
        portaX = Mathf.Clamp(portaX, -W / 2f + 1.8f, W / 2f - 1.8f);
        Undo.RecordObject(erborista, "Prologo quest 4");
        erborista.SetActive(false);

        var bottega = Vuoto("Bottega di Ilse", gruppo).transform;
        bottega.SetPositionAndRotation(new Vector3(te.position.x, te.position.y - scala.y / 2f, te.position.z), te.rotation);
        const float t = 0.3f;
        Material intonaco = Materiale("prologo-bottega", new Color(0.46f, 0.43f, 0.36f));
        Material verde = Materiale("prologo-erbe", new Color(0.3f, 0.42f, 0.22f));
        Material verdeSecco = Materiale("prologo-erbe-secche", new Color(0.45f, 0.42f, 0.24f));
        Material pozioneVerde = Materiale("prologo-pozione-verde", new Color(0.35f, 0.9f, 0.4f), true);
        Material vetro = Materiale("prologo-vetro", new Color(0.55f, 0.62f, 0.6f));

        Blocco("Pavimento", bottega, new Vector3(0f, 0.05f, 0f), new Vector3(W, 0.1f, D), LegnoScuro);
        float zFronte = s * (D / 2f - t / 2f), zFondo = -s * (D / 2f - t / 2f);
        MuroX("Muro della porta", bottega, zFronte, W, H, t, portaX, 1.8f, 0f, 2.4f, intonaco);
        Blocco("Muro di fondo", bottega, new Vector3(0f, H / 2f, zFondo), new Vector3(W, H, t), intonaco);
        MuroZ("Muro sinistro", bottega, -(W / 2f - t / 2f), D, H, t, s * 1.2f, 1.2f, 1.2f, 2.2f, intonaco);
        MuroZ("Muro destro", bottega, W / 2f - t / 2f, D, H, t, s * 1.2f, 1.2f, 1.2f, 2.2f, intonaco);
        Blocco("Soffitto", bottega, new Vector3(0f, H + 0.1f, 0f), new Vector3(W, 0.2f, D), LegnoScuro);
        float pendenza = 34f, falda = (D / 2f + 0.5f) / Mathf.Cos(pendenza * Mathf.Deg2Rad);
        float colmo = H + 0.2f + (D / 4f) * Mathf.Tan(pendenza * Mathf.Deg2Rad);
        Decoro("Falda davanti", bottega, new Vector3(0f, colmo, D / 4f), new Vector3(W + 0.8f, 0.22f, falda), Paglia, new Vector3(pendenza, 0f, 0f));
        Decoro("Falda dietro", bottega, new Vector3(0f, colmo, -D / 4f), new Vector3(W + 0.8f, 0.22f, falda), Paglia, new Vector3(-pendenza, 0f, 0f));
        Decoro("Timpano sinistro", bottega, new Vector3(-(W / 2f - t / 2f), H + 0.2f + D * 0.15f, 0f), new Vector3(t, D * 0.3f, D * 0.55f), intonaco);
        Decoro("Timpano destro", bottega, new Vector3(W / 2f - t / 2f, H + 0.2f + D * 0.15f, 0f), new Vector3(t, D * 0.3f, D * 0.55f), intonaco);
        Decoro("Stipite sinistro", bottega, new Vector3(portaX - 0.95f, 1.2f, zFronte + s * 0.18f), new Vector3(0.15f, 2.4f, 0.12f), Legno);
        Decoro("Stipite destro", bottega, new Vector3(portaX + 0.95f, 1.2f, zFronte + s * 0.18f), new Vector3(0.15f, 2.4f, 0.12f), Legno);
        Decoro("Architrave", bottega, new Vector3(portaX, 2.45f, zFronte + s * 0.18f), new Vector3(2.05f, 0.15f, 0.12f), Legno);
        Blocco("Soglia", bottega, new Vector3(portaX, 0.06f, s * (D / 2f + 0.5f)), new Vector3(2.2f, 0.12f, 1f), PietraScura);
        // insegna: un mazzo d'erbe appeso sopra la porta
        Decoro("Insegna (mazzo d'erbe)", bottega, new Vector3(portaX + 1.4f, 2.6f, s * (D / 2f + 0.25f)), new Vector3(0.35f, 0.5f, 0.15f), verdeSecco);

        // ---------- dentro: bancone, mensole, erbe, calderone ----------
        float zBancone = -s * 0.9f;
        Blocco("Bancone", bottega, new Vector3(-0.3f, 0.5f, zBancone), new Vector3(4.6f, 1f, 0.7f), Legno);
        Decoro("Piano del bancone", bottega, new Vector3(-0.3f, 1.03f, zBancone), new Vector3(4.8f, 0.06f, 0.85f), LegnoScuro);
        Forma(PrimitiveType.Cylinder, "Mortaio", bottega, new Vector3(-1.6f, 1.14f, zBancone), new Vector3(0.28f, 0.1f, 0.28f), PietraScura);
        Forma(PrimitiveType.Cylinder, "Boccetta sul bancone", bottega, new Vector3(0.9f, 1.15f, zBancone), new Vector3(0.12f, 0.1f, 0.12f), pozioneVerde);
        Decoro("Mazzetto sul bancone", bottega, new Vector3(0.2f, 1.1f, zBancone + s * 0.1f), new Vector3(0.4f, 0.08f, 0.18f), verde, new Vector3(0f, 25f, 0f));

        float zMensole = -s * (D / 2f - t - 0.25f);
        Color[] colori = { new Color(0.7f, 0.15f, 0.12f), new Color(0.3f, 0.6f, 0.3f), new Color(0.75f, 0.6f, 0.2f), new Color(0.35f, 0.4f, 0.75f), new Color(0.5f, 0.3f, 0.55f) };
        for (int r = 0; r < 3; r++)
        {
            float y = 1.0f + r * 0.65f;
            Decoro("Mensola " + (r + 1), bottega, new Vector3(0f, y, zMensole), new Vector3(W - 2.4f, 0.08f, 0.4f), Legno);
            for (int i = 0; i < 9; i++)
            {
                float x = -(W - 3f) / 2f + i * (W - 3f) / 8f;
                Color c = colori[(i + r * 2) % colori.Length];
                bool luce = (i + r) % 4 == 0;
                Forma(PrimitiveType.Cylinder, "Vasetto", bottega, new Vector3(x, y + 0.16f, zMensole), new Vector3(0.18f, 0.12f + 0.04f * ((i + r) % 3), 0.18f),
                    Materiale("prologo-vasetto-" + ColorUtility.ToHtmlStringRGB(c).ToLowerInvariant() + (luce ? "-luce" : ""), c, luce));
            }
        }
        // erbe appese alle travi del soffitto
        for (int i = 0; i < 7; i++)
        {
            float x = -W / 2f + 1.2f + i * (W - 2.4f) / 6f;
            Decoro("Trave", bottega, new Vector3(x, H - 0.15f, 0f), new Vector3(0.18f, 0.18f, D - 0.6f), LegnoScuro);
            for (int k = -1; k <= 1; k++)
                Decoro("Erbe appese", bottega, new Vector3(x, H - 0.55f, k * 1.6f + 0.3f * (i % 2)), new Vector3(0.18f, 0.6f, 0.18f), (i + k) % 2 == 0 ? verde : verdeSecco);
        }
        // calderone in un angolo in fondo, con il vapore verde
        // (gli arredi stanno attaccati ai muri: niente angolini in cui infilarsi)
        Vector3 calderone = new Vector3(W / 2f - t - 0.75f, 0f, -s * (D / 2f - t - 0.75f));
        Forma(PrimitiveType.Cylinder, "Calderone", bottega, calderone + Vector3.up * 0.4f, new Vector3(1f, 0.4f, 1f), AiutiPrologo.Ferro, true);
        Decoro("Infuso", bottega, calderone + Vector3.up * 0.79f, new Vector3(0.85f, 0.02f, 0.85f), pozioneVerde);
        Decoro("Brace sotto il calderone", bottega, calderone + Vector3.up * 0.06f, new Vector3(0.7f, 0.08f, 0.7f), Fuoco);
        Luce("Luce del calderone", bottega, calderone + Vector3.up * 1.4f, new Color(0.45f, 1f, 0.5f), 6f, 1.3f);
        // tavolo da lavoro e ceste di erbe vicino alla porta
        Vector3 tavolo = new Vector3(-(W / 2f - t - 0.5f), 0f, s * 0.9f);
        Blocco("Tavolo da lavoro", bottega, tavolo + Vector3.up * 0.4f, new Vector3(1f, 0.8f, 1.8f), Legno);
        Decoro("Erbe sul tavolo", bottega, tavolo + new Vector3(0f, 0.84f, 0.3f), new Vector3(0.5f, 0.08f, 0.6f), verde);
        Forma(PrimitiveType.Cylinder, "Ampolla", bottega, tavolo + new Vector3(0.1f, 0.95f, -0.5f), new Vector3(0.2f, 0.15f, 0.2f), vetro);
        foreach (var p in new[] { new Vector3(W / 2f - t - 0.36f, 0f, s * (D / 2f - t - 0.36f)), new Vector3(W / 2f - t - 0.36f, 0f, s * (D / 2f - t - 1.1f)) })
        {
            Forma(PrimitiveType.Cylinder, "Cesta", bottega, p + Vector3.up * 0.25f, new Vector3(0.7f, 0.25f, 0.7f), Paglia, true);
            Decoro("Erbe nella cesta", bottega, p + Vector3.up * 0.52f, new Vector3(0.55f, 0.1f, 0.55f), verde);
        }
        Luce("Luce della bottega", bottega, new Vector3(0f, H - 0.7f, 0f), new Color(1f, 0.82f, 0.6f), 9f, 0.7f);
        var zonaBottega = Undo.AddComponent<ZonaCamera>(Vuoto("Zona camera (bottega)", bottega));
        Imposta(zonaBottega, "dimensioni", new Vector3(W - 0.4f, H, D - 0.4f));
        Imposta(zonaBottega, "distanzaMassima", Mathf.Min(W, D) * 0.45f);
        Imposta(zonaBottega, "inclinazioneMassima", 28f);
        SgomberaAlberi(scena, bottega, Vector3.zero, new Vector3(W + 2f, 0f, D + 3f));
        SgomberaCase(scena, bottega, Vector3.zero, new Vector3(W, 0f, D));   // una casa vicina entrava nella bottega

        // ---------- Ilse dietro il bancone ----------
        var ilse = Personaggio("Ilse", bottega, new Vector3(0f, 0.1f, -s * 1.9f), 1.55f, new Color(0.3f, 0.32f, 0.22f), true);
        ilse.transform.rotation = Quaternion.LookRotation(bottega.TransformDirection(new Vector3(0f, 0f, s)));

        // ---------- i fiori di riva, nell'acqua bassa ----------
        var fiori = Vuoto("Fiori di riva", gruppo).transform;
        float zMolo = DatiVillaggioLagoNero.Molo[1];
        float[] zFiori = { zMolo + 40f, zMolo + 16f, zMolo - 21f };
        var punti = new Transform[zFiori.Length];
        for (int i = 0; i < zFiori.Length; i++)
        {
            float z = zFiori[i];
            var f = Vuoto("Fiore di riva " + (i + 1), fiori, new Vector3(Riva(z) + 2.6f, -0.05f, z));
            var raccoglibile = Undo.AddComponent<OggettoRaccoglibile>(f);
            Imposta(raccoglibile, "oggetto", (DatiOggetto)fiore);
            Imposta(raccoglibile, "distanza", 2.2f);
            // qualche foglia galleggiante intorno, da guardare
            for (int k = 0; k < 3; k++)
                Forma(PrimitiveType.Cylinder, "Foglia sull'acqua", f.transform, Quaternion.Euler(0f, k * 120f + i * 40f, 0f) * new Vector3(0.55f, 0.03f, 0f),
                    new Vector3(0.45f, 0.01f, 0.35f), verde);
            punti[i] = f.transform;
        }
        fiori.gameObject.SetActive(false);

        // ---------- collegamenti ----------
        var raccolta = Undo.AddComponent<RaccoltaOggetti>(Vuoto("Raccolta: fiori di riva", gruppo));
        Imposta(raccolta, "oggetto", (DatiOggetto)fiore);
        Imposta(raccolta, "quanti", 3);
        Imposta(raccolta, "chiaveObiettivo", "ob.q4.fiori");
        Imposta(raccolta, "punti", punti);

        // la bottega: si accende quando Ilse ha i fiori, da lì in poi resta sempre aperta
        var negozio = Undo.AddComponent<Negozio>(Vuoto("Bottega (negozio di Ilse)", ilse.transform));
        Imposta(negozio, "nomeChiave", "pg.ilse");
        Imposta(negozio, "titoloChiave", "neg.bottega_ilse");
        Imposta(negozio, "ricette", ricette);
        Imposta(negozio, "raggio", 3f);
        negozio.enabled = false;

        var bevuta = Undo.AddComponent<QuandoUsato>(Vuoto("Attesa: bevi la pozione", gruppo));
        Imposta(bevuta, "oggetto", pozione);

        var fine = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: Ilse dopo la pozione", ilse.transform));
        Imposta(fine, "righe", new[] { Riga("pg.ilse", "dlg.q4.05") });
        var dopo = Evento(fine, "alTermine");
        Su(dopo, missioni.QuestFinita, 4);
        Su(dopo, missioni.TogliObiettivo);
        Su(Evento(bevuta, "quandoUsato"), fine.Avvia);

        // Ilse, la seconda volta: vede i fiori e apre la bottega (si accende con 3 fiori nello zaino).
        // Parla una volta sola: poi vicino a lei resta solo "Commercia".
        var conIFiori = Vuoto("Ilse (con i fiori)", ilse.transform);
        var parlaFine = Undo.AddComponent<Interlocutore>(conIFiori);
        Imposta(parlaFine, "nomeChiave", "pg.ilse");
        Imposta(parlaFine, "raggio", 3f);
        Imposta(parlaFine, "unaVoltaSola", true);
        Imposta(parlaFine, "righe", new[] { Riga("pg.ilse", "dlg.q4.04") });
        var dati = Evento(parlaFine, "alTermine");
        Su(dati, Abilita(negozio), true);
        Su(dati, negozio.Apri);
        Su(dati, missioni.ImpostaObiettivo, "ob.q4.scambia");
        Su(dati, missioni.Suggerisci, "tip.q4.bottega");
        parlaFine.enabled = false;

        // primo scambio nella bottega -> "Bevi una pozione"
        var scambiato = Evento(negozio, "alPrimoScambio");
        Su(scambiato, missioni.ImpostaObiettivo, "ob.q4.bevi");
        Su(scambiato, missioni.TogliMeta);
        Su(scambiato, missioni.Suggerisci, "tip.q4.usa");
        Su(scambiato, bevuta.Avvia);

        // Ilse, la prima volta: chiede i fiori.
        var parla = Undo.AddComponent<Interlocutore>(ilse);
        Imposta(parla, "nomeChiave", "pg.ilse");
        Imposta(parla, "raggio", 3f);
        Imposta(parla, "unaVoltaSola", false);
        Imposta(parla, "righe", new[] { Riga("pg.ilse", "dlg.q4.01"), Riga("pg.ilse", "dlg.q4.02"), Riga("pg.ilse", "dlg.q4.03") });
        Imposta(parla, "righeRipetute", new[] { Riga("pg.ilse", "dlg.q4.02") });
        var inizio = Evento(parla, "alTermine");
        Su(inizio, fiori.gameObject.SetActive, true);
        Su(inizio, raccolta.Avvia);
        Su(inizio, missioni.Suggerisci, "tip.q4.raccogli");
        Su(inizio, missioni.Suggerisci, "tip.q4.acqua");
        parla.enabled = false;   // Ilse parla solo dopo la quest 3 (così gli obiettivi non si accavallano)

        var presi = Evento(raccolta, "quandoCompletato");
        Su(presi, missioni.ImpostaObiettivo, "ob.q4.torna");
        Su(presi, Abilita(parla), false);
        Su(presi, Abilita(parlaFine), true);
        Meta(presi, ilse);

        // Fine della quest 3 (Hobb ringrazia) -> "Vai dall'erborista Ilse".
        var dopoHobb = Evento(fineHobb.GetComponent<DialogoAutomatico>(), "alTermine");
        for (int i = dopoHobb.GetPersistentEventCount() - 1; i >= 0; i--)   // rifacendo la quest 4: via i collegamenti vecchi
        {
            var bersaglio = dopoHobb.GetPersistentTarget(i);
            string metodo = dopoHobb.GetPersistentMethodName(i);
            if (bersaglio == null || (metodo == "ImpostaObiettivo" && bersaglio == missioni))
                UnityEventTools.RemovePersistentListener(dopoHobb, i);
        }
        Su(dopoHobb, missioni.ImpostaObiettivo, "ob.q4.ilse");
        Su(dopoHobb, Abilita(parla), true);
        Meta(dopoHobb, ilse);
        EditorUtility.SetDirty(fineHobb.GetComponent<DialogoAutomatico>());

        EditorSceneManager.MarkSceneDirty(scena);
        Selection.activeGameObject = gruppo.gameObject;
        Debug.Log("Prologo: quest 4 preparata nel gruppo \"" + NomeGruppo + "\" (bottega di Ilse con il negozio, 3 fiori di riva). "
            + "Rifai \"Villaggio: chiudi i vicoli ciechi\", poi salva la scena (Ctrl+S) e Play.");
    }

    // X della riva del lago all'altezza z (la riva è fatta a strisce: DatiVillaggioLagoNero.Riva, da nord a sud).
    static float Riva(float z)
    {
        float[] r = DatiVillaggioLagoNero.Riva;
        for (int i = 0; i + 3 < r.Length; i += 2)
            if (z <= r[i + 1] && z >= r[i + 3]) return (r[i] + r[i + 2]) / 2f;
        return r[0];
    }

    // ---------- muri ----------

    // Muro lungo X (davanti o dietro) con un'apertura (porta o finestra) centrata in "c", larga "l", da "basso" a "alto".
    static void MuroX(string nome, Transform genitore, float z, float W, float H, float t, float c, float l, float basso, float alto, Material m)
    {
        float a = c - l / 2f, b = c + l / 2f;
        Blocco(nome + " (sinistra)", genitore, new Vector3((-W / 2f + a) / 2f, H / 2f, z), new Vector3(a + W / 2f, H, t), m);
        Blocco(nome + " (destra)", genitore, new Vector3((b + W / 2f) / 2f, H / 2f, z), new Vector3(W / 2f - b, H, t), m);
        Blocco(nome + " (sopra)", genitore, new Vector3(c, (alto + H) / 2f, z), new Vector3(l, H - alto, t), m);
        if (basso > 0f) Blocco(nome + " (sotto)", genitore, new Vector3(c, basso / 2f, z), new Vector3(l, basso, t), m);
    }

    // Muro lungo Z (di lato) con una finestra centrata in "c".
    static void MuroZ(string nome, Transform genitore, float x, float D, float H, float t, float c, float l, float basso, float alto, Material m)
    {
        float a = c - l / 2f, b = c + l / 2f;
        Blocco(nome + " (dietro)", genitore, new Vector3(x, H / 2f, (-D / 2f + a) / 2f), new Vector3(t, H, a + D / 2f), m);
        Blocco(nome + " (davanti)", genitore, new Vector3(x, H / 2f, (b + D / 2f) / 2f), new Vector3(t, H, D / 2f - b), m);
        Blocco(nome + " (sopra)", genitore, new Vector3(x, (alto + H) / 2f, c), new Vector3(t, H - alto, l), m);
        Blocco(nome + " (sotto)", genitore, new Vector3(x, basso / 2f, c), new Vector3(t, basso, l), m);
    }
}
