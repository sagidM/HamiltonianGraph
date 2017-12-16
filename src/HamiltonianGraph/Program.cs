using HamiltonianGraph.Utils;
using System;
using System.Linq;

namespace HamiltonianGraph
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var g =
@"5
- 2 0 1 1
3 - 0 4 4
0 1 - 4 1
0 4 4 - 3
1 1 4 3 -
";
            int?[,] weights = GraphUtil.FromMatrixFormat(g);
            var paths = new int[][] 
            {
                new LatinComposition(weights).GetShortestHamiltonianCycle(),
                new BranchAndBound(weights).GetShortestHamiltonianCycle(),
            };
            foreach (var path in paths)
            {
                Console.WriteLine(string.Join("", path.Select(s => (char)(s + '0'))));
            }
        }
    }
}
