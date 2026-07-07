using System.Collections.Generic;
using InkJam.Core;

namespace InkJam.Obstacles.Bleed
{
    public interface IBleedCleanseStrategy
    {
        /// <summary>
        /// Evaluates the board and the last move to determine if any bleed cells should be cleansed.
        /// </summary>
        /// <returns>A list of GridCoords that have been successfully cleansed and should be removed from the bleed state.</returns>
        List<GridCoord> GetCleansedCells(Board board, InkBleedObstacle bleedState, SlideResult lastMove);
        IBleedCleanseStrategy Clone();
    }
}
