using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Editor utility that builds the extended 360 tour for Meta Quest 2.
/// </summary>
public static class TourBuilder
{
    // Output folder and file name of the Quest 2 build
    private const string BuildFolder = "Builds/MetaQuest2";
    private const string BuildFile = "ExtendedImmersiveTour_MetaQuest2.apk";

    /// <summary>
    /// Scenes included in the build, in load order. The main menu opens first.
    /// </summary>
    public static readonly string[] Scenes =
    {
        "Assets/Scenes/MainMenuScene.unity",
        "Assets/Scenes/IntranetTourScene.unity",
        "Assets/Scenes/CustomCampusTourScene.unity"
    };

    /// <summary>
    /// Builds all tour scenes as an Android APK for Meta Quest 2.
    /// </summary>
    [MenuItem("Tools/Extended Tour/Build APK (Meta Quest 2)")]
    public static void BuildQuest2()
    {
        Directory.CreateDirectory(BuildFolder);

        var options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = Path.Combine(BuildFolder, BuildFile),
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Quest 2 build finished: " + report.summary.result + " (" + report.summary.totalSize + " bytes)");
    }
}
