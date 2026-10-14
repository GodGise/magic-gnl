using UnityEngine;

// Impostazioni del gioco scelte dal giocatore: volume generale, della musica e degli effetti, effetto retro PS2,
// luminosità, sensibilità della camera e asse verticale invertito
// (la lingua è in Lingua.cs, lo schermo intero lo ricorda Unity da solo).
// Restano salvate anche chiudendo il gioco. Le usano il menu iniziale e il menu di pausa,
// che mostrano le stesse voci di "Opzioni" con AggiungiVoci(): una pagina con Lingua e le sezioni
// Audio, Video e Controlli (una sola schermata con tutte le voci non entrerebbe nello schermo).
// Come montarlo: non si monta su nessun oggetto, si usa direttamente dal codice.
public static class Impostazioni
{
    const string ChiaveVolume = "VolumeGenerale", ChiaveMusica = "VolumeMusica", ChiaveRetro = "EffettoRetro";
    const string ChiaveEffetti = "VolumeEffetti", ChiaveLuminosita = "Luminosita";
    const string ChiaveSensibilita = "SensibilitaCamera", ChiaveAsseInvertito = "AsseVerticaleInvertito";

    // Le pagine delle opzioni.
    public enum Sezione { Principale, Audio, Video, Controlli }

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

    // Volume degli effetti (passi, colpi, suoni del menu); la musica ha il suo.
    public static float VolumeEffetti
    {
        get => PlayerPrefs.GetFloat(ChiaveEffetti, 1f);
        set { PlayerPrefs.SetFloat(ChiaveEffetti, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    // Luminosità da 0 a 1: 0,5 non cambia niente, sotto scurisce, sopra schiarisce (vedi Luminosita.cs).
    public static float Luminosita
    {
        get => PlayerPrefs.GetFloat(ChiaveLuminosita, 0.5f);
        set { PlayerPrefs.SetFloat(ChiaveLuminosita, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    // Quanto è veloce la camera, da 0,2 a 2 (1 = come prima). Moltiplica la sensibilità di mouse e pad.
    public static float SensibilitaCamera
    {
        get => Mathf.Clamp(PlayerPrefs.GetFloat(ChiaveSensibilita, 1f), 0.2f, 2f);
        set { PlayerPrefs.SetFloat(ChiaveSensibilita, Mathf.Clamp(value, 0.2f, 2f)); PlayerPrefs.Save(); }
    }

    // Se attivo, muovere il mouse (o la levetta) in su fa guardare in basso.
    public static bool AsseVerticaleInvertito
    {
        get => PlayerPrefs.GetInt(ChiaveAsseInvertito, 0) == 1;
        set { PlayerPrefs.SetInt(ChiaveAsseInvertito, value ? 1 : 0); PlayerPrefs.Save(); }
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

    // Aggiunge all'elenco le voci di una pagina delle opzioni e "Indietro".
    // "sezione": quale pagina (Principale: lingua e le tre sezioni; Audio; Video; Controlli).
    // "apri": cosa fare quando si sceglie una sezione (il menu cambia pagina). "indietro": cosa fa "Indietro".
    public static void AggiungiVoci(ElencoMenu elenco, Sezione sezione, System.Action<Sezione> apri, System.Action indietro)
    {
        switch (sezione)
        {
            case Sezione.Principale:
                elenco.voci.Add(new VoceMenu
                {
                    testo = () => Lingua.T("opzioni.lingua"),
                    valore = () => Lingua.NomeAttuale,
                    conferma = () => Lingua.Indice = Lingua.Indice + 1,
                    regola = d => Lingua.Indice = Lingua.Indice + d,
                });
                elenco.Aggiungi(() => Lingua.T("opzioni.sezione_audio"), () => apri(Sezione.Audio));
                elenco.Aggiungi(() => Lingua.T("opzioni.sezione_video"), () => apri(Sezione.Video));
                elenco.Aggiungi(() => Lingua.T("opzioni.sezione_controlli"), () => apri(Sezione.Controlli));
                break;

            case Sezione.Audio:
                elenco.voci.Add(VoceVolume("opzioni.volume_generale", () => VolumeGenerale, v => VolumeGenerale = v));
                elenco.voci.Add(VoceVolume("opzioni.volume_musica", () => VolumeMusica, v => VolumeMusica = v));
                elenco.voci.Add(VoceVolume("opzioni.volume_effetti", () => VolumeEffetti, v => VolumeEffetti = v));
                break;

            case Sezione.Video:
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
                elenco.voci.Add(VoceVolume("opzioni.luminosita", () => Luminosita, v => Luminosita = v));
                break;

            case Sezione.Controlli:
                elenco.voci.Add(new VoceMenu
                {
                    testo = () => Lingua.T("opzioni.sensibilita_camera"),
                    valore = () => Mathf.RoundToInt(SensibilitaCamera * 100) + "%",
                    conferma = () => SensibilitaCamera = PassoSensibilita(SensibilitaCamera, 1, true),
                    regola = d => SensibilitaCamera = PassoSensibilita(SensibilitaCamera, d, false),
                });
                elenco.voci.Add(new VoceMenu
                {
                    testo = () => Lingua.T("opzioni.asse_invertito"),
                    valore = () => SiNo(AsseVerticaleInvertito),
                    conferma = () => AsseVerticaleInvertito = !AsseVerticaleInvertito,
                    regola = d => AsseVerticaleInvertito = !AsseVerticaleInvertito,
                });
                // Comandi: apre la schermata con la tastiera e il cambio dei tasti (MenuComandi)
                elenco.Aggiungi(() => Lingua.T("menu.comandi"), MenuComandi.Apri);
                break;
        }
        elenco.Aggiungi(() => Lingua.T("menu.indietro"), indietro);
    }

    // Una voce con un valore da 0 a 100% a passi del 10% (volumi e luminosità).
    static VoceMenu VoceVolume(string chiave, System.Func<float> leggi, System.Action<float> scrivi)
    {
        return new VoceMenu
        {
            testo = () => Lingua.T(chiave),
            valore = () => Mathf.RoundToInt(leggi() * 100) + "%",
            conferma = () => { scrivi(Passo(leggi(), 1, true)); Applica(); },
            regola = d => { scrivi(Passo(leggi(), d, false)); Applica(); },
        };
    }

    // Sensibilità a passi del 10%, da 20% a 200%; con "giraIntorno" dopo il 200% si torna al 20%.
    static float PassoSensibilita(float valore, int direzione, bool giraIntorno)
    {
        float nuovo = Mathf.Round((valore + 0.1f * direzione) * 10f) / 10f;
        if (giraIntorno && nuovo > 2.001f) nuovo = 0.2f;
        return Mathf.Clamp(nuovo, 0.2f, 2f);
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
