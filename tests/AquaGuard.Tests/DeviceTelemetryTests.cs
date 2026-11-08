
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AquaGuard.Models;
using System;

namespace AquaGuard.Tests
{
    [TestClass]
    public class DeviceTelemetryTests
    {
        [TestMethod]
        public void DeviceTelemetry_ShouldStoreDeviceInformation()
        {
            // Arrange & Act
            var telemetry = new DeviceTelemetry
            {
                HabitatId = "H001",
                DeviceId = "TEMP001",
                Value = "25.5",
                Timestamp = DateTime.UtcNow
            };

            // Assert
            Assert.AreEqual("H001", telemetry.HabitatId);
            Assert.AreEqual("TEMP001", telemetry.DeviceId);
            Assert.AreEqual("25.5", telemetry.Value);
        }

        [TestMethod]
        public void DeviceTelemetry_ShouldStoreTimestamp()
        {
            // Arrange
            DateTime timestamp = DateTime.UtcNow;
            
            // Act
            var telemetry = new DeviceTelemetry
            {
                HabitatId = "H001",
                DeviceId = "TEMP001",
                Value = "25.5",
                Timestamp = timestamp
            };

            // Assert
            Assert.AreEqual(timestamp, telemetry.Timestamp);
        }
    }
}
