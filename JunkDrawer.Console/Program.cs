using System;
using Transformalize.Configuration;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {

    public class Program {

        static void Main(string[] args) {

            GlobalDiagnosticsContext.Set("process", Common.LogLength("JunkDrawer", 16));
            GlobalDiagnosticsContext.Set("entity", Common.LogLength("Request"));

            var request = new Request(args);
            if (!request.IsValid) {
                LogManager.GetLogger("JunkDrawer").Error(request.Message);
                LogManager.Flush();
                Environment.Exit(1);
            }

            new JunkImporter().Import(request.FileInfo);
            new JunkReporter().Report();
        }

    }
}
