using System;
using System.Collections.Generic;
using System.IO;

namespace JunkDrawer {
    /// <summary>Finds the arrangement shared by the desktop and command-line apps.</summary>
    public static class ArrangementLocator {
        public static string Resolve(string selected = null) {
            return Resolve(
                selected,
                Environment.GetEnvironmentVariable("JUNKDRAWER_CONFIG"),
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory,
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        public static string Resolve(
            string selected,
            string environmentSelection,
            string workingDirectory,
            string applicationDirectory,
            string homeDirectory) {
            var personalDirectory = Path.Combine(homeDirectory, ".junkdrawer");
            var choice = !string.IsNullOrWhiteSpace(selected) ? selected : environmentSelection;
            if (!string.IsNullOrWhiteSpace(choice)) {
                choice = HomePath.Expand(choice.Trim(), homeDirectory);

                var candidates = Path.IsPathRooted(choice) || choice.IndexOfAny(new[] { '/', '\\' }) >= 0
                    ? new[] { Path.GetFullPath(Path.IsPathRooted(choice) ? choice : Path.Combine(workingDirectory, choice)) }
                    : new[] {
                        Path.Combine(workingDirectory, choice),
                        Path.Combine(applicationDirectory, choice),
                        Path.Combine(personalDirectory, choice)
                    };
                foreach (var candidate in candidates) {
                    if (File.Exists(candidate)) return Path.GetFullPath(candidate);
                }
                throw new FileNotFoundException($"Arrangement '{choice}' was not found. Checked: {string.Join(", ", candidates)}.");
            }

            var defaults = new List<string>();
            foreach (var directory in new[] { workingDirectory, applicationDirectory, personalDirectory }) {
                defaults.Add(Path.Combine(directory, "config.xml"));
                defaults.Add(Path.Combine(directory, "sqlite.xml"));
            }
            defaults.Add(Path.Combine(applicationDirectory, "Examples", "sqlite.xml"));
            foreach (var candidate in defaults) {
                if (File.Exists(candidate)) return Path.GetFullPath(candidate);
            }
            throw new FileNotFoundException($"No arrangement found. Checked: {string.Join(", ", defaults)}.");
        }
    }
}
