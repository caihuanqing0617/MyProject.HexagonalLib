using System;
using System.Collections.Generic;
using HexagonalLib.Coordinates;

namespace HexagonalLib;

public readonly partial struct HexagonalGrid
{
    /// <summary>
    /// 为指定类型的六边形集合生成组合网格
    /// </summary>
    /// <param name="hexes">要生成网格的六边形集合</param>
    /// <param name="subdivide">每个六边形的细分次数，控制网格精细度（值越大三角形数量越多）</param>
    /// <param name="setVertex">设置顶点数据的委托，参数为本地顶点索引和坐标</param>
    /// <param name="setIndex">设置三角形索引的委托，参数为本地索引和顶点索引</param>m>
    public void CreateMesh(IEnumerable<Offset> hexes, int subdivide, Action<int, (float X, float Y)> setVertex, Action<int, int> setIndex)
        => CreateMesh(hexes, subdivide, setVertex, setIndex, ToPoint2);

    /// <summary>
    /// 为指定类型的六边形集合生成组合网格
    /// </summary>
    /// <param name="hexes">要生成网格的六边形集合</param>
    /// <param name="subdivide">每个六边形的细分次数，控制网格精细度（值越大三角形数量越多）</param>
    /// <param name="setVertex">设置顶点数据的委托，参数为本地顶点索引和坐标</param>
    /// <param name="setIndex">设置三角形索引的委托，参数为本地索引和顶点索引</param>
    public void CreateMesh(IEnumerable<Axial> hexes, int subdivide, Action<int, (float X, float Y)> setVertex, Action<int, int> setIndex)
        => CreateMesh(hexes, subdivide, setVertex, setIndex, ToPoint2);

    /// <summary>
    /// 为指定类型的六边形集合生成组合网格
    /// </summary>
    /// <param name="hexes">要生成网格的六边形集合</param>
    /// <param name="subdivide">每个六边形的细分次数，控制网格精细度（值越大三角形数量越多）</param>
    /// <param name="setVertex">设置顶点数据的委托，参数为本地顶点索引和坐标</param>
    /// <param name="setIndex">设置三角形索引的委托，参数为本地索引和顶点索引</param>
    public void CreateMesh(IEnumerable<Cubic> hexes, int subdivide, Action<int, (float X, float Y)> setVertex, Action<int, int> setIndex)
        => CreateMesh(hexes, subdivide, setVertex, setIndex, ToPoint2);

    /// <summary>
    /// 为指定类型的六边形集合生成组合网格
    /// </summary>
    /// <typeparam name="T">六边形坐标类型（如Offset、Axial、Cubic等）</typeparam>
    /// <param name="hexes">要生成网格的六边形集合</param>
    /// <param name="subdivide">每个六边形的细分次数，控制网格精细度（值越大三角形数量越多）</param>
    /// <param name="setVertex">设置顶点数据的委托，参数为本地顶点索引和坐标</param>
    /// <param name="setIndex">设置三角形索引的委托，参数为本地索引和顶点索引</param>
    /// <param name="toPoint">将六边形坐标转换为世界空间中心坐标的函数</param>
    /// <remarks>
    /// 算法来源：http://www.voidinspace.com/2014/07/project-twa-part-1-generating-a-hexagonal-tile-and-its-triangular-grid/
    /// 该方法通过循环处理每个六边形，为其计算中心坐标后调用单六边形网格生成方法，
    /// 并通过本地委托自动偏移顶点索引和坐标，实现多六边形网格的组合生成。
    /// </remarks>
    private void CreateMesh<T>(IEnumerable<T> hexes, int subdivide, Action<int, (float X, float Y)> setVertex, Action<int, int> setIndex,
        Func<T, (float X, float Y)> toPoint)
    {
        // 全局顶点索引偏移量，用于跟踪当前已处理的顶点总数
        var vertex = 0;
        // 全局索引缓冲区偏移量，用于跟踪当前已处理的索引总数
        var index = 0;

        // 获取单个六边形的网格数据（顶点数和索引数），用于计算偏移量
        var data = GetMeshData(1, subdivide);

        // 遍历每个六边形，生成并组合其网格数据
        foreach (var hex in hexes)
        {
            // 记录当前六边形的顶点起始索引（基于全局偏移量）
            var localVertex = vertex;
            // 记录当前六边形的索引起始位置（基于全局偏移量）
            var localIndex = index;
            // 计算当前六边形在世界空间中的中心坐标
            var center = toPoint(hex);

            // 本地顶点设置委托：将单个六边形的局部顶点坐标转换为世界坐标
            // 并通过全局委托设置到对应位置（自动应用顶点索引偏移）
            void SetVertexLocal(int i, (float X, float Y) currentVertex)
            {
                var (x, y) = currentVertex;                  // 局部坐标（六边形中心为原点）
                var shifted = (x + center.X, y + center.Y);  // 叠加中心坐标转换为世界坐标
                setVertex(localVertex + i, shifted);         // 设置带全局偏移的顶点数据
            }

            // 本地索引设置委托：将单个六边形的局部索引转换为全局索引
            // （自动应用索引位置偏移和顶点索引偏移）
            void SetIndexLocal(int i, int currentIndex) => 
                setIndex(localIndex + i, localVertex + currentIndex);  // 双偏移确保索引指向正确顶点

            // 生成当前六边形的网格数据（使用本地委托处理坐标和索引偏移）
            CreateMesh(subdivide, SetVertexLocal, SetIndexLocal);

            // 更新全局偏移量：累加当前六边形的顶点数和索引数
            vertex += data.VerticesCount;
            index += data.IndicesCount;
        }

    }

    /// <summary>
    /// 生成单个六边形的网格数据（顶点和三角形索引）
    /// </summary>
    /// <param name="subdivide">六边形的细分次数，决定网格精细度（值越大生成的三角形数量越多）</param>
    /// <param name="setVertex">设置顶点坐标的委托，参数为顶点索引和（X,Y）坐标</param>
    /// <param name="setIndex">设置三角形索引的委托，参数为索引位置和顶点索引值</param>
    /// <remarks>
    /// 算法来源：http://www.voidinspace.com/2014/07/project-twa-part-1-generating-a-hexagonal-tile-and-its-triangular-grid/
    /// 核心逻辑：通过行列迭代生成六边形网格的顶点坐标，再根据顶点位置关系构建三角形索引，
    /// 最终通过委托将顶点和索引数据输出。
    /// </remarks>
    private void CreateMesh(int subdivide, Action<int, (float X, float Y)> setVertex, Action<int, int> setIndex)
    {
        // 坐标计算常量初始化
        // 六边形外接圆半径 = 内切圆半径 / cos(30°)（几何关系推导）
        var radius = InscribedRadius / (float)Math.Cos(Math.PI / 6);
        var sin60 = (float)Math.Sin(Math.PI / 3);       // 60°正弦值（用于x坐标计算）
        var invTan60 = 1.0f / (float)Math.Tan(Math.PI / 3.0f); // 60°余切值（用于z坐标计算）
        var rdq = radius / subdivide;                   // 细分步长：每个细分层级的单位距离

        // 顶点和索引计数器
        var verticesIndex = 0;  // 当前顶点索引（从0开始累加）
        var indicesIndex = 0;   // 当前索引位置（从0开始累加）

        // 索引计算辅助变量
        var currentNumPoints = 0;   // 累计生成的顶点总数（用于索引边界检查）
        var prevRowNumPoints = 0;   // 上一列的顶点数量（用于计算右侧三角形索引偏移）

        // 列范围初始化
        var npCol0 = 2 * subdivide + 1;  // 中心列（itC=0）的顶点数量
        var colMin = -subdivide;         // 最小列索引（从负方向细分值开始）
        var colMax = subdivide;          // 最大列索引（到正方向细分值结束）

        // 生成网格主流程：按列迭代（从最左侧列到最右侧列）
        for (var itC = colMin; itC <= colMax; itC++)
        {
            // 计算当前列所有顶点的x坐标（列内x值相同）
            // sin60 * rdq 是列间距基础值，乘以列索引itC得到实际x偏移
            var x = sin60 * rdq * itC;

            // 计算当前列的顶点数量：从中心列开始向两侧递减（每侧减少1个顶点）
            var npColI = npCol0 - Math.Abs(itC);

            // 计算当前列的行索引范围
            var rowMin = -subdivide;       // 初始行最小值
            if (itC < 0) rowMin += Math.Abs(itC);  // 左侧列（itC<0）行范围上移，避免顶点重叠

            var rowMax = rowMin + npColI - 1;  // 行最大值 = 行最小值 + 顶点数量 - 1

            currentNumPoints += npColI;  // 累加当前列顶点数，用于后续索引有效性检查

            // 按行迭代生成当前列的所有顶点（从行最小值到行最大值）
            for (var itR = rowMin; itR <= rowMax; itR++)
            {
                // 计算当前顶点的z坐标
                // invTan60 * x 是列偏移基础值，rdq * itR 是行偏移量
                var z = invTan60 * x + rdq * itR;
                // 设置顶点坐标：将局部坐标（x,z）旋转到第一个邻居的角度方向
                setVertex(verticesIndex, (x, z).Rotate(AngleToFirstNeighbor));

                // 生成三角形索引（仅当当前顶点不是最后一个顶点时）
                if (verticesIndex < (currentNumPoints - 1))
                {
                    // 生成左侧三角形（当前列与右侧列顶点组成的三角形）
                    if (itC >= colMin && itC < colMax)
                    {
                        // 左侧填充偏移：左侧列（itC<0）需要额外偏移1个顶点
                        var padLeft = itC < 0 ? 1 : 0;
                        // 三角形顶点顺序：[当前顶点右侧列对应顶点, 当前顶点下一个顶点, 当前顶点]
                        setIndex(indicesIndex++, verticesIndex + npColI + padLeft);
                        setIndex(indicesIndex++, verticesIndex + 1);
                        setIndex(indicesIndex++, verticesIndex);
                    }

                    // 生成右侧三角形（当前列与左侧列顶点组成的三角形）
                    if (itC > colMin && itC <= colMax)
                    {
                        // 右侧填充偏移：右侧列（itC>0）需要额外偏移1个顶点
                        var padRight = itC > 0 ? 1 : 0;
                        // 三角形顶点顺序：[当前顶点左侧列对应顶点, 当前顶点, 当前顶点下一个顶点]
                        setIndex(indicesIndex++, verticesIndex - prevRowNumPoints + padRight);
                        setIndex(indicesIndex++, verticesIndex);
                        setIndex(indicesIndex++, verticesIndex + 1);
                    }
                }

                verticesIndex++;  // 移动到下一个顶点
            }

            prevRowNumPoints = npColI;  // 记录当前列顶点数，用于下一列的右侧三角形计算
        }
    }
    
    /// <summary>
    /// 计算生成六边形网格所需的顶点总数和索引总数
    /// </summary>
    /// <param name="hexesCount">网格中包含的六边形数量</param>
    /// <param name="subdivide">每个六边形的细分次数，用于控制网格的精细程度（值越大网格越精细）</param>
    /// <returns>包含两个整数的元组，其中：
    /// <para>VerticesCount: 生成网格所需的总顶点数量</para>
    /// <para>IndicesCount: 生成网格所需的总索引数量</para>
    /// </returns>
    private (int VerticesCount, int IndicesCount) GetMeshData(int hexesCount, int subdivide)
    {
        // 计算所需顶点总数
        // 顶点总数公式: 1 + Σ (i * 6)，其中i从1到subdivide（S表示细分次数）
        var numVertices = 1;  // 初始顶点数为1（六边形中心顶点）

        // 计算所需索引总数
        // 索引总数公式: Σ (36i - 18)，其中i从1到subdivide
        var numIndices = 0;   // 初始索引数为0

        // 循环累加各细分层级的顶点和索引数量（i表示当前细分层级）
        for (int i = 1; i <= subdivide; i++)
        {
            numVertices += i * 6;  // 累加第i层级的顶点数（每条边i个顶点，共6条边）
            numIndices += 36 * i - 18;  // 累加第i层级的索引数（每个三角形3个索引，36i-18对应12i-6个三角形）
        }

        // 返回总顶点数和总索引数（乘以六边形数量得到整体网格数据量）
        return (numVertices * hexesCount, numIndices * hexesCount);
    }
}