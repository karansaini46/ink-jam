using System.Collections.Generic;
using InkJam.Core;
using InkJam.Obstacles.Bleed;

namespace InkJam.Obstacles
{
    public class InkBleedObstacle : IObstacle
    {
        public HashSet<GridCoord> BledCells { get; private set; }
        public Dictionary<GridCoord, int> CleanseProgress { get; private set; }
        
        public int MovesSinceLastSpread { get; set; }
        public int SpreadInterval { get; private set; }
        public int CleanseRequirement { get; private set; }

        public IBleedSpreadStrategy SpreadStrategy { get; private set; }
        public IBleedCleanseStrategy CleanseStrategy { get; private set; }

        public InkBleedObstacle(int spreadInterval, int cleanseRequirement, IBleedSpreadStrategy spreadStrategy, IBleedCleanseStrategy cleanseStrategy)
        {
            BledCells = new HashSet<GridCoord>();
            CleanseProgress = new Dictionary<GridCoord, int>();
            SpreadInterval = spreadInterval;
            CleanseRequirement = cleanseRequirement;
            SpreadStrategy = spreadStrategy;
            CleanseStrategy = cleanseStrategy;
            MovesSinceLastSpread = 0;
        }

        public void AddBledCell(GridCoord coord)
        {
            BledCells.Add(coord);
        }

        public void RemoveBledCell(GridCoord coord)
        {
            BledCells.Remove(coord);
            CleanseProgress.Remove(coord);
        }

        public bool BlocksMovementThrough(GridCoord coord)
        {
            // Tiles can slide through/onto ink bleed cells
            return false;
        }

        public bool BlocksTileAt(GridCoord coord)
        {
            // Tiles can stop on ink bleed cells
            return false;
        }

        public IObstacle Clone()
        {
            var clone = new InkBleedObstacle(SpreadInterval, CleanseRequirement, SpreadStrategy.Clone(), CleanseStrategy.Clone())
            {
                MovesSinceLastSpread = this.MovesSinceLastSpread
            };

            foreach (var cell in BledCells)
            {
                clone.AddBledCell(cell);
            }

            foreach (var kvp in CleanseProgress)
            {
                clone.CleanseProgress[kvp.Key] = kvp.Value;
            }

            return clone;
        }
    }
}
