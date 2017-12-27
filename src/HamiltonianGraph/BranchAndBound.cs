using System;
using System.Collections.Generic;

namespace HamiltonianGraph
{
    public class BranchAndBound
    {
        private readonly int n;
        private readonly int?[,] graph;
        internal const int Infinity = int.MaxValue >> 2;

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
            //var sw = new System.Diagnostics.Stopwatch();
            // R - cost matrix
            var stateHeap = new List<StateTree>(32)
            {
                new StateTree { fine = Reduction(graph), graph = graph, isCheapestChild = true, isSheet = true, edge = (-1, -1) }
            };
            int n = this.n;
            StateTree state;
            var zeros = new List<(int i, int j)>();
            int[] pathFromTo = new int[n + 1];
            int[] pathToFrom = new int[n + 1];

            while (true)
            {
                state = stateHeap.ShiftAndRelax();
                var g = Copy(state.graph); // 0.07x

                zeros.Clear();
                // 0.17x
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        if (g[i, j] == 0) zeros.Add((i, j));

                if (zeros.Count == 1)
                    break;

                // search maximum among minimums in zeros
                int max = -1;
                (int horizontal, int vertical) crossMins = (-1, -1);
                (int i, int j) maxZero = (-1, -1);
                //sw.Start();
                //sw.Stop();
                foreach (var (i, j) in zeros)
                {
                    var keeper = g[i, j];
                    g[i, j] = Infinity;
                    // 0.3x
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

                //Print(g);
                var expensiveGraph = Copy(g); // 0.07x
                expensiveGraph[maxZero.i, maxZero.j] = Infinity;
                CrossReduction(expensiveGraph, maxZero, crossMins);
                RemoveCrossFromMatrix(g, maxZero);


                // prevent hamiltonian subcycles
                if (g[maxZero.j, maxZero.i].HasValue)
                    g[maxZero.j, maxZero.i] = Infinity;
                Array.Clear(pathFromTo, 0, pathFromTo.Length);
                Array.Clear(pathToFrom, 0, pathToFrom.Length);
                pathFromTo[maxZero.i+1] = maxZero.j+1;
                pathToFrom[maxZero.j+1] = maxZero.i+1;
                int pathLength = 1;
                var stateAncestor = state;
                while (stateAncestor.parent != null)
                {
                    if (stateAncestor.isCheapestChild)
                    {
                        pathLength++;
                        var (from, to) = stateAncestor.edge;
                        from++; to++;
                        pathFromTo[from] = to;
                        pathToFrom[to] = from;
                    }
                    stateAncestor = stateAncestor.parent;
                }
                if (pathLength + 1 != n)
                {
                    int tail = maxZero.j+1;
                    while (pathFromTo[tail] != 0)
                        tail = pathFromTo[tail];
                    int head = maxZero.i + 1;
                    while (pathToFrom[head] != 0)
                        head = pathToFrom[head];

                    if (g[tail - 1, head - 1].HasValue)
                        g[tail - 1, head - 1] = Infinity;
                }
                // end subcycles

                state.isSheet = false;
                var cheapestFine = Reduction(g);  // 0.24x

                // 0.04x
                stateHeap.AddAndSiftUp(new StateTree { fine = state.fine + max, graph= expensiveGraph, isSheet=true, isCheapestChild = false, edge= maxZero, parent= state });
                stateHeap.AddAndSiftUp(new StateTree { fine = state.fine + cheapestFine, graph = g, isSheet = true, isCheapestChild = true, edge = maxZero, parent = state });
            }
            //System.Diagnostics.Debug.WriteLine(sw.Elapsed);

            var edges = new int[n];
            edges[zeros[0].i] = zeros[0].j;
            for (int i = 0; i < n-1;)
            {
                if (state.isCheapestChild)
                {
                    edges[state.edge.from] = state.edge.to;
                    i++;
                }
                state = state.parent;
            }
            var cycle = new int[n+1];
            cycle[0] = 0;
            int edgeFrom = 0;
            for (int i = 1; i <= n; i++)
            {
                edgeFrom = edges[edgeFrom]; // => edge to
                cycle[i] = edgeFrom;
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

        /// <summary>
        /// Removse min.horizontal from pos.i line and
        /// removes min.vertical from pos.j column
        /// </summary>
        internal static void CrossReduction(int?[,] g, (int i, int j) pos, (int horizontal, int vertical) mins)
        {
            int n = g.GetLength(0);
            for (int k = 0; k < n; k++)
            {
                if (g[pos.i, k].HasValue)
                    g[pos.i, k] -= mins.horizontal;
                if (g[k, pos.j].HasValue)
                    g[k, pos.j] -= mins.vertical;
            }
            g[pos.i, pos.j] = Infinity;
        }

        /// <summary>
        /// Sets null in line and column
        /// </summary>
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

    }

    internal sealed class StateTree : IComparable<StateTree>
    {
        public int fine;
        public int?[,] graph;
        public bool isSheet;
        public bool isCheapestChild;
        public (int from, int to) edge;
        public StateTree parent;

#if DEBUG
        public static int nextId = 1;
        public int id = nextId++;
        public override string ToString()
        {
            return $"id: {id}, fine: {fine}, edge: {edge}, parent.id: {parent?.id.ToString() ?? "null"}";
        }
#endif

        public int CompareTo(StateTree other)
        {
            return fine - other.fine;
        }
    }

    // work with heap
    internal static class StateTreeExtensions
    {
        // fine in ascending order excluding sheet
        // e.g. states[0] would be with smallest fine
        public static void AddAndSiftUp(this List<StateTree> states, StateTree newState)
        {
            states.Add(newState);
            int pos = states.Count - 1;
            while (pos > 0)
            {
                int parent = pos - (pos % 2 == 0 ? 2 : 1) >> 1;
                if (states[pos].CompareTo(states[parent]) >= 0)
                    break;
                var t = states[pos];
                states[pos] = states[parent];
                states[parent] = t;
                pos = parent;
            }
        }
        public static StateTree ShiftAndRelax(this List<StateTree> states)
        {
            int lastIndex = states.Count - 1;
            var result = states[0];
            states[0] = states[lastIndex];
            states[lastIndex] = result;

            int parent = 0;
            while (true)
            {
                var left = parent * 2 + 1;
                var right = left + 1;
                if (left >= lastIndex) break;
                if (right == lastIndex)
                {
                    if (states[parent].CompareTo(states[left]) > 0)
                    {
                        var t = states[parent];
                        states[parent] = states[left];
                        states[left] = t;
                    }
                    break;
                }

                var better = states[left].CompareTo(states[right]) < 0 ? left : right;
                if (states[parent].CompareTo(states[better]) <= 0) break;

                var t1 = states[parent];
                states[parent] = states[better];
                states[better] = t1;
                parent = better;
            }

            states.RemoveAt(lastIndex);
            return result;
        }
    }
}
