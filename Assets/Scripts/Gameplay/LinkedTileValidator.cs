using System.Collections.Generic;
using System.Linq;
using InkJam.Core;

namespace InkJam.Gameplay
{
    public class LinkedTileValidator
    {
        private Dictionary<string, int> _firstExitMoves = new Dictionary<string, int>();
        private Dictionary<string, Tile> _firstExitedTiles = new Dictionary<string, Tile>();

        public void OnLevelStart()
        {
            _firstExitMoves.Clear();
            _firstExitedTiles.Clear();
        }

        public void ValidateAfterMove(Board board, int currentMoveCount)
        {
            var linkedGroups = board.Tiles
                .Where(t => !string.IsNullOrEmpty(t.LinkId))
                .GroupBy(t => t.LinkId);

            foreach (var group in linkedGroups)
            {
                var tiles = group.ToList();
                if (tiles.Count != 2) continue;

                var t1 = tiles[0];
                var t2 = tiles[1];

                bool t1Exited = t1.IsExited;
                bool t2Exited = t2.IsExited;

                if (t1Exited && t2Exited)
                {
                    _firstExitMoves.Remove(group.Key);
                    _firstExitedTiles.Remove(group.Key);
                }
                else if (t1Exited || t2Exited)
                {
                    var exitedTile = t1Exited ? t1 : t2;

                    if (!_firstExitMoves.ContainsKey(group.Key))
                    {
                        _firstExitMoves[group.Key] = currentMoveCount;
                        _firstExitedTiles[group.Key] = exitedTile;
                    }
                    else
                    {
                        int exitMove = _firstExitMoves[group.Key];
                        if (currentMoveCount > exitMove)
                        {
                            board.RollbackExit(exitedTile, exitedTile.PreExitCoord);
                            _firstExitMoves.Remove(group.Key);
                            _firstExitedTiles.Remove(group.Key);
                        }
                    }
                }
                else
                {
                    _firstExitMoves.Remove(group.Key);
                    _firstExitedTiles.Remove(group.Key);
                }
            }
        }
    }
}
