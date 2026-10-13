using System.Collections.Generic;
using UnityEngine;

// Gli effetti a tempo delle pozioni sul giocatore (Lorenzo, 13 ottobre; numeri in Docs/erbe-e-pozioni.md):
//   - Danni subiti in meno (Tonico di cardo: -10% per 3 minuti);
//   - Mana massimo in più (Infuso di lunaria: +5% per 3 minuti).
// Sono cumulabili: due tonici bevuti insieme = -20%, ognuno con il suo tempo. I danni subiti non scendono mai
// sotto il 10% (al massimo -90%). Quando un effetto finisce, il mana che supera il nuovo massimo si perde.
// Li applica GiocatoreControllo (RiceviColpo, RiceviDannoAmbiente, ManaMassimo); a schermo li mostra UsoConsumabili
// (sopra il rombo della pozione, con il tempo che resta).
// Come montarlo: non serve, si aggiunge da solo al giocatore la prima volta che beve una pozione (EffettiPozioni.Di).
public class EffettiPozioni : MonoBehaviour
{
    public enum Tipo { RiduzioneDanni, ManaMassimo }

    struct Effetto
    {
        public Tipo tipo;
        public float percento;
        public float fine;
    }

    readonly List<Effetto> attivi = new List<Effetto>();

    // Aggiunge un effetto. Se non è cumulabile, prende il posto di quelli dello stesso tipo.
    public void Aggiungi(Tipo tipo, float percento, float secondi, bool cumulabile)
    {
        if (percento <= 0f || secondi <= 0f) return;
        if (!cumulabile) attivi.RemoveAll(e => e.tipo == tipo);
        attivi.Add(new Effetto { tipo = tipo, percento = percento, fine = Time.time + secondi });
    }

    // Somma dei percento attivi di un tipo (toglie quelli finiti).
    public float Totale(Tipo tipo)
    {
        attivi.RemoveAll(e => Time.time >= e.fine);
        float somma = 0f;
        foreach (var e in attivi) if (e.tipo == tipo) somma += e.percento;
        return somma;
    }

    // Secondi che restano all'effetto di quel tipo che dura di più (0 = nessuno).
    public float SecondiRimasti(Tipo tipo)
    {
        float fine = 0f;
        foreach (var e in attivi) if (e.tipo == tipo && e.fine > fine) fine = e.fine;
        return Mathf.Max(0f, fine - Time.time);
    }

    // Per quanto moltiplicare un danno ricevuto (0,9 = -10%), mai sotto 0,1.
    public float MoltiplicatoreDanni => Mathf.Max(0.1f, 1f - Totale(Tipo.RiduzioneDanni) / 100f);
    // Per quanto moltiplicare il mana massimo (1,05 = +5%).
    public float MoltiplicatoreMana => 1f + Totale(Tipo.ManaMassimo) / 100f;

    public void Azzera() => attivi.Clear();

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
