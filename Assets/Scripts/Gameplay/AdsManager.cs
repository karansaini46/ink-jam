using System;
using UnityEngine;

namespace InkJam.Gameplay
{
    public static class AdsManager
    {
        public static void ShowRewardedAd(Action onSuccess)
        {
            Debug.Log("[AdsManager] Rewarded Ad Started...");
            
            // Simulate ad delay (in a real system, this happens via SDK callbacks)
            Debug.Log("[AdsManager] Rewarded Ad Finished successfully.");
            
            onSuccess?.Invoke();
        }
    }
}
