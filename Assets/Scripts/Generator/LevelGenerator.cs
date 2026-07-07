using System;
using System.Collections.Generic;
using System.Linq;
using InkJam.Core;
using InkJam.Data;

namespace InkJam.Generator
{
    public static class LevelGenerator
    {
        public static (LevelData level, List<MoveRecord> solution) Generate(GeneratorParams genParams)
        {
            int seed = genParams.Seed == -1 ? new Random().Next() : genParams.Seed;
            Random rng = new Random(seed);

            Board board = new Board(genParams.Width, genParams.Height);
            List<MoveRecord> solution = new List<MoveRecord>();

            // 1. Initialize Solved State
            InitializeSolvedState(board, genParams, rng);

            // 2. Reverse-walk TargetMoves times
            for (int i = 0; i < genParams.TargetMoves; i++)
            {
                if (!TryPerformReverseMove(board, rng, genParams, out MoveRecord forwardMove))
                {
                    // If we can't find ANY valid reverse move, we have to stop early.
                    // This could happen if the board gets too crowded.
                    break;
                }
                
                // Prepend the valid forward move to our solution list
                solution.Insert(0, forwardMove);
            }

            // 3. Convert to LevelData
            LevelData levelData = ExportToLevelData(board, genParams);

            return (levelData, solution);
        }

        private static void InitializeSolvedState(Board board, GeneratorParams genParams, Random rng)
        {
            // We want ColorCount exit frames around the edge.
            List<GridCoord> edgeCells = new List<GridCoord>();
            for (int x = 0; x < board.Width; x++)
            {
                edgeCells.Add(new GridCoord(x, 0));
                edgeCells.Add(new GridCoord(x, board.Height - 1));
            }
            for (int y = 1; y < board.Height - 1; y++)
            {
                edgeCells.Add(new GridCoord(0, y));
                edgeCells.Add(new GridCoord(board.Width - 1, y));
            }

            // Shuffle edge cells
            edgeCells = edgeCells.OrderBy(c => rng.Next()).ToList();

            TileColor[] availableColors = (TileColor[])Enum.GetValues(typeof(TileColor));
            int colorCount = Math.Min(genParams.ColorCount, availableColors.Length);

            for (int i = 0; i < colorCount; i++)
            {
                if (i >= edgeCells.Count) break;

                GridCoord coord = edgeCells[i];
                Direction edgeDir = GetEdgeDirection(coord, board.Width, board.Height);
                TileColor color = availableColors[i % availableColors.Length];

                ExitFrame frame = new ExitFrame(coord, edgeDir, color);
                board.AddExitFrame(frame);

                // Create a solved tile at the exit frame
                Tile tile = new Tile(i + 1, color, coord);
                board.AddTile(tile);
                board.MarkTileExited(tile); // Now it's effectively "solved"
            }
        }

        private static Direction GetEdgeDirection(GridCoord coord, int width, int height)
        {
            if (coord.y == 0) return Direction.Up; // Entering the bottom edge means moving Up
            if (coord.y == height - 1) return Direction.Down;
            if (coord.x == 0) return Direction.Right;
            if (coord.x == width - 1) return Direction.Left;
            return Direction.Up;
        }

        private static Direction Opposite(Direction dir)
        {
            return dir switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => Direction.None
            };
        }

        private static Direction[] AllDirections = new Direction[] 
        { 
            Direction.Up, Direction.Down, Direction.Left, Direction.Right 
        };

        private static bool TryPerformReverseMove(Board board, Random rng, GeneratorParams genParams, out MoveRecord forwardMove)
        {
            var candidates = new List<(Tile tile, GridCoord startCoord, Direction forwardDir, SlideResult result)>();

            foreach (var tile in board.Tiles)
            {
                if (tile.IsLocked) continue;

                GridCoord currentPos = tile.CurrentCoord;
                if (tile.IsExited)
                {
                    // Find the exit frame for this tile
                    ExitFrame frame = board.ExitFrames.FirstOrDefault(f => f.AcceptedColor == tile.Color);
                    if (frame != null)
                    {
                        currentPos = frame.Coord;
                    }
                }

                foreach (Direction reverseDir in AllDirections)
                {
                    Direction forwardDir = Opposite(reverseDir);

                    // If tile is on board, it MUST be resting against a stopper in forwardDir
                    if (!tile.IsExited)
                    {
                        GridCoord cellAhead = currentPos + forwardDir;
                        bool isBlocked = !board.IsCellInBounds(cellAhead) || 
                                         board.IsCellOccupied(cellAhead) || 
                                         board.Obstacles.Any(obs => obs.BlocksMovementThrough(cellAhead));
                        
                        if (!isBlocked) continue; // Can't be pulled from here in reverseDir
                    }

                    GridCoord testStart = currentPos + reverseDir;
                    while (board.IsCellInBounds(testStart) && 
                           !board.IsCellOccupied(testStart) && 
                           !board.Obstacles.Any(obs => obs.BlocksMovementThrough(testStart)))
                    {
                        // Found a clear candidate start! Verify with TrySlide
                        SlideResult result = null;
                        
                        // Save state
                        bool originallyExited = tile.IsExited;
                        GridCoord origCoord = tile.CurrentCoord;

                        if (originallyExited)
                        {
                            board.RollbackExit(tile, testStart);
                        }
                        else
                        {
                            board.UpdateTilePosition(tile, testStart);
                        }

                        // We can also test reverse-bouncing
                        bool reverseBounced = false;
                        if (genParams.AllowLayers && !originallyExited)
                        {
                            // If it's on the board, and the forward move would hit the exit frame of its color
                            // we could give it a layer so it bounces instead of exiting.
                            ExitFrame exitFrame = board.ExitFrames.FirstOrDefault(f => f.Coord == currentPos + forwardDir);
                            if (exitFrame != null && exitFrame.AcceptedColor == tile.Color)
                            {
                                // Give it a layer temporarily to test the bounce
                                tile.IncrementLayer();
                                reverseBounced = true;
                            }
                        }

                        // Test forward
                        result = MovementSystem.TrySlide(board, tile, forwardDir);

                        // Evaluate validity
                        bool isValid = false;
                        if (originallyExited)
                        {
                            isValid = result.DidExit;
                        }
                        else
                        {
                            isValid = result.EndCoord == origCoord && !result.DidExit;
                        }

                        if (isValid && result.IsValidMove)
                        {
                            candidates.Add((tile, testStart, forwardDir, result));
                        }

                        // Restore state
                        if (reverseBounced)
                        {
                            tile.DecrementLayer();
                        }

                        if (originallyExited)
                        {
                            board.MarkTileExited(tile);
                            // RollbackExit modifies position, MarkTileExited leaves it at testStart
                            // Let's explicitly put it back (even though it's exited)
                            tile.CurrentCoord = origCoord; 
                        }
                        else
                        {
                            board.UpdateTilePosition(tile, origCoord);
                        }

                        testStart += reverseDir;
                    }
                }
            }

            if (candidates.Count == 0)
            {
                forwardMove = default;
                return false;
            }

            // Pick a random valid reverse move
            var chosen = candidates[rng.Next(candidates.Count)];

            // Apply it backwards to the board
            bool wasExited = chosen.tile.IsExited;
            if (wasExited)
            {
                board.RollbackExit(chosen.tile, chosen.startCoord);
            }
            else
            {
                board.UpdateTilePosition(chosen.tile, chosen.startCoord);
            }

            // Apply reverse bounce
            if (chosen.result.Bounced)
            {
                // In forward time, it lost a layer. So in reverse time, it gained one.
                chosen.tile.IncrementLayer();
            }

            // The forward move we just verified
            forwardMove = new MoveRecord(chosen.tile.Id, chosen.forwardDir, chosen.result.Path);
            return true;
        }

        private static LevelData ExportToLevelData(Board board, GeneratorParams genParams)
        {
            LevelData data = new LevelData
            {
                formatVersion = 1,
                width = board.Width,
                height = board.Height,
                tiles = new TileData[board.Tiles.Count],
                exitFrames = new ExitFrameData[board.ExitFrames.Count],
                obstacles = new ObstacleData[board.Obstacles.Count]
            };

            for (int i = 0; i < board.Tiles.Count; i++)
            {
                Tile t = board.Tiles[i];
                data.tiles[i] = new TileData
                {
                    id = t.Id,
                    color = t.Color,
                    startX = t.CurrentCoord.x,
                    startY = t.CurrentCoord.y,
                    layerCount = t.LayerCount,
                    linkId = t.LinkId
                };
            }

            for (int i = 0; i < board.ExitFrames.Count; i++)
            {
                ExitFrame e = board.ExitFrames[i];
                data.exitFrames[i] = new ExitFrameData
                {
                    x = e.Coord.x,
                    y = e.Coord.y,
                    edge = e.Edge,
                    acceptedColor = e.AcceptedColor
                };
            }

            // Not implementing Obstacle serialization in this version yet
            for (int i = 0; i < board.Obstacles.Count; i++)
            {
                // TODO: Serialize obstacle data
            }

            return data;
        }
    }
}
