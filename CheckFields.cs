using System.Text.RegularExpressions;

namespace IPK_2024_1
{
    internal abstract class CheckFields
    {
        private const string NonZeroPattern = @"^[A-Za-z0-9\-]+$";
        private const string PrintablePattern = @"^[\x21-\x7E]+$";
        private const string PrintableWithSpacePattern = @"^[\x20-\x7E]+$";

        private static bool CheckType(string str, int length, string pattern) =>
            (str.Length <= length && Regex.IsMatch(str, pattern));

        public static bool CheckUserName(string str) => CheckType(str, 20, NonZeroPattern);

        public static bool CheckChannelId(string str) => CheckType(str, 20, NonZeroPattern);

        public static bool CheckSecret(string str) => CheckType(str, 128, NonZeroPattern);

        public static bool CheckDisplayName(string str) => CheckType(str, 20, PrintablePattern);

        public static bool CheckMessageContent(string str) => CheckType(str, 1400, PrintableWithSpacePattern);
    }
}
