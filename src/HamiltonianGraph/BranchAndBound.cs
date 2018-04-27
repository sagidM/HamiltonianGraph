using System;
using System.Collections.Generic;
using System.Linq;

namespace HamiltonianGraph
{
    public class BranchAndBound
    {
        private readonly int n;
        private readonly int[,] graph;
        internal const int Infinity = int.MaxValue >> 2;

        public BranchAndBound(int?[,] weights)
        {
            n = weights.GetLength(0);
            if (weights.GetLength(1) != n)
                throw new ArgumentException("Dimension of matrix must be the same: NxN", nameof(weights));

            graph = new int[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    graph[i, j] = weights[i, j] ?? Infinity;
        }
        public int[] GetShortestHamiltonianCycle()
        {
            int n = this.n;
            switch (n)
            {
                case 0:
                    return new int[0];
                case 1:
                    return new int[] { 0 };
                case 2:
                    return graph[0, 1] < Infinity && graph[1, 0] < Infinity
                             ? new[] { 0, 1, 0 } : null;
            }

            var sw = new System.Diagnostics.Stopwatch();
            StateNode cachedCheapestState = new StateNode
            {
                graph = graph, edge = (-1, -1),
                rowIndices = Enumerable.Range(0, n).ToArray(),
                columnIndices = Enumerable.Range(0, n).ToArray(),
                isCheapestChild = true, isSheet = true
            };
            cachedCheapestState.fine = Reduction(cachedCheapestState.graph);

            // R - cost tree
            var statesHeap = new List<StateNode>(32);
            var zeros = new List<(int i, int j)>();
            int[] pathFromToBuff = new int[n + 1];
            int[] pathToFromBuff = new int[n + 1];
            int[] minInColumnCached = new int[n];
            StateNode state = null;

            while (true)
            {
                sw.Start();
                state = cachedCheapestState ?? statesHeap.ShiftAndRelax();
                sw.Stop();
                if (state.fine >= Infinity) return null;
                // "g" is just a simple name for cheapestState.graph
                var g = Copy(state.graph); // 0.02
                n = g.GetLength(0);

                zeros.Clear();
                // 0.02
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        if (g[i, j] == 0) zeros.Add((i, j));

                if (zeros.Count == 1)
                    break;

                var cheapestState = new StateNode
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
                // 0.053
                foreach (var (i, j) in zeros)
                {
                    var keeper = g[i, j];
                    g[i, j] = Infinity;
                    if (minInRowCached.i != i)
                        minInRowCached = (i, MinInRow(matrix: g, rowIndex: i));
                    var horizontalMin = minInRowCached.value;
                    if (minInColumnCached[j] == 0)
                        minInColumnCached[j] = MinInColumn(matrix: g, columnIndex: j) + 1;
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
                var expensiveGraph = Copy(g); // ~ 0.014
                expensiveGraph[maxZero.i, maxZero.j] = Infinity;
                var expensiveState = new StateNode
                {
                    fine = state.fine + max, graph = expensiveGraph, isCheapestChild = false, isSheet = true, edge = maxZero,
                    rowIndices = Copy(state.rowIndices), columnIndices = Copy(state.columnIndices), parent = state
                };
                SubstractCross(expensiveState.graph, maxZero, crossMins);

                var rowIndex = Array.BinarySearch(cheapestState.rowIndices, cheapestState.columnIndices[maxZero.j]);
                if (rowIndex >= 0)
                {
                    var columnIndex = Array.BinarySearch(cheapestState.columnIndices, cheapestState.rowIndices[maxZero.i]);
                    if (columnIndex >= 0)
                        g[rowIndex, columnIndex] = Infinity;
                }

                UpdateStateNodeWithCrossClippedGraph(cheapestState, cutPosition: maxZero); // 0.048
                g = cheapestState.graph; // method above creates a new instance of graph


                PreventSubcycles(cheapestState, pathFromToBuff, pathToFromBuff);

                state.isSheet = false;
                statesHeap.AddAndSiftUp(expensiveState);

                var cheapestFine = Reduction(g);  // 0.043
                cheapestState.fine += cheapestFine;
                if (cheapestFine == 0)
                {
                    cachedCheapestState = cheapestState;
                }
                else
                {
                    statesHeap.AddAndSiftUp(cheapestState);
                    cachedCheapestState = null;
                }
            }
            System.Diagnostics.Debug.WriteLine(sw.Elapsed);

            n = this.n;

            // gather nodes
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

            // make cycle from gathered nodes
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

        // Complexity: O(state depth)
        private static void PreventSubcycles(StateNode state, int[] pathFromToBuff, int[] pathToFromBuff)
        {
            if (state.graph.Length == 1)
                return;

            Array.Clear(pathFromToBuff, 1, pathFromToBuff.Length - 1);
            Array.Clear(pathToFromBuff, 1, pathToFromBuff.Length - 1);
            var stateAncestor = state;
            while (stateAncestor.parent != null)
            {
                if (stateAncestor.isCheapestChild)
                {
                    var (from, to) = stateAncestor.edge;
                    from = stateAncestor.parent.rowIndices[from] + 1;
                    to = stateAncestor.parent.columnIndices[to] + 1;
                    pathFromToBuff[from] = to;
                    pathToFromBuff[to] = from;
                }
                stateAncestor = stateAncestor.parent;
            }
            int tail = state.parent.rowIndices[state.edge.from] + 1;
            while (pathFromToBuff[tail] != 0)
                tail = pathFromToBuff[tail];
            int head = state.parent.columnIndices[state.edge.to] + 1;
            while (pathToFromBuff[head] != 0)
                head = pathToFromBuff[head];

            tail = Array.BinarySearch(state.rowIndices, tail - 1);
            head = Array.BinarySearch(state.columnIndices, head - 1);
            state.graph[tail, head] = Infinity;
        }

        internal static int Reduction(int[,] matrix)
        {
            int sumOfMins = 0;
            int n = matrix.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                int minInRaw = MinInRow(matrix, rowIndex: i);
                if (minInRaw == 0) continue;
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] -= minInRaw;
                }
                if (sumOfMins < Infinity)   // Stack Overflow
                    sumOfMins += minInRaw;
            }

            for (int i = 0; i < n; i++)
            {
                int minInColumn = MinInColumn(matrix, columnIndex: i);
                if (minInColumn == 0) continue;
                for (int j = 0; j < n; j++)
                {
                    matrix[j, i] -= minInColumn;
                }
                if (sumOfMins < Infinity)
                    sumOfMins += minInColumn;
            }

            return sumOfMins;
        }

        /// <summary>
        /// Removse min.horizontal from pos.i line and
        /// removes min.vertical from pos.j column
        /// </summary>
        internal static void SubstractCross(int[,] matrix, (int i, int j) pos, (int horizontal, int vertical) mins)
        {
            int n = matrix.GetLength(0);
            for (int k = 0; k < n; k++)
            {
                matrix[pos.i, k] -= mins.horizontal;
                matrix[k, pos.j] -= mins.vertical;
            }
            matrix[pos.i, pos.j] = Infinity;
        }

        /// <summary>
        /// Provides a new inctance of graph without i row and j column.
        /// Also creates suited row's and column's indices
        /// </summary>
        internal static void UpdateStateNodeWithCrossClippedGraph(StateNode stateNode, (int i, int j) cutPosition)
        {
            var g = stateNode.graph;
            int n = g.GetLength(0);
            var graphWithoutPos = new int[n - 1, n - 1];  // without row[i] and column[j]
            //int[] r = new int[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                int gi = (i < cutPosition.i) ? i : i + 1;
               // r[i] = state.rowIndices[gi];
                for (int j = 0; j < n - 1; j++)
                {
                    int gj = (j < cutPosition.j) ? j : j + 1;
                    graphWithoutPos[i, j] = g[gi, gj];
                }
            }
            stateNode.graph = graphWithoutPos;
            stateNode.rowIndices = CopyExcept(stateNode.rowIndices, cutPosition.i);
            //state.rowIndices = r;
            stateNode.columnIndices = CopyExcept(stateNode.columnIndices, cutPosition.j);
        }

        internal static int MinInColumn(int[,] matrix, int columnIndex)
        {
            int min = Infinity;
            int n = matrix.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                min = Math.Min(min, matrix[i, columnIndex]);
            }
            return min;
        }
        internal static int MinInRow(int[,] matrix, int rowIndex)
        {
            int min = Infinity;
            int n = matrix.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                min = Math.Min(min, matrix[rowIndex, i]);
            }
            return min;
        }

        // ([0,1,2,3,4,5], 2) => [0,1,3,4,5]
        private static int[] CopyExcept(int[] array, int exceptIndex)
        {
            var result = new int[array.Length-1];
            Array.Copy(array, 0, result, 0, exceptIndex);
            Array.Copy(array, exceptIndex+1, result, exceptIndex, result.Length - exceptIndex);
            return result;
        }

        private static int[] Copy(int[] matrix) => (int[])matrix.Clone();
        private static int[,] Copy(int[,] matrix) => (int[,])matrix.Clone();


#if DEBUG
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
#endif
    }

    internal sealed class StateNode : IComparable<StateNode>
    {
        public int fine;
        public int[,] graph;
        public int[] columnIndices;
        public int[] rowIndices;
        public bool isSheet;
        public bool isCheapestChild;
        public (int from, int to) edge;
        public StateNode parent;

#if DEBUG
        public static int nextId = 1;
        public int id = nextId++;
        public string GO => parent.rowIndices[edge.from] + " => " + parent.columnIndices[edge.to];
        public override string ToString()
        {
            return $"id: {id}, fine: {fine}, edge: {edge}, parent.id: {parent?.id.ToString() ?? "null"}";
        }
#endif

        public int CompareTo(StateNode other)
        {
            if (fine == other.fine)
                return graph.GetLength(0) - other.graph.GetLength(0);
            return fine - other.fine;
        }
    }

    // work with heap
    internal static class StateNodeExtensions
    {
        // fine in ascending order excluding sheet
        // e.g. states[0] would be with smallest fine
        public static void AddAndSiftUp(this List<StateNode> statesHeap, StateNode newState)
        {
            statesHeap.Add(newState);
            int pos = statesHeap.Count - 1;
            while (pos > 0)
            {
                int parent = pos - (pos % 2 == 0 ? 2 : 1) >> 1;
                if (statesHeap[pos].CompareTo(statesHeap[parent]) >= 0)
                    break;
                var t = statesHeap[pos];
                statesHeap[pos] = statesHeap[parent];
                statesHeap[parent] = t;
                pos = parent;
            }
        }
        public static StateNode ShiftAndRelax(this List<StateNode> statesHeap)
        {
            int lastIndex = statesHeap.Count - 1;
            var result = statesHeap[0];
            statesHeap[0] = statesHeap[lastIndex];
            statesHeap[lastIndex] = result;

            int parent = 0;
            while (true)
            {
                var left = parent * 2 + 1;
                var right = left + 1;
                if (left >= lastIndex) break;
                if (right == lastIndex)
                {
                    if (statesHeap[parent].CompareTo(statesHeap[left]) > 0)
                    {
                        var t = statesHeap[parent];
                        statesHeap[parent] = statesHeap[left];
                        statesHeap[left] = t;
                    }
                    break;
                }

                var better = statesHeap[left].CompareTo(statesHeap[right]) < 0 ? left : right;
                if (statesHeap[parent].CompareTo(statesHeap[better]) <= 0) break;

                var t1 = statesHeap[parent];
                statesHeap[parent] = statesHeap[better];
                statesHeap[better] = t1;
                parent = better;
            }

            statesHeap.RemoveAt(lastIndex);
            return result;
        }
    }
}
