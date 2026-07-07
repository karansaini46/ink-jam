using InkJam.Obstacles;

namespace InkJam.Core
{
    public class Tile
    {
        public int Id { get; private set; }
        public TileColor Color { get; private set; }
        public GridCoord CurrentCoord { get; set; }
        public bool IsExited { get; set; }
        public bool IsLocked { get; set; }
        public IObstacleState ObstacleState { get; set; }

        public Tile(int id, TileColor color, GridCoord startingCoord)
        {
            Id = id;
            Color = color;
            CurrentCoord = startingCoord;
            IsExited = false;
        }

        public Tile Clone()
        {
            return new Tile(Id, Color, CurrentCoord)
            {
                IsExited = this.IsExited,
                IsLocked = this.IsLocked,
                ObstacleState = this.ObstacleState?.Clone()
            };
        }
    }
}
