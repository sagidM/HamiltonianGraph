using Microsoft.VisualStudio.TestTools.UnitTesting;
using HamiltonianGraph.GraphInputProvider;
using MatchingExtensions;

namespace HamiltonianGraph.UnitTests
{
    [TestClass]
    public class BranchAndBoundTest
    {
        [TestMethod]
        public void ReductionTestSmall()
        {
            int?[,] actualWeights = GenerateSampleWeights();

            int?[,] expectedWeights = new int?[3, 3];
            expectedWeights[0, 1] = 0;
            expectedWeights[0, 2] = 0;

            expectedWeights[1, 0] = 0;
            expectedWeights[1, 2] = 0;

            expectedWeights[2, 0] = 1;
            expectedWeights[2, 1] = 0;

            const int expectedReductionResult = 11;
            int actualReductionResult = BranchAndBound.Reduction(actualWeights);

            Assert.AreEqual(expectedReductionResult, actualReductionResult);
            Assert.IsTrue(actualWeights.AreDeepEqual(expectedWeights));
        }
        [TestMethod]
        public void ReductionTestBig()
        {
            int?[,] actualWeights = new int?[6, 6];
            actualWeights[0, 1] = 0;
            actualWeights[0, 2] = 3;
            actualWeights[0, 3] = 0;
            actualWeights[0, 5] = 0;

            actualWeights[1, 2] = 0;
            actualWeights[1, 3] = 6;
            actualWeights[1, 5] = 1;

            actualWeights[3, 0] = 0;
            actualWeights[3, 1] = 13;
            actualWeights[3, 5] = 3;

            actualWeights[4, 1] = 22;
            actualWeights[4, 2] = BranchAndBound.Infinity;
            actualWeights[4, 3] = 23;
            actualWeights[4, 5] = 4;

            actualWeights[5, 0] = 0;
            actualWeights[5, 1] = 8;


            int?[,] expectedWeights = (int?[,])actualWeights.Clone();
            expectedWeights[4, 1] = 18;
            expectedWeights[4, 2] = BranchAndBound.Infinity - 4;
            expectedWeights[4, 3] = 19;
            expectedWeights[4, 5] = 0;


            const int expectedReductionResult = 4;
            int actualReductionResult = BranchAndBound.Reduction(actualWeights);

            Assert.AreEqual(expectedReductionResult, actualReductionResult);
            Assert.IsTrue(actualWeights.AreDeepEqual(expectedWeights));
        }

        [TestMethod]
        public void CrossReductionTest()
        {
            int?[,] actualWeights = GenerateSampleWeights();
            int?[,] expectedWeights = (int?[,])actualWeights.Clone();
            (var i, var j) = (1, 2);
            (var h, var v) = (5, 3);
            expectedWeights[i, 0] -= h;
            expectedWeights[0, j] -= v;
            expectedWeights[i, j] = BranchAndBound.Infinity;

            BranchAndBound.CrossReduction(actualWeights, pos: (i, j), mins: (h, v));

            Assert.IsTrue(actualWeights.AreDeepEqual(expectedWeights));
        }

        [TestMethod]
        public void RemoveCrossFromMatrixTest()
        {
            int?[,] actualWeights = GenerateSampleWeights();
            int?[,] expectedWeights = (int?[,])actualWeights.Clone();
            (var i, var j) = (1, 2);
            expectedWeights[i, 0] = null;
            expectedWeights[0, j] = null;
            expectedWeights[i, j] = null;

            BranchAndBound.RemoveCrossFromMatrix(actualWeights, pos: (i, j));

            Assert.IsTrue(actualWeights.AreDeepEqual(expectedWeights));
        }

        [TestMethod]
        public void MinInRawTest()
        {
            int?[,] weights = new int?[4, 4];
            weights[0, 1] = 5;
            weights[0, 3] = 6;

            int actual = BranchAndBound.MinInRow(weights, 0);
            int expected = 5;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MinInColumnTest()
        {
            int?[,] weights = new int?[4, 4];
            weights[1, 0] = 5;
            weights[3, 0] = 6;

            int actual = BranchAndBound.MinInColumn(weights, 0);
            int expected = 5;
            Assert.AreEqual(expected, actual);
        }

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


        /**
         * |- 2 3|
         * |5 - 6|
         * |4 3 -|
         */
        private static int?[,] GenerateSampleWeights()
        {
            int?[,] actualWeights = new int?[3, 3];
            actualWeights[0, 1] = 2;
            actualWeights[0, 2] = 3;

            actualWeights[1, 0] = 5;
            actualWeights[1, 2] = 6;

            actualWeights[2, 0] = 4;
            actualWeights[2, 1] = 3;
            return actualWeights;
        }

        private static void GetShortestHamiltonianPathTest(int testNumber)
        {
            var matrix = AdjacencyMatrix.GetGraph(testNumber);
            var path = new BranchAndBound(matrix.Weights).GetShortestHamiltonianCycle();
            var actualDistance = AdjacencyMatrix.PathDistance(path, matrix.Weights);
            var expectedDistance = matrix.ShortestPathDistance;

            Assert.AreEqual(expectedDistance, actualDistance);
        }
    }

}
