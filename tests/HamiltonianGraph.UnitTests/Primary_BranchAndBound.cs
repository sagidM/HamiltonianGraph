using Microsoft.VisualStudio.TestTools.UnitTesting;
using HamiltonianGraph.GraphInputProvider;
using MatchingExtensions;

namespace HamiltonianGraph.UnitTests
{
    [TestClass]
    public class Primary_BranchAndBound
    {
        [TestMethod]
        public void GetShortestHamiltonianPathTest1()
        {
            GetShortestHamiltonianPathTest(1);
        }

        [TestMethod]
        public void GetShortestHamiltonianPathTest2()
        {
            GetShortestHamiltonianPathTest(2);
        }

        [TestMethod]
        public void GetShortestHamiltonianPathTest3()
        {
            GetShortestHamiltonianPathTest(3);
        }

        [TestMethod]
        public void GetShortestHamiltonianPathTest4()
        {
            GetShortestHamiltonianPathTest(4);
        }

        [TestMethod]
        public void GetShortestHamiltonianPathTest5()
        {
            GetShortestHamiltonianPathTest(5);
        }

        [TestMethod]
        public void GetShortestHamiltonianPathTest6()
        {
            GetShortestHamiltonianPathTest(6);
        }

        [TestMethod]
        public void GetShortestHamiltonianPathTest7()
        {
            GetShortestHamiltonianPathTest(7);
        }

        private static void GetShortestHamiltonianPathTest(int testNumber)
        {
            var matrix = AdjacencyMatrix.GetGraph(testNumber);
            var path = new BranchAndBound(matrix.Weights).GetShortestHamiltonianCycle();
            path.AreUnique(1, path.Length-1);

            var actualDistance = AdjacencyMatrix.PathDistance(path, matrix.Weights);
            var expectedDistance = matrix.ShortestPathDistance;

            Assert.AreEqual(expectedDistance, actualDistance);
        }
    }

}
