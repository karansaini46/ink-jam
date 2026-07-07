using System;

namespace InkJam.Core
{
    [Serializable]
    public readonly struct GridCoord : IEquatable<GridCoord>
    {
        public readonly int x;
        public readonly int y;

        public GridCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(GridCoord other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public static bool operator ==(GridCoord left, GridCoord right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridCoord left, GridCoord right)
        {
            return !(left == right);
        }

        public static GridCoord operator +(GridCoord left, GridCoord right)
        {
            return new GridCoord(left.x + right.x, left.y + right.y);
        }

        public static GridCoord operator -(GridCoord left, GridCoord right)
        {
            return new GridCoord(left.x - right.x, left.y - right.y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
