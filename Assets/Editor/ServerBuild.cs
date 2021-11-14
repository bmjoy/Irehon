using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ServerBuild
{
    [MenuItem("Build/Build Zones")]
    static void BuildServerZones()
    {
        Debug.Log("Start build north zone");
        NorthBuild();
        Debug.Log("Start build center zone");
        CenterBuild();
    }
    [MenuItem("Build/Build Zones with client")]
    static void BuildServerZonesAndClient()
    {
        Debug.Log("Start build client");
        ClientBuild();
        BuildServerZones();
    }
    static void NorthBuild()
    {
        string[] scenes = { "Assets/Scenes/North.unity" };
        BuildPipeline.BuildPlayer(scenes, "Builds/North/Irehon", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
        Debug.Log("North build done");
    }

    static void CenterBuild()
    {
        string[] scenes = { "Assets/Scenes/Center.unity" };
        BuildPipeline.BuildPlayer(scenes, "Builds/Center/Irehon", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
        Debug.Log("Center build done");
    }

    static void ClientBuild()
    {
        string[] scenes = { "Assets/Scenes/LoginScene.unity", "Assets/Scenes/FractionSelect.unity", "Assets/Scenes/Center.unity",
        "Assets/Scenes/North.unity"};
        BuildPipeline.BuildPlayer(scenes, "Builds/ClientWindows/Irehon.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("Client build done");
    }
}
