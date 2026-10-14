using UnityEngine;

// Gli effetti a tempo delle pozioni sul giocatore (Lorenzo, 13 ottobre; numeri in Docs/erbe-e-pozioni.md):
//   - Danni subiti in meno (Tonico di cardo: -10% per 3 minuti);
//   - Mana massimo in più (Infuso di lunaria: +5% per 3 minuti).
// Regola delle pozioni cumulabili (solo Tonico e Infuso): gli effetti si sommano, la durata no. Tre Tonici =
// danni subiti -30%; ogni pozione bevuta fa ripartire da capo i 3 minuti di tutta la somma, che finisce tutta insieme.
// Le pozioni non cumulabili (Balsamo, Elisir) hanno il loro effetto con il loro tempo; non si sommano a niente:
// conta il più forte fra la somma dei cumulabili e il loro (Balsamo -10% con tre Tonici attivi = sempre -30%).
// I danni subiti non scendono mai sotto il 10% (al massimo -90%). Quando un effetto finisce, il mana che supera il
// nuovo massimo si perde. Morendo finisce tutto.
// Li applica GiocatoreControllo (RiceviColpo, RiceviDannoAmbiente, ManaMassimo); a schermo li mostra HudGioco con
// un segnalino accanto alla barra della vita (niente conto alla rovescia: il segnalino si svuota e lampeggia alla fine).
// Come montarlo: non serve, si aggiunge da solo al giocatore la prima volta che beve una pozione (EffettiPozioni.Di).
public class EffettiPozioni : MonoBehaviour
{
    public enum Tipo { RiduzioneDanni, ManaMassimo }
    const int Tipi = 2;

    // Per ogni tipo: la somma delle pozioni cumulabili (con un solo tempo) e l'effetto non cumulabile più forte.
    readonly float[] somma = new float[Tipi];
    readonly float[] fineSomma = new float[Tipi];
    readonly float[] fisso = new float[Tipi];
    readonly float[] fineFisso = new float[Tipi];
    // Quanto durava l'effetto quando è ripartito l'ultima volta (per il segnalino che si svuota).
    readonly float[] durata = new float[Tipi];

    // Aggiunge l'effetto di una pozione.
    public void Aggiungi(Tipo tipo, float percento, float secondi, bool cumulabile)
    {
        if (percento <= 0f || secondi <= 0f) return;
        int t = (int)tipo;
        if (cumulabile)
        {
            if (Time.time >= fineSomma[t]) somma[t] = 0f;   // la somma vecchia era già finita
            somma[t] += percento;
            fineSomma[t] = Time.time + secondi;            // ogni pozione fa ripartire il tempo di tutta la somma
            durata[t] = secondi;
            return;
        }
        // non cumulabile: prende il posto del precedente se è più forte o se quello è finito; altrimenti allunga solo il tempo
        if (Time.time >= fineFisso[t] || percento >= fisso[t]) fisso[t] = percento;
        fineFisso[t] = Mathf.Max(fineFisso[t], Time.time + secondi);
        durata[t] = Mathf.Max(secondi, fineFisso[t] - Time.time);
    }

    // Da 1 (appena bevuta) a 0 (finito): quanto resta dell'effetto, per il segnalino.
    public float ParteRimasta(Tipo tipo)
    {
        int t = (int)tipo;
        return durata[t] <= 0f ? 0f : Mathf.Clamp01(SecondiRimasti(tipo) / durata[t]);
    }

    // L'effetto attivo di un tipo, in percento: il più forte fra la somma dei cumulabili e il non cumulabile.
    public float Totale(Tipo tipo)
    {
        int t = (int)tipo;
        float s = Time.time < fineSomma[t] ? somma[t] : 0f;
        float f = Time.time < fineFisso[t] ? fisso[t] : 0f;
        return Mathf.Max(s, f);
    }

    // Secondi che restano all'effetto che conta adesso (0 = nessuno).
    public float SecondiRimasti(Tipo tipo)
    {
        int t = (int)tipo;
        float s = Time.time < fineSomma[t] ? somma[t] : 0f;
        float f = Time.time < fineFisso[t] ? fisso[t] : 0f;
        float fine = s >= f ? fineSomma[t] : fineFisso[t];
        if (s > 0f && f > 0f && Mathf.Approximately(s, f)) fine = Mathf.Max(fineSomma[t], fineFisso[t]);
        return Mathf.Max(0f, fine - Time.time);
    }

    // Per quanto moltiplicare un danno ricevuto (0,9 = -10%), mai sotto 0,1.
    public float MoltiplicatoreDanni => Mathf.Max(0.1f, 1f - Totale(Tipo.RiduzioneDanni) / 100f);
    // Per quanto moltiplicare il mana massimo (1,05 = +5%).
    public float MoltiplicatoreMana => 1f + Totale(Tipo.ManaMassimo) / 100f;

    public void Azzera()
    {
        for (int t = 0; t < Tipi; t++) { somma[t] = 0f; fineSomma[t] = 0f; fisso[t] = 0f; fineFisso[t] = 0f; durata[t] = 0f; }
    }

    // Gli effetti del giocatore (li aggiunge se mancano).
    public static EffettiPozioni Di(Component giocatore)
    {
        if (giocatore == null) return null;
        var e = giocatore.GetComponent<EffettiPozioni>();
        if (e == null) e = giocatore.gameObject.AddComponent<EffettiPozioni>();
        return e;
    }

    // "3 min", "90 s" (nelle 8 lingue).
    public static string Durata(float secondi)
    {
        int s = Mathf.RoundToInt(secondi);
        return s >= 60 && s % 60 == 0 ? string.Format(Lingua.T("eff.minuti"), s / 60) : string.Format(Lingua.T("eff.secondi"), s);
    }

    // "2:41", per il tempo che resta.
    public static string Orologio(float secondi)
    {
        int s = Mathf.CeilToInt(secondi);
        return (s / 60) + ":" + (s % 60).ToString("00");
    }
}
