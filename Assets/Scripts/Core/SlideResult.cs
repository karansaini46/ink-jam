using System;
using System.Collections.Generic;

namespace InkJam.Core
{
    public class SlideResult
    {
        public int TileId { get; }
        public Direction Dir { get; }
        public GridCoord StartCoord { get; }
        public GridCoord EndCoord { get; }
        public List<GridCoord> Path { get; }
        public bool DidExit { get; }
        public bool Bounced { get; }

        public bool IsValidMove => Path.Count > 1 || Bounced;

        public SlideResult(int tileId, Direction dir, GridCoord startCoord, GridCoord endCoord, List<GridCoord> path, bool didExit, bool bounced = false)
        {
            TileId = tileId;
            Dir = dir;
            StartCoord = startCoord;
            EndCoord = endCoord;
            Path = new List<GridCoord>(path);
            DidExit = didExit;
            Bounced = bounced;
        }

        public void Apply(Board board)
        {
            if (!IsValidMove)
            {
                return;
            }

            Tile tile = board.Tiles.Find(t => t.Id == TileId);
            if (tile == null)
            {
                throw new InvalidOperationException($"Tile {TileId} not found on board.");
            }

            if (Bounced)
            {
                tile.DecrementLayer();
            }

            if (DidExit)
            {
                if (Path.Count >= 2)
                {
                    tile.PreExitCoord = Path[Path.Count - 2];
                }
                board.MarkTileExited(tile);
            }
            else
            {
                board.UpdateTilePosition(tile, EndCoord);
            }

            board.RecordMove(new MoveRecord(TileId, Dir, Path));
        }
    }
}
