using UnityEngine;
using System.Collections.Generic;
using InkJam.Core;
using InkJam.Obstacles;

using InkJam.Art;

namespace InkJam.Gameplay
{
    [RequireComponent(typeof(LevelController))]
    public class BoardRenderer : MonoBehaviour
    {
        [Header("Settings")]
        public float tileSize = 1f;
        public float spacing = 0.1f;
        public float slideSpeed = 15f;
        
        [Header("Animation Timings")]
        public float slideDuration = 0.15f;
        public float bleedAnimDuration = 0.2f;
        public float exitAnimDuration = 0.3f;

        private LevelController _levelController;
        private List<GameObject> _staticVisuals = new List<GameObject>();
        private Dictionary<Tile, GameObject> _tileViews = new Dictionary<Tile, GameObject>();

        private void Awake()
        {
            _levelController = GetComponent<LevelController>();
            _levelController.OnLevelLoaded += BuildBoardVisuals;
            _levelController.OnLevelWon += ClearBoard;
            _levelController.OnTileSlid += HandleTileSlid;
            _levelController.OnTileExited += HandleTileExited;
            BleedSystem.OnBleedSpread += HandleBleedSpread;

            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.OnThemeChanged += BuildBoardVisuals;
            }
        }

        private void OnDestroy()
        {
            if (_levelController != null)
            {
                _levelController.OnLevelLoaded -= BuildBoardVisuals;
                _levelController.OnLevelWon -= ClearBoard;
                _levelController.OnTileSlid -= HandleTileSlid;
                _levelController.OnTileExited -= HandleTileExited;
            }
            BleedSystem.OnBleedSpread -= HandleBleedSpread;
            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.OnThemeChanged -= BuildBoardVisuals;
            }
        }

        private void Update()
        {
            // Update loop removed, tiles are now animated via Coroutines in HandleTileSlid
        }

        private void HandleTileSlid(Tile tile, SlideResult result)
        {
            if (_tileViews.TryGetValue(tile, out GameObject visual))
            {
                StartCoroutine(SlideCoroutine(visual, GridToWorld(result.EndCoord), tile));
            }
        }

        private System.Collections.IEnumerator SlideCoroutine(GameObject visual, Vector3 targetPos, Tile tile)
        {
            InkJam.Audio.AudioManager.Instance?.PlaySlide();
            
            Vector3 startPos = visual.transform.position;
            float elapsed = 0f;
            
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                visual.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            visual.transform.position = targetPos;

            if (tile.IsExited)
            {
                visual.SetActive(false);
            }
        }

        private void HandleTileExited(Tile tile)
        {
            InkJam.Audio.AudioManager.Instance?.PlayExit();

            ThemeData theme = ThemeManager.Instance != null ? ThemeManager.Instance.currentTheme : null;
            Sprite splashSprite = theme != null ? theme.inkBleedSprite : null;
            if (splashSprite != null)
            {
                InkJam.VFX.SplashEffect.Spawn(splashSprite, GridToWorld(tile.CurrentCoord), GetColor(tile.Color), exitAnimDuration);
            }
        }

        private void HandleBleedSpread(GridCoord cell)
        {
            InkJam.Audio.AudioManager.Instance?.PlayBleedSpread();

            ThemeData theme = ThemeManager.Instance != null ? ThemeManager.Instance.currentTheme : null;

            GameObject bObj = new GameObject($"InkBleed_{cell.X}_{cell.Y}");
            bObj.transform.SetParent(this.transform);
            bObj.transform.position = GridToWorld(cell) - new Vector3(0, 0, 0.1f);
            
            var sr = bObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 0;
            if (theme != null && theme.inkBleedSprite != null)
            {
                sr.sprite = theme.inkBleedSprite;
            }
            else
            {
                sr.color = new Color(0.6f, 0f, 0.8f);
            }
            _staticVisuals.Add(bObj);

            float targetScale = theme != null && theme.inkBleedSprite != null ? (tileSize * 0.9f / theme.inkBleedSprite.bounds.size.x) : 1f;
            StartCoroutine(BleedPopCoroutine(bObj, targetScale));
        }

        private System.Collections.IEnumerator BleedPopCoroutine(GameObject bObj, float targetScale)
        {
            float elapsed = 0f;
            Vector3 finalScale = Vector3.one * targetScale;
            bObj.transform.localScale = Vector3.zero;

            while (elapsed < bleedAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / bleedAnimDuration);
                
                // Simple pop-in with overshoot
                float scaleT = t < 0.7f ? Mathf.Lerp(0f, 1.2f, t / 0.7f) : Mathf.Lerp(1.2f, 1f, (t - 0.7f) / 0.3f);
                
                bObj.transform.localScale = finalScale * scaleT;
                yield return null;
            }
            bObj.transform.localScale = finalScale;
        }

        public void BuildBoardVisuals()
        {
            ClearBoard();

            Board board = _levelController.CurrentBoard;
            if (board == null) return;

            ThemeData theme = ThemeManager.Instance != null ? ThemeManager.Instance.currentTheme : null;

            // 1. Draw Grid Background
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    GameObject bg = new GameObject($"Cell_{x}_{y}");
                    bg.transform.SetParent(this.transform);
                    bg.transform.position = GridToWorld(new GridCoord(x, y));
                    
                    var sr = bg.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = -10;
                    if (theme != null && theme.gridCellSprite != null)
                    {
                        sr.sprite = theme.gridCellSprite;
                        bg.transform.localScale = Vector3.one * (tileSize / theme.gridCellSprite.bounds.size.x);
                    }
                    _staticVisuals.Add(bg);
                }
            }

            // 2. Draw Exit Frames
            foreach (var frame in board.ExitFrames)
            {
                GameObject fObj = new GameObject($"ExitFrame_{frame.Location.x}_{frame.Location.y}");
                fObj.transform.SetParent(this.transform);
                
                Vector2 dirOffset = GetDirectionVector(frame.Edge);
                Vector3 worldPos = GridToWorld(frame.Location) + new Vector3(dirOffset.x, dirOffset.y, 0) * (tileSize + spacing);
                fObj.transform.position = worldPos;
                
                var sr = fObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -5;
                if (theme != null && theme.exitFrameSprite != null)
                {
                    sr.sprite = theme.exitFrameSprite;
                    fObj.transform.localScale = Vector3.one * (tileSize * 0.8f / theme.exitFrameSprite.bounds.size.x);
                }
                sr.color = GetColor(frame.AcceptedColor);
                
                _staticVisuals.Add(fObj);
            }

            // 3. Draw Obstacles
            foreach (var obs in board.Obstacles)
            {
                if (obs is InkBleedObstacle bleedObs)
                {
                    foreach (var cell in bleedObs.BledCells)
                    {
                        GameObject bObj = new GameObject($"InkBleed_{cell.x}_{cell.y}");
                        bObj.transform.SetParent(this.transform);
                        bObj.transform.position = GridToWorld(cell) - new Vector3(0, 0, 0.1f);
                        
                        var sr = bObj.AddComponent<SpriteRenderer>();
                        sr.sortingOrder = 0;
                        if (theme != null && theme.inkBleedSprite != null)
                        {
                            sr.sprite = theme.inkBleedSprite;
                            bObj.transform.localScale = Vector3.one * (tileSize * 0.9f / theme.inkBleedSprite.bounds.size.x);
                        }
                        else
                        {
                            sr.color = new Color(0.6f, 0f, 0.8f);
                        }
                        _staticVisuals.Add(bObj);
                    }
                }
                else if (obs is FixedObstacle fixedObs)
                {
                    GameObject fObj = new GameObject($"FixedObs_{fixedObs.Location.x}_{fixedObs.Location.y}");
                    fObj.transform.SetParent(this.transform);
                    fObj.transform.position = GridToWorld(fixedObs.Location) - new Vector3(0, 0, 0.1f);
                    
                    var sr = fObj.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 0;
                    if (theme != null && theme.fixedObstacleSprite != null)
                    {
                        sr.sprite = theme.fixedObstacleSprite;
                        fObj.transform.localScale = Vector3.one * (tileSize * 0.9f / theme.fixedObstacleSprite.bounds.size.x);
                    }
                    else
                    {
                        sr.color = Color.black;
                    }
                    _staticVisuals.Add(fObj);
                }
            }

            // 4. Draw Tiles
            foreach (var tile in board.Tiles)
            {
                GameObject tObj = new GameObject($"Tile_{tile.Id}");
                tObj.transform.SetParent(this.transform);
                tObj.transform.position = GridToWorld(tile.CurrentCoord) - new Vector3(0, 0, 0.2f);
                
                TileView view = tObj.AddComponent<TileView>();
                view.Tile = tile;

                var sr = tObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;
                
                if (theme != null)
                {
                    Sprite s = theme.GetTileSprite(tile.Color);
                    if (s != null)
                    {
                        sr.sprite = s;
                        tObj.transform.localScale = Vector3.one * (tileSize * 0.8f / s.bounds.size.x);
                    }
                }
                
                if (sr.sprite == null)
                {
                    // Fallback visual
                    sr.color = GetColor(tile.Color);
                }

                // Add BoxCollider2D for Raycasts since we removed 3D primitives
                var col = tObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(tileSize * 0.8f, tileSize * 0.8f);

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
