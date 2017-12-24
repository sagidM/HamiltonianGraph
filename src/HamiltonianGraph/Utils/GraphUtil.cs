using System;
using System.Collections.Generic;
using System.IO;

namespace HamiltonianGraph.Utils
{
    public static class GraphUtil
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


        public static int[] ShortestRoute(int?[,] weights, IList<int[]> routes)
        {
            var idx = ShortestRouteIndex(weights, routes);
            return routes[idx];
        }

        public static int ShortestRouteIndex(int?[,] weights, IList<int[]> routes)
        {
            int shortestIndex = -1;
            var shortestDistance = int.MaxValue;

            for (int i = 0; i < routes.Count; i++)
            {
                // to count the distance
                int distance = RouteDistance(weights, routes[i]);
                if (shortestDistance > distance)
                {
                    shortestDistance = distance;
                    shortestIndex = i;
                }
            }
            return shortestIndex;
        }

        public static int RouteDistance(int?[,] weights, IList<int> route)
        {
            return RouteDistance(weights, route, 0, route.Count);
        }

        public static int RouteDistance(int?[,] weights, IList<int> route, int index, int length)
        {
            int distance = 0;
            for (int i = index+1; i < index + length; i++)
                distance += weights[route[i - 1], route[i]].Value;
            return distance;
        }

        private static readonly char[] NewLineSeparators = new[] { '\r', '\n' };
    }
}
