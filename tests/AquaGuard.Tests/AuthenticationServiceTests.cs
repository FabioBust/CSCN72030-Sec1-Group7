using System.Security.Cryptography;
using System.Text;
using AquaGuard.Core;

namespace AquaGuard.Tests
{
    [TestClass]
    public class AuthenticationServiceTests
    {
        private static readonly string CodeHash =
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("test-code")));

        [TestMethod]
        public void ValidCodeGrantsAccess()
        {
            Assert.IsTrue(new AuthenticationService(CodeHash).ValidateStaffCode("test-code"));
        }

        [TestMethod]
        [DataRow("wrong-code")]
        [DataRow("")]
        [DataRow(null)]
        public void InvalidCodeDeniesAccess(string? code)
        {
            Assert.IsFalse(new AuthenticationService(CodeHash).ValidateStaffCode(code));
        }

        [TestMethod]
        [DataRow("invalid-hash")]
        [DataRow(null)]
        public void MissingOrMalformedConfigurationDeniesAccess(string? hash)
        {
            var service = new AuthenticationService(hash);

            Assert.IsFalse(service.IsConfigured);
            Assert.IsFalse(service.ValidateStaffCode("test-code"));
        }
    }
}
