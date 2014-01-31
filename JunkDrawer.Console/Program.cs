using System;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {

    public class Program {

        static void Main(string[] args) {

            GlobalDiagnosticsContext.Set("process", Common.LogLength("JunkDrawer", 16));
            GlobalDiagnosticsContext.Set("entity", Common.LogLength("Request"));

            var request = new Request(args);

            if (!request.IsValid) {
                LogManager.GetLogger(string.Empty).Error(request.Message);
                Environment.Exit(1);
            }

            new FileImporter().Import(request.FileInfo);
        }

    }
}
