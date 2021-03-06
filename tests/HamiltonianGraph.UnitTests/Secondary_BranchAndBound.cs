﻿using MatchingExtensions;
using System.Linq;
using Xunit;

namespace HamiltonianGraph.UnitTests
{
    public class Secondary_BranchAndBound
    {
        private const int Infinity = BranchAndBound.Infinity;

        [Fact(DisplayName = "[BaB] Reduction (small)")]
        public void ReductionTestSmall()
        {
            int[,] actualWeights = GenerateSampleWeights();

            int[,] expectedWeights = new int[3, 3];
            expectedWeights[0, 0] = Infinity;
            expectedWeights[0, 1] = 0;
            expectedWeights[0, 2] = 0;

            expectedWeights[1, 0] = 0;
            expectedWeights[1, 1] = Infinity;
            expectedWeights[1, 2] = 0;

            expectedWeights[2, 0] = 1;
            expectedWeights[2, 1] = 0;
            expectedWeights[2, 2] = Infinity;

            const int expectedReductionResult = 11;
            int actualReductionResult = BranchAndBound.Reduction(actualWeights);

            Assert.Equal(expectedReductionResult, actualReductionResult);

            // test does not care about the main diagonal
            SetMainDiagonal(actualWeights, Infinity);
            Assert.True(actualWeights.AreDeepEqual(expectedWeights));
        }
        [Fact(DisplayName = "[BaB] Reduction (big)")]
        public void ReductionTestBig()
        {
            int[,] actualWeights = GenerateMatrixWithValue(sizeN: 6, value: 0);

            // some random values that change nothing
            actualWeights[0, 2] = 3;
            actualWeights[1, 3] = 6;
            actualWeights[1, 5] = 1;
            actualWeights[3, 1] = 13;
            actualWeights[3, 5] = 3;
            actualWeights[5, 0] = 0;
            actualWeights[5, 1] = 8;

            // minimum value in this row - 4
            actualWeights[4, 0] = Infinity;
            actualWeights[4, 1] = 22;
            actualWeights[4, 2] = Infinity;
            actualWeights[4, 3] = 23;
            actualWeights[4, 4] = Infinity;
            actualWeights[4, 5] = 4;

            const int expectedReductionResult = 4;

            int[,] expectedWeights = (int[,])actualWeights.Clone();
            for (int i = 0; i < 6; i++)
                expectedWeights[4, i] -= expectedReductionResult;


            int actualReductionResult = BranchAndBound.Reduction(actualWeights);

            Assert.Equal(expectedReductionResult, actualReductionResult);
            Assert.True(actualWeights.AreDeepEqual(expectedWeights));
        }

        [Fact(DisplayName = "[BaB] CrossReduction")]
        public void CrossReductionTest()
        {
            int[,] actualWeights = GenerateSampleWeights();

            int[,] expectedWeights = (int[,])actualWeights.Clone();
            (var i, var j) = (1, 2);
            (var h, var v) = (5, 3);
            expectedWeights[i, 0] -= h;
            expectedWeights[i, 1] -= h;
            expectedWeights[0, j] -= v;
            expectedWeights[2, j] -= v;
            expectedWeights[i, j] = BranchAndBound.Infinity;

            BranchAndBound.SubstractCross(actualWeights, pos: (i, j), mins: (h, v));

            Assert.True(actualWeights.AreDeepEqual(expectedWeights));
        }

        [Fact(DisplayName = "[BaB] Cross")]
        public void RemoveCrossFromMatrixTest()
        {
            int[,] actualWeights = GenerateSampleWeights();
            int[,] expectedWeights = new int[2, 2];
            (var i, var j) = (1, 2);
            expectedWeights[0, 0] = actualWeights[0, 0];
            expectedWeights[0, 1] = actualWeights[0, 1];
            expectedWeights[1, 0] = actualWeights[2, 0];
            expectedWeights[1, 1] = actualWeights[2, 1];
            var expectedRowIndices = new[] { 0, 2 };
            var expectedColumnIndices = new[] { 0, 1 };

            var range = Enumerable.Range(0, 3).ToArray();
            var state = new StateNode
            {
                graph = actualWeights,
                rowIndices = range,
                columnIndices = range
            };
            BranchAndBound.UpdateStateNodeWithCrossClippedGraph(state, cutPosition: (i: i, j: j));

            Assert.True(state.graph.AreDeepEqual(expectedWeights));
            Assert.True(state.rowIndices.AreDeepEqual(expectedRowIndices));
            Assert.True(state.columnIndices.AreDeepEqual(expectedColumnIndices));
        }

        [Fact(DisplayName = "[BaB] Min in raw")]
        public void MinInRawTest()
        {
            int[,] weights = GenerateMatrixWithValue(sizeN: 4, value: Infinity);
            weights[0, 1] = 5;
            weights[0, 3] = 6;

            int actual = BranchAndBound.MinInRow(weights, 0);
            int expected = 5;
            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "[BaB] Min in column")]
        public void MinInColumnTest()
        {
            int[,] weights = GenerateMatrixWithValue(sizeN: 4, value: Infinity);
            weights[1, 0] = 5;
            weights[3, 0] = 6;

            int actual = BranchAndBound.MinInColumn(weights, 0);
            int expected = 5;
            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "[BaB] Huge test")]
        public void BaB_Huge()
        {
            for (int i = 43; i <= 43; i++)
            {
                var weights = Compare_LC_And_BaB.GenerateRandomFullGraph(n: 43, maxRandomValue: 100);
                var bb = new BranchAndBound(weights).GetShortestHamiltonianCycle();
            }
        }

        [Fact(DisplayName ="[BaB] Single Vertex")]
        public void BaB_1x1()
        {
            var cycle = new BranchAndBound(new int?[1, 1]).GetShortestHamiltonianCycle();
            Assert.Single(cycle, 0);
        }

        [Fact(DisplayName ="[BaB] Empty Graph")]
        public void BaB_0x0()
        {
            var actual = new BranchAndBound(new int?[0, 0]).GetShortestHamiltonianCycle();
            Assert.Empty(actual);
        }


        /**
         * |- 2 3|
         * |5 - 6|
         * |4 3 -|
         */
        private static int[,] GenerateSampleWeights()
        {
            int[,] actualWeights = new int[3, 3];
            actualWeights[0, 0] = Infinity;
            actualWeights[0, 1] = 2;
            actualWeights[0, 2] = 3;

            actualWeights[1, 0] = 5;
            actualWeights[1, 1] = Infinity;
            actualWeights[1, 2] = 6;

            actualWeights[2, 0] = 4;
            actualWeights[2, 1] = 3;
            actualWeights[2, 2] = Infinity;
            return actualWeights;
        }

        private static void SetMainDiagonal(int[,] matrix, int value)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
                matrix[i, i] = value;
        }
        private static int[,] GenerateMatrixWithValue(int sizeN, int value)
        {
            var matrix = new int[sizeN, sizeN];
            for (int i = 0; i < sizeN; i++)
                for (int j = 0; j < sizeN; j++)
                    matrix[i, j] = value;
            return matrix;
        }
    }
}
