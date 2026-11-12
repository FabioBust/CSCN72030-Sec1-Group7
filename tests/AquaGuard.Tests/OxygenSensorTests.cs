
using AquaGuard.Data;
using AquaGuard.Devices.Sensors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace AquaGuard.Tests
{
    [TestClass]
    public class OxygenSensorTests
    {
        private static OxygenSensor CreateSensor(
            string filePath = "unused.txt")
        {
            var provider = new FileDataProvider(filePath);

            return new OxygenSensor(
                "OXY001", "HAB001", "Oxygen Sensor", provider);
        }

        [TestMethod]
        public void OxygenSensor_ShouldStoreDeviceInformation()
        {
            // Arrange
            string deviceId = "OXY001";
            string habitatId = "HAB001";
            string name = "Oxygen Sensor";
            var provider = new FileDataProvider("unused.txt");

            // Act
            var sensor = new OxygenSensor(
                deviceId, habitatId, name, provider);

            // Assert
            Assert.AreEqual(deviceId, sensor.DeviceId);
            Assert.AreEqual(habitatId, sensor.HabitatId);
            Assert.AreEqual(name, sensor.Name);
        }

        [TestMethod]
        public void OxygenSensor_ShouldInitiallyHaveNoReading()
        {
            // Arrange
            var sensor = CreateSensor();

            // Act
            double? oxygen = sensor.CurrentOxygen;
            bool hasReading = sensor.HasReading;

            // Assert
            Assert.IsNull(oxygen);
            Assert.IsFalse(hasReading);
        }

        [TestMethod]
        public void OxygenSensor_ShouldUseMilligramsPerLiter()
        {
            // Arrange
            string expectedUnit = "mg/L";

            // Act
            string actualUnit = OxygenSensor.Unit;

            // Assert
            Assert.AreEqual(expectedUnit, actualUnit);
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(7.8)]
        [DataRow(15.0)]
        public void UpdateReading_ShouldAcceptValidOxygenLevels(
            double oxygen)
        {
            // Arrange
            var sensor = CreateSensor();

            // Act
            sensor.UpdateReading(oxygen);

            // Assert
            Assert.AreEqual(oxygen, sensor.CurrentOxygen);
            Assert.IsTrue(sensor.HasReading);
        }

        [TestMethod]
        public void UpdateReading_ShouldReplacePreviousReading()
        {
            // Arrange
            var sensor = CreateSensor();
            sensor.UpdateReading(7.8);

            // Act
            sensor.UpdateReading(8.2);

            // Assert
            Assert.AreEqual(8.2, sensor.CurrentOxygen);
            Assert.IsTrue(sensor.HasReading);
        }

        [TestMethod]
        public void UpdateReading_Null_ShouldClearCurrentReading()
        {
            // Arrange
            var sensor = CreateSensor();
            sensor.UpdateReading(7.8);

            // Act
            sensor.UpdateReading(null);

            // Assert
            Assert.IsNull(sensor.CurrentOxygen);
            Assert.IsFalse(sensor.HasReading);
        }

        [TestMethod]
        [DataRow(-1.0)]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        public void UpdateReading_ShouldRejectInvalidOxygenLevels(
            double invalidOxygen)
        {
            // Arrange
            var sensor = CreateSensor();
            sensor.UpdateReading(7.8);

            // Act & Assert
            var exception =
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                    () => sensor.UpdateReading(invalidOxygen));

            Assert.AreEqual("oxygen", exception.ParamName);
            Assert.AreEqual(7.8, sensor.CurrentOxygen);
            Assert.IsTrue(sensor.HasReading);
        }

        [TestMethod]
        public void OxygenSensor_ShouldRejectNullDataProvider()
        {
            // Arrange
            FileDataProvider? provider = null;

            // Act & Assert
            var exception =
                Assert.ThrowsExactly<ArgumentNullException>(
                    () => new OxygenSensor(
                        "OXY001",
                        "HAB001",
                        "Oxygen Sensor",
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
                File.WriteAllLines(
                    filePath, new[] { "7.8", "8.2" });

                var sensor = CreateSensor(filePath);

                // Act & Assert
                sensor.ReadNextReading();
                Assert.AreEqual(7.8, sensor.CurrentOxygen);

                sensor.ReadNextReading();
                Assert.AreEqual(8.2, sensor.CurrentOxygen);

                sensor.ReadNextReading();
                Assert.AreEqual(7.8, sensor.CurrentOxygen);
                Assert.IsTrue(sensor.HasReading);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [TestMethod]
        [DataRow("invalid")]
        [DataRow("-1.0")]
        [DataRow("NaN")]
        [DataRow(" ")]
        public void ReadNextReading_ShouldClearInvalidSimulatedData(
            string invalidRecord)
        {
            // Arrange
            string filePath = Path.GetTempFileName();

            try
            {
                File.WriteAllLines(
                    filePath, new[] { "7.8", invalidRecord });

                var sensor = CreateSensor(filePath);
                sensor.ReadNextReading();

                // Act
                sensor.ReadNextReading();

                // Assert
                Assert.IsNull(sensor.CurrentOxygen);
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
                $"missing-oxygen-{Guid.NewGuid():N}.txt");

            var sensor = CreateSensor(filePath);
            sensor.UpdateReading(7.8);

            // Act
            sensor.ReadNextReading();

            // Assert
            Assert.IsNull(sensor.CurrentOxygen);
            Assert.IsFalse(sensor.HasReading);
        }
    }
}
