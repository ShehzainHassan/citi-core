using System.Security.Cryptography;
using System.Text;

namespace citi_core.Utilities
{
    public static class AccountNumberGenerator
    {
        public static string Generate()
        {
            var prefix = "ACC";
            var randomNumber = GenerateRandomDigits(9);
            return $"{prefix}{randomNumber}";
        }

        private static string GenerateRandomDigits(int length)
        {
            var digits = new StringBuilder();
            using var rng = RandomNumberGenerator.Create();

            while (digits.Length < length)
            {
                var byteArray = new byte[1];
                rng.GetBytes(byteArray);
                var digit = byteArray[0] % 10;
                digits.Append(digit);
            }

            return digits.ToString();
        }
    }

}
