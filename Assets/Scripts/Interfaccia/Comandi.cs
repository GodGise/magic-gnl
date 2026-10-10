using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Le azioni del giocatore che si possono assegnare a un altro tasto (Opzioni > Comandi, schermata MenuComandi).
// Solo tastiera e mouse: i tasti del pad restano quelli scritti negli script.
public enum Azione { Avanti, Indietro, Sinistra, Destra, Corsa, Schiva, Attacca, Para, Interagisci, Abilita, Aggancia, Inventario }

// Elenco unico dei tasti del gioco, con quelli scelti dal giocatore (salvati anche chiudendo il gioco).
// Come si usa negli script: si crea l'InputAction come sempre, con il tasto predefinito, e subito dopo si chiama
//     Comandi.Collega(comandoInteragisci, Azione.Interagisci, this);
// Comandi trova il tasto predefinito dell'azione e lo sostituisce con quello scelto dal giocatore; se il giocatore
// lo cambia mentre gioca, il cambio arriva da solo a tutte le azioni collegate.
// Per un tasto letto senza InputAction: Comandi.PremutoOra(Azione.Inventario).
// Per scriverlo a schermo ("E  Apri"): Comandi.NomeTasto(Azione.Interagisci).
// Un tasto nuovo che il giocatore può cambiare va aggiunto all'enum Azione, a "predefiniti", a "chiaviNomi"
// e a Lingua.cs (chiave "comando.<nome>").
// Come montarlo: non si monta su nessun oggetto, si usa dal codice.
public static class Comandi
{
    static readonly string[] predefiniti =
    {
        "<Keyboard>/w", "<Keyboard>/s", "<Keyboard>/a", "<Keyboard>/d",
        "<Keyboard>/shift", "<Keyboard>/space", "<Mouse>/leftButton", "<Mouse>/rightButton",
        "<Keyboard>/e", "<Keyboard>/q", "<Mouse>/middleButton", "<Keyboard>/tab",
    };

    static readonly string[] chiaviNomi =
    {
        "comando.avanti", "comando.indietro", "comando.sinistra", "comando.destra",
        "comando.corsa", "comando.schiva", "comando.attacca", "comando.para",
        "comando.interagisci", "comando.abilita", "comando.aggancia", "comando.inventario",
    };

    // Tasti che non si possono usare perché hanno già un compito fisso: Esc (pausa), 1-6 (armi e incantesimi),
    // F1 (pannello di prova), F2 (effetto retro).
    static readonly string[] riservati =
    {
        "<Keyboard>/escape", "<Keyboard>/1", "<Keyboard>/2", "<Keyboard>/3", "<Keyboard>/4", "<Keyboard>/5",
        "<Keyboard>/6", "<Keyboard>/f1", "<Keyboard>/f2",
    };

    public static int Numero => predefiniti.Length;

    struct Collegamento
    {
        public InputAction azione;
        public int indice;
        public Azione quale;
        public Object proprietario;   // lo script che usa l'azione: quando è distrutto, il collegamento si toglie
    }

    static string[] attuali;
    static readonly List<Collegamento> collegati = new List<Collegamento>();

    static string ChiaveSalvataggio(Azione a) => "Comando." + a;

    static void Carica()
    {
        if (attuali != null) return;
        attuali = new string[predefiniti.Length];
        for (int i = 0; i < attuali.Length; i++)
            attuali[i] = PlayerPrefs.GetString(ChiaveSalvataggio((Azione)i), predefiniti[i]);
    }

    public static string Percorso(Azione a)
    {
        Carica();
        return attuali[(int)a];
    }

    public static string NomeAzione(Azione a) => Lingua.T(chiaviNomi[(int)a]);
    public static string NomeTasto(Azione a) => NomeTasto(Percorso(a));

    // Collega un'azione del gioco: cerca il tasto predefinito fra i suoi tasti e lo sostituisce con quello scelto.
    public static void Collega(InputAction azione, Azione quale, Object proprietario)
    {
        if (azione == null) return;
        Carica();
        // via i collegamenti degli oggetti già distrutti (oggetti raccolti, scene cambiate)
        collegati.RemoveAll(c => c.proprietario == null);
        string predefinito = predefiniti[(int)quale];
        var tasti = azione.bindings;
        for (int i = 0; i < tasti.Count; i++)
        {
            if (!string.Equals(tasti[i].path, predefinito, System.StringComparison.OrdinalIgnoreCase)) continue;
            collegati.Add(new Collegamento { azione = azione, indice = i, quale = quale, proprietario = proprietario });
            Applica(azione, i, quale);
            return;
        }
        Debug.LogWarning("Comandi: l'azione " + azione.name + " non ha il tasto " + predefinito);
    }

    static void Applica(InputAction azione, int indice, Azione quale)
    {
        string scelto = attuali[(int)quale];
        if (scelto == predefiniti[(int)quale]) azione.RemoveBindingOverride(indice);
        else azione.ApplyBindingOverride(indice, scelto);
    }

    static void RiapplicaTutti()
    {
        for (int i = collegati.Count - 1; i >= 0; i--)
        {
            var c = collegati[i];
            if (c.proprietario == null) { collegati.RemoveAt(i); continue; }
            try { Applica(c.azione, c.indice, c.quale); }
            catch (System.Exception) { collegati.RemoveAt(i); }   // azione già distrutta (oggetto raccolto, scena cambiata)
        }
    }

    public static bool Riservato(string percorso)
    {
        foreach (var r in riservati)
            if (string.Equals(r, percorso, System.StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    // Assegna un tasto a un'azione. Se lo usava già un'altra azione, le due si scambiano i tasti.
    // Restituisce l'azione che ha ricevuto il vecchio tasto, oppure -1.
    public static int Imposta(Azione quale, string percorso)
    {
        Carica();
        int scambiata = -1;
        string vecchio = attuali[(int)quale];
        for (int i = 0; i < attuali.Length; i++)
        {
            if (i == (int)quale || !(Combacia(attuali[i], percorso) || Combacia(percorso, attuali[i]))) continue;
            attuali[i] = vecchio;
            scambiata = i;
        }
        attuali[(int)quale] = percorso;
        Salva();
        return scambiata;
    }

    public static void RipristinaTutti()
    {
        Carica();
        for (int i = 0; i < attuali.Length; i++) attuali[i] = predefiniti[i];
        Salva();
    }

    static void Salva()
    {
        for (int i = 0; i < attuali.Length; i++)
        {
            if (attuali[i] == predefiniti[i]) PlayerPrefs.DeleteKey(ChiaveSalvataggio((Azione)i));
            else PlayerPrefs.SetString(ChiaveSalvataggio((Azione)i), attuali[i]);
        }
        PlayerPrefs.Save();
        RiapplicaTutti();
    }

    // Per i tasti letti senza InputAction (per esempio l'inventario).
    public static bool PremutoOra(Azione a)
    {
        var controllo = InputSystem.FindControl(Percorso(a)) as ButtonControl;
        return controllo != null && controllo.wasPressedThisFrame;
    }

    // Quale azione usa questo tasto (per la tastiera disegnata), oppure -1.
    public static int AzioneDelTasto(string percorso)
    {
        Carica();
        for (int i = 0; i < attuali.Length; i++)
            if (Combacia(attuali[i], percorso)) return i;
        return -1;
    }

    // "<Keyboard>/shift" vale sia per Shift sinistro sia per il destro (lo stesso per Ctrl e Alt).
    public static bool Combacia(string scelto, string tasto)
    {
        if (string.Equals(scelto, tasto, System.StringComparison.OrdinalIgnoreCase)) return true;
        string[] doppi = { "shift", "ctrl", "alt" };
        foreach (var d in doppi)
            if (string.Equals(scelto, "<Keyboard>/" + d, System.StringComparison.OrdinalIgnoreCase)
                && (string.Equals(tasto, "<Keyboard>/left" + d, System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(tasto, "<Keyboard>/right" + d, System.StringComparison.OrdinalIgnoreCase)))
                return true;
        return false;
    }

    // Nome del tasto da scrivere a schermo, nella lingua scelta per spazio, invio e tasti del mouse.
    public static string NomeTasto(string percorso)
    {
        if (string.IsNullOrEmpty(percorso)) return "—";
        switch (percorso.ToLowerInvariant())
        {
            case "<mouse>/leftbutton": return Lingua.T("tasto.mouse_sx");
            case "<mouse>/rightbutton": return Lingua.T("tasto.mouse_dx");
            case "<mouse>/middlebutton": return Lingua.T("tasto.mouse_centro");
            case "<mouse>/backbutton": return "Mouse 4";
            case "<mouse>/forwardbutton": return "Mouse 5";
            case "<keyboard>/space": return Lingua.T("tasto.spazio");
            case "<keyboard>/enter": return Lingua.T("tasto.invio");
            case "<keyboard>/shift": return "Shift";
            case "<keyboard>/ctrl": return "Ctrl";
            case "<keyboard>/alt": return "Alt";
        }
        // il nome che il sistema dà al tasto, secondo la tastiera del giocatore (per esempio Z su una AZERTY)
        var controllo = InputSystem.FindControl(percorso);
        if (controllo != null && !string.IsNullOrEmpty(controllo.displayName)) return controllo.displayName.ToUpperInvariant();
        int barra = percorso.LastIndexOf('/');
        return (barra >= 0 ? percorso.Substring(barra + 1) : percorso).ToUpperInvariant();
    }
}
