using System;
using System.Collections.Generic;
using InkJam.Obstacles;

namespace InkJam.Core
{
    public static class BleedSystem
    {
        public static event Action<GridCoord> OnBleedSpread;

        public static void Tick(Board board, SlideResult lastMove, Random random)
        {
            // Find InkBleedObstacle(s)
            var bleedObstacles = new List<InkBleedObstacle>();
            foreach (var obs in board.Obstacles)
            {
                if (obs is InkBleedObstacle bleedObs)
                {
                    bleedObstacles.Add(bleedObs);
                }
            }

            foreach (var bleedState in bleedObstacles)
            {
                // 1. Cleansing
                var cleansedCells = bleedState.CleanseStrategy.GetCleansedCells(board, bleedState, lastMove);
                foreach (var cell in cleansedCells)
                {
                    bleedState.RemoveBledCell(cell);
                }

                // 2. Spreading
                // Only spread if a valid move happened
                if (lastMove != null && lastMove.IsValidMove)
                {
                    bleedState.MovesSinceLastSpread++;
                    if (bleedState.MovesSinceLastSpread >= bleedState.SpreadInterval)
                    {
                        var nextCell = bleedState.SpreadStrategy.GetNextSpread(board, bleedState, random);
                        if (nextCell.HasValue)
                        {
                            bleedState.AddBledCell(nextCell.Value);
                            OnBleedSpread?.Invoke(nextCell.Value);
                        }
                        bleedState.MovesSinceLastSpread = 0;
                    }
                }
            }
            
            // 3. Locking tiles update
            if (bleedObstacles.Count > 0)
            {
                foreach (var tile in board.Tiles)
                {
                    bool shouldLock = false;
                    foreach (var bleedState in bleedObstacles)
                    {
                        if (bleedState.BledCells.Contains(tile.CurrentCoord))
                        {
                            shouldLock = true;
                            break;
                        }
                    }
                    tile.IsLocked = shouldLock;
                }
            }
        }
    }
}
