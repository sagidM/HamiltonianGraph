using System;
using System.Text;
using HamiltonianGraph.GraphInputProvider;
using Xunit;

namespace HamiltonianGraph.UnitTests
{
    public class Compare_LC_And_BaB
    {
        [Theory(DisplayName = "BaB vs LC")]
        [InlineData(2, 3, 1000)]
        [InlineData(2, 10, 1000)]
        [InlineData(3, 5, 1000)]
        [InlineData(3, 15, 1000)]
        [InlineData(4, 5, 1000)]
        [InlineData(4, 50, 1000)]
        [InlineData(5, 5, 1000)]
        [InlineData(5, 100, 1000)]
        [InlineData(6, 10, 100)]
        [InlineData(6, 100, 100)]
        [InlineData(7, 10, 100)]
        [InlineData(7, 100, 200)]
        [InlineData(8, 10, 100)]
        [InlineData(8, 100, 100)]
        public void Compare_BaB_And_LC1(int n, int maxRandomValue, int repeat)
        {
            for (int i = 0; i < repeat; i++)
            {
                var weights = GenerateRandomFullGraph(n, maxRandomValue);
                var lc = new LatinComposition(weights).GetShortestHamiltonianCycle();
                var bb = new BranchAndBound(weights).GetShortestHamiltonianCycle();

                var lcDistance = AdjacencyMatrix.PathDistance(lc, weights);
                var bbDistance = AdjacencyMatrix.PathDistance(bb, weights);

                var msg = lcDistance == bbDistance ? "" : $"\nLC distance: {lcDistance};\nB&B distance: {bbDistance}\n" + ToString(weights);
                Assert.True(lcDistance == bbDistance, msg);
            }
        }

        [Theory(DisplayName = "BaB vs LC [nullable]")]
        [InlineData(10, 10, 100, 0.5)]
        [InlineData(10, 100, 100, 0.4)]
        [InlineData(10, 100, 10, 0.7)]
        [InlineData(10, 1000000, 10, 0.8)]
        public void Compare_BaB_And_LC(int n, int maxRandomValue, int repeat, double possibilityOfNull)
        {
            for (int i = 0; i < repeat; i++)
            {
                var weights = GenerateRandomGraph(n, maxRandomValue, possibilityOfNull);
                var lc = new LatinComposition(weights).GetShortestHamiltonianCycle();
                var bb = new BranchAndBound(weights).GetShortestHamiltonianCycle();
                if (lc == null && bb == null) continue;
                //var s = "";
                //for (int j = 0; j < n; j++)
                //{
                //    var v = new string[n];
                //    for (int k = 0; k < n; k++)
                //    {
                //        v[k] = weights[j, k]?.ToString() ?? "-";
                //    }
                //    s += string.Join(" ", v) + "\n";
                //}
                //System.IO.File.WriteAllText("D:/1.txt", s);

                var lcDistance = AdjacencyMatrix.PathDistance(lc, weights);
                var bbDistance = AdjacencyMatrix.PathDistance(bb, weights);

                var msg = lcDistance == bbDistance ? "" : $"\nLC distance: {lcDistance};\nB&B distance: {bbDistance}\n" + ToString(weights);
                Assert.True(lcDistance == bbDistance, msg);
            }
        }

        internal static int?[,] GenerateRandomGraph(int n, int maxRandomValue, double possibilityOfNull)
        {
            var weights = new int?[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j && Random.NextDouble() <= possibilityOfNull)
                        weights[i, j] = Random.Next(n);
                }
            }
            return weights;
        }
        internal static int?[,] GenerateRandomFullGraph(int n, int maxRandomValue)
        {
            var weights = new int?[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                        weights[i, j] = Random.Next(n);
                }
            }
            return weights;
        }
        private static string ToString(int?[,] weights)
        {
            var sb = new StringBuilder();
            sb.Append(weights.GetLength(0));
            for (int i = 0; i < weights.GetLength(0); i++)
            {
                sb.AppendLine().Append(weights[i, 0]?.ToString() ?? "-");
                for (int j = 1; j < weights.GetLength(1); j++)
                {
                    sb.Append(' ').Append(weights[i, j]?.ToString() ?? "-");
                }
            }
            return sb.ToString();
        }

        private static readonly Random Random = new Random(41);
    }
}
