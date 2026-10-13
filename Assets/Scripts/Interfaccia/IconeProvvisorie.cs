using UnityEngine;

// Icone provvisorie degli oggetti, disegnate con rombi e rettangoli finché Nazar non fa le icone vere
// (richiesta di Lorenzo, 13 ottobre: nell'inventario si vedevano solo le iniziali e gli oggetti non si distinguevano).
// Ogni tipo ha la sua sagoma, e ogni oggetto dello stesso tipo ha un dettaglio diverso:
//   - armi: spada, spadone (lama larga e lunga), ascia, mazza, pugnale, stiletto (sottile), doppi pugnali (incrociati);
//   - archi e balestre: arco corto, arco lungo, balestra;
//   - scudi: grande (pavese alto), medio (a goccia), piccolo (rotella);
//   - armature e vesti: busto con spalle, colore secondo il peso (ferro, ferro scuro, stoffa, cuoio, ombra);
//   - amuleti: catenella e pietra (dorata per i magici, viola scuro per gli arcani);
//   - bastoni e verghe: asta di legno con la gemma del colore della scuola di magia;
//   - libri: copertina del colore della scuola, pagine e fermaglio;
//   - incantesimi: sfera luminosa del colore dell'incantesimo con scintille;
//   - pozioni: boccetta del loro colore; fiori: petali del loro colore; sacchetti: sacco legato.
// Il dettaglio diverso (pietra del pomolo, borchia dello scudo, fibbia dell'armatura...) prende un colore dal nome
// dell'oggetto, sempre lo stesso per lo stesso oggetto.
// Se l'oggetto ha la sua icona (DatiOggetto.icona) si usa quella: queste spariscono da sole quando arrivano le vere.
// Come montarlo: non si monta. La usa GraficaMenu.Icona (inventario, bottega, interfaccia in partita).
public static class IconeProvvisorie
{
    static readonly Color Ferro = new Color(0.72f, 0.74f, 0.78f);
    static readonly Color FerroScuro = new Color(0.38f, 0.39f, 0.43f);
    static readonly Color Legno = new Color(0.45f, 0.3f, 0.17f);
    static readonly Color LegnoScuro = new Color(0.27f, 0.18f, 0.1f);
    static readonly Color Cuoio = new Color(0.45f, 0.28f, 0.15f);
    static readonly Color Oro = new Color(0.86f, 0.7f, 0.3f);
    static readonly Color Stoffa = new Color(0.5f, 0.46f, 0.38f);
    static readonly Color Carta = new Color(0.9f, 0.85f, 0.7f);
    static readonly Color OmbraScura = new Color(0.22f, 0.14f, 0.32f);
    static readonly Color Corda = new Color(0.82f, 0.78f, 0.66f);

    // Colori per il dettaglio di ogni oggetto (scelto dal nome).
    static readonly Color[] dettagli =
    {
        new Color(0.85f, 0.2f, 0.18f), new Color(0.25f, 0.55f, 0.95f), new Color(0.35f, 0.8f, 0.4f),
        new Color(0.9f, 0.75f, 0.25f), new Color(0.65f, 0.35f, 0.9f), new Color(0.95f, 0.5f, 0.2f),
        new Color(0.3f, 0.85f, 0.85f), new Color(0.9f, 0.9f, 0.92f),
    };

    static float alfa;
    static Vector2 c;
    static float s;

    // Disegna l'icona provvisoria dell'oggetto nel riquadro. Falso se per quel tipo non c'è (si scrive l'iniziale).
    public static bool Disegna(Rect r, DatiOggetto o, float alfaIcona)
    {
        if (o == null) return false;
        alfa = alfaIcona;
        c = r.center;
        s = Mathf.Min(r.width, r.height) / 60f;   // le misure qui sotto sono pensate per un riquadro di 60
        Color dettaglio = Dettaglio(o);
        switch (o)
        {
            case DatiConsumabile p: Boccetta(p.colore); return true;
            case DatiOggettoMissione m:
                if (m.forma == DatiOggettoMissione.FormaProvvisoria.Fiore) Fiore(m.colore); else Sacchetto(m.colore);
                return true;
            case DatiArma a: Arma(a, dettaglio); return true;
            case DatiArmaDistanza d: Distanza(d, dettaglio); return true;
            case DatiScudo sc: Scudo(sc, dettaglio); return true;
            case DatiArmatura v: Armatura(v, dettaglio); return true;
            case DatiAmuleto am: Amuleto(am, dettaglio); return true;
            case DatiBastone b: Bastone(b, FormeOggetti.ColoreMagia(b.magia, new Color(0.45f, 0.75f, 1f)), dettaglio); return true;
            case DatiLibro l: Libro(FormeOggetti.ColoreMagia(l.magia, Cuoio), dettaglio); return true;
            case DatiIncantesimo i: Incantesimo(i.colore); return true;
        }
        return false;
    }

    static Color Dettaglio(DatiOggetto o)
    {
        string nome = string.IsNullOrEmpty(o.name) ? o.nomeDiLavoro : o.name;
        int h = 17;
        if (nome != null) foreach (char ch in nome) h = h * 31 + ch;
        return dettagli[(h & 0x7fffffff) % dettagli.Length];
    }

    // ---------- pezzi di base ----------

    static Color A(Color col) => GraficaMenu.Con(col, alfa);

    // Un rettangolo lungo "lunghezza" e largo "spessore", con il centro in "p" (rispetto al centro dell'icona,
    // misure su 60) e girato di "angolo" gradi.
    static void Barra(Vector2 p, float lunghezza, float spessore, float angolo, Color col)
    {
        Vector2 centro = c + p * s;
        Matrix4x4 prima = GUI.matrix;
        GUI.matrix = prima * Matrix4x4.TRS(centro, Quaternion.Euler(0f, 0f, angolo), Vector3.one) * Matrix4x4.TRS(-centro, Quaternion.identity, Vector3.one);
        GraficaMenu.Riempi(new Rect(centro.x - lunghezza * s * 0.5f, centro.y - spessore * s * 0.5f, lunghezza * s, spessore * s), A(col));
        GUI.matrix = prima;
    }

    static void Rombo(Vector2 p, float lato, Color col) => GraficaMenu.Rombo(c + p * s, lato * s, A(col));
    static void Quadro(Vector2 p, float larghezza, float altezza, Color col) =>
        GraficaMenu.Riempi(new Rect(c.x + (p.x - larghezza * 0.5f) * s, c.y + (p.y - altezza * 0.5f) * s, larghezza * s, altezza * s), A(col));

    // Un punto lungo una direzione inclinata (0 gradi = verso destra, i gradi crescono in senso orario come lo schermo).
    static Vector2 Lungo(float angolo, float distanza)
    {
        float r = angolo * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(r), Mathf.Sin(r)) * distanza;
    }

    // ---------- armi ----------

    // Le armi stanno in diagonale, punta in alto a destra (-45 gradi).
    static void Arma(DatiArma a, Color dettaglio)
    {
        const float giu = -45f;
        switch (a.tipo)
        {
            case DatiArma.Tipo.Spadone: Lama(giu, 48f, 8f, 14f, dettaglio, Vector2.zero); break;
            case DatiArma.Tipo.Pugnale: Lama(giu, 30f, 6f, 9f, dettaglio, Vector2.zero); break;
            case DatiArma.Tipo.Stiletto: Lama(giu, 32f, 3f, 8f, dettaglio, Vector2.zero); break;
            case DatiArma.Tipo.DoppiPugnali:
                Lama(giu, 28f, 5f, 8f, dettaglio, new Vector2(-7f, 0f));
                Lama(-135f, 28f, 5f, 8f, dettaglio, new Vector2(7f, 0f));
                break;
            case DatiArma.Tipo.Ascia:
                Barra(Lungo(giu, 2f), 50f, 4f, giu, Legno);
                // testa dell'ascia: lama larga da un lato del manico, in alto
                Vector2 testa = Lungo(giu, 16f);
                Barra(testa + Lungo(giu + 90f, 7f), 16f, 12f, giu, Ferro);
                Barra(testa + Lungo(giu + 90f, 13f), 18f, 3f, giu, new Color(0.9f, 0.92f, 0.95f));   // filo
                Rombo(Lungo(giu, -22f), 5f, dettaglio);
                break;
            case DatiArma.Tipo.Mazza:
                Barra(Lungo(giu, -2f), 44f, 4f, giu, Legno);
                Vector2 capo = Lungo(giu, 16f);
                Rombo(capo, 18f, FerroScuro);
                Rombo(capo, 11f, Ferro);
                for (int k = 0; k < 4; k++) Rombo(capo + Lungo(giu + k * 90f, 11f), 5f, Ferro);   // borchie
                Rombo(capo, 4f, dettaglio);
                break;
            default: Lama(giu, 40f, 6f, 13f, dettaglio, Vector2.zero); break;   // spada
        }
    }

    // Lama con guardia, impugnatura e pomolo del colore del dettaglio. "spostamento" sposta tutta l'arma.
    static void Lama(float angolo, float lunghezza, float larghezza, float guardia, Color dettaglio, Vector2 spostamento)
    {
        Vector2 o = spostamento;
        float manico = lunghezza * 0.3f;
        Vector2 baseLama = o + Lungo(angolo, -lunghezza * 0.2f);
        Barra(baseLama + Lungo(angolo, lunghezza * 0.5f), lunghezza, larghezza, angolo, Ferro);
        Barra(baseLama + Lungo(angolo, lunghezza * 0.5f), lunghezza * 0.85f, Mathf.Max(1f, larghezza * 0.3f), angolo, new Color(0.9f, 0.92f, 0.96f));
        Rombo(baseLama + Lungo(angolo, lunghezza), larghezza * 0.9f, Ferro);   // punta
        Barra(baseLama, guardia, 3f, angolo + 90f, Oro);
        Barra(baseLama + Lungo(angolo, -manico * 0.5f), manico, 3.5f, angolo, Cuoio);
        Rombo(baseLama + Lungo(angolo, -manico), 6f, dettaglio);
    }

    static void Distanza(DatiArmaDistanza d, Color dettaglio)
    {
        if (d.tipo == DatiArmaDistanza.Tipo.Balestra)
        {
            Barra(new Vector2(0f, 4f), 40f, 6f, 90f, Legno);                   // calcio
            Arco(new Vector2(0f, -10f), 22f, 8f, true, LegnoScuro);            // arco orizzontale
            Barra(new Vector2(0f, -10f), 44f, 1.5f, 0f, Corda);                // corda
            Barra(new Vector2(0f, -16f), 3f, 18f, 0f, Ferro);                  // dardo
            Rombo(new Vector2(0f, 22f), 6f, dettaglio);
            return;
        }
        bool lungo = d.tipo == DatiArmaDistanza.Tipo.ArcoLungo;
        float meta = lungo ? 26f : 20f;
        Arco(new Vector2(-4f, 0f), meta, lungo ? 9f : 11f, false, Legno);
        Barra(new Vector2(-4f - (lungo ? 9f : 11f) + 2f, 0f), 1.5f, meta * 2f, 0f, Corda);   // corda
        Quadro(new Vector2(4f, 0f), 4f, 8f, dettaglio);                                          // impugnatura
        Barra(new Vector2(4f, 0f), 34f, 2f, -10f, Legno);                                        // freccia
        Rombo(new Vector2(4f, 0f) + Lungo(-10f, 18f), 5f, Ferro);
    }

    // Arco fatto di 6 pezzi lungo una curva. Verticale (o orizzontale) con la pancia verso destra (o in alto).
    static void Arco(Vector2 centro, float meta, float pancia, bool orizzontale, Color col)
    {
        const int pezzi = 6;
        for (int k = 0; k < pezzi; k++)
        {
            float t0 = -1f + 2f * k / pezzi, t1 = -1f + 2f * (k + 1) / pezzi;
            Vector2 a = new Vector2(pancia * (1f - t0 * t0), t0 * meta);
            Vector2 b = new Vector2(pancia * (1f - t1 * t1), t1 * meta);
            if (orizzontale) { a = new Vector2(a.y, -a.x); b = new Vector2(b.y, -b.x); }
            Vector2 d = b - a;
            Barra(centro + (a + b) * 0.5f, d.magnitude + 1.5f, 4f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, col);
        }
    }

    // ---------- difesa ----------

    static void Scudo(DatiScudo sc, Color dettaglio)
    {
        switch (sc.taglia)
        {
            case DatiScudo.Taglia.Grande:   // pavese: alto e dritto
                Quadro(Vector2.zero, 34f, 46f, FerroScuro);
                Quadro(Vector2.zero, 28f, 40f, Legno);
                Quadro(Vector2.zero, 5f, 40f, dettaglio);
                Quadro(Vector2.zero, 28f, 5f, dettaglio);
                break;
            case DatiScudo.Taglia.Piccolo:  // rotella: piccola e tonda
                Rombo(Vector2.zero, 30f, FerroScuro);
                Quadro(Vector2.zero, 30f, 30f, FerroScuro);
                Quadro(Vector2.zero, 24f, 24f, Ferro);
                Rombo(Vector2.zero, 12f, dettaglio);
                break;
            default:                        // medio: a goccia, largo in alto
                Quadro(new Vector2(0f, -6f), 38f, 24f, FerroScuro);
                Rombo(new Vector2(0f, 6f), 30f, FerroScuro);
                Quadro(new Vector2(0f, -6f), 32f, 19f, Legno);
                Rombo(new Vector2(0f, 6f), 24f, Legno);
                Rombo(new Vector2(0f, -2f), 10f, dettaglio);
                break;
        }
    }

    static void Armatura(DatiArmatura v, Color dettaglio)
    {
        Color col;
        switch (v.peso)
        {
            case DatiArmatura.Peso.Pesante: col = Ferro; break;
            case DatiArmatura.Peso.Media: col = FerroScuro; break;
            case DatiArmatura.Peso.Leggera: col = Stoffa; break;
            case DatiArmatura.Peso.Cuoio: col = Cuoio; break;
            default: col = OmbraScura; break;
        }
        // vesti dello Stregone: più lunghe, con il colore della magia sull'orlo
        bool veste = v.classe == ClasseGiocatore.Stregone;
        Color bordo = veste ? FormeOggetti.ColoreMagia(v.magia, dettaglio) : col * 0.7f;
        bordo.a = 1f;
        Quadro(new Vector2(0f, -14f), 38f, 10f, col);                       // spalle
        if (veste)
        {
            // veste lunga che si allarga verso il basso, con le maniche larghe
            for (int k = 0; k < 5; k++) Quadro(new Vector2(0f, -4f + k * 7f), 24f + k * 4f, 8f, col);
            Quadro(new Vector2(-19f, -4f), 8f, 16f, col);
            Quadro(new Vector2(19f, -4f), 8f, 16f, col);
            Quadro(new Vector2(0f, 28f), 42f, 3f, bordo);                   // orlo
            Quadro(new Vector2(0f, 6f), 4f, 40f, bordo * 0.85f + new Color(0f, 0f, 0f, 0.15f));   // bordura davanti
        }
        else
        {
            Quadro(new Vector2(0f, 2f), 26f, 32f, col);                     // busto
            Quadro(new Vector2(0f, 18f), 26f, 3f, bordo);                   // cintura
        }
        Quadro(new Vector2(0f, -10f), 8f, 6f, bordo * 0.8f);                // scollo
        Rombo(new Vector2(0f, veste ? 0f : 4f), 7f, dettaglio);             // fibbia
        if (v.peso == DatiArmatura.Peso.Pesante)
            for (int k = -1; k <= 1; k += 2) Rombo(new Vector2(k * 17f, -14f), 7f, FerroScuro);   // spallacci
    }

    static void Amuleto(DatiAmuleto am, Color dettaglio)
    {
        bool arcano = am.tipo == DatiAmuleto.Tipo.Arcano;
        Barra(new Vector2(-9f, -14f), 24f, 2f, 60f, Oro);   // catenella
        Barra(new Vector2(9f, -14f), 24f, 2f, -60f, Oro);
        // arcani: montatura scura e pietra più cupa, ma ognuno con il suo colore
        Rombo(new Vector2(0f, 6f), 26f, arcano ? OmbraScura : Oro);
        Rombo(new Vector2(0f, 6f), 18f, arcano ? Color.Lerp(dettaglio, new Color(0.3f, 0.12f, 0.45f), 0.45f) : dettaglio);
        if (arcano) Rombo(new Vector2(0f, 6f), 6f, new Color(0.08f, 0.05f, 0.12f));   // occhio scuro al centro
        Rombo(new Vector2(-3f, 3f), 5f, new Color(1f, 1f, 1f, 0.55f));   // riflesso
    }

    // ---------- magia ----------

    static void Bastone(DatiBastone b, Color gemma, Color dettaglio)
    {
        bool verga = b.tipo == DatiBastone.Tipo.Verga;
        const float angolo = -60f;
        float lunghezza = verga ? 38f : 54f;
        Barra(Lungo(angolo, -4f), lunghezza, verga ? 3f : 4.5f, angolo, verga ? LegnoScuro : Legno);
        Vector2 punta = Lungo(angolo, lunghezza * 0.5f - 4f);
        GraficaMenu.Alone(new Rect(c.x + (punta.x - 14f) * s, c.y + (punta.y - 14f) * s, 28f * s, 28f * s), A(new Color(gemma.r, gemma.g, gemma.b, 0.5f)));
        Rombo(punta, verga ? 9f : 12f, gemma);
        Rombo(punta, verga ? 4f : 5f, Color.white);
        // ghiera e legature del colore del dettaglio: bastoni della stessa scuola restano diversi
        Barra(punta + Lungo(angolo, verga ? -6f : -8f), 6f, verga ? 5f : 7f, angolo, dettaglio);
        Barra(Lungo(angolo, -4f), 4f, verga ? 5f : 6.5f, angolo, dettaglio);
    }

    static void Libro(Color copertina, Color dettaglio)
    {
        Color scura = copertina * 0.55f;
        scura.a = 1f;
        Quadro(new Vector2(3f, 3f), 32f, 42f, Carta);         // pagine
        Quadro(new Vector2(-1f, 0f), 32f, 42f, scura);        // copertina
        Quadro(new Vector2(-14f, 0f), 5f, 42f, scura * 0.7f + new Color(0f, 0f, 0f, 0.3f));   // dorso
        Rombo(new Vector2(1f, -3f), 14f, copertina);
        Rombo(new Vector2(1f, -3f), 6f, dettaglio);
        Quadro(new Vector2(14f, 4f), 6f, 6f, Oro);            // fermaglio
    }

    static void Incantesimo(Color col)
    {
        GraficaMenu.Alone(new Rect(c.x - 26f * s, c.y - 26f * s, 52f * s, 52f * s), A(new Color(col.r, col.g, col.b, 0.6f)));
        Rombo(Vector2.zero, 22f, col);
        Rombo(Vector2.zero, 12f, Color.Lerp(col, Color.white, 0.6f));
        for (int k = 0; k < 4; k++) Rombo(Lungo(45f + k * 90f, 18f), 5f, col);   // scintille
    }

    // ---------- consumabili e missioni ----------

    static void Boccetta(Color liquido)
    {
        Rombo(new Vector2(0f, 6f), 34f, liquido);
        Rombo(new Vector2(-5f, 1f), 8f, new Color(1f, 1f, 1f, 0.35f));   // riflesso
        Quadro(new Vector2(0f, -17f), 8f, 14f, new Color(0.75f, 0.8f, 0.82f, 0.9f));
        Quadro(new Vector2(0f, -25.5f), 12f, 5f, new Color(0.45f, 0.32f, 0.2f));
    }

    static void Fiore(Color petali)
    {
        Quadro(new Vector2(0f, 13f), 4f, 26f, new Color(0.3f, 0.45f, 0.22f));
        for (int k = 0; k < 4; k++) Rombo(new Vector2(0f, -6f) + Lungo(k * 90f, 9f), 14f, petali);
        Rombo(new Vector2(0f, -6f), 8f, new Color(1f, 0.9f, 0.6f));
    }

    static void Sacchetto(Color stoffa)
    {
        Rombo(new Vector2(0f, 8f), 34f, stoffa);
        Quadro(new Vector2(0f, 12f), 30f, 16f, stoffa);
        Quadro(new Vector2(0f, -12f), 10f, 6f, stoffa * 0.8f + new Color(0f, 0f, 0f, 0.2f));
        Barra(new Vector2(0f, -8f), 16f, 2.5f, 0f, Corda);   // laccio
    }
}
