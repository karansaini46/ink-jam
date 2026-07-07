using UnityEngine;

namespace InkJam.Generator
{
    [CreateAssetMenu(fileName = "NewGeneratorParams", menuName = "Ink Jam/Generator Params")]
    public class GeneratorParams : ScriptableObject
    {
        [Header("Grid Setup")]
        public int Width = 8;
        public int Height = 8;
        public int ColorCount = 4;
        
        [Header("Difficulty Tuning")]
        public int TargetMoves = 10;
        public int Seed = -1; // -1 means random seed
        
        [Range(0f, 1f)]
        public float ObstacleDensity = 0.2f;

        [Header("Unlocked Features")]
        public bool AllowLayers = true;
        public bool AllowFixed = false;
        public bool AllowInkBleed = false;
        public bool AllowLinks = false;

        public GeneratorParams Clone()
        {
            return Instantiate(this);
        }

        public GeneratorParams GetEasierVariant(int failCount)
        {
            GeneratorParams variant = Clone();
            variant.name = this.name + "_Easier";

            // Rubber-banding starts acting on 3rd fail (failCount == 2 before starting, so pass in total fails)
            // If failCount is 3, penalty is 1. If 4, penalty is 2.
            int penaltyLevel = Mathf.Max(1, failCount - 2);

            // Reduce target moves by 15% per penalty level, min 5
            float moveReduction = 1f - (0.15f * penaltyLevel);
            variant.TargetMoves = Mathf.Max(5, Mathf.RoundToInt(variant.TargetMoves * moveReduction));

            // Reduce obstacle density slightly
            variant.ObstacleDensity = Mathf.Max(0f, variant.ObstacleDensity - (0.05f * penaltyLevel));

            return variant;
        }
    }
}
