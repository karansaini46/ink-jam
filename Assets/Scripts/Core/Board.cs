using System.Collections.Generic;
using InkJam.Obstacles;

namespace InkJam.Core
{
    public class Board
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        public List<Tile> Tiles { get; private set; }
        public List<ExitFrame> ExitFrames { get; private set; }
        public List<IObstacle> Obstacles { get; private set; }
        public List<MoveRecord> MoveLog { get; private set; }

        private Dictionary<GridCoord, Tile> _occupiedCells;

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new List<Tile>();
            ExitFrames = new List<ExitFrame>();
            Obstacles = new List<IObstacle>();
            MoveLog = new List<MoveRecord>();
            _occupiedCells = new Dictionary<GridCoord, Tile>();
        }

        public void AddTile(Tile tile)
        {
            Tiles.Add(tile);
            if (!tile.IsExited)
            {
                _occupiedCells[tile.CurrentCoord] = tile;
            }
        }

        public void AddExitFrame(ExitFrame exitFrame)
        {
            ExitFrames.Add(exitFrame);
        }

        public void AddObstacle(IObstacle obstacle)
        {
            Obstacles.Add(obstacle);
        }

        public void UpdateTilePosition(Tile tile, GridCoord newCoord)
        {
            if (_occupiedCells.TryGetValue(tile.CurrentCoord, out var occupant) && occupant == tile)
            {
                _occupiedCells.Remove(tile.CurrentCoord);
            }
            
            tile.CurrentCoord = newCoord;
            
            if (!tile.IsExited)
            {
                _occupiedCells[newCoord] = tile;
            }
        }

        public void MarkTileExited(Tile tile)
        {
            if (_occupiedCells.TryGetValue(tile.CurrentCoord, out var occupant) && occupant == tile)
            {
                _occupiedCells.Remove(tile.CurrentCoord);
            }
            tile.IsExited = true;
        }

        public void RecordMove(MoveRecord record)
        {
            MoveLog.Add(record);
        }

        public bool CheckWinCondition()
        {
            foreach (var tile in Tiles)
            {
                if (!tile.IsLocked && !tile.IsExited)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsCellInBounds(GridCoord coord)
        {
            return coord.x >= 0 && coord.x < Width && coord.y >= 0 && coord.y < Height;
        }

        public bool IsCellOccupied(GridCoord coord)
        {
            return _occupiedCells.ContainsKey(coord);
        }

        public Tile GetTileAt(GridCoord coord)
        {
            if (_occupiedCells.TryGetValue(coord, out Tile tile))
            {
                return tile;
            }
            return null;
        }

        public Board CloneState()
        {
            var clonedBoard = new Board(Width, Height);

            foreach (var frame in ExitFrames)
            {
                clonedBoard.AddExitFrame(frame.Clone());
            }

            foreach (var obs in Obstacles)
            {
                clonedBoard.AddObstacle(obs.Clone());
            }

            foreach (var tile in Tiles)
            {
                clonedBoard.AddTile(tile.Clone());
            }

            foreach (var move in MoveLog)
            {
                clonedBoard.RecordMove(move);
            }

            return clonedBoard;
        }
    }
}
