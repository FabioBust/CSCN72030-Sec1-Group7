using System;
using System.Collections.Generic;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Devices.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class EquipmentStatusMonitorTests
    {
        private EquipmentStatusMonitor monitor = null!;

        [TestInitialize]
        public void Setup()
        {
            monitor = new EquipmentStatusMonitor();
        }

        [TestMethod]
        public void GetStatus_PumpOff_ShouldReturnFalse()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var status = monitor.GetStatus(pump);

            Assert.IsFalse(status.IsOn);
        }

        [TestMethod]
        public void GetStatus_PumpOn_ShouldReturnTrue()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            pump.TurnOn();

            var status = monitor.GetStatus(pump);

            Assert.IsTrue(status.IsOn);
        }

        [TestMethod]
        public void GetStatus_PumpSpeed_ShouldReturnCorrectSpeed()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            pump.TurnOn();
            pump.SetSpeed(75);

            var status = monitor.GetStatus(pump);

            Assert.AreEqual<int?>(
                75, status.SettingPercentage);
        }

        [TestMethod]
        public void GetStatus_FilterOn_ShouldReturnTrue()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            filter.TurnOn();

            var status = monitor.GetStatus(filter);

            Assert.IsTrue(status.IsOn);
        }

        [TestMethod]
        public void GetStatus_FilterOff_ShouldReturnFalse()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            filter.TurnOn();
            filter.TurnOff();

            var status = monitor.GetStatus(filter);

            Assert.IsFalse(status.IsOn);
            Assert.IsNull(status.SettingPercentage);
        }

        [TestMethod]
        public void GetStatus_Aerator_ShouldReturnCorrectOutput()
        {
            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            aerator.SetOutput(60);

            var status = monitor.GetStatus(aerator);

            Assert.IsTrue(status.IsOn);
            Assert.AreEqual<int?>(
                60, status.SettingPercentage);
        }

        [TestMethod]
        public void GetStatus_ShouldDetectStatusChanges()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            pump.TurnOn();

            Assert.IsTrue(
                monitor.GetStatus(pump).IsOn);

            pump.TurnOff();

            Assert.IsFalse(
                monitor.GetStatus(pump).IsOn);
        }

        [TestMethod]
        public void GetStatus_ShouldReturnDeviceDetails()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var status = monitor.GetStatus(pump);

            Assert.AreEqual("P001", status.DeviceId);
            Assert.AreEqual("H001", status.HabitatId);
            Assert.AreEqual("Main Pump", status.Name);
        }

        [TestMethod]
        public void GetStatus_MultipleDevices_ShouldTrackSeparately()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            pump.TurnOn();

            Assert.IsTrue(
                monitor.GetStatus(pump).IsOn);

            Assert.IsFalse(
                monitor.GetStatus(filter).IsOn);
        }

        [TestMethod]
        public void GetStatus_DifferentHabitats_ShouldReturnCorrectHabitat()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var aerator = new Aerator(
                "A001", "H002", "Main Aerator");

            Assert.AreEqual(
                "H001",
                monitor.GetStatus(pump).HabitatId);

            Assert.AreEqual(
                "H002",
                monitor.GetStatus(aerator).HabitatId);
        }
    }
}

