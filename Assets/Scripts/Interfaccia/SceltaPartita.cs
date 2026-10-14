using UnityEngine;

// Le tre classi giocabili del gioco.
public enum ClasseGiocatore { Guerriero, Ladro, Stregone }

// Ricorda le scelte fatte nel menu prima di iniziare una partita: la classe e il nome del personaggio.
// A cosa serve: il menu iniziale salva qui la classe e il nome scelti (vedi Salvataggio.cs: il personaggio dello slot
// attivo); il gioco li legge quando la scena parte,
// per esempio con: if (SceltaPartita.Classe == ClasseGiocatore.Stregone) { ... }
// La scelta resta salvata anche chiudendo il gioco (PlayerPrefs).
// Come montarlo: non si monta su nessun oggetto, si usa direttamente dal codice.
public static class SceltaPartita
{
    const string ChiaveClasse = "ClasseScelta", ChiaveNome = "NomePersonaggio";

    // Il nome del personaggio con cui si gioca (vuoto nelle scene di prova: in co-op si usa "Giocatore 1, 2, 3").
    public static string Nome
    {
        get => PlayerPrefs.GetString(ChiaveNome, "");
        set { PlayerPrefs.SetString(ChiaveNome, value ?? ""); PlayerPrefs.Save(); }
    }

    public static ClasseGiocatore Classe
    {
        get => (ClasseGiocatore)PlayerPrefs.GetInt(ChiaveClasse, (int)ClasseGiocatore.Guerriero);
        set { PlayerPrefs.SetInt(ChiaveClasse, (int)value); PlayerPrefs.Save(); }
    }
}
