
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AquaGuard.Devices;

namespace AquaGuard.Tests
{
    [TestClass]
    public class DeviceBaseTests
    {
        // Concrete test class for our abstract DeviceBase
        private class TestDevice : DeviceBase
        {
            public TestDevice(
                string deviceId,
                string habitatId,
                string name)
                : base(deviceId, habitatId, name)
            {
            }
        }

        [TestMethod]
        public void DeviceBase_ShouldStoreDeviceInformation()
        {
            var device = new TestDevice(
                "DEV001",
                "HAB001",
                "Temperature Sensor");

            Assert.AreEqual("DEV001", device.DeviceId);
            Assert.AreEqual("HAB001", device.HabitatId);
            Assert.AreEqual("Temperature Sensor", device.Name);
        }

        [TestMethod]
        public void DeviceBase_ShouldImplementIDevice()
        {
            var device = new TestDevice(
                "DEV001",
                "HAB001",
                "Temperature Sensor");

            Assert.IsInstanceOfType(device, typeof(IDevice));
        }

        [DataTestMethod]
        [DataRow("", "HAB001", "Temperature Sensor")]
        [DataRow("DEV001", "", "Temperature Sensor")]
        [DataRow("DEV001", "HAB001", "")]
        public void DeviceBase_ShouldRejectEmptyRequiredFields(
            string deviceId,
            string habitatId,
            string name)
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                new TestDevice(deviceId, habitatId, name));
        }

    }
}
