using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// L'apertura con il logo dello studio: "GNL STUDIO presenta".
// A cosa serve: all'avvio del gioco, prima della schermata del titolo, su fondo nero i pezzi del sigillo (quattro archi
// dell'anello, il rombo e la spada) arrivano da fuori e si compongono; un lampo e una scintillio chiudono la composizione,
// poi compaiono "GNL STUDIO" (le lettere si stringono) e sotto, piccolo, "presenta"; infine tutto si spegne e parte il menu.
// Non si salta (il logo dello studio è sacro): solo nell'editor di Unity, per chi lo prova spesso, F2 lo salta.
// Si vede una volta sola a ogni avvio del gioco.
// Suono: se nella cartella Assets/Resources/Audio/Effetti c'è un file "logo-studio" (ogg o wav) parte da solo quando
// inizia l'animazione, cioè dopo 1,5 secondi di nero.
// I momenti dell'animazione (per far combaciare il suono) sono in Docs/logo-studio.md e nelle costanti qui sotto.
// I pezzi del logo sono in Assets/Resources/Logo (si rifanno con Docs/logo-studio/genera_logo.py).
// Come montarlo: non si monta, si crea da solo. Nella scena "Menu" ferma MenuPrincipale finché il logo non è finito.
public class LogoStudio : MonoBehaviour
{
    // Prima schermo nero e silenzio per 1,5 secondi; poi parte l'animazione (e il suono).
    const float AttesaNera = 1.5f;

    // ---------- momenti (secondi dall'inizio dell'animazione, cioè dopo il nero iniziale) ----------
    const float InizioAnelli = 0.0f, DurataAnelli = 2.0f;     // i quattro archi arrivano da fuori, uno dopo l'altro
    const float InizioRombo = 1.5f, DurataRombo = 1.3f;       // il rombo si apre e si gira in posizione
    const float InizioSpada = 2.4f, DurataSpada = 0.9f;       // la spada cala dall'alto
    const float MomentoLampo = 3.3f;                          // la spada tocca: lampo, onda e scintille
    const float InizioNome = 3.7f, DurataNome = 1.9f;         // "GNL STUDIO"
    const float InizioPresenta = 5.3f, DurataPresenta = 1.0f; // "presenta"
    const float InizioSpegni = 7.3f, DurataSpegni = 1.0f;     // dissolvenza al nero
    const float AttesaFinale = 0.35f;                         // nero prima del menu

    // Una volta per avvio del gioco (nell'editor con "Reload Domain" spento si azzera da solo).
    static bool giaMostrato;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Azzera() => giaMostrato = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreaAllAvvio()
    {
        if (giaMostrato) return;
        var menu = FindFirstObjectByType<MenuPrincipale>();
        if (menu == null) return;   // si parte da un'altra scena (prove nell'editor): niente logo
        giaMostrato = true;
        // Il menu parte (Start, musica, sequenza del titolo) solo a logo finito.
        menu.enabled = false;
        var oggetto = new GameObject("Logo dello studio");
        oggetto.AddComponent<LogoStudio>().menu = menu;
    }

    MenuPrincipale menu;
    Texture2D[] anelli = new Texture2D[4];
    Texture2D rombo, spada;
    AudioSource sorgente;
    float inizio = float.MaxValue;   // quando parte l'animazione (dopo il nero iniziale)
    bool suonoPartito;
    float saltatoA = -1f;       // solo nell'editor: quando si è premuto F2
    bool finito;
    Matrix4x4 base_;
    readonly List<Scintilla> scintille = new List<Scintilla>();

    struct Scintilla { public Vector2 direzione; public float velocita, vita, taglia; }

    void Start()
    {
        for (int i = 0; i < 4; i++) anelli[i] = Resources.Load<Texture2D>("Logo/anello-" + (i + 1));
        rombo = Resources.Load<Texture2D>("Logo/rombo");
        spada = Resources.Load<Texture2D>("Logo/spada");
        Cursor.visible = false;

        Impostazioni.Applica();
        var suono = Resources.Load<AudioClip>("Audio/Effetti/logo-studio");
        sorgente = gameObject.AddComponent<AudioSource>();
        sorgente.playOnAwake = false;
        sorgente.spatialBlend = 0f;
        sorgente.volume = Impostazioni.VolumeEffetti;
        if (suono != null) { sorgente.clip = suono; }

        var caso = new System.Random(7);
        for (int i = 0; i < 46; i++)
        {
            float a = (float)(caso.NextDouble() * Mathf.PI * 2f);
            scintille.Add(new Scintilla
            {
                direzione = new Vector2(Mathf.Cos(a), Mathf.Sin(a)),
                velocita = 140f + (float)caso.NextDouble() * 520f,
                vita = 0.9f + (float)caso.NextDouble() * 1.2f,
                taglia = 2f + (float)caso.NextDouble() * 4f,
            });
        }
        inizio = Time.unscaledTime + AttesaNera;
    }

    // Secondi dall'inizio dell'animazione: negativo durante il nero iniziale.
    float Tempo => inizio == float.MaxValue ? -AttesaNera : Time.unscaledTime - inizio;

    // Solo nell'editor, con F2: la dissolvenza al nero parte subito (più veloce).
    float TempoSpegni => saltatoA >= 0f ? Mathf.Min(InizioSpegni, saltatoA - inizio) : InizioSpegni;
    float DurataSpegniAttuale => saltatoA >= 0f ? 0.6f : DurataSpegni;

    void Update()
    {
        if (finito) return;
        float t = Tempo;
        if (!suonoPartito && t >= 0f)
        {
            suonoPartito = true;
            if (sorgente != null && sorgente.clip != null) sorgente.Play();
        }
        if (Application.isEditor && saltatoA < 0f && t < InizioSpegni && Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame)
            saltatoA = Time.unscaledTime;
        float fine = (saltatoA >= 0f ? saltatoA - inizio + 0.6f : InizioSpegni + DurataSpegni) + AttesaFinale;
        if (saltatoA >= 0f && sorgente != null && sorgente.isPlaying) sorgente.volume = Mathf.MoveTowards(sorgente.volume, 0f, Time.unscaledDeltaTime / 0.5f);
        if (t >= fine) Concludi();
    }

    void Concludi()
    {
        finito = true;
        Cursor.visible = true;
        if (menu != null) menu.enabled = true;   // da qui parte il menu (Start, musica, titolo)
        Destroy(gameObject);
    }

    static float Fluido(float x) => Mathf.Clamp01(x);
    static float Uscita(float x) { x = Mathf.Clamp01(x); return 1f - Mathf.Pow(1f - x, 4f); }       // frena alla fine
    static float UscitaForte(float x) { x = Mathf.Clamp01(x); return 1f - Mathf.Pow(1f - x, 5f); }
    static float Morbido(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

    // ---------- disegno ----------

    void OnGUI()
    {
        if (finito) return;
        GUI.depth = -1000;   // sopra a tutto il resto
        GraficaMenu.PreparaStili();
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GraficaMenu.Pixel);
        GUI.color = Color.white;
        GraficaMenu.FoglioVirtuale();
        base_ = GUI.matrix;

        float t = Tempo;
        float tSpegni = TempoSpegni, dSpegni = DurataSpegniAttuale;
        float luce = 1f - Morbido(Mathf.Clamp01((t - tSpegni) / dSpegni));   // 1 = tutto acceso, 0 = nero
        if (luce <= 0f) return;

        const float lato = 500f;
        var centro = new Vector2(GraficaMenu.Larghezza * 0.5f, 360f);
        var r = new Rect(centro.x - lato * 0.5f, centro.y - lato * 0.5f, lato, lato);

        // bagliore caldo dietro il sigillo: cresce mentre si compone, respira alla fine
        float cresce = Morbido((t - InizioAnelli) / (MomentoLampo - InizioAnelli));
        float respiro = t > MomentoLampo + 1f ? 0.85f + 0.15f * Mathf.Sin(t * 1.8f) : 1f;
        float lampo = Mathf.Exp(-Mathf.Max(0f, t - MomentoLampo) * 3.2f) * (t >= MomentoLampo ? 1f : 0f);
        Bagliore(centro, 980f + 260f * lampo, new Color(1f, 0.5f, 0.18f), (0.10f * cresce * respiro + 0.55f * lampo) * luce);

        // colore del sigillo: spento mentre arriva, si accende al lampo e si assesta su un bronzo caldo
        Color spento = new Color(0.50f, 0.40f, 0.26f), caldo = new Color(1f, 0.86f, 0.62f), riposo = new Color(0.84f, 0.69f, 0.44f);
        Color colore = t < MomentoLampo ? Color.Lerp(spento, riposo, Morbido((t - InizioAnelli) / (MomentoLampo - InizioAnelli)))
                                        : Color.Lerp(caldo, riposo, Morbido((t - MomentoLampo) / 1.6f));

        // i quattro archi: ognuno arriva da fuori, ruotando, in un momento suo
        int[] ordine = { 0, 2, 1, 3 };
        for (int i = 0; i < 4; i++)
        {
            int k = ordine[i];
            float p = Uscita((t - (InizioAnelli + i * 0.18f)) / DurataAnelli);
            if (p <= 0f) continue;
            float angolo = Mathf.Deg2Rad * (k * 90f - 90f + 45f);
            var direzione = new Vector2(Mathf.Cos(angolo), Mathf.Sin(angolo));
            float lontano = 720f * (1f - p);
            Strato(anelli[k], r, direzione * lontano, (k % 2 == 0 ? -1f : 1f) * 55f * (1f - p), 1f + 0.35f * (1f - p), Con(colore, Mathf.Clamp01(p * 2.2f) * luce));
        }

        // il rombo: si apre dal centro girando
        float pr = UscitaForte((t - InizioRombo) / DurataRombo);
        if (pr > 0f) Strato(rombo, r, Vector2.zero, -90f * (1f - pr), 0.45f + 0.55f * pr, Con(colore, Mathf.Clamp01(pr * 1.6f) * luce));

        // la spada: cala dall'alto e si ferma al suo posto
        float ps = Uscita((t - InizioSpada) / DurataSpada);
        if (ps > 0f) Strato(spada, r, new Vector2(0f, -380f * (1f - ps)), 0f, 1f, Con(colore, Mathf.Clamp01(ps * 3f) * luce));

        // al lampo: l'anello si allarga come un'onda, poi le scintille
        if (t >= MomentoLampo)
        {
            float dt = t - MomentoLampo;
            float onda = Uscita(dt / 1.1f);
            float alfaOnda = (1f - onda) * 0.8f * luce;
            for (int k = 0; k < 4; k++)
                Strato(anelli[k], r, Vector2.zero, 0f, 1f + 0.55f * onda, Con(caldo, alfaOnda));
            DisegnaScintille(centro, dt, luce);
        }

        DisegnaNome(t, luce);
    }

    // Lettere con la spaziatura che si stringe: partono larghe e quasi invisibili, si raccolgono nel nome.
    void DisegnaNome(float t, float luce)
    {
        float p = Morbido((t - InizioNome) / DurataNome);
        if (p > 0f)
        {
            string[] parti = { "GNL", "STUDIO" };
            int[] corpo = { 92, 92 };
            Color[] colori = { new Color(0.93f, 0.88f, 0.78f), new Color(0.62f, 0.50f, 0.32f) };
            var stile = new GUIStyle(GraficaMenu.Titolo);
            // la spaziatura passa da molto larga (2,2 volte il corpo) a quella del titolo del gioco (0,16)
            float spazio = Mathf.Lerp(2.2f, 0.16f, Uscita((t - InizioNome) / DurataNome)) * 92f;
            float spazioParole = 92f * 0.7f;
            // misure di ogni lettera
            var lettere = new List<(string c, float w, int parte)>();
            float totale = 0f;
            for (int i = 0; i < parti.Length; i++)
            {
                stile.fontSize = corpo[i];
                foreach (char c in parti[i])
                {
                    float w = stile.CalcSize(new GUIContent(c.ToString())).x;
                    lettere.Add((c.ToString(), w, i));
                    totale += w + spazio;
                }
                if (i == 0) totale += spazioParole;
            }
            totale -= spazio;
            float x = GraficaMenu.Larghezza * 0.5f - totale * 0.5f;
            int indice = 0;
            for (int i = 0; i < lettere.Count; i++)
            {
                var (c, w, parte) = lettere[i];
                stile.fontSize = corpo[parte];
                float ritardo = indice * 0.07f;   // le lettere si accendono una dopo l'altra
                float a = Morbido((t - InizioNome - ritardo) / 0.9f) * luce;
                GraficaMenu.Scritta(new Rect(x, 620f, w, 120f), c, stile, colori[parte], a);
                x += w + spazio;
                if (i + 1 < lettere.Count && lettere[i + 1].parte != parte) x += spazioParole;
                indice++;
            }
            // filo di luce sotto il nome, si allunga con le lettere
            float lunghezza = Mathf.Lerp(0f, 520f, Uscita((t - InizioNome - 0.3f) / (DurataNome * 0.9f)));
            GraficaMenu.Riempi(new Rect(GraficaMenu.Larghezza * 0.5f - lunghezza * 0.5f, 752f, lunghezza, 2f), GraficaMenu.Con(GraficaMenu.Bronzo, 0.9f * luce));
            GraficaMenu.Rombo(new Vector2(GraficaMenu.Larghezza * 0.5f, 753f), 11f * Mathf.Clamp01(lunghezza / 520f), GraficaMenu.Con(GraficaMenu.Selezione, luce));
        }

        float pp = Morbido((t - InizioPresenta) / DurataPresenta);
        if (pp > 0f)
        {
            string testo = Lingua.T("logo.presenta");
            if (Lingua.Indice != 7) testo = GraficaMenu.Spaziato(testo);
            var stile = new GUIStyle(GraficaMenu.Voce) { fontSize = 28 };
            GraficaMenu.Scritta(new Rect(0f, 780f, GraficaMenu.Larghezza, 50f), testo, stile, new Color(0.62f, 0.58f, 0.5f), pp * luce);
        }
    }

    void DisegnaScintille(Vector2 centro, float dt, float luce)
    {
        foreach (var s in scintille)
        {
            if (dt > s.vita) continue;
            float k = dt / s.vita;
            // schizzano fuori e rallentano; salgono piano come braci
            float distanza = s.velocita * (1f - Mathf.Pow(1f - Mathf.Clamp01(dt / 0.9f), 2f)) * 0.9f;
            Vector2 pos = centro + s.direzione * (200f + distanza) + new Vector2(0f, -40f * k * k - 30f * dt);
            float alfa = (1f - k) * (1f - k) * luce;
            GraficaMenu.Riempi(new Rect(pos.x - s.taglia * 0.5f, pos.y - s.taglia * 0.5f, s.taglia, s.taglia), new Color(1f, 0.72f, 0.34f, alfa));
        }
    }

    // ---------- piccole utilità di disegno ----------

    static Color Con(Color c, float alfa) => new Color(c.r, c.g, c.b, alfa);

    // Disegna un pezzo del logo spostato, ruotato e ingrandito intorno al centro del rettangolo.
    void Strato(Texture2D tex, Rect r, Vector2 spostamento, float gradi, float scala, Color colore)
    {
        if (tex == null || colore.a <= 0.002f) return;
        Vector2 c = r.center;
        GUI.matrix = base_
            * Matrix4x4.TRS(new Vector3(c.x + spostamento.x, c.y + spostamento.y, 0f), Quaternion.Euler(0f, 0f, gradi), new Vector3(scala, scala, 1f))
            * Matrix4x4.TRS(new Vector3(-c.x, -c.y, 0f), Quaternion.identity, Vector3.one);
        GUI.color = colore;
        GUI.DrawTexture(r, tex, ScaleMode.StretchToFill, true);
        GUI.matrix = base_;
        GUI.color = Color.white;
    }

    void Bagliore(Vector2 centro, float diametro, Color colore, float alfa)
    {
        if (alfa <= 0.002f) return;
        GUI.color = Con(colore, Mathf.Clamp01(alfa));
        GUI.DrawTexture(new Rect(centro.x - diametro * 0.5f, centro.y - diametro * 0.5f, diametro, diametro), GraficaMenu.Bagliore);
        GUI.color = Color.white;
    }
}
