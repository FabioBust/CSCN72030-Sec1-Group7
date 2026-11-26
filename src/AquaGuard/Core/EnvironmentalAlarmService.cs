using System;
using System.Collections.Generic;
using System.Text;


namespace AquaGuard.Core
{
    public class EnvironmentalAlarmService
    {
        private readonly SafeRangeEngine safeRangeEngine;
        private readonly AlertManager alertManager;

        public EnvironmentalAlarmService(
            SafeRangeEngine safeRangeEngine,
            AlertManager alertManager)
        {
            this.safeRangeEngine = safeRangeEngine;
            this.alertManager = alertManager;
        }

        public RangeStatus ProcessReading(
            string habitatId,
            string deviceId,
            double? value,
            double minimum,
            double maximum)
        {
            RangeStatus status = safeRangeEngine.Evaluate(
                value, minimum, maximum);

            switch (status)
            {
                case RangeStatus.Normal:

                    alertManager.ClearAlert(
                        habitatId, deviceId);
                    break;

                case RangeStatus.TooLow:

                    alertManager.RaiseAlert(
                        habitatId,
                        deviceId,
                        "Sensor reading is below the safe range.");
                    break;

                case RangeStatus.TooHigh:

                    alertManager.RaiseAlert(
                        habitatId,
                        deviceId,
                        "Sensor reading is above the safe range.");
                    break;

                case RangeStatus.NoData:

                    alertManager.RaiseAlert(
                        habitatId,
                        deviceId,
                        "Sensor data is unavailable.");
                    break;
            }

            return status;
        }
    }
}
