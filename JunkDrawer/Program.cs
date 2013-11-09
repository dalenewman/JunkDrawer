using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Transformalize.Configuration.Builders;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {
    class Program {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args) {

            var fileName = string.Join(" ", args);
            if (string.IsNullOrEmpty(fileName)) {
                Log.Error(@"Please provide a file name (i.e. jd c:\junk\initial\header\temp.txt).");
                Environment.Exit(1);
            }

            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists) {
                Log.Error("File '{0}' does not exist!", fileInfo.FullName);
                Environment.Exit(1);
            }

            var folders = string.Join(@"\", (fileInfo.DirectoryName ?? string.Empty).ToLower());
            var hasHeaders = folders.Contains(TryGetSetting("HeaderFolder", "Header"));
            var options = folders.Contains(TryGetSetting("InitFolder", "Initial")) ? new Options { Mode = "init" } : new Options();
            var fi = new FileInformationReader(5).Read(fileInfo.FullName);

            var builder = new ProcessBuilder(fi.ProcessName)
                .Connection("input").Provider("File").File(fileInfo.FullName).Delimiter(fi.Delimiter).Start(hasHeaders ? 2 : 1)
                .Connection("output").Database("Junk")
                .Entity("Data");

            for (var i = 0; i < fi.ColumnCount; i++) {
                builder.Field(fi.ColumnNames[i]).Length(255);
            }

            ProcessFactory.Create(builder.Process(), options).Run();

        }

        private static string TryGetSetting(string key, string alternate) {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.Equals(key, StringComparison.OrdinalIgnoreCase))) {
                return ConfigurationManager.AppSettings[key].ToLower();
            }
            return alternate.ToLower();
        }

    }
}
