using System;
using System.Collections.Generic;
using AquaGuard.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AquaGuard.Tests
{
    [TestClass]
    public class SafeRangeEngineTests
    {
        private SafeRangeEngine engine = null!;

        [TestInitialize]
        public void Setup()
        {
            engine = new SafeRangeEngine();
        }

        [TestMethod]
        public void Evaluate_ValueWithinRange_ShouldReturnNormal()
        {
            var result = engine.Evaluate(26, 24, 28);

            Assert.AreEqual(RangeStatus.Normal, result);
        }

        [TestMethod]
        public void Evaluate_ValueBelowRange_ShouldReturnTooLow()
        {
            var result = engine.Evaluate(22, 24, 28);

            Assert.AreEqual(RangeStatus.TooLow, result);
        }

        [TestMethod]
        public void Evaluate_ValueAboveRange_ShouldReturnTooHigh()
        {
            var result = engine.Evaluate(30, 24, 28);

            Assert.AreEqual(RangeStatus.TooHigh, result);
        }

        [TestMethod]
        public void Evaluate_MinimumBoundary_ShouldReturnNormal()
        {
            var result = engine.Evaluate(24, 24, 28);

            Assert.AreEqual(RangeStatus.Normal, result);
        }

        [TestMethod]
        public void Evaluate_MaximumBoundary_ShouldReturnNormal()
        {
            var result = engine.Evaluate(28, 24, 28);

            Assert.AreEqual(RangeStatus.Normal, result);
        }

        [TestMethod]
        public void Evaluate_InvalidRange_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(
                () => engine.Evaluate(26, 28, 24));
        }

        [TestMethod]
        public void Evaluate_NullReading_ShouldReturnNoData()
        {
            var result = engine.Evaluate(null, 24, 28);

            Assert.AreEqual(RangeStatus.NoData, result);
        }

        [TestMethod]
        public void Evaluate_NaN_ShouldReturnNoData()
        {
            var result = engine.Evaluate(double.NaN, 24, 28);

            Assert.AreEqual(RangeStatus.NoData, result);
        }

        [TestMethod]
        public void Evaluate_Infinity_ShouldReturnNoData()
        {
            var result = engine.Evaluate(
                double.PositiveInfinity, 24, 28);

            Assert.AreEqual(RangeStatus.NoData, result);
        }

        [TestMethod]
        public void Evaluate_DecimalValue_ShouldReturnNormal()
        {
            var result = engine.Evaluate(7.5, 7.0, 8.5);

            Assert.AreEqual(RangeStatus.Normal, result);
        }
    }
}

