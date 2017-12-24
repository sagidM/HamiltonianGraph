using HamiltonianGraph.Utils;
using System;
using System.IO;
using System.Linq;

namespace HamiltonianGraph
{
    // https://habrahabr.ru/post/246437/
    internal class Program
    {
        static void Main(string[] args)
        {
            var g =
@"8
- 6 5 4 5 6 0 0
6 - 0 6 3 2 3 2
1 0 - 6 2 0 7 3
3 2 1 - 7 4 2 6
7 2 2 1 - 4 3 4
2 0 2 6 7 - 6 3
1 0 0 7 3 0 - 0
4 4 6 0 6 6 3 -";
            g =
@"7
- 0 2 4 1 5 1
5 - 0 0 4 2 2
5 4 - 1 0 4 1
0 2 3 - 2 0 6
2 2 1 2 - 4 0
5 2 4 0 3 - 6
2 3 5 0 0 4 -
";
            int?[,] weights = GraphUtil.FromMatrixFormat(g);
            var q = new LatinComposition(weights).GetShortestHamiltonianCycle();
            Console.WriteLine(string.Join(", ", q));
            var cycles = new int[][]
            {
                new LatinComposition(weights).GetShortestHamiltonianCycle(),
                new BranchAndBound(weights).GetShortestHamiltonianCycle(),
            };
            foreach (var cycle in cycles)
            {
                int d = GraphUtil.RouteDistance(weights, cycle);
                Console.WriteLine(string.Join("", cycle.Select(s => (char)(s + '0'))) + " => " + d);
            }
        }
    }
}
