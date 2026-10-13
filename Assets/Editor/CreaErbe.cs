using UnityEditor;
using UnityEngine;

// Strumento dell'editor: crea, se mancano, i fiori, le pozioni e le ricette della bottega di Ilse
// (proposta di Lorenzo del 13 ottobre, numeri e idee in Docs/erbe-e-pozioni.md). Ogni fiore ha uno scopo:
//   - Fiore di riva (acqua bassa del lago)      -> vita
//   - Cardo nero (fra i sassi dei campi incolti) -> difesa: -10% danni subiti per 3 minuti
//   - Lunaria (si apre di notte, vicino a tombe e rovine) -> mana massimo +5% per 3 minuti
// (effetti cumulabili, EffettiPozioni; nello zaino al massimo 3 pozioni all'inizio del gioco)
// I file sono già nella repository: questo comando serve solo a ricrearli se qualcuno li cancella.
// Fiori in   Assets/Dati/Oggetti/Missioni/    (fiore-di-riva, cardo-nero, lunaria)
// Pozioni in Assets/Dati/Oggetti/Consumabili/ (pozione-di-ilse, tonico-di-cardo, infuso-di-lunaria, balsamo-di-riva,
//                                              elisir-del-crepuscolo)
// Ricette in Assets/Dati/Ricette/             (prefisso "ilse-"): baratto (fiori -> pozione), compra (Lacrime del
//                                              lago -> pozione, costa molto di più) e vendi (fiore -> Lacrime)
// Quelli che esistono già non vengono toccati: i numeri si cambiano poi dall'Inspector, oggetto per oggetto.
// Lo usa anche "magic-gnl > Prologo: prepara la quest 4" per riempire la bottega di Ilse.
// Per mettere i nuovi fiori nel mondo: oggetto vuoto + componente OggettoRaccoglibile con "Oggetto" = il fiore.
// Come si usa: menu "magic-gnl > Erbe: crea fiori, pozioni e ricette". Non va montato su niente: è un comando
// dell'editor e non finisce nel gioco.
public static class CreaErbe
{
    const string Missioni = "Assets/Dati/Oggetti/Missioni/";
    const string Consumabili = "Assets/Dati/Oggetti/Consumabili/";
    const string Ricette = "Assets/Dati/Ricette/";

    [MenuItem("magic-gnl/Erbe: crea fiori, pozioni e ricette")]
    static void Menu()
    {
        var r = RicetteIlse();
        EditorUtility.DisplayDialog("Erbe", "Pronte " + r.Length + " voci della bottega di Ilse, 3 fiori e 5 pozioni in Assets/Dati (quelli che c'erano già non sono cambiati).", "Ok");
        Selection.activeObject = r[0];
    }

    // ---------- i fiori ----------

    internal static DatiOggettoMissione FioreDiRiva => Fiore("fiore-di-riva", "Fiore di riva", "fiore_di_riva", new Color(0.6f, 0.82f, 1f));
    internal static DatiOggettoMissione CardoNero => Fiore("cardo-nero", "Cardo nero", "cardo_nero", new Color(0.42f, 0.18f, 0.5f));
    internal static DatiOggettoMissione Lunaria => Fiore("lunaria", "Lunaria", "lunaria", new Color(0.88f, 0.88f, 0.97f));

    // ---------- le pozioni ----------

    // +60 vita in 1 s: la pozione della quest 4.
    internal static DatiConsumabile PozioneDiIlse => Pozione("pozione-di-ilse", "Pozione di Ilse", "pozione_di_ilse", new Color(0.75f, 0.12f, 0.1f),
        p => { p.cura = 60f; p.durataCura = 1f; p.attesa = 1.2f; });
    // -10% danni subiti per 3 minuti (cumulabile: tre tonici = -30%, ogni tonico fa ripartire i 3 minuti)
    internal static DatiConsumabile TonicoDiCardo => Pozione("tonico-di-cardo", "Tonico di cardo", "tonico_di_cardo", new Color(0.6f, 0.68f, 0.15f),
        p => { p.cura = 0f; p.riduzioneDanniPercento = 10f; p.durataEffetto = 180f; p.attesa = 1.2f; });
    // +5% mana massimo per 3 minuti (cumulabile)
    internal static DatiConsumabile InfusoDiLunaria => Pozione("infuso-di-lunaria", "Infuso di lunaria", "infuso_di_lunaria", new Color(0.25f, 0.45f, 0.95f),
        p => { p.cura = 0f; p.manaMassimoPercento = 5f; p.durataEffetto = 180f; p.attesa = 1.2f; });
    // +100 vita in 7 s e -10% danni subiti per 3 minuti (non cumulabile)
    internal static DatiConsumabile BalsamoDiRiva => Pozione("balsamo-di-riva", "Balsamo di riva", "balsamo_di_riva", new Color(0.85f, 0.25f, 0.3f),
        p => { p.cura = 100f; p.durataCura = 7f; p.riduzioneDanniPercento = 10f; p.durataEffetto = 180f; p.cumulabile = false; p.attesa = 2f; });
    // un po' di tutto: +40 vita, -10% danni subiti e +5% mana massimo per 3 minuti (non cumulabile)
    internal static DatiConsumabile ElisirDelCrepuscolo => Pozione("elisir-del-crepuscolo", "Elisir del crepuscolo", "elisir_del_crepuscolo", new Color(0.55f, 0.25f, 0.75f),
        p => { p.cura = 40f; p.durataCura = 1f; p.riduzioneDanniPercento = 10f; p.manaMassimoPercento = 5f; p.durataEffetto = 180f; p.cumulabile = false; p.attesa = 1.5f; });

    // ---------- le voci della bottega di Ilse (prezzi provvisori in Lacrime del lago, Docs/monete.md) ----------

    internal static DatiRicetta[] RicetteIlse()
    {
        var riva = FioreDiRiva;
        var cardo = CardoNero;
        var lunaria = Lunaria;
        return new[]
        {
            // baratto: il modo più conveniente
            Ricetta("ilse-pozione-di-ilse", PozioneDiIlse, 2, 0, 0, (riva, 3)),
            Ricetta("ilse-tonico-di-cardo", TonicoDiCardo, 1, 0, 0, (cardo, 2)),
            Ricetta("ilse-infuso-di-lunaria", InfusoDiLunaria, 1, 0, 0, (lunaria, 2)),
            Ricetta("ilse-balsamo-di-riva", BalsamoDiRiva, 1, 0, 0, (riva, 2), (cardo, 1)),
            Ricetta("ilse-elisir-del-crepuscolo", ElisirDelCrepuscolo, 1, 0, 0, (riva, 1), (cardo, 1), (lunaria, 1)),
            // compra con le Lacrime: quando mancano i fiori (circa 3-5 volte più caro dei fiori venduti)
            Ricetta("ilse-compra-pozione-di-ilse", PozioneDiIlse, 1, 30, 0),
            Ricetta("ilse-compra-tonico-di-cardo", TonicoDiCardo, 1, 35, 0),
            Ricetta("ilse-compra-infuso-di-lunaria", InfusoDiLunaria, 1, 40, 0),
            Ricetta("ilse-compra-balsamo-di-riva", BalsamoDiRiva, 1, 70, 0),
            Ricetta("ilse-compra-elisir-del-crepuscolo", ElisirDelCrepuscolo, 1, 90, 0),
            // vendi i fiori a Ilse
            Ricetta("ilse-vendi-fiore-di-riva", null, 1, 0, 4, (riva, 1)),
            Ricetta("ilse-vendi-cardo-nero", null, 1, 0, 5, (cardo, 1)),
            Ricetta("ilse-vendi-lunaria", null, 1, 0, 7, (lunaria, 1)),
        };
    }

    // ---------- creazione ----------

    static DatiOggettoMissione Fiore(string file, string nome, string chiave, Color colore)
    {
        string percorso = Missioni + file + ".asset";
        var f = AssetDatabase.LoadAssetAtPath<DatiOggettoMissione>(percorso);
        if (f != null) return f;
        Cartella("Assets/Dati/Oggetti", "Missioni");
        f = ScriptableObject.CreateInstance<DatiOggettoMissione>();
        f.nomeDiLavoro = nome;
        f.chiaveNome = "oggetto." + chiave + ".nome";
        f.chiaveDescrizione = "oggetto." + chiave + ".descrizione";
        f.tutteLeClassi = true;
        f.colore = colore;
        f.forma = DatiOggettoMissione.FormaProvvisoria.Fiore;
        AssetDatabase.CreateAsset(f, percorso);
        AssetDatabase.SaveAssets();
        return f;
    }

    static DatiConsumabile Pozione(string file, string nome, string chiave, Color colore, System.Action<DatiConsumabile> numeri)
    {
        string percorso = Consumabili + file + ".asset";
        var p = AssetDatabase.LoadAssetAtPath<DatiConsumabile>(percorso);
        if (p != null) return p;
        Cartella("Assets/Dati/Oggetti", "Consumabili");
        p = ScriptableObject.CreateInstance<DatiConsumabile>();
        p.nomeDiLavoro = nome;
        p.chiaveNome = "oggetto." + chiave + ".nome";
        p.chiaveDescrizione = "oggetto." + chiave + ".descrizione";
        p.tutteLeClassi = true;
        p.colore = colore;
        numeri(p);
        AssetDatabase.CreateAsset(p, percorso);
        AssetDatabase.SaveAssets();
        return p;
    }

    static DatiRicetta Ricetta(string file, DatiOggetto risultato, int quanti, int prezzo, int ricavo, params (DatiOggetto oggetto, int quanti)[] chiede)
    {
        string percorso = Ricette + file + ".asset";
        var r = AssetDatabase.LoadAssetAtPath<DatiRicetta>(percorso);
        if (r != null) return r;
        Cartella("Assets/Dati", "Ricette");
        r = ScriptableObject.CreateInstance<DatiRicetta>();
        r.da = risultato;
        r.quantiDa = quanti;
        r.prezzo = prezzo;
        r.ricavo = ricavo;
        r.chiede = new DatiRicetta.Ingrediente[chiede.Length];
        for (int i = 0; i < chiede.Length; i++) r.chiede[i] = new DatiRicetta.Ingrediente { oggetto = chiede[i].oggetto, quanti = chiede[i].quanti };
        AssetDatabase.CreateAsset(r, percorso);
        AssetDatabase.SaveAssets();
        return r;
    }

    static void Cartella(string genitore, string nome)
    {
        if (!AssetDatabase.IsValidFolder(genitore + "/" + nome)) AssetDatabase.CreateFolder(genitore, nome);
    }
}
