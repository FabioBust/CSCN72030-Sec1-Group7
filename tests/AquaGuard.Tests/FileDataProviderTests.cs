using Microsoft.VisualStudio.TestTools.UnitTesting;
using AquaGuard.Data;
using System;
using System.IO;

namespace AquaGuard.Tests
{
    [TestClass]
    public class FileDataProviderTests
    {
        private string _directory = null!;

        [TestInitialize]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "AquaGuardTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }

        private string CreateFile(string name, params string[] records)
        {
            string path = Path.Combine(_directory, name);
            File.WriteAllLines(path, records);
            return path;
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Constructor_ShouldRejectInvalidPath(string? path)
        {
            var exception = Assert.ThrowsExactly<ArgumentException>(
                () => new FileDataProvider(path!));
            Assert.AreEqual("filePath", exception.ParamName);
        }

        [TestMethod]
        public void ReadNextRecord_ShouldReadSequentiallyAndLoopRepeatedly()
        {
            var provider = new FileDataProvider(CreateFile("sensor.txt", "25.5", "26.0", "24.5"));
            for (int cycle = 0; cycle < 3; cycle++)
            {
                Assert.AreEqual("25.5", provider.ReadNextRecord());
                Assert.AreEqual("26.0", provider.ReadNextRecord());
                Assert.AreEqual("24.5", provider.ReadNextRecord());
            }
        }

        [TestMethod]
        public void ReadNextRecord_ShouldLoopSingleRecord()
        {
            var provider = new FileDataProvider(CreateFile("single.txt", "On"));
            for (int i = 0; i < 3; i++)
                Assert.AreEqual("On", provider.ReadNextRecord());
        }

        [TestMethod]
        public void Reset_ShouldRestartFromFirstRecord()
        {
            var provider = new FileDataProvider(CreateFile("reset.txt", "first", "second", "third"));
            provider.ReadNextRecord();
            provider.ReadNextRecord();
            provider.Reset();
            Assert.AreEqual("first", provider.ReadNextRecord());
            Assert.AreEqual("second", provider.ReadNextRecord());
        }

        [TestMethod]
        public void ReadNextRecord_ShouldReturnNullForEmptyFile()
        {
            var provider = new FileDataProvider(CreateFile("empty.txt"));
            Assert.IsNull(provider.ReadNextRecord());
            Assert.IsNull(provider.ReadNextRecord());
        }

        [TestMethod]
        public void ReadNextRecord_ShouldReturnNullForMissingFile()
        {
            var provider = new FileDataProvider(Path.Combine(_directory, "missing.txt"));
            Assert.IsNull(provider.ReadNextRecord());
        }

        [TestMethod]
        public void ReadNextRecord_ShouldAdvancePastBlankRecords()
        {
            var provider = new FileDataProvider(CreateFile("blank.txt", "first", "", "   ", "last"));
            Assert.AreEqual("first", provider.ReadNextRecord());
            Assert.IsNull(provider.ReadNextRecord());
            Assert.IsNull(provider.ReadNextRecord());
            Assert.AreEqual("last", provider.ReadNextRecord());
            Assert.AreEqual("first", provider.ReadNextRecord());
        }

        [TestMethod]
        public void Providers_ShouldMaintainIndependentPositions()
        {
            string path = CreateFile("shared.txt", "first", "second");
            var first = new FileDataProvider(path);
            var second = new FileDataProvider(path);
            Assert.AreEqual("first", first.ReadNextRecord());
            Assert.AreEqual("second", first.ReadNextRecord());
            Assert.AreEqual("first", second.ReadNextRecord());
            first.Reset();
            Assert.AreEqual("second", second.ReadNextRecord());
        }

        [TestMethod]
        public void UnavailableSource_ShouldNotInterruptOperationalProvider()
        {
            string path = CreateFile("available.txt", "first", "second");
            var unavailable = new FileDataProvider(Path.Combine(_directory, "missing.txt"));
            var operational = new FileDataProvider(path);
            Assert.AreEqual("first", operational.ReadNextRecord());
            Assert.IsNull(unavailable.ReadNextRecord());
            Assert.AreEqual("second", operational.ReadNextRecord());
            Assert.AreEqual("first", operational.ReadNextRecord());
        }

        [TestMethod]
        public void ReadNextRecord_ShouldRecoverWhenSourceBecomesAvailable()
        {
            string path = Path.Combine(_directory, "recover.txt");
            var provider = new FileDataProvider(path);
            Assert.IsNull(provider.ReadNextRecord());
            File.WriteAllLines(path, new[] { "restored" });
            Assert.AreEqual("restored", provider.ReadNextRecord());
        }

        [TestMethod]
        public void ReadNextRecord_ShouldReportSourceDisappearingAfterValidRead()
        {
            string path = CreateFile("removed.txt", "first", "second");
            var provider = new FileDataProvider(path);
            Assert.AreEqual("first", provider.ReadNextRecord());
            File.Delete(path);
            Assert.IsNull(provider.ReadNextRecord());
        }
    }
}
