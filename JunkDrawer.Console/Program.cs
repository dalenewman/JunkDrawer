using System;
using System.Configuration;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {

    public class Program {

        static void Main(string[] args) {

            GlobalDiagnosticsContext.Set("process", Common.LogLength("JunkDrawer", 16));
            GlobalDiagnosticsContext.Set("entity", Common.LogLength("Request"));

            var request = new Request(args);

            var logger = LogManager.GetLogger(string.Empty);
            if (!request.IsValid) {
                logger.Error(request.Message);
                LogManager.Flush();
                Environment.Exit(1);
            }

            var configuration = ConfigurationFactory.Create();

            logger.Info("Default data type is {0}.", configuration.DefaultType);
            logger.Info("Default string data type length is {0}.", configuration.DefaultLength);
            logger.Info("Inspecting for {0} data types in the top {1} records.", configuration.DataTypes.Count, configuration.Top);
            foreach (var type in configuration.DataTypes) {
                logger.Info("Inspecting for data type: {0}.", type);
            }

            new FileImporter().Import(request.FileInfo, configuration);
            new JunkReporter().Report();
        }

    }
}
