using System;
using System.Collections.Generic;
using HexagonalLib.Coordinates;
using static System.Math;

namespace HexagonalLib
{
    /// <summary>
    /// Represent geometry logic for infinity hexagonal grid
    /// </summary>
    public readonly partial struct HexagonalGrid
    {
        /// <summary>
        /// Total count of edges in one Hex
        /// </summary>
        public const int EDGES_COUNT = 6;

        public static readonly float Sqrt3 = (float) Sqrt(3);

        /// <summary>
        /// Inscribed radius of the hex
        /// </summary>
        public readonly float InscribedRadius;

        /// <summary>
        /// Described radius of hex
        /// </summary>
        public readonly float DescribedRadius;

        /// <summary>
        /// Hexagon side length
        /// </summary>
        public float Side => DescribedRadius;

        /// <summary>
        /// Inscribed diameter of hex
        /// </summary>
        public float InscribedDiameter => InscribedRadius * 2;

        /// <summary>
        /// Described diameter of hex
        /// </summary>
        public float DescribedDiameter => DescribedRadius * 2;

        /// <summary>
        /// Orientation and layout of this grid
        /// </summary>
        public readonly HexagonalGridType Type;

        /// <summary>
        /// Offset between hex and its right-side neighbour on X axis
        /// </summary>
        public float HorizontalOffset
        {
            get
            {
                switch (Type)
                {
                    case HexagonalGridType.PointyOdd:
                    case HexagonalGridType.PointyEven:
                        return InscribedRadius * 2.0f;
                    case HexagonalGridType.FlatOdd:
                    case HexagonalGridType.FlatEven:
                        return DescribedRadius * 1.5f;
                    default:
                        throw new HexagonalException($"Can't get {nameof(HorizontalOffset)} with unexpected {nameof(Type)}", this);
                }
            }
        }

        /// <summary>
        /// Offset between hex and its up-side neighbour on Y axis
        /// </summary>
        public float VerticalOffset
        {
            get
            {
                switch (Type)
                {
                    case HexagonalGridType.PointyOdd:
                    case HexagonalGridType.PointyEven:
                        return DescribedRadius * 1.5f;
                    case HexagonalGridType.FlatOdd:
                    case HexagonalGridType.FlatEven:
                        return InscribedRadius * 2.0f;
                    default:
                        throw new HexagonalException($"Can't get {nameof(VerticalOffset)} with unexpected {nameof(Type)}", this);
                }
            }
        }

        /// <summary>
        /// The angle between the centers of any hex and its first neighbor relative to the vector (0, 1) clockwise
        /// </summary>
        /// <exception cref="HexagonalException"></exception>
        public float AngleToFirstNeighbor
        {
            get
            {
                switch (Type)
                {
                    case HexagonalGridType.PointyOdd:
                    case HexagonalGridType.PointyEven:
                        return 0.0f;
                    case HexagonalGridType.FlatOdd:
                    case HexagonalGridType.FlatEven:
                        return 30.0f;
                    default:
                        throw new HexagonalException($"Can't get {nameof(AngleToFirstNeighbor)} with unexpected {nameof(Type)}", this);
                }
            }
        }

        /// <summary>
        /// Base constructor for hexagonal grid
        /// </summary>
        /// <param name="type">Orientation and layout of the grid</param>
        /// <param name="radius">Inscribed radius</param>
        public HexagonalGrid(HexagonalGridType type, float radius)
        {
            Type = type;
            InscribedRadius = radius;
            DescribedRadius = (float) (radius / Cos(PI / EDGES_COUNT));
        }

        #region ToOffset

        /// <summary>
        /// Convert cubic coordinate to offset
        /// </summary>
        public Offset ToOffset(Cubic coord)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                {
                    var col = coord.X + (coord.Z - (coord.Z & 1)) / 2;
                    var row = coord.Z;
                    return new Offset(col, row);
                }
                case HexagonalGridType.PointyEven:
                {
                    var col = coord.X + (coord.Z + (coord.Z & 1)) / 2;
                    var row = coord.Z;
                    return new Offset(col, row);
                }
                case HexagonalGridType.FlatOdd:
                {
                    var col = coord.X;
                    var row = coord.Z + (coord.X - (coord.X & 1)) / 2;
                    return new Offset(col, row);
                }
                case HexagonalGridType.FlatEven:
                {
                    var col = coord.X;
                    var row = coord.Z + (coord.X + (coord.X & 1)) / 2;
                    return new Offset(col, row);
                }
                default:
                    throw new HexagonalException($"{nameof(ToOffset)} failed with unexpected {nameof(Type)}", this, (nameof(coord), coord));
            }
        }

        /// <summary>
        /// Convert axial coordinate to offset
        /// </summary>
        public Offset ToOffset(Axial axial) => ToOffset(ToCubic(axial));

        /// <summary>
        /// Returns the offset coordinate of the hex which contains a point
        /// </summary>
        public Offset ToOffset(float x, float y) => ToOffset(ToCubic(x, y));

        /// <summary>
        /// Returns the offset coordinate of the hex which contains a point
        /// </summary>
        public Offset ToOffset((float X, float Y) point) => ToOffset(ToCubic(point.X, point.Y));

        #endregion

        #region ToAxial

        /// <summary>
        /// Convert cubic coordinate to axial
        /// </summary>
        public Axial ToAxial(Cubic cubic) => new(cubic.X, cubic.Z);

        /// <summary>
        /// Convert offset coordinate to axial
        /// </summary>
        public Axial ToAxial(Offset offset) => ToAxial(ToCubic(offset));

        /// <summary>
        /// Returns the axial coordinate of the hex which contains a point
        /// </summary>
        public Axial ToAxial(float x, float y) => ToAxial(ToCubic(x, y));

        /// <summary>
        /// Returns the axial coordinate of the hex which contains a point
        /// </summary>
        public Axial ToAxial((float X, float Y) point) => ToAxial(ToCubic(point.X, point.Y));

        #endregion

        #region ToCubic

        /// <summary>
        /// Convert offset coordinate to cubic
        /// </summary>
        public Cubic ToCubic(Offset coord)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                {
                    var x = coord.X - (coord.Y - (coord.Y & 1)) / 2;
                    var z = coord.Y;
                    var y = -x - z;
                    return new Cubic(x, y, z);
                }
                case HexagonalGridType.PointyEven:
                {
                    var x = coord.X - (coord.Y + (coord.Y & 1)) / 2;
                    var z = coord.Y;
                    var y = -x - z;
                    return new Cubic(x, y, z);
                }
                case HexagonalGridType.FlatOdd:
                {
                    var x = coord.X;
                    var z = coord.Y - (coord.X - (coord.X & 1)) / 2;
                    var y = -x - z;
                    return new Cubic(x, y, z);
                }
                case HexagonalGridType.FlatEven:
                {
                    var x = coord.X;
                    var z = coord.Y - (coord.X + (coord.X & 1)) / 2;
                    var y = -x - z;
                    return new Cubic(x, y, z);
                }
                default:
                    throw new HexagonalException($"{nameof(ToCubic)} failed with unexpected {nameof(Type)}", this, (nameof(coord), coord));
            }
        }

        /// <summary>
        /// Convert axial coordinate to cubic
        /// </summary>
        public Cubic ToCubic(Axial axial) => new(axial.Q, -axial.Q - axial.R, axial.R);

        /// <summary>
        /// Returns the cubic coordinate of the hex which contains a point
        /// </summary>
        public Cubic ToCubic(float x, float y)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                case HexagonalGridType.PointyEven:
                {
                    var q = (x * Sqrt3 / 3.0f - y / 3.0f) / Side;
                    var r = y * 2.0f / 3.0f / Side;
                    return new Cubic(q, -q - r, r);
                }
                case HexagonalGridType.FlatOdd:
                case HexagonalGridType.FlatEven:
                {
                    var q = x * 2.0f / 3.0f / Side;
                    var r = (-x / 3.0f + Sqrt3 / 3.0f * y) / Side;
                    return new Cubic(q, -q - r, r);
                }
                default:
                    throw new HexagonalException($"{nameof(ToCubic)} failed with unexpected {nameof(Type)}", this, (nameof(x), x), (nameof(y), y));
            }
        }

        /// <summary>
        /// Returns the cubic coordinate of the hex which contains a point
        /// </summary>
        public Cubic ToCubic((float X, float Y) point) => ToCubic(point.X, point.Y);

        #endregion

        #region ToPoint2

        /// <summary>
        /// Convert hex based on its offset coordinate to it center position in 2d space
        /// </summary>
        public (float X, float Y) ToPoint2(Offset coord) => ToPoint2(ToAxial(coord));

        /// <summary>
        /// 将基于轴向坐标的六边形转换为其在二维空间中的中心位置
        /// </summary>
        /// <param name="coord">六边形的轴向坐标</param>
        /// <returns>二维空间中的中心位置，格式为(X, Y)</returns>
        public (float X, float Y) ToPoint2(Axial coord)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                case HexagonalGridType.PointyEven:
                {
                    var x = Side * (Sqrt3 * coord.Q + Sqrt3 / 2 * coord.R);
                    var y = Side * (3.0f / 2.0f * coord.R);
                    return (x, y);
                }
                case HexagonalGridType.FlatOdd:
                case HexagonalGridType.FlatEven:
                {
                    var x = Side * (3.0f / 2.0f * coord.Q);
                    var y = Side * (Sqrt3 / 2 * coord.Q + Sqrt3 * coord.R);
                    return (x, y);
                }
                default:
                    throw new HexagonalException($"{nameof(ToPoint2)} failed with unexpected {nameof(Type)}", this, (nameof(coord), coord));
            }
        }

        /// <summary>
        /// Convert hex based on its cubic coordinate to it center position in 2d space
        /// </summary>
        public (float X, float Y) ToPoint2(Cubic coord) => ToPoint2(ToAxial(coord));

        #endregion

        #region GetCornerPoint

        /// <summary>
        /// Returns corner point in 2d space of given coordinate
        /// </summary>
        public (float X, float Y) GetCornerPoint(Offset coord, int edge)
            => GetCornerPoint(coord, edge, ToPoint2);

        /// <summary>
        /// Returns corner point in 2d space of given coordinate
        /// </summary>
        public (float X, float Y) GetCornerPoint(Axial coord, int edge)
            => GetCornerPoint(coord, edge, ToPoint2);

        /// <summary>
        /// Returns corner point in 2d space of given coordinate
        /// </summary>
        public (float X, float Y) GetCornerPoint(Cubic coord, int edge)
            => GetCornerPoint(coord, edge, ToPoint2);

        /// <summary>
        /// 计算指定六边形坐标的指定边缘的角点在2D空间中的坐标
        /// </summary>
        /// <typeparam name="T">六边形坐标类型（如Offset、Axial或Cubic）</typeparam>
        /// <param name="coord">六边形的坐标</param>
        /// <param name="edge">边缘索引（0-5），表示六边形的六条边之一，会被自动归一化到有效范围</param>
        /// <param name="toPoint">将六边形坐标转换为其中心在2D空间中坐标的函数</param>
        /// <returns>角点在2D空间中的（X,Y）坐标</returns>
        /// <remarks>
        /// 方法会先将边缘索引归一化（确保在 0-5 范围内），然后根据网格类型（Pointy或Flat）调整角度，
        /// 最终结合六边形中心坐标、外接圆半径和三角函数计算出角点位置。
        /// </remarks>
        private (float X, float Y) GetCornerPoint<T>(T coord, int edge, Func<T, (float X, float Y)> toPoint)
            where T : struct
        {
            edge = NormalizeIndex(edge);
            var angleDeg = 60 * edge;
            if (Type is HexagonalGridType.PointyEven or HexagonalGridType.PointyOdd)
                angleDeg -= 30;

            var center = toPoint(coord);
            var angleRad = PI / 180 * angleDeg;
            var x = (float) (center.X + DescribedRadius * Cos(angleRad));
            var y = (float) (center.Y + DescribedRadius * Sin(angleRad));
            return (x, y);
        }

        #endregion

        #region GetNeighbor

        /// <summary>
        /// 获取指定偏移坐标的六边形在特定方向上的相邻六边形坐标
        /// </summary>
        /// <param name="coord">源六边形的偏移坐标</param>
        /// <param name="neighborIndex">邻居方向索引（0-5），表示六个可能的相邻方向</param>
        /// <returns>指定方向上相邻六边形的偏移坐标</returns>
        /// <remarks>
        /// 方法会先通过<see cref="GetNeighborsOffsets(Offset)"/>获取该坐标对应的邻居偏移量表，
        /// 然后将邻居索引通过<see cref="NormalizeIndex(int)"/>归一化（确保在0-5范围内），
        /// 最后将源坐标与对应方向的偏移量相加，得到相邻六边形的坐标。
        /// 六个方向按照顺时针顺序排列，方向定义取决于六边形网格的类型（Pointy或Flat）。
        /// </remarks>
        public Offset GetNeighbor(Offset coord, int neighborIndex)
            => coord + GetNeighborsOffsets(coord)[NormalizeIndex(neighborIndex)];
        
        /// <summary>
        /// 获取指定轴向坐标的六边形在特定方向上的相邻六边形坐标
        /// </summary>
        /// <param name="coord">源六边形的轴向坐标</param>
        /// <param name="neighborIndex">邻居方向索引（0-5），表示六个可能的相邻方向</param>
        /// <returns>指定方向上相邻六边形的轴向坐标</returns>
        /// <remarks>
        /// 方法会先将邻居索引归一化（确保在 0-5 范围内），然后通过从预定义的轴向邻居偏移量数组中获取对应方向的偏移量，
        /// 最后将该偏移量与源坐标相加，得到相邻六边形的坐标。
        /// 六个方向按照顺时针顺序排列，方向定义取决于六边形网格的类型（Pointy或Flat）。
        /// </remarks>
        public Axial GetNeighbor(Axial coord, int neighborIndex)
            => coord + _sAxialNeighbors[NormalizeIndex(neighborIndex)];

        /// <summary>
        /// Returns the neighbor at the specified index.
        /// </summary>
        public Cubic GetNeighbor(Cubic coord, int neighborIndex)
            => coord + _sCubicNeighbors[NormalizeIndex(neighborIndex)];

        #endregion

        #region GetNeighbors

        /// <summary>
        /// 获取指定偏移坐标的六边形的所有相邻六边形坐标
        /// </summary>
        /// <param name="hex">源六边形的偏移坐标</param>
        /// <returns>包含六个相邻六边形偏移坐标的可枚举集合，按顺时针顺序排列</returns>
        /// <remarks>
        /// 方法通过调用<see cref="GetNeighborsOffsets(Offset)"/>获取适用于当前网格类型和坐标奇偶性的邻居偏移量表，
        /// 然后遍历每个偏移量并与源坐标相加，生成并返回所有相邻六边形的坐标。
        /// 
        /// 邻居偏移量的选择逻辑基于网格类型和坐标的奇偶性：
        /// <list type="bullet">
        ///   <item><b>PointyOdd网格类型</b>：根据Y坐标的奇偶性选择<see cref="_sPointyOddNeighbors"/>或<see cref="_sPointyEvenNeighbors"/></item>
        ///   <item><b>PointyEven网格类型</b>：根据Y坐标的奇偶性选择<see cref="_sPointyEvenNeighbors"/>或<see cref="_sPointyOddNeighbors"/></item>
        ///   <item><b>FlatOdd网格类型</b>：根据X坐标的奇偶性选择<see cref="_sFlatOddNeighbors"/>或<see cref="_sFlatEvenNeighbors"/></item>
        ///   <item><b>FlatEven网格类型</b>：根据X坐标的奇偶性选择<see cref="_sFlatEvenNeighbors"/>或<see cref="_sFlatOddNeighbors"/></item>
        /// </list>
        /// 
        /// 六个邻居按照顺时针顺序排列，具体方向定义取决于所使用的偏移量表。
        /// 该方法使用yield return实现懒加载，仅在需要时才计算邻居坐标。
        /// </remarks>
        public IEnumerable<Offset> GetNeighbors(Offset hex)
        {
            foreach (var offset in GetNeighborsOffsets(hex))
                yield return offset + hex;
        }
        
        /// <summary>
        /// 获取指定轴向坐标的六边形的所有相邻六边形坐标
        /// </summary>
        /// <param name="hex">源六边形的轴向坐标</param>
        /// <returns>包含六个相邻六边形轴向坐标的可枚举集合，按顺时针顺序排列</returns>
        /// <remarks>
        /// 方法通过遍历预定义的轴向邻居偏移量数组<see cref="_sAxialNeighbors"/>，
        /// 将每个偏移量与源坐标相加，生成并返回所有相邻六边形的坐标。
        /// 六个邻居按照顺时针顺序排列，方向定义为：
        /// <list type="bullet">
        ///   <item>索引0：东 (+1, 0)</item>
        ///   <item>索引1：东北 (+1, -1)</item>
        ///   <item>索引2：西北 (0, -1)</item>
        ///   <item>索引3：西 (-1, 0)</item>
        ///   <item>索引4：西南 (-1, +1)</item>
        ///   <item>索引5：东南 (0, +1)</item>
        /// </list>
        /// 这些方向与<see cref="_sAxialNeighbors"/>中定义的偏移量一一对应。
        /// </remarks>
        public IEnumerable<Axial> GetNeighbors(Axial hex)
        {
            foreach (var offset in _sAxialNeighbors)
                yield return offset + hex;
        }

        /// <summary>
        /// Return all neighbors of the hex
        /// </summary>
        public IEnumerable<Cubic> GetNeighbors(Cubic hex)
        {
            foreach (var offset in _sCubicNeighbors)
                yield return offset + hex;
        }

        #endregion

        #region IsNeighbors

        /// <summary>
        /// Checks whether the two hexes are neighbors or no
        /// </summary>
        public bool IsNeighbors(Offset coord1, Offset coord2)
            => IsNeighbors(coord1, coord2, GetNeighbor);

        /// <summary>
        /// Checks whether the two hexes are neighbors or no
        /// </summary>
        public bool IsNeighbors(Axial coord1, Axial coord2)
        {
            Func<Axial, int, Axial> getNeighbor = GetNeighbor;
            return IsNeighbors(coord1, coord2, getNeighbor);
        }

        /// <summary>
        /// Checks whether the two hexes are neighbors or no
        /// </summary>
        public bool IsNeighbors(Cubic coord1, Cubic coord2)
            => IsNeighbors(coord1, coord2, GetNeighbor);

        /// <summary>
        /// 检查两个六边形坐标是否为相邻关系
        /// </summary>
        /// <typeparam name="T">六边形坐标类型（如Offset、Axial、Cubic等），需实现相等性比较接口</typeparam>
        /// <param name="coord1">第一个六边形坐标</param>
        /// <param name="coord2">第二个六边形坐标</param>
        /// <param name="getNeighbor">获取邻居坐标的函数，参数为源坐标和邻居索引（0-5），返回对应方向的邻居坐标</param>
        /// <returns>若两个坐标为相邻六边形则返回 true，否则返回 false</returns>
        /// <remarks>
        /// 方法通过循环检查第一个坐标的所有6个邻居（使用 <paramref name="getNeighbor"/> 函数获取），
        /// 若其中任何一个邻居与第二个坐标相等，则判定为相邻关系。
        /// </remarks>
        private bool IsNeighbors<T>(T coord1, T coord2, in Func<T, int, T> getNeighbor)
            where T : struct, IEqualityComparer<T>
        {
            for (var neighborIndex = 0; neighborIndex < EDGES_COUNT; neighborIndex++)
            {
                var neighbor = getNeighbor(coord1, neighborIndex);
                if (neighbor.Equals(coord2))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region GetNeighborsRing

        /// <summary>
        /// Returns a ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Offset> GetNeighborsRing(Offset center, int radius) => GetNeighborsRing(center, radius, GetNeighbor);

        /// <summary>
        /// Returns a ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Axial> GetNeighborsRing(Axial center, int radius) => GetNeighborsRing(center, radius, GetNeighbor);

        /// <summary>
        /// Returns a ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Cubic> GetNeighborsRing(Cubic center, int radius) => GetNeighborsRing(center, radius, GetNeighbor);

        /// <summary>
        /// 生成以指定中心为原点、指定半径的六边形环形邻居集合
        /// </summary>
        /// <typeparam name="T">六边形坐标类型（如Offset、Axial或Cubic），必须为值类型</typeparam>
        /// <param name="center">环形的中心六边形坐标</param>
        /// <param name="radius">环形半径（单位：六边形数量），值为0时仅返回中心坐标</param>
        /// <param name="getNeighbor">获取邻居坐标的函数，参数为源坐标和邻居方向索引（0-5），返回对应方向的邻居坐标</param>
        /// <returns>环形上所有六边形坐标的可枚举集合，顺序为顺时针方向</returns>
        /// <remarks>
        /// 算法核心步骤：
        /// 1. 若半径为0，直接返回中心坐标
        /// 2. 否则，将中心沿方向索引4移动<paramref name="radius"/>步，到达环形起点
        /// 3. 依次沿6个方向（0-5）各移动<paramref name="radius"/>步，收集途经的坐标形成闭合环形
        /// </remarks>
        private static IEnumerable<T> GetNeighborsRing<T>(T center, int radius, Func<T, int, T> getNeighbor)
            where T : struct
        {
            if (radius == 0)
            {
                yield return center;
                yield break;
            }

            for (var i = 0; i < radius; i++)
                center = getNeighbor(center, 4);

            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < radius; j++)
                {
                    yield return center;
                    center = getNeighbor(center, i);
                }
            }
        }

        #endregion

        #region GetNeighborsAround

        /// <summary>
        /// Returns an all hexes in the ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Offset> GetNeighborsAround(Offset center, int radius) => GetNeighborsAround(center, radius, GetNeighborsRing);

        /// <summary>
        /// Returns an all hexes in the ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Axial> GetNeighborsAround(Axial center, int radius) => GetNeighborsAround(center, radius, GetNeighborsRing);

        /// <summary>
        /// Returns an all hexes in the ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Cubic> GetNeighborsAround(Cubic center, int radius) => GetNeighborsAround(center, radius, GetNeighborsRing);

        /// <summary>
        /// 生成以指定中心为原点、指定半径范围内的所有六边形区域邻居集合（包含多层环形）
        /// </summary>
        /// <typeparam name="T">六边形坐标类型（如Offset、Axial或Cubic），必须为值类型</typeparam>
        /// <param name="center">区域的中心六边形坐标</param>
        /// <param name="radius">区域半径（单位：六边形数量），定义包含的环形层数（0表示仅中心，1表示中心+1层环，以此类推）</param>
        /// <param name="getNeighborRing">获取指定半径环形邻居的函数，参数为中心坐标和环形半径，返回该环形上的所有坐标</param>
        /// <returns>区域内所有六边形坐标的可枚举集合，按半径从小到大顺序排列</returns>
        /// <remarks>
        /// 实现逻辑：通过循环获取从半径0到<paramref name="radius"/>-1的所有环形邻居集合，
        /// 并将这些环形坐标按半径顺序合并为一个连续的区域集合。例如，radius=2时将包含半径0（中心）、半径1（内环）的所有坐标。
        /// </remarks>
        private static IEnumerable<T> GetNeighborsAround<T>(T center, int radius, Func<T, int, IEnumerable<T>> getNeighborRing)
            where T : struct
        {
            for (var i = 0; i < radius; i++)
            {
                foreach (var hex in getNeighborRing(center, i))
                    yield return hex;
            }
        }

        #endregion

        #region GetNeighborIndex

        /// <summary>
        /// Returns the bypass index to the specified neighbor
        /// </summary>
        public byte GetNeighborIndex(Offset center, Offset neighbor) => GetNeighborIndex(center, neighbor, GetNeighbors);

        /// <summary>
        /// Returns the bypass index to the specified neighbor
        /// </summary>
        public byte GetNeighborIndex(Axial center, Axial neighbor) => GetNeighborIndex(center, neighbor, GetNeighbors);

        /// <summary>
        /// Returns the bypass index to the specified neighbor
        /// </summary>
        public byte GetNeighborIndex(Cubic center, Cubic neighbor) => GetNeighborIndex(center, neighbor, GetNeighbors);

        /// <summary>
        /// 获取指定邻居相对于中心六边形的方向索引（0-5）
        /// </summary>
        /// <typeparam name="T">六边形坐标类型（如Offset、Axial或Cubic），需为值类型且实现相等性比较接口</typeparam>
        /// <param name="center">中心六边形坐标</param>
        /// <param name="neighbor">需要查找索引的邻居六边形坐标</param>
        /// <param name="getNeighbors">获取中心所有邻居坐标的函数，返回按方向索引顺序排列的邻居集合</param>
        /// <returns>邻居相对于中心的方向索引（0-5），对应六边形的六个方向</returns>
        /// <exception cref="HexagonalException">当<paramref name="neighbor"/>不是<paramref name="center"/>的邻居时抛出</exception>
        /// <remarks>
        /// 实现逻辑：通过<paramref name="getNeighbors"/>获取中心的所有邻居集合，按顺序遍历并与<paramref name="neighbor"/>比较，
        /// 返回第一个匹配项的索引。索引顺序与六边形的六个方向一一对应（0-5）。
        /// </remarks>
        private byte GetNeighborIndex<T>(T center, T neighbor, Func<T, IEnumerable<T>> getNeighbors)
            where T : struct, IEqualityComparer<T>
        {
            byte neighborIndex = 0;
            foreach (var current in getNeighbors(center))
            {
                if (current.Equals(neighbor))
                    return neighborIndex;

                neighborIndex++;
            }

            throw new HexagonalException($"Can't find bypass index", this, (nameof(center), center), (nameof(neighbor), neighbor));
        }

        #endregion

        #region GetPointBetweenTwoNeighbours

        /// <summary>
        /// Returns the midpoint of the boundary segment of two neighbors
        /// </summary>
        public (float x, float y) GetPointBetweenTwoNeighbours(Offset coord1, Offset coord2) => GetPointBetweenTwoNeighbours(coord1, coord2, IsNeighbors, ToPoint2);

        /// <summary>
        /// Returns the midpoint of the boundary segment of two neighbors
        /// </summary>
        public (float x, float y) GetPointBetweenTwoNeighbours(Axial coord1, Axial coord2) => GetPointBetweenTwoNeighbours(coord1, coord2, IsNeighbors, ToPoint2);

        /// <summary>
        /// Returns the midpoint of the boundary segment of two neighbors
        /// </summary>
        public (float x, float y) GetPointBetweenTwoNeighbours(Cubic coord1, Cubic coord2) => GetPointBetweenTwoNeighbours(coord1, coord2, IsNeighbors, ToPoint2);

        /// <summary>
        /// 计算两个相邻六边形边界线段的中点坐标
        /// </summary>
        /// <typeparam name="T">六边形坐标类型（如Offset、Axial或Cubic），必须为值类型</typeparam>
        /// <param name="coord1">第一个六边形坐标（邻居之一）</param>
        /// <param name="coord2">第二个六边形坐标（邻居之二）</param>
        /// <param name="isNeighbor">判断两个坐标是否为相邻关系的函数</param>
        /// <param name="toPoint">将六边形坐标转换为其中心在2D空间中坐标的函数</param>
        /// <returns>两个相邻六边形边界线段的中点（X,Y）坐标</returns>
        /// <exception cref="HexagonalException">当<paramref name="coord1"/>与<paramref name="coord2"/>不是相邻关系时抛出</exception>
        /// <remarks>
        /// 实现逻辑：
        /// 1. 首先通过<paramref name="isNeighbor"/>验证两个坐标是否为邻居
        /// 2. 若不相邻则抛出异常，否则通过<paramref name="toPoint"/>获取两个六边形的中心坐标
        /// 3. 计算两个中心坐标的平均值，得到边界线段的中点
        /// </remarks>
        private (float x, float y) GetPointBetweenTwoNeighbours<T>(T coord1, T coord2, Func<T, T, bool> isNeighbor, Func<T, (float X, float Y)> toPoint)
            where T : struct
        {
            if (!isNeighbor(coord1, coord2))
            {
                throw new HexagonalException($"Can't calculate point between not neighbors", this, (nameof(coord1), coord1), (nameof(coord2), coord2));
            }

            var c1 = toPoint(coord1);
            var c2 = toPoint(coord2);

            return ((c1.X + c2.X) / 2, (c1.Y + c2.Y) / 2);
        }

        #endregion

        #region CubeDistance

        /// <summary>
        /// Manhattan distance between two hexes
        /// </summary>
        public int CubeDistance(Offset h1, Offset h2)
        {
            var cubicFrom = ToCubic(h1);
            var cubicTo = ToCubic(h2);
            return CubeDistance(cubicFrom, cubicTo);
        }

        /// <summary>
        /// Manhattan distance between two hexes
        /// </summary>
        public int CubeDistance(Axial h1, Axial h2)
        {
            var cubicFrom = ToCubic(h1);
            var cubicTo = ToCubic(h2);
            return CubeDistance(cubicFrom, cubicTo);
        }

        /// <summary>
        /// 计算两个立方体坐标（Cubic）之间的曼哈顿距离（Manhattan distance）
        /// </summary>
        /// <param name="h1">第一个六边形的立方体坐标</param>
        /// <param name="h2">第二个六边形的立方体坐标</param>
        /// <returns>两个六边形坐标之间的曼哈顿距离值，范围为非负整数</returns>
        /// <remarks>
        /// 立方体坐标系统下的曼哈顿距离计算公式为：( |x1-x2| + |y1-y2| + |z1-z2| ) / 2
        /// 该公式利用了立方体坐标的性质，其中x + y + z = 0，因此总和必为偶数，结果总是整数
        /// 距离表示从一个六边形移动到另一个六边形所需的最少相邻六边形步数
        /// </remarks>
        public static int CubeDistance(Cubic h1, Cubic h2)
            => (Abs(h1.X - h2.X) + Abs(h1.Y - h2.Y) + Abs(h1.Z - h2.Z)) / 2;

        #endregion

        #region Neighbors

        /// <summary>
        /// 获取指定六边形坐标的所有邻居偏移量列表
        /// </summary>
        /// <param name="coord">六边形的Offset坐标</param>
        /// <returns>包含6个邻居相对偏移量的只读列表，偏移量顺序对应六边形的6个方向</returns>
        /// <remarks>
        /// 根据网格类型（<see cref="Type"/>）和坐标的奇偶性选择对应的邻居偏移量列表：
        /// <list type="bullet">
        ///   <item>PointyOdd/PointyEven：根据 <paramref name="coord"/> 的Y值奇偶性选择 <see cref="_sPointyOddNeighbors"/> 或 <see cref="_sPointyEvenNeighbors"/></item>
        ///   <item>FlatOdd/FlatEven：根据 <paramref name="coord"/> 的X值奇偶性选择 <see cref="_sFlatOddNeighbors"/> 或 <see cref="_sFlatEvenNeighbors"/></item>
        /// </list>
        /// </remarks>
        private IReadOnlyList<Offset> GetNeighborsOffsets(Offset coord)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                    return Abs(coord.Y % 2) == 0 ? _sPointyEvenNeighbors : _sPointyOddNeighbors;
                case HexagonalGridType.PointyEven:
                    return Abs(coord.Y % 2) == 1 ? _sPointyEvenNeighbors : _sPointyOddNeighbors;
                case HexagonalGridType.FlatOdd:
                    return Abs(coord.X % 2) == 0 ? _sFlatEvenNeighbors : _sFlatOddNeighbors;
                case HexagonalGridType.FlatEven:
                    return Abs(coord.X % 2) == 1 ? _sFlatEvenNeighbors : _sFlatOddNeighbors;
                default:
                    throw new HexagonalException($"{nameof(GetNeighborsOffsets)} failed with unexpected {nameof(Type)}", this, (nameof(coord), coord));
            }
        }

        private static readonly List<Offset> _sPointyOddNeighbors =
        [
            new Offset(+1, 0), new Offset(+1, -1), new Offset(0, -1),
            new Offset(-1, 0), new Offset(0, +1), new Offset(+1, +1)
        ];

        private static readonly List<Offset> _sPointyEvenNeighbors =
        [
            new Offset(+1, 0), new Offset(0, -1), new Offset(-1, -1),
            new Offset(-1, 0), new Offset(-1, +1), new Offset(0, +1)
        ];

        private static readonly List<Offset> _sFlatOddNeighbors =
        [
            new Offset(+1, +1), new Offset(+1, 0), new Offset(0, -1),
            new Offset(-1, 0), new Offset(-1, +1), new Offset(0, +1)
        ];

        private static readonly List<Offset> _sFlatEvenNeighbors =
        [
            new Offset(+1, 0), new Offset(+1, -1), new Offset(0, -1),
            new Offset(-1, -1), new Offset(-1, 0), new Offset(0, +1)
        ];

        private static readonly List<Axial> _sAxialNeighbors =
        [
            new Axial(+1, 0), new Axial(+1, -1), new Axial(0, -1),
            new Axial(-1, 0), new Axial(-1, +1), new Axial(0, +1)
        ];

        private static readonly List<Cubic> _sCubicNeighbors =
        [
            new Cubic(+1, -1, 0), new Cubic(+1, 0, -1), new Cubic(0, +1, -1),
            new Cubic(-1, +1, 0), new Cubic(-1, 0, +1), new Cubic(0, -1, +1)
        ];

        #endregion

        private static int NormalizeIndex(int index)
        {
            index %= EDGES_COUNT;
            if (index < 0) index += EDGES_COUNT;

            return index;
        }
    }
}