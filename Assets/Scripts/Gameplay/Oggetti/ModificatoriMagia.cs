using System.Collections.Generic;
using UnityEngine;

// Le quattro scuole di magia dello Stregone (Docs/incantesimi-stregone.md).
public enum ScuolaMagia { Brace, LagoNero, Ombra, Evocazione }

// Pro e contro che bastoni, libri, vesti e amuleti danno agli incantesimi dello Stregone.
// A cosa serve: ogni oggetto dello Stregone ha uno di questi blocchi (nell'Inspector, sezione "Magia").
// MagiaStregone li somma tutti e li applica a ogni incantesimo: costo, carica, recupero, attesa, danno,
// durata degli effetti, armatura ignorata, vita e durata delle evocazioni, caselle in più.
// Le percentuali si sommano tra loro (+15 e -5 = +10). I valori negativi sono i contro.
// Come si usa: non si monta. Si compila dentro DatiBastone, DatiLibro, DatiArmatura e DatiAmuleto.
[System.Serializable]
public class ModificatoriMagia
{
    [System.Serializable]
    public class PerScuola
    {
        [Tooltip("Danno in più o in meno degli incantesimi di questa scuola, in percentuale (15 = +15%).")]
        public float dannoPercento;
        [Tooltip("Durata di rallentamenti, blocchi e stordimenti di questa scuola, in percentuale (30 = +30%, -25 = -25%).")]
        public float durataEffettiPercento;
        [Tooltip("Quota dell'armatura del nemico ignorata dagli incantesimi di questa scuola (0,25 = un quarto).")]
        [Range(0f, 1f)] public float ignoraArmatura;

        public bool Vuoto => dannoPercento == 0f && durataEffettiPercento == 0f && ignoraArmatura == 0f;
    }

    [Header("Tutti gli incantesimi")]
    [Tooltip("Secondi di carica in più per ogni incantesimo (0,5 = +0,5 s).")]
    public float caricaSecondi;
    [Tooltip("Carica più lunga o più corta, in percentuale (10 = il 10% più lunga).")]
    public float caricaPercento;
    [Tooltip("Recupero più lungo o più corto, in percentuale (10 = il 10% più lungo).")]
    public float recuperoPercento;
    [Tooltip("Attesa prima di rilanciare lo stesso incantesimo, in percentuale (-15 = il 15% più corta).")]
    public float attesaPercento;
    [Tooltip("Costo in mana, in percentuale (17 = il 17% in più, -15 = il 15% in meno).")]
    public float costoPercento;
    [Tooltip("Danno degli incantesimi che fanno danno (non le evocazioni), in percentuale (-20 = il 20% in meno).")]
    public float dannoPercento;

    [Header("Per scuola")]
    public PerScuola brace = new PerScuola();
    public PerScuola lagoNero = new PerScuola();
    public PerScuola ombra = new PerScuola();
    public PerScuola evocazione = new PerScuola();

    [Header("Evocazioni")]
    [Tooltip("Vita delle evocazioni, in percentuale (25 = +25%).")]
    public float vitaEvocazioniPercento;
    [Tooltip("Durata delle evocazioni, in percentuale (30 = +30%, -30 = -30%).")]
    public float durataEvocazioniPercento;
    [Tooltip("Carica degli incantesimi di evocazione, in percentuale (-15 = il 15% più corta).")]
    public float caricaEvocazioniPercento;
    [Tooltip("Permette due Spiriti del lupo e due Bambole di ossa insieme (amuleto Occhio del lago).")]
    public bool doppiaEvocazione;

    [Header("Libri")]
    [Tooltip("Caselle per gli incantesimi in più (le caselle di base sono 4, al massimo 6).")]
    public int caselleExtra;

    public PerScuola Di(ScuolaMagia scuola)
    {
        switch (scuola)
        {
            case ScuolaMagia.Brace: return brace;
            case ScuolaMagia.LagoNero: return lagoNero;
            case ScuolaMagia.Ombra: return ombra;
            default: return evocazione;
        }
    }

    // Somma di più blocchi (bastone + libro + veste + amuleto).
    public static ModificatoriMagia Somma(IEnumerable<ModificatoriMagia> tutti)
    {
        var s = new ModificatoriMagia();
        foreach (var m in tutti)
        {
            if (m == null) continue;
            s.caricaSecondi += m.caricaSecondi;
            s.caricaPercento += m.caricaPercento;
            s.recuperoPercento += m.recuperoPercento;
            s.attesaPercento += m.attesaPercento;
            s.costoPercento += m.costoPercento;
            s.dannoPercento += m.dannoPercento;
            Aggiungi(s.brace, m.brace);
            Aggiungi(s.lagoNero, m.lagoNero);
            Aggiungi(s.ombra, m.ombra);
            Aggiungi(s.evocazione, m.evocazione);
            s.vitaEvocazioniPercento += m.vitaEvocazioniPercento;
            s.durataEvocazioniPercento += m.durataEvocazioniPercento;
            s.caricaEvocazioniPercento += m.caricaEvocazioniPercento;
            s.doppiaEvocazione |= m.doppiaEvocazione;
            s.caselleExtra += m.caselleExtra;
        }
        return s;
    }

    static void Aggiungi(PerScuola a, PerScuola b)
    {
        if (b == null) return;
        a.dannoPercento += b.dannoPercento;
        a.durataEffettiPercento += b.durataEffettiPercento;
        a.ignoraArmatura = Mathf.Clamp01(a.ignoraArmatura + b.ignoraArmatura);
    }

    // Le righe "pro e contro" da mostrare nell'inventario: chiave di Lingua dell'etichetta, eventuale scuola
    // (da scrivere davanti), valore e se più alto è meglio. Restituisce solo i valori diversi da zero.
    public void Descrivi(List<VoceEffetto> voci)
    {
        Voce(voci, "mod.carica_s", null, caricaSecondi, false, " s");
        Voce(voci, "mod.carica", null, caricaPercento, false);
        Voce(voci, "mod.recupero", null, recuperoPercento, false);
        Voce(voci, "mod.attesa", null, attesaPercento, false);
        Voce(voci, "mod.costo", null, costoPercento, false);
        Voce(voci, "mod.danno_incantesimi", null, dannoPercento, true);
        foreach (ScuolaMagia sc in System.Enum.GetValues(typeof(ScuolaMagia)))
        {
            var p = Di(sc);
            Voce(voci, "mod.danno", sc, p.dannoPercento, true);
            Voce(voci, "mod.durata_effetti", sc, p.durataEffettiPercento, true);
            Voce(voci, "mod.ignora_armatura", sc, p.ignoraArmatura * 100f, true);
        }
        Voce(voci, "mod.vita_evocazioni", null, vitaEvocazioniPercento, true);
        Voce(voci, "mod.durata_evocazioni", null, durataEvocazioniPercento, true);
        Voce(voci, "mod.carica_evocazioni", null, caricaEvocazioniPercento, false);
        if (doppiaEvocazione) voci.Add(new VoceEffetto { chiave = "mod.doppia_evocazione", testoValore = "", positivo = true });
        Voce(voci, "mod.caselle", null, caselleExtra, true, "");
    }

    static void Voce(List<VoceEffetto> voci, string chiave, ScuolaMagia? scuola, float valore, bool piuAltoMeglio, string unita = "%")
    {
        if (Mathf.Abs(valore) < 0.001f) return;
        voci.Add(new VoceEffetto
        {
            chiave = chiave,
            scuola = scuola,
            testoValore = (valore > 0f ? "+" : "") + Formatta(valore) + unita,
            positivo = (valore > 0f) == piuAltoMeglio,
        });
    }

    public static string Formatta(float v) =>
        Mathf.Approximately(v, Mathf.Round(v)) ? Mathf.RoundToInt(v).ToString() : v.ToString("0.#");
}

// Una riga "pro o contro" di un oggetto, per l'inventario (vedi ModificatoriMagia.Descrivi e InventarioGioco).
public struct VoceEffetto
{
    public string chiave;          // chiave di Lingua dell'etichetta, per esempio "mod.costo"
    public ScuolaMagia? scuola;    // se c'è, il nome della scuola va davanti all'etichetta
    public string testoValore;     // per esempio "+15%"
    public bool positivo;          // verde (pro) o rosso (contro)
    public bool neutro;            // né pro né contro (per esempio il tipo dell'amuleto): colore normale
}
