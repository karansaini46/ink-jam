using System.Collections.Generic;
using System.Linq;

namespace InkJam.Core
{
    public static class MovementSystem
    {
        public static SlideResult TrySlide(Board board, Tile tile, Direction dir)
        {
            GridCoord current = tile.CurrentCoord;
            GridCoord offset = GetDirectionOffset(dir);
            List<GridCoord> path = new List<GridCoord> { current };
            bool didExit = false;

            if (dir == Direction.None || tile.IsExited)
            {
                return new SlideResult(tile.Id, dir, current, current, path, false);
            }

            while (true)
            {
                GridCoord next = current + offset;

                // Check for Exit Frame matching color
                ExitFrame exitFrame = board.ExitFrames.FirstOrDefault(e => e.Coord == next);
                if (exitFrame != null && exitFrame.AcceptedColor == tile.Color)
                {
                    // Valid exit
                    path.Add(next);
                    current = next;
                    didExit = true;
                    break;
                }
                
                // Check if any obstacle blocks movement through this cell
                bool blocked = false;
                foreach (var obs in board.Obstacles)
                {
                    if (obs.BlocksMovementThrough(next))
                    {
                        blocked = true;
                        break;
                    }
                }

                // If there's an exit frame but wrong color, or just out of bounds, stop.
                // Assuming normal grid bounds and standard occupied cells block movement.
                if (!board.IsCellInBounds(next) || board.IsCellOccupied(next) || blocked)
                {
                    break;
                }
                
                // Move forward
                path.Add(next);
                current = next;
            }

            // Check if the final position is valid for stopping
            bool validStop = true;
            foreach (var obs in board.Obstacles)
            {
                if (obs.BlocksTileAt(current))
                {
                    validStop = false;
                    break;
                }
            }

            if (!validStop && !didExit)
            {
                // If it can't stop here, we could revert or mark as invalid.
                // For now, if we somehow ended up on a cell we can't stop at (e.g. ice that ends in a wall),
                // we treat the entire move as invalid or fall back. 
                // Since FixedObstacle blocks movement through anyway, this won't be hit for walls.
            }

            return new SlideResult(tile.Id, dir, tile.CurrentCoord, current, path, didExit);
        }

        private static GridCoord GetDirectionOffset(Direction dir)
        {
            return dir switch
            {
                Direction.Up => new GridCoord(0, 1),
                Direction.Down => new GridCoord(0, -1),
                Direction.Left => new GridCoord(-1, 0),
                Direction.Right => new GridCoord(1, 0),
                _ => new GridCoord(0, 0)
            };
        }
    }
}
