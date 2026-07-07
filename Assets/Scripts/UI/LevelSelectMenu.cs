using UnityEngine;
using InkJam.Gameplay;

namespace InkJam.UI
{
    [RequireComponent(typeof(LevelController))]
    public class LevelSelectMenu : MonoBehaviour
    {
        private LevelController _levelController;
        private TextAsset[] _levelFiles;
        private bool _showMenu = true;
        
        private Vector2 _scrollPos;

        private InkJam.Generator.DifficultyCurve _difficultyCurve;

        private void Awake()
        {
            _levelController = GetComponent<LevelController>();
            
            // Try to load DifficultyCurve from Resources or AssetDatabase
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DifficultyCurve");
            if (guids.Length > 0)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                _difficultyCurve = UnityEditor.AssetDatabase.LoadAssetAtPath<InkJam.Generator.DifficultyCurve>(assetPath);
            }
#endif
            if (_difficultyCurve == null)
            {
                _difficultyCurve = Resources.Load<InkJam.Generator.DifficultyCurve>("DifficultyCurve");
            }

            if (_difficultyCurve == null)
            {
                _difficultyCurve = ScriptableObject.CreateInstance<InkJam.Generator.DifficultyCurve>();
            }
        }

        private void OnDestroy()
        {
            // Clean up
        }

        private void Start()
        {
            if (InkJam.Gameplay.DailyRewardManager.IsRewardAvailable())
            {
                var dailyUI = gameObject.GetComponent<DailyRewardUI>();
                if (dailyUI == null)
                {
                    dailyUI = gameObject.AddComponent<DailyRewardUI>();
                }
                dailyUI.Show();
            }
        }

        public void ShowMenu()
        {
            _showMenu = true;
            if (_levelController != null && _levelController.CurrentBoard != null)
            {
                _levelController.CurrentBoard.Tiles.Clear(); // Just to clear visual state if needed
            }
        }

        private void OnGUI()
        {
            var dailyUI = GetComponent<DailyRewardUI>();
            if (dailyUI != null && dailyUI.IsShowing) return; // Wait for claim

            if (!_showMenu) return;

            // Draw a semi-transparent background
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Ink Jam Playtest");

            // Define menu area
            float menuWidth = 400f;
            float menuHeight = Screen.height - 100f;
            Rect menuRect = new Rect((Screen.width - menuWidth) / 2f, 50f, menuWidth, menuHeight);

            GUILayout.BeginArea(menuRect);
            
            GUILayout.Label("Select a Level (Procedural)", new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(20);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            
            // Provide 50 levels for playtesting
            for (int i = 1; i <= 50; i++)
            {
                if (GUILayout.Button($"Level {i}", GUILayout.Height(50)))
                {
                    LoadProceduralLevel(i);
                }
                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void LoadProceduralLevel(int levelNumber)
        {
            _showMenu = false;
            var genParams = _difficultyCurve.GetParamsForLevel(levelNumber);
            
            // We lock the seed based on the level number so "Level 5" is always the same puzzle 
            // for the same difficulty curve constants. This allows rubber-banding to work predictably.
            genParams.Seed = 1000 + levelNumber; 

            _levelController.LoadGeneratedLevel(genParams);
        }
    }
}
