namespace AquaGuard.Devices.Equipment;

public sealed class HeaterChiller(string deviceId, string habitatId, string name)
    : Equipment(deviceId, habitatId, name)
{
    public decimal? TargetTemperature { get; private set; }

    public void SetTargetTemperature(decimal target)
    {
        ValidateTarget(target);
        TargetTemperature = target;
    }

    internal static void ValidateTarget(decimal target)
    {
        if (target is < 15m or > 32m || target % 0.5m != 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(target), target,
                "Target temperature must be 15.0–32.0 °C in 0.5 °C increments.");
        }
    }
}
