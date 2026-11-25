using AquaGuard.Data;

namespace AquaGuard.Devices.Equipment;

public sealed class FoodDispenser(
    string deviceId, string habitatId, string name, TimeProvider? clock = null)
    : Equipment(deviceId, habitatId, name)
{
    private readonly TimeProvider timeProvider = clock ?? TimeProvider.System;
    private readonly Queue<DateTimeOffset> feedings = new();
    private readonly object gate = new();
    private bool enabled;
    private DateTimeOffset? lastFeeding;
    private bool historyLoaded;

    internal DataLogger? Logger { get; init; }

    public bool IsEnabled
    {
        get { lock (gate) return enabled; }
    }

    public void SetEnabled(bool value)
    {
        lock (gate) enabled = value;
    }

    public bool CanFeed
    {
        get { lock (gate) return IsEligible(timeProvider.GetUtcNow()); }
    }

    public bool TryDispense(decimal simulatedDonationCad)
    {
        lock (gate)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            if (simulatedDonationCad != 2m || !IsEligible(now))
                return false;

            Logger?.LogFeeding(HabitatId, DeviceId, now);
            feedings.Enqueue(now);
            lastFeeding = now;
            return true;
        }
    }

    private bool IsEligible(DateTimeOffset now)
    {
        if (!enabled)
            return false;

        if (!historyLoaded && Logger is not null)
        {
            foreach (var feeding in Logger.ReadFeedings(HabitatId, DeviceId, now - TimeSpan.FromHours(24)))
            {
                feedings.Enqueue(feeding);
                lastFeeding = feeding;
            }

            historyLoaded = true;
        }

        while (feedings.TryPeek(out var oldest) && now - oldest >= TimeSpan.FromHours(24))
            feedings.Dequeue();

        return feedings.Count < 20
            && (lastFeeding is null || now - lastFeeding.Value >= TimeSpan.FromHours(2));
    }
}
