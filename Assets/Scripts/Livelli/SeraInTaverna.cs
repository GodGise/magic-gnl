using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// La scena della quest 6 "Una sera in taverna" dopo le chiacchiere di Hobb (Docs/dialoghi-capitolo-1.md):
//   1. Maren entra dalla porta con Tobin addormentato in braccio, raggiunge il giocatore e parla (2 righe);
//   2. in lontananza i tamburi: le luci tremano, la camera trema appena, tutti si zittiscono;
//   3. Hobb: "...Avete sentito anche voi?"  Maren: "Dentro. Tutti dentro.";
//   4. l'uscita della taverna si apre (fuori la razzia è già cominciata: vedi RazziaVillaggio) e parte "Al Termine".
// I tamburi continuano piano finché si resta dentro.
// Come montarlo: nella scena della taverna, su un oggetto vuoto. "Maren" = la figura di Maren (spenta all'inizio),
// "Ingresso Maren" e "Posto Maren" = da dove entra e dove si ferma, "Uscita" = il PassaggioScena della porta
// (spento). Si fa partire dall'evento "Al Termine" del dialogo di Hobb: SeraInTaverna > Avvia.
// Lo prepara da solo il comando "magic-gnl > Taverna: crea la scena della taverna".
public class SeraInTaverna : MonoBehaviour
{
    [SerializeField] GameObject maren;
    [SerializeField] Transform ingressoMaren;
    [SerializeField] Transform postoMaren;
    [SerializeField, Min(0.5f)] float velocitaMaren = 1.4f;
    [SerializeField] RigaDialogo[] righeMaren;
    [SerializeField] RigaDialogo[] righeDopoITamburi;
    [Tooltip("Secondi di tamburi prima che Hobb parli.")]
    [SerializeField, Min(0f)] float secondiTamburi = 4f;
    [SerializeField] PassaggioScena uscita;
    [SerializeField] UnityEvent alTermine;

    bool partita, finita;
    float prossimoTamburo;
    Light[] luci;
    float[] intensita;

    public void Avvia()
    {
        if (partita) return;
        partita = true;
        StartCoroutine(Sequenza());
    }

    IEnumerator Sequenza()
    {
        luci = transform.root.GetComponentsInChildren<Light>();
        intensita = new float[luci.Length];
        for (int i = 0; i < luci.Length; i++) intensita[i] = luci[i].intensity;
        yield return new WaitForSeconds(0.8f);

        // 1. Maren entra e viene verso il giocatore
        if (maren != null && ingressoMaren != null && postoMaren != null)
        {
            maren.transform.SetPositionAndRotation(ingressoMaren.position, ingressoMaren.rotation);
            maren.SetActive(true);
            Suoni.Suona(Suono.PortaPietra, ingressoMaren.position, 0.5f, 1.6f);
            Vector3 da = ingressoMaren.position, a = postoMaren.position;
            float durata = Vector3.Distance(da, a) / velocitaMaren;
            for (float t = 0f; t < durata; t += Time.deltaTime)
            {
                float k = t / durata;
                Vector3 p = Vector3.Lerp(da, a, k);
                p.y += Mathf.Abs(Mathf.Sin(k * durata * 7f)) * 0.04f;   // passo
                maren.transform.position = p;
                Vector3 verso = a - da;
                verso.y = 0f;
                if (verso.sqrMagnitude > 0.01f) maren.transform.rotation = Quaternion.LookRotation(verso);
                yield return null;
            }
            maren.transform.SetPositionAndRotation(a, postoMaren.rotation);
        }
        yield return Parla(righeMaren);

        // 2. i tamburi in lontananza
        float inizio = Time.time;
        int colpo = 0;
        while (Time.time - inizio < secondiTamburi)
        {
            if (colpo % 4 != 3) Suoni.SuonaOvunque(Suono.Tamburo, 0.35f + 0.1f * (colpo % 4 == 0 ? 1f : 0f));
            if (colpo % 2 == 0) CameraTerzaPersona.Scuoti(0.015f, 0.3f);
            colpo++;
            float fine = Time.time + 0.5f;
            while (Time.time < fine) { TremanoLeLuci(0.35f); yield return null; }
        }
        TremanoLeLuci(0f);

        // 3. "Avete sentito anche voi?" / "Dentro. Tutti dentro."
        yield return Parla(righeDopoITamburi);

        // 4. si può uscire
        finita = true;
        prossimoTamburo = Time.time + 1.2f;
        if (uscita != null) uscita.enabled = true;
        alTermine.Invoke();
    }

    IEnumerator Parla(RigaDialogo[] righe)
    {
        if (righe == null || righe.Length == 0) yield break;
        while (Dialogo.InCorso) yield return null;
        bool chiuso = false;
        Dialogo.Avvia(righe, () => chiuso = true);
        while (!chiuso) yield return null;
    }

    // Le luci della taverna tremano come se il pavimento vibrasse (forza 0 = ferme).
    void TremanoLeLuci(float forza)
    {
        if (luci == null) return;
        for (int i = 0; i < luci.Length; i++)
        {
            var l = luci[i];
            if (l == null || l.GetComponent<Torcia>() != null) continue;   // le torce tremano già da sole
            l.intensity = intensita[i] * (1f - forza * Mathf.PerlinNoise(Time.time * 18f, i * 3.1f));
        }
    }

    void Update()
    {
        // dopo: i tamburi continuano lontani finché si resta dentro
        if (!finita || Time.time < prossimoTamburo) return;
        prossimoTamburo = Time.time + Random.Range(0.45f, 0.7f);
        Suoni.SuonaOvunque(Suono.Tamburo, 0.22f);
    }
}
