using UnityEngine;
using System;

namespace InkJam.Data
{
    public static class EconomyManager
    {
        private const string INK_DROPS_KEY = "InkJam_InkDrops";

        public static event Action<int> OnInkDropsChanged;

        public static int InkDrops
        {
            get => PlayerPrefs.GetInt(INK_DROPS_KEY, 500); // 500 starting drops for testing
            private set
            {
                PlayerPrefs.SetInt(INK_DROPS_KEY, value);
                PlayerPrefs.Save();
                OnInkDropsChanged?.Invoke(value);
            }
        }

        public static void AddDrops(int amount)
        {
            if (amount < 0) return;
            InkDrops += amount;
            Debug.Log($"[Economy] Added {amount} Ink Drops. Total: {InkDrops}");
        }

        public static bool TrySpendDrops(int amount)
        {
            if (amount < 0) return false;
            
            if (InkDrops >= amount)
            {
                InkDrops -= amount;
                Debug.Log($"[Economy] Spent {amount} Ink Drops. Total: {InkDrops}");
                return true;
            }
            
            Debug.Log($"[Economy] Failed to spend {amount} Ink Drops. Not enough funds.");
            return false;
        }

        public enum BoosterType
        {
            Undo,
            Cleanse,
            ExtraMoves
        }

        private static string GetBoosterKey(BoosterType type)
        {
            return $"InkJam_BoosterCount_{type}";
        }

        public static int GetBoosterCount(BoosterType type)
        {
            return PlayerPrefs.GetInt(GetBoosterKey(type), 0);
        }

        public static void AddBooster(BoosterType type, int amount = 1)
        {
            if (amount < 0) return;
            int current = GetBoosterCount(type);
            PlayerPrefs.SetInt(GetBoosterKey(type), current + amount);
            PlayerPrefs.Save();
            Debug.Log($"[Economy] Added {amount} {type} Booster(s). Total: {current + amount}");
        }

        public static bool TryConsumeBooster(BoosterType type)
        {
            int current = GetBoosterCount(type);
            if (current > 0)
            {
                PlayerPrefs.SetInt(GetBoosterKey(type), current - 1);
                PlayerPrefs.Save();
                Debug.Log($"[Economy] Consumed 1 {type} Booster. Total remaining: {current - 1}");
                return true;
            }
            return false;
        }
    }
}
