
using AquaGuard.Data;
using AquaGuard.Devices.Sensors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace AquaGuard.Tests
{
    [TestClass]
    public class TemperatureSensorTests
    {
        private static TemperatureSensor CreateSensor()
        {
            var provider = new FileDataProvider("unused.txt");

            return new TemperatureSensor(
                "TEMP001", "HAB001", "Temperature Sensor", provider);
        }

        [TestMethod]
        public void TemperatureSensor_ShouldStoreDeviceInformation()
        {
            // Arrange
            string deviceId = "TEMP001";
            string habitatId = "HAB001";
            string name = "Temperature Sensor";
            var provider = new FileDataProvider("unused.txt");

            // Act
            var sensor = new TemperatureSensor(
                deviceId, habitatId, name, provider);

            // Assert
            Assert.AreEqual(deviceId, sensor.DeviceId);
            Assert.AreEqual(habitatId, sensor.HabitatId);
            Assert.AreEqual(name, sensor.Name);
        }

        [TestMethod]
        public void TemperatureSensor_ShouldInitiallyHaveNoReading()
        {
            // Arrange
            var sensor = CreateSensor();

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
        public void UpdateReading_ShouldAcceptFiniteTemperatures(
            double temperature)
        {
            // Arrange
            var sensor = CreateSensor();

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
            var sensor = CreateSensor();
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
            var sensor = CreateSensor();
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
        public void UpdateReading_ShouldRejectNonFiniteTemperatures(
            double invalidTemperature)
        {
            // Arrange
            var sensor = CreateSensor();
            sensor.UpdateReading(24.5);

            // Act & Assert
            var exception =
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                    () => sensor.UpdateReading(invalidTemperature));

            Assert.AreEqual("temperature", exception.ParamName);
            Assert.AreEqual(24.5, sensor.CurrentTemperature);
            Assert.IsTrue(sensor.HasReading);
        }

        [TestMethod]
        public void TemperatureSensor_ShouldRejectNullDataProvider()
        {
            // Arrange
            FileDataProvider? provider = null;

            // Act & Assert
            var exception =
                Assert.ThrowsExactly<ArgumentNullException>(
                    () => new TemperatureSensor(
                        "TEMP001",
                        "HAB001",
                        "Temperature Sensor",
                        provider!));

            Assert.AreEqual("dataProvider", exception.ParamName);
        }

        [TestMethod]
        public void ReadNextReading_ShouldReadAndLoopSimulatedData()
        {
            // Arrange
            string filePath = Path.GetTempFileName();

            try
            {
                File.WriteAllLines(filePath, new[] { "24.5", "26.0" });

                var provider = new FileDataProvider(filePath);
                var sensor = new TemperatureSensor(
                    "TEMP001", "HAB001", "Temperature Sensor", provider);

                // Act & Assert
                sensor.ReadNextReading();
                Assert.AreEqual(24.5, sensor.CurrentTemperature);

                sensor.ReadNextReading();
                Assert.AreEqual(26.0, sensor.CurrentTemperature);

                sensor.ReadNextReading();
                Assert.AreEqual(24.5, sensor.CurrentTemperature);
                Assert.IsTrue(sensor.HasReading);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [TestMethod]
        public void ReadNextReading_ShouldClearInvalidSimulatedData()
        {
            // Arrange
            string filePath = Path.GetTempFileName();

            try
            {
                File.WriteAllLines(filePath, new[] { "24.5", "invalid" });

                var provider = new FileDataProvider(filePath);
                var sensor = new TemperatureSensor(
                    "TEMP001", "HAB001", "Temperature Sensor", provider);

                sensor.ReadNextReading();

                // Act
                sensor.ReadNextReading();

                // Assert
                Assert.IsNull(sensor.CurrentTemperature);
                Assert.IsFalse(sensor.HasReading);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [TestMethod]
        public void ReadNextReading_ShouldHandleMissingFile()
        {
            // Arrange
            string filePath = Path.Combine(
                Path.GetTempPath(),
                $"missing-temperature-{Guid.NewGuid():N}.txt");

            var provider = new FileDataProvider(filePath);
            var sensor = new TemperatureSensor(
                "TEMP001", "HAB001", "Temperature Sensor", provider);

            sensor.UpdateReading(24.5);

            // Act
            sensor.ReadNextReading();

            // Assert
            Assert.IsNull(sensor.CurrentTemperature);
            Assert.IsFalse(sensor.HasReading);
        }
    }
}
