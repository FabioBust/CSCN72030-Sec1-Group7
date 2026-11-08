
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
    }
}
