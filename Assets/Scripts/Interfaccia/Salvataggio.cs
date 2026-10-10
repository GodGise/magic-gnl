using System;
using System.IO;
using UnityEngine;

// I dati di un personaggio salvato in uno slot. Per ora: nome e classe (e il tempo di gioco, che per ora resta a zero).
// Quando il salvataggio vero sarà pronto (ai falò e ai checkpoint) qui si aggiungono oggetti, posizione e progressi.
// "versione" serve a leggere in futuro anche i salvataggi vecchi quando i campi cambiano.
[Serializable]
public class DatiSlot
{
    public int versione = 1;
    public string nome;
    public int classe;          // ClasseGiocatore
    public float secondiGioco;
    public string ultimoSalvataggio;   // data e ora, per mostrarle nel menu
}

// Gli slot di salvataggio dei personaggi (5), uno per file in una cartella del computer di chi gioca:
//   Windows: %USERPROFILE%/AppData/LocalLow/<azienda>/<gioco>/Salvataggi/slot1.json ... slot5.json
// A cosa serve: il menu iniziale ci fa scegliere (o creare) il personaggio prima di giocare: slot, poi nome, poi classe.
// Il personaggio scelto diventa "attivo": nome e classe vanno in SceltaPartita e il gioco li legge da lì.
// In co-op ogni giocatore ha i suoi slot sul proprio computer; il mondo lo salverà chi ospita (da fare).
// Come montarlo: non si monta su nessun oggetto, si usa direttamente dal codice (Salvataggio.Esiste(0), ...).
public static class Salvataggio
{
    public const int NumeroSlot = 5;
    public const int LunghezzaMassimaNome = 16;

    // Lo slot del personaggio con cui si sta giocando (-1 = nessuno, per esempio nelle scene di prova).
    public static int SlotAttivo { get; private set; } = -1;

    static string Cartella => Path.Combine(Application.persistentDataPath, "Salvataggi");
    static string Percorso(int slot) => Path.Combine(Cartella, "slot" + (slot + 1) + ".json");

    public static bool Esiste(int slot) => Leggi(slot) != null;

    public static bool QualcunoEsiste
    {
        get
        {
            for (int i = 0; i < NumeroSlot; i++) if (Esiste(i)) return true;
            return false;
        }
    }

    // Il personaggio dello slot, o null se è vuoto (o il file è rovinato).
    public static DatiSlot Leggi(int slot)
    {
        if (slot < 0 || slot >= NumeroSlot) return null;
        try
        {
            string percorso = Percorso(slot);
            if (!File.Exists(percorso)) return null;
            var dati = JsonUtility.FromJson<DatiSlot>(File.ReadAllText(percorso));
            return dati != null && !string.IsNullOrEmpty(dati.nome) ? dati : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Salvataggio] Non riesco a leggere lo slot " + (slot + 1) + ": " + e.Message);
            return null;
        }
    }

    // Crea un personaggio nuovo nello slot (sostituendo quello che c'era) e lo rende attivo. Vero se è riuscito.
    public static bool Crea(int slot, string nome, ClasseGiocatore classe)
    {
        var dati = new DatiSlot
        {
            nome = PulisciNome(nome),
            classe = (int)classe,
            secondiGioco = 0f,
            ultimoSalvataggio = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
        };
        if (string.IsNullOrEmpty(dati.nome) || !Scrivi(slot, dati)) return false;
        Attiva(slot);
        return true;
    }

    // Rende attivo il personaggio dello slot: il gioco userà il suo nome e la sua classe.
    public static void Attiva(int slot)
    {
        var dati = Leggi(slot);
        if (dati == null) return;
        SlotAttivo = slot;
        SceltaPartita.Nome = dati.nome;
        SceltaPartita.Classe = (ClasseGiocatore)Mathf.Clamp(dati.classe, 0, 2);
    }

    public static void Elimina(int slot)
    {
        try
        {
            string percorso = Percorso(slot);
            if (File.Exists(percorso)) File.Delete(percorso);
            if (SlotAttivo == slot) SlotAttivo = -1;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Salvataggio] Non riesco a cancellare lo slot " + (slot + 1) + ": " + e.Message);
        }
    }

    // Scrive in sicurezza: prima su un file temporaneo, poi lo sostituisce, così uno stop a metà non rovina il salvataggio.
    static bool Scrivi(int slot, DatiSlot dati)
    {
        try
        {
            Directory.CreateDirectory(Cartella);
            string percorso = Percorso(slot), temporaneo = percorso + ".tmp";
            File.WriteAllText(temporaneo, JsonUtility.ToJson(dati, true));
            if (File.Exists(percorso)) File.Delete(percorso);
            File.Move(temporaneo, percorso);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Salvataggio] Non riesco a scrivere lo slot " + (slot + 1) + ": " + e.Message);
            return false;
        }
    }

    // Un carattere si accetta nel nome se è una lettera latina (anche con accento), un numero, uno spazio, un apostrofo o un trattino.
    // Solo caratteri latini: i caratteri dei menu e quelli sopra la testa in co-op non hanno gli altri alfabeti.
    public static bool CarattereValido(char c) =>
        c < 0x250 && (char.IsLetterOrDigit(c) || c == ' ' || c == '\'' || c == '-');

    // Toglie gli spazi in più e i caratteri non validi e accorcia il nome alla lunghezza massima.
    public static string PulisciNome(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return "";
        var testo = new System.Text.StringBuilder();
        foreach (char c in nome)
        {
            if (!CarattereValido(c)) continue;
            if (c == ' ' && (testo.Length == 0 || testo[testo.Length - 1] == ' ')) continue;
            testo.Append(c);
        }
        string pulito = testo.ToString().TrimEnd();
        return pulito.Length > LunghezzaMassimaNome ? pulito.Substring(0, LunghezzaMassimaNome).TrimEnd() : pulito;
    }
}
