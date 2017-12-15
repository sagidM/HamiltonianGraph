using System;
using System.IO;

namespace HamiltonianGraph.Utils
{
    internal static class GraphUtil
    {
        public static int?[,] FromMatrixFormat(Stream matrixAsStream)
        {
            string text;
            using (var streamReader = new StreamReader(matrixAsStream))
            {
                text = streamReader.ReadToEnd();
            }
            return FromMatrixFormat(text);
        }
        public static int?[,] FromMatrixFormat(string matrixAsText)
        {
            var lines = matrixAsText.Split(NewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
            return FromMatrixFormat(lines);
        }
        public static int?[,] FromMatrixFormat(string[] matrixAsLines)
        {
            var n = int.Parse(matrixAsLines[0]);
            var weights = new int?[n, n];
            for (int i = 0; i < n; i++)
            {
                var numbers = matrixAsLines[i + 1].Split(' ');
                for (int j = 0; j < n; j++)
                    if (numbers[j][0] != '-')
                        weights[i, j] = int.Parse(numbers[j]);
            }
            return weights;
        }

        private static readonly char[] NewLineSeparators = new[] { '\r', '\n' };
    }
}
