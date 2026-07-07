using UnityEngine;
using InkJam.Core;

namespace InkJam.Gameplay
{
    [RequireComponent(typeof(LevelController))]
    public class TileDragController : MonoBehaviour
    {
        private LevelController _levelController;
        private Camera _mainCamera;

        private Tile _draggedTile;
        private Vector2 _touchStartPos;
        private bool _isDragging;

        private const float SwipeThreshold = 30f; // Pixels needed to register a swipe

        private void Awake()
        {
            _levelController = GetComponent<LevelController>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_levelController.CurrentState != LevelState.InProgress) return;

            // Handle both touch and mouse input interchangeably
            if (Input.GetMouseButtonDown(0))
            {
                HandlePointerDown(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                HandlePointerDrag(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                HandlePointerUp();
            }

            // Touch support
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    HandlePointerDown(touch.position);
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    HandlePointerDrag(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    HandlePointerUp();
                }
            }
        }

        private void HandlePointerDown(Vector2 screenPosition)
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_mainCamera.transform.position.z));
            
            // Raycast in 2D
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null)
            {
                TileView view = hit.collider.GetComponent<TileView>();
                if (view != null && !view.Tile.IsExited && !view.Tile.IsLocked)
                {
                    _draggedTile = view.Tile;
                    _touchStartPos = screenPosition;
                    _isDragging = true;
                }
            }
        }

        private void HandlePointerDrag(Vector2 screenPosition)
        {
            if (!_isDragging || _draggedTile == null) return;

            Vector2 swipeDelta = screenPosition - _touchStartPos;
            
            // If the swipe distance exceeds the threshold, trigger the move immediately
            if (swipeDelta.sqrMagnitude >= SwipeThreshold * SwipeThreshold)
            {
                Direction swipeDir = GetSwipeDirection(swipeDelta);
                if (swipeDir != Direction.None)
                {
                    _levelController.Slide(_draggedTile, swipeDir);
                }
                
                // Reset dragging state so we don't trigger multiple moves per single swipe
                _isDragging = false;
                _draggedTile = null;
            }
        }

        private void HandlePointerUp()
        {
            _isDragging = false;
            _draggedTile = null;
        }

        private Direction GetSwipeDirection(Vector2 swipeDelta)
        {
            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                // Horizontal
                return swipeDelta.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                // Vertical
                return swipeDelta.y > 0 ? Direction.Up : Direction.Down;
            }
        }
    }
}
