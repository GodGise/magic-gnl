using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Crea una build di prova del gioco per Windows, per provare il co-op con due finestre sullo stesso PC
// (una build + l'editor in Play). Usa le scene delle Build Settings (prima il Menu).
// La build va in Builds/Prova/magic-gnl.exe (la cartella Builds/ non va su GitHub, vedi .gitignore).
// Come si usa: menu "magic-gnl > Crea build di prova (Windows)". Non va montato su niente.
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

    static string[] ScenePresenti()
    {
        var elenco = new System.Collections.Generic.List<string>();
        foreach (var scena in EditorBuildSettings.scenes)
            if (scena.enabled) elenco.Add(scena.path);
        return elenco.ToArray();
    }
}
