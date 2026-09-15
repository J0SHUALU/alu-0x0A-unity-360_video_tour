using UnityEditor;
using UnityEngine;

/// <summary>
/// Import rules for the 360 video clips. On Android (Meta Quest 2) the clips are
/// transcoded to 4096x2048 H.264 so the headset can decode them smoothly, keeping
/// the 2:1 equirectangular ratio of the 5248x2624 source files.
/// </summary>
public class VideoImportSettings : AssetPostprocessor
{
    // Folder holding the 360 source clips
    private const string VideoFolder = "Assets/Videos/";

    // Target size of the transcoded clips, matching the render textures
    private const int TargetWidth = 4096;
    private const int TargetHeight = 2048;

    /// <summary>
    /// Version of these import rules. Changing it reimports the affected clips.
    /// </summary>
    /// <returns>The rules version number.</returns>
    public override uint GetVersion()
    {
        return 1;
    }

    // Applies the Android transcode settings before a clip in the Videos folder is imported
    private void OnPreprocessAsset()
    {
        if (!assetPath.StartsWith(VideoFolder) || !assetPath.EndsWith(".mp4"))
            return;

        var importer = assetImporter as VideoClipImporter;
        if (importer == null)
            return;

        var settings = importer.GetTargetSettings("Android") ?? new VideoImporterTargetSettings();
        settings.enableTranscoding = true;
        settings.codec = VideoCodec.H264;
        settings.resizeMode = VideoResizeMode.CustomSize;
        settings.aspectRatio = VideoEncodeAspectRatio.Stretch;
        settings.customWidth = TargetWidth;
        settings.customHeight = TargetHeight;
        settings.bitrateMode = VideoBitrateMode.High;
        settings.spatialQuality = VideoSpatialQuality.HighSpatialQuality;
        importer.SetTargetSettings("Android", settings);
    }
}
