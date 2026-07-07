using UnityEngine;

namespace InkJam.Generator
{
    [CreateAssetMenu(fileName = "DifficultyCurve", menuName = "Ink Jam/Difficulty Curve")]
    public class DifficultyCurve : ScriptableObject
    {
        [Header("Grid Scaling")]
        public int BaseWidth = 4;
        public int MaxWidth = 12;
        public int WidthStepLevels = 5; // Grid grows by 1 every 5 levels
        
        public int BaseHeight = 4;
        public int MaxHeight = 12;
        public int HeightStepLevels = 5; // Grid grows by 1 every 5 levels
        
        [Header("Color Scaling")]
        public int BaseColorCount = 2;
        public int MaxColorCount = 6;
        public int ColorStepLevels = 8; // Introduce new color every 8 levels
        
        [Header("Move Scaling")]
        public int BaseMoves = 5;
        public int MaxMoves = 50;
        public float MovesPerLevel = 1.5f; // Linear scaling for target moves
        
        [Header("Obstacle Density")]
        public float BaseObstacleDensity = 0.05f;
        public float MaxObstacleDensity = 0.4f;
        public float DensityPerLevel = 0.02f;

        [Header("Obstacle Unlock Thresholds")]
        public int UnlockFixedLevel = 1;
        public int UnlockLayersLevel = 45;
        public int UnlockBleedLevel = 95;
        public int UnlockLinksLevel = 145;

        public GeneratorParams GetParamsForLevel(int level)
        {
            // Ensure level is at least 1
            level = Mathf.Max(1, level);

            GeneratorParams p = ScriptableObject.CreateInstance<GeneratorParams>();
            p.name = $"Level_{level}_Params";

            // Grid Size (Stepped)
            p.Width = Mathf.Clamp(BaseWidth + (level / WidthStepLevels), BaseWidth, MaxWidth);
            p.Height = Mathf.Clamp(BaseHeight + (level / HeightStepLevels), BaseHeight, MaxHeight);

            // Color Count (Stepped)
            p.ColorCount = Mathf.Clamp(BaseColorCount + (level / ColorStepLevels), BaseColorCount, MaxColorCount);

            // Target Moves (Linear)
            p.TargetMoves = Mathf.Clamp(BaseMoves + Mathf.RoundToInt(level * MovesPerLevel), BaseMoves, MaxMoves);

            // Obstacle Density (Linear)
            p.ObstacleDensity = Mathf.Clamp(BaseObstacleDensity + (level * DensityPerLevel), 0f, MaxObstacleDensity);

            // Unlocks
            p.AllowFixed = level >= UnlockFixedLevel;
            p.AllowLayers = level >= UnlockLayersLevel;
            p.AllowInkBleed = level >= UnlockBleedLevel;
            p.AllowLinks = level >= UnlockLinksLevel;

            // Random seed for generation
            p.Seed = -1; 

            return p;
        }
    }
}
