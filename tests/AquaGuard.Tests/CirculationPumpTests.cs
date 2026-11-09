using AquaGuard.Devices.Equipment;
using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Tests
{
    [TestClass]
    public class CirculationPumpTests
    {
        [TestMethod]
        public void Constructor_ShouldCreatePumpAsOff()
        {
            var pump = new CirculationPump("P001", "H001", "Main Pump");

            Assert.IsFalse(pump.IsOn);
            Assert.AreEqual(0, pump.SpeedPercentage);
        }

        [TestMethod]
        public void TurnOn_ShouldSetPumpOn()
        {
            var pump = new CirculationPump("P001", "H001", "Main Pump");

            pump.TurnOn();

            Assert.IsTrue(pump.IsOn);
        }

        [TestMethod]
        public void TurnOff_ShouldSetPumpOff()
        {
            var pump = new CirculationPump("P001", "H001", "Main Pump");

            pump.TurnOn();
            pump.TurnOff();

            Assert.IsFalse(pump.IsOn);
        }

        [TestMethod]
        public void SetSpeed_ShouldUpdateSpeed()
        {
            var pump = new CirculationPump("P001", "H001", "Main Pump");

            pump.SetSpeed(75);

            Assert.AreEqual(75, pump.SpeedPercentage);
        }

        [TestMethod]
        public void SetSpeed_ShouldRejectNegativeValue()
        {
            var pump = new CirculationPump("P001", "H001", "Main Pump");

            Assert.Throws<ArgumentOutOfRangeException>(() => pump.SetSpeed(-10));
        }

        [TestMethod]
        public void SetSpeed_ShouldRejectValueAbove100()
        {
            var pump = new CirculationPump("P001", "H001", "Main Pump");

            Assert.Throws<ArgumentOutOfRangeException>(() => pump.SetSpeed(110));
        }

        [TestMethod]
        public void SetSpeed_InvalidValue_ShouldKeepPreviousSpeed()
        {
            var pump = new CirculationPump("P001", "H001", "Main Pump");

            pump.SetSpeed(60);

            Assert.Throws<ArgumentOutOfRangeException>(() => pump.SetSpeed(150));

            Assert.AreEqual(60, pump.SpeedPercentage);
        }
    }
}
