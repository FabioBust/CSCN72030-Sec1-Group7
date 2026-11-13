namespace AquaGuard.Models
{
    public enum TelemetryKind
    {
        SensorReading,
        EquipmentState
    }

    public sealed record SystemMessage(
        TelemetryKind Kind,
        string HabitatId,
        string DeviceId,
        string? Value,
        DateTime Timestamp);
}
