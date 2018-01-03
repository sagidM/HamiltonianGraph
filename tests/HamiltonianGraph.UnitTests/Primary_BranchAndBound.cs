using HamiltonianGraph.GraphInputProvider;
using MatchingExtensions;
using Xunit;

namespace HamiltonianGraph.UnitTests
{
    public class Primary_BranchAndBound
    {
        [Fact]
        public void GetShortestHamiltonianPathTest1()
        {
            GetShortestHamiltonianPathTest(1);
        }

        [Fact]
        public void GetShortestHamiltonianPathTest2()
        {
            GetShortestHamiltonianPathTest(2);
        }

        [Fact]
        public void GetShortestHamiltonianPathTest3()
        {
            GetShortestHamiltonianPathTest(3);
        }

        [Fact]
        public void GetShortestHamiltonianPathTest4()
        {
            GetShortestHamiltonianPathTest(4);
        }

        [Fact]
        public void GetShortestHamiltonianPathTest5()
        {
            GetShortestHamiltonianPathTest(5);
        }

        [Fact]
        public void GetShortestHamiltonianPathTest6()
        {
            GetShortestHamiltonianPathTest(6);
        }

        [Fact]
        public void GetShortestHamiltonianPathTest7()
        {
            GetShortestHamiltonianPathTest(7);
        }

        [Fact]
        public void GetShortestHamiltonianPathTest8()
        {
            GetShortestHamiltonianPathTest(8);
        }

        [Fact]
        public void GetShortestHamiltonianPathTest9()
        {
            GetShortestHamiltonianPathTest(9);
        }

        private static void GetShortestHamiltonianPathTest(int testNumber)
        {
            var matrix = AdjacencyMatrix.GetGraph(testNumber);
            var path = new BranchAndBound(matrix.Weights).GetShortestHamiltonianCycle();
            path.AreUnique(1, path.Length-1);

            var actualDistance = AdjacencyMatrix.PathDistance(path, matrix.Weights);
            var expectedDistance = matrix.ShortestPathDistance;

            Assert.Equal(expectedDistance, actualDistance);
        }
    }

}
