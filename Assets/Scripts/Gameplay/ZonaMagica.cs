using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Le aree degli incantesimi dello Stregone che restano un po' nel mondo (Docs/incantesimi-stregone.md):
//   - Scia di brace: striscia di fuoco a terra davanti allo Stregone; chi ci sta sopra brucia (danno al secondo);
//   - Velo di nebbia: nebbia attorno allo Stregone; finché lo Stregone ci sta dentro, i nemici lo vedono solo da
//     vicinissimo (furtività al massimo) e chi lo inseguiva lo perde di vista;
//   - Pozza nera e Onda del lago: solo l'aspetto (gli effetti li dà MagiaStregone quando l'incantesimo parte);
//   - Gas della Bambola di ossa: chi è dentro quando scoppia rallenta e prende danno al secondo per qualche secondo;
//   - Lampo: lo scoppio della Palla di fuoco.
// Sono forme semplici e trasparenti, senza collider: non bloccano niente.
// Come montarlo: non si monta. Le crea MagiaStregone con i metodi Crea...
public class ZonaMagica : MonoBehaviour
{
    enum Tipo { Scia, Velo, Aspetto, Gas }

    Tipo tipo;
    MagiaStregone padrone;
    DatiIncantesimo dati;
    float fine;
    float prossimoColpo;
    Vector3 centro, avanti;
    float lunghezza, larghezza, raggio;
    Renderer aspetto;
    Color colore;
    float alfaIniziale;
    float nascita;

    // Velo: la furtività aggiunta allo Stregone mentre ci sta dentro.
    Statistiche statisticheStregone;
    Statistiche.Modificatore nebbia;
    bool dentro;

    // ---------- creazione ----------

    public static void CreaScia(MagiaStregone chi, DatiIncantesimo inc, Vector3 da, Vector3 direzione)
    {
        direzione.y = 0f;
        direzione.Normalize();
        float lunghezza = Mathf.Max(1f, inc.lunghezza), larghezza = Mathf.Max(0.5f, inc.raggio);
        Vector3 centro = Terreno(da + direzione * (0.5f + lunghezza * 0.5f));
        var zona = Nuova("Scia di brace", PrimitiveType.Cube, centro + Vector3.up * 0.05f, new Vector3(larghezza, 0.08f, lunghezza),
            Quaternion.LookRotation(direzione), inc.colore, 0.55f, true);
        zona.Prepara(Tipo.Scia, chi, inc, inc.durata);
        zona.centro = centro;
        zona.avanti = direzione;
        zona.lunghezza = lunghezza;
        zona.larghezza = larghezza;
        var luce = zona.gameObject.AddComponent<Light>();
        luce.type = LightType.Point;
        luce.color = inc.colore;
        luce.range = lunghezza;
        luce.intensity = 1.5f;
    }

    public static void CreaVelo(MagiaStregone chi, DatiIncantesimo inc, Vector3 punto)
    {
        float r = Mathf.Max(1f, inc.raggio);
        var zona = Nuova("Velo di nebbia", PrimitiveType.Sphere, Terreno(punto) + Vector3.up * 0.8f, new Vector3(r * 2f, 2.4f, r * 2f),
            Quaternion.identity, inc.colore, 0.45f, false);
        zona.Prepara(Tipo.Velo, chi, inc, inc.durata);
        zona.centro = Terreno(punto);
        zona.raggio = r;
        zona.statisticheStregone = Statistiche.Di(chi);
        zona.nebbia = new Statistiche.Modificatore { fonte = "Velo di nebbia", furtivita = 90f };
    }

    public static void CreaPozza(MagiaStregone chi, DatiIncantesimo inc, Vector3 punto, float durata)
    {
        float r = Mathf.Max(0.5f, inc.raggio);
        var zona = Nuova("Pozza nera", PrimitiveType.Cylinder, punto + Vector3.up * 0.04f, new Vector3(r * 2f, 0.02f, r * 2f),
            Quaternion.identity, inc.colore, 0.8f, false);
        zona.Prepara(Tipo.Aspetto, chi, inc, Mathf.Max(0.5f, durata));
    }

    public static void CreaOnda(MagiaStregone chi, DatiIncantesimo inc, Vector3 da, Vector3 direzione)
    {
        float larghezza = Mathf.Max(0.5f, inc.raggio);
        var zona = Nuova("Onda del lago", PrimitiveType.Cube, Terreno(da) + Vector3.up * inc.altezza * 0.5f + direzione * 0.8f,
            new Vector3(larghezza, Mathf.Max(0.5f, inc.altezza), 0.6f), Quaternion.LookRotation(direzione), inc.colore, 0.5f, false);
        zona.Prepara(Tipo.Aspetto, chi, inc, 0.45f);
        zona.avanti = direzione;
        zona.lunghezza = Mathf.Max(1f, inc.lunghezza);
        zona.StartCoroutine(zona.Avanza());
    }

    public static void CreaGas(MagiaStregone chi, DatiIncantesimo inc, Vector3 punto)
    {
        float r = Mathf.Max(0.5f, inc.raggio);
        var zona = Nuova("Gas di ossa", PrimitiveType.Sphere, punto + Vector3.up * 0.6f, Vector3.one * r * 2f,
            Quaternion.identity, new Color(0.45f, 0.6f, 0.25f), 0.5f, false);
        float durata = Mathf.Max(0.5f, inc.durataRallentamento);
        zona.Prepara(Tipo.Gas, chi, inc, durata + 0.6f);   // un po' più a lungo, così arriva anche l'ultimo colpo
        // Chi è dentro quando scoppia: rallentato e danno al secondo per tutta la durata, anche se poi esce.
        foreach (var b in MagiaStregone.NemiciVicini(punto, r))
        {
            EffettiNemico.Applica(b, EffettiNemico.Tipo.Rallenta, inc.rallentamento, chi.DurataEffetto(inc, durata), punto);
            zona.StartCoroutine(zona.DannoNelTempo(b, inc.dannoAlSecondo, durata, punto));
        }
        Suoni.Suona(Suono.SferaImpatto, punto, 0.8f, 0.6f);
    }

    public static void Lampo(Vector3 punto, float raggio, Color colore)
    {
        var zona = Nuova("Scoppio", PrimitiveType.Sphere, punto, Vector3.one * Mathf.Max(0.5f, raggio) * 2f, Quaternion.identity, colore, 0.6f, true);
        zona.Prepara(Tipo.Aspetto, null, null, 0.3f);
        var luce = zona.gameObject.AddComponent<Light>();
        luce.type = LightType.Point;
        luce.color = colore;
        luce.range = raggio * 3f;
        luce.intensity = 4f;
        Suoni.Suona(Suono.SferaImpatto, punto, 1f, 0.7f);
    }

    static ZonaMagica Nuova(string nome, PrimitiveType forma, Vector3 posizione, Vector3 scala, Quaternion rotazione, Color colore, float alfa, bool luminosa)
    {
        var oggetto = GameObject.CreatePrimitive(forma);
        oggetto.name = nome;
        Destroy(oggetto.GetComponent<Collider>());
        oggetto.transform.SetPositionAndRotation(posizione, rotazione);
        oggetto.transform.localScale = scala;
        var r = oggetto.GetComponent<Renderer>();
        var materiale = new Material(r.sharedMaterial);
        Trasparente(materiale);
        materiale.color = new Color(colore.r, colore.g, colore.b, alfa);
        if (luminosa)
        {
            materiale.EnableKeyword("_EMISSION");
            materiale.SetColor("_EmissionColor", colore * 1.5f);
        }
        r.sharedMaterial = materiale;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var zona = oggetto.AddComponent<ZonaMagica>();
        zona.aspetto = r;
        zona.colore = colore;
        zona.alfaIniziale = alfa;
        return zona;
    }

    // Il materiale Standard della pipeline di base, in modalità trasparente (per nebbia, gas e onda).
    static void Trasparente(Material m)
    {
        m.SetFloat("_Mode", 3f);
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
    }

    static Vector3 Terreno(Vector3 punto)
    {
        if (Physics.Raycast(punto + Vector3.up * 2f, Vector3.down, out RaycastHit terra, 8f, ~0, QueryTriggerInteraction.Ignore)
            && terra.collider.GetComponentInParent<Bersaglio>() == null && !ObiettiviNemici.EGiocatore(terra.collider))
            return terra.point;
        return punto;
    }

    void Prepara(Tipo t, MagiaStregone chi, DatiIncantesimo inc, float durata)
    {
        tipo = t;
        padrone = chi;
        dati = inc;
        nascita = Time.time;
        fine = Time.time + Mathf.Max(0.1f, durata);
    }

    // ---------- vita della zona ----------

    void Update()
    {
        if (Time.time >= fine) { Finisci(); return; }
        // Sparisce piano piano nell'ultimo mezzo secondo.
        float resto = Mathf.Clamp01((fine - Time.time) / 0.5f);
        aspetto.sharedMaterial.color = new Color(colore.r, colore.g, colore.b, alfaIniziale * resto);

        switch (tipo)
        {
            case Tipo.Scia:
                if (padrone == null || Time.time < prossimoColpo) break;
                prossimoColpo = Time.time + 0.5f;
                var meta = new Vector3(larghezza * 0.5f, 2f, lunghezza * 0.5f);
                var colpiti = new HashSet<Bersaglio>();
                foreach (Collider c in Physics.OverlapBox(centro + Vector3.up, meta, Quaternion.LookRotation(avanti), ~0, QueryTriggerInteraction.Ignore))
                {
                    Bersaglio b = c.GetComponentInParent<Bersaglio>();
                    if (b != null && !b.Morto && colpiti.Add(b)) padrone.DannoNelTempo(dati, b, dati.dannoAlSecondo, 0.5f, centro);
                }
                break;

            case Tipo.Velo:
                bool oraDentro = padrone != null && Piano(padrone.transform.position - centro).magnitude <= raggio;
                if (oraDentro != dentro) Nebbia(oraDentro);
                break;
        }
    }

    void Nebbia(bool attiva)
    {
        dentro = attiva;
        if (statisticheStregone == null) return;
        if (attiva) statisticheStregone.AggiungiModificatore(nebbia);
        else statisticheStregone.TogliModificatore(nebbia);
    }

    void Finisci()
    {
        if (tipo == Tipo.Velo && dentro) Nebbia(false);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (tipo == Tipo.Velo && dentro && statisticheStregone != null) statisticheStregone.TogliModificatore(nebbia);
    }

    IEnumerator Avanza()
    {
        // L'onda corre in avanti fino alla sua lunghezza.
        Vector3 partenza = transform.position;
        float durata = fine - nascita;
        while (Time.time < fine)
        {
            float t = Mathf.Clamp01((Time.time - nascita) / durata);
            transform.position = partenza + avanti * lunghezza * t;
            yield return null;
        }
    }

    IEnumerator DannoNelTempo(Bersaglio b, float dannoAlSecondo, float durata, Vector3 da)
    {
        for (float t = 0f; t < durata - 0.01f; t += 0.5f)
        {
            yield return new WaitForSeconds(0.5f);
            if (b == null || b.Morto || padrone == null) yield break;
            padrone.DannoNelTempo(dati, b, dannoAlSecondo, 0.5f, da, true);
        }
    }

    static Vector3 Piano(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
