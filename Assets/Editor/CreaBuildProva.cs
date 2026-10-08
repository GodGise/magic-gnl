using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Crea una build di prova del gioco per Windows, per provare il co-op con due finestre sullo stesso PC
// (una build + l'editor in Play). Usa le scene delle Build Settings (prima il Menu).
// La build va in Builds/Prova/magic-gnl.exe (la cartella Builds/ non va su GitHub, vedi .gitignore).
// Come si usa: menu "magic-gnl > Crea build di prova (Windows)", poi "magic-gnl > Avvia build di prova" per aprirla
// (anche più volte, per avere più giocatori). Non va montato su niente.
public static class CreaBuildProva
{
    const string Cartella = "Builds/Prova";

    [MenuItem("magic-gnl/Crea build di prova (Windows)")]
    static void Crea()
    {
        Directory.CreateDirectory(Cartella);
        var opzioni = new BuildPlayerOptions
        {
            scenes = ScenePresenti(),
            locationPathName = Cartella + "/magic-gnl.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development,
        };
        BuildReport esito = BuildPipeline.BuildPlayer(opzioni);
        if (esito.summary.result == BuildResult.Succeeded)
            Debug.Log("[Build] Build di prova pronta: " + Path.GetFullPath(opzioni.locationPathName));
        else
            Debug.LogError("[Build] Build non riuscita: " + esito.summary.result + " (errori: " + esito.summary.totalErrors + ")");
    }

    [MenuItem("magic-gnl/Avvia build di prova")]
    static void Avvia()
    {
        string exe = Path.GetFullPath(Cartella + "/magic-gnl.exe");
        if (!File.Exists(exe)) { Debug.LogWarning("[Build] Prima crea la build: menu magic-gnl > Crea build di prova (Windows)."); return; }
        // Finestra piccola, così si vedono insieme la build e l'editor.
        System.Diagnostics.Process.Start(exe, "-screen-fullscreen 0 -screen-width 960 -screen-height 540");
    }

    static string[] ScenePresenti()
    {
        var elenco = new System.Collections.Generic.List<string>();
        foreach (var scena in EditorBuildSettings.scenes)
            if (scena.enabled) elenco.Add(scena.path);
        return elenco.ToArray();
    }
}
