using System;
using System.Collections.Generic;
using System.Linq;

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
            int n = this.n;
            switch (n)
            {
                case 0: return new int[0];
                case 1: return new[] { 0 };
                case 2: return
                            graph[0,1] < Infinity && graph[1,0] < Infinity
                                ? new[] { 0, 1, 0 } : null;
            }

            var sw = new System.Diagnostics.Stopwatch();
            StateTree state = new StateTree
            {
                graph = graph, graph2 = graph, edge = (-1, -1),
                rowIndices = Enumerable.Range(0, n).ToArray(),
                columnIndices = Enumerable.Range(0, n).ToArray(),
                isCheapestChild = true, isSheet = true
            };
            state.fine = Reduction(state);
            StateTree cachedCheapestState = state;
            // R - cost matrix
            var stateHeap = new List<StateTree>(32);
            var zeros = new List<(int i, int j)>();
            int[] pathFromTo = new int[n + 1];
            int[] pathToFrom = new int[n + 1];
            int[] minInColumnCached = new int[n];

            while (true)
            {
                state = cachedCheapestState ?? stateHeap.ShiftAndRelax();
                if (state.fine >= Infinity) return null;
                var g = Copy(state.graph); // 0.02
                n = g.GetLength(0);

                zeros.Clear();
                // 0.056
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        if (g[i, j] == 0) zeros.Add((i, j));
                        else if (g[i, j] == null) throw new Exception("the null was found in the graph: " + i + "x" + j);

                if (zeros.Count == 1)
                    break;

                var cheapestState = new StateTree
                {
                    fine = state.fine, graph = g, isCheapestChild = true, isSheet = true,
                    rowIndices = Copy(state.rowIndices), columnIndices = Copy(state.columnIndices), parent = state
                };

                // search maximum among minimums in zeros
                int max = -1;
                (int horizontal, int vertical) crossMins = (-1, -1);
                (int i, int j) maxZero = (-1, -1);
                (int i, int value) minInRowCached = (-1, 0);
                Array.Clear(minInColumnCached, 0, n);
                foreach (var (i, j) in zeros)
                {
                    var keeper = g[i, j];
                    g[i, j] = Infinity;
                    // 0.15
                    if (minInRowCached.i != i)
                        minInRowCached = (i, MinInRow(cheapestState, i));
                    var horizontalMin = minInRowCached.value;
                    if (minInColumnCached[j] == 0)
                        minInColumnCached[j] = MinInColumn(cheapestState, j) + 1;
                    var verticalMin = minInColumnCached[j] - 1;

                    if (max < horizontalMin + verticalMin)
                    {
                        max = horizontalMin + verticalMin;
                        crossMins = (horizontalMin, verticalMin);
                        maxZero = (i, j);
                    }

                    g[i, j] = keeper;
                }
                cheapestState.edge = maxZero;

                //Print(g);
                var expensiveGraph = Copy(g); // 0.05
                expensiveGraph[maxZero.i, maxZero.j] = Infinity;
                var expensiveState = new StateTree
                {
                    fine = state.fine + max, graph = expensiveGraph, isCheapestChild = false, isSheet = true, edge = maxZero,
                    rowIndices = Copy(state.rowIndices), columnIndices = Copy(state.columnIndices), parent = state
                };
                CrossReduction(expensiveState, maxZero, crossMins);
                var maxZeroReverse = maxZero;
                maxZeroReverse.i = Array.BinarySearch(cheapestState.columnIndices, cheapestState.rowIndices[maxZero.i]);
                maxZeroReverse.j = Array.BinarySearch(cheapestState.rowIndices, cheapestState.columnIndices[maxZero.j]);
                if (maxZeroReverse.i >= 0 && maxZeroReverse.j >= 0)
                    g[maxZeroReverse.j, maxZeroReverse.i] = Infinity;

                sw.Start();
                RemoveCrossFromMatrix(cheapestState, maxZero); // 0.1
                sw.Stop();
                g = cheapestState.graph;


                // prevent subcycles
                if (n != 2)
                {
                    Array.Clear(pathFromTo, 0, pathFromTo.Length);
                    Array.Clear(pathToFrom, 0, pathToFrom.Length);
                    var stateAncestor = cheapestState;
                    while (stateAncestor.parent != null)
                    {
                        if (stateAncestor.isCheapestChild)
                        {
                            var (from1, to1) = stateAncestor.edge;
                            from1 = stateAncestor.parent.rowIndices[from1];
                            to1 = stateAncestor.parent.columnIndices[to1];
                            from1++; to1++;
                            pathFromTo[from1] = to1;
                            pathToFrom[to1] = from1;
                        }
                        stateAncestor = stateAncestor.parent;
                    }
                    int tail = cheapestState.parent.rowIndices[cheapestState.edge.from] + 1;
                    while (pathFromTo[tail] != 0)
                        tail = pathFromTo[tail];
                    int head = cheapestState.parent.columnIndices[cheapestState.edge.to] + 1;
                    while (pathToFrom[head] != 0)
                        head = pathToFrom[head];

                    var from = Array.BinarySearch(cheapestState.rowIndices, tail - 1);
                    var to = Array.BinarySearch(cheapestState.columnIndices, head - 1);
                    if (g[from, to].HasValue)
                        g[from, to] = Infinity;
                }
                // end subcycles

                state.isSheet = false;
                var cheapestFine = Reduction(cheapestState);  // 0.24x
                
                stateHeap.AddAndSiftUp(expensiveState);
                cheapestState.fine += cheapestFine;
                if (cheapestFine == 0)
                {
                    cachedCheapestState = cheapestState;
                }
                else
                {
                    stateHeap.AddAndSiftUp(cheapestState);
                    cachedCheapestState = null;
                }
            }
            System.Diagnostics.Debug.WriteLine(sw.Elapsed);

            n = this.n;
            var edges = new int[n];
            edges[state.rowIndices[zeros[0].i]] = state.columnIndices[zeros[0].j];
            for (int i = 0; i < n-1;)
            {
                if (state.isCheapestChild)
                {
                    edges[state.parent.rowIndices[state.edge.from]] = state.parent.columnIndices[state.edge.to];
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

        internal static int Reduction(StateTree state)
        {
            int?[,] g = state.graph;
            int sumOfMins = 0;
            int n = g.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                int minInRaw = MinInRow(state, i);
                if (minInRaw <= 0) continue;
                for (int j = 0; j < n; j++)
                {
                    if (g[i, j] == null) continue;
                    g[i, j] -= minInRaw;
                }
                if (sumOfMins < Infinity)
                    sumOfMins += minInRaw;
            }

            for (int i = 0; i < n; i++)
            {
                int minInColumn = MinInColumn(state, i);
                if (minInColumn <= 0) continue;
                for (int j = 0; j < n; j++)
                {
                    if (g[j, i] == null) continue;
                    g[j, i] -= minInColumn;
                }
                if (sumOfMins < Infinity)
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
        internal static void CrossReduction(StateTree state, (int i, int j) pos, (int horizontal, int vertical) mins)
        {
            var g = state.graph;
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
        internal static void RemoveCrossFromMatrix(StateTree state, (int i, int j) pos)
        {
            int?[,] g = state.graph;
            int n = g.GetLength(0);
            var t = new int?[n-1, n-1];
            //int[] r = new int[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                int gi = (i < pos.i) ? i : i + 1;
               // r[i] = state.rowIndices[gi];
                for (int j = 0; j < n - 1; j++)
                {
                    int gj = (j < pos.j) ? j : j + 1;
                    if (g[gi, gj].HasValue)
                        t[i, j] = g[gi, gj].Value;
                }
            }
            state.graph = t;
            state.rowIndices = CopyExcept(state.rowIndices, pos.i);
            //state.rowIndices = r;
            state.columnIndices = CopyExcept(state.columnIndices, pos.j);
        }

        internal static int MinInColumn(StateTree state, int columnIndex)
        {
            int?[,] g = state.graph;
            int min = Infinity + 1;
            int n = g.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                if (g[i, columnIndex] == null) continue;
                min = Math.Min(min, g[i, columnIndex].Value);
            }
            return min == Infinity + 1 ? -1 : min;
        }
        internal static int MinInRow(StateTree state, int rowIndex)
        {
            int?[,] g = state.graph;
            int min = Infinity + 1;
            int n = g.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                if (g[rowIndex, i] == null) continue;
                min = Math.Min(min, g[rowIndex, i].Value);
            }
            return min == Infinity + 1 ? -1 : min;
        }

        // ([0,1,2,3,4,5], 2) => [0,1,3,4,5]
        private static int[] CopyExcept(int[] array, int exceptIndex)
        {
            var result = new int[array.Length-1];
            Array.Copy(array, 0, result, 0, exceptIndex);
            Array.Copy(array, exceptIndex+1, result, exceptIndex, result.Length - exceptIndex);
            return result;
        }
        private static T Copy<T>(T matrix) where T : ICloneable => (T)matrix.Clone();
    }

    internal sealed class StateTree : IComparable<StateTree>
    {
        public int fine;
        public int?[,] graph;
        public int?[,] graph2;
        public int[] columnIndices;
        public int[] rowIndices;
        public bool isSheet;
        public bool isCheapestChild;
        public (int from, int to) edge;
        public StateTree parent;

#if DEBUG
        public static int nextId = 1;
        public int id = nextId++;
        public string FT => parent.rowIndices[edge.from] + " => " + parent.columnIndices[edge.to];
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
