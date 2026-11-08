using AquaGuard.Devices.Sensors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AquaGuard.Tests
{
    [TestClass]
    public class TemperatureSensorTests
    {
        [TestMethod]
        public void TemperatureSensor_ShouldStoreDeviceInformation()
        {
            // Arrange
            string deviceId = "TEMP001";
            string habitatId = "HAB001";
            string name = "Temperature Sensor";

            // Act
            var sensor = new TemperatureSensor(deviceId, habitatId, name);

            // Assert
            Assert.AreEqual(deviceId, sensor.DeviceId);
            Assert.AreEqual(habitatId, sensor.HabitatId);
            Assert.AreEqual(name, sensor.Name);
        }

        [TestMethod]
        public void TemperatureSensor_ShouldInitiallyHaveNoReading()
        {
            // Arrange
            var sensor = new TemperatureSensor("TEMP001", "HAB001", "Temperature Sensor");

            // Act
            double? temperature = sensor.CurrentTemperature;
            bool hasReading = sensor.HasReading;

            // Assert
            Assert.IsNull(temperature);
            Assert.IsFalse(hasReading);
        }

        [TestMethod]
        public void TemperatureSensor_ShouldUseCelsius()
        {
            // Arrange
            string expectedUnit = "°C";

            // Act
            string actualUnit = TemperatureSensor.Unit;

            // Assert
            Assert.AreEqual(expectedUnit, actualUnit);
        }

        [TestMethod]
        [DataRow(-5.0)]
        [DataRow(0.0)]
        [DataRow(24.5)]
        public void UpdateReading_ShouldAcceptFiniteTemperatures(double temperature)
        {
            // Arrange
            var sensor = new TemperatureSensor("TEMP001", "HAB001", "Temperature Sensor");

            // Act
            sensor.UpdateReading(temperature);

            // Assert
            Assert.AreEqual(temperature, sensor.CurrentTemperature);
            Assert.IsTrue(sensor.HasReading);
        }

        [TestMethod]
        public void UpdateReading_ShouldReplacePreviousReading()
        {
            // Arrange
            var sensor = new TemperatureSensor("TEMP001", "HAB001", "Temperature Sensor");

            sensor.UpdateReading(24.5);

            // Act
            sensor.UpdateReading(26.0);

            // Assert
            Assert.AreEqual(26.0, sensor.CurrentTemperature);
            Assert.IsTrue(sensor.HasReading);
        }

        [TestMethod]
        public void UpdateReading_Null_ShouldClearCurrentReading()
        {
            // Arrange
            var sensor = new TemperatureSensor("TEMP001", "HAB001", "Temperature Sensor");

            sensor.UpdateReading(24.5);

            // Act
            sensor.UpdateReading(null);

            // Assert
            Assert.IsNull(sensor.CurrentTemperature);
            Assert.IsFalse(sensor.HasReading);
        }

        [TestMethod]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        public void UpdateReading_ShouldRejectNonFiniteTemperatures(double invalidTemperature)
        {
            // Arrange
            var sensor = new TemperatureSensor("TEMP001", "HAB001", "Temperature Sensor");

            sensor.UpdateReading(24.5);

            // Act & Assert
            var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => sensor.UpdateReading(invalidTemperature));

            Assert.AreEqual("temperature", exception.ParamName);
            Assert.AreEqual(24.5, sensor.CurrentTemperature);
            Assert.IsTrue(sensor.HasReading);
        }
    }
}
