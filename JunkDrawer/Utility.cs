using System;
using System.Text.RegularExpressions;
using Transformalize.Extensions;

namespace JunkDrawer {
    public static class Utility {
        public const string CleanPattern = @"[\s\-]|^[\d]";

        public static string CleanIdentifier(string input) {
            return Regex.Replace(input, CleanPattern, String.Empty).Trim(' ');
        }

        public static string LogLength(string value, int totalWidth = 20) {
            return value.Length > totalWidth ? value.Left(totalWidth) : value.PadRight(totalWidth, '.');
        }
    }
}