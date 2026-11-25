namespace AquaGuard.Core;

public enum EnvironmentalSeverity { Normal, Warning, Critical }

internal sealed class SensorThresholds
{
    public required double WarningLow { get; init; }
    public required double NormalLow { get; init; }
    public required double NormalHigh { get; init; }
    public required double WarningHigh { get; init; }

    internal bool IsValid =>
        double.IsFinite(WarningLow) && double.IsFinite(NormalLow) &&
        double.IsFinite(NormalHigh) && double.IsFinite(WarningHigh) &&
        WarningLow <= NormalLow && NormalLow <= NormalHigh && NormalHigh <= WarningHigh;

    internal EnvironmentalSeverity Classify(double value) =>
        value >= NormalLow && value <= NormalHigh ? EnvironmentalSeverity.Normal :
        value >= WarningLow && value <= WarningHigh ? EnvironmentalSeverity.Warning :
        EnvironmentalSeverity.Critical;
}
