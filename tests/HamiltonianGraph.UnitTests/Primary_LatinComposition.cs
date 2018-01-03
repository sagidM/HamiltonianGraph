using HamiltonianGraph.GraphInputProvider;
using MatchingExtensions;
using HamiltonianGraph.Utils;
using Xunit;

namespace HamiltonianGraph.UnitTests
{
    public class Primary_LatinComposition
    {
        [Fact]
        public void GetHamiltonianPathsTest1()
        {
            GetHamiltonianPathsTest(1);
        }

        [Fact]
        public void GetHamiltonianPathsTest2()
        {
            GetHamiltonianPathsTest(2);
        }

        [Fact]
        public void GetHamiltonianPathsTest3()
        {
            GetHamiltonianPathsTest(3);
        }

        [Fact]
        public void GetHamiltonianPathsTest4()
        {
            GetHamiltonianPathsTest(4);
        }

        [Fact]
        public void GetHamiltonianPathsTest5()
        {
            GetHamiltonianPathsTest(5);
        }

        [Fact]
        public void GetHamiltonianPathsTest6()
        {
            GetHamiltonianPathsTest(6);
        }

        [Fact]
        public void GetHamiltonianPathsTest7()
        {
            GetHamiltonianPathsTest(7);
        }

        [Fact]
        public void GetHamiltonianPathsTest8()
        {
            GetHamiltonianPathsTest(8);
        }

        [Fact]
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

            Assert.True(expectedPaths.InnerSequencesEqualInSomeOrder(actualPaths));
            Assert.Equal(expectedDistance, actualDistance);
        }
    }
}
