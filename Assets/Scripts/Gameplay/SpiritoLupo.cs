using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spirito del lupo, evocazione dello Stregone (Docs/incantesimi-stregone.md): un lupo spettrale che corre verso
// il nemico più vicino e lo morde ("Danno Evocazione" ogni "Intervallo Colpi" secondi). Ha la sua vita (i nemici
// possono attaccarlo), recupera "Vita Per Uccisione" per ogni nemico ucciso da lui e lascia dietro di sé una scia
// lunga "Lunghezza Scia" metri che fa "Danno Al Secondo" ai nemici che ci passano. Senza nemici segue lo Stregone.
// Uno solo per giocatore (due con l'amuleto Occhio del lago).
// Come montarlo: non si monta. Lo crea MagiaStregone con SpiritoLupo.Crea.
public class SpiritoLupo : Evocazione
{
    const float Velocita = 6.5f, Portata = 20f, DistanzaMorso = 1.6f;
    float prossimoMorso, prossimaScia;
    readonly List<Vector3> scia = new List<Vector3>();
    readonly List<Renderer> parti = new List<Renderer>();
    Color colore;

    public static SpiritoLupo Crea(MagiaStregone chi, DatiIncantesimo inc, Vector3 posto)
    {
        var oggetto = new GameObject("Spirito del lupo");
        oggetto.transform.SetPositionAndRotation(posto, chi.transform.rotation);
        var lupo = oggetto.AddComponent<SpiritoLupo>();
        lupo.colore = inc.colore;
        // Corpo, testa, muso, zampe e coda: forme semplici, azzurre e luminose.
        lupo.Parte(PrimitiveType.Capsule, new Vector3(0f, 0.55f, 0f), new Vector3(0.45f, 0.5f, 0.45f), Quaternion.Euler(90f, 0f, 0f));
        lupo.Parte(PrimitiveType.Sphere, new Vector3(0f, 0.8f, 0.55f), new Vector3(0.35f, 0.32f, 0.38f));
        lupo.Parte(PrimitiveType.Cube, new Vector3(0f, 0.75f, 0.78f), new Vector3(0.15f, 0.13f, 0.25f));
        foreach (float x in new[] { -0.15f, 0.15f })
            foreach (float z in new[] { -0.3f, 0.3f })
                lupo.Parte(PrimitiveType.Cube, new Vector3(x, 0.2f, z), new Vector3(0.09f, 0.4f, 0.09f));
        lupo.Parte(PrimitiveType.Cube, new Vector3(0f, 0.65f, -0.55f), new Vector3(0.07f, 0.07f, 0.4f), Quaternion.Euler(-30f, 0f, 0f));
        var luce = oggetto.AddComponent<Light>();
        luce.type = LightType.Point;
        luce.color = inc.colore;
        luce.range = 3.5f;
        luce.intensity = 1.2f;
        lupo.Prepara(chi, inc, inc.vitaEvocazione, inc.durata);
        return lupo;
    }

    void Parte(PrimitiveType tipo, Vector3 posizione, Vector3 scala, Quaternion? rotazione = null)
    {
        var p = Forma(tipo, transform, posizione, scala, colore, 1.2f);
        if (rotazione.HasValue) p.transform.localRotation = rotazione.Value;
        parti.Add(p.GetComponent<Renderer>());
    }

    protected override void Aggiorna()
    {
        Bersaglio nemico = NemicoVicino(Portata);
        if (nemico != null)
        {
            CamminaVerso(nemico.transform.position, Velocita, DistanzaMorso * 0.8f);
            if (Time.time >= prossimoMorso && Vector3.Distance(Piano(transform.position), Piano(nemico.transform.position)) <= DistanzaMorso + 0.3f)
            {
                prossimoMorso = Time.time + dati.intervalloColpi;
                padrone.DannoNelTempo(dati, nemico, dati.dannoEvocazione, 1f, transform.position, true);
                Suoni.Suona(Suono.ImpattoColpo, nemico.transform.position + Vector3.up, 0.7f, 1.3f);
                StartCoroutine(ControllaUccisione(nemico));
            }
        }
        else
        {
            // Nessun nemico: resta vicino allo Stregone.
            CamminaVerso(padrone.transform.position - padrone.transform.forward * 1.5f, Velocita, 1.2f);
        }
        AggiornaScia();
    }

    static Vector3 Piano(Vector3 v) => new Vector3(v.x, 0f, v.z);

    // Se il nemico morso muore, il lupo recupera vita (solo per le uccisioni sue).
    IEnumerator ControllaUccisione(Bersaglio nemico)
    {
        yield return new WaitForSeconds(0.3f);
        if (nemico != null && nemico.Morto) vita = Mathf.Min(vitaMassima, vita + dati.vitaPerUccisione);
    }

    // La scia: gli ultimi punti percorsi, lunghi "Lunghezza Scia" metri. Ogni mezzo secondo fa metà del danno al
    // secondo a ogni nemico che ci sta sopra.
    void AggiornaScia()
    {
        if (scia.Count == 0 || (scia[scia.Count - 1] - transform.position).sqrMagnitude > 0.04f) scia.Add(transform.position);
        float lunghezza = 0f;
        for (int i = scia.Count - 1; i > 0; i--)
        {
            lunghezza += Vector3.Distance(scia[i], scia[i - 1]);
            if (lunghezza > dati.lunghezzaScia) { scia.RemoveRange(0, i - 1); break; }
        }
        if (Time.time < prossimaScia || dati.dannoAlSecondo <= 0f) return;
        prossimaScia = Time.time + 0.5f;
        var colpiti = new HashSet<Bersaglio>();
        foreach (var punto in scia)
            foreach (var b in MagiaStregone.NemiciVicini(punto, 0.6f))
                if (colpiti.Add(b)) padrone.DannoNelTempo(dati, b, dati.dannoAlSecondo, 0.5f, punto, true);
    }

    protected override void Lampeggia()
    {
        foreach (var r in parti) if (r != null) r.sharedMaterial.color = Color.white;
        CancelInvoke(nameof(RipristinaColore));
        Invoke(nameof(RipristinaColore), 0.1f);
    }

    void RipristinaColore()
    {
        foreach (var r in parti) if (r != null) r.sharedMaterial.color = colore;
    }
}
