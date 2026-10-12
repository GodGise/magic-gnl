using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// L'apertura con il logo dello studio: "GNL STUDIO presenta".
// A cosa serve: all'avvio del gioco, prima della schermata del titolo, su fondo nero (3 secondi) il sigillo si compone GRANDE
// AL CENTRO dello schermo: il filo esterno, i quattro archi dell'anello, le rune che si accendono una dopo l'altra, il rombo;
// poi una scintilla cala dall'alto e cade nel lago: la runa centrale si accende (lampo, onde, scintille, braci). Il sigillo sale e rimpicciolisce e al centro
// compaiono "GNL STUDIO" (le lettere si stringono) e sotto, piccolo, "presenta"; infine tutto si spegne e parte il menu.
// Non si salta (il logo dello studio è sacro): solo nell'editor di Unity, per chi lo prova spesso, F2 lo salta.
// Si vede una volta sola a ogni avvio del gioco.
// Suono: se nella cartella Assets/Resources/Audio/Effetti c'è un file "logo-studio" (ogg o wav) parte da solo quando
// inizia l'animazione, cioè dopo 3 secondi di nero.
// I momenti dell'animazione (per far combaciare il suono) sono in Docs/logo-studio.md e nelle costanti qui sotto.
// I pezzi del logo sono in Assets/Resources/Logo (si rifanno con Docs/logo-studio/genera_logo.py).
// Come montarlo: non si monta, si crea da solo. Nella scena "Menu" ferma MenuPrincipale finché il logo non è finito.
public class LogoStudio : MonoBehaviour
{
    // Prima schermo nero e silenzio per 3 secondi; poi parte l'animazione (e il suono).
    const float AttesaNera = 3.0f;

    // Il suono si aggancia da solo: il gioco cerca nel file audio il COLPO (il punto più forte) e fa partire il file in modo che
    // il colpo cada proprio quando la runa centrale si accende (MomentoLampo). Il file può avere qualsiasi silenzio all'inizio.
    // Per ritocchi fini: secondi di correzione, positivo = suono più tardi, negativo = più presto.
    // Nell'editor di Unity si regola anche con i tasti (vedi sotto); quel valore si somma a questo.
    const float CorrezioneSuono = -1.45f;   // regolata a orecchio da Giuseppe nell'editor (13 ottobre)
    const string ChiaveCorrezione = "LogoStudio.CorrezioneSuono";

    // Correzione totale: la costante più, solo nell'editor, la regolazione fatta con F3/F4 (salvata tra una prova e l'altra).
    static float CorrezioneTotale => CorrezioneSuono + (Application.isEditor ? PlayerPrefs.GetFloat(ChiaveCorrezione, 0f) : 0f);

    // ---------- momenti (secondi dall'inizio dell'animazione, cioè dopo il nero iniziale) ----------
    const float InizioEsterno = 0.0f, DurataEsterno = 1.4f;   // il filo esterno si accende e comincia a girare piano
    const float InizioAnelli = 0.3f, DurataAnelli = 1.8f;     // i quattro archi arrivano da fuori, uno dopo l'altro
    const float InizioRune = 1.3f, PassoRune = 0.045f;        // le 28 rune si accendono in giro, una dopo l'altra
    const float InizioRombo = 1.7f, DurataRombo = 1.2f;       // il rombo si apre e si gira in posizione
    const float InizioCometa = 2.3f, DurataCometa = 1.0f;     // la scintilla cala dall'alto, sempre più veloce
    const float MomentoLampo = 3.3f;                          // la scintilla tocca il lago: la runa si accende, lampo, onde, scintille
    const float InizioSposta = 5.0f, DurataSposta = 1.1f;     // il sigillo sale e rimpicciolisce (dopo mezzo secondo di pausa in più sull'impatto)
    const float InizioNome = 5.5f, DurataNome = 1.9f;         // "GNL STUDIO" al centro
    const float InizioPresenta = 7.0f, DurataPresenta = 1.0f; // "presenta"
    const float InizioSpegni = 9.0f, DurataSpegni = 1.0f;     // dissolvenza al nero
    const float AttesaFinale = 0.35f;                         // nero prima del menu

    // Il sigillo: grandezza e posizione (in pixel del foglio virtuale 1920x1080)
    const float LatoGrande = 770f, LatoPiccolo = 440f;
    const float YCentro = 540f, YAlto = 262f;

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
    Texture2D[] rune = new Texture2D[7];
    Texture2D rombo, runaCentrale, esterno, onda, raggi, vignetta;
    AudioSource sorgente;
    // Orologio del logo: conta solo i fotogrammi veri. I primi caricamenti (caratteri, scena, suoni) possono bloccare il gioco
    // anche per un secondo: quel tempo non conta, altrimenti il nero iniziale si accorcia o sparisce.
    float orologio;
    int fotogrammi;
    bool suonoPartito;
    float partenzaSuono;   // a che secondo dall'avvio far partire il file (può essere 0 se il colpo è lontano dall'inizio del file)
    float saltoNelFile;    // da che punto del file partire (se bisogna anticipare)
    float saltatoA = -1f;       // solo nell'editor: l'orologio quando si è premuto F2
    bool finito;
    Matrix4x4 base_;
    readonly List<Particella> particelle = new List<Particella>();

    // Una particella: nasce a un certo momento in un punto del disegno (coordinate 1024x1024, centro 512,512),
    // parte in una direzione, rallenta e svanisce. "tipo" 0 = scintilla, 1 = brace che sale, 2 = scintilla con alone.
    struct Particella { public Vector2 origine, direzione; public float nascita, velocita, vita, taglia, salita; public int tipo; }

    // Angolo (gradi, 0 = destra, senso orario sullo schermo) della runa i del quarto k: uguale allo script che disegna i pezzi.
    static float AngoloRuna(int k, int i) => k * 90f - 90f + 8f + i * (74f / 6f);

    void Start()
    {
        for (int i = 0; i < 4; i++) anelli[i] = Resources.Load<Texture2D>("Logo/anello-" + (i + 1));
        for (int i = 0; i < 7; i++) rune[i] = Resources.Load<Texture2D>("Logo/runa-" + (i + 1));
        rombo = Resources.Load<Texture2D>("Logo/rombo");
        runaCentrale = Resources.Load<Texture2D>("Logo/runa-centrale");
        esterno = Resources.Load<Texture2D>("Logo/anello-esterno");
        onda = Resources.Load<Texture2D>("Logo/onda");
        raggi = Resources.Load<Texture2D>("Logo/raggi");
        Cursor.visible = false;

        // vignetta: trasparente al centro, scura ai bordi
        vignetta = new Texture2D(128, 128, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float d = Mathf.Sqrt((x - 63.5f) * (x - 63.5f) + (y - 63.5f) * (y - 63.5f)) / 64f;   // 0 centro, 1 bordo, 1,41 angolo
                vignetta.SetPixel(x, y, new Color(0f, 0f, 0f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.62f, 1.3f, d))));
            }
        vignetta.Apply();

        Impostazioni.Applica();
        var suono = Resources.Load<AudioClip>("Audio/Effetti/logo-studio");
        sorgente = gameObject.AddComponent<AudioSource>();
        sorgente.playOnAwake = false;
        sorgente.spatialBlend = 0f;
        sorgente.volume = Impostazioni.VolumeEffetti;
        if (suono != null)
        {
            sorgente.clip = suono;
            float colpo = TrovaColpo(suono);
            if (colpo >= 0f)
            {
                // il file deve far cadere il colpo a (nero + MomentoLampo) secondi dall'avvio
                float partenza = AttesaNera + MomentoLampo - colpo + CorrezioneTotale;
                partenzaSuono = Mathf.Max(0f, partenza);
                saltoNelFile = Mathf.Max(0f, -partenza);
                Debug.Log("[Logo] Colpo trovato a " + colpo.ToString("0.00") + " s nel file (durata " + suono.length.ToString("0.00") + " s): il file parte a " + partenzaSuono.ToString("0.00") + " s dall'avvio");
            }
        }

        var caso = new System.Random(7);
        float R() => (float)caso.NextDouble();
        Vector2 Dir(float a) => new Vector2(Mathf.Cos(a), Mathf.Sin(a));

        // 1) quando un arco si posa, una manciata di scintille sul suo bordo
        int[] ordine = { 0, 2, 1, 3 };
        for (int i = 0; i < 4; i++)
        {
            int k = ordine[i];
            float quando = InizioAnelli + i * 0.16f + DurataAnelli * 0.72f;
            for (int n = 0; n < 16; n++)
            {
                float ang = (k * 90f - 90f + 8f + R() * 74f) * Mathf.Deg2Rad;
                particelle.Add(new Particella { origine = new Vector2(512f, 512f) + Dir(ang) * 478f, direzione = Dir(ang + (R() - 0.5f) * 1.6f),
                    nascita = quando + R() * 0.12f, velocita = 40f + R() * 150f, vita = 0.5f + R() * 0.7f, taglia = 3f + R() * 4f, salita = 20f, tipo = n % 4 == 0 ? 2 : 0 });
            }
        }
        // 2) ogni runa, accendendosi, getta qualche scintilla
        for (int k = 0; k < 4; k++)
            for (int i = 0; i < 7; i++)
            {
                float ang = AngoloRuna(k, i) * Mathf.Deg2Rad;
                float quando = InizioRune + (k * 7 + i) * PassoRune;
                for (int n = 0; n < 4; n++)
                    particelle.Add(new Particella { origine = new Vector2(512f, 512f) + Dir(ang) * 425f, direzione = Dir(ang + (R() - 0.5f) * 2.4f),
                        nascita = quando + R() * 0.15f, velocita = 20f + R() * 90f, vita = 0.5f + R() * 0.8f, taglia = 2.5f + R() * 3.5f, salita = 30f, tipo = 0 });
            }
        // 3) l'impatto: grande scoppio dal centro del lago
        for (int n = 0; n < 260; n++)
        {
            float ang = R() * Mathf.PI * 2f;
            particelle.Add(new Particella { origine = new Vector2(512f, 512f), direzione = Dir(ang),
                nascita = MomentoLampo + R() * 0.06f, velocita = 160f + R() * 900f, vita = 0.9f + R() * 1.6f, taglia = 2f + R() * 6f, salita = 40f + R() * 60f, tipo = n % 5 == 0 ? 2 : 0 });
        }
        // 4) gocce d'acqua luminose che schizzano verso l'alto dal lago
        for (int n = 0; n < 50; n++)
        {
            float ang = -Mathf.PI * 0.5f + (R() - 0.5f) * 1.5f;
            particelle.Add(new Particella { origine = new Vector2(512f + (R() - 0.5f) * 60f, 500f), direzione = Dir(ang),
                nascita = MomentoLampo + R() * 0.12f, velocita = 200f + R() * 520f, vita = 1.0f + R() * 1.0f, taglia = 3f + R() * 4f, salita = -120f, tipo = 2 });
        }
        // 4b) grandi braci sfocate e lente, in lontananza: danno profondità
        for (int n = 0; n < 26; n++)
            particelle.Add(new Particella { origine = new Vector2(R() * 1024f, 700f + R() * 500f), direzione = new Vector2((R() - 0.5f) * 0.5f, -1f),
                nascita = 1.0f + R() * 6.0f, velocita = 10f + R() * 16f, vita = 3.5f + R() * 2.5f, taglia = 18f + R() * 30f, salita = 0f, tipo = 3 });
        // 5) braci che salgono piano dal sigillo, dall'accensione fino alla fine
        for (int n = 0; n < 150; n++)
        {
            float ang = R() * Mathf.PI * 2f, raggio = Mathf.Sqrt(R()) * 470f;
            particelle.Add(new Particella { origine = new Vector2(512f, 512f) + Dir(ang) * raggio, direzione = new Vector2((R() - 0.5f) * 0.6f, -1f),
                nascita = 0.8f + R() * 7.2f, velocita = 18f + R() * 38f, vita = 2.2f + R() * 2.4f, taglia = 2f + R() * 3.5f, salita = 0f, tipo = 1 });
        }
    }

    // Cerca nel file il colpo: la finestra di 20 ms con più energia. Ritorna i secondi dall'inizio del file, o -1 se non riesce a leggerlo.
    static float TrovaColpo(AudioClip clip)
    {
        try
        {
            int canali = clip.channels;
            var campioni = new float[clip.samples * canali];
            if (!clip.GetData(campioni, 0))
            {
                Debug.LogWarning("[Logo] Non riesco a leggere il file audio: il suono parte con l'animazione senza aggancio.");
                return -1f;
            }
            int finestra = Mathf.Max(1, clip.frequency / 50);    // 20 ms
            int passo = Mathf.Max(1, finestra / 2);
            float migliore = 0f; int posizione = -1;
            for (int inizio = 0; inizio + finestra <= clip.samples; inizio += passo)
            {
                float energia = 0f;
                for (int i = inizio; i < inizio + finestra; i++)
                    for (int ch = 0; ch < canali; ch++) { float v = campioni[i * canali + ch]; energia += v * v; }
                if (energia > migliore) { migliore = energia; posizione = inizio; }
            }
            if (posizione < 0) return -1f;
            return (float)posizione / clip.frequency;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Logo] Errore nel leggere il file audio (" + e.Message + ")");
            return -1f;
        }
    }

    // Secondi dall'inizio dell'animazione: negativo durante il nero iniziale.
    float Tempo => orologio - AttesaNera;

    // Solo nell'editor, con F2: la dissolvenza al nero parte subito (più veloce).
    float TempoSpegni => saltatoA >= 0f ? Mathf.Min(InizioSpegni, saltatoA - AttesaNera) : InizioSpegni;
    float DurataSpegniAttuale => saltatoA >= 0f ? 0.6f : DurataSpegni;

    void Update()
    {
        if (finito) return;
        // i primi fotogrammi (caricamenti pesanti) non contano; poi si conta con passi mai più lunghi di 50 ms
        fotogrammi++;
        if (fotogrammi > 4) orologio += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        float t = Tempo;
        if (!suonoPartito && orologio >= partenzaSuono)
        {
            suonoPartito = true;
            if (sorgente != null && sorgente.clip != null)
            {
                sorgente.time = Mathf.Clamp(saltoNelFile, 0f, Mathf.Max(0f, sorgente.clip.length - 0.1f));
                sorgente.Play();
            }
        }
        // Solo nell'editor: F3 = suono 0,1 s più presto, F4 = 0,1 s più tardi (vale dalla prova successiva), F5 = azzera
        if (Application.isEditor && Keyboard.current != null)
        {
            // passo: 0,1 s; con Maiusc premuto 0,01 s (un centesimo); con Ctrl premuto 0,5 s
            float grandezza = Keyboard.current.shiftKey.isPressed ? 0.01f : Keyboard.current.ctrlKey.isPressed ? 0.5f : 0.1f;
            float passo = 0f;
            if (Keyboard.current.f3Key.wasPressedThisFrame) passo = -grandezza;
            if (Keyboard.current.f4Key.wasPressedThisFrame) passo = grandezza;
            if (passo != 0f) PlayerPrefs.SetFloat(ChiaveCorrezione, PlayerPrefs.GetFloat(ChiaveCorrezione, 0f) + passo);
            if (Keyboard.current.f5Key.wasPressedThisFrame) PlayerPrefs.SetFloat(ChiaveCorrezione, 0f);
        }
        if (Application.isEditor && saltatoA < 0f && t < InizioSpegni && Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame)
            saltatoA = orologio;
        float fine = (saltatoA >= 0f ? saltatoA - AttesaNera + 0.6f : InizioSpegni + DurataSpegni) + AttesaFinale;
        if (saltatoA >= 0f && sorgente != null && sorgente.isPlaying) sorgente.volume = Mathf.MoveTowards(sorgente.volume, 0f, Time.unscaledDeltaTime / 0.5f);
        if (t >= fine) Concludi();
    }

    void Concludi()
    {
        finito = true;
        Cursor.visible = true;
        if (vignetta != null) Destroy(vignetta);
        if (menu != null) menu.enabled = true;   // da qui parte il menu (Start, musica, titolo)
        Destroy(gameObject);
    }

    static float Uscita(float x) { x = Mathf.Clamp01(x); return 1f - Mathf.Pow(1f - x, 4f); }       // frena alla fine
    static float UscitaForte(float x) { x = Mathf.Clamp01(x); return 1f - Mathf.Pow(1f - x, 5f); }
    static float Entrata(float x) { x = Mathf.Clamp01(x); return Mathf.Pow(x, 2.4f); }              // accelera (caduta)
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

        // dove sta il sigillo adesso: grande al centro, poi sale e si rimpicciolisce
        float sposta = Morbido((t - InizioSposta) / DurataSposta);
        float lato = Mathf.Lerp(LatoGrande, LatoPiccolo, sposta);
        float u = lato / 1024f;   // un pixel del disegno in pixel dello schermo
        float dopo = t - MomentoLampo;
        float tremito = dopo >= 0f ? Mathf.Exp(-dopo * 8f) * 9f : 0f;
        var centro = new Vector2(GraficaMenu.Larghezza * 0.5f, Mathf.Lerp(YCentro, YAlto, sposta))
                   + new Vector2(Mathf.Sin(t * 83f), Mathf.Cos(t * 71f)) * tremito;
        var r = new Rect(centro.x - lato * 0.5f, centro.y - lato * 0.5f, lato, lato);

        // bagliore caldo dietro il sigillo: cresce mentre si compone, respira alla fine
        float cresce = Morbido((t - InizioAnelli) / (MomentoLampo - InizioAnelli));
        float respiro = dopo > 1f ? 0.85f + 0.15f * Mathf.Sin(t * 1.8f) : 1f;
        float lampo = dopo >= 0f ? Mathf.Exp(-dopo * 3.2f) : 0f;
        Bagliore(centro, lato * 1.35f + 300f * lampo, new Color(1f, 0.5f, 0.18f), (0.11f * cresce * respiro + 0.60f * lampo) * luce);
        if (t < MomentoLampo)   // prima dell'impatto: un punto di luce al centro che cresce
            Bagliore(centro, 60f + 340f * Morbido(t / MomentoLampo), new Color(1f, 0.7f, 0.35f), 0.35f * Morbido(t / 1.0f) * luce);

        // colore del sigillo: spento mentre arriva, si accende al lampo e si assesta su un bronzo caldo
        Color spento = new Color(0.50f, 0.40f, 0.26f), caldo = new Color(1f, 0.86f, 0.62f), riposo = new Color(0.84f, 0.69f, 0.44f);
        Color colore = t < MomentoLampo ? Color.Lerp(spento, riposo, Morbido((t - InizioAnelli) / (MomentoLampo - InizioAnelli)))
                                        : Color.Lerp(caldo, riposo, Morbido(dopo / 1.6f));

        // raggi di luce dietro al sigillo: si aprono al lampo e poi girano pianissimo
        if (dopo >= 0f)
        {
            float a = Mathf.Clamp01(dopo / 0.35f) * (0.20f + 0.35f * lampo) * luce;
            float s = 1.45f + 0.55f * Uscita(dopo / 2.2f);
            Strato(raggi, r, Vector2.zero, dopo * 2.5f, s, Con(new Color(1f, 0.72f, 0.38f), a));
        }

        // il filo esterno: si accende subito e gira piano per tutta la durata
        float pe = Morbido((t - InizioEsterno) / DurataEsterno);
        if (pe > 0f) Strato(esterno, r, Vector2.zero, t * 4f, 1f + 0.06f * (1f - pe), Con(colore, pe * 0.9f * luce));

        // i quattro archi: ognuno arriva da fuori, ruotando, in un momento suo
        int[] ordine = { 0, 2, 1, 3 };
        for (int i = 0; i < 4; i++)
        {
            int k = ordine[i];
            float p = Uscita((t - (InizioAnelli + i * 0.16f)) / DurataAnelli);
            if (p <= 0f) continue;
            float angolo = Mathf.Deg2Rad * (k * 90f - 90f + 45f);
            var direzione = new Vector2(Mathf.Cos(angolo), Mathf.Sin(angolo));
            float lontano = 760f * u * (1f - p);
            Strato(anelli[k], r, direzione * lontano, (k % 2 == 0 ? -1f : 1f) * 60f * (1f - p), 1f + 0.4f * (1f - p), Con(colore, Mathf.Clamp01(p * 2.2f) * luce));
        }

        // il rombo: si apre dal centro girando
        float pr = UscitaForte((t - InizioRombo) / DurataRombo);
        if (pr > 0f) Strato(rombo, r, Vector2.zero, -90f * (1f - pr), 0.45f + 0.55f * pr, Con(colore, Mathf.Clamp01(pr * 1.6f) * luce));

        // le rune: si accendono una dopo l'altra, in giro; al lampo si illuminano tutte insieme e poi restano accese, con un'onda di luce
        DisegnaRune(centro, u, t, dopo, lampo, luce);

        // onde del lago: dalla punta della spada si allargano cerchi
        if (dopo >= 0f)
        {
            for (int n = 0; n < 4; n++)
            {
                float d = (dopo - n * 0.16f) / 1.5f;
                if (d <= 0f || d >= 1f) continue;
                float e = Uscita(d);
                Strato(onda, r, Vector2.zero, 0f, 0.08f + 0.92f * e, Con(caldo, (1f - d) * (1f - d) * (n == 0 ? 0.85f : 0.55f) * luce));
            }
            // l'anello più grande, veloce: l'onda d'urto
            float os = Uscita(dopo / 0.9f);
            for (int k = 0; k < 4; k++)
                Strato(anelli[k], r, Vector2.zero, 0f, 1f + 0.5f * os, Con(caldo, (1f - os) * 0.75f * luce));
        }

        // la runa centrale: prima appena visibile (spenta), al lampo si accende forte e poi pulsa piano
        float vista = Morbido((t - InizioRombo - 0.3f) / 0.8f);
        float accesa = dopo >= 0f ? 1f : 0f;
        float forzaRuna = dopo >= 0f ? Mathf.Clamp01(0.62f + 0.38f * Mathf.Exp(-dopo * 2.2f) + 0.12f * Mathf.Sin(t * 2.4f)) : 0f;
        if (vista > 0f)
        {
            if (accesa > 0f)
            {
                Bagliore(centro, lato * 0.42f, new Color(1f, 0.55f, 0.2f), (0.25f + 0.55f * Mathf.Exp(-dopo * 2.5f)) * forzaRuna * luce);
                Strato(runaCentrale, r, Vector2.zero, 0f, 1f + 0.1f * Mathf.Exp(-dopo * 4f), Con(Color.Lerp(riposo, new Color(1f, 0.95f, 0.78f), forzaRuna), luce));
            }
            else Strato(runaCentrale, r, Vector2.zero, 0f, 1f, Con(spento, 0.28f * vista * luce));
        }

        // la scintilla: cala dall'alto sempre più veloce e cade nel lago, con una scia di luce
        float pc = Entrata((t - InizioCometa) / DurataCometa);
        if (pc > 0f && dopo < 0f)
        {
            float y = -(1f - pc) * 700f * u;
            for (int n = 0; n < 7; n++)
                Bagliore(centro + new Vector2(0f, y - n * 26f * u * (0.4f + pc)), (110f - n * 10f) * u + 30f, new Color(1f, 0.72f - n * 0.04f, 0.38f), (0.75f - n * 0.1f) * pc * luce);
            Bagliore(centro + new Vector2(0f, y), 70f * u + 20f, new Color(1f, 0.97f, 0.85f), 0.95f * luce);
            float coda = 360f * u * pc;
            GraficaMenu.Riempi(new Rect(centro.x - 1.5f, centro.y + y - coda, 3f, coda), new Color(1f, 0.85f, 0.55f, 0.55f * luce));
        }

        // lampo su tutto lo schermo, breve
        if (dopo >= 0f && dopo < 0.5f) { GUI.color = new Color(1f, 0.85f, 0.6f, (1f - dopo / 0.5f) * (1f - dopo / 0.5f) * 0.28f * luce); GUI.DrawTexture(new Rect(0, 0, GraficaMenu.Larghezza, GraficaMenu.Altezza), GraficaMenu.Pixel); GUI.color = Color.white; }

        DisegnaParticelle(t, luce);
        // vignetta scura ai bordi: si chiude piano mentre il sigillo si compone e si stringe al lampo
        float chiusa = Morbido(t / MomentoLampo) * 0.62f + 0.28f * lampo;
        if (chiusa > 0.01f) { GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(chiusa) * luce); GUI.DrawTexture(new Rect(0, 0, GraficaMenu.Larghezza, GraficaMenu.Altezza), vignetta); GUI.color = Color.white; }
        DisegnaNome(t, luce);
        if (Application.isEditor && !finito)
        {
            var piccolo = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.LowerLeft };
            GUI.color = new Color(1f, 1f, 1f, 0.55f);
            GUI.Label(new Rect(20f, GraficaMenu.Altezza - 60f, 1400f, 40f),
                "Solo editor - Correzione suono: " + CorrezioneTotale.ToString("+0.00;-0.00;0.00") + " s   (F3 = suono più presto, F4 = più tardi; Maiusc = passi da 0,01 s, Ctrl = 0,5 s; F5 = azzera; vale dalla prova successiva)", piccolo);
            GUI.color = Color.white;
        }
    }

    // Le 28 rune: ogni runa ha il suo momento di accensione. Brillano forte, poi calano a una luce calda;
    // al lampo si accendono tutte insieme, poi una piccola onda di luce gira intorno all'anello.
    void DisegnaRune(Vector2 centro, float u, float t, float dopo, float lampo, float luce)
    {
        float lunga = 62f * u;   // grandezza della runa in pixel di schermo (circa 62 nel disegno)
        for (int k = 0; k < 4; k++)
        {
            // l'arco deve essere arrivato: la runa non appare prima del suo arco
            int posizione = k == 0 ? 0 : k == 2 ? 1 : k == 1 ? 2 : 3;
            float arrivo = InizioAnelli + posizione * 0.16f + DurataAnelli * 0.35f;
            for (int i = 0; i < 7; i++)
            {
                float quando = Mathf.Max(InizioRune + (k * 7 + i) * PassoRune, arrivo);
                float da = t - quando;
                if (da < 0f) continue;
                float lampeggio = Mathf.Exp(-da * 5f);                         // forte all'accensione
                float base_luce = 0.50f + 0.50f * Morbido(da / 0.6f);          // poi resta accesa
                float giro = 0f;
                if (dopo > 0.5f) giro = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * 2.2f - (k * 7 + i) * 0.22f)), 3f) * 0.35f;
                float forza = Mathf.Clamp01(base_luce * 0.75f + lampeggio * 0.6f + lampo * 0.9f + giro);
                float ang = AngoloRuna(k, i);
                float rad = ang * Mathf.Deg2Rad;
                Vector2 pos = centro + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * 425f * u;
                Color calda = Color.Lerp(new Color(0.95f, 0.62f, 0.26f), new Color(1f, 0.95f, 0.78f), forza);
                // alone dietro la runa
                Bagliore(pos, lunga * (1.9f + 0.9f * forza), new Color(1f, 0.55f, 0.2f), forza * 0.6f * luce);
                var rr = new Rect(pos.x - lunga * 0.5f, pos.y - lunga * 0.5f, lunga, lunga);
                var tex = rune[(i + k * 3) % 7];
                if (tex == null) continue;
                // la punta della runa guarda fuori dal cerchio
                GUI.matrix = base_ * Matrix4x4.TRS(new Vector3(pos.x, pos.y, 0f), Quaternion.Euler(0f, 0f, ang + 90f), Vector3.one)
                                   * Matrix4x4.TRS(new Vector3(-pos.x, -pos.y, 0f), Quaternion.identity, Vector3.one);
                GUI.color = Con(calda, Mathf.Clamp01(0.35f + forza) * luce);
                GUI.DrawTexture(rr, tex, ScaleMode.StretchToFill, true);
                GUI.matrix = base_;
                GUI.color = Color.white;
            }
        }
    }

    // Lettere con la spaziatura che si stringe: partono larghe e quasi invisibili, si raccolgono nel nome. Al centro dello schermo.
    void DisegnaNome(float t, float luce)
    {
        const float yNome = 520f;   // alto del nome (corpo 92): il nome sta circa a metà schermo
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
                float sale = (1f - Morbido((t - InizioNome - ritardo) / 0.9f)) * 18f;   // salgono leggere mentre si accendono
                // un chiarore caldo dietro la lettera mentre compare
                Bagliore(new Vector2(x + w * 0.5f, yNome + 60f), 150f, new Color(1f, 0.6f, 0.25f), 0.22f * Mathf.Sin(Mathf.Clamp01((t - InizioNome - ritardo) / 0.9f) * Mathf.PI) * luce);
                GraficaMenu.Scritta(new Rect(x, yNome + sale, w, 120f), c, stile, colori[parte], a);
                x += w + spazio;
                if (i + 1 < lettere.Count && lettere[i + 1].parte != parte) x += spazioParole;
                indice++;
            }
            // filo di luce sotto il nome, si allunga con le lettere
            float lunghezza = Mathf.Lerp(0f, 560f, Uscita((t - InizioNome - 0.3f) / (DurataNome * 0.9f)));
            GraficaMenu.Riempi(new Rect(GraficaMenu.Larghezza * 0.5f - lunghezza * 0.5f, yNome + 132f, lunghezza, 2f), GraficaMenu.Con(GraficaMenu.Bronzo, 0.9f * luce));
            GraficaMenu.Rombo(new Vector2(GraficaMenu.Larghezza * 0.5f, yNome + 133f), 11f * Mathf.Clamp01(lunghezza / 560f), GraficaMenu.Con(GraficaMenu.Selezione, luce));
        }

        float pp = Morbido((t - InizioPresenta) / DurataPresenta);
        if (pp > 0f)
        {
            string testo = Lingua.T("logo.presenta");
            if (Lingua.Indice != 7) testo = GraficaMenu.Spaziato(testo);
            var stile = new GUIStyle(GraficaMenu.Voce) { fontSize = 28 };
            GraficaMenu.Scritta(new Rect(0f, yNome + 160f, GraficaMenu.Larghezza, 50f), testo, stile, new Color(0.62f, 0.58f, 0.5f), pp * luce);
        }
    }

    // Tutte le particelle: scintille (quadratini), con alone (scintille più grandi con un chiarore), braci che salgono.
    void DisegnaParticelle(float t, float luce)
    {
        // le particelle restano dove sono nate sullo schermo (il sigillo grande al centro), anche quando poi il sigillo sale
        foreach (var s in particelle)
        {
            float dt = t - s.nascita;
            if (dt < 0f || dt > s.vita) continue;
            float k = dt / s.vita;
            float corsa = s.velocita * (1f - Mathf.Pow(1f - Mathf.Clamp01(dt / Mathf.Min(0.9f, s.vita)), 2f)) * 0.9f;
            if (s.tipo == 1 || s.tipo == 3) corsa = s.velocita * dt;   // le braci salgono piano e costanti
            Vector2 p = s.origine + s.direzione * corsa;
            p.y -= s.salita * dt * (s.tipo == 1 ? 0f : 1f) * (1f - 0.4f * k);
            // posizione sullo schermo con il sigillo nel posto iniziale (grande, al centro)
            Vector2 schermo = new Vector2(GraficaMenu.Larghezza * 0.5f, YCentro) - new Vector2(512f, 512f) * (LatoGrande / 1024f) + p * (LatoGrande / 1024f);
            float alfa;
            if (s.tipo == 3) { Bagliore(schermo, s.taglia * 3.2f, new Color(1f, 0.5f, 0.2f), Mathf.Sin(k * Mathf.PI) * 0.16f * luce); continue; }
            if (s.tipo == 1) alfa = Mathf.Sin(k * Mathf.PI) * 0.7f * (0.75f + 0.25f * Mathf.Sin(t * 9f + s.nascita * 13f));
            else alfa = (1f - k) * (1f - k);
            alfa *= luce;
            if (alfa <= 0.01f) continue;
            float taglia = s.taglia * (s.tipo == 1 ? 1f : (1f - 0.5f * k));
            if (s.tipo == 2) Bagliore(schermo, taglia * 7f, new Color(1f, 0.6f, 0.25f), alfa * 0.5f);
            Color c = s.tipo == 1 ? new Color(1f, 0.62f, 0.28f, alfa) : new Color(1f, 0.78f, 0.42f, alfa);
            GraficaMenu.Riempi(new Rect(schermo.x - taglia * 0.5f, schermo.y - taglia * 0.5f, taglia, taglia), c);
            // le scintille veloci lasciano una piccola coda
            if (s.tipo != 1 && dt < 0.4f)
            {
                Vector2 coda = schermo - s.direzione * (s.velocita * 0.035f);
                GraficaMenu.Riempi(new Rect(coda.x - taglia * 0.35f, coda.y - taglia * 0.35f, taglia * 0.7f, taglia * 0.7f), new Color(1f, 0.55f, 0.2f, alfa * 0.6f));
            }
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
