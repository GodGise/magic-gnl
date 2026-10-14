using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Strumento dell'editor per provare il prologo in fretta: durante il Play porta il giocatore davanti a un punto
// della quest (Maren, la catasta, il camino, Brannoc, il manichino, Edric), senza doverci camminare.
// Le quest restano quelle vere: si salta solo la strada, non i passi (dialoghi e tasti vanno fatti lo stesso).
// Come si usa: in Play, menu "magic-gnl > Prova prologo > Vai da ...". Non va montato su niente.
// "Ferisci i ratti del granaio" toglie 20 di vita a ogni ratto (per provare la fine della quest 3 in fretta);
// "Togli 50 di vita al giocatore" serve a provare le pozioni (tasto F).
// Bottega e monete: "Apri la bottega di Ilse" porta da Ilse e apre il negozio anche prima della quest 4;
// "Dai 100 monete" e "Dai 3 fiori di ogni tipo" servono a provare acquisti, vendite e baratto.
// "Salta alla quest N": fa partire, in ordine, le fini vere delle quest prima di N (gli stessi eventi del gioco:
// obiettivi, personaggi accesi e spenti, ora che avanza) e porta il giocatore dove indica la bussola.
public static class ProvaPrologo
{
    const string Cartella = "magic-gnl/Prova prologo/";

    [MenuItem(Cartella + "Vai da Maren")] static void Maren() => Vai("Maren");
    [MenuItem(Cartella + "Vai alla catasta del capanno")] static void Catasta() => Vai("Punto: prendi la legna");
    [MenuItem(Cartella + "Vai al camino")] static void Camino() => Vai("Punto: metti la legna nel fuoco");
    [MenuItem(Cartella + "Vai alla fucina (arrivo)")] static void Fucina() => Vai("Arrivo dal fabbro");
    [MenuItem(Cartella + "Vai da Brannoc")] static void Brannoc() => Vai("Brannoc");
    [MenuItem(Cartella + "Vai al manichino")] static void Manichino() => Vai("Manichino di Brannoc");
    [MenuItem(Cartella + "Vai da Edric (Stregone)")] static void Edric() => Vai("Edric");
    [MenuItem(Cartella + "Vai da Hobb (granaio)")] static void Hobb() => Vai("Hobb");
    [MenuItem(Cartella + "Vai al centro del granaio")] static void Granaio() => Vai("Centro del granaio");
    [MenuItem(Cartella + "Vai da Ilse (erborista)")] static void Ilse() => Vai("Ilse");
    // Il primo dei tre fiori della quest 4 non ancora raccolto (spenti finché Ilse non li chiede).
    [MenuItem(Cartella + "Vai al prossimo fiore di riva")]
    static void Fiore()
    {
        for (int i = 1; i <= 3; i++)
        {
            // un fiore raccolto si spegne (activeSelf falso); quelli ancora da prendere restano accesi
            var f = TrovaOggetto("Fiore di riva " + i);
            if (f != null && f.activeSelf) { Vai("Fiore di riva " + i); return; }
        }
        Debug.Log("Prova prologo: nessun fiore di riva da raccogliere.");
    }

    // Porta al mausoleo del cimitero e apre l'ingresso della cripta anche prima della quest 5.
    [MenuItem(Cartella + "Vai alla cripta (ingresso aperto)")]
    static void Cripta()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        var ingresso = TrovaOggetto("Ingresso della cripta");
        if (ingresso == null) { Debug.LogWarning("Prova prologo: manca l'ingresso della cripta (prepara la quest 5)."); return; }
        var passaggio = ingresso.GetComponent<PassaggioScena>();
        if (passaggio != null) passaggio.enabled = true;
        Vai("Ingresso della cripta");
    }

    // Quest 6: apre l'ingresso della taverna (come dopo la quest 5) e porta lì davanti.
    [MenuItem(Cartella + "Vai alla taverna (ingresso aperto)")]
    static void Taverna()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        var ingresso = TrovaOggetto("Ingresso della taverna");
        if (ingresso == null) { Debug.LogWarning("Prova prologo: manca l'ingresso della taverna (prepara la quest 6 e la razzia)."); return; }
        var passaggio = ingresso.GetComponent<PassaggioScena>();
        if (passaggio != null) passaggio.enabled = true;
        Vai("Ingresso della taverna");
    }

    // Razzia: parte subito (notte di colpo), senza fare le quest.
    [MenuItem(Cartella + "Razzia: fai partire subito")]
    static void RazziaSubito()
    {
        if (!Application.isPlaying || RazziaVillaggio.Istanza == null) { Debug.LogWarning("Prova prologo: serve il Play nel villaggio con la razzia preparata."); return; }
        if (OrarioPrologo.Istanza != null && !OrarioPrologo.Istanza.TempoLibero) OrarioPrologo.Istanza.NotteSubito();
        else RazziaVillaggio.Istanza.Inizia();
    }

    [MenuItem(Cartella + "Razzia: abbatti l'ondata in corso (ne restano 2)")]
    static void RazziaAbbatti()
    {
        if (Application.isPlaying && RazziaVillaggio.Istanza != null) RazziaVillaggio.Istanza.ProvaAbbatti();
    }

    [MenuItem(Cartella + "Razzia: salta il minuto di tamburi")]
    static void RazziaTamburi()
    {
        if (Application.isPlaying && RazziaVillaggio.Istanza != null) RazziaVillaggio.Istanza.ProvaSaltaTamburi();
    }

    // Dentro la cripta: porta davanti alla leva, al baule o alla scala dell'uscita.
    [MenuItem(Cartella + "Cripta: vai alla leva")] static void CriptaLeva() => Vai("Leva del cancello");
    [MenuItem(Cartella + "Cripta: vai al baule")] static void CriptaBaule() => Vai("Baule della prima guardia");
    [MenuItem(Cartella + "Cripta: vai all'uscita")] static void CriptaUscita() => Vai("Scala dell'uscita");
    [MenuItem(Cartella + "Cripta: vai al tunnel delle tracce")] static void CriptaTracce() => Vai("Nel tunnel delle tracce");

    // Toglie 20 di vita a ogni scheletro vivo (per provare in fretta la cripta).
    [MenuItem(Cartella + "Ferisci gli scheletri della cripta (-20)")]
    static void FerisciScheletri()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        int quanti = 0;
        foreach (var s in Object.FindObjectsByType<ScheletroVivente>(FindObjectsSortMode.None))
        {
            var b = s.GetComponent<Bersaglio>();
            if (b == null || b.Morto) continue;
            b.RiceviColpo(20f, b.transform.position + Vector3.up);
            quanti++;
        }
        Debug.Log("Prova prologo: feriti " + quanti + " scheletri.");
    }

    [MenuItem(Cartella + "Apri la bottega di Ilse")]
    static void Bottega()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        var negozio = Object.FindFirstObjectByType<Negozio>(FindObjectsInactive.Include);
        if (negozio == null) { Debug.LogWarning("Prova prologo: nessun Negozio nella scena (prepara la quest 4)."); return; }
        Vai("Ilse");
        negozio.enabled = true;
        negozio.Apri();
    }

    [MenuItem(Cartella + "Dai 100 monete")]
    static void Monete()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        var g = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (g == null) return;
        Borsa.Di(g).Aggiungi(100);
        Debug.Log("Prova prologo: monete nella borsa " + Borsa.Di(g).Quante + ".");
    }

    [MenuItem(Cartella + "Dai 3 fiori di ogni tipo")]
    static void Fiori()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        var g = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (g == null) return;
        var zaino = Zaino.Di(g);
        foreach (string file in new[] { "fiore-di-riva", "cardo-nero", "lunaria" })
        {
            var fiore = AssetDatabase.LoadAssetAtPath<DatiOggetto>("Assets/Dati/Oggetti/Missioni/" + file + ".asset");
            for (int i = 0; i < 3 && fiore != null; i++) zaino.Aggiungi(fiore);
        }
        Debug.Log("Prova prologo: 3 fiori di riva, 3 cardi neri e 3 lunarie nello zaino.");
    }

    static GameObject TrovaOggetto(string nome)
    {
        foreach (var radice in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var t in radice.GetComponentsInChildren<Transform>(true))
                if (t.name == nome) return t.gameObject;
        return null;
    }

    // Toglie 50 di vita al giocatore (senza scendere sotto 1): serve a provare la Pozione di Ilse (tasto F).
    [MenuItem(Cartella + "Togli 50 di vita al giocatore")]
    static void TogliVita()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        var g = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (g == null) return;
        float danno = Mathf.Min(50f, g.Vita - 1f);
        if (danno > 0f) g.RiceviColpo(danno, g.transform.position - g.transform.forward);
        Debug.Log("Prova prologo: vita del giocatore " + Mathf.CeilToInt(g.Vita) + ".");
    }

    // Toglie 20 di vita a ogni ratto del granaio ancora vivo (due volte = tutti morti): serve a provare il contatore
    // "Ratti rimasti" e la fine della quest 3 senza dover vincere lo scontro.
    [MenuItem(Cartella + "Ferisci i ratti del granaio (-20)")]
    static void FerisciRatti()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        int quanti = 0;
        foreach (var ratto in Object.FindObjectsByType<RattoGigante>(FindObjectsSortMode.None))
        {
            var b = ratto.GetComponent<Bersaglio>();
            if (b == null || b.Morto) continue;
            b.RiceviColpo(20f, b.transform.position + Vector3.up);
            quanti++;
        }
        Debug.Log(quanti > 0 ? "Prova prologo: feriti " + quanti + " ratti." : "Prova prologo: nessun ratto vivo (parla prima con Hobb).");
    }

    [MenuItem(Cartella + "Salta alla quest 2")] static void Salta2() => SaltaAllaQuest(2);
    [MenuItem(Cartella + "Salta alla quest 3")] static void Salta3() => SaltaAllaQuest(3);
    [MenuItem(Cartella + "Salta alla quest 4")] static void Salta4() => SaltaAllaQuest(4);
    [MenuItem(Cartella + "Salta alla quest 5")] static void Salta5() => SaltaAllaQuest(5);
    [MenuItem(Cartella + "Salta alla quest 6")] static void Salta6() => SaltaAllaQuest(6);

    static void SaltaAllaQuest(int quest)
    {
        if (!Application.isPlaying) { Debug.LogWarning("Prova prologo: funziona solo durante il Play."); return; }
        var giocatore = Object.FindFirstObjectByType<GiocatoreControllo>();
        for (int q = 1; q < quest; q++)
        {
            var fini = FiniDellaQuest(q);
            if (fini.Count == 0) { Debug.LogWarning("Prova prologo: non trovo la fine della quest " + q + "."); return; }
            // quest 2: due fini (Guerriero e Ladro dal fabbro, Stregone da Edric): si usa quella della classe
            UnityEngine.Events.UnityEvent scelta = fini[0].evento;
            foreach (var f in fini)
                if ((SceltaPartita.Classe == ClasseGiocatore.Stregone) == f.nome.Contains("Edric")) scelta = f.evento;
            scelta.Invoke();
        }
        var missioni = Missioni.Istanza;
        if (missioni != null && missioni.Meta != null && giocatore != null)
        {
            Vector3 punto = missioni.Meta.position + Vector3.back * 1.8f + Vector3.up * 1.2f;
            giocatore.Teletrasporta(punto, Quaternion.LookRotation(Vector3.forward));
        }
        Debug.Log("Prova prologo: quest 1-" + (quest - 1) + " finite. Obiettivo: " + (missioni != null ? missioni.Obiettivo : "?"));
    }

    struct Fine { public string nome; public UnityEngine.Events.UnityEvent evento; }

    // Gli eventi che chiamano Missioni > QuestFinita con quel numero.
    static System.Collections.Generic.List<Fine> FiniDellaQuest(int quest)
    {
        var elenco = new System.Collections.Generic.List<Fine>();
        var scena = SceneManager.GetActiveScene();
        foreach (var radice in scena.GetRootGameObjects())
            foreach (var c in radice.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (c == null) continue;
                foreach (var f in c.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                {
                    if (f.FieldType != typeof(UnityEngine.Events.UnityEvent) || !(f.GetValue(c) is UnityEngine.Events.UnityEvent e)) continue;
                    var chiamate = new SerializedObject(c).FindProperty(f.Name + ".m_PersistentCalls.m_Calls");
                    if (chiamate == null) continue;
                    for (int i = 0; i < chiamate.arraySize; i++)
                    {
                        var ch = chiamate.GetArrayElementAtIndex(i);
                        if (ch.FindPropertyRelative("m_MethodName").stringValue != "QuestFinita") continue;
                        if (ch.FindPropertyRelative("m_Arguments.m_IntArgument").intValue != quest) continue;
                        elenco.Add(new Fine { nome = c.gameObject.name, evento = e });
                        break;
                    }
                }
            }
        return elenco;
    }

    static void Vai(string nome)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Prova prologo: funziona solo durante il Play.");
            return;
        }
        GameObject meta = null;
        for (int k = 0; k < SceneManager.sceneCount && meta == null; k++)   // anche nella cripta, se è caricata
            foreach (var radice in SceneManager.GetSceneAt(k).GetRootGameObjects())
                foreach (var t in radice.GetComponentsInChildren<Transform>(true))
                    if (t.name == nome) { meta = t.gameObject; break; }
        var giocatore = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (meta == null || giocatore == null)
        {
            Debug.LogWarning("Prova prologo: non trovo " + (meta == null ? "\"" + nome + "\"" : "il giocatore") + ".");
            return;
        }
        // Un passo e mezzo davanti al punto (dove guarda), girati verso di lui.
        Vector3 davanti = meta.transform.forward;
        davanti.y = 0f;
        if (davanti.sqrMagnitude < 0.01f) davanti = Vector3.forward;
        Vector3 punto = meta.transform.position + davanti.normalized * 1.6f;
        punto.y = meta.transform.position.y + 1.2f;
        var cc = giocatore.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        giocatore.transform.SetPositionAndRotation(punto, Quaternion.LookRotation(-davanti.normalized));
        if (cc != null) cc.enabled = true;
        Debug.Log("Prova prologo: giocatore portato da " + nome + ".");
    }
}
