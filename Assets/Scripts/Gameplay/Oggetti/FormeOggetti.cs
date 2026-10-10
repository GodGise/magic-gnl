using System.Collections.Generic;
using UnityEngine;

// Forme provvisorie degli oggetti, fatte di cubi, cilindri e sfere in stile PS2, finché non arrivano i modelli di Nazar.
// A cosa serve: ogni oggetto del gioco (armi, scudi, archi, armature, amuleti, bastoni, libri, incantesimi) ha una sua
// forma riconoscibile: lo stiletto è sottile, lo spadone lungo, l'ascia del boia ha una lama enorme, il pavese è alto,
// le vesti dello Stregone arrivano alle ginocchia, gli amuleti hanno il colore della loro pietra, e così via.
// Le usano AspettoEquipaggiamento (oggetti addosso al personaggio) e OggettoRaccoglibile (oggetti a terra).
// Se un oggetto ha il suo "Modello" (DatiOggetto.modello, il prefab di Nazar), si usa quello e non la forma.
// Come sono orientate:
//   - armi, bastoni: l'impugnatura è nel punto zero e la lama (o l'asta) va verso il basso (-Y), come la spada
//     provvisoria di AspettoUmanoide: in mano prosegue il braccio;
//   - archi e balestre: in verticale, centrati (stanno sulla schiena);
//   - scudi e libri: centrati sulla mano sinistra;
//   - armature: centrate sul busto (0,25 m sopra il centro della figura);
//   - amuleti: il ciondolo nel punto zero.
// Gli oggetti nuovi senza una forma scritta qui ricevono quella del loro tipo (per esempio una spada qualsiasi).
// Come montarlo: non si monta. È una raccolta di funzioni (FormeOggetti.Crea).
public static class FormeOggetti
{
    // ---------- colori ----------
    static readonly Color Ferro = new Color(0.6f, 0.62f, 0.66f);
    static readonly Color FerroScuro = new Color(0.28f, 0.29f, 0.32f);
    static readonly Color Ruggine = new Color(0.48f, 0.38f, 0.3f);
    static readonly Color Legno = new Color(0.3f, 0.2f, 0.12f);
    static readonly Color LegnoScuro = new Color(0.16f, 0.11f, 0.07f);
    static readonly Color Cuoio = new Color(0.28f, 0.17f, 0.09f);
    static readonly Color Osso = new Color(0.86f, 0.82f, 0.7f);
    static readonly Color Oro = new Color(0.78f, 0.62f, 0.25f);
    static readonly Color Nero = new Color(0.07f, 0.07f, 0.09f);
    static readonly Color Stoffa = new Color(0.36f, 0.33f, 0.28f);
    static readonly Color Carta = new Color(0.85f, 0.8f, 0.66f);

    static readonly Color Brace = new Color(1f, 0.45f, 0.15f);
    static readonly Color LagoNero = new Color(0.35f, 0.7f, 1f);
    static readonly Color Ombra = new Color(0.55f, 0.3f, 0.85f);
    static readonly Color Evocazione = new Color(0.6f, 0.95f, 0.65f);

    // ---------- materiali (uno per colore, riusato) ----------
    static Material baseStandard;
    static readonly Dictionary<Color, Material> opachi = new Dictionary<Color, Material>();
    static readonly Dictionary<Color, Material> luminosi = new Dictionary<Color, Material>();

    static Material Mat(Color c, bool luce = false)
    {
        var elenco = luce ? luminosi : opachi;
        if (elenco.TryGetValue(c, out Material m) && m != null) return m;
        if (baseStandard == null)
        {
            // Il materiale di base delle forme di Unity (shader Standard), così funziona anche nel gioco finito.
            var cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseStandard = cubo.GetComponent<Renderer>().sharedMaterial;
            Object.Destroy(cubo);
        }
        m = new Material(baseStandard) { color = c, name = (luce ? "Luce " : "Forma ") + ColorUtility.ToHtmlStringRGB(c) };
        m.SetFloat("_Glossiness", luce ? 0.4f : 0.08f);
        if (luce)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * 1.6f);
        }
        elenco[c] = m;
        return m;
    }

    // ---------- creazione ----------

    // Crea la forma dell'oggetto (o il suo modello, se c'è) come figlia di "genitore". null se l'oggetto è vuoto.
    public static Transform Crea(DatiOggetto o, Transform genitore)
    {
        if (o == null) return null;
        var radice = new GameObject("Forma " + o.name).transform;
        radice.SetParent(genitore, false);
        if (o.modello != null)
        {
            Modello(o, radice);
            return radice;
        }
        switch (o)
        {
            case DatiArma a: Arma(a, radice); break;
            case DatiArmaDistanza d: ArmaDistanza(d, radice); break;
            case DatiScudo s: Scudo(s, radice); break;
            case DatiArmatura v: Armatura(v, radice); break;
            case DatiAmuleto am: Amuleto(am, radice); break;
            case DatiBastone b: Bastone(b, radice); break;
            case DatiLibro l: Libro(l, radice); break;
            case DatiIncantesimo i: Incantesimo(i, radice); break;
            default: P(radice, PrimitiveType.Cube, Vector3.zero, new Vector3(0.25f, 0.4f, 0.08f), Oro); break;
        }
        return radice;
    }

    // Il modello di Nazar al posto della forma. Armi e bastoni sono fatti con l'impugnatura nel punto zero e la lama
    // in alto: qui si girano con la lama in basso, come le forme. Poi le correzioni scritte nell'oggetto e il materiale.
    static void Modello(DatiOggetto o, Transform radice)
    {
        var perno = new GameObject("Modello").transform;
        perno.SetParent(radice, false);
        Quaternion giro = o is DatiArma || o is DatiBastone ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
        perno.localPosition = o.posizioneModello;
        perno.localRotation = giro * Quaternion.Euler(o.rotazioneModello);
        perno.localScale = Vector3.one * (o.scalaModello > 0f ? o.scalaModello : 1f);
        var copia = Object.Instantiate(o.modello, perno, false);
        foreach (var c in copia.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);   // solo da vedere
        if (o.materialeModello == null) return;
        foreach (var r in copia.GetComponentsInChildren<Renderer>())
        {
            var materiali = r.sharedMaterials;
            for (int i = 0; i < materiali.Length; i++) materiali[i] = o.materialeModello;
            r.sharedMaterials = materiali;
        }
    }

    // La spada provvisoria di quando non si ha un'arma (uguale a quella di AspettoUmanoide).
    public static void SpadaSenzaArma(Transform t)
    {
        P(t, PrimitiveType.Cube, new Vector3(0f, -0.04f, 0f), new Vector3(0.22f, 0.04f, 0.06f), Cuoio);
        P(t, PrimitiveType.Cube, new Vector3(0f, -0.46f, 0f), new Vector3(0.06f, 0.8f, 0.03f), Ferro);
    }

    // Il bastone provvisorio di quando lo Stregone non ha un bastone (uguale a quello di AspettoUmanoide).
    public static void BastoneSenzaBastone(Transform t)
    {
        AstaConGemma(t, 1f, 0.05f, Legno, LagoNero, 0.14f);
    }

    // ---------- armi del Guerriero e del Ladro ----------

    static void Arma(DatiArma a, Transform t)
    {
        switch (a.name)
        {
            case "spada-della-vecchia-vita": Spada(t, 0.8f, 0.06f, Ruggine, Cuoio, Cuoio); return;
            case "spada-del-capitano":
                Spada(t, 0.9f, 0.065f, Ferro, Oro, Cuoio);
                P(t, PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0f), Vector3.one * 0.06f, Oro);
                return;
            case "spadone-del-cavaliere":
                P(t, PrimitiveType.Cube, new Vector3(0f, 0.1f, 0f), new Vector3(0.05f, 0.24f, 0.05f), Cuoio);
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.04f, 0f), new Vector3(0.36f, 0.05f, 0.07f), FerroScuro);
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.72f, 0f), new Vector3(0.1f, 1.32f, 0.03f), Ferro);
                return;
            case "ascia-da-legna": Ascia(t, 0.8f, new Vector3(0.2f, 0.16f, 0.03f), Legno, Ruggine); return;
            case "ascia-del-boia":
                Ascia(t, 1.05f, new Vector3(0.36f, 0.34f, 0.035f), LegnoScuro, FerroScuro);
                P(t, PrimitiveType.Cube, new Vector3(-0.07f, -0.95f, 0f), new Vector3(0.08f, 0.08f, 0.05f), FerroScuro);
                return;
            case "martello-di-ossa":
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.38f, 0f), new Vector3(0.06f, 0.8f, 0.06f), Osso);
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.82f, 0f), new Vector3(0.34f, 0.16f, 0.16f), Osso);
                P(t, PrimitiveType.Sphere, new Vector3(0.17f, -0.82f, 0f), Vector3.one * 0.15f, Osso);
                return;
            case "mazza-ferrata":
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.3f, 0f), new Vector3(0.06f, 0.62f, 0.06f), Legno);
                P(t, PrimitiveType.Sphere, new Vector3(0f, -0.68f, 0f), Vector3.one * 0.2f, FerroScuro);
                for (int i = 0; i < 4; i++)
                    P(t, PrimitiveType.Cube, new Vector3(0f, -0.68f, 0f), new Vector3(0.28f, 0.04f, 0.04f), FerroScuro, new Vector3(0f, i * 45f, 0f));
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.68f, 0f), new Vector3(0.04f, 0.28f, 0.04f), FerroScuro);
                return;
            case "pugnale-da-scuoiare": Pugnale(t, 0.28f, 0.055f, Ferro, Cuoio); return;
            case "pugnale-ricurvo-degli-orchi":
                P(t, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.04f, 0.12f, 0.04f), Osso);
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.02f, 0f), new Vector3(0.12f, 0.03f, 0.04f), FerroScuro);
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.14f, 0f), new Vector3(0.06f, 0.22f, 0.02f), FerroScuro);
                P(t, PrimitiveType.Cube, new Vector3(0.04f, -0.3f, 0f), new Vector3(0.05f, 0.16f, 0.02f), FerroScuro, new Vector3(0f, 0f, 28f));
                return;
            case "pugnali-gemelli": Pugnale(t, 0.3f, 0.045f, Ferro, Nero); return;
            case "stiletto-d-ombra":
                Pugnale(t, 0.36f, 0.022f, Ombra * 0.6f, Nero);
                P(t, PrimitiveType.Sphere, new Vector3(0f, 0.11f, 0f), Vector3.one * 0.04f, Ombra, luce: true);
                return;
            // Armi improvvisate della gattabuia (tutte le classi)
            case "osso-lungo":
                P(t, PrimitiveType.Sphere, new Vector3(0f, 0.04f, 0f), new Vector3(0.1f, 0.08f, 0.08f), Osso);
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.22f, 0f), new Vector3(0.05f, 0.46f, 0.05f), Osso);
                P(t, PrimitiveType.Sphere, new Vector3(0.02f, -0.48f, 0f), new Vector3(0.12f, 0.1f, 0.09f), Osso);
                return;
            case "catenaccio":
                for (int i = 0; i < 7; i++)
                    P(t, PrimitiveType.Cube, new Vector3(0f, -0.06f - i * 0.11f, 0f), new Vector3(0.05f, 0.09f, 0.02f), FerroScuro,
                        new Vector3(0f, i % 2 == 0 ? 0f : 90f, 0f));
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.88f, 0f), new Vector3(0.12f, 0.12f, 0.06f), Ruggine);
                P(t, PrimitiveType.Cylinder, new Vector3(0f, -0.79f, 0f), new Vector3(0.08f, 0.02f, 0.08f), FerroScuro, new Vector3(90f, 0f, 0f));
                return;
            case "stiletto-del-tagliagole": Pugnale(t, 0.34f, 0.025f, Ferro, new Color(0.45f, 0.06f, 0.06f)); return;
        }
        switch (a.tipo)
        {
            case DatiArma.Tipo.Spadone: Spada(t, 1.3f, 0.1f, Ferro, FerroScuro, Cuoio); break;
            case DatiArma.Tipo.Ascia: Ascia(t, 0.85f, new Vector3(0.24f, 0.2f, 0.03f), Legno, Ferro); break;
            case DatiArma.Tipo.Mazza:
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.3f, 0f), new Vector3(0.06f, 0.6f, 0.06f), Legno);
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.64f, 0f), new Vector3(0.2f, 0.22f, 0.2f), FerroScuro);
                break;
            case DatiArma.Tipo.Pugnale:
            case DatiArma.Tipo.DoppiPugnali: Pugnale(t, 0.3f, 0.05f, Ferro, Cuoio); break;
            case DatiArma.Tipo.Stiletto: Pugnale(t, 0.34f, 0.025f, Ferro, Cuoio); break;
            default: Spada(t, 0.85f, 0.06f, Ferro, Cuoio, Cuoio); break;
        }
    }

    static void Spada(Transform t, float lama, float larghezza, Color ferro, Color elsa, Color impugnatura)
    {
        P(t, PrimitiveType.Cube, new Vector3(0f, 0.06f, 0f), new Vector3(0.045f, 0.14f, 0.045f), impugnatura);
        P(t, PrimitiveType.Cube, new Vector3(0f, -0.03f, 0f), new Vector3(0.24f, 0.04f, 0.06f), elsa);
        P(t, PrimitiveType.Cube, new Vector3(0f, -0.05f - lama * 0.5f, 0f), new Vector3(larghezza, lama, 0.025f), ferro);
    }

    static void Ascia(Transform t, float manico, Vector3 testa, Color legno, Color ferro)
    {
        P(t, PrimitiveType.Cube, new Vector3(0f, 0.05f - manico * 0.5f, 0f), new Vector3(0.055f, manico, 0.055f), legno);
        P(t, PrimitiveType.Cube, new Vector3(testa.x * 0.5f, 0.05f - manico + testa.y * 0.6f, 0f), testa, ferro);
    }

    static void Pugnale(Transform t, float lama, float larghezza, Color ferro, Color impugnatura)
    {
        P(t, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.04f, 0.12f, 0.04f), impugnatura);
        P(t, PrimitiveType.Cube, new Vector3(0f, -0.02f, 0f), new Vector3(0.1f, 0.025f, 0.04f), FerroScuro);
        P(t, PrimitiveType.Cube, new Vector3(0f, -0.035f - lama * 0.5f, 0f), new Vector3(larghezza, lama, 0.018f), ferro);
    }

    // ---------- archi e balestre (in verticale, centrati) ----------

    static void ArmaDistanza(DatiArmaDistanza d, Transform t)
    {
        switch (d.name)
        {
            case "arco-corto-da-caccia": Arco(t, 0.45f, Legno); return;
            case "arco-lungo-di-tasso": Arco(t, 0.8f, LegnoScuro); return;
            case "arco-d-osso-degli-orchi":
                Arco(t, 0.55f, Osso);
                P(t, PrimitiveType.Cube, new Vector3(0f, 0f, 0f), new Vector3(0.07f, 0.16f, 0.07f), Cuoio);
                return;
            case "balestra-da-posta": Balestra(t, 0.65f, Legno, Ferro); return;
            case "balestra-del-carceriere": Balestra(t, 0.8f, LegnoScuro, FerroScuro); return;
        }
        if (d.tipo == DatiArmaDistanza.Tipo.Balestra) Balestra(t, 0.7f, Legno, Ferro);
        else Arco(t, d.tipo == DatiArmaDistanza.Tipo.ArcoLungo ? 0.8f : 0.5f, Legno);
    }

    // Due bracci inclinati e la corda: alto circa 2 × "braccio".
    static void Arco(Transform t, float braccio, Color colore)
    {
        P(t, PrimitiveType.Cube, Vector3.zero, new Vector3(0.05f, 0.14f, 0.05f), Cuoio);
        P(t, PrimitiveType.Cube, new Vector3(0f, braccio * 0.5f, -0.05f), new Vector3(0.035f, braccio, 0.035f), colore, new Vector3(-12f, 0f, 0f));
        P(t, PrimitiveType.Cube, new Vector3(0f, -braccio * 0.5f, -0.05f), new Vector3(0.035f, braccio, 0.035f), colore, new Vector3(12f, 0f, 0f));
        P(t, PrimitiveType.Cube, new Vector3(0f, 0f, -0.15f), new Vector3(0.008f, braccio * 1.95f, 0.008f), Carta);
    }

    static void Balestra(Transform t, float lunghezza, Color legno, Color ferro)
    {
        P(t, PrimitiveType.Cube, Vector3.zero, new Vector3(0.07f, lunghezza, 0.08f), legno);
        P(t, PrimitiveType.Cube, new Vector3(0f, lunghezza * 0.42f, 0.02f), new Vector3(lunghezza * 0.85f, 0.04f, 0.05f), ferro);
        P(t, PrimitiveType.Cube, new Vector3(0f, lunghezza * 0.3f, 0.02f), new Vector3(lunghezza * 0.8f, 0.01f, 0.01f), Carta);
    }

    // ---------- scudi (sulla mano sinistra, faccia in avanti) ----------

    static void Scudo(DatiScudo s, Transform t)
    {
        switch (s.name)
        {
            case "brocchiere-di-ferro":
                Disco(t, 0.36f, Ferro);
                P(t, PrimitiveType.Sphere, new Vector3(0f, 0f, 0.05f), Vector3.one * 0.1f, FerroScuro);
                return;
            case "pavese-di-quercia":
                P(t, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0.04f), new Vector3(0.55f, 1f, 0.06f), Legno);
                P(t, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0.075f), new Vector3(0.08f, 1f, 0.02f), FerroScuro);
                P(t, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0.075f), new Vector3(0.55f, 0.06f, 0.02f), FerroScuro);
                return;
            default:   // Scudo della vecchia vita e scudi senza forma propria
                Disco(t, 0.5f, Legno);
                P(t, PrimitiveType.Sphere, new Vector3(0f, 0f, 0.05f), Vector3.one * 0.09f, Ruggine);
                return;
        }
    }

    static void Disco(Transform t, float diametro, Color colore)
    {
        P(t, PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.03f), new Vector3(diametro, 0.025f, diametro), colore, new Vector3(90f, 0f, 0f));
    }

    // ---------- armature e vesti (centrate sul busto) ----------

    static void Armatura(DatiArmatura v, Transform t)
    {
        switch (v.name)
        {
            // Guerriero
            case "giubba-di-cuoio": Busto(t, Cuoio, 0.01f); return;
            case "cotta-di-maglia-rattoppata":
                Busto(t, Ferro * 0.85f, 0.02f);
                P(t, PrimitiveType.Cube, new Vector3(0.12f, 0.1f, 0.19f), new Vector3(0.14f, 0.12f, 0.01f), Cuoio);
                P(t, PrimitiveType.Cube, new Vector3(-0.14f, -0.15f, 0.19f), new Vector3(0.1f, 0.1f, 0.01f), Cuoio);
                Gonna(t, 0.22f, Ferro * 0.85f);
                return;
            case "corazza-di-piastre-annerite":
                Busto(t, FerroScuro, 0.04f);
                Spallacci(t, FerroScuro, 0.2f);
                Gonna(t, 0.2f, FerroScuro);
                return;
            // Ladro
            case "corpetto-di-cuoio-rinforzato":
                Busto(t, Cuoio, 0.015f);
                P(t, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0.18f), new Vector3(0.05f, 0.75f, 0.01f), FerroScuro, new Vector3(0f, 0f, 35f));
                return;
            case "giubba-scura":
                Busto(t, Nero * 2f, 0.01f);
                Cappuccio(t, Nero * 2f);
                return;
            case "manto-dell-ombra":
                Busto(t, Nero, 0.01f);
                Cappuccio(t, Nero);
                Mantello(t, Nero, 1.1f);
                return;
            // Stregone
            case "tunica-stracciata":
                Busto(t, Stoffa, 0.01f);
                Gonna(t, 0.55f, Stoffa);
                return;
            case "veste-dell-evocatore":
                Busto(t, new Color(0.12f, 0.16f, 0.3f), 0.015f);
                Gonna(t, 0.75f, new Color(0.12f, 0.16f, 0.3f));
                Cappuccio(t, new Color(0.12f, 0.16f, 0.3f));
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.3f, 0.18f), new Vector3(0.6f, 0.05f, 0.02f), Evocazione * 0.7f);
                return;
            case "manto-di-cenere":
                Busto(t, new Color(0.42f, 0.42f, 0.42f), 0.015f);
                Gonna(t, 0.6f, new Color(0.42f, 0.42f, 0.42f));
                Mantello(t, new Color(0.25f, 0.24f, 0.24f), 1.15f);
                return;
        }
        // Senza forma propria: in base alla classe e al peso.
        if (v.classe == ClasseGiocatore.Stregone) { Busto(t, Stoffa, 0.01f); Gonna(t, 0.55f, Stoffa); }
        else if (v.peso == DatiArmatura.Peso.Pesante) { Busto(t, FerroScuro, 0.04f); Spallacci(t, FerroScuro, 0.2f); }
        else if (v.peso == DatiArmatura.Peso.Media) Busto(t, Ferro * 0.85f, 0.02f);
        else Busto(t, Cuoio, 0.01f);
    }

    // Il busto della figura è 0,56 × 0,7 × 0,32: l'armatura lo ricopre, un po' più grande.
    static void Busto(Transform t, Color c, float spessore)
    {
        P(t, PrimitiveType.Cube, Vector3.zero, new Vector3(0.58f + spessore * 2f, 0.72f, 0.34f + spessore * 2f), c);
    }

    static void Spallacci(Transform t, Color c, float grandezza)
    {
        P(t, PrimitiveType.Cube, new Vector3(-0.38f, 0.32f, 0f), new Vector3(grandezza * 1.2f, grandezza * 0.6f, grandezza * 1.3f), c);
        P(t, PrimitiveType.Cube, new Vector3(0.38f, 0.32f, 0f), new Vector3(grandezza * 1.2f, grandezza * 0.6f, grandezza * 1.3f), c);
    }

    // Gonna o falda sotto il busto, lunga "lunghezza" metri (le vesti arrivano alle ginocchia).
    static void Gonna(Transform t, float lunghezza, Color c)
    {
        P(t, PrimitiveType.Cube, new Vector3(0f, -0.36f - lunghezza * 0.5f, 0f), new Vector3(0.56f, lunghezza, 0.34f), c);
    }

    static void Cappuccio(Transform t, Color c)
    {
        P(t, PrimitiveType.Cube, new Vector3(0f, 0.6f, -0.06f), new Vector3(0.38f, 0.36f, 0.34f), c);
    }

    static void Mantello(Transform t, Color c, float lunghezza)
    {
        P(t, PrimitiveType.Cube, new Vector3(0f, 0.34f - lunghezza * 0.5f, -0.2f), new Vector3(0.62f, lunghezza, 0.03f), c);
    }

    // ---------- amuleti (ciondolo sul petto) ----------

    static void Amuleto(DatiAmuleto am, Transform t)
    {
        // Laccetto che sale verso il collo.
        P(t, PrimitiveType.Cube, new Vector3(0f, 0.09f, 0f), new Vector3(0.012f, 0.16f, 0.012f), Cuoio);
        switch (am.name)
        {
            case "cuore-di-brace": Gemma(t, Brace, 0.07f, true); return;
            case "occhio-di-corvo":
                Gemma(t, Nero, 0.06f, false);
                P(t, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.09f, 0.005f, 0.09f), Oro, new Vector3(90f, 0f, 0f));
                return;
            case "pietra-del-focolare": P(t, PrimitiveType.Cube, Vector3.zero, Vector3.one * 0.07f, new Color(0.7f, 0.35f, 0.15f)); return;
            case "respiro-del-lago": Gemma(t, LagoNero, 0.06f, true); return;
            case "sangue-antico": Gemma(t, new Color(0.45f, 0.03f, 0.05f), 0.065f, true); return;
            case "zanna-di-lupo": Dente(t, Osso); return;
            case "dente-di-vipera": Dente(t, new Color(0.45f, 0.65f, 0.3f)); return;
            case "fiato-del-predatore": P(t, PrimitiveType.Cube, Vector3.zero, new Vector3(0.06f, 0.08f, 0.03f), new Color(0.5f, 0.5f, 0.45f), new Vector3(0f, 0f, 45f)); return;
            case "goccia-di-sangue-nero": P(t, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.05f, 0.08f, 0.05f), new Color(0.25f, 0.02f, 0.04f), luce: true); return;
            case "laccio-del-borsaiolo": P(t, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.07f, 0.05f, 0.04f), Cuoio); return;
            case "piuma-di-civetta": P(t, PrimitiveType.Cube, new Vector3(0f, -0.04f, 0f), new Vector3(0.04f, 0.14f, 0.005f), new Color(0.75f, 0.72f, 0.65f), new Vector3(0f, 0f, 15f)); return;
            case "ultimo-respiro": Gemma(t, new Color(0.75f, 0.7f, 0.95f), 0.06f, true); return;
            case "osso-inciso": P(t, PrimitiveType.Cube, Vector3.zero, new Vector3(0.03f, 0.1f, 0.03f), Osso, new Vector3(0f, 0f, 20f)); return;
            case "cristallo-opaco": P(t, PrimitiveType.Cube, Vector3.zero, Vector3.one * 0.06f, new Color(0.6f, 0.68f, 0.75f), new Vector3(45f, 0f, 45f)); return;
            case "cenere-benedetta": Gemma(t, new Color(0.5f, 0.5f, 0.5f), 0.065f, false); return;
            case "sigillo-del-focolare": P(t, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.08f, 0.01f, 0.08f), Brace * 0.8f, new Vector3(90f, 0f, 0f)); return;
            case "occhio-del-lago":
                Gemma(t, LagoNero, 0.055f, true);
                P(t, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.085f, 0.005f, 0.085f), Ferro, new Vector3(90f, 0f, 0f));
                return;
            case "cuore-del-lago-nero": Gemma(t, new Color(0.1f, 0.15f, 0.45f), 0.07f, true); return;
        }
        Gemma(t, am.tipo == DatiAmuleto.Tipo.Arcano ? Ombra : Oro, 0.06f, am.tipo == DatiAmuleto.Tipo.Arcano);
    }

    static void Gemma(Transform t, Color c, float grandezza, bool luce)
    {
        P(t, PrimitiveType.Sphere, Vector3.zero, Vector3.one * grandezza, c, luce: luce);
    }

    static void Dente(Transform t, Color c)
    {
        P(t, PrimitiveType.Cube, new Vector3(0f, -0.03f, 0f), new Vector3(0.025f, 0.09f, 0.025f), c, new Vector3(0f, 0f, 12f));
    }

    // ---------- bastoni e verghe (impugnatura nel punto zero, asta verso il basso) ----------

    static void Bastone(DatiBastone b, Transform t)
    {
        Color colore = ColoreScuola(ScuolaPiuForte(b.magia), LagoNero);
        switch (b.name)
        {
            case "bastone-della-vecchia-vita": AstaConGemma(t, 1f, 0.05f, Legno, LagoNero, 0.14f); return;
            case "bastone-di-quercia-nera":
                AstaConGemma(t, 1.05f, 0.07f, LegnoScuro, colore, 0.13f);
                P(t, PrimitiveType.Cube, new Vector3(0.03f, -0.35f, 0f), new Vector3(0.09f, 0.08f, 0.09f), LegnoScuro, new Vector3(0f, 20f, 30f));
                P(t, PrimitiveType.Cube, new Vector3(-0.03f, -0.6f, 0f), new Vector3(0.08f, 0.07f, 0.08f), LegnoScuro, new Vector3(0f, 40f, -25f));
                return;
            case "bastone-del-lago":
                AstaConGemma(t, 1f, 0.05f, new Color(0.25f, 0.3f, 0.32f), LagoNero, 0.16f);
                P(t, PrimitiveType.Cylinder, new Vector3(0f, -0.83f, 0f), new Vector3(0.24f, 0.01f, 0.24f), Ferro);
                return;
            case "bastone-d-ossidiana":
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.25f, 0f), new Vector3(0.05f, 1f, 0.05f), Nero);
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.85f, 0f), new Vector3(0.12f, 0.2f, 0.12f), Ombra, new Vector3(0f, 45f, 0f), luce: true);
                return;
            case "bastone-del-cimitero":
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.25f, 0f), new Vector3(0.05f, 1f, 0.05f), Osso * 0.8f);
                P(t, PrimitiveType.Sphere, new Vector3(0f, -0.85f, 0f), new Vector3(0.17f, 0.19f, 0.17f), Osso);
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.85f, 0.07f), new Vector3(0.1f, 0.03f, 0.04f), Evocazione, luce: true);
                return;
            case "verga-d-osso":
                P(t, PrimitiveType.Cube, new Vector3(0f, -0.15f, 0f), new Vector3(0.035f, 0.5f, 0.035f), Osso);
                P(t, PrimitiveType.Sphere, new Vector3(0f, -0.42f, 0f), Vector3.one * 0.07f, colore, luce: true);
                return;
        }
        if (b.tipo == DatiBastone.Tipo.Verga)
        {
            P(t, PrimitiveType.Cube, new Vector3(0f, -0.15f, 0f), new Vector3(0.035f, 0.5f, 0.035f), Legno);
            P(t, PrimitiveType.Sphere, new Vector3(0f, -0.42f, 0f), Vector3.one * 0.07f, colore, luce: true);
        }
        else AstaConGemma(t, 1f, 0.05f, Legno, colore, 0.14f);
    }

    static void AstaConGemma(Transform t, float lunghezza, float spessore, Color legno, Color gemma, float grandezzaGemma)
    {
        P(t, PrimitiveType.Cube, new Vector3(0f, 0.25f - lunghezza * 0.5f, 0f), new Vector3(spessore, lunghezza, spessore), legno);
        P(t, PrimitiveType.Cube, new Vector3(0f, 0.26f - lunghezza, 0f), new Vector3(0.12f, 0.04f, 0.12f), legno);
        P(t, PrimitiveType.Sphere, new Vector3(0f, 0.17f - lunghezza, 0f), Vector3.one * grandezzaGemma, gemma, luce: true);
    }

    // ---------- libri (nella mano sinistra) ----------

    static void Libro(DatiLibro l, Transform t)
    {
        Color copertina;
        switch (l.name)
        {
            case "libro-della-vecchia-vita": copertina = Cuoio; break;
            case "libro-dei-sussurri": copertina = Ombra * 0.5f; break;
            case "libro-delle-braci": copertina = Brace * 0.55f; break;
            case "libro-dell-evocatore": copertina = Evocazione * 0.4f; break;
            case "libro-del-lago-nero": copertina = new Color(0.08f, 0.12f, 0.3f); break;
            default: copertina = ColoreScuola(ScuolaPiuForte(l.magia), Cuoio) * 0.5f; break;
        }
        P(t, PrimitiveType.Cube, new Vector3(0f, -0.08f, 0.06f), new Vector3(0.22f, 0.28f, 0.07f), copertina);
        P(t, PrimitiveType.Cube, new Vector3(0.008f, -0.08f, 0.06f), new Vector3(0.21f, 0.26f, 0.075f), Carta);
        P(t, PrimitiveType.Cube, new Vector3(-0.105f, -0.08f, 0.06f), new Vector3(0.02f, 0.28f, 0.08f), copertina);
    }

    // ---------- incantesimi (pergamena col sigillo della scuola, solo a terra) ----------

    static void Incantesimo(DatiIncantesimo i, Transform t)
    {
        Color colore = ColoreScuola(i.scuola, Oro);
        P(t, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.09f, 0.16f, 0.09f), Carta, new Vector3(0f, 0f, 90f));
        P(t, PrimitiveType.Sphere, new Vector3(0f, 0f, 0.05f), Vector3.one * 0.07f, colore, luce: true);
    }

    // ---------- aiuti ----------

    static Color ColoreScuola(ScuolaMagia? scuola, Color senza)
    {
        if (scuola == null) return senza;
        switch (scuola.Value)
        {
            case ScuolaMagia.Brace: return Brace;
            case ScuolaMagia.LagoNero: return LagoNero;
            case ScuolaMagia.Ombra: return Ombra;
            default: return Evocazione;
        }
    }

    // La scuola a cui l'oggetto dà più danno o durata degli effetti; null se nessuna.
    static ScuolaMagia? ScuolaPiuForte(ModificatoriMagia m)
    {
        if (m == null) return null;
        ScuolaMagia? migliore = null;
        float massimo = 0f;
        foreach (ScuolaMagia sc in System.Enum.GetValues(typeof(ScuolaMagia)))
        {
            var p = m.Di(sc);
            float valore = p.dannoPercento + p.durataEffettiPercento * 0.5f + p.ignoraArmatura * 100f;
            if (valore > massimo) { massimo = valore; migliore = sc; }
        }
        if (migliore == null && (m.vitaEvocazioniPercento > 0f || m.durataEvocazioniPercento > 0f || m.doppiaEvocazione))
            migliore = ScuolaMagia.Evocazione;
        return migliore;
    }

    static void P(Transform genitore, PrimitiveType tipo, Vector3 posizione, Vector3 scala, Color colore,
        Vector3 rotazione = default, bool luce = false)
    {
        var parte = GameObject.CreatePrimitive(tipo);
        parte.name = tipo.ToString();
        Object.DestroyImmediate(parte.GetComponent<Collider>());   // solo da vedere: non deve urtare niente
        parte.transform.SetParent(genitore, false);
        parte.transform.localPosition = posizione;
        parte.transform.localRotation = Quaternion.Euler(rotazione);
        parte.transform.localScale = scala;
        var r = parte.GetComponent<Renderer>();
        r.sharedMaterial = Mat(colore, luce);
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
}
