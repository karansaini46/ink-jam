#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using InkJam.Art;
using InkJam.Core;
using UnityEditor.U2D;
using UnityEngine.U2D;

namespace InkJam.Editor
{
    public class ImportFinalArt : EditorWindow
    {
        [MenuItem("Ink Jam/Import Final Sumi-e Art")]
        public static void ImportSumiEArt()
        {
            string themeFolder = "Assets/Art/SumiETheme";
            if (!Directory.Exists(themeFolder))
            {
                Directory.CreateDirectory(themeFolder);
            }

            AssetDatabase.Refresh();

            // Load sprites
            Sprite sRed = LoadAndConfigureSprite($"{themeFolder}/tile_red.png", 100);
            Sprite sBlue = LoadAndConfigureSprite($"{themeFolder}/tile_blue.png", 100);
            Sprite sGreen = LoadAndConfigureSprite($"{themeFolder}/tile_green.png", 100);
            Sprite sYellow = LoadAndConfigureSprite($"{themeFolder}/tile_yellow.png", 100);
            Sprite sOrange = LoadAndConfigureSprite($"{themeFolder}/tile_orange.png", 100);
            Sprite sPurple = LoadAndConfigureSprite($"{themeFolder}/tile_purple.png", 100);

            Sprite sBg = LoadAndConfigureSprite($"{themeFolder}/board_bg.png", 100);
            Sprite sCell = LoadAndConfigureSprite($"{themeFolder}/grid_cell.png", 100);
            Sprite sFixed = LoadAndConfigureSprite($"{themeFolder}/obs_fixed.png", 100);
            Sprite sBleed = LoadAndConfigureSprite($"{themeFolder}/obs_bleed.png", 100);
            
            // Exit frame requires 9-slice potentially, but we'll use a normal sprite config
            Sprite sFrame = LoadAndConfigureSprite($"{themeFolder}/exit_frame.png", 100);

            // Create ThemeData
            string themePath = "Assets/Resources/SumiETheme.asset";
            ThemeData theme = AssetDatabase.LoadAssetAtPath<ThemeData>(themePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ThemeData>();
                AssetDatabase.CreateAsset(theme, themePath);
            }

            theme.boardBackgroundSprite = sBg;
            theme.gridCellSprite = sCell;
            theme.exitFrameSprite = sFrame;
            theme.fixedObstacleSprite = sFixed;
            theme.inkBleedSprite = sBleed;

            theme.tileSprites = new List<TileSpriteMapping>
            {
                new TileSpriteMapping { color = TileColor.Red, sprite = sRed },
                new TileSpriteMapping { color = TileColor.Blue, sprite = sBlue },
                new TileSpriteMapping { color = TileColor.Green, sprite = sGreen },
                new TileSpriteMapping { color = TileColor.Yellow, sprite = sYellow },
                new TileSpriteMapping { color = TileColor.Orange, sprite = sOrange },
                new TileSpriteMapping { color = TileColor.Purple, sprite = sPurple }
            };

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();

            // Create Atlas
            string atlasPath = "Assets/Art/SumiETheme/SumiEAtlas.spriteatlas";
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                SpriteAtlasAsset.Save(atlas, atlasPath);
                Object dirObj = AssetDatabase.LoadAssetAtPath<Object>(themeFolder);
                if (dirObj != null)
                {
                    SpriteAtlasExtensions.Add(atlas, new Object[] { dirObj });
                }
            }

            // Set as active theme
            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.SetTheme(theme);
                // Also set it to the prefab/scene if it's there
                ThemeManager.Instance.currentTheme = theme;
            }

            Debug.Log("Final Sumi-e Art Imported and Theme created!");
        }

        private static Sprite LoadAndConfigureSprite(string path, float ppu)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Missing file: {path}");
                return null;
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = ppu;
                importer.alphaIsTransparency = true; // Important for generative art if we added alpha channel later
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
#endif
