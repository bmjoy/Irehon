using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ServerBuild
{
    [MenuItem("Build/Build Zones")]
    static void BuildAllZones()
    {
        Debug.Log("Start build north zone");
        NorthBuild();
        Debug.Log("Start build center zone");
        CenterBuild();
    }
    static void NorthBuild()
    {
        string[] scenes = { "Assets/Scenes/North.unity" };
        BuildPipeline.BuildPlayer(scenes, "Builds/North/Irehon.exe", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
        Debug.Log("North build done");
    }

    static void CenterBuild()
    {
        string[] scenes = { "Assets/Scenes/Center.unity" };
        BuildPipeline.BuildPlayer(scenes, "Builds/Center/Irehon.exe", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
        Debug.Log("Center build done");
    }


}
