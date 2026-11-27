using System;
using System.Collections.Generic;
using System.Text;
using AquaGuard.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class AlarmIntegrationTests
    {
        private AlarmIntegrationService service = null!;

        [TestInitialize]
        public void Setup()
        {
            service = new AlarmIntegrationService();

            service.SetThreshold(
                "H001", "TEMP001", 24, 28);
        }

        [TestMethod]
        public void NormalReading_ShouldNotRaiseAlert()
        {
            var status = service.ProcessReading(
                "H001", "TEMP001", 26);

            Assert.AreEqual(RangeStatus.Normal, status);
            Assert.AreEqual(0, service.ActiveAlerts.Count);
        }

        [TestMethod]
        public void LowReading_ShouldRaiseAlert()
        {
            var status = service.ProcessReading(
                "H001", "TEMP001", 20);

            Assert.AreEqual(RangeStatus.TooLow, status);
            Assert.AreEqual(1, service.ActiveAlerts.Count);
        }

        [TestMethod]
        public void HighReading_ShouldRaiseAlert()
        {
            var status = service.ProcessReading(
                "H001", "TEMP001", 32);

            Assert.AreEqual(RangeStatus.TooHigh, status);
            Assert.AreEqual(1, service.ActiveAlerts.Count);
        }

        [TestMethod]
        public void MissingReading_ShouldRaiseAlert()
        {
            var status = service.ProcessReading(
                "H001", "TEMP001", null);

            Assert.AreEqual(RangeStatus.NoData, status);
            Assert.AreEqual(1, service.ActiveAlerts.Count);
        }

        [TestMethod]
        public void ReadingReturnsToNormal_ShouldClearAlert()
        {
            service.ProcessReading(
                "H001", "TEMP001", 32);

            service.ProcessReading(
                "H001", "TEMP001", 26);

            Assert.AreEqual(0, service.ActiveAlerts.Count);
        }

        [TestMethod]
        public void RepeatedUnsafeReading_ShouldNotDuplicateAlert()
        {
            service.ProcessReading(
                "H001", "TEMP001", 32);

            service.ProcessReading(
                "H001", "TEMP001", 33);

            Assert.AreEqual(1, service.ActiveAlerts.Count);
        }

        [TestMethod]
        public void UpdatedThreshold_ShouldUseNewRange()
        {
            service.SetThreshold(
                "H001", "TEMP001", 20, 30);

            var status = service.ProcessReading(
                "H001", "TEMP001", 29);

            Assert.AreEqual(RangeStatus.Normal, status);
            Assert.AreEqual(0, service.ActiveAlerts.Count);
        }

        [TestMethod]
        public void MultipleSensors_ShouldHaveIndependentAlerts()
        {
            service.SetThreshold(
                "H001", "PH001", 7, 8.5);

            service.ProcessReading(
                "H001", "TEMP001", 32);

            service.ProcessReading(
                "H001", "PH001", 6);

            Assert.AreEqual(2, service.ActiveAlerts.Count);

            service.ProcessReading(
                "H001", "TEMP001", 26);

            Assert.AreEqual(1, service.ActiveAlerts.Count);
            Assert.AreEqual(
                "PH001", service.ActiveAlerts[0].DeviceId);
        }

        [TestMethod]
        public void DifferentHabitats_ShouldRemainIndependent()
        {
            service.SetThreshold(
                "H002", "TEMP001", 20, 25);

            var first = service.ProcessReading(
                "H001", "TEMP001", 27);

            var second = service.ProcessReading(
                "H002", "TEMP001", 27);

            Assert.AreEqual(RangeStatus.Normal, first);
            Assert.AreEqual(RangeStatus.TooHigh, second);

            Assert.AreEqual(1, service.ActiveAlerts.Count);
            Assert.AreEqual(
                "H002", service.ActiveAlerts[0].HabitatId);
        }

        [TestMethod]
        public void MissingThreshold_ShouldThrowException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                service.ProcessReading(
                    "H001", "UNKNOWN", 25));
        }
    }
}
