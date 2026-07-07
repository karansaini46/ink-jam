using System;
using UnityEngine;
using InkJam.Data;

namespace InkJam.Gameplay
{
    public static class DailyRewardManager
    {
        private const string LAST_CLAIM_DATE_KEY = "InkJam_LastClaimDate";
        private const string REWARD_STREAK_KEY = "InkJam_RewardStreak";

        public static bool IsRewardAvailable()
        {
            string lastClaimDateStr = PlayerPrefs.GetString(LAST_CLAIM_DATE_KEY, "");
            string todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            return lastClaimDateStr != todayStr;
        }

        public static int GetCurrentStreak()
        {
            // Returns 0 to 6
            if (!IsRewardAvailable())
            {
                return PlayerPrefs.GetInt(REWARD_STREAK_KEY, 0);
            }

            string lastClaimDateStr = PlayerPrefs.GetString(LAST_CLAIM_DATE_KEY, "");
            if (string.IsNullOrEmpty(lastClaimDateStr))
            {
                return 0;
            }

            if (DateTime.TryParseExact(lastClaimDateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime lastClaimDate))
            {
                TimeSpan diff = DateTime.UtcNow.Date - lastClaimDate.Date;
                if (diff.Days == 1)
                {
                    // Consecutive day!
                    int streak = PlayerPrefs.GetInt(REWARD_STREAK_KEY, 0);
                    return (streak + 1) % 7;
                }
                else if (diff.Days > 1)
                {
                    // Missed a day, reset streak
                    return 0;
                }
                else
                {
                    // Claimed today
                    return PlayerPrefs.GetInt(REWARD_STREAK_KEY, 0);
                }
            }
            
            return 0;
        }

        public static void ClaimReward()
        {
            if (!IsRewardAvailable()) return;

            int currentStreak = GetCurrentStreak();
            GrantRewardForDay(currentStreak);

            PlayerPrefs.SetString(LAST_CLAIM_DATE_KEY, DateTime.UtcNow.ToString("yyyyMMdd"));
            PlayerPrefs.SetInt(REWARD_STREAK_KEY, currentStreak);
            PlayerPrefs.Save();
        }

        private static void GrantRewardForDay(int dayIndex)
        {
            switch (dayIndex)
            {
                case 0:
                    EconomyManager.AddDrops(50);
                    break;
                case 1:
                    EconomyManager.AddDrops(100);
                    break;
                case 2:
                    EconomyManager.AddBooster(EconomyManager.BoosterType.Undo, 1);
                    break;
                case 3:
                    EconomyManager.AddDrops(150);
                    break;
                case 4:
                    EconomyManager.AddBooster(EconomyManager.BoosterType.Cleanse, 1);
                    break;
                case 5:
                    EconomyManager.AddDrops(200);
                    break;
                case 6:
                    EconomyManager.AddBooster(EconomyManager.BoosterType.ExtraMoves, 1);
                    EconomyManager.AddDrops(100);
                    break;
            }
        }
    }
}
