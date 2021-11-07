using UnityEditor;

public class BuildGame
{
    public static void BuildGameServers()
    {
        BuildServerNorth();
        BuildServerCenter();
    }

    public static void BuildServerNorth()
    {
        string[] scenes = { "Assets/Scenes/North.unity" };
        BuildPipeline.BuildPlayer(scenes, "builds/North/North.exe", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
    }

    public static void BuildServerCenter()
    {
        string[] scenes = { "Assets/Scenes/Center.unity" };
        BuildPipeline.BuildPlayer(scenes, "builds/Center/Center.exe", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
    }
}
