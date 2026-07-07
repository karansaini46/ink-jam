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

        public event Action OnLevelWon;
        public event Action OnLevelLoaded;

        private System.Random _random = new System.Random();

        private void Start()
        {
            if (!string.IsNullOrEmpty(levelToLoad))
            {
                LoadLevel(levelToLoad);
            }
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

            Debug.Log($"Level '{levelName}' loaded.");
            OnLevelLoaded?.Invoke();
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
                    var tile = new Tile(tileData.id, tileData.color, new GridCoord(tileData.startX, tileData.startY));
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

        private void Update()
        {
            if (CurrentState == LevelState.Won) return;

            // Stubbed input events
            if (Input.GetKeyDown(KeyCode.UpArrow)) TryMoveStub(Direction.Up);
            if (Input.GetKeyDown(KeyCode.DownArrow)) TryMoveStub(Direction.Down);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) TryMoveStub(Direction.Left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) TryMoveStub(Direction.Right);
        }

        private void TryMoveStub(Direction dir)
        {
            if (CurrentBoard == null) return;

            // For the stub, just grab the first non-exited, non-locked tile
            Tile tileToMove = CurrentBoard.Tiles.Find(t => !t.IsExited && !t.IsLocked);
            if (tileToMove == null) return;

            Slide(tileToMove, dir);
        }

        public void Slide(Tile tileToMove, Direction dir)
        {
            if (CurrentBoard == null || CurrentState == LevelState.Won) return;
            if (tileToMove == null || tileToMove.IsExited || tileToMove.IsLocked) return;

            var result = MovementSystem.TrySlide(CurrentBoard, tileToMove, dir);
            if (result.IsValidMove)
            {
                result.Apply(CurrentBoard);
                BleedSystem.Tick(CurrentBoard, result, _random);
                CheckWin();
            }
        }

        private void CheckWin()
        {
            if (CurrentBoard.CheckWinCondition())
            {
                CurrentState = LevelState.Won;
                Debug.Log("Level Won!");
                OnLevelWon?.Invoke();
            }
        }
    }
}
