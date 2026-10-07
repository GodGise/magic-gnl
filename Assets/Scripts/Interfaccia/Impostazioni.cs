using UnityEngine;

// Impostazioni del gioco scelte dal giocatore: volume generale, volume della musica, effetto retro PS2
// (la lingua è in Lingua.cs, lo schermo intero lo ricorda Unity da solo).
// Restano salvate anche chiudendo il gioco. Le usano il menu iniziale e il menu di pausa,
// che mostrano le stesse voci di "Opzioni" con AggiungiVoci().
// Come montarlo: non si monta su nessun oggetto, si usa direttamente dal codice.
public static class Impostazioni
{
    const string ChiaveVolume = "VolumeGenerale", ChiaveMusica = "VolumeMusica", ChiaveRetro = "EffettoRetro";

    public static float VolumeGenerale
    {
        get => PlayerPrefs.GetFloat(ChiaveVolume, 0.8f);
        set { PlayerPrefs.SetFloat(ChiaveVolume, Mathf.Clamp01(value)); Applica(); }
    }

    public static float VolumeMusica
    {
        get => PlayerPrefs.GetFloat(ChiaveMusica, 0.7f);
        set { PlayerPrefs.SetFloat(ChiaveMusica, Mathf.Clamp01(value)); Applica(); }
    }

    public static bool EffettoRetro
    {
        get => PlayerPrefs.GetInt(ChiaveRetro, 1) == 1;
        set { PlayerPrefs.SetInt(ChiaveRetro, value ? 1 : 0); Applica(); }
    }

    // Applica le impostazioni salvate a quello che c'è in scena.
    public static void Applica()
    {
        PlayerPrefs.Save();
        AudioListener.volume = VolumeGenerale;
        bool retro = EffettoRetro;
        foreach (var effetto in Object.FindObjectsByType<global::EffettoRetro>(FindObjectsSortMode.None))
            effetto.attivo = retro;
    }

    // Aggiunge all'elenco le voci delle opzioni (lingua, volumi, schermo intero, effetto retro) e "Indietro".
    public static void AggiungiVoci(ElencoMenu elenco, System.Action indietro)
    {
        elenco.voci.Add(new VoceMenu
        {
            testo = () => Lingua.T("opzioni.lingua"),
            valore = () => Lingua.NomeAttuale,
            conferma = () => Lingua.Indice = Lingua.Indice + 1,
            regola = d => Lingua.Indice = Lingua.Indice + d,
        });
        elenco.voci.Add(new VoceMenu
        {
            testo = () => Lingua.T("opzioni.volume_generale"),
            valore = () => Mathf.RoundToInt(VolumeGenerale * 100) + "%",
            conferma = () => VolumeGenerale = Passo(VolumeGenerale, 1, true),
            regola = d => VolumeGenerale = Passo(VolumeGenerale, d, false),
        });
        elenco.voci.Add(new VoceMenu
        {
            testo = () => Lingua.T("opzioni.volume_musica"),
            valore = () => Mathf.RoundToInt(VolumeMusica * 100) + "%",
            conferma = () => VolumeMusica = Passo(VolumeMusica, 1, true),
            regola = d => VolumeMusica = Passo(VolumeMusica, d, false),
        });
        elenco.voci.Add(new VoceMenu
        {
            testo = () => Lingua.T("opzioni.schermo_intero"),
            valore = () => SiNo(Screen.fullScreen),
            conferma = () => Screen.fullScreen = !Screen.fullScreen,
            regola = d => Screen.fullScreen = !Screen.fullScreen,
        });
        elenco.voci.Add(new VoceMenu
        {
            testo = () => Lingua.T("opzioni.effetto_retro"),
            valore = () => SiNo(EffettoRetro),
            conferma = () => EffettoRetro = !EffettoRetro,
            regola = d => EffettoRetro = !EffettoRetro,
        });
        elenco.Aggiungi(() => Lingua.T("menu.indietro"), indietro);
    }

    public static string SiNo(bool valore) => Lingua.T(valore ? "comune.si" : "comune.no");

    // Volume a passi del 10%; con "giraIntorno" dopo il 100% si torna a 0.
    static float Passo(float volume, int direzione, bool giraIntorno)
    {
        float nuovo = Mathf.Round((volume + 0.1f * direzione) * 10f) / 10f;
        if (giraIntorno && nuovo > 1.001f) nuovo = 0f;
        return Mathf.Clamp01(nuovo);
    }
}
