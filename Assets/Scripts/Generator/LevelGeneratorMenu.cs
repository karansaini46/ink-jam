#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using InkJam.Data;

namespace InkJam.Generator
{
    public class LevelGeneratorMenu
    {
        [MenuItem("Ink Jam/Generate Test Level")]
        public static void GenerateTestLevel()
        {
            // Try to find a DifficultyCurve asset, or create a temporary one
            DifficultyCurve curve = null;
            string[] guids = AssetDatabase.FindAssets("t:DifficultyCurve");
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                curve = AssetDatabase.LoadAssetAtPath<DifficultyCurve>(assetPath);
            }
            if (curve == null)
            {
                curve = ScriptableObject.CreateInstance<DifficultyCurve>();
            }

            // Use the curve to get params for an example level (e.g., Level 5)
            GeneratorParams genParams = curve.GetParamsForLevel(5);

            var (levelData, solution) = LevelGenerator.Generate(genParams);

            string json = JsonUtility.ToJson(levelData, true);
            string path = Path.Combine(Application.dataPath, "GeneratedLevel.json");
            File.WriteAllText(path, json);

            Debug.Log($"[LevelGenerator] Successfully generated backward! TargetMoves: {genParams.TargetMoves}, ActualMoves: {solution.Count}");
            Debug.Log($"[LevelGenerator] Saved LevelData JSON to: {path}");
            
            // Print the solution for the user to verify
            Debug.Log("[LevelGenerator] Forward Solution Path:");
            for (int i = 0; i < solution.Count; i++)
            {
                var step = solution[i];
                Debug.Log($"Step {i+1}: Tile {step.TileId} -> {step.Direction} (Path length: {step.Path.Count})");
            }
            
            AssetDatabase.Refresh();
        }
    }
}
#endif
