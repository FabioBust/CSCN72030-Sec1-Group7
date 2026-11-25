using System.IO;
using System.Security.Cryptography;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Data;

namespace AquaGuard.Tests;

[TestClass]
public sealed class HabitatHistoryTests
{
    [TestMethod]
    public void StaffHistoryCombinesExistingTablesAndPagesOnlyTheSelectedHabitat()
    {
        string folder = Directory.CreateTempSubdirectory("aquaguard-history-").FullName;
        try
        {
            string path = Path.Combine(folder, "history.db");
            var logger = new DataLogger(path);
            var auth = new AuthenticationService(Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes("test-code"))));
            var hub = new Hub(auth, new DataTransitBus(),
                [new Habitat("reef", "Reef"), new Habitat("kelp", "Kelp")], null, logger);
            var now = DateTimeOffset.UtcNow;

            Assert.ThrowsExactly<UnauthorizedAccessException>(() => hub.ReadHabitatHistory("reef", now, 0, 10));
            Assert.IsTrue(auth.TrySignIn("test-code"));
            Assert.HasCount(0, hub.ReadHabitatHistory("reef", now, 0, 10));
            Assert.IsFalse(File.Exists(path));

            logger.LogEquipmentSetting("reef", "pump", "speed_percentage", "40");
            var through = DateTimeOffset.UtcNow;
            Assert.AreEqual("40", hub.ReadHabitatHistory("reef", through, 0, 10).Single().Value);
            logger.LogFeeding("reef", "food", now.AddMinutes(-1));
            logger.LogFeeding("kelp", "food", now.AddMinutes(-1));
            logger.LogSensorReadings([
                new("reef", "temperature", 23.5, "°C", now.AddMinutes(-2)),
                new("reef", "temperature", 99, "°C", through.AddSeconds(1))
            ]);

            var first = hub.ReadHabitatHistory("reef", through, 0, 2);
            CollectionAssert.AreEqual(new[] { "Equipment setting", "Feeding" },
                first.Select(row => row.Kind).ToArray());
            Assert.AreEqual("1", first[1].Value);
            var older = hub.ReadHabitatHistory("reef", through, 2, 2).Single();
            Assert.AreEqual("Sensor reading", older.Kind);
            Assert.AreEqual("23.5", older.Value);
            Assert.AreEqual("°C", older.Unit);
            Assert.AreEqual("Feeding", hub.ReadHabitatHistory("kelp", through, 0, 10).Single().Kind);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
