using InkJam.Core;

namespace InkJam.Obstacles
{
    public interface IObstacle
    {
        bool BlocksMovementThrough(GridCoord coord);
        bool BlocksTileAt(GridCoord coord);
        IObstacle Clone();
    }
}
