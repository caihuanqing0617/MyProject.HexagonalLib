using System;
using System.Collections.Generic;

namespace HexagonalLib.Coordinates
{
    [Serializable]
    public readonly partial struct Offset : IEquatable<Offset>, IEqualityComparer<Offset>
    {
        public static Offset Zero => new(0, 0);

        public readonly int X;
        public readonly int Y;

        public Offset(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }

        public Offset Add(int xOffset, int yOffset) => new(X + xOffset, Y + yOffset);

        public static Offset Clamp(Offset coord, Offset min, Offset max)
        {
            var x = Clamp(coord.X, min.X, max.X);
            var y = Clamp(coord.Y, min.Y, max.Y);

            return new Offset(x, y);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        public static bool operator ==(Offset coord1, Offset coord2)
            => (coord1.X, coord1.Y) == (coord2.X, coord2.Y);

        public static bool operator !=(Offset coord1, Offset coord2)
            => (coord1.X, coord1.Y) != (coord2.X, coord2.Y);

        public static Offset operator +(Offset coord1, Offset coord2)
            => new(coord1.X + coord2.X, coord1.Y + coord2.Y);

        public static Offset operator +(Offset coord, int offset)
            => new(coord.X + offset, coord.Y + offset);

        public static Offset operator -(Offset coord, Offset index2)
            => new(coord.X - index2.X, coord.Y - index2.Y);

        public static Offset operator /(Offset coord, int value)
            => new(coord.X / value, coord.Y / value);

        public static Offset operator *(Offset coord, int offset)
            => new(coord.X * offset, coord.Y * offset);

        public override bool Equals(object obj)
            => obj is Offset other && Equals(other);

        public bool Equals(Offset other) => (X, Y) == (other.X, other.Y);

        public bool Equals(Offset coord1, Offset coord2) => coord1.Equals(coord2);

        public override int GetHashCode() => (X, Y).GetHashCode();

        public int GetHashCode(Offset coord) => coord.GetHashCode();

        public override string ToString() => $"O-[{X}:{Y}]";
    }
}