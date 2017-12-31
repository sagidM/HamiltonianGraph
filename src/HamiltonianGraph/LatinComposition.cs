using HamiltonianGraph.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HamiltonianGraph
{
    public class LatinComposition
    {
        private Dictionary<int, List<int[]>[,]> pathsMatrixes;
        private readonly int n;
        private readonly int?[,] weights;

        public LatinComposition(int?[,] weights)
        {
            n = weights.GetLength(0);
            if (weights.GetLength(1) != n)
                throw new ArgumentException("Dimension of matrix must be the same: NxN", nameof(weights));

            this.weights = weights;
        }

        public IList<int[]> GetAllHamiltorianCycles()
        {
            var one = new List<int[]>[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (weights[i, j] != null)
                        one[i, j] = new List<int[]> { new[] { i, j } };
            pathsMatrixes = new Dictionary<int, List<int[]>[,]> { [1] = one };

            var truncatedN = n / 2;
            var ceiledN = n - truncatedN;
            GeneratePaths(ceiledN);
            GeneratePaths(truncatedN);

            return GatherPaths(ceiledN, truncatedN);
        }

        public int[] GetShortestHamiltonianCycle()
        {
            var cycles = GetAllHamiltorianCycles();
            return cycles.Count == 0 ? null : GraphUtil.ShortestRoute(weights, cycles);
        }

        internal static List<int[]>[,] Sum(List<int[]>[,] a, List<int[]>[,] b, int n)
        {
            var result = new List<int[]>[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int ja = 0; ja < n; ja++)
                {
                    if (a[i, ja] == null) continue;
                    foreach (var aPath in a[i,ja])
                    {
                        for (int jb = 0; jb < n; jb++)
                        {
                            if (b[ja, jb] == null || i == jb) continue;
                            foreach (var bPath in b[ja, jb])
                            {
                                var isUnique = true;
                                // TODO: check performance
                                //for (int k = 1; k < bPath.Length; k++)
                                //{
                                //    var b0 = bPath[k];
                                //}
                                foreach (var b0 in bPath.Skip(1))
                                {
                                    if (Array.IndexOf(aPath, b0) < 0) continue;
                                    isUnique = false;
                                    break;
                                }
                                if (isUnique)
                                {
                                    if (result[i, jb] == null)
                                        result[i, jb] = new List<int[]>();
                                    result[i, jb].Add(MergePaths(aPath, bPath));
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        internal List<int[]> GatherPaths(int n1, int n2)
        {
            var paths = new List<int[]>();
            var m1 = pathsMatrixes[n1];
            var m2 = pathsMatrixes[n2];
            for (int head = 1; head < n; head++)
            {
                if (m1[0, head] == null) continue;
                foreach (var path1 in m1[0, head])
                {
                    var tail = path1[path1.Length - 1];
                    if (m2[tail, 0] == null) continue;
                    foreach (var path2 in m2[tail, 0])
                    {
                        var isUnique = true;
                        for (int k = 1; k < path2.Length-1; k++)
                        {
                            if (Array.IndexOf(path1, path2[k]) < 0) continue;
                            isUnique = false;
                            break;
                        }
                        if (isUnique)
                            paths.Add(MergePaths(path1, path2));
                    }
                }
            }
            return paths;
        }

        // [1,2,3] + [3,4,5] => [1,2,3,4,5]
        internal static int[] MergePaths(int[] aPath, int[] bPath)
        {
            var newPath = new int[aPath.Length + bPath.Length - 1];
            Array.Copy(aPath, newPath, aPath.Length);
            Array.Copy(bPath, 1, newPath, aPath.Length, bPath.Length - 1);
            return newPath;
        }

        private void GeneratePaths(int n)
        {
            if (n == 1 || pathsMatrixes.ContainsKey(n))
                return;

            var truncatedN = n / 2;
            int ceiledN = n - truncatedN;
            GeneratePaths(ceiledN);
            if (truncatedN << 1 != n)
                GeneratePaths(truncatedN);
            pathsMatrixes[n] = Sum(pathsMatrixes[ceiledN], pathsMatrixes[truncatedN], this.n);
        }


#if DEBUG
        private IList<int[]> Full()
        {
            var one = new List<int[]>[n, n];   // matrix
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (weights[i, j] == null) continue;
                    one[i, j] = new List<int[]> { new[] { i, j } };
                }
            }

            var m = one;   // matrix
            for (int step = 2; step < n; step++)
            {
                var t = new List<int[]>[n, n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (m[i, j] == null) continue;
                        foreach (var path in m[i, j])
                        {
                            var ib = path[path.Length - 1];
                            for (int jb = 0; jb < n; jb++)
                            {
                                if (weights[ib, jb] == null || Array.IndexOf(path, jb) >= 0) continue;
                                if (t[i, j] == null)
                                    t[i, j] = new List<int[]>();
                                var newPath = Concat(path, jb);
                                t[i, j].Add(newPath);
                            }
                        }
                    }
                }
                m = t;
            }
            var paths = new List<int[]>();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (m[i, j] == null) continue;
                    foreach (var path in m[i, j])
                    {
                        if (path[0] == 0 && weights[path[path.Length - 1], 0].HasValue)
                            paths.Add(Concat(path, 0));
                    }
                }
            }
            return paths;
        }
        static int[] Concat(int[] path, int v)
        {
            int len = path.Length;
            var res = new int[len + 1];
            Array.Copy(path, res, len);
            res[len] = v;
            return res;
        }

        // OutOfMemory
        private IList<int[]> FullUsingHashSet()
        {
            var one = new List<(int[] edges, HashSet<int> set)>[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (weights[i, j] == null) continue;
                    var edge = new[]{ i, j };
                    var set = new HashSet<int>();
                    for (int k = 0; k < n; k++)
                        if (k != i && k != j)
                            set.Add(k);

                    one[i, j] = new List<(int[], HashSet<int>)> { (edge, set) };
                }
            }

            var m = one;   // matrix
            for (int step = 2; step < n; step++)
            {
                var t = new List<(int[] edges, HashSet<int> set)>[n, n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (m[i, j] == null) continue;
                        foreach (var (edges, set) in m[i, j])
                        {
                            foreach (var s in set)
                            {
                                if (weights[edges[edges.Length-1], s] == null) continue;
                                var newEdges = Concat(edges, s);
                                var newSet = new HashSet<int>(set);
                                newSet.Remove(s);
                                if (t[i, j] == null)
                                    t[i, j] = new List<(int[] edges, HashSet<int> set)>();
                                t[i, j].Add((newEdges, newSet));
                            }
                        }
                    }
                }
                m = t;
            }

            var paths = new List<int[]>();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (m[i, j] == null) continue;
                    foreach (var (edges, set) in m[i, j])
                    {
                        if (edges[0] == 0 && weights[edges[n-1], 0].HasValue)
                        {
                            paths.Add(Concat(edges, 0));
                        }
                    }
                }
            }
            return paths;
        }

        private IList<int[]> AddByOne()
        {
            var one = new List<int[]>[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (weights[i, j] != null)
                        one[i, j] = new List<int[]> { new[] { i, j } };
            var res = one;
            for (int i = 2; i < n; i++)
            {
                res = Sum(res, one, n);
            }
            pathsMatrixes = new Dictionary<int, List<int[]>[,]> { [1] = one };
            pathsMatrixes[n] = res;
            return GatherPaths(n, 1);
        }
#endif
    }
}
