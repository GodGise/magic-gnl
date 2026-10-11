using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Strumento dell'editor: prepara nella scena VillaggioLagoNero (aperta) l'inizio del prologo e la quest 1
// "Un'altra mattina" (Docs/quest-capitolo-1.md, Docs/dialoghi-capitolo-1.md), tutto con forme provvisorie.
// Cosa fa, in un gruppo nuovo "Prologo - quest 1" (tutto si sposta e si regola a mano dopo):
//   - spegne il blocco pieno "Casa dell'eroe" e al suo posto costruisce una casa vuota dentro, con la porta dallo
//     stesso lato: pavimento, muri, tetto, letto, camino acceso, tavolo e una luce calda;
//   - Maren vicino al camino e Tobin accanto al letto (forme provvisorie);
//   - un capanno della legna aperto, accanto alla casa, con la catasta;
//   - il giocatore in piedi accanto al letto;
//   - Missioni (obiettivo a schermo e suggerimenti), OrarioPrologo (il tempo a tappe) e la dissolvenza dal nero;
//   - la quest 1 già collegata con gli eventi (si vedono e si cambiano nell'Inspector):
//       risveglio (Tobin, Maren, "L'eroe si alza") -> suggerimenti per muoversi e parlare -> parli con Maren
//       -> obiettivo "Porta la legna", Tobin ti segue -> E alla catasta, Tobin parla -> E al camino, Maren parla
//       -> quest 1 finita (il sole avanza), obiettivo "Vai dal fabbro" -> arrivi alla fucina, obiettivo tolto;
//   - spegne l'orco di prova della piazza e i segnalini (di giorno l'orco attaccherebbe).
// Non salva la scena: dopo il comando si guarda il risultato e si salva con Ctrl+S.
// Se il gruppo esiste già, chiede se rifarlo da capo (le modifiche fatte a mano a quel gruppo si perdono).
// Come si usa: aprire Assets/Scenes/VillaggioLagoNero.unity, poi menu "magic-gnl > Prologo: prepara la quest 1".
// Non va montato su niente: è un comando dell'editor e non finisce nel gioco.
public static class CreaPrologoQuest1
{
    const string NomeGruppo = "Prologo - quest 1";
    const string Cartella = "Assets/Segnaposto/Prologo";

    static Material legno, legnoScuro, pietra, paglia, fuoco, stoffaMaren, stoffaTobin, pelle;

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

        PreparaMateriali();
        var gruppo = new GameObject(NomeGruppo).transform;
        Undo.RegisterCreatedObjectUndo(gruppo.gameObject, "Prologo quest 1");

        // ---------- la casa vuota al posto del blocco pieno ----------
        Transform tc = casa.transform;
        Vector3 scala = tc.lossyScale;
        float W = Mathf.Max(scala.x, 7f), D = Mathf.Max(scala.z, 6f), H = Mathf.Max(scala.y, 3.2f);
        float s = 1f, portaX = 0f;
        Transform porta = tc.Find("Porta");
        if (porta != null && porta.gameObject.activeSelf)
        {
            s = Mathf.Sign(porta.localPosition.z);
            portaX = porta.localPosition.x * scala.x;
        }
        portaX = Mathf.Clamp(portaX, -W / 2f + 1.6f, W / 2f - 1.6f);
        Undo.RecordObject(casa, "Prologo quest 1");
        casa.SetActive(false);

        var interno = new GameObject("Casa dell'eroe (vuota dentro)").transform;
        interno.SetParent(gruppo, false);
        interno.SetPositionAndRotation(new Vector3(tc.position.x, tc.position.y - scala.y / 2f, tc.position.z), tc.rotation);
        const float t = 0.3f;
        Blocco("Pavimento", interno, new Vector3(0f, 0.05f, 0f), new Vector3(W, 0.1f, D), legnoScuro);
        Blocco("Muro di fondo", interno, new Vector3(0f, H / 2f, -s * (D / 2f - t / 2f)), new Vector3(W, H, t), pietra);
        Blocco("Muro sinistro", interno, new Vector3(-(W / 2f - t / 2f), H / 2f, 0f), new Vector3(t, H, D), pietra);
        Blocco("Muro destro", interno, new Vector3(W / 2f - t / 2f, H / 2f, 0f), new Vector3(t, H, D), pietra);
        float zFronte = s * (D / 2f - t / 2f), a = portaX - 0.9f, b = portaX + 0.9f;
        Blocco("Muro della porta (sinistra)", interno, new Vector3((-W / 2f + a) / 2f, H / 2f, zFronte), new Vector3(a + W / 2f, H, t), pietra);
        Blocco("Muro della porta (destra)", interno, new Vector3((b + W / 2f) / 2f, H / 2f, zFronte), new Vector3(W / 2f - b, H, t), pietra);
        Blocco("Sopra la porta", interno, new Vector3(portaX, (2.4f + H) / 2f, zFronte), new Vector3(1.8f, H - 2.4f, t), pietra);
        Blocco("Tetto", interno, new Vector3(0f, H + 0.15f, 0f), new Vector3(W + 0.6f, 0.3f, D + 0.6f), paglia);

        // Arredi: letto nell'angolo in fondo a destra, camino sul muro sinistro, tavolo al centro.
        float xLetto = W / 2f - t - 0.7f, zLetto = -s * (D / 2f - t - 1.1f);
        Blocco("Letto", interno, new Vector3(xLetto, 0.3f, zLetto), new Vector3(1.3f, 0.5f, 2.1f), legno);
        Blocco("Coperta", interno, new Vector3(xLetto, 0.58f, zLetto + s * 0.25f), new Vector3(1.25f, 0.08f, 1.4f), stoffaMaren);
        float xCamino = -(W / 2f - t - 0.45f), zCamino = -s * (D / 2f - 2.4f);
        Blocco("Camino", interno, new Vector3(xCamino, 1.1f, zCamino), new Vector3(0.9f, 2.2f, 1.7f), pietra);
        var braci = Blocco("Fuoco del camino", interno, new Vector3(xCamino + 0.5f, 0.3f, zCamino), new Vector3(0.3f, 0.25f, 0.9f), fuoco);
        braci.GetComponent<Collider>().enabled = false;
        Luce("Luce del camino", interno, new Vector3(xCamino + 0.9f, 0.8f, zCamino), new Color(1f, 0.55f, 0.25f), 7f, 1.6f);
        Luce("Luce della stanza", interno, new Vector3(0f, H - 0.6f, 0f), new Color(1f, 0.85f, 0.65f), 9f, 0.6f);
        Blocco("Tavolo", interno, new Vector3(-0.6f, 0.4f, s * 0.6f), new Vector3(1.6f, 0.8f, 0.9f), legno);

        // ---------- Maren e Tobin ----------
        var maren = Personaggio("Maren", interno, new Vector3(xCamino + 1.5f, 0f, zCamino + s * 0.4f), 1.75f, stoffaMaren, true);
        maren.transform.rotation = Quaternion.LookRotation(interno.TransformDirection(new Vector3(1f, 0f, s * 0.6f)));
        var tobin = Personaggio("Tobin", interno, new Vector3(xLetto - 1.3f, 0f, zLetto - s * 0.4f), 1.2f, stoffaTobin, false);
        tobin.transform.rotation = Quaternion.LookRotation(interno.TransformDirection(new Vector3(0f, 0f, s)));
        var segui = Undo.AddComponent<SeguiGiocatore>(tobin);
        Imposta(segui, "altezzaDaTerra", 0.6f);
        segui.enabled = false;

        // ---------- il capanno della legna, accanto alla casa ----------
        var capanno = new GameObject("Capanno della legna").transform;
        capanno.SetParent(gruppo, false);
        capanno.SetPositionAndRotation(interno.TransformPoint(new Vector3(W / 2f + 3.5f, 0f, s * (D / 2f - 1.5f))), interno.rotation);
        Blocco("Fondo", capanno, new Vector3(0f, 1.2f, -s * 1.3f), new Vector3(3f, 2.4f, 0.2f), legno);
        Blocco("Lato sinistro", capanno, new Vector3(-1.4f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 2.6f), legno);
        Blocco("Lato destro", capanno, new Vector3(1.4f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 2.6f), legno);
        Blocco("Tettoia", capanno, new Vector3(0f, 2.5f, 0f), new Vector3(3.4f, 0.15f, 3f), paglia);
        var catasta = new GameObject("Catasta").transform;
        catasta.SetParent(capanno, false);
        catasta.localPosition = new Vector3(0f, 0f, -s * 0.6f);
        for (int i = 0; i < 9; i++)
        {
            var ceppo = Forma(PrimitiveType.Cylinder, "Ceppo", catasta, new Vector3(-0.8f + (i % 3) * 0.8f, 0.2f + (i / 3) * 0.36f, 0f), new Vector3(0.34f, 0.9f, 0.34f), legno);
            ceppo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        // ---------- registro delle missioni, orario, dissolvenza ----------
        var missioni = Undo.AddComponent<Missioni>(Vuoto("Missioni", gruppo));
        if (UnityEngine.Object.FindFirstObjectByType<OrarioPrologo>() == null)
            Undo.AddComponent<OrarioPrologo>(Vuoto("Orario del prologo", gruppo));
        Undo.AddComponent<DissolvenzaIniziale>(Vuoto("Dissolvenza dal nero", gruppo));

        // ---------- la quest 1, collegata con gli eventi ----------
        // Maren al ritorno con la legna.
        var marenRitorno = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: Maren al ritorno", maren.transform));
        Imposta(marenRitorno, "righe", new[] { Riga("pg.maren", "dlg.q1.09") });
        // Tobin al capanno.
        var tobinCapanno = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: Tobin al capanno", tobin.transform));
        Imposta(tobinCapanno, "righe", new[] { Riga("pg.tobin", "dlg.q1.08") });

        // Arrivo alla fucina (si accende alla fine della quest 1).
        var fucina = Vuoto("Arrivo dal fabbro", gruppo);
        fucina.transform.position = PuntoDavantiAllaPorta(scena, "Fabbro", interno.position);
        var zona = Undo.AddComponent<ZonaArrivo>(fucina);
        Imposta(zona, "raggio", 5f);
        UnityEventTools.AddVoidPersistentListener(Evento(zona, "allArrivo"), missioni.TogliObiettivo);
        fucina.SetActive(false);

        // Camino: E per mettere la legna (si accende quando hai preso la legna).
        var camino = Vuoto("Punto: metti la legna nel fuoco", interno);
        camino.transform.localPosition = new Vector3(xCamino + 0.9f, 0.5f, zCamino);
        var puntoCamino = Undo.AddComponent<PuntoInterazione>(camino);
        Imposta(puntoCamino, "chiaveAzione", "hud.metti_legna");
        Imposta(puntoCamino, "raggio", 1.8f);
        UnityEventTools.AddPersistentListener(Evento(puntoCamino, "alPremere"), marenRitorno.Avvia);
        camino.SetActive(false);

        // Fine della quest 1, dopo le parole di Maren.
        var fineMaren = Evento(marenRitorno, "alTermine");
        UnityEventTools.AddStringPersistentListener(fineMaren, missioni.ImpostaObiettivo, "ob.q1.fabbro");
        UnityEventTools.AddIntPersistentListener(fineMaren, missioni.QuestFinita, 1);
        UnityEventTools.AddBoolPersistentListener(fineMaren, Abilita(segui), false);
        UnityEventTools.AddBoolPersistentListener(fineMaren, fucina.SetActive, true);

        // Catasta: E per prendere la legna (si accende dopo aver parlato con Maren).
        var puntoLegna = Vuoto("Punto: prendi la legna", capanno);
        puntoLegna.transform.localPosition = new Vector3(0f, 0.5f, -s * 0.2f);
        var prendi = Undo.AddComponent<PuntoInterazione>(puntoLegna);
        Imposta(prendi, "chiaveAzione", "hud.prendi_legna");
        Imposta(prendi, "raggio", 2.2f);
        UnityEventTools.AddPersistentListener(Evento(prendi, "alPremere"), tobinCapanno.Avvia);
        UnityEventTools.AddBoolPersistentListener(Evento(prendi, "alPremere"), camino.SetActive, true);
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
        UnityEventTools.AddStringPersistentListener(fineParla, missioni.ImpostaObiettivo, "ob.q1.legna");
        UnityEventTools.AddBoolPersistentListener(fineParla, Abilita(segui), true);
        UnityEventTools.AddBoolPersistentListener(fineParla, puntoLegna.SetActive, true);
        // Presa la legna, Maren non risponde più al tasto: altrimenti al camino (vicino a lei) il tasto farebbe
        // partire la sua riga ripetuta invece di "Metti la legna nel fuoco".
        UnityEventTools.AddBoolPersistentListener(Evento(prendi, "alPremere"), Abilita(parlaMaren), false);

        // Risveglio: parte da solo dopo la dissolvenza; poi i suggerimenti per muoversi e per parlare.
        var risveglio = Undo.AddComponent<DialogoAutomatico>(Vuoto("Dialogo: risveglio", gruppo));
        Imposta(risveglio, "righe", new[] { Riga("pg.tobin", "dlg.q1.01"), Riga("pg.maren", "dlg.q1.02"), Riga("", "dlg.q1.03") });
        Imposta(risveglio, "allAvvio", true);
        Imposta(risveglio, "ritardo", 3.5f);
        var fineRisveglio = Evento(risveglio, "alTermine");
        UnityEventTools.AddStringPersistentListener(fineRisveglio, missioni.Suggerisci, "tip.q1.muovi");
        UnityEventTools.AddStringPersistentListener(fineRisveglio, missioni.Suggerisci, "tip.q1.parla");

        // ---------- il giocatore accanto al letto ----------
        var giocatore = UnityEngine.Object.FindFirstObjectByType<GiocatoreControllo>(FindObjectsInactive.Include);
        if (giocatore != null)
        {
            Undo.RecordObject(giocatore.transform, "Prologo quest 1");
            giocatore.transform.position = interno.TransformPoint(new Vector3(xLetto - 0.2f, 1.1f, zLetto + s * 1.8f));
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

        foreach (var c in new Component[] { missioni, segui, zona, puntoCamino, prendi, parlaMaren, marenRitorno, tobinCapanno, risveglio })
            EditorUtility.SetDirty(c);
        EditorSceneManager.MarkSceneDirty(scena);
        Selection.activeGameObject = gruppo.gameObject;
        Debug.Log("Prologo: quest 1 preparata nel gruppo \"" + NomeGruppo + "\". Controlla e salva la scena (Ctrl+S), poi Play.");
    }

    // ---------- aiuti ----------

    static RigaDialogo Riga(string parlante, string testo) => new RigaDialogo { parlante = parlante, testo = testo };

    // Il campo privato di un componente (righe, eventi...), letto o scritto per nome.
    static void Imposta(Component c, string campo, object valore)
    {
        var f = c.GetType().GetField(campo, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (f == null) { Debug.LogWarning("Prologo: campo " + campo + " non trovato in " + c.GetType().Name); return; }
        f.SetValue(c, valore);
    }

    static UnityEvent Evento(Component c, string campo)
    {
        var f = c.GetType().GetField(campo, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var e = (UnityEvent)f.GetValue(c);
        if (e == null) { e = new UnityEvent(); f.SetValue(c, e); }
        return e;
    }

    // "enabled = vero/falso" di un componente, da usare in un evento.
    static UnityAction<bool> Abilita(Behaviour b) =>
        (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), b, typeof(Behaviour).GetProperty("enabled").GetSetMethod());

    static GameObject Trova(Scene scena, string nome)
    {
        foreach (var radice in scena.GetRootGameObjects())
        {
            if (radice.name == nome) return radice;
            foreach (var t in radice.GetComponentsInChildren<Transform>(true))
                if (t.name == nome) return t.gameObject;
        }
        return null;
    }

    // Due metri fuori dalla porta di un edificio (se non trova la porta, davanti al centro, verso "verso").
    static Vector3 PuntoDavantiAllaPorta(Scene scena, string nome, Vector3 verso)
    {
        var edificio = Trova(scena, nome);
        if (edificio == null) return verso;
        Transform porta = edificio.transform.Find("Porta");
        Vector3 centro = edificio.transform.position;
        Vector3 punto = porta != null && porta.gameObject.activeSelf ? porta.position : centro;
        Vector3 fuori = punto - centro;
        fuori.y = 0f;
        if (fuori.sqrMagnitude < 0.01f) fuori = verso - centro;
        fuori.y = 0f;
        punto += fuori.normalized * 2f;
        punto.y = 0f;
        return punto;
    }

    static GameObject Vuoto(string nome, Transform genitore)
    {
        var o = new GameObject(nome);
        o.transform.SetParent(genitore, false);
        return o;
    }

    static GameObject Personaggio(string nome, Transform genitore, Vector3 piedi, float altezza, Material vestito, bool solido)
    {
        var radice = new GameObject(nome);
        radice.transform.SetParent(genitore, false);
        radice.transform.localPosition = piedi + Vector3.up * (altezza / 2f);
        var corpo = Forma(PrimitiveType.Capsule, "Corpo", radice.transform, Vector3.zero, new Vector3(altezza * 0.28f, altezza * 0.42f, altezza * 0.28f), vestito);
        corpo.transform.localPosition = Vector3.down * altezza * 0.08f;
        Forma(PrimitiveType.Sphere, "Testa", radice.transform, Vector3.up * altezza * 0.4f, Vector3.one * altezza * 0.17f, pelle);
        var naso = Forma(PrimitiveType.Cube, "Naso (dove guarda)", radice.transform, new Vector3(0f, altezza * 0.4f, altezza * 0.09f), Vector3.one * altezza * 0.04f, pelle);
        UnityEngine.Object.DestroyImmediate(naso.GetComponent<Collider>());
        if (solido)
        {
            var c = radice.AddComponent<CapsuleCollider>();
            c.height = altezza;
            c.radius = 0.3f;
        }
        return radice;
    }

    static GameObject Blocco(string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale)
    {
        var o = Forma(PrimitiveType.Cube, nome, genitore, posizione, scala, materiale);
        o.isStatic = true;
        return o;
    }

    static GameObject Forma(PrimitiveType tipo, string nome, Transform genitore, Vector3 posizione, Vector3 scala, Material materiale)
    {
        var o = GameObject.CreatePrimitive(tipo);
        o.name = nome;
        o.transform.SetParent(genitore, false);
        o.transform.localPosition = posizione;
        o.transform.localScale = scala;
        o.GetComponent<Renderer>().sharedMaterial = materiale;
        if (tipo != PrimitiveType.Cube)
        {
            // Solo i blocchi fanno da muro: sfere, capsule e ceppi sono decorazione.
            UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());
        }
        return o;
    }

    static void Luce(string nome, Transform genitore, Vector3 posizione, Color colore, float portata, float intensita)
    {
        var o = Vuoto(nome, genitore);
        o.transform.localPosition = posizione;
        var l = o.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = colore;
        l.range = portata;
        l.intensity = intensita;
    }

    static void PreparaMateriali()
    {
        legno = Materiale("prologo-legno", new Color(0.36f, 0.25f, 0.16f));
        legnoScuro = Materiale("prologo-pavimento", new Color(0.22f, 0.16f, 0.11f));
        pietra = Materiale("prologo-pietra", new Color(0.42f, 0.4f, 0.37f));
        paglia = Materiale("prologo-paglia", new Color(0.5f, 0.42f, 0.25f));
        fuoco = Materiale("prologo-fuoco", new Color(1f, 0.5f, 0.15f), true);
        stoffaMaren = Materiale("prologo-maren", new Color(0.45f, 0.18f, 0.16f));
        stoffaTobin = Materiale("prologo-tobin", new Color(0.25f, 0.35f, 0.22f));
        pelle = Materiale("prologo-pelle", new Color(0.78f, 0.62f, 0.5f));
    }

    static Material Materiale(string nome, Color colore, bool luminoso = false)
    {
        string[] parti = Cartella.Split('/');
        string attuale = parti[0];
        for (int i = 1; i < parti.Length; i++)
        {
            string prossima = attuale + "/" + parti[i];
            if (!AssetDatabase.IsValidFolder(prossima)) AssetDatabase.CreateFolder(attuale, parti[i]);
            attuale = prossima;
        }
        string percorso = Cartella + "/" + nome + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(percorso);
        if (m != null) return m;
        m = new Material(Shader.Find("Standard")) { color = colore };
        m.SetFloat("_Glossiness", 0.05f);
        if (luminoso)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", colore * 2f);
        }
        AssetDatabase.CreateAsset(m, percorso);
        return m;
    }
}
