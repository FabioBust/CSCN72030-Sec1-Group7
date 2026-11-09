
using System;

namespace AquaGuard.Core
{
    public class Habitat
    {
        public string HabitatId { get; }
        public string Name { get; }

        public Habitat(string habitatId, string name)
        {
            if (string.IsNullOrWhiteSpace(habitatId))
            {
                throw new ArgumentException(
                    "Habitat ID cannot be empty.",
                    nameof(habitatId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Habitat name cannot be empty.",
                    nameof(name));
            }

            HabitatId = habitatId;
            Name = name;
        }
    }
}
