using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AquaGuard.Core;

namespace AquaGuard.Tests
{
    [TestClass]
    public class HabitatTests
    {
        [TestMethod]
        public void Habitat_ShouldStoreIdAndName()
        {
            // Arrange
            var habitat = new Habitat("HAB001", "Coral Reef");

            // Assert
            Assert.AreEqual("HAB001", habitat.HabitatId);
            Assert.AreEqual("Coral Reef", habitat.Name);
        }

        [DataTestMethod]
        [DataRow("", "Coral Reef")]
        [DataRow("HAB001", "")]
        [DataRow(" ", "Coral Reef")]
        [DataRow("HAB001", " ")]
        public void Habitat_ShouldRejectEmptyRequiredFields(
            string habitatId, string name)
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                new Habitat(habitatId, name));
        }

    }
}
