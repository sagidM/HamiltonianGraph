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
@"3
- 1 -
- - 1
1 - -
";
            int?[,] weights = GraphUtil.FromMatrixFormat(g);
            var paths = new int[][] { new BranchAndBound(weights).GetShortestHamiltonianCycle() };
            foreach (var path in paths)
            {
                Console.WriteLine(string.Join("", path.Select(s => (char)(s + '1'))));
            }
        }
    }
}
