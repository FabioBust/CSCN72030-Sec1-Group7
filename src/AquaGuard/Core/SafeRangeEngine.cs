using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Core
{
    public class SafeRangeEngine
    {
        public RangeStatus Evaluate(double? value, double minimum, double maximum)
        {
            // Validate the configured range
            if (!double.IsFinite(minimum) || !double.IsFinite(maximum) ||
                minimum > maximum)
            {
                throw new ArgumentException(
                    "Invalid safe range.");
            }

            // Check for missing or invalid readings
            if (!value.HasValue || !double.IsFinite(value.Value))
            {
                return RangeStatus.NoData;
            }

            // Check minimum limit
            if (value.Value < minimum)
            {
                return RangeStatus.TooLow;
            }

            // Check maximum limit
            if (value.Value > maximum)
            {
                return RangeStatus.TooHigh;
            }

            return RangeStatus.Normal;
        }
    }
}

