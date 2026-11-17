using System.IO;
using AquaGuard.Core;
using AquaGuard.Data;

namespace AquaGuard.Tests
{
    [TestClass]
    public class SensorMonitorTests
    {
        [TestMethod]
        [DataRow(null)]
        [DataRow("invalid")]
        [DataRow("NaN")]
        public void MissingReadingRecoversWhileOtherSensorContinues(string? invalidRecord)
        {
            var folder = Directory.CreateTempSubdirectory();
            try
            {
                string intermittent = Path.Combine(folder.FullName, "intermittent.txt");
                string stable = Path.Combine(folder.FullName, "stable.txt");
                if (invalidRecord is not null)
                {
                    File.WriteAllText(intermittent, invalidRecord);
                }
                File.WriteAllLines(stable, ["21.5", "22"]);

                var monitor = new SensorMonitor();
                monitor.AddSource("reef", "intermittent", new FileDataProvider(intermittent));
                monitor.AddSource("reef", "stable", new FileDataProvider(stable));
                var first = monitor.Poll();
                Assert.AreEqual("No Data", first[0].Reading);
                Assert.AreEqual("21.5", first[1].Reading);

                File.WriteAllText(intermittent, "18.5");
                var second = monitor.Poll();
                Assert.AreEqual("18.5", second[0].Reading);
                Assert.AreEqual("22", second[1].Reading);
            }
            finally
            {
                folder.Delete(recursive: true);
            }
        }
    }
}
