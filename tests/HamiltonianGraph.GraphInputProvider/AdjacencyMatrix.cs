using HamiltonianGraph.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HamiltonianGraph.GraphInputProvider
{
    public class AdjacencyMatrix
    {
        public int?[,] Weights { get; private set; }
        public List<int[]> AllPaths { get; private set; }
        public int ShortestPathIndex { get; private set; }
        public int[] ShortestPath => AllPaths[ShortestPathIndex];
        public int ShortestPathDistance => PathDistance(ShortestPath, Weights);

        public static AdjacencyMatrix GetGraph(int inputNumber)
        {
            // get weights
            var lines = ReadLinesFromBuiltInInput($"{CurrentNamespace}.inputs.in{inputNumber}.txt");
            var weights = GraphUtil.FromMatrixFormat(lines);

            // get paths
            lines = ReadLinesFromBuiltInInput($"{CurrentNamespace}.inputs.in{inputNumber}_res.txt");
            var (n1, shortestPathIndex) = lines[0].Split(' ').Deconstruct(int.Parse);
            var paths = new List<int[]>(n1);
            for (int i = 0; i < n1; i++)
            {
                var path = lines[i + 1].Split(' ').Select(int.Parse).ToArray();
                paths.Add(path);
            }

            return new AdjacencyMatrix
            {
                Weights = weights,
                AllPaths = paths,
                ShortestPathIndex = shortestPathIndex,
            };
        }

        public static int PathDistance(IList<int> path, int?[,] weights)
        {
            int distance = 0;
            for (int i = 1; i < path.Count; i++)
                if (weights[path[i - 1], path[i]] != null)
                    distance += weights[path[i - 1], path[i]].Value;
            return distance;
        }

        private static string[] ReadLinesFromBuiltInInput(string name)
        {
            string text;
            using (var stream = ThisAssembly.GetManifestResourceStream(name))
            using (var streamReader = new StreamReader(stream))
            {
                text = streamReader.ReadToEnd();
            }
            return text.Split(NewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        private static readonly char[] NewLineSeparators = new[] { '\r', '\n' };
        private static readonly Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        private static readonly string CurrentNamespace = typeof(AdjacencyMatrix).Namespace;
    }
    internal static class Extensions
    {
        public static (T, T) Deconstruct<T>(this IList<T> values)
        {
            return (values[0], values[1]);
        }
        public static (TResult, TResult) Deconstruct<T, TResult>(this IList<T> values, Func<T, TResult> parse)
        {
            return (parse(values[0]), parse(values[1]));
        }
    }
}
