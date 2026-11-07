
namespace AquaGuard.Devices
{
    public interface IDevice
    {
        string DeviceId { get; }

        string HabitatId { get; }

        string Name { get; }
    }
}
