using UnityEngine;
using System.Collections.Generic;
using InkJam.Core;
using InkJam.Obstacles;

namespace InkJam.Gameplay
{
    [RequireComponent(typeof(LevelController))]
    public class BoardRenderer : MonoBehaviour
    {
        [Header("Settings")]
        public float tileSize = 1f;
        public float spacing = 0.1f;
        public float slideSpeed = 15f;

        private LevelController _levelController;
        private List<GameObject> _staticVisuals = new List<GameObject>();
        private Dictionary<Tile, GameObject> _tileViews = new Dictionary<Tile, GameObject>();

        private void Awake()
        {
            _levelController = GetComponent<LevelController>();
            _levelController.OnLevelLoaded += BuildBoardVisuals;
            _levelController.OnLevelWon += ClearBoard;
        }

        private void OnDestroy()
        {
            if (_levelController != null)
            {
                _levelController.OnLevelLoaded -= BuildBoardVisuals;
                _levelController.OnLevelWon -= ClearBoard;
            }
        }

        private void Update()
        {
            // Smoothly move tiles to their current logical coordinates
            foreach (var kvp in _tileViews)
            {
                Tile tileData = kvp.Key;
                GameObject visual = kvp.Value;
                
                if (tileData.IsExited)
                {
                    visual.SetActive(false); // Hide if exited
                    continue;
                }

                Vector3 targetPos = GridToWorld(tileData.CurrentCoord);
                visual.transform.position = Vector3.Lerp(visual.transform.position, targetPos, Time.deltaTime * slideSpeed);
            }
        }

        public void BuildBoardVisuals()
        {
            ClearBoard();

            Board board = _levelController.CurrentBoard;
            if (board == null) return;

            // Center the board by offsetting the parent GameObject's position, or just build from origin
            // We'll build from origin for simplicity.

            // 1. Draw Grid Background (optional, but helps see the board)
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    bg.transform.SetParent(this.transform);
                    bg.transform.position = GridToWorld(new GridCoord(x, y));
                    bg.transform.localScale = Vector3.one * tileSize;
                    bg.GetComponent<MeshRenderer>().material.color = new Color(0.9f, 0.9f, 0.9f); // light grey
                    // Remove collider so raycasts pass through to tiles
                    Destroy(bg.GetComponent<Collider>());
                    _staticVisuals.Add(bg);
                }
            }

            // 2. Draw Exit Frames
            foreach (var frame in board.ExitFrames)
            {
                GameObject fObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fObj.transform.SetParent(this.transform);
                
                // Position it slightly outside the grid based on edge
                Vector2 dirOffset = GetDirectionVector(frame.Edge);
                Vector3 worldPos = GridToWorld(frame.Location) + new Vector3(dirOffset.x, dirOffset.y, 0) * (tileSize + spacing);
                fObj.transform.position = worldPos;
                
                // Make it look like a frame (thinner, slightly larger)
                fObj.transform.localScale = Vector3.one * tileSize * 0.8f;
                
                // Color it
                fObj.GetComponent<MeshRenderer>().material.color = GetColor(frame.AcceptedColor);
                Destroy(fObj.GetComponent<Collider>());
                
                _staticVisuals.Add(fObj);
            }

            // 3. Draw Obstacles
            foreach (var obs in board.Obstacles)
            {
                if (obs is InkBleedObstacle bleedObs)
                {
                    // Draw each bleed cell
                    foreach (var cell in bleedObs.BledCells)
                    {
                        GameObject bObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        bObj.transform.SetParent(this.transform);
                        bObj.transform.position = GridToWorld(cell);
                        bObj.transform.localScale = Vector3.one * tileSize * 0.9f;
                        bObj.GetComponent<MeshRenderer>().material.color = new Color(0.6f, 0f, 0.8f); // Purple for bleed
                        // Bring slightly forward so it sits on top of background but under tiles
                        bObj.transform.position -= new Vector3(0, 0, 0.1f);
                        Destroy(bObj.GetComponent<Collider>());
                        _staticVisuals.Add(bObj);
                    }
                }
                else if (obs is FixedObstacle fixedObs)
                {
                    GameObject fObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    fObj.transform.SetParent(this.transform);
                    fObj.transform.position = GridToWorld(fixedObs.Location);
                    fObj.transform.localScale = Vector3.one * tileSize * 0.9f;
                    fObj.GetComponent<MeshRenderer>().material.color = Color.black; // Black for fixed wall
                    fObj.transform.position -= new Vector3(0, 0, 0.1f);
                    Destroy(fObj.GetComponent<Collider>());
                    _staticVisuals.Add(fObj);
                }
            }

            // 4. Draw Tiles
            foreach (var tile in board.Tiles)
            {
                GameObject tObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tObj.transform.SetParent(this.transform);
                tObj.transform.position = GridToWorld(tile.CurrentCoord);
                tObj.transform.localScale = Vector3.one * tileSize * 0.8f;
                
                // Important: Ensure it has a TileView so input raycast works
                TileView view = tObj.AddComponent<TileView>();
                view.Tile = tile;

                tObj.GetComponent<MeshRenderer>().material.color = GetColor(tile.Color);
                
                // Bring tiles further forward
                tObj.transform.position -= new Vector3(0, 0, 0.2f);
                
                // A BoxCollider is automatically added by CreatePrimitive, which is perfect for Raycasts.

                _tileViews.Add(tile, tObj);
            }
            
            // Adjust Camera to fit board
            Camera.main.transform.position = new Vector3(
                ((board.Width - 1) * (tileSize + spacing)) / 2f,
                ((board.Height - 1) * (tileSize + spacing)) / 2f,
                -10f
            );
            Camera.main.orthographicSize = Mathf.Max(board.Width, board.Height) * (tileSize + spacing) * 0.8f;
            Camera.main.orthographic = true;
        }

        public void ClearBoard()
        {
            foreach (var obj in _staticVisuals)
            {
                if (obj != null) Destroy(obj);
            }
            _staticVisuals.Clear();

            foreach (var kvp in _tileViews)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _tileViews.Clear();
        }

        private Vector3 GridToWorld(GridCoord coord)
        {
            float xPos = coord.X * (tileSize + spacing);
            float yPos = coord.Y * (tileSize + spacing);
            return new Vector3(xPos, yPos, 0);
        }

        private Vector2 GetDirectionVector(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return Vector2.up;
                case Direction.Down: return Vector2.down;
                case Direction.Left: return Vector2.left;
                case Direction.Right: return Vector2.right;
                default: return Vector2.zero;
            }
        }

        private Color GetColor(TileColor tc)
        {
            switch (tc)
            {
                case TileColor.Red: return Color.red;
                case TileColor.Blue: return Color.blue;
                case TileColor.Green: return Color.green;
                case TileColor.Yellow: return Color.yellow;
                case TileColor.Orange: return new Color(1f, 0.5f, 0f);
                case TileColor.Purple: return new Color(0.5f, 0f, 0.5f);
                default: return Color.white;
            }
        }
    }
}
