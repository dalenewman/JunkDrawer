using System.IO;
using Transformalize.Configuration;
using Transformalize.Logging;
using Transformalize.Main.Providers.File;

namespace JunkDrawer {

    public class JunkImporter {

        public void Import(FileInfo fileInfo) {
            var configuration = new ConfigurationFactory("JunkDrawer").CreateSingle();
            var fileInspection = configuration.FileInspection.GetInspectionRequest();
            var connection = configuration.Connections["output"];

            TflLogger.Info("JunkDrawer", fileInfo.Name, "Default data type is {0}.", fileInspection.DefaultType);
            TflLogger.Info("JunkDrawer", fileInfo.Name, "Default string data type length is {0}.", fileInspection.DefaultLength);
            TflLogger.Info("JunkDrawer", fileInfo.Name, "Inspecting for {0} data types.", fileInspection.DataTypes.Count);
            foreach (var type in fileInspection.DataTypes) {
                TflLogger.Info("JunkDrawer", fileInfo.Name, "Inspecting for data type: {0}.", type);
            }

            new FileImporter().ImportScaler(fileInfo, fileInspection, connection);
        }
    }
}