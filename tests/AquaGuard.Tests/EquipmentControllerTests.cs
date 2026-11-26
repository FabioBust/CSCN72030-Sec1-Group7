using System;
using System.Collections.Generic;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Devices.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class EquipmentControllerTests
    {
        private EquipmentController controller = null!;

        [TestInitialize]
        public void Setup()
        {
            controller = new EquipmentController();
        }

        [TestMethod]
        public void TurnOn_Pump_ShouldTurnOn()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            controller.TurnOn(pump);

            Assert.IsTrue(pump.IsOn);
        }

        [TestMethod]
        public void TurnOff_Pump_ShouldTurnOff()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            controller.TurnOn(pump);
            controller.TurnOff(pump);

            Assert.IsFalse(pump.IsOn);
        }

        [TestMethod]
        public void TurnOn_Filter_ShouldTurnOn()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            controller.TurnOn(filter);

            Assert.IsTrue(filter.IsOn);
        }

        [TestMethod]
        public void TurnOff_Filter_ShouldTurnOff()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            controller.TurnOn(filter);
            controller.TurnOff(filter);

            Assert.IsFalse(filter.IsOn);
        }

        [TestMethod]
        public void TurnOn_Aerator_ShouldTurnOn()
        {
            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            controller.TurnOn(aerator);

            Assert.IsTrue(aerator.IsOn);
            Assert.AreEqual(100, aerator.OutputPercentage);
        }

        [TestMethod]
        public void TurnOff_Aerator_ShouldTurnOff()
        {
            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            controller.TurnOn(aerator);
            controller.TurnOff(aerator);

            Assert.IsFalse(aerator.IsOn);
            Assert.AreEqual(0, aerator.OutputPercentage);
        }

        [TestMethod]
        public void TurnOn_MultipleDevices_ShouldRemainIndependent()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            controller.TurnOn(pump);

            Assert.IsTrue(pump.IsOn);
            Assert.IsFalse(filter.IsOn);
        }

        [TestMethod]
        public void TurnOff_Pump_ShouldPreserveSpeed()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            pump.SetSpeed(75);

            controller.TurnOn(pump);
            controller.TurnOff(pump);

            Assert.IsFalse(pump.IsOn);
            Assert.AreEqual(75, pump.SpeedPercentage);
        }

        [TestMethod]
        public void TurnOn_NullDevice_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(
                () => controller.TurnOn(null!));
        }

        [TestMethod]
        public void TurnOff_NullDevice_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(
                () => controller.TurnOff(null!));
        }

        [TestMethod]
        public void TurnOn_ShouldUpdateMonitoredStatus()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var monitor = new EquipmentStatusMonitor();

            controller.TurnOn(pump);

            var status = monitor.GetStatus(pump);

            Assert.IsTrue(status.IsOn);
        }

        [TestMethod]
        public void TurnOff_ShouldUpdateMonitoredStatus()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var monitor = new EquipmentStatusMonitor();

            controller.TurnOn(pump);
            controller.TurnOff(pump);

            var status = monitor.GetStatus(pump);

            Assert.IsFalse(status.IsOn);
        }
    }
}

