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
            var states = new List<StateTree>();
            states.AddAndSiftUp(new StateTree { fine = 3 });
            states.AddAndSiftUp(new StateTree { fine = 12 });
            Assert.AreEqual(3, states[0].fine);

            states.AddAndSiftUp(new StateTree { fine = 15 });
            states.AddAndSiftUp(new StateTree { fine = 7 });
            states.AddAndSiftUp(new StateTree { fine = 8 });
            Assert.AreEqual(3, states[0].fine);

            states.AddAndSiftUp(new StateTree { fine = 2 });
            Assert.AreEqual(2, states[0].fine);
            states.AddAndSiftUp(new StateTree { fine = 1 });
            Assert.AreEqual(1, states[0].fine);
        }

        [TestMethod]
        public void ShiftAndRelax()
        {
            var states = new List<StateTree>();
            int[] fines = new[] { 3, 66, 3, 78, 3 };
            foreach (var fine in fines)
            {
                states.AddAndSiftUp(new StateTree { fine = fine });
            }
            Array.Sort(fines);

            foreach (var fine in fines)
            {
                Assert.AreEqual(fine, states.ShiftAndRelax().fine);
            }
        }
    }
}
