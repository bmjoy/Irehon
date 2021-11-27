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
        Debug.Log("South build done");
        SouthBuild();
    }
    [MenuItem("Build/Build Zones with client")]
    static void BuildServerZonesAndClient()
    {
        Debug.Log("Start build client");
        ClientBuild();
        BuildServerZones();
    }
    [MenuItem("Build/Build client")]
    static void BuildClient()
    {
        Debug.Log("Start build client");
        ClientBuild();
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
    
    static void SouthBuild()
    {
        string[] scenes = { "Assets/Scenes/South.unity" };
        BuildPipeline.BuildPlayer(scenes, "Builds/South/Irehon", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
        Debug.Log("South build done");
    }

    static void ClientBuild()
    {
        string[] scenes = { "Assets/Scenes/LoginScene.unity", "Assets/Scenes/FractionSelect.unity", "Assets/Scenes/Center.unity",
        "Assets/Scenes/North.unity"};
        BuildPipeline.BuildPlayer(scenes, "Builds/ClientWindows/Irehon.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("Client build done");
    }
}
