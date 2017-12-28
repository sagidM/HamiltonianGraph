using System;
using System.Text;
using HamiltonianGraph.GraphInputProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HamiltonianGraph.UnitTests
{
    [TestClass]
    public class Compare_LC_And_BaB
    {
        [TestMethod]
        public void BaB_And_LC_2_3()
        {
            Compare_BaB_And_LC(n: 2, maxRandomValue: 3, repeat: 1000);
        }

        [TestMethod]
        public void BaB_And_LC_2_10()
        {
            Compare_BaB_And_LC(n: 2, maxRandomValue: 10, repeat: 1000);
        }

        [TestMethod]
        public void BaB_And_LC_3_5()
        {
            Compare_BaB_And_LC(n: 3, maxRandomValue: 5, repeat: 1000);
        }

        [TestMethod]
        public void BaB_And_LC_3_15()
        {
            Compare_BaB_And_LC(n: 3, maxRandomValue: 15, repeat: 1000);
        }

        [TestMethod]
        public void BaB_And_LC_4_5()
        {
            Compare_BaB_And_LC(n: 4, maxRandomValue: 5, repeat: 1000);
        }

        [TestMethod]
        public void BaB_And_LC_4_50()
        {
            Compare_BaB_And_LC(n: 4, maxRandomValue: 50, repeat: 1000);
        }

        [TestMethod]
        public void BaB_And_LC_5_5()
        {
            Compare_BaB_And_LC(n: 5, maxRandomValue: 5, repeat: 1000);
        }

        [TestMethod]
        public void BaB_And_LC_5_100()
        {
            Compare_BaB_And_LC(n: 5, maxRandomValue: 100, repeat: 1000);
        }

        [TestMethod]
        public void BaB_And_LC_6_10()
        {
            Compare_BaB_And_LC(n: 6, maxRandomValue: 10, repeat: 100);
        }

        [TestMethod]
        public void BaB_And_LC_6_100()
        {
            Compare_BaB_And_LC(n: 6, maxRandomValue: 100, repeat: 100);
        }

        [TestMethod]
        public void BaB_And_LC_7_10()
        {
            Compare_BaB_And_LC(n: 7, maxRandomValue: 10, repeat: 100);
        }

        [TestMethod]
        public void BaB_And_LC_7_100()
        {
            Compare_BaB_And_LC(n: 7, maxRandomValue: 100, repeat: 200);
        }

        [TestMethod]
        public void BaB_And_LC_8_10()
        {
            Compare_BaB_And_LC(n: 8, maxRandomValue: 10, repeat: 100);
        }

        [TestMethod]
        public void BaB_And_LC_8_100()
        {
            Compare_BaB_And_LC(n: 8, maxRandomValue: 100, repeat: 100);
        }


        private static void Compare_BaB_And_LC(int n, int maxRandomValue, int repeat)
        {
            for (int i = 0; i < repeat; i++)
            {
                var weights = GenerateRandomFullGraph(n, maxRandomValue);
                var lc = new LatinComposition(weights).GetShortestHamiltonianCycle();
                var bb = new BranchAndBound(weights).GetShortestHamiltonianCycle();

                var lcDistance = AdjacencyMatrix.PathDistance(lc, weights);
                var bbDistance = AdjacencyMatrix.PathDistance(bb, weights);

                var msg = lcDistance == bbDistance ? "" : "\nLC is expected;\nB&B is actual\n" + ToString(weights);
                Assert.AreEqual(lcDistance, bbDistance, msg);
            }
        }

        [TestMethod]
        public void CompareWithNull_10_10_05()
        {
            Compare_BaB_And_LC(n: 10, maxRandomValue: 10, repeat: 100, possibilityOfNull: 0.5);
        }

        [TestMethod]
        public void CompareWithNull_10_100_04()
        {
            Compare_BaB_And_LC(n: 10, maxRandomValue: 100, repeat: 100, possibilityOfNull: 0.4);
        }

        [TestMethod]
        public void CompareWithNull_10_100_07()
        {
            Compare_BaB_And_LC(n: 10, maxRandomValue: 100, repeat: 10, possibilityOfNull: 0.7);
        }

        private static void Compare_BaB_And_LC(int n, int maxRandomValue, int repeat, double possibilityOfNull)
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

                var msg = lcDistance == bbDistance ? "" : "\nLC is expected;\nB&B is actual\n" + ToString(weights);
                Assert.AreEqual(lcDistance, bbDistance, msg);
            }
        }

        internal static int?[,] GenerateRandomGraph(int n, int maxRandomValue, double possibilityOfNull)
        {
            var weights = new int?[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j || Random.NextDouble() > possibilityOfNull) continue;
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
                    if (i == j) continue;
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
