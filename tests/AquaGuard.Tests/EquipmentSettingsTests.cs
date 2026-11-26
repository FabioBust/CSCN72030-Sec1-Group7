using System;
using System.Collections.Generic;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Devices.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class EquipmentSettingsTests
    {
        private EquipmentController controller = null!;

        [TestInitialize]
        public void Setup()
        {
            controller = new EquipmentController();
        }

        [TestMethod]
        public void SetSetting_Pump_ShouldUpdateSpeed()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            controller.SetSetting(pump, 75);

            Assert.AreEqual(75, pump.SpeedPercentage);
        }

        [TestMethod]
        public void SetSetting_Aerator_ShouldUpdateOutput()
        {
            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            controller.SetSetting(aerator, 60);

            Assert.AreEqual(60, aerator.OutputPercentage);
            Assert.IsTrue(aerator.IsOn);
        }

        [TestMethod]
        public void SetSetting_PumpZero_ShouldBeValid()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            controller.SetSetting(pump, 0);

            Assert.AreEqual(0, pump.SpeedPercentage);
        }

        [TestMethod]
        public void SetSetting_Pump100_ShouldBeValid()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            controller.SetSetting(pump, 100);

            Assert.AreEqual(100, pump.SpeedPercentage);
        }

        [TestMethod]
        public void SetSetting_AeratorZero_ShouldTurnOff()
        {
            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            aerator.TurnOn();

            controller.SetSetting(aerator, 0);

            Assert.AreEqual(0, aerator.OutputPercentage);
            Assert.IsFalse(aerator.IsOn);
        }

        [TestMethod]
        public void SetSetting_Aerator100_ShouldBeValid()
        {
            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            controller.SetSetting(aerator, 100);

            Assert.AreEqual(100, aerator.OutputPercentage);
            Assert.IsTrue(aerator.IsOn);
        }

        [TestMethod]
        public void SetSetting_Negative_ShouldThrowException()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => controller.SetSetting(pump, -1));
        }

        [TestMethod]
        public void SetSetting_Above100_ShouldThrowException()
        {
            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => controller.SetSetting(aerator, 101));
        }

        [TestMethod]
        public void SetSetting_Filter_ShouldThrowException()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            Assert.Throws<ArgumentException>(
                () => controller.SetSetting(filter, 50));
        }

        [TestMethod]
        public void SetSetting_NullDevice_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(
                () => controller.SetSetting(null!, 50));
        }

        [TestMethod]
        public void SetSetting_ShouldUpdateMonitoredStatus()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var monitor = new EquipmentStatusMonitor();

            controller.SetSetting(pump, 85);

            var status = monitor.GetStatus(pump);

            Assert.AreEqual<int?>(
                85, status.SettingPercentage);
        }

        [TestMethod]
        public void SetSetting_MultipleDevices_ShouldRemainIndependent()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            controller.SetSetting(pump, 75);
            controller.SetSetting(aerator, 40);

            Assert.AreEqual(75, pump.SpeedPercentage);
            Assert.AreEqual(40, aerator.OutputPercentage);
        }
    }
}

