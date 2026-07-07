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
    public class ArtPipelineSetup : EditorWindow
    {
        [MenuItem("Ink Jam/Setup Art Pipeline")]
        public static void SetupArtPipeline()
        {
            string artDir = "Assets/Art/Placeholders";
            if (!Directory.Exists(artDir))
            {
                Directory.CreateDirectory(artDir);
            }

            // 1. Generate PNGs
            string[] tileColors = { "Red", "Blue", "Green", "Yellow", "Orange", "Purple" };
            Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 0.5f) };
            
            List<Sprite> generatedSprites = new List<Sprite>();

            for (int i = 0; i < tileColors.Length; i++)
            {
                generatedSprites.Add(GenerateSolidTexture(artDir, $"Tile_{tileColors[i]}", 100, 100, colors[i]));
            }

            Sprite bgSprite = GenerateSolidTexture(artDir, "Board_Bg", 100, 100, new Color(0.9f, 0.9f, 0.9f));
            Sprite cellSprite = GenerateSolidTexture(artDir, "Grid_Cell", 100, 100, new Color(0.8f, 0.8f, 0.8f));
            Sprite fixedObsSprite = GenerateSolidTexture(artDir, "Obs_Fixed", 100, 100, Color.black);
            Sprite bleedObsSprite = GenerateSolidTexture(artDir, "Obs_Bleed", 100, 100, new Color(0.6f, 0f, 0.8f));
            Sprite exitFrameSprite = GenerateSolidTexture(artDir, "Exit_Frame", 100, 100, new Color(0.4f, 0.4f, 0.4f)); // Outline placeholder
            
            generatedSprites.AddRange(new Sprite[] { bgSprite, cellSprite, fixedObsSprite, bleedObsSprite, exitFrameSprite });

            // Generate 9-Slice Manga Panel
            Sprite mangaPanelSprite = GenerateMangaPanel(artDir);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 2. Create Sprite Atlas
            string atlasPath = "Assets/Art/ThemeAtlas.spriteatlas";
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                SpriteAtlasAsset.Save(atlas, atlasPath);
                
                // Add directory to atlas
                Object dirObj = AssetDatabase.LoadAssetAtPath<Object>(artDir);
                if (dirObj != null)
                {
                    SpriteAtlasExtensions.Add(atlas, new Object[] { dirObj });
                }
            }

            // 3. Create Default ThemeData
            string themePath = "Assets/Resources/DefaultTheme.asset";
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }

            ThemeData theme = AssetDatabase.LoadAssetAtPath<ThemeData>(themePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ThemeData>();
                AssetDatabase.CreateAsset(theme, themePath);
            }

            theme.boardBackgroundSprite = bgSprite;
            theme.gridCellSprite = cellSprite;
            theme.exitFrameSprite = exitFrameSprite;
            theme.fixedObstacleSprite = fixedObsSprite;
            theme.inkBleedSprite = bleedObsSprite;

            theme.tileSprites = new List<TileSpriteMapping>
            {
                new TileSpriteMapping { color = TileColor.Red, sprite = generatedSprites[0] },
                new TileSpriteMapping { color = TileColor.Blue, sprite = generatedSprites[1] },
                new TileSpriteMapping { color = TileColor.Green, sprite = generatedSprites[2] },
                new TileSpriteMapping { color = TileColor.Yellow, sprite = generatedSprites[3] },
                new TileSpriteMapping { color = TileColor.Orange, sprite = generatedSprites[4] },
                new TileSpriteMapping { color = TileColor.Purple, sprite = generatedSprites[5] }
            };

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();

            Debug.Log("Art Pipeline Setup Complete! Generated placeholders, Atlas, and DefaultTheme.");
        }

        private static Sprite GenerateSolidTexture(string folder, string name, int width, int height, Color color)
        {
            string path = $"{folder}/{name}.png";
            
            Texture2D tex = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100f; // 1 unit = 100 pixels
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite GenerateMangaPanel(string folder)
        {
            string path = $"{folder}/MangaPanel9Slice.png";
            int width = 100;
            int height = 100;
            int border = 10;

            Texture2D tex = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Thick black border, white inside
                    bool isBorder = x < border || x >= width - border || y < border || y >= height - border;
                    pixels[y * width + x] = isBorder ? Color.black : Color.white;
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteBorder = new Vector4(border, border, border, border); // Setup 9-slicing
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
#endif
