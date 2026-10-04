using System.Collections.Generic;
using UnityEngine;

// Tutti i suoni provvisori del gioco.
public enum Suono
{
    PassoErba, PassoTerra, PassoPietra, PassoLegno,
    Fendente, Schivata, ImpattoColpo, Parata, GuardiaRotta, Colpito, Morte, Rinascita,
    ColpoMuro, CrolloMuro, Leva, PortaPietra, FuocoAcceso, ScattoTrappola, Spuntoni, MorteNemico,
    Raccolta, Serratura, BauleAperto, CambioArma, SferaLancio, SferaImpatto, Negato
}

// Suoni provvisori creati direttamente dal codice, senza file audio.
// A cosa serve: dà un suono a passi, attacchi, schivate, colpi, morte e interazioni, finché il team
// non avrà suoni veri. I suoni sono originali (rumore e toni calcolati qui), quindi non ci sono
// problemi di licenza, e hanno un tono lo-fi adatto allo stile PS2. Vengono creati la prima volta
// che servono e poi riusati; ognuno suona nel punto della scena da cui arriva (più piano da lontano).
// Come si usa (dagli altri script): Suoni.Suona(Suono.Leva, transform.position);
// Come montarlo: non serve montarlo. L'oggetto con le sorgenti audio si crea da solo durante il Play.
// L'ascoltatore è l'Audio Listener della Main Camera, che c'è già.
public static class Suoni
{
    // Bassa frequenza di campionamento: suono un po' "sporco", da gioco di quegli anni (e meno memoria).
    const int Frequenza = 22050;
    const int NumeroSorgenti = 12;

    // Volume di tutti i suoni (da 0 a 1).
    public static float VolumeGenerale = 0.8f;

    static readonly Dictionary<int, AudioClip> archivio = new Dictionary<int, AudioClip>();
    static GameObject gestore;
    static AudioSource[] sorgenti;
    static int prossima;

    // Suona un suono nel punto indicato. tono > 1 più acuto, < 1 più grave.
    // Ogni volta cambia un poco tono e variante, così non sembra sempre identico.
    public static void Suona(Suono suono, Vector3 posizione, float volume = 1f, float tono = 1f)
    {
        AudioClip clip = Prendi(suono, Random.Range(0, Varianti(suono)));
        if (clip == null) return;

        AudioSource sorgente = ProssimaSorgente();
        sorgente.transform.position = posizione;
        sorgente.clip = clip;
        sorgente.volume = Mathf.Clamp01(volume * VolumeGenerale);
        sorgente.pitch = tono * Random.Range(0.94f, 1.06f);
        sorgente.Play();
    }

    // Un piccolo gruppo di sorgenti audio usate a turno, così più suoni possono sovrapporsi.
    static AudioSource ProssimaSorgente()
    {
        if (gestore == null)
        {
            gestore = new GameObject("Suoni (creati dal codice)");
            sorgenti = new AudioSource[NumeroSorgenti];
            for (int i = 0; i < NumeroSorgenti; i++)
            {
                var oggetto = new GameObject("Sorgente " + i);
                oggetto.transform.SetParent(gestore.transform, false);
                AudioSource s = oggetto.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 1f; // suono 3D
                s.rolloffMode = AudioRolloffMode.Logarithmic;
                s.minDistance = 3f;
                s.maxDistance = 50f;
                s.dopplerLevel = 0f;
                sorgenti[i] = s;
            }
        }
        prossima = (prossima + 1) % NumeroSorgenti;
        return sorgenti[prossima];
    }

    static int Varianti(Suono suono)
    {
        switch (suono)
        {
            case Suono.PassoErba:
            case Suono.PassoTerra:
            case Suono.PassoPietra:
            case Suono.PassoLegno:
                return 4;
            case Suono.Fendente:
            case Suono.Schivata:
            case Suono.Colpito:
            case Suono.ColpoMuro:
                return 2;
            case Suono.ImpattoColpo:
                return 3;
            default:
                return 1;
        }
    }

    static AudioClip Prendi(Suono suono, int variante)
    {
        int chiave = (int)suono * 16 + variante;
        if (archivio.TryGetValue(chiave, out AudioClip esistente) && esistente != null) return esistente;

        float[] dati = Genera(suono, new System.Random(chiave * 7919 + 13));
        Normalizza(dati, 0.85f);
        AudioClip clip = AudioClip.Create(suono + " " + variante, dati.Length, 1, Frequenza, false);
        clip.SetData(dati, 0);
        archivio[chiave] = clip;
        return clip;
    }

    // ---------- Come è fatto ogni suono ----------

    static float[] Genera(Suono suono, System.Random caso)
    {
        switch (suono)
        {
            case Suono.PassoErba: return PassoErba(caso);
            case Suono.PassoTerra: return PassoTerra(caso);
            case Suono.PassoPietra: return PassoPietra(caso);
            case Suono.PassoLegno: return PassoLegno(caso);
            case Suono.Fendente: return Sibilo(caso, 0.28f, 0.04f, 0.35f, 0.9f);
            case Suono.Schivata: return Schivata(caso);
            case Suono.ImpattoColpo: return Tonfo(caso, 0.25f, 110f, 55f, 18f, 0.6f);
            case Suono.Parata: return Metallo(caso, 0.8f, new[] { 820f, 1340f, 2050f, 2890f }, new[] { 7f, 9f, 12f, 15f });
            case Suono.GuardiaRotta: return GuardiaRotta(caso);
            case Suono.Colpito: return Colpito(caso);
            case Suono.Morte: return Morte(caso);
            case Suono.Rinascita: return Rinascita();
            case Suono.ColpoMuro: return ColpoMuro(caso);
            case Suono.CrolloMuro: return CrolloMuro(caso);
            case Suono.Leva: return Leva(caso);
            case Suono.PortaPietra: return PortaPietra(caso);
            case Suono.FuocoAcceso: return FuocoAcceso(caso);
            case Suono.ScattoTrappola: return Scatto(caso);
            case Suono.Spuntoni: return Spuntoni(caso);
            case Suono.MorteNemico: return MorteNemico(caso);
            case Suono.Raccolta: return Raccolta();
            case Suono.Serratura: return Serratura(caso);
            case Suono.BauleAperto: return BauleAperto(caso);
            case Suono.CambioArma: return CambioArma(caso);
            case Suono.SferaLancio: return SferaLancio(caso);
            case Suono.SferaImpatto: return SferaImpatto(caso);
            case Suono.Negato: return Negato();
        }
        return Vuoto(0.1f);
    }

    // Passo sull'erba: fruscio morbido, con un secondo fruscio più piccolo subito dopo.
    static float[] PassoErba(System.Random caso)
    {
        float[] d = Vuoto(0.16f);
        float basso = 0f, lento = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            basso += 0.25f * (Rumore(caso) - basso);
            lento += 0.02f * (basso - lento);
            float busta = Busta(t, 0.004f, 30f) + 0.5f * Busta(t - 0.045f, 0.004f, 40f);
            d[i] = (basso - lento) * busta;
        }
        return d;
    }

    // Passo sul sentiero: terra battuta con qualche sassolino che scricchiola.
    static float[] PassoTerra(System.Random caso)
    {
        float[] d = Vuoto(0.12f);
        float basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float grana = caso.NextDouble() < 0.08 ? Rumore(caso) * 2.5f : 0f;
            basso += 0.5f * (Rumore(caso) * 0.5f + grana - basso);
            d[i] = basso * Busta(t, 0.002f, 40f);
        }
        return d;
    }

    // Passo sulla pietra: schiocco secco, un po' di rumore acuto e un colpo basso.
    static float[] PassoPietra(System.Random caso)
    {
        float[] d = Vuoto(0.12f);
        float basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float n = Rumore(caso);
            basso += 0.3f * (n - basso);
            d[i] = Seno(1600f, t) * Busta(t, 0.0005f, 250f) * 0.6f
                 + (n - basso) * Busta(t, 0.001f, 70f) * 0.6f
                 + Seno(140f, t) * Busta(t, 0.002f, 45f) * 0.5f;
        }
        return d;
    }

    // Passo sul legno: colpo sordo che risuona un poco.
    static float[] PassoLegno(System.Random caso)
    {
        float[] d = Vuoto(0.2f);
        float basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            basso += 0.6f * (Rumore(caso) - basso);
            d[i] = Seno(170f, t) * Busta(t, 0.002f, 22f)
                 + Seno(340f, t) * Busta(t, 0.002f, 35f) * 0.5f
                 + basso * Busta(t, 0.001f, 120f) * 0.4f;
        }
        return d;
    }

    // Sibilo d'aria (per i fendenti): rumore filtrato che si apre e si richiude.
    static float[] Sibilo(System.Random caso, float durata, float filtroBase, float filtroVariazione, float volume)
    {
        float[] d = Vuoto(durata);
        float a = 0f, b = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float onda = Mathf.Sin(Mathf.PI * t / durata);
            float filtro = filtroBase + filtroVariazione * onda;
            a += filtro * (Rumore(caso) - a);
            b += filtro * 0.3f * (a - b);
            d[i] = (a - b) * onda * onda * volume;
        }
        return d;
    }

    // Schivata: sibilo veloce più un fruscio di vestiti.
    static float[] Schivata(System.Random caso)
    {
        float[] d = Sibilo(caso, 0.22f, 0.02f, 0.2f, 0.8f);
        float basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            basso += 0.12f * (Rumore(caso) - basso);
            d[i] += basso * Busta(T(i), 0.01f, 14f) * 0.6f;
        }
        return d;
    }

    // Colpo sordo con il tono che scende (impatti, cadute).
    static float[] Tonfo(System.Random caso, float durata, float tonoInizio, float tonoFine, float decadimento, float rumore)
    {
        float[] d = Vuoto(durata);
        float fase = 0f, basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            fase += 2f * Mathf.PI * Mathf.Lerp(tonoInizio, tonoFine, t / durata) / Frequenza;
            basso += 0.3f * (Rumore(caso) - basso);
            d[i] = Mathf.Sin(fase) * Busta(t, 0.002f, decadimento) * 0.8f + basso * Busta(t, 0.001f, 40f) * rumore;
        }
        return d;
    }

    // Suono metallico: alcune frequenze che si spengono a velocità diverse, più uno schiocco iniziale.
    static float[] Metallo(System.Random caso, float durata, float[] toni, float[] decadimenti)
    {
        float[] d = Vuoto(durata);
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float somma = 0f;
            for (int k = 0; k < toni.Length; k++) somma += Seno(toni[k], t) * Busta(t, 0.0005f, decadimenti[k]) / (k + 1f);
            d[i] = somma + Rumore(caso) * Busta(t, 0f, 200f) * 0.5f;
        }
        return d;
    }

    static float[] GuardiaRotta(System.Random caso)
    {
        float[] metallo = Metallo(caso, 0.9f, new[] { 520f, 870f, 1500f }, new[] { 6f, 8f, 11f });
        float[] tonfo = Tonfo(caso, 0.9f, 80f, 45f, 12f, 0.4f);
        for (int i = 0; i < metallo.Length; i++) metallo[i] = metallo[i] * 0.8f + tonfo[i];
        return metallo;
    }

    // Lamento corto quando il giocatore viene colpito: un ronzio basso che scende, filtrato.
    static float[] Colpito(System.Random caso)
    {
        float[] d = Vuoto(0.24f);
        float fase = 0f, filtrato = 0f;
        float tono = 150f + (float)caso.NextDouble() * 20f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            fase += Mathf.Lerp(tono, tono * 0.7f, t / 0.24f) / Frequenza;
            float dente = 2f * (fase - Mathf.Floor(fase)) - 1f;
            filtrato += 0.15f * (dente - filtrato);
            d[i] = filtrato * Busta(t, 0.01f, 12f) + Seno(90f, t) * Busta(t, 0.002f, 30f) * 0.4f;
        }
        return d;
    }

    // Morte: il tonfo del corpo e un suono cupo che scende piano.
    static float[] Morte(System.Random caso)
    {
        float durata = 1.6f;
        float[] d = Vuoto(durata);
        float fase = 0f, filtrato = 0f, basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            fase += Mathf.Lerp(220f, 55f, Mathf.Clamp01(t / 1.4f)) / Frequenza;
            float dente = 2f * (fase - Mathf.Floor(fase)) - 1f;
            filtrato += 0.08f * (dente - filtrato);
            basso += 0.2f * (Rumore(caso) - basso);
            d[i] = Seno(60f, t) * Busta(t, 0.003f, 10f)
                 + basso * Busta(t, 0.001f, 25f) * 0.5f
                 + (filtrato * 0.7f + Mathf.Sin(fase * 2f * Mathf.PI) * 0.3f) * Busta(t, 0.05f, 1.8f) * 0.7f;
        }
        return d;
    }

    // Rinascita: quattro note che salgono, dolci.
    static float[] Rinascita()
    {
        float[] d = Vuoto(1.6f);
        float[] note = { 392f, 494f, 587f, 784f };
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float somma = 0f;
            for (int k = 0; k < note.Length; k++)
            {
                float inizio = k * 0.12f;
                somma += (Seno(note[k], t) + Seno(note[k] * 2f, t) * 0.2f) * Busta(t - inizio, 0.01f, 3f) * 0.4f;
            }
            d[i] = somma;
        }
        return d;
    }

    // Colpo su un muro di pietra: tonfo basso e qualche briciola che cade.
    static float[] ColpoMuro(System.Random caso)
    {
        float[] d = Tonfo(caso, 0.4f, 75f, 50f, 16f, 0.3f);
        float basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float grana = caso.NextDouble() < 0.05 ? Rumore(caso) * 3f : 0f;
            basso += 0.35f * (grana - basso);
            d[i] += basso * Busta(T(i), 0.01f, 9f) * 0.5f;
        }
        return d;
    }

    // Crollo: un boato lungo con tanti colpi di pietre che cadono.
    static float[] CrolloMuro(System.Random caso)
    {
        float durata = 1.8f;
        float[] d = Vuoto(durata);
        float basso = 0f;
        float[] colpi = new float[10];
        float[] toni = new float[10];
        for (int k = 0; k < colpi.Length; k++)
        {
            colpi[k] = (float)caso.NextDouble() * 1.2f;
            toni[k] = 60f + (float)caso.NextDouble() * 60f;
        }
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            basso += 0.04f * (Rumore(caso) - basso);
            float somma = basso * Busta(t, 0.05f, 1.6f) * 3f;
            for (int k = 0; k < colpi.Length; k++) somma += Seno(toni[k], t) * Busta(t - colpi[k], 0.002f, 20f) * 0.6f;
            d[i] = somma;
        }
        return d;
    }

    // Leva: tre scatti metallici e poi il colpo della leva che arriva in fondo.
    static float[] Leva(System.Random caso)
    {
        float[] d = Vuoto(0.55f);
        float[] scatti = { 0f, 0.06f, 0.12f };
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float somma = 0f;
            foreach (float s in scatti) somma += (Rumore(caso) * Busta(t - s, 0f, 300f) * 0.5f + Seno(2500f, t) * Busta(t - s, 0f, 400f) * 0.4f);
            somma += (Seno(300f, t) + Seno(620f, t) * 0.6f) * Busta(t - 0.2f, 0.002f, 14f) * 0.7f;
            somma += Seno(90f, t) * Busta(t - 0.2f, 0.002f, 25f) * 0.6f;
            d[i] = somma;
        }
        return d;
    }

    // Porta di pietra che scorre: rumore grave e ruvido, con un ronzio basso.
    static float[] PortaPietra(System.Random caso)
    {
        float durata = 1.6f;
        float[] d = Vuoto(durata);
        float basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            basso += 0.06f * (Rumore(caso) - basso);
            float ruvido = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 7f * t + Mathf.Sin(2f * Mathf.PI * 1.3f * t));
            float dissolvenza = Mathf.Min(1f, t / 0.15f) * Mathf.Clamp01((durata - t) / 0.3f);
            d[i] = (basso * ruvido * 3f + Seno(55f, t) * 0.3f) * dissolvenza;
        }
        return d;
    }

    // Fuoco che si accende: soffio che sale, qualche scoppiettio e due note leggere.
    static float[] FuocoAcceso(System.Random caso)
    {
        float durata = 1f;
        float[] d = Vuoto(durata);
        float basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            basso += (0.02f + 0.3f * Mathf.Clamp01(t / 0.5f)) * (Rumore(caso) - basso);
            float soffio = basso * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 0.7f)) * 0.8f;
            float scoppio = caso.NextDouble() < 0.004 ? Rumore(caso) * 2f : 0f;
            float note = (Seno(660f, t) + Seno(990f, t) * 0.6f) * Busta(t - 0.25f, 0.01f, 3f) * 0.25f;
            d[i] = soffio + scoppio * Busta(t, 0f, 2f) + note;
        }
        return d;
    }

    // Scatto della trappola: un clic meccanico.
    static float[] Scatto(System.Random caso)
    {
        float[] d = Vuoto(0.08f);
        float basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float n = Rumore(caso);
            basso += 0.3f * (n - basso);
            d[i] = Seno(2200f, t) * Busta(t, 0f, 250f) + (n - basso) * Busta(t, 0f, 150f) * 0.4f;
        }
        return d;
    }

    // Spuntoni che escono: strisciata metallica con un colpo secco.
    static float[] Spuntoni(System.Random caso)
    {
        float[] d = Vuoto(0.35f);
        float basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float n = Rumore(caso);
            basso += 0.2f * (n - basso);
            d[i] = (n - basso) * Busta(t, 0.002f, 14f) * 0.6f
                 + (Seno(1500f, t) + Seno(2300f, t)) * Busta(t, 0.001f, 20f) * 0.3f
                 + Seno(100f, t) * Busta(t, 0.001f, 30f) * 0.5f;
        }
        return d;
    }

    // Morte di un nemico: tonfo e un lamento grave.
    static float[] MorteNemico(System.Random caso)
    {
        float durata = 0.9f;
        float[] d = Vuoto(durata);
        float fase = 0f, filtrato = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            fase += Mathf.Lerp(140f, 70f, Mathf.Clamp01(t / 0.7f)) / Frequenza;
            float dente = 2f * (fase - Mathf.Floor(fase)) - 1f;
            filtrato += 0.1f * (dente - filtrato);
            d[i] = Seno(65f, t) * Busta(t, 0.003f, 9f) + filtrato * Busta(t, 0.03f, 4f) * 0.5f;
        }
        return d;
    }

    // Oggetto raccolto: due note brillanti veloci.
    static float[] Raccolta()
    {
        float[] d = Vuoto(0.7f);
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            d[i] = (Seno(880f, t) + Seno(1760f, t) * 0.3f) * Busta(t, 0.005f, 6f) * 0.5f
                 + (Seno(1318f, t) + Seno(2636f, t) * 0.3f) * Busta(t - 0.09f, 0.005f, 5f) * 0.5f;
        }
        return d;
    }

    // Chiave che gira nella serratura: due scatti metallici e un clac.
    static float[] Serratura(System.Random caso)
    {
        float[] d = Vuoto(0.45f);
        float[] scatti = { 0f, 0.12f, 0.25f };
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float somma = 0f;
            foreach (float s in scatti) somma += Rumore(caso) * Busta(t - s, 0f, 350f) * 0.5f + Seno(1900f, t) * Busta(t - s, 0f, 300f) * 0.4f;
            somma += Seno(420f, t) * Busta(t - 0.25f, 0.001f, 25f) * 0.6f;
            d[i] = somma;
        }
        return d;
    }

    // Baule che si apre: cigolio del legno e un luccichio.
    static float[] BauleAperto(System.Random caso)
    {
        float durata = 1.2f;
        float[] d = Vuoto(durata);
        float fase = 0f, basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            float cigolio = 300f + 200f * Mathf.Sin(2f * Mathf.PI * 3f * t);
            fase += cigolio / Frequenza;
            float dente = 2f * (fase - Mathf.Floor(fase)) - 1f;
            basso += 0.2f * (dente * (0.5f + 0.5f * Rumore(caso)) - basso);
            float legno = basso * Mathf.Clamp01(t / 0.05f) * Mathf.Clamp01((0.6f - t) / 0.2f);
            float luccichio = (Seno(1568f, t) + Seno(2093f, t) * 0.7f + Seno(2637f, t) * 0.5f) * Busta(t - 0.55f, 0.01f, 3f) * 0.3f;
            d[i] = legno * 0.8f + luccichio;
        }
        return d;
    }

    // Cambio arma: fruscio corto e un tintinnio.
    static float[] CambioArma(System.Random caso)
    {
        float[] d = Sibilo(caso, 0.18f, 0.03f, 0.25f, 0.6f);
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            d[i] += (Seno(2400f, t) + Seno(3600f, t) * 0.5f) * Busta(t - 0.12f, 0.001f, 40f) * 0.3f;
        }
        return d;
    }

    // Lancio della sfera magica: soffio con un tono che sale.
    static float[] SferaLancio(System.Random caso)
    {
        float durata = 0.45f;
        float[] d = Sibilo(caso, durata, 0.05f, 0.3f, 0.5f);
        float fase = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            fase += Mathf.Lerp(300f, 900f, t / durata) / Frequenza;
            d[i] += Mathf.Sin(2f * Mathf.PI * fase) * Mathf.Sin(Mathf.PI * t / durata) * 0.4f;
        }
        return d;
    }

    // Impatto della sfera: scoppio con un tono che scende.
    static float[] SferaImpatto(System.Random caso)
    {
        float durata = 0.5f;
        float[] d = Vuoto(durata);
        float fase = 0f, basso = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            fase += Mathf.Lerp(700f, 150f, t / durata) / Frequenza;
            basso += 0.4f * (Rumore(caso) - basso);
            d[i] = Mathf.Sin(2f * Mathf.PI * fase) * Busta(t, 0.002f, 9f) * 0.6f + basso * Busta(t, 0.001f, 18f) * 0.6f;
        }
        return d;
    }

    // "Non si può": due note basse e corte (porta chiusa, mana finito).
    static float[] Negato()
    {
        float[] d = Vuoto(0.35f);
        for (int i = 0; i < d.Length; i++)
        {
            float t = T(i);
            d[i] = Seno(196f, t) * Busta(t, 0.005f, 18f) * 0.5f + Seno(147f, t) * Busta(t - 0.12f, 0.005f, 14f) * 0.5f;
        }
        return d;
    }

    // ---------- Attrezzi ----------

    static float[] Vuoto(float secondi) => new float[Mathf.CeilToInt(secondi * Frequenza)];
    static float T(int campione) => campione / (float)Frequenza;
    static float Rumore(System.Random caso) => (float)(caso.NextDouble() * 2.0 - 1.0);
    static float Seno(float tono, float t) => Mathf.Sin(2f * Mathf.PI * tono * t);

    // Volume nel tempo: sale in "attacco" secondi, poi si spegne (più "decadimento" è alto, più in fretta).
    static float Busta(float t, float attacco, float decadimento)
    {
        if (t < 0f) return 0f;
        if (t < attacco) return t / attacco;
        return Mathf.Exp(-(t - attacco) * decadimento);
    }

    // Porta il punto più forte del suono al volume indicato, così tutti i suoni hanno un livello simile.
    static void Normalizza(float[] dati, float picco)
    {
        float massimo = 0.0001f;
        foreach (float v in dati) massimo = Mathf.Max(massimo, Mathf.Abs(v));
        float fattore = picco / massimo;
        for (int i = 0; i < dati.Length; i++) dati[i] *= fattore;
    }
}
