using System;
using System.Collections.Generic;
using System.Text;


using AquaGuard.Devices.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class AeratorTests
    {
        [TestMethod]
        public void Constructor_ShouldCreateAeratorAsOff()
        {
            var aerator = new Aerator("A001", "H001", "Main Aerator");

            Assert.IsFalse(aerator.IsOn);
            Assert.AreEqual(0, aerator.OutputPercentage);
        }

        [TestMethod]
        public void TurnOn_ShouldSetAeratorOn()
        {
            var aerator = new Aerator("A001", "H001", "Main Aerator");

            aerator.TurnOn();

            Assert.IsTrue(aerator.IsOn);
            Assert.AreEqual(100, aerator.OutputPercentage);
        }

        [TestMethod]
        public void TurnOff_ShouldSetAeratorOff()
        {
            var aerator = new Aerator("A001", "H001", "Main Aerator");

            aerator.TurnOn();
            aerator.TurnOff();

            Assert.IsFalse(aerator.IsOn);
            Assert.AreEqual(0, aerator.OutputPercentage);
        }

        [TestMethod]
        public void SetOutput_ShouldAcceptValidPercentage()
        {
            var aerator = new Aerator("A001", "H001", "Main Aerator");

            aerator.SetOutput(50);

            Assert.AreEqual(50, aerator.OutputPercentage);
            Assert.IsTrue(aerator.IsOn);
        }

        [TestMethod]
        public void SetOutput_Zero_ShouldTurnAeratorOff()
        {
            var aerator = new Aerator("A001", "H001", "Main Aerator");

            aerator.TurnOn();
            aerator.SetOutput(0);

            Assert.IsFalse(aerator.IsOn);
            Assert.AreEqual(0, aerator.OutputPercentage);
        }

        [TestMethod]
        public void SetOutput_100_ShouldSetMaximumOutput()
        {
            var aerator = new Aerator("A001", "H001", "Main Aerator");

            aerator.SetOutput(100);

            Assert.AreEqual(100, aerator.OutputPercentage);
            Assert.IsTrue(aerator.IsOn);
        }

        [TestMethod]
        public void SetOutput_Negative_ShouldThrowException()
        {
            var aerator = new Aerator("A001", "H001", "Main Aerator");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => aerator.SetOutput(-1));
        }

        [TestMethod]
        public void SetOutput_Above100_ShouldThrowException()
        {
            var aerator = new Aerator("A001", "H001", "Main Aerator");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => aerator.SetOutput(101));
        }

        [TestMethod]
        public void Constructor_EmptyDeviceId_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(
                () => new Aerator("", "H001", "Main Aerator"));
        }
    }
}
