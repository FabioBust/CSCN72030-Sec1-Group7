
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AquaGuard.Core;
using AquaGuard.Devices;

namespace AquaGuard.Tests
{
    [TestClass]
    public class HabitatTests
    {
        // Test helper must be INSIDE HabitatTests
        private class TestSensor : IDevice
        {
            public string DeviceId { get; }
            public string HabitatId { get; }
            public string Name { get; }

            public TestSensor(
                string deviceId,
                string habitatId,
                string name)
            {
                DeviceId = deviceId;
                HabitatId = habitatId;
                Name = name;
            }
        }

        private class TestEquipment : IDevice
        {
            public string DeviceId { get; }
            public string HabitatId { get; }
            public string Name { get; }

            public TestEquipment(
                string deviceId,
                string habitatId,
                string name)
            {
                DeviceId = deviceId;
                HabitatId = habitatId;
                Name = name;
            }
        }


        [TestMethod]
        public void Habitat_ShouldStoreIdAndName()
        {
            var habitat = new Habitat("HAB001", "Coral Reef");

            Assert.AreEqual("HAB001", habitat.HabitatId);
            Assert.AreEqual("Coral Reef", habitat.Name);
        }

        [TestMethod]
        public void Habitat_ShouldRegisterAndRetrieveSensor()
        {
            var habitat = new Habitat("HAB001", "Coral Reef");

            var sensor = new TestSensor(
                "SEN001",
                "HAB001",
                "Temperature Sensor");

            habitat.AddSensor(sensor);

            Assert.AreSame(sensor, habitat.GetSensor("SEN001"));
        }

        [DataTestMethod]
        [DataRow("", "Coral Reef")]
        [DataRow("HAB001", "")]
        [DataRow(" ", "Coral Reef")]
        [DataRow("HAB001", " ")]
        public void Habitat_ShouldRejectEmptyRequiredFields(
            string habitatId, string name)
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                new Habitat(habitatId, name));
        }

        [TestMethod]
        public void Habitat_ShouldRejectSensorFromDifferentHabitat()
        {
            var habitat = new Habitat("HAB001", "Coral Reef");

            var sensor = new TestSensor(
                "SEN001", "HAB002", "Temperature Sensor");

            Assert.ThrowsExactly<ArgumentException>(() =>
                habitat.AddSensor(sensor));
        }

        [TestMethod]
        public void Habitat_ShouldRejectDuplicateSensorId()
        {
            var habitat = new Habitat("HAB001", "Coral Reef");

            var sensor1 = new TestSensor(
                "SEN001", "HAB001", "Temperature Sensor");

            var sensor2 = new TestSensor(
                "SEN001", "HAB001", "Oxygen Sensor");

            habitat.AddSensor(sensor1);

            Assert.ThrowsExactly<ArgumentException>(() =>
                habitat.AddSensor(sensor2));
        }

        [TestMethod]
        public void Habitat_ShouldRejectNullSensor()
        {
            var habitat = new Habitat("HAB001", "Coral Reef");

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                habitat.AddSensor(null!));
        }

        [TestMethod]
        public void Habitat_ShouldRegisterAndRetrieveEquipment()
        {
            // Arrange
            var habitat = new Habitat("HAB001", "Coral Reef");

            var equipment = new TestEquipment(
                "EQ001",
                "HAB001",
                "Water Pump");

            // Act
            habitat.AddEquipment(equipment);

            // Assert
            Assert.AreSame(
                equipment,
                habitat.GetEquipment("EQ001"));
        }

        [TestMethod]
        public void Habitat_ShouldRejectEquipmentFromDifferentHabitat()
        {
            var habitat = new Habitat("HAB001", "Coral Reef");

            var equipment = new TestEquipment(
                "EQ001", "HAB002", "Water Pump");

            Assert.ThrowsExactly<ArgumentException>(() =>
                habitat.AddEquipment(equipment));
        }

        [TestMethod]
        public void Habitat_ShouldRejectDuplicateEquipmentId()
        {
            var habitat = new Habitat("HAB001", "Coral Reef");

            var equipment1 = new TestEquipment(
                "EQ001", "HAB001", "Water Pump");

            var equipment2 = new TestEquipment(
                "EQ001", "HAB001", "Water Filter");

            habitat.AddEquipment(equipment1);

            Assert.ThrowsExactly<ArgumentException>(() =>
                habitat.AddEquipment(equipment2));
        }

        [TestMethod]
        public void Habitat_ShouldRejectNullEquipment()
        {
            var habitat = new Habitat("HAB001", "Coral Reef");

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                habitat.AddEquipment(null!));
        }


    }
}
