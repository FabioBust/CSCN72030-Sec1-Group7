
using AquaGuard.Devices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace AquaGuard.Tests
{
    [TestClass]
    public class IDeviceTests
    {
        [TestMethod]
        public void IDevice_ShouldContainDeviceId()
        {
            var property = typeof(IDevice).GetProperty("DeviceId");

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(string), property.PropertyType);
        }

        [TestMethod]
        public void IDevice_ShouldContainHabitatId()
        {
            var property = typeof(IDevice).GetProperty("HabitatId");

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(string), property.PropertyType);
        }

        [TestMethod]
        public void IDevice_ShouldContainName()
        {
            var property = typeof(IDevice).GetProperty("Name");

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(string), property.PropertyType);
        }
    }
}
