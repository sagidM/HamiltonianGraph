using System;
using System.Collections.Generic;

namespace HamiltonianGraph
{
    public class BranchAndBound
    {
        private readonly int n;
        private readonly int?[,] graph;
        internal const int Infinity = int.MaxValue >> 1;

        public BranchAndBound(int?[,] weights)
        {
            n = weights.GetLength(0);
            if (weights.GetLength(1) != n)
                throw new ArgumentException("Dimension of matrix must be the same: NxN", nameof(weights));

            graph = new int?[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    graph[i, j] = weights[i, j] ?? Infinity;
        }
        public int[] GetShortestHamiltonianCycle()
        {
            var states = new List<StateTree>(16)
            {
                new StateTree { fine = Reduction(graph), graph = graph, isCheapestChild = true, isSheet = true }
            };
            int n = this.n;
            (int i, int j) maxZero = (-1, -1);
            StateTree state;

            while (true)
            {
                state = GetCheapestState(states);
                var g = Copy(state.graph);

                var zeros = new List<(int, int)>();
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        if (g[i, j] == 0) zeros.Add((i, j));

                if (zeros.Count == 1)
                {
                    maxZero = zeros[0];
                    break;
                }

                // search maximum among minimums in zeros
                int max = -1;
                (int horizontal, int vertical) crossMins = (-1, -1);
                foreach (var (i, j) in zeros)
                {
                    var keeper = g[i, j];
                    g[i, j] = Infinity;
                    var horizontalMin = MinInRow(g, i);
                    var verticalMin = MinInColumn(g, j);

                    if (max < horizontalMin + verticalMin)
                    {
                        max = horizontalMin + verticalMin;
                        crossMins = (horizontalMin, verticalMin);
                        maxZero = (i, j);
                    }

                    g[i, j] = keeper;
                }

                g[maxZero.i, maxZero.j] = Infinity;
                var expensiveGraph = Copy(g);
                CrossReduction(expensiveGraph, maxZero, crossMins);
                RemoveCrossFromMatrix(g, maxZero);
                
                if (g[maxZero.j, maxZero.i].HasValue)
                    g[maxZero.j, maxZero.i] = Infinity;

                state.isSheet = false;
                int newFine = Reduction(g);

                states.Add(new StateTree { fine = state.fine + max, graph= expensiveGraph, isSheet=true, isCheapestChild = false, edge= maxZero, parent= state });
                states.Add(new StateTree { fine = state.fine + newFine, graph= Copy(g), isSheet=true, isCheapestChild = true, edge= maxZero, parent= state });
            }

            var edges = new int[n];
            edges[maxZero.i] = maxZero.j;
            for (int i = 0; i < n-1; i++)
            {
                if (state.isCheapestChild)
                    edges[state.edge.from] = state.edge.to;
                state = state.parent;
            }
            var cycle = new int[n+1];
            cycle[0] = 0;
            int k = 0;
            for (int i = 0; i < n; i++)
            {
                k = edges[k];
                cycle[i+1] = k;
            }
            return cycle;
        }

        private static StateTree GetCheapestState(IList<StateTree> states)
        {
            // the last state is always a sheet
            var cheapestState = states[states.Count - 1];
            for (int i = 0; i < states.Count - 1; i++)
            {
                var state = states[i];
                if (state.isSheet && cheapestState.fine > state.fine)
                    cheapestState = state;
            }
            return cheapestState;
        }

        internal static int Reduction(int?[,] g)
        {
            int sumOfMins = 0;
            int n = g.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                int minInRaw = MinInRow(g, i);
                if (minInRaw <= 0) continue;
                for (int j = 0; j < n; j++)
                {
                    if (g[i, j] == null) continue;
                    g[i, j] -= minInRaw;
                }
                sumOfMins += minInRaw;
            }

            for (int i = 0; i < n; i++)
            {
                int minInColumn = MinInColumn(g, i);
                if (minInColumn <= 0) continue;
                for (int j = 0; j < n; j++)
                {
                    if (g[j, i] == null) continue;
                    g[j, i] -= minInColumn;
                }
                sumOfMins += minInColumn;
            }

            return sumOfMins;
        }

        private static void Print(int?[,] g)
        {
            Console.WriteLine("Matrix:");
            for (int i = 0; i < g.GetLength(0); i++)
            {
                Console.Write("[");
                for (int j = 0; j < g.GetLength(1); j++)
                {
                    var s = g[i, j] == null ? "-" : g[i, j] > Infinity / 2 ? "INF" : g[i, j] + "";
                    Console.Write(s + (j + 1 < g.GetLength(1) ? ", " : ""));
                }
                Console.WriteLine("]");
            }
            Console.WriteLine();
        }

        internal static void CrossReduction(int?[,] g, (int i, int j) pos, (int vertical, int horizontal) mins)
        {
            int n = g.GetLength(0);
            for (int k = 0; k < n; k++)
            {
                if (g[k, pos.j].HasValue)
                    g[k, pos.j] -= mins.horizontal;
                if (g[pos.i, k].HasValue)
                    g[pos.i, k] -= mins.vertical;
            }
            g[pos.i, pos.j] = Infinity;
        }

        internal static void RemoveCrossFromMatrix(int?[,] g, (int i, int j) pos)
        {
            int n = g.GetLength(0);
            for (int k = 0; k < n; k++)
            {
                g[pos.i, k] = null;
                g[k, pos.j] = null;
            }
        }

        internal static int MinInColumn(int?[,] g, int columnIndex)
        {
            int min = Infinity + 1;
            int n = g.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                if (g[i, columnIndex] == null) continue;
                min = Math.Min(min, g[i, columnIndex].Value);
            }
            return min == Infinity + 1 ? -1 : min;
        }
        internal static int MinInRow(int?[,] g, int rowIndex)
        {
            int min = Infinity + 1;
            int n = g.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                if (g[rowIndex, i] == null) continue;
                min = Math.Min(min, g[rowIndex, i].Value);
            }
            return min == Infinity + 1 ? -1 : min;
        }

        private static int?[,] Copy(int?[,] matrix) => (int?[,])matrix.Clone();

        private class StateTree
        {
            public int fine;
            public int?[,] graph;
            public bool isSheet;
            public bool isCheapestChild;
            public (int from, int to) edge;
            public StateTree parent;
        }
    }
}
