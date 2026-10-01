#if UNITY_EDITOR
using UnityEditor;

public sealed class RedMemoryPlayerSpriteImporter : AssetPostprocessor
{
    private const string LegacySpriteFolder = "Assets/Art/Characters/RedMemory/XiaoHe_Player/";
    private const string LiTuoTuoSpriteFolder = "Assets/Art/Characters/RedMemory/LiTuoTuo_Player/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(LegacySpriteFolder, System.StringComparison.Ordinal)
            && !assetPath.StartsWith(LiTuoTuoSpriteFolder, System.StringComparison.Ordinal)) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 512f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = UnityEngine.FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)UnityEngine.SpriteAlignment.Custom;
        settings.spritePivot = new UnityEngine.Vector2(0.5f, 0.055f);
        settings.spriteMeshType = UnityEngine.SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
    }
}
#endif
