using System.IO;
using System.Linq;
using Transformalize.Logging;
using Transformalize.Main.Providers.File;

namespace JunkDrawer {

    public class JunkImporter {
        private readonly ILogger _logger;

        public JunkImporter(ILogger logger) {
            _logger = logger;
        }

        public Response Import(Request request) {

            var connection = request.Cfg.Connections.First();
            var fileInspection = request.Cfg.FileInspection.First().GetInspectionRequest(request.FileInfo.FullName);

            // If TableName is set, make it the entity name, which is what Tfl uses for the name of the table
            if (!string.IsNullOrEmpty(request.TableName)) {
                fileInspection.EntityName = request.TableName;
            }

            var name = Path.GetFileNameWithoutExtension(request.FileInfo.Name);
            _logger.EntityInfo(name, "Default data type is {0}.", fileInspection.DefaultType);
            _logger.EntityInfo(name, "Default string data type length is {0}.", fileInspection.DefaultLength);
            _logger.EntityInfo(name, "Inspecting for {0} data types.", fileInspection.DataTypes.Count);

            var records = new FileImporter(_logger).ImportScaler(fileInspection, connection);
            return new Response() { TableName = fileInspection.EntityName, Records = records };
        }
    }
}