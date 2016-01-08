using Pipeline.Extensions;

namespace JunkDrawer {
    public static class Utility {
        public static string LogLength(string value, int totalWidth = 20) {
            return value.Length > totalWidth ? value.Left(totalWidth) : value.PadRight(totalWidth, '.');
        }
    }
}