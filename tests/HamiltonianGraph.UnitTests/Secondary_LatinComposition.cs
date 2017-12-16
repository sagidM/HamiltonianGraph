﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HamiltonianGraph.UnitTests
{
    [TestClass]
    public class Secondary_LatinComposition
    {

        [TestMethod]
        public void MergePathsTest()
        {
            var actual = LatinComposition.MergePaths(new int[] { 1, 2, 3 }, new int[] { 3, 4, 5 });
            var expected = new int[] { 1, 2, 3, 4, 5 };

            Assert.IsTrue(actual.SequenceEqual(expected));
        }

        [TestMethod]
        public void SumTest()
        {
            const int n = 8;
            var a = new List<int[]>[n, n];
            GeneratePaths(a, 0, 1);
            GeneratePaths(a, 0, 7);
            GeneratePaths(a, 1, 0);
            GeneratePaths(a, 1, 2);
            GeneratePaths(a, 1, 3);
            GeneratePaths(a, 2, 3);
            GeneratePaths(a, 2, 5);
            GeneratePaths(a, 3, 2);
            GeneratePaths(a, 3, 4);
            GeneratePaths(a, 4, 2);
            GeneratePaths(a, 4, 6);
            GeneratePaths(a, 5, 0);
            GeneratePaths(a, 5, 1);
            GeneratePaths(a, 5, 4);
            GeneratePaths(a, 6, 5);
            GeneratePaths(a, 6, 7);
            GeneratePaths(a, 7, 0);
            GeneratePaths(a, 7, 1);
            GeneratePaths(a, 7, 2);
            GeneratePaths(a, 7, 3);

            var e = new List<int[]>[n, n];
            GeneratePaths(e, 0, 7, 1);
            GeneratePaths(e, 0, 1, 2);
            GeneratePaths(e, 0, 7, 2);
            GeneratePaths(e, 0, 1, 3);
            GeneratePaths(e, 0, 7, 3);
            GeneratePaths(e, 1, 3, 2);
            GeneratePaths(e, 1, 2, 3);
            GeneratePaths(e, 1, 3, 4);
            GeneratePaths(e, 1, 2, 5);
            GeneratePaths(e, 1, 0, 7);
            GeneratePaths(e, 2, 5, 0);
            GeneratePaths(e, 2, 5, 1);
            GeneratePaths(e, 2, 3, 4);
            GeneratePaths(e, 2, 5, 4);
            GeneratePaths(e, 3, 4, 2);
            GeneratePaths(e, 3, 2, 5);
            GeneratePaths(e, 3, 4, 6);
            GeneratePaths(e, 4, 2, 3);
            GeneratePaths(e, 4, 2, 5);
            GeneratePaths(e, 4, 6, 5);
            GeneratePaths(e, 4, 6, 7);
            GeneratePaths(e, 5, 1, 0);
            GeneratePaths(e, 5, 0, 1);
            GeneratePaths(e, 5, 1, 2);
            GeneratePaths(e, 5, 4, 2);
            GeneratePaths(e, 5, 1, 3);
            GeneratePaths(e, 5, 4, 6);
            GeneratePaths(e, 5, 0, 7);
            GeneratePaths(e, 6, 5, 0);
            GeneratePaths(e, 6, 7, 0);
            GeneratePaths(e, 6, 7, 1);
            GeneratePaths(e, 6, 5, 1);
            GeneratePaths(e, 6, 7, 2);
            GeneratePaths(e, 6, 7, 3);
            GeneratePaths(e, 6, 5, 4);
            GeneratePaths(e, 7, 1, 0);
            GeneratePaths(e, 7, 0, 1);
            GeneratePaths(e, 7, 1, 2);
            GeneratePaths(e, 7, 3, 2);
            GeneratePaths(e, 7, 1, 3);
            GeneratePaths(e, 7, 2, 3);
            GeneratePaths(e, 7, 3, 4);
            GeneratePaths(e, 7, 2, 5);

            var actual = LatinComposition.Sum(a, a, n);
            var en = e.GetEnumerator();
            foreach (var item in actual)
            {
                en.MoveNext();
                if (item == en.Current) continue;
                var cur = (List<int[]>)en.Current;
                for (int i = 0; i < cur.Count; i++)
                {
                    cur[i].SequenceEqual(item[i]);
                }
            }
        }

        private static void GeneratePaths(List<int[]>[,] a, params int[] v)
        {
            if (a[v[0], v[v.Length - 1]] == null)
                a[v[0], v[v.Length - 1]] = new List<int[]>() { v };
            else
                a[v[0], v[v.Length - 1]].Add(v);
        }

        private static void GeneratePaths(List<int[]>[,] a, int i, int j)
        {
            a[i, j] = new List<int[]> { new int[] { i, j } };
        }
    }
}