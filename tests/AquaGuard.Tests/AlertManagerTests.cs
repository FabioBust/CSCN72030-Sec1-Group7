using System;
using System.Collections.Generic;
using System.Text;
using AquaGuard.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class AlertManagerTests
    {
        private AlertManager manager = null!;

        [TestInitialize]
        public void Setup()
        {
            manager = new AlertManager();
        }

        [TestMethod]
        public void Constructor_ShouldStartWithNoAlerts()
        {
            Assert.AreEqual(0, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void RaiseAlert_ShouldAddNewAlert()
        {
            manager.RaiseAlert(
                "H001",
                "TEMP001",
                "Temperature too high");

            Assert.AreEqual(1, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void RaiseAlert_ShouldStoreCorrectDetails()
        {
            manager.RaiseAlert(
                "H001",
                "TEMP001",
                "Temperature too high");

            var alert = manager.ActiveAlerts[0];

            Assert.AreEqual("H001", alert.HabitatId);
            Assert.AreEqual("TEMP001", alert.DeviceId);
            Assert.AreEqual(
                "Temperature too high",
                alert.Message);
        }

        [TestMethod]
        public void RaiseAlert_Duplicate_ShouldNotAddTwice()
        {
            manager.RaiseAlert(
                "H001",
                "TEMP001",
                "Temperature too high");

            manager.RaiseAlert(
                "H001",
                "TEMP001",
                "Temperature too high");

            Assert.AreEqual(1, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ClearAlert_ShouldRemoveExistingAlert()
        {
            manager.RaiseAlert(
                "H001",
                "TEMP001",
                "Temperature too high");

            manager.ClearAlert("H001", "TEMP001");

            Assert.AreEqual(0, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ClearAlert_NonExisting_ShouldNotThrow()
        {
            manager.ClearAlert("H001", "TEMP001");

            Assert.AreEqual(0, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void RaiseAlert_MultipleDevices_ShouldTrackSeparately()
        {
            manager.RaiseAlert(
                "H001",
                "TEMP001",
                "Temperature too high");

            manager.RaiseAlert(
                "H001",
                "PH001",
                "pH too low");

            Assert.AreEqual(2, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void RaiseAlert_SameDeviceDifferentHabitat_ShouldTrackSeparately()
        {
            manager.RaiseAlert(
                "H001",
                "TEMP001",
                "Temperature too high");

            manager.RaiseAlert(
                "H002",
                "TEMP001",
                "Temperature too high");

            Assert.AreEqual(2, manager.ActiveAlerts.Count);
        }

        [TestMethod]
        public void RaiseAlert_ShouldStoreTimestamp()
        {
            var before = DateTime.Now;

            manager.RaiseAlert(
                "H001",
                "TEMP001",
                "Temperature too high");

            var after = DateTime.Now;
            var alert = manager.ActiveAlerts[0];

            Assert.IsTrue(alert.Timestamp >= before);
            Assert.IsTrue(alert.Timestamp <= after);
        }
    }
}

