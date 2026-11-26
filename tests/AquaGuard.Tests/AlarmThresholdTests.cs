using System;
using System.Collections.Generic;
using System.Text;
using AquaGuard.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class AlarmThresholdTests
    {
        private AlarmThresholdManager manager = null!;

        [TestInitialize]
        public void Setup()
        {
            manager = new AlarmThresholdManager();
        }

        [TestMethod]
        public void SetThreshold_ShouldStoreMinimumAndMaximum()
        {
            manager.SetThreshold("H001", "TEMP001", 24, 28);

            var threshold = manager.GetThreshold(
                "H001", "TEMP001");

            Assert.IsNotNull(threshold);
            Assert.AreEqual(24.0, threshold.Minimum);
            Assert.AreEqual(28.0, threshold.Maximum);
        }

        [TestMethod]
        public void SetThreshold_ShouldUpdateExistingThreshold()
        {
            manager.SetThreshold("H001", "TEMP001", 24, 28);
            manager.SetThreshold("H001", "TEMP001", 22, 30);

            var threshold = manager.GetThreshold(
                "H001", "TEMP001");

            Assert.IsNotNull(threshold);
            Assert.AreEqual(22.0, threshold.Minimum);
            Assert.AreEqual(30.0, threshold.Maximum);
        }

        [TestMethod]
        public void SetThreshold_InvalidRange_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() =>
                manager.SetThreshold("H001", "TEMP001", 30, 20));
        }

        [TestMethod]
        public void GetThreshold_UnknownDevice_ShouldReturnNull()
        {
            var threshold = manager.GetThreshold(
                "H001", "UNKNOWN");

            Assert.IsNull(threshold);
        }

        [TestMethod]
        public void SetThreshold_MultipleSensors_ShouldStoreSeparately()
        {
            manager.SetThreshold("H001", "TEMP001", 24, 28);
            manager.SetThreshold("H001", "PH001", 7, 8.5);

            var temperature = manager.GetThreshold(
                "H001", "TEMP001");

            var ph = manager.GetThreshold(
                "H001", "PH001");

            Assert.IsNotNull(temperature);
            Assert.IsNotNull(ph);

            Assert.AreEqual(24.0, temperature.Minimum);
            Assert.AreEqual(7.0, ph.Minimum);
        }

        [TestMethod]
        public void SetThreshold_DifferentHabitats_ShouldStoreSeparately()
        {
            manager.SetThreshold("H001", "TEMP001", 24, 28);
            manager.SetThreshold("H002", "TEMP001", 20, 25);

            var first = manager.GetThreshold(
                "H001", "TEMP001");

            var second = manager.GetThreshold(
                "H002", "TEMP001");

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);

            Assert.AreEqual(24.0, first.Minimum);
            Assert.AreEqual(20.0, second.Minimum);
        }

        [TestMethod]
        public void RemoveThreshold_ShouldRemoveConfiguration()
        {
            manager.SetThreshold("H001", "TEMP001", 24, 28);

            manager.RemoveThreshold("H001", "TEMP001");

            Assert.IsNull(
                manager.GetThreshold("H001", "TEMP001"));
        }

        [TestMethod]
        public void SetThreshold_NaN_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() =>
                manager.SetThreshold(
                    "H001", "TEMP001", double.NaN, 28));
        }

        [TestMethod]
        public void SetThreshold_Infinity_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() =>
                manager.SetThreshold(
                    "H001", "TEMP001",
                    24, double.PositiveInfinity));
        }

        [TestMethod]
        public void SetThreshold_EqualLimits_ShouldBeValid()
        {
            manager.SetThreshold("H001", "TEMP001", 25, 25);

            var threshold = manager.GetThreshold(
                "H001", "TEMP001");

            Assert.IsNotNull(threshold);
            Assert.AreEqual(25.0, threshold.Minimum);
            Assert.AreEqual(25.0, threshold.Maximum);
        }
    }
}

