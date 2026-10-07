using UnityEngine;

// Stile grafico comune dei menu (menu iniziale, menu di pausa): colori, caratteri, riquadri e decorazioni
// in stile dark fantasy. Così i menu sono tutti uguali e si cambiano in un posto solo.
// Caratteri (in Assets/Resources/Caratteri, licenza SIL OFL: gratuiti anche in un gioco venduto):
//   - lingue con l'alfabeto latino: Cinzel (titoli e voci) e IM Fell English (testi);
//   - russo: Forum (titoli e voci) e Cormorant Garamond (testi);
//   - cinese: ZCOOL XiaoWei.
// Tutto si disegna su un foglio virtuale di 1920x1080, ingrandito o rimpicciolito per lo schermo.
// Come montarlo: non si monta su nessun oggetto, lo usano MenuPrincipale e MenuPausa.
public static class GraficaMenu
{
    public const float Larghezza = 1920f, Altezza = 1080f;

    public static readonly Color Testo = new Color(0.85f, 0.81f, 0.74f);
    public static readonly Color Selezione = new Color(0.95f, 0.63f, 0.3f);
    public static readonly Color Spento = new Color(0.45f, 0.43f, 0.4f);
    public static readonly Color Bronzo = new Color(0.55f, 0.44f, 0.28f);
    public static readonly Color Riquadro = new Color(0.025f, 0.025f, 0.035f, 0.82f);

    // ---------- trame ----------

    static Texture2D pixel, vignetta, bagliore, fascia, sfumatura;

    public static Texture2D Pixel { get { if (pixel == null) pixel = Tinta(Color.white); return pixel; } }
    public static Texture2D Vignetta { get { if (vignetta == null) vignetta = Radiale(128, 0.45f, 1.35f, 0f, 0.92f, Color.black); return vignetta; } }
    public static Texture2D Bagliore { get { if (bagliore == null) bagliore = Radiale(64, 0f, 1f, 1f, 0f, Color.white); return bagliore; } }

    // Striscia orizzontale che sfuma ai due lati (per le fasce di luce e i divisori).
    public static Texture2D Fascia
    {
        get
        {
            if (fascia != null) return fascia;
            fascia = new Texture2D(128, 1) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            for (int x = 0; x < 128; x++)
            {
                float t = 1f - Mathf.Abs(x / 127f * 2f - 1f);
                fascia.SetPixel(x, 0, new Color(1f, 1f, 1f, Mathf.SmoothStep(0f, 1f, t)));
            }
            fascia.Apply();
            return fascia;
        }
    }

    // Sfumatura verticale: nera in basso, trasparente in alto.
    public static Texture2D Sfumatura
    {
        get
        {
            if (sfumatura != null) return sfumatura;
            sfumatura = new Texture2D(1, 64) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            for (int y = 0; y < 64; y++)
                sfumatura.SetPixel(0, y, new Color(0f, 0f, 0f, Mathf.Lerp(0.85f, 0f, y / 63f)));
            sfumatura.Apply();
            return sfumatura;
        }
    }

    static Texture2D Tinta(Color c)
    {
        var t = new Texture2D(1, 1) { hideFlags = HideFlags.DontSave };
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    static Texture2D Radiale(int lato, float inizio, float fine, float alfaCentro, float alfaBordo, Color colore)
    {
        var t = new Texture2D(lato, lato) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
        float c = (lato - 1) * 0.5f;
        for (int y = 0; y < lato; y++)
            for (int x = 0; x < lato; x++)
            {
                float dx = (x - c) / c, dy = (y - c) / c;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(inizio, fine, Mathf.Sqrt(dx * dx + dy * dy)));
                t.SetPixel(x, y, new Color(colore.r, colore.g, colore.b, Mathf.Lerp(alfaCentro, alfaBordo, k)));
            }
        t.Apply();
        return t;
    }

    // ---------- caratteri e stili ----------

    static bool caratteriCaricati;
    static Font cinzel, cinzelGrassetto, fell, fellCorsivo, forum, cormorant, cormorantCorsivo, xiaowei;

    static void CaricaCaratteri()
    {
        if (caratteriCaricati) return;
        caratteriCaricati = true;
        cinzel = Resources.Load<Font>("Caratteri/Cinzel-Regular");
        cinzelGrassetto = Resources.Load<Font>("Caratteri/Cinzel-Bold");
        fell = Resources.Load<Font>("Caratteri/IMFeENrm28P");
        fellCorsivo = Resources.Load<Font>("Caratteri/IMFeENit28P");
        forum = Resources.Load<Font>("Caratteri/Forum-Regular");
        cormorant = Resources.Load<Font>("Caratteri/CormorantGaramond-Medium");
        cormorantCorsivo = Resources.Load<Font>("Caratteri/CormorantGaramond-MediumItalic");
        xiaowei = Resources.Load<Font>("Caratteri/ZCOOLXiaoWei-Regular");
    }

    public static GUIStyle Titolo, Emblema, Intestazione, NomeClasse, Sottotitolo, Voce, VoceSinistra, Descrizione, Piccolo;
    static int linguaStili = -1;

    // Da chiamare all'inizio di OnGUI: prepara gli stili e li rifà quando cambia la lingua.
    public static void PreparaStili()
    {
        int lingua = Lingua.Indice;
        if (Titolo != null && lingua == linguaStili) return;
        linguaStili = lingua;
        CaricaCaratteri();

        Font voce, forte, libro, corsivo;
        if (lingua == 6)        // russo
        {
            voce = forte = forum;
            libro = cormorant;
            corsivo = cormorantCorsivo;
        }
        else if (lingua == 7)   // cinese
        {
            voce = forte = libro = corsivo = xiaowei;
        }
        else
        {
            voce = cinzel;
            forte = cinzelGrassetto;
            libro = fell;
            corsivo = fellCorsivo;
        }

        Titolo = Stile(108, cinzelGrassetto, true);      // il nome del gioco è sempre in lettere latine
        Emblema = Stile(92, cinzelGrassetto, true);      // numeri romani
        Intestazione = Stile(62, forte, true);
        NomeClasse = Stile(38, forte, true);
        Sottotitolo = Stile(30, corsivo, false, true);
        Voce = Stile(36, voce, false);
        VoceSinistra = Stile(32, voce, false);
        VoceSinistra.alignment = TextAnchor.MiddleLeft;
        Descrizione = Stile(29, libro, false);
        Descrizione.wordWrap = true;
        Descrizione.alignment = TextAnchor.UpperCenter;
        Piccolo = Stile(20, null, false);
    }

    // Se il carattere manca (file non trovato), si usa quello di base con grassetto o corsivo finti.
    static GUIStyle Stile(int dimensione, Font font, bool grassetto, bool corsivo = false)
    {
        var s = new GUIStyle(GUI.skin.label)
        {
            fontSize = dimensione,
            fontStyle = font != null ? FontStyle.Normal : grassetto ? FontStyle.Bold : corsivo ? FontStyle.Italic : FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,
            richText = false,
            clipping = TextClipping.Overflow,
        };
        s.normal.textColor = Color.white;
        if (font != null) s.font = font;
        return s;
    }

    // ---------- disegno ----------

    // Imposta il foglio virtuale 1920x1080 centrato sullo schermo.
    public static void FoglioVirtuale()
    {
        float s = Screen.height / Altezza;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - Larghezza * s) * 0.5f, 0f, 0f), Quaternion.identity, new Vector3(s, s, 1f));
    }

    // Bordi scuri e sfumatura in basso, su tutto lo schermo (fuori dal foglio virtuale).
    public static void Atmosfera(float scurimento)
    {
        GUI.matrix = Matrix4x4.identity;
        if (scurimento > 0f) Riempi(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, scurimento));
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Vignetta, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(0, Screen.height * 0.45f, Screen.width, Screen.height * 0.55f), Sfumatura, ScaleMode.StretchToFill);
    }

    public static Color Con(Color c, float alfa) => new Color(c.r, c.g, c.b, c.a * alfa);

    public static void Riempi(Rect r, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(r, Pixel);
        GUI.color = Color.white;
    }

    // Alone di luce morbido (per esempio dietro al titolo o alla voce scelta).
    public static void Alone(Rect r, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(r, Bagliore);
        GUI.color = Color.white;
    }

    // Fascia di luce orizzontale che sfuma ai lati.
    public static void FasciaLuce(Rect r, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(r, Fascia, ScaleMode.StretchToFill);
        GUI.color = Color.white;
    }

    // Quadratino ruotato di 45 gradi.
    public static void Rombo(Vector2 centro, float lato, Color c)
    {
        Matrix4x4 prima = GUI.matrix;
        GUI.matrix = prima * Matrix4x4.TRS(centro, Quaternion.Euler(0f, 0f, 45f), Vector3.one) * Matrix4x4.TRS(-centro, Quaternion.identity, Vector3.one);
        Riempi(new Rect(centro.x - lato * 0.5f, centro.y - lato * 0.5f, lato, lato), c);
        GUI.matrix = prima;
    }

    public static void Bordo(Rect r, float spessore, Color c)
    {
        Riempi(new Rect(r.x, r.y, r.width, spessore), c);
        Riempi(new Rect(r.x, r.yMax - spessore, r.width, spessore), c);
        Riempi(new Rect(r.x, r.y, spessore, r.height), c);
        Riempi(new Rect(r.xMax - spessore, r.y, spessore, r.height), c);
    }

    // Riquadro scuro con doppia cornice color bronzo, rombi agli angoli e al centro dei lati.
    public static void Cornice(Rect r, float alfa, bool acceso)
    {
        Color bordo = acceso ? Selezione : Bronzo;
        Riempi(r, Con(Riquadro, alfa));
        Bordo(r, 2f, Con(bordo, 0.9f * alfa));
        Bordo(new Rect(r.x + 7f, r.y + 7f, r.width - 14f, r.height - 14f), 1f, Con(bordo, 0.4f * alfa));
        Angolo(new Vector2(r.x, r.y), bordo, alfa);
        Angolo(new Vector2(r.xMax, r.y), bordo, alfa);
        Angolo(new Vector2(r.x, r.yMax), bordo, alfa);
        Angolo(new Vector2(r.xMax, r.yMax), bordo, alfa);
        Rombo(new Vector2(r.center.x, r.y), 10f, Con(bordo, alfa));
        Rombo(new Vector2(r.center.x, r.yMax), 10f, Con(bordo, alfa));
    }

    static void Angolo(Vector2 punto, Color bordo, float alfa)
    {
        Rombo(punto, 14f, Con(bordo, alfa));
        Rombo(punto, 6f, Con(Riquadro, alfa));
    }

    // Linea che sfuma ai lati, con un rombo al centro e due più piccoli accanto.
    public static void Divisore(float centroX, float y, float larghezza, float alfa)
    {
        FasciaLuce(new Rect(centroX - larghezza * 0.5f, y - 1f, larghezza, 2f), Con(Bronzo, 0.9f * alfa));
        Rombo(new Vector2(centroX, y), 12f, Con(Bronzo, alfa));
        Rombo(new Vector2(centroX, y), 5f, Con(Riquadro, alfa));
        Rombo(new Vector2(centroX - 24f, y), 6f, Con(Bronzo, alfa));
        Rombo(new Vector2(centroX + 24f, y), 6f, Con(Bronzo, alfa));
    }

    // Scritta con un'ombra scura sotto, per leggerla bene sopra la scena.
    public static void Scritta(Rect area, string testo, GUIStyle stile, Color colore, float alfa)
    {
        if (alfa <= 0.001f || string.IsNullOrEmpty(testo)) return;
        GUI.color = new Color(0f, 0f, 0f, 0.75f * alfa);
        GUI.Label(new Rect(area.x + 3, area.y + 3, area.width, area.height), testo, stile);
        GUI.color = new Color(colore.r, colore.g, colore.b, colore.a * alfa);
        GUI.Label(area, testo, stile);
        GUI.color = Color.white;
    }

    // Lettere spaziate, come in un'iscrizione: "TITOLO" diventa "T I T O L O".
    public static string Spaziato(string testo)
    {
        var parti = new System.Text.StringBuilder();
        for (int i = 0; i < testo.Length; i++)
        {
            parti.Append(testo[i]);
            if (i < testo.Length - 1) parti.Append(testo[i] == ' ' ? "  " : " ");
        }
        return parti.ToString();
    }
}
