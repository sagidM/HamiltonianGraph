#if DEBUG
using HamiltonianGraph.Utils;
using System;
using System.Linq;

namespace HamiltonianGraph
{
    // https://habrahabr.ru/post/246437/
    internal class Program
    {
        ///////////////////////////////////////
        //  This file is only for DEBUGGING  //
        ///////////////////////////////////////
        static void Main(string[] args)
        {
            // Example

            var g =
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
            var cycles = new int[][]
            {
                new LatinComposition(weights).GetShortestHamiltonianCycle(),
                new BranchAndBound(weights).GetShortestHamiltonianCycle(),
            };

            foreach (var cycle in cycles)
            {
                // 4
                int distance = GraphUtil.RouteDistance(weights, cycle);
                // 1 2 3 5 7 6 4 1
                string cycleString = string.Join(" ", cycle.Select(vertex => (char)(vertex + '1')));

                Console.WriteLine("The distance = " + distance);
                Console.WriteLine("The path:\n" + cycleString);
                Console.WriteLine("\n");
            }
        }
    }
}
#endif
