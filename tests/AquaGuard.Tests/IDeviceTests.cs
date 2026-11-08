using AquaGuard.Devices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class IDeviceTests
    {
        [TestMethod]
        public void IDevice_ShouldContainDeviceId()
        {
            // Arrange
            var deviceType = typeof(IDevice);

            // Act
            var property = deviceType.GetProperty(nameof(IDevice.DeviceId));

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(string), property.PropertyType);
        }

        [TestMethod]
        public void IDevice_ShouldContainHabitatId()
        {
            // Arrange
            var deviceType = typeof(IDevice);

            // Act
            var property = deviceType.GetProperty(nameof(IDevice.HabitatId));

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(string), property.PropertyType);
        }

        [TestMethod]
        public void IDevice_ShouldContainName()
        {
            // Arrange
            var deviceType = typeof(IDevice);

            // Act
            var property = deviceType.GetProperty(nameof(IDevice.Name));

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(string), property.PropertyType);
        }
    }
}