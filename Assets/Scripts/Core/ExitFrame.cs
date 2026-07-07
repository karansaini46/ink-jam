namespace InkJam.Core
{
    public class ExitFrame
    {
        public GridCoord Coord { get; private set; }
        public Direction Edge { get; private set; }
        public TileColor AcceptedColor { get; private set; }

        public ExitFrame(GridCoord coord, Direction edge, TileColor acceptedColor)
        {
            Coord = coord;
            Edge = edge;
            AcceptedColor = acceptedColor;
        }

        public ExitFrame Clone()
        {
            return new ExitFrame(Coord, Edge, AcceptedColor);
        }
    }
}
