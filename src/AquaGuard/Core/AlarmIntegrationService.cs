using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;

namespace AquaGuard.Core
{
    public class AlarmIntegrationService
    {
        private readonly AlarmThresholdManager thresholdManager;
        private readonly AlertManager alertManager;
        private readonly SafeRangeEngine safeRangeEngine;
        private readonly EnvironmentalAlarmService alarmService;

        public AlarmIntegrationService()
        {
            thresholdManager = new AlarmThresholdManager();
            alertManager = new AlertManager();
            safeRangeEngine = new SafeRangeEngine();

            alarmService = new EnvironmentalAlarmService(
                safeRangeEngine,
                alertManager);
        }

        public IReadOnlyList<Alert> ActiveAlerts
        {
            get
            {
                return alertManager.ActiveAlerts;
            }
        }

        public void SetThreshold(
            string habitatId,
            string deviceId,
            double minimum,
            double maximum)
        {
            thresholdManager.SetThreshold(
                habitatId,
                deviceId,
                minimum,
                maximum);
        }

        public RangeStatus ProcessReading(
            string habitatId,
            string deviceId,
            double? value)
        {
            var threshold = thresholdManager.GetThreshold(
                habitatId,
                deviceId);

            if (threshold == null)
            {
                throw new InvalidOperationException(
                    "No alarm threshold configured for this sensor.");
            }

            return alarmService.ProcessReading(
                habitatId,
                deviceId,
                value,
                threshold.Minimum,
                threshold.Maximum);
        }
    }
}
