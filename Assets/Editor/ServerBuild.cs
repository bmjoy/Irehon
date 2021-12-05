using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ServerBuild
{
    private static readonly string[] gameZones = { "North", "South", "Center" };

    [MenuItem("Build/Build Zones")]
    static void BuildServerZones()
    {
        foreach (string zone in gameZones)
            BuildServerZone(zone);
    }

    [MenuItem("Build/Build Zones with client")]
    static void BuildServerZonesAndClient()
    {
        Debug.Log("Start build client");
        ClientBuild(gameZones);
        BuildServerZones();
    }

    [MenuItem("Build/Build client")]
    static void BuildClient()
    {
        Debug.Log("Start build client");
        ClientBuild(gameZones);
    }

    static void BuildServerZone(string zone)
    {
        string[] scenes = { $"Assets/Scenes/{zone}.unity" };
        BuildPipeline.BuildPlayer(scenes, $"Builds/{zone}/Irehon", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
        Debug.Log($"{zone} build done");
    }

    static void ClientBuild(string[] zones)
    {
        List<string> scenes = new List<string>{ "Assets/Scenes/LoginScene.unity", "Assets/Scenes/FractionSelect.unity" };
        foreach (string zone in gameZones)
            scenes.Add($"Assets/Scenes/{zone}.unity");
        BuildPipeline.BuildPlayer(scenes.ToArray(), "Builds/ClientWindows/Irehon.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("Client build done");
    }
}
