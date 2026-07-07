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

        private void Awake()
        {
            _levelController = GetComponent<LevelController>();
            _levelController.OnLevelWon += HandleLevelWon;
            
            // Load all text assets from Resources/Levels/
            _levelFiles = Resources.LoadAll<TextAsset>("Levels");
        }

        private void OnDestroy()
        {
            if (_levelController != null)
            {
                _levelController.OnLevelWon -= HandleLevelWon;
            }
        }

        private void HandleLevelWon()
        {
            _showMenu = true;
        }

        private void OnGUI()
        {
            if (!_showMenu) return;

            // Draw a semi-transparent background
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Ink Jam Playtest");

            // Define menu area
            float menuWidth = 400f;
            float menuHeight = Screen.height - 100f;
            Rect menuRect = new Rect((Screen.width - menuWidth) / 2f, 50f, menuWidth, menuHeight);

            GUILayout.BeginArea(menuRect);
            
            GUILayout.Label("Select a Level", new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(20);

            if (_levelFiles == null || _levelFiles.Length == 0)
            {
                GUILayout.Label("No levels found in Resources/Levels/");
            }
            else
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                
                foreach (var level in _levelFiles)
                {
                    // Draw a big button for each level
                    if (GUILayout.Button(level.name, GUILayout.Height(50)))
                    {
                        LoadLevel(level.name);
                    }
                    GUILayout.Space(10);
                }

                GUILayout.EndScrollView();
            }

            GUILayout.EndArea();
        }

        private void LoadLevel(string levelName)
        {
            _showMenu = false;
            _levelController.LoadLevel(levelName);
        }
    }
}
