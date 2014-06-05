using System.IO;
using Transformalize.Libs.NLog;
using Transformalize.Main.Providers.File;

namespace JunkDrawer
{
    public class JunkImporter {
        private readonly Logger _log = LogManager.GetLogger("JunkDrawer");
        public void Import(FileInfo fileInfo) {
            var configuration = JunkDrawerConfiguration.GetFileInspectionRequest();
            var connection = JunkDrawerConfiguration.GetTransformalizeConnection();

            _log.Info("Default data type is {0}.", configuration.DefaultType);
            _log.Info("Default string data type length is {0}.", configuration.DefaultLength);
            _log.Info("Inspecting for {0} data types in the top {1} records.", configuration.DataTypes.Count, configuration.Top);
            foreach (var type in configuration.DataTypes) {
                _log.Info("Inspecting for data type: {0}.", type);
            }

            new FileImporter().ImportScaler(fileInfo, configuration, connection);
        }
    }
}