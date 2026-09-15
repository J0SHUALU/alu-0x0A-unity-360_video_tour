using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Editor utility that builds the 360 video tour for Meta Quest 2.
/// </summary>
public static class TourBuilder
{
    // Output folder and file name of the Quest 2 build
    private const string BuildFolder = "Builds/MetaQuest2";
    private const string BuildFile = "360VideoTour_MetaQuest2.apk";

    /// <summary>
    /// Builds the 360VideoTour scene as an Android APK for Meta Quest 2.
    /// </summary>
    [MenuItem("Tools/Build 360VideoTour (Meta Quest 2)")]
    public static void BuildQuest2()
    {
        Directory.CreateDirectory(BuildFolder);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/360VideoTour.unity" },
            locationPathName = Path.Combine(BuildFolder, BuildFile),
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Quest 2 build finished: " + report.summary.result + " (" + report.summary.totalSize + " bytes)");
    }
}
