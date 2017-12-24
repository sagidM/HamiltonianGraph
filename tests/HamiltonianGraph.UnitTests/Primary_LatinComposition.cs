using Microsoft.VisualStudio.TestTools.UnitTesting;
using HamiltonianGraph.GraphInputProvider;
using MatchingExtensions;
using HamiltonianGraph.Utils;

namespace HamiltonianGraph.UnitTests
{
    [TestClass]
    public class Primary_LatinComposition
    {
        [TestMethod]
        public void GetHamiltonianPathsTest1()
        {
            GetHamiltonianPathsTest(1);
        }

        [TestMethod]
        public void GetHamiltonianPathsTest2()
        {
            GetHamiltonianPathsTest(2);
        }

        [TestMethod]
        public void GetHamiltonianPathsTest3()
        {
            GetHamiltonianPathsTest(3);
        }

        [TestMethod]
        public void GetHamiltonianPathsTest4()
        {
            GetHamiltonianPathsTest(4);
        }

        [TestMethod]
        public void GetHamiltonianPathsTest5()
        {
            GetHamiltonianPathsTest(5);
        }

        [TestMethod]
        public void GetHamiltonianPathsTest6()
        {
            GetHamiltonianPathsTest(6);
        }

        [TestMethod]
        public void GetHamiltonianPathsTest7()
        {
            GetHamiltonianPathsTest(7);
        }

        [TestMethod]
        public void GetHamiltonianPathsTest8()
        {
            GetHamiltonianPathsTest(8);
        }

        [TestMethod]
        public void GetHamiltonianPathsTest9()
        {
            GetHamiltonianPathsTest(9);
        }

        private void GetHamiltonianPathsTest(int testNumber)
        {
            var matrix = AdjacencyMatrix.GetGraph(testNumber);
            var actualPaths = new LatinComposition(matrix.Weights).GetAllHamiltorianCycles();
            var expectedPaths = matrix.AllPaths;
            // due to there can be not only one shortest path,
            // we compare distance between actual and expected shortest paths
            int[] actualShortestPath = GraphUtil.ShortestRoute(matrix.Weights, actualPaths);
            var actualDistance = AdjacencyMatrix.PathDistance(actualShortestPath, matrix.Weights);
            var expectedDistance = matrix.ShortestPathDistance;

            Assert.IsTrue(expectedPaths.InnerSequencesEqualInSomeOrder(actualPaths));
            Assert.AreEqual(expectedDistance, actualDistance);
        }
    }
}
