using System.Security.Cryptography;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Devices.Equipment;

namespace AquaGuard.Tests
{
    [TestClass]
    public class EquipmentControlServiceTests
    {
        private static readonly string CodeHash =
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("test-code")));

        [TestMethod]
        public void DirectVisitorCommandIsDeniedWithoutChangingEquipment()
        {
            var authentication = new AuthenticationService(CodeHash);
            var controls = new EquipmentControlService(authentication);
            var habitat = new Habitat("H1", "Reef");
            var pump = new CirculationPump("P1", "H1", "Pump");
            habitat.AddEquipment(pump);

            Assert.ThrowsExactly<UnauthorizedAccessException>(() => controls.SetPower(habitat, "P1", true));
            Assert.IsFalse(pump.IsOn);
        }

        [TestMethod]
        public void ValidStaffSessionCanControlEquipment()
        {
            var authentication = new AuthenticationService(CodeHash);
            var controls = new EquipmentControlService(authentication);
            var habitat = new Habitat("H1", "Reef");
            var pump = new CirculationPump("P1", "H1", "Pump");
            habitat.AddEquipment(pump);

            Assert.IsTrue(authentication.TrySignIn("test-code"));
            controls.SetPower(habitat, "P1", true);

            Assert.IsTrue(pump.IsOn);
        }
    }
}
