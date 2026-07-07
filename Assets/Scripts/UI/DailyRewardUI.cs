using UnityEngine;
using InkJam.Gameplay;

namespace InkJam.UI
{
    public class DailyRewardUI : MonoBehaviour
    {
        public bool IsShowing { get; private set; } = false;
        private int _currentStreak = 0;

        public void Show()
        {
            _currentStreak = DailyRewardManager.GetCurrentStreak();
            IsShowing = true;
        }

        private void OnGUI()
        {
            if (!IsShowing) return;

            // Full screen overlay
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

            float width = 600f;
            float height = 500f;
            Rect windowRect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

            GUILayout.BeginArea(windowRect, "Daily Login Rewards", GUI.skin.window);
            GUILayout.Space(30);

            GUILayout.Label("Come back every day for better rewards!", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 });
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            
            string[] rewards = new string[]
            {
                "Day 1\n50 Drops",
                "Day 2\n100 Drops",
                "Day 3\n1x Undo",
                "Day 4\n150 Drops",
                "Day 5\n1x Cleanse",
                "Day 6\n200 Drops",
                "Day 7\n1x +5 Moves\n100 Drops"
            };

            for (int i = 0; i < 7; i++)
            {
                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.alignment = TextAnchor.MiddleCenter;
                
                if (i < _currentStreak)
                {
                    // Already claimed this cycle
                    GUI.color = Color.gray;
                    GUILayout.Box("CLAIMED\n" + rewards[i], boxStyle, GUILayout.Height(100), GUILayout.Width(80));
                }
                else if (i == _currentStreak)
                {
                    // Today's reward
                    GUI.color = Color.green;
                    GUILayout.Box("TODAY\n" + rewards[i], boxStyle, GUILayout.Height(100), GUILayout.Width(80));
                }
                else
                {
                    // Future reward
                    GUI.color = Color.white;
                    GUILayout.Box(rewards[i], boxStyle, GUILayout.Height(100), GUILayout.Width(80));
                }
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(40);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Claim Reward!", GUILayout.Width(200), GUILayout.Height(50)))
            {
                DailyRewardManager.ClaimReward();
                IsShowing = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}
