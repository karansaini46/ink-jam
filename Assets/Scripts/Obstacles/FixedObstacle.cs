using InkJam.Core;

namespace InkJam.Obstacles
{
    public class FixedObstacle : IObstacle
    {
        public GridCoord Coord { get; private set; }

        public FixedObstacle(GridCoord coord)
        {
            Coord = coord;
        }

        public bool BlocksMovementThrough(GridCoord coord)
        {
            return Coord == coord;
        }

        public bool BlocksTileAt(GridCoord coord)
        {
            return Coord == coord;
        }

        public IObstacle Clone()
        {
            return new FixedObstacle(Coord);
        }
    }
}
