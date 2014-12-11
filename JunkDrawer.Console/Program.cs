using System;
using System.Diagnostics.Tracing;
using Transformalize.Libs.SemanticLogging;
using Transformalize.Logging;

namespace JunkDrawer {

    public class Program {
        private const string PROCESS_NAME = "JunkDrawer";

        static void Main(string[] args) {

            var listener = new ObservableEventListener();
            listener.EnableEvents(TflEventSource.Log, EventLevel.Informational);
            var subscription = listener.LogToConsole(new LegacyLogFormatter());

            var request = new Request(args);
            if (!request.IsValid) {
                TflLogger.Error(PROCESS_NAME, request.FileInfo.Name, request.Message);
                Environment.Exit(1);
            }

            try {
                new JunkImporter().Import(request.FileInfo);
            } catch (Exception ex) {
                TflLogger.Error(PROCESS_NAME, request.FileInfo.Name, ex.Message);
                TflLogger.Debug(PROCESS_NAME, request.FileInfo.Name, ex.StackTrace);
            }

            subscription.Dispose();
            listener.DisableEvents(TflEventSource.Log);
            listener.Dispose();
        }

    }
}
