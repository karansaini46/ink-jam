using System;
using UnityEngine;
using InkJam.Core;
using InkJam.Data;

namespace InkJam.Gameplay
{
    public class LevelController : MonoBehaviour
    {
        public string levelToLoad;
        public Board CurrentBoard { get; private set; }
        public LevelState CurrentState { get; private set; }

        public int MoveCount { get; private set; }
        public string CurrentLevelName { get; private set; }

        public int MaxMoves { get; private set; }
        
        public event Action OnLevelWon;
        public event Action OnLevelFailed;
        public event Action OnLevelLoaded;
        public event Action<int, int> OnMoveCountChanged; // Current, Max
        public event Action<Tile, SlideResult> OnTileSlid;
        public event Action<Tile> OnTileExited;

        public InkJam.Generator.GeneratorParams CurrentGenParams { get; private set; }
        private InkJam.Generator.GeneratorParams _baseGenParams;
        private int _consecutiveFails = 0;

        private System.Collections.Generic.Stack<Board> _boardHistory = new System.Collections.Generic.Stack<Board>();

        private System.Random _random = new System.Random();
        private LinkedTileValidator _linkedValidator = new LinkedTileValidator();

        private void Awake()
        {
            if (GetComponent<TileDragController>() == null)
            {
                gameObject.AddComponent<TileDragController>();
            }
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(levelToLoad))
            {
                LoadLevel(levelToLoad);
            }
        }

        public void RestartLevel()
        {
            if (CurrentGenParams != null)
            {
                _consecutiveFails++;
                Debug.Log($"[RubberBanding] Restarting procedurally generated level. Consecutive fails: {_consecutiveFails}");
                
                if (_consecutiveFails >= 3)
                {
                    // Generate easier variant!
                    var easierParams = _baseGenParams.GetEasierVariant(_consecutiveFails);
                    Debug.Log($"[RubberBanding] Triggering easier variant! Target moves reduced to: {easierParams.TargetMoves}");
                    LoadGeneratedLevel(easierParams, true);
                }
                else
                {
                    LoadGeneratedLevel(CurrentGenParams, true);
                }
            }
            else if (!string.IsNullOrEmpty(CurrentLevelName))
            {
                LoadLevel(CurrentLevelName);
            }
            else if (!string.IsNullOrEmpty(levelToLoad))
            {
                LoadLevel(levelToLoad);
            }
        }

        public void LoadGeneratedLevel(InkJam.Generator.GeneratorParams genParams, bool isRestart = false)
        {
            if (!isRestart)
            {
                _consecutiveFails = 0;
                _baseGenParams = genParams.Clone(); // Store original
            }

            CurrentGenParams = genParams;
            CurrentLevelName = $"Procedural_{genParams.name}";
            
            // Wait, we need to generate it!
            var (levelData, solution) = InkJam.Generator.LevelGenerator.Generate(genParams);
            
            CurrentBoard = BuildBoardFromData(levelData);
            CurrentState = LevelState.InProgress;
            MoveCount = 0;
            MaxMoves = genParams.TargetMoves + 5; // Give a small buffer
            _boardHistory.Clear();
            _linkedValidator.OnLevelStart();

            Debug.Log($"Generated Level '{CurrentLevelName}' loaded. Target Moves: {genParams.TargetMoves}");
            OnLevelLoaded?.Invoke();
            OnMoveCountChanged?.Invoke(MoveCount, MaxMoves);
        }

        public void LoadLevel(string levelName)
        {
            TextAsset levelJson = Resources.Load<TextAsset>($"Levels/{levelName}");
            if (levelJson == null)
            {
                Debug.LogWarning($"Level '{levelName}' not found in Resources/Levels/ - stubbing empty board.");
                CurrentBoard = new Board(5, 5); // Fallback for testing
                CurrentState = LevelState.InProgress;
                return;
            }

            LevelData data = JsonUtility.FromJson<LevelData>(levelJson.text);
            CurrentBoard = BuildBoardFromData(data);
            CurrentState = LevelState.InProgress;
            CurrentLevelName = levelName;
            CurrentGenParams = null; // Clear procedural state
            _baseGenParams = null;
            _consecutiveFails = 0;
            MoveCount = 0;
            MaxMoves = 20; // Default for static JSON levels without target moves
            _boardHistory.Clear();
            _linkedValidator.OnLevelStart();

            Debug.Log($"Level '{levelName}' loaded.");
            OnLevelLoaded?.Invoke();
            OnMoveCountChanged?.Invoke(MoveCount, MaxMoves);
        }

        private Board BuildBoardFromData(LevelData data)
        {
            var board = new Board(data.width, data.height);

            if (data.exitFrames != null)
            {
                foreach (var frameData in data.exitFrames)
                {
                    var coord = new GridCoord(frameData.x, frameData.y);
                    board.AddExitFrame(new ExitFrame(coord, frameData.edge, frameData.acceptedColor));
                }
            }

            if (data.tiles != null)
            {
                foreach (var tileData in data.tiles)
                {
                    var tile = new Tile(tileData.id, tileData.color, new GridCoord(tileData.startX, tileData.startY), tileData.layerCount, tileData.linkId);
                    board.AddTile(tile);
                }
            }

            // Obstacle logic
            if (data.obstacles != null)
            {
                foreach (var obsData in data.obstacles)
                {
                    if (obsData.type == "FixedObstacle")
                    {
                        board.AddObstacle(new InkJam.Obstacles.FixedObstacle(new GridCoord(obsData.x, obsData.y)));
                    }
                    else if (obsData.type == "InkBleed")
                    {
                        int spreadInterval = 3;
                        int cleanseRequirement = 1;
                        var bleedObs = new InkJam.Obstacles.InkBleedObstacle(
                            spreadInterval, 
                            cleanseRequirement, 
                            new InkJam.Obstacles.Bleed.RandomAdjacentSpreadStrategy(), 
                            new InkJam.Obstacles.Bleed.ExitAdjacentCleanseStrategy()
                        );
                        bleedObs.AddBledCell(new GridCoord(obsData.x, obsData.y));
                        board.AddObstacle(bleedObs);
                    }
                    // Additional obstacle types can be parsed here
                }
            }

            return board;
        }

        // Update() and TryMoveStub() removed. Input is now handled entirely by TileDragController.

        public void Slide(Tile tileToMove, Direction dir)
        {
            if (CurrentBoard == null || CurrentState != LevelState.InProgress) return;
            if (tileToMove == null || tileToMove.IsExited || tileToMove.IsLocked) return;

            var result = MovementSystem.TrySlide(CurrentBoard, tileToMove, dir);
            if (result.IsValidMove)
            {
                // Save history for Undo
                _boardHistory.Push(CurrentBoard.CloneState());

                MoveCount++;
                OnMoveCountChanged?.Invoke(MoveCount, MaxMoves);

                result.Apply(CurrentBoard);
                
                OnTileSlid?.Invoke(tileToMove, result);
                if (result.DidExit)
                {
                    OnTileExited?.Invoke(tileToMove);
                }

                BleedSystem.Tick(CurrentBoard, result, _random);
                _linkedValidator.ValidateAfterMove(CurrentBoard, MoveCount);
                
                if (!CheckWin() && MoveCount >= MaxMoves)
                {
                    CurrentState = LevelState.Failed;
                    Debug.Log("Level Failed - Out of Moves!");
                    InkJam.Audio.AudioManager.Instance?.PlayFail();
                    OnLevelFailed?.Invoke();
                }
            }
        }

        private bool CheckWin()
        {
            if (CurrentBoard.CheckWinCondition())
            {
                CurrentState = LevelState.Won;
                Debug.Log("Level Won!");
                InkJam.Audio.AudioManager.Instance?.PlayWin();
                
                int reward = CurrentGenParams != null ? 10 + CurrentGenParams.TargetMoves : 20;
                InkJam.Data.EconomyManager.AddDrops(reward);
                
                OnLevelWon?.Invoke();
                return true;
            }
            return false;
        }

        public void ApplyUndo()
        {
            if (_boardHistory.Count > 0)
            {
                CurrentBoard = _boardHistory.Pop();
                MoveCount = Mathf.Max(0, MoveCount - 1);
                
                // If we undo from a failed state, we resume playing
                if (CurrentState == LevelState.Failed)
                {
                    CurrentState = LevelState.InProgress;
                }
                
                OnMoveCountChanged?.Invoke(MoveCount, MaxMoves);
                Debug.Log("[Booster] Applied Undo");
            }
        }

        public void ApplyCleanse()
        {
            if (CurrentBoard == null) return;

            foreach (var obs in CurrentBoard.Obstacles)
            {
                if (obs is InkJam.Obstacles.InkBleedObstacle bleedObs && bleedObs.BledCells.Count > 0)
                {
                    // Cleanse the first available bleed cell
                    var enumerator = bleedObs.BledCells.GetEnumerator();
                    enumerator.MoveNext();
                    GridCoord cellToClear = enumerator.Current;
                    
                    bleedObs.RemoveBledCell(cellToClear);
                    Debug.Log($"[Booster] Cleansed Ink Bleed at {cellToClear}");
                    return;
                }
            }
            Debug.Log("[Booster] Cleanse used, but no Ink Bleed found.");
        }

        public void AddExtraMoves(int amount)
        {
            MaxMoves += amount;
            
            if (CurrentState == LevelState.Failed && MoveCount < MaxMoves)
            {
                CurrentState = LevelState.InProgress;
            }
            
            OnMoveCountChanged?.Invoke(MoveCount, MaxMoves);
            Debug.Log($"[Booster] Added {amount} Extra Moves. New Max: {MaxMoves}");
        }
    }
}
