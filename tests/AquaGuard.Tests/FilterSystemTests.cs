using AquaGuard.Devices.Equipment;
using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Tests
{
    [TestClass]
    public class FilterSystemTests
    {
        [TestMethod]
        public void Constructor_ShouldCreateFilterAsOff()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            Assert.IsFalse(filter.IsOn);
        }

        [TestMethod]
        public void TurnOn_ShouldSetFilterOn()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            filter.TurnOn();

            Assert.IsTrue(filter.IsOn);
        }

        [TestMethod]
        public void TurnOff_ShouldSetFilterOff()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            filter.TurnOn();
            filter.TurnOff();

            Assert.IsFalse(filter.IsOn);
        }

        [TestMethod]
        public void TurnOn_MultipleTimes_ShouldRemainOn()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            filter.TurnOn();
            filter.TurnOn();

            Assert.IsTrue(filter.IsOn);
        }

        [TestMethod]
        public void TurnOff_MultipleTimes_ShouldRemainOff()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            filter.TurnOff();
            filter.TurnOff();

            Assert.IsFalse(filter.IsOn);
        }
    }
}
