using UnityEngine;
using UnityEngine.UI;
using InkJam.Gameplay;

namespace InkJam.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("System References")]
        [Tooltip("Leave empty to auto-find in scene")]
        public LevelController levelController;
        [Tooltip("Leave empty to auto-find in scene")]
        public LevelSelectMenu levelSelectMenu;

        [Header("HUD Elements")]
        public Text levelNameText;
        public Text moveCounterText;
        public Button restartButton;
        public GameObject inGameUIPanel;

        [Header("Level Complete Overlay")]
        public GameObject levelCompleteOverlay;
        public Text finalMoveCountText;
        public Button backToLevelSelectButton;

        [Header("Economy & Boosters UI")]
        public Text inkDropsText;
        public Button undoButton;
        public Button cleanseButton;
        public Button extraMovesButton;

        [Header("Out of Moves Overlay")]
        public GameObject outOfMovesOverlay;
        public Button buyExtraMovesButton;
        public Button outOfMovesRestartButton;

        private void Start()
        {
            if (levelController == null)
                levelController = FindObjectOfType<LevelController>();

            if (levelSelectMenu == null)
                levelSelectMenu = FindObjectOfType<LevelSelectMenu>();

            if (levelController != null)
            {
                levelController.OnLevelLoaded += HandleLevelLoaded;
                levelController.OnMoveCountChanged += HandleMoveCountChanged;
                levelController.OnLevelWon += HandleLevelWon;
                levelController.OnLevelFailed += HandleLevelFailed;
            }

            InkJam.Data.EconomyManager.OnInkDropsChanged += HandleInkDropsChanged;
            HandleInkDropsChanged(InkJam.Data.EconomyManager.InkDrops);

            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (backToLevelSelectButton != null) backToLevelSelectButton.onClick.AddListener(OnBackToLevelSelectClicked);
            
            if (undoButton != null) undoButton.onClick.AddListener(OnUndoClicked);
            if (cleanseButton != null) cleanseButton.onClick.AddListener(OnCleanseClicked);
            if (extraMovesButton != null) extraMovesButton.onClick.AddListener(OnExtraMovesClicked);

            if (buyExtraMovesButton != null) buyExtraMovesButton.onClick.AddListener(OnExtraMovesClicked);
            if (outOfMovesRestartButton != null) outOfMovesRestartButton.onClick.AddListener(OnRestartClicked);

            // Initially hide game UI if not in a level
            if (levelController != null && levelController.CurrentState != InkJam.Core.LevelState.InProgress)
            {
                if (inGameUIPanel != null) inGameUIPanel.SetActive(false);
                if (levelCompleteOverlay != null) levelCompleteOverlay.SetActive(false);
                if (outOfMovesOverlay != null) outOfMovesOverlay.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (levelController != null)
            {
                levelController.OnLevelLoaded -= HandleLevelLoaded;
                levelController.OnMoveCountChanged -= HandleMoveCountChanged;
                levelController.OnLevelWon -= HandleLevelWon;
                levelController.OnLevelFailed -= HandleLevelFailed;
            }
            InkJam.Data.EconomyManager.OnInkDropsChanged -= HandleInkDropsChanged;
        }

        private void HandleInkDropsChanged(int newAmount)
        {
            if (inkDropsText != null)
            {
                inkDropsText.text = newAmount.ToString();
            }
        }

        private void HandleLevelLoaded()
        {
            if (inGameUIPanel != null) inGameUIPanel.SetActive(true);
            if (levelCompleteOverlay != null) levelCompleteOverlay.SetActive(false);
            if (outOfMovesOverlay != null) outOfMovesOverlay.SetActive(false);

            if (levelNameText != null)
                levelNameText.text = $"Level: {levelController.CurrentLevelName}";
                
            HandleMoveCountChanged(levelController.MoveCount, levelController.MaxMoves);
        }

        private void HandleMoveCountChanged(int moveCount, int maxMoves)
        {
            if (moveCounterText != null)
            {
                int movesLeft = Mathf.Max(0, maxMoves - moveCount);
                moveCounterText.text = $"Moves Left: {movesLeft}";
            }
        }

        private void HandleLevelWon()
        {
            if (inGameUIPanel != null) inGameUIPanel.SetActive(false);
            if (levelCompleteOverlay != null) levelCompleteOverlay.SetActive(true);
            if (outOfMovesOverlay != null) outOfMovesOverlay.SetActive(false);

            if (finalMoveCountText != null)
                finalMoveCountText.text = $"Completed with {levelController.MaxMoves - levelController.MoveCount} moves remaining!";
        }

        private void HandleLevelFailed()
        {
            if (outOfMovesOverlay != null) outOfMovesOverlay.SetActive(true);
        }

        private void OnUndoClicked()
        {
            BoosterManager.TryUseUndo(levelController);
        }

        private void OnCleanseClicked()
        {
            BoosterManager.TryUseCleanse(levelController);
        }

        private void OnExtraMovesClicked()
        {
            if (BoosterManager.TryUseExtraMoves(levelController, 5))
            {
                if (outOfMovesOverlay != null) outOfMovesOverlay.SetActive(false);
            }
        }

        private void OnRestartClicked()
        {
            if (outOfMovesOverlay != null) outOfMovesOverlay.SetActive(false);
            if (levelController != null)
            {
                levelController.RestartLevel();
            }
        }

        private void OnBackToLevelSelectClicked()
        {
            if (levelCompleteOverlay != null) levelCompleteOverlay.SetActive(false);
            if (inGameUIPanel != null) inGameUIPanel.SetActive(false);
            if (outOfMovesOverlay != null) outOfMovesOverlay.SetActive(false);

            if (levelSelectMenu != null)
            {
                levelSelectMenu.ShowMenu();
            }
        }
    }
}
