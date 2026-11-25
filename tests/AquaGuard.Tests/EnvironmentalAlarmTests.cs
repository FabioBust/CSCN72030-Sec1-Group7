using System;
using System.Collections.Generic;
using System.Text;

using AquaGuard.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class EnvironmentalAlarmTests
    {
        private AlertManager manager = null!;
        private EnvironmentalAlarmService service = null!;

        [TestInitialize]
        public void Setup()
        {
            manager = new AlertManager();

            service = new EnvironmentalAlarmService(
                new SafeRangeEngine(),
                manager);
        }

        [TestMethod]
        public void ProcessReading_Normal_ShouldNotRaiseAlert()
        {
            service.ProcessReading(
                "H001", "TEMP001", 26, 24, 28);

            Assert.AreEqual(0, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ProcessReading_TooHigh_ShouldRaiseAlert()
        {
            service.ProcessReading(
                "H001", "TEMP001", 30, 24, 28);

            Assert.AreEqual(1, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ProcessReading_TooLow_ShouldRaiseAlert()
        {
            service.ProcessReading(
                "H001", "TEMP001", 20, 24, 28);

            Assert.AreEqual(1, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ProcessReading_ReturnToNormal_ShouldClearAlert()
        {
            service.ProcessReading(
                "H001", "TEMP001", 30, 24, 28);

            service.ProcessReading(
                "H001", "TEMP001", 26, 24, 28);

            Assert.AreEqual(0, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ProcessReading_RepeatedUnsafe_ShouldNotDuplicate()
        {
            service.ProcessReading(
                "H001", "TEMP001", 30, 24, 28);

            service.ProcessReading(
                "H001", "TEMP001", 31, 24, 28);

            Assert.AreEqual(1, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ProcessReading_NoData_ShouldRaiseAlert()
        {
            service.ProcessReading(
                "H001", "TEMP001", null, 24, 28);

            Assert.AreEqual(1, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ProcessReading_MultipleSensors_ShouldTrackSeparately()
        {
            service.ProcessReading(
                "H001", "TEMP001", 30, 24, 28);

            service.ProcessReading(
                "H001", "PH001", 5, 7, 8.5);

            Assert.AreEqual(2, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ProcessReading_ClearOne_ShouldKeepOtherAlert()
        {
            service.ProcessReading(
                "H001", "TEMP001", 30, 24, 28);

            service.ProcessReading(
                "H001", "PH001", 5, 7, 8.5);

            service.ProcessReading(
                "H001", "TEMP001", 26, 24, 28);

            Assert.AreEqual(1, manager.ActiveAlerts.Count);

            Assert.AreEqual(
                "PH001",
                manager.ActiveAlerts[0].DeviceId);
        }
    }
}

