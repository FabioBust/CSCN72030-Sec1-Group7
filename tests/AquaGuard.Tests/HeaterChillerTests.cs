using AquaGuard.Devices.Equipment;

namespace AquaGuard.Tests;

[TestClass]
public sealed class HeaterChillerTests
{
    [TestMethod]
    [DataRow("15.0")]
    [DataRow("22.5")]
    [DataRow("32.0")]
    public void AcceptsSupportedTarget(string input)
    {
        var device = new HeaterChiller("heater", "reef", "Heater");
        decimal target = decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture);

        device.SetTargetTemperature(target);

        Assert.AreEqual(target, device.TargetTemperature);
    }

    [TestMethod]
    [DataRow("14.5")]
    [DataRow("32.5")]
    [DataRow("22.25")]
    public void RejectsUnsupportedTargetWithoutChangingSetting(string input)
    {
        var device = new HeaterChiller("heater", "reef", "Heater");
        device.SetTargetTemperature(22m);
        decimal target = decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => device.SetTargetTemperature(target));
        Assert.AreEqual(22m, device.TargetTemperature);
    }
}
