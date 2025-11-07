using System;
using System.Collections.Generic;

namespace HexagonalLib.Coordinates
{
    [Serializable]
    public readonly partial struct Axial(int q, int r) : IEquatable<Axial>, IEqualityComparer<Axial>
    {
        public static Axial Zero => new(0, 0);

        public readonly int Q = q;
        public readonly int R = r;

        public static bool operator ==(Axial coord1, Axial coord2)
            => (coord1.Q, coord1.R) == (coord2.Q, coord2.R);

        public static bool operator !=(Axial coord1, Axial coord2)
            => (coord1.Q, coord1.R) != (coord2.Q, coord2.R);

        public static Axial operator +(Axial coord1, Axial coord2)
            => new(coord1.Q + coord2.Q, coord1.R + coord2.R);

        public static Axial operator +(Axial coord, int offset)
            => new(coord.Q + offset, coord.R + offset);

        public static Axial operator -(Axial coord1, Axial coord2)
            => new(coord1.Q - coord2.Q, coord1.R - coord2.R);

        public static Axial operator -(Axial coord, int offset)
            => new(coord.Q - offset, coord.R - offset);

        public static Axial operator *(Axial coord, int offset)
            => new(coord.Q * offset, coord.R * offset);

        public override bool Equals(object other)
            => other is Axial axial && Equals(axial);

        public bool Equals(Axial other) => (Q, R) == (other.Q, other.R);

        public bool Equals(Axial coord1, Axial coord2) => coord1.Equals(coord2);

        public override int GetHashCode() => (Q, R).GetHashCode();

        public int GetHashCode(Axial axial) => axial.GetHashCode();

        public override string ToString() => $"A-[{Q}:{R}]";
    }
}