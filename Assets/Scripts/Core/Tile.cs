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
        public string LinkId { get; set; }
        public GridCoord PreExitCoord { get; set; }

        public int LayerCount { get; private set; }
        public event System.Action<int> OnLayerCountChanged;

        public Tile(int id, TileColor color, GridCoord startingCoord, int layerCount = 0, string linkId = null)
        {
            Id = id;
            Color = color;
            CurrentCoord = startingCoord;
            LayerCount = layerCount;
            IsExited = false;
            LinkId = linkId;
        }

        public void DecrementLayer()
        {
            if (LayerCount > 0)
            {
                LayerCount--;
                OnLayerCountChanged?.Invoke(LayerCount);
            }
        }

        public void IncrementLayer()
        {
            LayerCount++;
            OnLayerCountChanged?.Invoke(LayerCount);
        }

        public Tile Clone()
        {
            return new Tile(Id, Color, CurrentCoord, LayerCount, LinkId)
            {
                IsExited = this.IsExited,
                IsLocked = this.IsLocked,
                ObstacleState = this.ObstacleState?.Clone(),
                PreExitCoord = this.PreExitCoord
            };
        }
    }
}
