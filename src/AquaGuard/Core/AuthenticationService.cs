using System.Security.Cryptography;
using System.Text;

namespace AquaGuard.Core
{
    public sealed class AuthenticationService
    {
        private readonly byte[]? staffCodeHash;

        public AuthenticationService(string? staffCodeHashHex)
        {
            if (staffCodeHashHex is { Length: 64 } && staffCodeHashHex.All(Uri.IsHexDigit))
            {
                staffCodeHash = Convert.FromHexString(staffCodeHashHex);
            }
        }

        public bool IsConfigured => staffCodeHash is not null;

        private bool isStaff;

        public bool TrySignIn(string? code)
        {
            isStaff = ValidateStaffCode(code);
            return isStaff;
        }

        internal void RequireStaff()
        {
            if (!isStaff)
            {
                throw new UnauthorizedAccessException("Staff sign-in is required.");
            }
        }

        public bool ValidateStaffCode(string? code)
        {
            return staffCodeHash is not null
                && !string.IsNullOrWhiteSpace(code)
                && CryptographicOperations.FixedTimeEquals(
                    staffCodeHash, SHA256.HashData(Encoding.UTF8.GetBytes(code)));
        }
    }
}
