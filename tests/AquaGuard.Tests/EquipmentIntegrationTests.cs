using System;
using System.Collections.Generic;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Devices.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class EquipmentIntegrationTests
    {
        private EquipmentIntegrationService service = null!;

        [TestInitialize]
        public void Setup()
        {
            service = new EquipmentIntegrationService();
        }

        [TestMethod]
        public void Pump_TurnOnAndAdjustSpeed_ShouldReturnUpdatedStatus()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            service.TurnOn(pump);

            var status = service.SetSetting(pump, 75);

            Assert.IsTrue(status.IsOn);
            Assert.AreEqual<int?>(75, status.SettingPercentage);
        }

        [TestMethod]
        public void Pump_TurnOff_ShouldPreserveSpeed()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            service.TurnOn(pump);
            service.SetSetting(pump, 80);

            var status = service.TurnOff(pump);

            Assert.IsFalse(status.IsOn);
            Assert.AreEqual<int?>(80, status.SettingPercentage);
        }

        [TestMethod]
        public void Filter_TurnOnAndOff_ShouldUpdateStatus()
        {
            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            var onStatus = service.TurnOn(filter);

            Assert.IsTrue(onStatus.IsOn);

            var offStatus = service.TurnOff(filter);

            Assert.IsFalse(offStatus.IsOn);
            Assert.IsNull(offStatus.SettingPercentage);
        }

        [TestMethod]
        public void Aerator_AdjustOutput_ShouldUpdateStatus()
        {
            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            var status = service.SetSetting(aerator, 65);

            Assert.IsTrue(status.IsOn);
            Assert.AreEqual<int?>(65, status.SettingPercentage);
        }

        [TestMethod]
        public void Aerator_ZeroOutput_ShouldTurnOff()
        {
            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            service.TurnOn(aerator);

            var status = service.SetSetting(aerator, 0);

            Assert.IsFalse(status.IsOn);
            Assert.AreEqual<int?>(0, status.SettingPercentage);
        }

        [TestMethod]
        public void MultipleEquipment_ShouldRemainIndependent()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            var filter = new FilterSystem(
                "F001", "H001", "Main Filter");

            var aerator = new Aerator(
                "A001", "H001", "Main Aerator");

            service.TurnOn(pump);
            service.SetSetting(pump, 70);

            service.TurnOn(filter);

            service.SetSetting(aerator, 40);
            service.TurnOff(filter);

            Assert.IsTrue(service.GetStatus(pump).IsOn);
            Assert.IsFalse(service.GetStatus(filter).IsOn);
            Assert.IsTrue(service.GetStatus(aerator).IsOn);

            Assert.AreEqual<int?>(
                70, service.GetStatus(pump).SettingPercentage);

            Assert.AreEqual<int?>(
                40, service.GetStatus(aerator).SettingPercentage);
        }

        [TestMethod]
        public void InvalidSetting_ShouldPreservePreviousSetting()
        {
            var pump = new CirculationPump(
                "P001", "H001", "Main Pump");

            service.SetSetting(pump, 50);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => service.SetSetting(pump, 150));

            var status = service.GetStatus(pump);

            Assert.AreEqual<int?>(50, status.SettingPercentage);
        }

        [TestMethod]
        public void DifferentHabitats_ShouldRemainIndependent()
        {
            var firstPump = new CirculationPump(
                "P001", "H001", "First Pump");

            var secondPump = new CirculationPump(
                "P001", "H002", "Second Pump");

            service.TurnOn(firstPump);
            service.SetSetting(firstPump, 80);

            service.SetSetting(secondPump, 30);

            var firstStatus = service.GetStatus(firstPump);
            var secondStatus = service.GetStatus(secondPump);

            Assert.AreEqual("H001", firstStatus.HabitatId);
            Assert.AreEqual("H002", secondStatus.HabitatId);

            Assert.IsTrue(firstStatus.IsOn);
            Assert.IsFalse(secondStatus.IsOn);

            Assert.AreEqual<int?>(
                80, firstStatus.SettingPercentage);

            Assert.AreEqual<int?>(
                30, secondStatus.SettingPercentage);
        }
    }
}
