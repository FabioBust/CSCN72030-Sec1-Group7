using AquaGuard.Devices.Sensors;
using AquaGuard.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Core
{
    public sealed class Hub
    {
        private readonly Dictionary<string, Habitat> _habitats = new();

        public DataTransitBus Bus { get; } = new();

        public void RegisterHabitat(Habitat habitat)
        {
            ArgumentNullException.ThrowIfNull(habitat);
            _habitats.Add(habitat.HabitatId, habitat);
        }

        public void PollSensors()
        {
            foreach (var habitat in _habitats.Values)
            {
                foreach (var device in habitat.Sensors)
                {
                    double? value;
                    string unit;

                    switch (device)
                    {
                        case TemperatureSensor temperature:
                            temperature.ReadNextReading();
                            value = temperature.CurrentTemperature;
                            unit = TemperatureSensor.Unit;
                            break;

                        case OxygenSensor oxygen:
                            oxygen.ReadNextReading();
                            value = oxygen.CurrentOxygen;
                            unit = OxygenSensor.Unit;
                            break;

                        case PHSensor ph:
                            value = ph.CurrentPH;
                            unit = "pH";
                            break;

                        case FlowSensor flow:
                            value = flow.CurrentFlowRate;
                            unit = "L/min";
                            break;

                        default:
                            continue;
                    }

                    Bus.Publish(new SensorReading(
                        habitat.HabitatId,
                        device.DeviceId,
                        device.Name,
                        value,
                        unit,
                        DateTime.UtcNow));
                }
            }
        }
    }
}