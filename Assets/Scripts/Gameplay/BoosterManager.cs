using UnityEngine;
using InkJam.Data;

namespace InkJam.Gameplay
{
    public static class BoosterManager
    {
        public const int UNDO_COST = 20;
        public const int CLEANSE_COST = 30;
        public const int EXTRA_MOVES_COST = 50;

        public static bool TryUseUndo(LevelController controller)
        {
            if (EconomyManager.TryConsumeBooster(EconomyManager.BoosterType.Undo) || EconomyManager.TrySpendDrops(UNDO_COST))
            {
                controller.ApplyUndo();
                return true;
            }
            return false;
        }

        public static bool TryUseCleanse(LevelController controller)
        {
            if (EconomyManager.TryConsumeBooster(EconomyManager.BoosterType.Cleanse) || EconomyManager.TrySpendDrops(CLEANSE_COST))
            {
                controller.ApplyCleanse();
                return true;
            }
            return false;
        }

        public static bool TryUseExtraMoves(LevelController controller, int amount = 5)
        {
            if (EconomyManager.TryConsumeBooster(EconomyManager.BoosterType.ExtraMoves) || EconomyManager.TrySpendDrops(EXTRA_MOVES_COST))
            {
                controller.AddExtraMoves(amount);
                return true;
            }
            return false;
        }
    }
}
