using UnityEngine;

// I tre livelli di difficoltà scelti dal giocatore: Normale, Difficile, Estremo.
public enum LivelloDifficolta { Normale, Difficile, Estremo }

// La difficoltà scelta dal giocatore (da non confondere con DifficoltaCoop, che la applica ai nemici).
// A cosa serve: si parte su Normale. Il livello si può cambiare UNA SOLA VOLTA, dopo una conferma
// ("Sei sicuro? Non potrai più cambiarla"): dopo il cambio la scelta è definitiva e resta salvata
// anche chiudendo il gioco (PlayerPrefs, quindi per ora è per questo PC e non per salvataggio).
// Il menu iniziale la mostra in Opzioni (vedi MenuPrincipale); il menu di pausa la mostra senza poterla cambiare.
// In co-op conta la scelta di chi ospita la partita: gli altri ricevono il livello dall'host (vedi MondoRete).
// Come montarlo: non si monta su nessun oggetto, si usa direttamente dal codice.
public static class Difficolta
{
    const string ChiaveLivello = "DifficoltaScelta", ChiaveBloccata = "DifficoltaBloccata";

    public static readonly string[] Chiavi = { "difficolta.normale", "difficolta.difficile", "difficolta.estremo" };
    public static readonly string[] ChiaviDescrizione = { "difficolta.desc_normale", "difficolta.desc_difficile", "difficolta.desc_estremo" };

    public static LivelloDifficolta Livello =>
        (LivelloDifficolta)Mathf.Clamp(PlayerPrefs.GetInt(ChiaveLivello, (int)LivelloDifficolta.Normale), 0, Chiavi.Length - 1);

    // Vero dopo che il giocatore ha cambiato la difficoltà una volta: da allora non si può più cambiare.
    public static bool Bloccata => PlayerPrefs.GetInt(ChiaveBloccata, 0) == 1;

    public static string Nome(LivelloDifficolta livello) => Lingua.T(Chiavi[(int)livello]);

    // Scrive la scelta e la rende definitiva. Se è già bloccata, o è lo stesso livello, non fa niente.
    // Chi la chiama deve aver già chiesto la conferma al giocatore.
    public static bool Scegli(LivelloDifficolta nuovo)
    {
        if (Bloccata || nuovo == Livello) return false;
        PlayerPrefs.SetInt(ChiaveLivello, (int)nuovo);
        PlayerPrefs.SetInt(ChiaveBloccata, 1);
        PlayerPrefs.Save();
        return true;
    }
}
