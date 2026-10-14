using UnityEngine;

// Lo scontro con un boss, per le regole del co-op (Lorenzo, 10 ottobre; vedi GiocatoreControllo, "a terra"):
//   - c'è uno scontro quando un boss vivo, entro "Raggio" metri, insegue qualcuno o è stato colpito da poco
//     (lo scontro resta "aperto" per 15 s dopo l'ultimo inseguimento o colpo, così una pausa breve non lo chiude);
//   - in uno scontro con un boss ogni giocatore può andare a terra una sola volta, la seconda diventa spettatore;
//   - se tutto il gruppo va a terra (o da soli si muore), i boss in scontro ricominciano da capo: tornano al loro
//     posto con la vita piena (Bersaglio.Ripristina) e i giocatori rinascono al checkpoint.
// Funziona su tutti i PC (chi non ospita sa dall'host se il boss insegue e quando viene colpito); il ripristino
// del boss lo fa solo l'host, o il giocatore da solo.
// Come montarlo: non si monta. È una raccolta di funzioni usata da GiocatoreControllo.
public static class CombattimentoBoss
{
    // Distanza entro cui un boss "conta" come scontro in corso.
    public const float Raggio = 40f;
    // Per questi secondi dopo l'ultimo colpo o l'ultimo inseguimento il boss è ancora in scontro.
    const float SecondiDopoScontro = 15f;

    // Il boss in scontro più vicino a "punto", entro Raggio metri; null se non c'è.
    public static Bersaglio Vicino(Vector3 punto)
    {
        Bersaglio migliore = null;
        float minima = Raggio * Raggio;
        foreach (var b in RegistroNemici.Tutti)
        {
            if (!InScontro(b)) continue;
            float d = (b.transform.position - punto).sqrMagnitude;
            if (d > minima) continue;
            minima = d;
            migliore = b;
        }
        return migliore;
    }

    static bool InScontro(Bersaglio b)
    {
        if (b == null || !b.Boss || b.Morto) return false;
        if (b.TryGetComponent(out InseguimentoNemico vista) && vista.StaInseguendo) return true;
        return Time.time - Mathf.Max(b.UltimoColpo, b.UltimoInseguimento) < SecondiDopoScontro;
    }

    // Il gruppo è stato sconfitto: ogni boss in scontro torna al suo posto con la vita piena (solo host o da soli).
    public static void Ripristina()
    {
        if (!Rete.ComandaIlMondo) return;
        foreach (var b in RegistroNemici.Tutti)
            if (InScontro(b)) b.Ripristina();
    }
}
