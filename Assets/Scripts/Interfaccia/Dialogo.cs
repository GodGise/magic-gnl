using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

// Una riga di dialogo: chi parla e cosa dice. Sono due chiavi di Lingua (non testi scritti qui), così le 8 lingue si
// cambiano al volo. "parlante" vuoto = narrazione o suggerimento a schermo (senza nome, in colore bronzo).
[Serializable]
public class RigaDialogo
{
    [Tooltip("Chiave del nome di chi parla (per esempio pg.maren). Vuoto = nessun nome.")]
    public string parlante;
    [Tooltip("Chiave del testo (per esempio dlg.q1.01). Può contenere {Interagisci}, {Attacca}... al posto dei tasti.")]
    public string testo;
    [Tooltip("Solo per questa classe. Tutte = la riga la sentono tutti.")]
    public ClasseRiga soloClasse = ClasseRiga.Tutte;
}

public enum ClasseRiga { Tutte, Guerriero, Ladro, Stregone }

// Il riquadro dei dialoghi, in basso nello schermo.
// A cosa serve: mostra le righe una alla volta con il nome di chi parla; il testo compare lettera per lettera.
// Il tasto "Interagisci" (scelto in Opzioni > Comandi; A sul pad) completa la riga e poi passa alla successiva.
// In co-op il tempo non si ferma: ogni giocatore vede il proprio dialogo e, finché parla, il suo personaggio sta fermo.
// Come si usa dal codice: Dialogo.Avvia(righe, () => ...); di solito lo fa il componente Interlocutore.
// Come montarlo: non si monta, si crea da solo all'avvio del gioco.
public class Dialogo : MonoBehaviour
{
    // C'è un dialogo in corso: il giocatore non si muove e non attacca (lo controllano GiocatoreControllo e gli altri).
    public static bool InCorso { get; private set; }
    // Quando è finito l'ultimo dialogo: serve a chi apre i dialoghi, per non ricominciarlo con lo stesso tasto che lo ha chiuso.
    public static float UltimaChiusura { get; private set; } = -10f;
    public static bool ApenaFinito => Time.unscaledTime - UltimaChiusura < 0.3f;

    const float LettereAlSecondo = 55f;
    const float AttesaPrimaDelTasto = 0.25f;   // per non saltare la prima riga con lo stesso tasto che ha aperto il dialogo

    static Dialogo istanza;
    RigaDialogo[] righe;
    int indice;
    float inizio;
    Action alTermine;
    string testoCompleto = "";
    InputAction comandoAvanti;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreaAllAvvio() => Istanza();

    static Dialogo Istanza()
    {
        if (istanza != null) return istanza;
        var oggetto = new GameObject("Dialoghi");
        DontDestroyOnLoad(oggetto);
        istanza = oggetto.AddComponent<Dialogo>();
        return istanza;
    }

    // Mostra le righe (quelle che la classe del giocatore può sentire) e poi chiama alTermine.
    public static void Avvia(RigaDialogo[] righe, Action alTermine = null)
    {
        var utili = new System.Collections.Generic.List<RigaDialogo>();
        if (righe != null)
            foreach (var r in righe)
                if (r != null && !string.IsNullOrEmpty(r.testo) && (r.soloClasse == ClasseRiga.Tutte || (int)r.soloClasse - 1 == (int)SceltaPartita.Classe))
                    utili.Add(r);
        if (utili.Count == 0) { alTermine?.Invoke(); return; }
        var d = Istanza();
        d.righe = utili.ToArray();
        d.indice = 0;
        d.alTermine = alTermine;
        d.Mostra();
        InCorso = true;
    }

    // Una riga sola, per i suggerimenti del tutorial (di solito si usano MessaggiSchermo con Dialogo.Testo).
    public static string Testo(string chiave) => SostituisciTasti(Lingua.T(chiave));

    // "Premi {Interagisci}" diventa "Premi E" (o il tasto scelto dal giocatore). Un nome sbagliato resta com'è.
    public static string SostituisciTasti(string testo)
    {
        if (string.IsNullOrEmpty(testo) || testo.IndexOf('{') < 0) return testo;
        foreach (Azione a in Enum.GetValues(typeof(Azione)))
        {
            string segnaposto = "{" + a + "}";
            if (testo.Contains(segnaposto)) testo = testo.Replace(segnaposto, Comandi.NomeTasto(a));
        }
        return testo;
    }

    void Awake()
    {
        comandoAvanti = new InputAction("AvantiDialogo", InputActionType.Button);
        comandoAvanti.AddBinding("<Keyboard>/e");
        comandoAvanti.AddBinding("<Gamepad>/buttonSouth");
        Comandi.Collega(comandoAvanti, Azione.Interagisci, this);
        Lingua.Cambiata += Mostra;
    }

    void OnEnable() => comandoAvanti.Enable();
    void OnDisable() => comandoAvanti.Disable();
    void OnDestroy()
    {
        Lingua.Cambiata -= Mostra;
        comandoAvanti.Dispose();
        if (istanza == this) { istanza = null; InCorso = false; }
    }

    void Mostra()
    {
        if (righe == null || indice >= righe.Length) return;
        testoCompleto = SostituisciTasti(Lingua.T(righe[indice].testo));
        inizio = Time.unscaledTime;
    }

    int LettereVisibili => Mathf.Min(testoCompleto.Length, Mathf.FloorToInt((Time.unscaledTime - inizio) * LettereAlSecondo));

    void Update()
    {
        if (!InCorso) return;
        if (MenuPausa.InPausa || InventarioGioco.Aperto) return;
        if (Time.unscaledTime - inizio < AttesaPrimaDelTasto && indice == 0) return;
        if (!comandoAvanti.WasPressedThisFrame()) return;

        if (LettereVisibili < testoCompleto.Length) { inizio = -1000f; return; }   // prima completa la riga
        indice++;
        if (indice >= righe.Length) { Chiudi(); return; }
        Mostra();
    }

    void Chiudi()
    {
        InCorso = false;
        UltimaChiusura = Time.unscaledTime;
        var fine = alTermine;
        righe = null;
        alTermine = null;
        testoCompleto = "";
        fine?.Invoke();
    }

    // ---------- disegno ----------

    void OnGUI()
    {
        if (!InCorso || righe == null || indice >= righe.Length) return;
        if (MenuPausa.InPausa || InventarioGioco.Aperto) return;
        GUI.depth = 4;
        GraficaMenu.PreparaStili();
        float larghezza = GraficaMenu.FoglioIntero();

        float w = Mathf.Min(1200f, larghezza - 120f);
        var riquadro = new Rect(larghezza * 0.5f - w * 0.5f, 770f, w, 250f);
        GraficaMenu.Cornice(riquadro, 0.96f, false);

        string nome = string.IsNullOrEmpty(righe[indice].parlante) ? "" : Lingua.T(righe[indice].parlante);
        float y = riquadro.y + 20f;
        if (nome.Length > 0)
        {
            var stileNome = new GUIStyle(GraficaMenu.TestoSinistra) { fontSize = Mathf.RoundToInt(GraficaMenu.TestoSinistra.fontSize * 1.1f), fontStyle = FontStyle.Bold };
            GraficaMenu.Scritta(new Rect(riquadro.x + 40f, y, w - 80f, 40f), nome, stileNome, GraficaMenu.Selezione, 1f);
            y += 46f;
        }

        var stile = new GUIStyle(GraficaMenu.TestoSinistra) { wordWrap = false, alignment = TextAnchor.UpperLeft };
        bool narrazione = nome.Length == 0;
        string a_capo = AndareACapo(testoCompleto, stile, w - 80f, LettereVisibili);
        GraficaMenu.Scritta(new Rect(riquadro.x + 40f, y, w - 80f, riquadro.yMax - y - 40f), a_capo, stile, narrazione ? GraficaMenu.Bronzo : GraficaMenu.Testo, 1f);

        if (LettereVisibili >= testoCompleto.Length)
        {
            var piccolo = new GUIStyle(GraficaMenu.Piccolo) { alignment = TextAnchor.MiddleRight };
            string tasto = Comandi.NomeTasto(Azione.Interagisci);
            GraficaMenu.Scritta(new Rect(riquadro.x + 40f, riquadro.yMax - 40f, w - 80f, 30f), tasto + "  " + Lingua.T("dlg.continua"), piccolo, GraficaMenu.Selezione, 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2.5f)));
        }
    }

    // Va a capo da sé, per parole (per lettere in cinese, che non usa spazi), misurando il testo con lo stile.
    // Il risultato tiene per ogni lettera il suo posto nel testo originale: così si scrive lettera per lettera
    // senza che le parole saltino da una riga all'altra mentre il testo cresce.
    string accapoPer, accapoRisultato;
    int[] accapoPosti;
    float accapoLarghezza;
    int accapoLingua = -1;

    string AndareACapo(string testo, GUIStyle stile, float larghezza, int lettereVisibili)
    {
        if (accapoPer != testo || !Mathf.Approximately(accapoLarghezza, larghezza) || accapoLingua != Lingua.Indice)
        {
            var uscita = new StringBuilder();
            var posti = new System.Collections.Generic.List<int>();
            var riga = new StringBuilder();
            var postiRiga = new System.Collections.Generic.List<int>();
            bool perLettere = Lingua.Indice == 7;
            int p = 0;
            while (p < testo.Length)
            {
                int f = p + 1;
                if (!perLettere) { while (f < testo.Length && testo[f] != ' ') f++; if (f < testo.Length) f++; }   // la parola con lo spazio dopo
                string pezzo = testo.Substring(p, f - p);
                if (riga.Length > 0 && stile.CalcSize(new GUIContent((riga.ToString() + pezzo).TrimEnd())).x > larghezza)
                {
                    Scarica(uscita, posti, riga, postiRiga, true);
                }
                for (int k = 0; k < pezzo.Length; k++) { riga.Append(pezzo[k]); postiRiga.Add(p + k); }
                p = f;
            }
            Scarica(uscita, posti, riga, postiRiga, false);
            accapoPer = testo; accapoLarghezza = larghezza; accapoLingua = Lingua.Indice;
            accapoRisultato = uscita.ToString(); accapoPosti = posti.ToArray();
        }
        var visibile = new StringBuilder();
        for (int i = 0; i < accapoRisultato.Length; i++)
            if (accapoPosti[i] < lettereVisibili) visibile.Append(accapoRisultato[i]);
        return visibile.ToString();
    }

    // Chiude la riga (senza lo spazio finale) e, se serve, va a capo.
    static void Scarica(StringBuilder uscita, System.Collections.Generic.List<int> posti, StringBuilder riga, System.Collections.Generic.List<int> postiRiga, bool vaACapo)
    {
        int fine = riga.Length;
        while (fine > 0 && riga[fine - 1] == ' ') fine--;
        for (int i = 0; i < fine; i++) { uscita.Append(riga[i]); posti.Add(postiRiga[i]); }
        if (vaACapo) { uscita.Append('\n'); posti.Add(postiRiga.Count > 0 ? postiRiga[postiRiga.Count - 1] : 0); }
        riga.Length = 0;
        postiRiga.Clear();
    }
}
