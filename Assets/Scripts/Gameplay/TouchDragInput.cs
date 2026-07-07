using UnityEngine;
using InkJam.Core;

namespace InkJam.Gameplay
{
    [RequireComponent(typeof(LevelController))]
    public class TouchDragInput : MonoBehaviour
    {
        [Tooltip("Minimum drag distance in pixels to register as a slide.")]
        public float minDragDistance = 50f;
        
        [Tooltip("If the drag angle is within this many degrees of a diagonal (45, 135, 225, 315), it will be rejected as ambiguous.")]
        public float diagonalToleranceAngle = 15f;

        private LevelController _levelController;
        private Camera _mainCamera;
        
        private bool _isDragging = false;
        private Vector2 _startDragPosition;
        private TileView _selectedTileView;

        private void Awake()
        {
            _levelController = GetComponent<LevelController>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_levelController.CurrentState == LevelState.Won) return;

            // Handle Mouse Input (Editor & Standalone)
            if (Input.GetMouseButtonDown(0))
            {
                HandleInputDown(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && _isDragging)
            {
                HandleInputUp(Input.mousePosition);
            }

            // Handle Touch Input (Device)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    HandleInputDown(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    if (_isDragging)
                    {
                        HandleInputUp(touch.position);
                    }
                }
            }
        }

        private void HandleInputDown(Vector2 screenPosition)
        {
            _isDragging = false;
            _selectedTileView = null;

            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            
            // Try 2D Raycast first (assuming 2D game)
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
            if (hit2D.collider != null)
            {
                _selectedTileView = hit2D.collider.GetComponent<TileView>();
            }
            else
            {
                // Fallback for 3D colliders
                if (Physics.Raycast(ray, out RaycastHit hit3D))
                {
                    _selectedTileView = hit3D.collider.GetComponent<TileView>();
                }
            }

            if (_selectedTileView != null && _selectedTileView.Tile != null)
            {
                _isDragging = true;
                _startDragPosition = screenPosition;
            }
        }

        private void HandleInputUp(Vector2 screenPosition)
        {
            _isDragging = false;
            if (_selectedTileView == null || _selectedTileView.Tile == null) return;

            Vector2 dragVector = screenPosition - _startDragPosition;
            if (dragVector.magnitude < minDragDistance)
            {
                _selectedTileView = null;
                return;
            }

            Direction dragDir = DetermineDragDirection(dragVector);
            
            if (dragDir != Direction.None)
            {
                _levelController.Slide(_selectedTileView.Tile, dragDir);
            }

            _selectedTileView = null;
        }

        private Direction DetermineDragDirection(Vector2 dragVector)
        {
            // Calculate angle in degrees [-180, 180] -> [0, 360]
            float angle = Mathf.Atan2(dragVector.y, dragVector.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // Reject if too close to diagonals (45, 135, 225, 315)
            float distTo45 = Mathf.Abs(Mathf.DeltaAngle(angle, 45f));
            float distTo135 = Mathf.Abs(Mathf.DeltaAngle(angle, 135f));
            float distTo225 = Mathf.Abs(Mathf.DeltaAngle(angle, 225f));
            float distTo315 = Mathf.Abs(Mathf.DeltaAngle(angle, 315f));

            if (distTo45 < diagonalToleranceAngle || 
                distTo135 < diagonalToleranceAngle || 
                distTo225 < diagonalToleranceAngle || 
                distTo315 < diagonalToleranceAngle)
            {
                return Direction.None; // Ambiguous drag
            }

            // Determine dominant axis
            if (angle > 315f || angle <= 45f) return Direction.Right;
            if (angle > 45f && angle <= 135f) return Direction.Up;
            if (angle > 135f && angle <= 225f) return Direction.Left;
            if (angle > 225f && angle <= 315f) return Direction.Down;

            return Direction.None;
        }
    }
}
