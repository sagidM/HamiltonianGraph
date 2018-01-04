using HamiltonianGraph.GraphInputProvider;
using MatchingExtensions;
using HamiltonianGraph.Utils;
using Xunit;

namespace HamiltonianGraph.UnitTests
{
    public class Primary_LatinComposition
    {
        [Theory(DisplayName = "[LC] Shortest cycles")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        public void GetHamiltonianCyclesTest(int testNumber)
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
