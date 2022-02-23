using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ServerBuild : Editor
{
    private static readonly string[] gameZones = { "North", "South", "Center" };


    [MenuItem("Build/Build server Zones")]
    static void BuildServerZones()
    {
        var popUp = CreateInstance<BuildVersionPopUp>();
        popUp.SetButtonAction(BuildAllServerZones);
        popUp.ShowPopup();
    }

    [MenuItem("Build/Build North server")]
    static void BuildServerNorth()
    {
        var popUp = CreateInstance<BuildVersionPopUp>();
        popUp.SetButtonAction(() => BuildServerZone("North"));
        popUp.ShowPopup();
    }
    
    static void BuildAllServerZones()
    {
        foreach (string zone in gameZones)
            if (!BuildServerZone(zone))
            {
                Debug.LogError("Build error");
                break;
            }
    }

    static void BuildServerAndClientClient()
    {
        BuildAllServerZones();
        BuildClientZones();
    }

    static void BuildClientZones()
    {
        Debug.Log("Start build client");
        if (!ClientBuild(gameZones))
            Debug.LogError("Build error");
    }


    [MenuItem("Build/Build server Zones with client")]
    static void BuildServerZonesAndClient()
    {
        var popUp = CreateInstance<BuildVersionPopUp>();
        popUp.SetButtonAction(BuildServerAndClientClient);
        popUp.ShowPopup();
    }

    [MenuItem("Build/Build client")]
    static void BuildClient()
    {
        var popUp = CreateInstance<BuildVersionPopUp>();
        popUp.SetButtonAction(BuildClientZones);
        popUp.ShowPopup();
    }

    static bool BuildServerZone(string zone)
    {
        string[] scenes = { $"Assets/Scenes/{zone}.unity" };
        var report = BuildPipeline.BuildPlayer(scenes, $"Builds/{zone}/Irehon", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
        Debug.Log($"{zone} build done");
        return report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
    }

    static bool ClientBuild(string[] zones)
    {
        List<string> scenes = new List<string>{ "Assets/Scenes/LoginScene.unity", "Assets/Scenes/FractionSelect.unity" };
        foreach (string zone in gameZones)
            scenes.Add($"Assets/Scenes/{zone}.unity");
        var report = BuildPipeline.BuildPlayer(scenes.ToArray(), "Builds/ClientWindows/Irehon.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("Client build done");
        return report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
    }
}
