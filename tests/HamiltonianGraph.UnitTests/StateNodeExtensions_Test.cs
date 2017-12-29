using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HamiltonianGraph.UnitTests
{
    [TestClass]
    public class StateTreeExtensions_Test
    {
        [TestMethod]
        public void AddAndSiftUp()
        {
            var states = new List<StateNode>();
            states.AddAndSiftUp(new StateNode { fine = 3 });
            states.AddAndSiftUp(new StateNode { fine = 12 });
            Assert.AreEqual(3, states[0].fine);

            states.AddAndSiftUp(new StateNode { fine = 15 });
            states.AddAndSiftUp(new StateNode { fine = 7 });
            states.AddAndSiftUp(new StateNode { fine = 8 });
            Assert.AreEqual(3, states[0].fine);

            states.AddAndSiftUp(new StateNode { fine = 2 });
            Assert.AreEqual(2, states[0].fine);
            states.AddAndSiftUp(new StateNode { fine = 1 });
            Assert.AreEqual(1, states[0].fine);
        }

        [TestMethod]
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
                Assert.AreEqual(fine, states.ShiftAndRelax().fine);
            }
        }
    }
}
