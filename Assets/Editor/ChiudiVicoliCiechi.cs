using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Strumento dell'editor: trova nella scena aperta i "vicoli ciechi" fra le costruzioni e li chiude con una staccionata.
// Vicolo cieco = uno spazio stretto (meno di 2,4 m) fra case, muri, recinti o oggetti in cui il giocatore può entrare,
// anche usando la strettoia, ma che non porta da nessuna parte: ci si infila e si resta incastrati nell'angolo.
// I passaggi stretti che collegano due posti (anche solo con la strettoia) restano aperti. Dove una staccionata
// non si può tirare (la bocca non ha muri vicini ai due lati) l'angolo si riempie con una catasta di legna.
// Le fessure troppo strette per entrarci (meno di circa mezzo metro) non servono: lì il giocatore non entra.
// Non chiude gli angoli poco profondi (meno di 1 m), gli spazi in mezzo al bosco senza costruzioni, e gli spazi
// vicino a un punto delle quest (un personaggio con cui parlare, un punto con E, un arrivo).
// Come si usa: con la scena aperta (per esempio VillaggioLagoNero), menu "magic-gnl > Villaggio: chiudi i vicoli ciechi",
// poi salvare la scena (Ctrl+S). Si può rifare quando si aggiungono costruzioni: toglie le staccionate vecchie
// (gruppo "Recinzioni dei vicoli ciechi" dentro "Villaggio") e rifà l'analisi. Si annulla con Ctrl+Z.
// Alla fine scrive nella Console dove ha messo ogni staccionata e salva una mappa vista dall'alto in
// Temp/vicoli-villaggio.png (bianco: spazio aperto, giallo: passaggi stretti che portano da qualche parte,
// viola: staccionate, rosso: vicoli ciechi rimasti, azzurro: vicoli lasciati aperti per una quest), e la mostra
// nella finestra "Mappa dei vicoli" (MappaVicoli.cs).
// Non va montato su niente: è solo un comando dell'editor.
public static class ChiudiVicoliCiechi
{
    const string NomeGruppo = "Recinzioni dei vicoli ciechi";
    const float Cella = 0.2f;              // lato di un quadretto dell'analisi, in metri
    const float RaggioStrettoia = 0.24f;   // il personaggio nella strettoia (raggio 0,22 m) più un margine
    const float RaggioLargo = 1.2f;        // "spazio aperto": almeno 2,4 m liberi
    const float ProfonditaMinima = 1f;     // gli angoli meno profondi di così non si chiudono
    const float AreaApertaMinima = 25f;    // uno spazio aperto più piccolo di così fa parte del vicolo
    const float BoccaMassima = 6f;         // una bocca più larga di così non si chiude (meglio guardarla a mano)
    const float RaggioProtetto = 1.2f;     // distanza dai punti delle quest entro cui non si chiude niente
    const float AltezzaBassa = 0.35f, AltezzaAlta = 1.65f;   // la fascia del corpo che deve passare
    const int Giri = 4;

    enum Tipo : byte { Libero, Struttura, Albero, Invisibile }
    enum Stato : byte { Irraggiungibile, Aperto, Passaggio, Cieco, Angolo, Protetto }

    class Vicolo
    {
        public List<int> celle = new List<int>();
        public List<int> bocca = new List<int>();
        public int[] profondita;   // in quadretti, parallelo a celle
    }

    static int nx, nz;
    static float x0, z0;
    static Tipo[] tipo;
    static int[] chi;              // indice in "ostacoli" del collider che occupa il quadretto, -1 se libero
    static List<Collider> ostacoli;
    static Stato[] stato;
    static bool[] apertoGrande;

    [MenuItem("magic-gnl/Villaggio: chiudi i vicoli ciechi")]
    static void Esegui()
    {
        if (Application.isPlaying) { Debug.LogWarning("Vicoli ciechi: ferma il Play prima di usare il comando."); return; }
        var scena = SceneManager.GetActiveScene();
        var giocatore = Object.FindFirstObjectByType<GiocatoreControllo>();
        if (giocatore == null) { Debug.LogWarning("Vicoli ciechi: nella scena non c'è il Giocatore (serve come punto di partenza)."); return; }

        // Via le staccionate di prima: l'analisi si rifà da capo.
        var vecchi = new List<GameObject>();
        foreach (var radice in scena.GetRootGameObjects())
            foreach (var t in radice.GetComponentsInChildren<Transform>(true))
                if (t.name == NomeGruppo) vecchi.Add(t.gameObject);
        foreach (var v in vecchi) if (v != null) Undo.DestroyObjectImmediate(v);

        var gruppo = new GameObject(NomeGruppo);
        Undo.RegisterCreatedObjectUndo(gruppo, "Chiudi i vicoli ciechi");
        SceneManager.MoveGameObjectToScene(gruppo, scena);
        var villaggio = TrovaRadice(scena, "Villaggio");
        if (villaggio != null) gruppo.transform.SetParent(villaggio, false);
        var legno = AssetDatabase.LoadAssetAtPath<Material>("Assets/Segnaposto/Villaggio/LegnoVillaggio.mat");

        var protetti = PuntiProtetti(scena);
        var elenco = new StringBuilder();
        var saltati = new StringBuilder();
        int messe = 0;
        bool pulito = false;
        float inizio = (float)EditorApplication.timeSinceStartup;
        try
        {
            for (int giro = 0; giro < Giri; giro++)
            {
                EditorUtility.DisplayProgressBar("Vicoli ciechi", "Analisi della mappa (giro " + (giro + 1) + ")...", (giro + 0.5f) / Giri);
                Physics.SyncTransforms();
                saltati.Clear();
                var ciechi = Analizza(scena, giocatore.transform.position, protetti, gruppo.transform, saltati);
                int questoGiro = 0;
                foreach (var v in ciechi)
                    if (Recinta(v, gruppo.transform, legno, elenco, saltati)) { questoGiro++; messe++; }
                if (questoGiro == 0) { pulito = true; break; }
            }
            if (!pulito)   // l'ultimo giro ha messo staccionate: un'analisi in più per la mappa e per l'elenco dei rimasti
            {
                Physics.SyncTransforms();
                saltati.Clear();
                foreach (var v in Analizza(scena, giocatore.transform.position, protetti, gruppo.transform, saltati))
                    saltati.AppendLine("- rimasto aperto dopo " + Giri + " giri, a " + Posizione(Centro(v.bocca)) + " (accanto a " + NomeVicino(v.bocca) + ")");
            }
        }
        finally { EditorUtility.ClearProgressBar(); }
        string immagine = SalvaImmagine(gruppo.transform);

        EditorSceneManager.MarkSceneDirty(scena);
        Selection.activeGameObject = gruppo;
        Debug.Log("Vicoli ciechi: " + messe + " staccionate messe in " + ((float)EditorApplication.timeSinceStartup - inizio).ToString("0.0")
            + " s. Mappa: " + immagine + ". Salva la scena (Ctrl+S).\n" + elenco);
        if (saltati.Length > 0) Debug.LogWarning("Vicoli ciechi lasciati aperti (da guardare a mano):\n" + saltati);
    }

    // ---------------------------------------------------------------- analisi

    static List<Vicolo> Analizza(Scene scena, Vector3 partenza, List<Vector3> protetti, Transform recinzioni, StringBuilder saltati)
    {
        Rasterizza(scena, partenza);
        int n = nx * nz;

        // Quanto spazio libero ha intorno ogni quadretto.
        var bloccato = new bool[n];
        for (int i = 0; i < n; i++) bloccato[i] = tipo[i] != Tipo.Libero;
        float[] spazio = Distanza(bloccato);
        for (int i = 0; i < n; i++) spazio[i] -= Cella * 0.5f;

        // Dove arriva il giocatore partendo da dov'è, stringendosi al massimo.
        var raggiunto = new bool[n];
        int primo = CellaLibera(partenza, spazio);
        if (primo >= 0) Riempi(primo, i => spazio[i] >= RaggioStrettoia, raggiunto);

        // Spazio aperto: i punti con almeno 2,4 m liberi, allargati fino ai muri.
        var largo = new bool[n];
        for (int i = 0; i < n; i++) largo[i] = raggiunto[i] && spazio[i] >= RaggioLargo;
        float[] daLargo = Distanza(largo);
        var aperto = new bool[n];
        for (int i = 0; i < n; i++) aperto[i] = raggiunto[i] && daLargo[i] <= RaggioLargo + Cella * 0.5f;

        // Gli spazi aperti piccoli (nicchie un po' più larghe) contano come parte del vicolo, salvo i punti delle quest.
        var protetto = new bool[n];
        foreach (var p in protetti) Segna(p, RaggioProtetto, protetto);
        int[] gruppiAperti = Etichetta(aperto, out int quantiAperti);
        var area = new int[quantiAperti + 1];
        var conPunto = new bool[quantiAperti + 1];
        for (int i = 0; i < n; i++) if (gruppiAperti[i] > 0) { area[gruppiAperti[i]]++; if (protetto[i]) conPunto[gruppiAperti[i]] = true; }
        apertoGrande = new bool[n];
        for (int i = 0; i < n; i++)
        {
            int g = gruppiAperti[i];
            apertoGrande[i] = g > 0 && (area[g] * Cella * Cella >= AreaApertaMinima || conPunto[g]);
        }

        // Il resto di quello che si raggiunge è stretto: vicoli e passaggi.
        var stretto = new bool[n];
        for (int i = 0; i < n; i++) stretto[i] = raggiunto[i] && !apertoGrande[i];
        int[] gruppiStretti = Etichetta(stretto, out int quantiStretti);
        var celleDi = new List<int>[quantiStretti + 1];
        for (int g = 1; g <= quantiStretti; g++) celleDi[g] = new List<int>();
        for (int i = 0; i < n; i++) if (gruppiStretti[i] > 0) celleDi[gruppiStretti[i]].Add(i);

        stato = new Stato[n];
        for (int i = 0; i < n; i++) stato[i] = apertoGrande[i] ? Stato.Aperto : Stato.Irraggiungibile;

        var ciechi = new List<Vicolo>();
        var buf4 = new int[4];
        for (int g = 1; g <= quantiStretti; g++)
        {
            var celle = celleDi[g];
            var v = new Vicolo { celle = celle };
            var contatti = new HashSet<int>();
            var gruppiToccati = new HashSet<int>();
            foreach (int c in celle)
            {
                bool inBocca = false;
                int q = Vicini4(c, buf4);
                for (int j = 0; j < q; j++)
                {
                    int vic = buf4[j];
                    if (apertoGrande[vic]) { contatti.Add(vic); gruppiToccati.Add(gruppiAperti[vic]); inBocca = true; }
                }
                if (inBocca) v.bocca.Add(c);
            }
            Stato esito = Stato.Passaggio;
            if (gruppiToccati.Count == 1 && Pezzi(contatti) == 1)
            {
                v.profondita = Profondita(v);
                int massima = 0;
                foreach (int p in v.profondita) massima = Mathf.Max(massima, p);
                bool vicinoACase = false, vicinoAPunto = false;
                foreach (int c in celle)
                {
                    if (protetto[c]) vicinoAPunto = true;
                    if (!vicinoACase && StrutturaVicina(c, 3)) vicinoACase = true;
                }
                if (massima * Cella < ProfonditaMinima || !vicinoACase) esito = Stato.Angolo;
                else if (vicinoAPunto)
                {
                    esito = Stato.Protetto;
                    saltati.AppendLine("- vicino a un punto delle quest, a " + Posizione(Centro(v.bocca)) + " (accanto a " + NomeVicino(v.bocca) + ")");
                }
                else { esito = Stato.Cieco; ciechi.Add(v); }
            }
            foreach (int c in celle) stato[c] = esito;
        }
        return ciechi;
    }

    // Segna i quadretti occupati da case, muri, recinti, oggetti, alberi e confini invisibili, nella fascia del corpo.
    static void Rasterizza(Scene scena, Vector3 partenza)
    {
        ostacoli = new List<Collider>();
        var tipi = new Dictionary<Collider, Tipo>();
        var indici = new Dictionary<Collider, int>();
        Bounds tutto = new Bounds(partenza, Vector3.one * 4f);
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!c.enabled || c.isTrigger || c.gameObject.scene != scena) continue;
            var b = c.bounds;
            if (b.max.y < AltezzaBassa || b.min.y > AltezzaAlta) continue;          // pavimenti, strade, tetti
            if (b.size.x * b.size.z > 5000f) continue;                               // il terreno intero
            if (c.GetComponentInParent<GiocatoreControllo>() != null || c.GetComponentInParent<CharacterController>() != null) continue;
            if (c.GetComponentInParent<Bersaglio>() != null) continue;                // nemici e manichini
            if (c.transform.Find("Testa") != null) continue;                          // i personaggi (si muovono o si spostano)
            Tipo t = Tipo.Struttura;
            if (SottoAlbero(c.transform)) t = Tipo.Albero;
            else if (c.GetComponentInChildren<Renderer>() == null) t = Tipo.Invisibile;
            tipi[c] = t;
            indici[c] = ostacoli.Count;
            ostacoli.Add(c);
            tutto.Encapsulate(b);
        }
        tutto.Expand(new Vector3(4f, 0f, 4f));
        x0 = tutto.min.x; z0 = tutto.min.z;
        nx = Mathf.CeilToInt(tutto.size.x / Cella);
        nz = Mathf.CeilToInt(tutto.size.z / Cella);
        int n = nx * nz;
        tipo = new Tipo[n];
        chi = new int[n];
        for (int i = 0; i < n; i++) chi[i] = -1;
        var fatto = new bool[n];
        var risultati = new Collider[32];
        var meta = new Vector3(Cella * 0.5f, (AltezzaAlta - AltezzaBassa) * 0.5f, Cella * 0.5f);
        float yCentro = (AltezzaAlta + AltezzaBassa) * 0.5f;
        foreach (var c in ostacoli)
        {
            var b = c.bounds;
            int ix0 = Mathf.Max(0, Mathf.FloorToInt((b.min.x - x0) / Cella)), ix1 = Mathf.Min(nx - 1, Mathf.FloorToInt((b.max.x - x0) / Cella));
            int iz0 = Mathf.Max(0, Mathf.FloorToInt((b.min.z - z0) / Cella)), iz1 = Mathf.Min(nz - 1, Mathf.FloorToInt((b.max.z - z0) / Cella));
            for (int iz = iz0; iz <= iz1; iz++)
                for (int ix = ix0; ix <= ix1; ix++)
                {
                    int i = iz * nx + ix;
                    if (fatto[i]) continue;
                    fatto[i] = true;
                    var centro = new Vector3(x0 + (ix + 0.5f) * Cella, yCentro, z0 + (iz + 0.5f) * Cella);
                    int quanti = Physics.OverlapBoxNonAlloc(centro, meta, risultati, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                    for (int k = 0; k < quanti; k++)
                    {
                        if (!tipi.TryGetValue(risultati[k], out Tipo t)) continue;
                        // una costruzione vince su un albero, un albero su un confine invisibile
                        if (tipo[i] == Tipo.Libero || Priorita(t) > Priorita(tipo[i]))
                        {
                            tipo[i] = t;
                            chi[i] = indici[risultati[k]];
                        }
                    }
                }
        }
    }

    static int Priorita(Tipo t) => t == Tipo.Struttura ? 3 : t == Tipo.Albero ? 2 : t == Tipo.Invisibile ? 1 : 0;

    static bool SottoAlbero(Transform t)
    {
        for (; t != null; t = t.parent) if (t.name == "Albero" || t.name == "Albero (sgomberato)") return true;
        return false;
    }

    static Transform TrovaRadice(Scene scena, string nome)
    {
        foreach (var r in scena.GetRootGameObjects()) if (r.name == nome) return r.transform;
        return null;
    }

    // I punti delle quest: lì non si chiude niente (porte dei capanni, personaggi, arrivi).
    static List<Vector3> PuntiProtetti(Scene scena)
    {
        var punti = new List<Vector3>();
        foreach (var radice in scena.GetRootGameObjects())
        {
            foreach (var p in radice.GetComponentsInChildren<PuntoInterazione>(true)) punti.Add(p.transform.position);
            foreach (var p in radice.GetComponentsInChildren<Interlocutore>(true)) punti.Add(p.transform.position);
            foreach (var p in radice.GetComponentsInChildren<ZonaArrivo>(true)) punti.Add(p.transform.position);
        }
        return punti;
    }

    // ---------------------------------------------------------------- staccionata

    static bool Recinta(Vicolo v, Transform gruppo, Material legno, StringBuilder elenco, StringBuilder saltati)
    {
        Vector3 bocca = Centro(v.bocca);
        // La direzione "dentro il vicolo": verso i quadretti poco oltre la bocca.
        var vicini = new List<int>();
        int lontano = v.celle[0], profMax = -1;
        for (int k = 0; k < v.celle.Count; k++)
        {
            if (v.profondita[k] >= 2 && v.profondita[k] <= Mathf.RoundToInt(1.2f / Cella)) vicini.Add(v.celle[k]);
            if (v.profondita[k] > profMax) { profMax = v.profondita[k]; lontano = v.celle[k]; }
        }
        Vector3 dentro = (vicini.Count > 0 ? Centro(vicini) : PosCella(lontano)) - bocca;
        dentro.y = 0f;
        if (dentro.sqrMagnitude < 0.0025f) { dentro = PosCella(lontano) - bocca; dentro.y = 0f; }
        if (dentro.sqrMagnitude < 0.0025f) return false;
        dentro.Normalize();
        Vector3 lato = new Vector3(-dentro.z, 0f, dentro.x);
        Vector3 baseLinea = bocca + dentro * 0.25f;

        float a = Fino(baseLinea, lato), b = Fino(baseLinea, -lato);
        if (a < 0f || b < 0f) return Riempi(v, dentro, lato, gruppo, legno, elenco, saltati);
        Vector3 p1 = baseLinea + lato * (a + 0.3f), p2 = baseLinea - lato * (b + 0.3f);
        Vector3 mezzo = (p1 + p2) * 0.5f;
        float lunghezza = Vector3.Distance(p1, p2);
        mezzo.y = Pavimento(mezzo);
        Staccionata(gruppo, mezzo, lato, lunghezza, legno);
        elenco.AppendLine("- staccionata di " + lunghezza.ToString("0.0") + " m a " + Posizione(mezzo) + ", accanto a " + NomeVicino(v.bocca));
        return true;
    }

    // Quando la staccionata non si può tirare (la bocca non ha due muri vicini ai lati), l'angolo si riempie
    // con una catasta di legna grande quanto il vicolo.
    static bool Riempi(Vicolo v, Vector3 dentro, Vector3 lato, Transform gruppo, Material legno, StringBuilder elenco, StringBuilder saltati)
    {
        float minL = float.MaxValue, maxL = float.MinValue, minD = float.MaxValue, maxD = float.MinValue;
        Vector3 origine = PosCella(v.celle[0]);
        foreach (int c in v.celle)
        {
            Vector3 p = PosCella(c) - origine;
            float l = Vector3.Dot(p, lato), d = Vector3.Dot(p, dentro);
            minL = Mathf.Min(minL, l); maxL = Mathf.Max(maxL, l); minD = Mathf.Min(minD, d); maxD = Mathf.Max(maxD, d);
        }
        float largo = maxL - minL + Cella + 0.3f, lungo = maxD - minD + Cella + 0.3f;
        Vector3 centro = origine + lato * ((minL + maxL) * 0.5f) + dentro * ((minD + maxD) * 0.5f);
        if (largo > BoccaMassima || lungo > BoccaMassima)
        {
            saltati.AppendLine("- vicolo troppo grande da chiudere (" + largo.ToString("0.0") + " x " + lungo.ToString("0.0") + " m), a "
                + Posizione(centro) + " (accanto a " + NomeVicino(v.bocca) + ")");
            return false;
        }
        centro.y = Pavimento(centro);
        var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(o, "Chiudi i vicoli ciechi");
        o.name = "Catasta (chiude un vicolo)";
        o.transform.SetParent(gruppo, false);
        o.transform.SetPositionAndRotation(centro + Vector3.up * 0.6f, Quaternion.LookRotation(dentro));
        o.transform.localScale = new Vector3(largo, 1.2f, lungo);
        if (legno != null) o.GetComponent<Renderer>().sharedMaterial = legno;
        elenco.AppendLine("- catasta di " + largo.ToString("0.0") + " x " + lungo.ToString("0.0") + " m a " + Posizione(centro) + ", accanto a " + NomeVicino(v.bocca));
        return true;
    }

    // Quanti metri lungo una direzione fino al primo ostacolo (-1 se non c'è entro metà della bocca massima).
    static float Fino(Vector3 da, Vector3 verso)
    {
        for (float d = 0f; d <= BoccaMassima * 0.5f; d += Cella * 0.5f)
        {
            int i = Indice(da + verso * d);
            if (i < 0 || tipo[i] != Tipo.Libero) return d;
        }
        return -1f;
    }

    static float Pavimento(Vector3 p)
    {
        float y = 0f, migliore = float.MaxValue;
        foreach (var h in Physics.RaycastAll(new Vector3(p.x, 1.2f, p.z), Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore))
            if (h.normal.y > 0.6f && h.distance < migliore) { migliore = h.distance; y = h.point.y; }
        return y;
    }

    // Una staccionata di legno: pali ogni metro e mezzo, due traverse, un solo collider pieno alto 1,4 m.
    static void Staccionata(Transform gruppo, Vector3 mezzo, Vector3 lato, float lunghezza, Material legno)
    {
        var s = new GameObject("Staccionata");
        Undo.RegisterCreatedObjectUndo(s, "Chiudi i vicoli ciechi");
        s.transform.SetParent(gruppo, false);
        s.transform.SetPositionAndRotation(mezzo, Quaternion.LookRotation(lato));
        var box = s.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.7f, 0f);
        box.size = new Vector3(0.3f, 1.4f, lunghezza);
        int pali = Mathf.Max(2, Mathf.CeilToInt(lunghezza / 1.5f) + 1);
        for (int k = 0; k < pali; k++)
        {
            float z = -lunghezza / 2f + 0.1f + (lunghezza - 0.2f) * k / (pali - 1);
            Pezzo("Palo", s.transform, new Vector3(0f, 0.65f, z), new Vector3(0.14f, 1.3f, 0.14f), legno);
        }
        Pezzo("Traversa bassa", s.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.07f, 0.12f, lunghezza), legno);
        Pezzo("Traversa alta", s.transform, new Vector3(0f, 1.05f, 0f), new Vector3(0.07f, 0.12f, lunghezza), legno);
    }

    static void Pezzo(string nome, Transform genitore, Vector3 pos, Vector3 scala, Material m)
    {
        var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
        o.name = nome;
        Object.DestroyImmediate(o.GetComponent<Collider>());
        o.transform.SetParent(genitore, false);
        o.transform.localPosition = pos;
        o.transform.localScale = scala;
        if (m != null) o.GetComponent<Renderer>().sharedMaterial = m;
    }

    // ---------------------------------------------------------------- aiuti della griglia

    static int Indice(Vector3 p)
    {
        int ix = Mathf.FloorToInt((p.x - x0) / Cella), iz = Mathf.FloorToInt((p.z - z0) / Cella);
        if (ix < 0 || iz < 0 || ix >= nx || iz >= nz) return -1;
        return iz * nx + ix;
    }

    static Vector3 PosCella(int i) => new Vector3(x0 + (i % nx + 0.5f) * Cella, 0f, z0 + (i / nx + 0.5f) * Cella);

    static Vector3 Centro(List<int> celle)
    {
        Vector3 s = Vector3.zero;
        foreach (int c in celle) s += PosCella(c);
        return celle.Count > 0 ? s / celle.Count : s;
    }

    static string Posizione(Vector3 p) => "(" + p.x.ToString("0.0") + "; " + p.z.ToString("0.0") + ")";

    static string NomeVicino(List<int> celle)
    {
        foreach (int c in celle)
        {
            int ix = c % nx, iz = c / nx;
            for (int r = 1; r <= 6; r++)
                for (int dz = -r; dz <= r; dz++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        int x = ix + dx, z = iz + dz;
                        if (x < 0 || z < 0 || x >= nx || z >= nz) continue;
                        int i = z * nx + x;
                        if (tipo[i] == Tipo.Struttura && chi[i] >= 0 && ostacoli[chi[i]] != null)
                        {
                            var t = ostacoli[chi[i]].transform;
                            return "\"" + t.name + "\"" + (t.parent != null ? " in \"" + t.parent.name + "\"" : "");
                        }
                    }
        }
        return "?";
    }

    static bool StrutturaVicina(int c, int raggio)
    {
        int ix = c % nx, iz = c / nx;
        for (int dz = -raggio; dz <= raggio; dz++)
            for (int dx = -raggio; dx <= raggio; dx++)
            {
                int x = ix + dx, z = iz + dz;
                if (x >= 0 && z >= 0 && x < nx && z < nz && tipo[z * nx + x] == Tipo.Struttura) return true;
            }
        return false;
    }

    static void Segna(Vector3 p, float raggio, bool[] segno)
    {
        int r = Mathf.CeilToInt(raggio / Cella);
        int c = Indice(p);
        if (c < 0) return;
        int ix = c % nx, iz = c / nx;
        for (int dz = -r; dz <= r; dz++)
            for (int dx = -r; dx <= r; dx++)
            {
                int x = ix + dx, z = iz + dz;
                if (x >= 0 && z >= 0 && x < nx && z < nz && (dx * dx + dz * dz) * Cella * Cella <= raggio * raggio) segno[z * nx + x] = true;
            }
    }

    static int CellaLibera(Vector3 p, float[] spazio)
    {
        int c = Indice(p);
        if (c < 0) return -1;
        int ix = c % nx, iz = c / nx;
        for (int r = 0; r <= 15; r++)
            for (int dz = -r; dz <= r; dz++)
                for (int dx = -r; dx <= r; dx++)
                {
                    int x = ix + dx, z = iz + dz;
                    if (x >= 0 && z >= 0 && x < nx && z < nz && spazio[z * nx + x] >= RaggioLargo) return z * nx + x;
                }
        return -1;
    }

    // I 4 vicini (destra, sinistra, sopra, sotto) di un quadretto, scritti in "buf"; restituisce quanti sono.
    static int Vicini4(int i, int[] buf)
    {
        int ix = i % nx, iz = i / nx, k = 0;
        if (ix > 0) buf[k++] = i - 1;
        if (ix < nx - 1) buf[k++] = i + 1;
        if (iz > 0) buf[k++] = i - nx;
        if (iz < nz - 1) buf[k++] = i + nx;
        return k;
    }

    static void Riempi(int primo, System.Func<int, bool> passa, bool[] segno)
    {
        var coda = new Queue<int>();
        var buf = new int[4];
        segno[primo] = true;
        coda.Enqueue(primo);
        while (coda.Count > 0)
        {
            int c = coda.Dequeue();
            int q = Vicini4(c, buf);
            for (int j = 0; j < q; j++)
            {
                int v = buf[j];
                if (!segno[v] && passa(v)) { segno[v] = true; coda.Enqueue(v); }
            }
        }
    }

    static int[] Etichetta(bool[] maschera, out int quanti)
    {
        var lab = new int[maschera.Length];
        quanti = 0;
        var coda = new Queue<int>();
        var buf = new int[4];
        for (int i = 0; i < maschera.Length; i++)
        {
            if (!maschera[i] || lab[i] != 0) continue;
            quanti++;
            lab[i] = quanti;
            coda.Enqueue(i);
            while (coda.Count > 0)
            {
                int c = coda.Dequeue();
                int q = Vicini4(c, buf);
                for (int j = 0; j < q; j++)
                {
                    int v = buf[j];
                    if (maschera[v] && lab[v] == 0) { lab[v] = quanti; coda.Enqueue(v); }
                }
            }
        }
        return lab;
    }

    // In quanti pezzi separati sono i quadretti di contatto (vicini anche in diagonale = stesso pezzo).
    static int Pezzi(HashSet<int> celle)
    {
        var visti = new HashSet<int>();
        int pezzi = 0;
        var coda = new Queue<int>();
        foreach (int s in celle)
        {
            if (visti.Contains(s)) continue;
            pezzi++;
            visti.Add(s);
            coda.Enqueue(s);
            while (coda.Count > 0)
            {
                int c = coda.Dequeue();
                int ix = c % nx, iz = c / nx;
                for (int dz = -1; dz <= 1; dz++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int x = ix + dx, z = iz + dz;
                        if (x < 0 || z < 0 || x >= nx || z >= nz) continue;
                        int v = z * nx + x;
                        if (celle.Contains(v) && visti.Add(v)) coda.Enqueue(v);
                    }
            }
        }
        return pezzi;
    }

    // Distanza (in quadretti) di ogni quadretto del vicolo dalla bocca.
    static int[] Profondita(Vicolo v)
    {
        var pos = new Dictionary<int, int>();
        for (int k = 0; k < v.celle.Count; k++) pos[v.celle[k]] = k;
        var prof = new int[v.celle.Count];
        for (int k = 0; k < prof.Length; k++) prof[k] = -1;
        var coda = new Queue<int>();
        var buf = new int[4];
        foreach (int b in v.bocca) { prof[pos[b]] = 0; coda.Enqueue(b); }
        while (coda.Count > 0)
        {
            int c = coda.Dequeue();
            int pc = prof[pos[c]];
            int q = Vicini4(c, buf);
            for (int j = 0; j < q; j++)
                if (pos.TryGetValue(buf[j], out int k) && prof[k] < 0) { prof[k] = pc + 1; coda.Enqueue(buf[j]); }
        }
        return prof;
    }

    // Distanza in metri di ogni quadretto dal quadretto "vero" più vicino (trasformata di Felzenszwalb).
    static float[] Distanza(bool[] segno)
    {
        const double Inf = 1e20;
        int n = nx * nz, m = Mathf.Max(nx, nz);
        var g = new double[n];
        var f = new double[m];
        var d = new double[m];
        var v = new int[m];
        var z = new double[m + 1];
        for (int iz = 0; iz < nz; iz++)
        {
            for (int ix = 0; ix < nx; ix++) f[ix] = segno[iz * nx + ix] ? 0 : Inf;
            Riga(f, nx, d, v, z);
            for (int ix = 0; ix < nx; ix++) g[iz * nx + ix] = d[ix];
        }
        var risultato = new float[n];
        for (int ix = 0; ix < nx; ix++)
        {
            for (int iz = 0; iz < nz; iz++) f[iz] = g[iz * nx + ix];
            Riga(f, nz, d, v, z);
            for (int iz = 0; iz < nz; iz++) risultato[iz * nx + ix] = d[iz] >= Inf * 0.5 ? 1e6f : (float)System.Math.Sqrt(d[iz]) * Cella;
        }
        return risultato;
    }

    static void Riga(double[] f, int n, double[] d, int[] v, double[] z)
    {
        const double Inf = 1e20;
        int k = 0;
        v[0] = 0; z[0] = -Inf; z[1] = Inf;
        for (int q = 1; q < n; q++)
        {
            double s = ((f[q] + (double)q * q) - (f[v[k]] + (double)v[k] * v[k])) / (2.0 * q - 2.0 * v[k]);
            while (s <= z[k])
            {
                k--;
                s = ((f[q] + (double)q * q) - (f[v[k]] + (double)v[k] * v[k])) / (2.0 * q - 2.0 * v[k]);
            }
            k++;
            v[k] = q; z[k] = s; z[k + 1] = Inf;
        }
        k = 0;
        for (int q = 0; q < n; q++)
        {
            while (z[k + 1] < q) k++;
            d[q] = (double)(q - v[k]) * (q - v[k]) + f[v[k]];
        }
    }

    // ---------------------------------------------------------------- mappa per controllare

    static string SalvaImmagine(Transform recinzioni)
    {
        if (tipo == null || stato == null) return "(nessuna)";
        var tex = new Texture2D(nx, nz, TextureFormat.RGB24, false);
        var px = new Color32[nx * nz];
        for (int i = 0; i < px.Length; i++)
        {
            Color32 c;
            if (tipo[i] == Tipo.Struttura)
                c = chi[i] >= 0 && ostacoli[chi[i]] != null && ostacoli[chi[i]].transform.IsChildOf(recinzioni) ? new Color32(200, 40, 220, 255) : new Color32(70, 70, 70, 255);
            else if (tipo[i] == Tipo.Albero) c = new Color32(40, 100, 45, 255);
            else if (tipo[i] == Tipo.Invisibile) c = new Color32(60, 80, 150, 255);
            else
            {
                switch (stato[i])
                {
                    case Stato.Aperto: c = new Color32(235, 230, 215, 255); break;
                    case Stato.Passaggio: c = new Color32(245, 200, 50, 255); break;
                    case Stato.Cieco: c = new Color32(230, 30, 30, 255); break;
                    case Stato.Angolo: c = new Color32(215, 175, 150, 255); break;
                    case Stato.Protetto: c = new Color32(70, 170, 235, 255); break;
                    default: c = new Color32(25, 25, 25, 255); break;
                }
            }
            px[i] = c;
        }
        tex.SetPixels32(px);
        tex.Apply();
        string cartella = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, "Temp");
        System.IO.Directory.CreateDirectory(cartella);
        string percorso = System.IO.Path.Combine(cartella, "vicoli-villaggio.png");
        System.IO.File.WriteAllBytes(percorso, tex.EncodeToPNG());
        tex.filterMode = FilterMode.Point;
        MappaVicoli.Mostra(tex);   // la finestra "Mappa dei vicoli"
        return "Temp/vicoli-villaggio.png";
    }
}
