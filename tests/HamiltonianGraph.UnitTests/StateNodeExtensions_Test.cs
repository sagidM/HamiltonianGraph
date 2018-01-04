using System;
using System.Collections.Generic;
using Xunit;

namespace HamiltonianGraph.UnitTests
{
    public class StateTreeExtensions_Test
    {
        [Fact(DisplayName = "[StateNode] Add")]
        public void AddAndSiftUp()
        {
            var states = new List<StateNode>();
            states.AddAndSiftUp(new StateNode { fine = 3 });
            states.AddAndSiftUp(new StateNode { fine = 12 });
            Assert.Equal(3, states[0].fine);

            states.AddAndSiftUp(new StateNode { fine = 15 });
            states.AddAndSiftUp(new StateNode { fine = 7 });
            states.AddAndSiftUp(new StateNode { fine = 8 });
            Assert.Equal(3, states[0].fine);

            states.AddAndSiftUp(new StateNode { fine = 2 });
            Assert.Equal(2, states[0].fine);
            states.AddAndSiftUp(new StateNode { fine = 1 });
            Assert.Equal(1, states[0].fine);
        }

        [Fact(DisplayName = "[StateNode] Relax")]
        public void ShiftAndRelax()
        {
            var states = new List<StateNode>();
            int[] fines = new[] { 3, 66, 3, 78, 3 };
            var mock = new int[0,0];
            foreach (var fine in fines)
            {
                states.AddAndSiftUp(new StateNode { fine = fine, graph = mock });
            }
            Array.Sort(fines);

            foreach (var fine in fines)
            {
                Assert.Equal(fine, states.ShiftAndRelax().fine);
            }
        }
    }
}
