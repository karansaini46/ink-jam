using System;
using System.Collections.Generic;
using InkJam.Core;

namespace InkJam.Obstacles.Bleed
{
    public class ExitAdjacentCleanseStrategy : IBleedCleanseStrategy
    {
        public List<GridCoord> GetCleansedCells(Board board, InkBleedObstacle bleedState, SlideResult lastMove)
        {
            var cleansedCells = new List<GridCoord>();

            if (lastMove == null || !lastMove.DidExit)
            {
                return cleansedCells;
            }

            // An exiting tile exits from its EndCoord
            GridCoord exitCoord = lastMove.EndCoord;
            var directions = new[] { new GridCoord(0, 1), new GridCoord(0, -1), new GridCoord(1, 0), new GridCoord(-1, 0) };

            foreach (var dir in directions)
            {
                var adjacent = exitCoord + dir;
                
                if (bleedState.BledCells.Contains(adjacent))
                {
                    // Increment progress
                    if (!bleedState.CleanseProgress.ContainsKey(adjacent))
                    {
                        bleedState.CleanseProgress[adjacent] = 0;
                    }
                    
                    bleedState.CleanseProgress[adjacent]++;

                    if (bleedState.CleanseProgress[adjacent] >= bleedState.CleanseRequirement)
                    {
                        cleansedCells.Add(adjacent);
                    }
                }
            }

            return cleansedCells;
        }

        public IBleedCleanseStrategy Clone()
        {
            return new ExitAdjacentCleanseStrategy();
        }
    }
}
