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
            return GraphUtil.ShortestRoute(weights, cycles);
        }

        static internal List<int[]>[,] Sum(List<int[]>[,] a, List<int[]>[,] b, int n)
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

        void GeneratePaths(int n)
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
    }
}
