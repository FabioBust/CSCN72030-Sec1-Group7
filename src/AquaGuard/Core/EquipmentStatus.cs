using System;
using System.Collections.Generic;
using System.Text;


namespace AquaGuard.Core
{
    public class EquipmentStatus
    {
        public string DeviceId { get; }
        public string HabitatId { get; }
        public string Name { get; }
        public bool IsOn { get; }
        public int? SettingPercentage { get; }

        public EquipmentStatus(
            string deviceId,
            string habitatId,
            string name,
            bool isOn,
            int? settingPercentage)
        {
            DeviceId = deviceId;
            HabitatId = habitatId;
            Name = name;
            IsOn = isOn;
            SettingPercentage = settingPercentage;
        }
    }
}

