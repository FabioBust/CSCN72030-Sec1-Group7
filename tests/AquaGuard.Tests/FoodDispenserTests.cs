using AquaGuard.Devices.Equipment;

namespace AquaGuard.Tests;

[TestClass]
public sealed class FoodDispenserTests
{
    [TestMethod]
    public void RequiresEnabledDispenserAndExactSimulatedDonation()
    {
        var device = new FoodDispenser("food", "reef", "Feeder");

        Assert.IsFalse(device.TryDispense(2m));
        device.SetEnabled(true);
        Assert.IsFalse(device.TryDispense(1.99m));
        Assert.IsTrue(device.TryDispense(2m));
        device.SetEnabled(false);
        Assert.IsFalse(device.CanFeed);
    }

    [TestMethod]
    public void DispensesOnePortionAndAllowsNextAtExactlyTwoHours()
    {
        var clock = new TestClock();
        var device = new FoodDispenser("food", "reef", "Feeder", clock);
        device.SetEnabled(true);

        Assert.IsTrue(device.TryDispense(2m));
        Assert.IsFalse(device.TryDispense(2m));
        clock.Now += TimeSpan.FromHours(2) - TimeSpan.FromTicks(1);
        Assert.IsFalse(device.TryDispense(2m));
        device.SetEnabled(false);
        device.SetEnabled(true);
        clock.Now += TimeSpan.FromTicks(1);
        Assert.IsTrue(device.TryDispense(2m));
    }

    [TestMethod]
    public void FeedingHistoryIsPerDispenser()
    {
        var clock = new TestClock();
        var reef = new FoodDispenser("food", "reef", "Feeder", clock);
        var kelp = new FoodDispenser("food", "kelp", "Feeder", clock);
        reef.SetEnabled(true);
        kelp.SetEnabled(true);

        Assert.IsTrue(reef.TryDispense(2m));
        Assert.IsTrue(kelp.TryDispense(2m));
    }

    private sealed class TestClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UnixEpoch;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
