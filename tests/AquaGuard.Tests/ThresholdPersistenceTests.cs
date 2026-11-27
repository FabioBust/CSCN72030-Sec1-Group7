using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using AquaGuard.Core;

namespace AquaGuard.Tests;

[TestClass]
public class ThresholdPersistenceTests
{
    private string root = null!;
    private string reef = null!;
    private string freshwater = null!;

    [TestInitialize]
    public void Setup()
    {
        root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        reef = Directory.CreateDirectory(
            Path.Combine(root, "reef")).FullName;

        freshwater = Directory.CreateDirectory(
            Path.Combine(root, "freshwater")).FullName;

        File.WriteAllText(
            Path.Combine(reef, "temperature.txt"),
            "25");

        File.WriteAllText(
            Path.Combine(reef, "oxygen.txt"),
            "7");

        File.WriteAllText(
            Path.Combine(freshwater, "temperature.txt"),
            "24");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, true);
        }
    }

    private Hub CreateHub(bool staff = true)
    {
        string hash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes("test-code")));

        var authentication =
            new AuthenticationService(hash);

        if (staff)
        {
            Assert.IsTrue(
                authentication.TrySignIn("test-code"));
        }

        var hub = new Hub(
            authentication,
            new DataTransitBus(),
            []);

        HabitatFileSources.Load(hub, root);

        return hub;
    }

    private SensorThresholds CreateThresholds()
    {
        return new SensorThresholds
        {
            WarningLow = 18,
            NormalLow = 21,
            NormalHigh = 27,
            WarningHigh = 30
        };
    }

    // TP-01: Save thresholds to JSON

    [TestMethod]
    public void StaffThresholdChangeCreatesJsonFile()
    {
        var hub = CreateHub();

        hub.SetSensorThresholds(
            "reef",
            "temperature",
            CreateThresholds());

        string path = Path.Combine(
            reef,
            "thresholds.json");

        Assert.IsTrue(File.Exists(path));
    }

    // TP-02: Restore thresholds after restart

    [TestMethod]
    public void ThresholdsAreRestoredAfterRestart()
    {
        var hub = CreateHub();

        hub.SetSensorThresholds(
            "reef",
            "temperature",
            CreateThresholds());

        var restartedHub = CreateHub();

        var saved = restartedHub.GetSensorThresholds(
            "reef",
            "temperature");

        Assert.IsNotNull(saved);

        Assert.AreEqual(18d, saved.WarningLow);
        Assert.AreEqual(21d, saved.NormalLow);
        Assert.AreEqual(27d, saved.NormalHigh);
        Assert.AreEqual(30d, saved.WarningHigh);
    }

    // TP-03: Keep habitat settings separate

    [TestMethod]
    public void ThresholdsRemainSeparateForEachHabitat()
    {
        var hub = CreateHub();

        hub.SetSensorThresholds(
            "reef",
            "temperature",
            CreateThresholds());

        hub.SetSensorThresholds(
            "freshwater",
            "temperature",
            new SensorThresholds
            {
                WarningLow = 15,
                NormalLow = 20,
                NormalHigh = 25,
                WarningHigh = 28
            });

        var restartedHub = CreateHub();

        var reefThresholds =
            restartedHub.GetSensorThresholds(
                "reef", "temperature");

        var freshwaterThresholds =
            restartedHub.GetSensorThresholds(
                "freshwater", "temperature");

        Assert.IsNotNull(reefThresholds);
        Assert.IsNotNull(freshwaterThresholds);

        Assert.AreEqual(
            27d, reefThresholds.NormalHigh);

        Assert.AreEqual(
            25d, freshwaterThresholds.NormalHigh);
    }

    // TP-04: Keep sensor settings separate

    [TestMethod]
    public void SavingOneSensorPreservesOtherSensors()
    {
        var hub = CreateHub();

        hub.SetSensorThresholds(
            "reef",
            "temperature",
            CreateThresholds());

        hub.SetSensorThresholds(
            "reef",
            "oxygen",
            new SensorThresholds
            {
                WarningLow = 3,
                NormalLow = 5,
                NormalHigh = 9,
                WarningHigh = 12
            });

        var restartedHub = CreateHub();

        var temperature =
            restartedHub.GetSensorThresholds(
                "reef", "temperature");

        var oxygen =
            restartedHub.GetSensorThresholds(
                "reef", "oxygen");

        Assert.IsNotNull(temperature);
        Assert.IsNotNull(oxygen);

        Assert.AreEqual(27d, temperature.NormalHigh);
        Assert.AreEqual(9d, oxygen.NormalHigh);
    }

    // TP-05: Reject invalid thresholds

    [TestMethod]
    public void InvalidThresholdsAreNotSaved()
    {
        var hub = CreateHub();

        var invalid = new SensorThresholds
        {
            WarningLow = 20,
            NormalLow = 30,
            NormalHigh = 25,
            WarningHigh = 35
        };

        Assert.ThrowsExactly<ArgumentException>(() =>
            hub.SetSensorThresholds(
                "reef",
                "temperature",
                invalid));

        Assert.IsFalse(File.Exists(
            Path.Combine(reef, "thresholds.json")));
    }

    // TP-06: Missing configuration file

    [TestMethod]
    public void MissingConfigurationDoesNotCrashApplication()
    {
        var hub = CreateHub();

        Assert.IsFalse(
            hub.ThresholdConfigurationFailed);

        Assert.IsNull(
            hub.GetSensorThresholds(
                "reef", "temperature"));
    }

    // TP-07: Corrupted configuration file

    [TestMethod]
    public void CorruptedConfigurationIsDetected()
    {
        File.WriteAllText(
            Path.Combine(reef, "thresholds.json"),
            "invalid json");

        var hub = CreateHub();

        Assert.IsTrue(
            hub.ThresholdConfigurationFailed);

        Assert.IsNull(
            hub.GetSensorThresholds(
                "reef", "temperature"));
    }

    // TP-08: Visitors cannot save thresholds

    [TestMethod]
    public void VisitorCannotSaveThresholds()
    {
        var hub = CreateHub(staff: false);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
            hub.SetSensorThresholds(
                "reef",
                "temperature",
                CreateThresholds()));

        Assert.IsFalse(File.Exists(
            Path.Combine(reef, "thresholds.json")));
    }


    [TestMethod]
    public void FailedSavePreservesPreviousThresholds()
    {
        var hub = CreateHub();

        // Arrange: Save valid initial thresholds
        hub.SetSensorThresholds(
            "reef",
            "temperature",
            CreateThresholds());

        string path = Path.Combine(
            reef, "thresholds.json");

        string originalJson = File.ReadAllText(path);

        var updatedThresholds = new SensorThresholds
        {
            WarningLow = 10,
            NormalLow = 15,
            NormalHigh = 20,
            WarningHigh = 25
        };

        // Lock the configuration file to prevent replacement
        using (var lockedFile = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.None))
        {
            // Act and Assert
            Exception? exception = null;

            try
            {
                hub.SetSensorThresholds(
                    "reef",
                    "temperature",
                    updatedThresholds);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsTrue(
                exception is IOException ||
                exception is UnauthorizedAccessException,
                "Expected a file-saving failure.");
        }

        // Verify previous thresholds remain in memory
        var current = hub.GetSensorThresholds(
            "reef", "temperature");

        Assert.IsNotNull(current);

        Assert.AreEqual(18d, current.WarningLow);
        Assert.AreEqual(21d, current.NormalLow);
        Assert.AreEqual(27d, current.NormalHigh);
        Assert.AreEqual(30d, current.WarningHigh);

        // Verify original JSON remains unchanged
        Assert.AreEqual(
            originalJson,
            File.ReadAllText(path));
    }

}
