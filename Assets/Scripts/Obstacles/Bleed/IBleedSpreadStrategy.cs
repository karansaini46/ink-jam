using System;
using InkJam.Core;

namespace InkJam.Obstacles.Bleed
{
    public interface IBleedSpreadStrategy
    {
        GridCoord? GetNextSpread(Board board, InkBleedObstacle bleedState, Random random);
        IBleedSpreadStrategy Clone();
    }
}
