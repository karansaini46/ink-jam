using System;
using System.Collections.Generic;
using System.Linq;
using InkJam.Core;

namespace InkJam.Obstacles.Bleed
{
    public class RandomAdjacentSpreadStrategy : IBleedSpreadStrategy
    {
        public GridCoord? GetNextSpread(Board board, InkBleedObstacle bleedState, Random random)
        {
            if (bleedState.BledCells.Count == 0) return null;

            var eligibleNeighbors = new List<GridCoord>();
            var directions = new[] { new GridCoord(0, 1), new GridCoord(0, -1), new GridCoord(1, 0), new GridCoord(-1, 0) };

            foreach (var cell in bleedState.BledCells)
            {
                foreach (var dir in directions)
                {
                    var neighbor = cell + dir;
                    // Check if neighbor is in bounds, not already bled, and not blocked by a solid obstacle
                    if (board.IsCellInBounds(neighbor) && !bleedState.BledCells.Contains(neighbor))
                    {
                        // Check if another obstacle blocks entering it (like a fixed wall)
                        bool blocked = board.Obstacles.Any(o => o != bleedState && o.BlocksMovementThrough(neighbor));
                        if (!blocked)
                        {
                            eligibleNeighbors.Add(neighbor);
                        }
                    }
                }
            }

            if (eligibleNeighbors.Count == 0) return null;

            // Remove duplicates
            var uniqueEligible = eligibleNeighbors.Distinct().ToList();

            int index = random.Next(uniqueEligible.Count);
            return uniqueEligible[index];
        }

        public IBleedSpreadStrategy Clone()
        {
            return new RandomAdjacentSpreadStrategy();
        }
    }
}
