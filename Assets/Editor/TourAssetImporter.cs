using UnityEditor;
using UnityEngine;

/// <summary>
/// Import rules for the extended tour's images:
/// 360 photos are kept at 4096x2048 for Meta Quest 2, UI images become sprites,
/// and the rounded panel sprite gets 9-slice borders.
/// </summary>
public class TourAssetImporter : AssetPostprocessor
{
    // Folder holding the custom campus 360 media
    private const string CampusFolder = "Assets/CampusMedia/";

    // Folder holding menu and navigation UI images
    private const string UiFolder = "Assets/UI/";

    // Equirectangular backdrop used by the main menu
    private const string BackdropPath = "Assets/UI/MenuBackdrop.jpg";

    // 9-slice panel sprite
    private const string PanelPath = "Assets/UI/RoundedPanel.png";

    /// <summary>
    /// Version of these import rules. Changing it reimports the affected images.
    /// </summary>
    /// <returns>The rules version number.</returns>
    public override uint GetVersion()
    {
        return 1;
    }

    // Applies texture settings before an image in the tour folders is imported
    private void OnPreprocessTexture()
    {
        var importer = (TextureImporter)assetImporter;

        if (assetPath.StartsWith(CampusFolder) || assetPath == BackdropPath)
        {
            ConfigurePanorama(importer);
        }
        else if (assetPath.StartsWith(UiFolder))
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;

            if (assetPath == PanelPath)
            {
                importer.spriteBorder = new Vector4(64f, 64f, 64f, 64f);
            }
        }
    }

    // Settings for equirectangular 360 images shown on the inside of a sphere
    private static void ConfigurePanorama(TextureImporter importer)
    {
        importer.textureType = TextureImporterType.Default;
        importer.textureShape = TextureImporterShape.Texture2D;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 4096;
        importer.mipmapEnabled = true;
        importer.wrapModeU = TextureWrapMode.Repeat;
        importer.wrapModeV = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Trilinear;
        importer.anisoLevel = 4;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.None;

        var android = importer.GetPlatformTextureSettings("Android");
        android.overridden = true;
        android.maxTextureSize = 4096;
        android.format = TextureImporterFormat.ASTC_6x6;
        importer.SetPlatformTextureSettings(android);
    }
}
