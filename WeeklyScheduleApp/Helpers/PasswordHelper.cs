using System.Security.Cryptography;
using System.Text;

namespace WeeklyScheduleApp.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            var hashOfInput = HashPassword(inputPassword);
            return hashOfInput == storedHash;
        }
    }
}