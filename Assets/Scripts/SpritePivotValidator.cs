using UnityEditor;
using UnityEngine;

public static class SpritePivotValidator
{
    // Change this to the pivot you want.
    // (0.5, 0.5) = center
    // (0.5, 0)   = bottom center
    // (0, 0)     = bottom left
    private static readonly Vector2 TargetPivot = new Vector2(0.5f, 0.5f);

    private const float Tolerance = 0.0001f;

    [MenuItem("Tools/Sprites/Fix Selected Sprite Pivots")]
    public static void FixSelectedSpritePivots()
    {
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(path))
                continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                continue;

            if (importer.textureType != TextureImporterType.Sprite)
                continue;

            bool changed = false;

            if (importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                SpriteMetaData[] spritesheet = importer.spritesheet;

                for (int i = 0; i < spritesheet.Length; i++)
                {
                    SpriteMetaData sprite = spritesheet[i];

                    if (Vector2.Distance(sprite.pivot, TargetPivot) > Tolerance ||
                        sprite.alignment != (int)SpriteAlignment.Custom)
                    {
                        sprite.alignment = (int)SpriteAlignment.Custom;
                        sprite.pivot = TargetPivot;
                        spritesheet[i] = sprite;
                        changed = true;
                    }
                }

                if (changed)
                {
                    importer.spritesheet = spritesheet;
                }
            }
            else
            {
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);

                if (Vector2.Distance(settings.spritePivot, TargetPivot) > Tolerance ||
                    settings.spriteAlignment != (int)SpriteAlignment.Custom)
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    settings.spritePivot = TargetPivot;

                    importer.SetTextureSettings(settings);
                    changed = true;
                }
            }

            if (changed)
            {
                importer.SaveAndReimport();
                Debug.Log($"Fixed sprite pivot: {path}");
            }
            else
            {
                Debug.Log($"Sprite pivot already correct: {path}");
            }
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Sprites/Validate Selected Sprite Pivots")]
    public static void ValidateSelectedSpritePivots()
    {
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(path))
                continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                continue;

            if (importer.textureType != TextureImporterType.Sprite)
                continue;

            if (importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                foreach (SpriteMetaData sprite in importer.spritesheet)
                {
                    bool valid =
                        sprite.alignment == (int)SpriteAlignment.Custom &&
                        Vector2.Distance(sprite.pivot, TargetPivot) <= Tolerance;

                    if (!valid)
                    {
                        Debug.LogWarning(
                            $"Invalid pivot in {path}, sprite '{sprite.name}'. " +
                            $"Current: {sprite.pivot}, Expected: {TargetPivot}"
                        );
                    }
                }
            }
            else
            {
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);

                bool valid =
                    settings.spriteAlignment == (int)SpriteAlignment.Custom &&
                    Vector2.Distance(settings.spritePivot, TargetPivot) <= Tolerance;

                if (!valid)
                {
                    Debug.LogWarning(
                        $"Invalid pivot in {path}. " +
                        $"Current: {settings.spritePivot}, Expected: {TargetPivot}"
                    );
                }
            }
        }
    }
}