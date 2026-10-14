using UnityEngine;

// Un oggetto che si usa e sparisce: le pozioni della bottega di Ilse (Docs/erbe-e-pozioni.md). Ogni pozione può
// ridare vita, mana e resistenza e, per "Durata Effetto" secondi, ridurre i danni subiti, alzare il mana massimo o far
// ricaricare più in fretta la resistenza (EffettiPozioni); i valori a zero non contano.
// Nello zaino si portano al massimo 3 consumabili all'inizio del gioco (Zaino, "Massimo Consumabili").
// Come si usa in partita: sta nello zaino; il tasto "Usa oggetto" (F all'inizio, si cambia in Opzioni > Comandi)
// usa quello scelto. Nell'inventario (scheda Varie) E o doppio clic sceglie quale usare con F (UsoConsumabili).
// Più copie dello stesso oggetto = più voci uguali nello zaino.
// Come si crea: pannello Project, tasto destro > Create > magic-gnl > Oggetti > Consumabile, poi nome e descrizione
// come chiavi di Lingua (per esempio oggetto.pozione_di_ilse.nome) e la spunta "Tutte Le Classi".
[CreateAssetMenu(fileName = "nuovo-consumabile", menuName = "magic-gnl/Oggetti/Consumabile")]
public class DatiConsumabile : DatiOggetto
{
    [Header("Effetto")]
    [Tooltip("Vita recuperata.")]
    [Min(0f)] public float cura = 60f;
    [Tooltip("In quanti secondi arriva tutta la cura (0 = subito).")]
    [Min(0f)] public float durataCura = 1f;
    [Tooltip("Mana ridato (serve allo Stregone e a chi usa il bastone).")]
    [Min(0f)] public float mana = 0f;
    [Tooltip("Resistenza ridata subito.")]
    [Min(0f)] public float resistenza = 0f;
    [Tooltip("Ricarica della resistenza più veloce, in percento (50 = +50%), per i secondi di \"Durata Effetto\".")]
    [Min(0f)] public float recuperoResistenzaPercento = 0f;
    [Tooltip("Danni subiti in meno, in percento (10 = -10%), per i secondi di \"Durata Effetto\" (Tonico di cardo).")]
    [Min(0f)] public float riduzioneDanniPercento = 0f;
    [Tooltip("Mana massimo in più, in percento (5 = +5%), per i secondi di \"Durata Effetto\" (Infuso di lunaria).")]
    [Min(0f)] public float manaMassimoPercento = 0f;
    [Tooltip("Secondi degli effetti a tempo (180 = 3 minuti).")]
    [Min(0f)] public float durataEffetto = 0f;
    [Tooltip("Se attivo (Tonico e Infuso), bevendone più d'una gli effetti a tempo si sommano e ogni pozione fa ripartire il tempo di tutta la somma. Se spento, l'effetto non si somma a niente.")]
    public bool cumulabile = true;
    [Tooltip("Secondi prima di poter usare un altro oggetto.")]
    [Min(0f)] public float attesa = 1.2f;
    [Tooltip("Colore della forma provvisoria (la boccetta) finché Nazar non fa il modello.")]
    public Color colore = new Color(0.75f, 0.15f, 0.12f);

    // Gli effetti in una riga, per esempio "+60 Vita  ·  +40 Mana" (messaggio dopo l'uso, bottega).
    public string Effetti()
    {
        var parti = new System.Collections.Generic.List<string>();
        if (cura > 0f) parti.Add("+" + Mathf.RoundToInt(cura) + " " + Lingua.T("stat.vita"));
        if (mana > 0f) parti.Add("+" + Mathf.RoundToInt(mana) + " " + Lingua.T("stat.mana"));
        if (resistenza > 0f) parti.Add("+" + Mathf.RoundToInt(resistenza) + " " + Lingua.T("stat.resistenza"));
        if (durataEffetto > 0f)
        {
            string durata = EffettiPozioni.Durata(durataEffetto);
            if (riduzioneDanniPercento > 0f) parti.Add(string.Format(Lingua.T("eff.riduzione_danni"), Mathf.RoundToInt(riduzioneDanniPercento), durata));
            if (manaMassimoPercento > 0f) parti.Add(string.Format(Lingua.T("eff.mana_massimo"), Mathf.RoundToInt(manaMassimoPercento), durata));
            if (recuperoResistenzaPercento > 0f) parti.Add(string.Format(Lingua.T("eff.recupero_resistenza"), Mathf.RoundToInt(recuperoResistenzaPercento), durata));
        }
        return string.Join(GraficaMenu.Separatore, parti);
    }
}
