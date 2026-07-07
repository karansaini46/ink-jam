namespace InkJam.Obstacles
{
    public interface IObstacleState
    {
        /// <summary>
        /// Creates a deep copy of the obstacle state, used for board state cloning.
        /// </summary>
        IObstacleState Clone();
    }
}
