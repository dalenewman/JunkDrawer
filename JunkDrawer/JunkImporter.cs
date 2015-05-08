using System.IO;
using System.Linq;
using Transformalize.Logging;
using Transformalize.Main.Providers.File;

namespace JunkDrawer {

    public class JunkImporter {

        public Response Import(Request request, ILogger logger) {

            var connection = request.Cfg.Connections.First();
            var fileInspection = request.Cfg.FileInspection.First().GetInspectionRequest(request.FileInfo.FullName);

            // If TableName is set, make it the entity name, which is what Tfl uses for the name of the table
            if (!string.IsNullOrEmpty(request.TableName)) {
                fileInspection.EntityName = request.TableName;
            }

            var name = Path.GetFileNameWithoutExtension(request.FileInfo.Name);
            logger.EntityInfo(name, "Default data type is {0}.", fileInspection.DefaultType);
            logger.EntityInfo(name, "Default string data type length is {0}.", fileInspection.DefaultLength);
            logger.EntityInfo(name, "Inspecting for {0} data types.", fileInspection.DataTypes.Count);

            var records = new FileImporter(logger).ImportScaler(fileInspection, connection);
            return new Response() { TableName = fileInspection.EntityName, Records = records };

        }
    }
}