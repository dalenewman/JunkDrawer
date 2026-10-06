using System;
using System.IO;

namespace JunkDrawer {
    public static class HomePath {
        public static string Expand(string path) =>
            Expand(path, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        public static string Expand(string path, string homeDirectory) {
            if (string.IsNullOrEmpty(path) || path[0] != '~' ||
                (path.Length > 1 && path[1] != '/' && path[1] != '\\')) return path;

            if (string.IsNullOrWhiteSpace(homeDirectory))
                throw new InvalidOperationException("The current user's home directory could not be found.");

            var remainder = path.Length == 1 ? string.Empty : path.Substring(2)
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(homeDirectory, remainder));
        }
    }
}
