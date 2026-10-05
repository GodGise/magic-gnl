using UnityEngine;

// Le tre classi giocabili del gioco.
public enum ClasseGiocatore { Guerriero, Ladro, Stregone }

// Ricorda le scelte fatte nel menu prima di iniziare una partita (per ora: la classe).
// A cosa serve: il menu iniziale salva qui la classe scelta; il gioco la legge quando la scena parte,
// per esempio con: if (SceltaPartita.Classe == ClasseGiocatore.Stregone) { ... }
// La scelta resta salvata anche chiudendo il gioco (PlayerPrefs).
// Come montarlo: non si monta su nessun oggetto, si usa direttamente dal codice.
public static class SceltaPartita
{
    const string ChiaveClasse = "ClasseScelta";

    public static ClasseGiocatore Classe
    {
        get => (ClasseGiocatore)PlayerPrefs.GetInt(ChiaveClasse, (int)ClasseGiocatore.Guerriero);
        set { PlayerPrefs.SetInt(ChiaveClasse, (int)value); PlayerPrefs.Save(); }
    }
}
