using HamiltonianGraph.GraphInputProvider;
using MatchingExtensions;
using Xunit;

namespace HamiltonianGraph.UnitTests
{
    public class Primary_BranchAndBound
    {
        [Theory(DisplayName = "[BaB] Shortest cycle")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        public static void GetShortestHamiltonianCycleTest(int testNumber)
        {
            var matrix = AdjacencyMatrix.GetGraph(testNumber);
            var path = new BranchAndBound(matrix.Weights).GetShortestHamiltonianCycle();
            Assert.True(path.AreUnique(1, path.Length-1), string.Join(", ", path));

            var actualDistance = AdjacencyMatrix.PathDistance(path, matrix.Weights);
            var expectedDistance = matrix.ShortestPathDistance;

            Assert.Equal(expectedDistance, actualDistance);
        }
    }

}
