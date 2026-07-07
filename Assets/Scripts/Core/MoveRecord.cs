using System;
using System.Collections.Generic;

namespace InkJam.Core
{
    [Serializable]
    public struct MoveRecord
    {
        public int TileId { get; }
        public Direction Direction { get; }
        public List<GridCoord> Path { get; }

        public MoveRecord(int tileId, Direction direction, List<GridCoord> path)
        {
            TileId = tileId;
            Direction = direction;
            Path = new List<GridCoord>(path);
        }
    }
}
