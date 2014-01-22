using System;
using System.Text.RegularExpressions;

namespace JunkDrawer
{
    public static class Utility
    {
        public const string CleanPattern = @"[\s\-]|^[\d]";

        public static string CleanIdentifier(string input) {
            return Regex.Replace(input, CleanPattern, String.Empty).Trim(' ');
        }
    }
}