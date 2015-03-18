using System.Linq;
using Transformalize.Logging;
using Transformalize.Main.Providers.File;

namespace JunkDrawer {

    public class JunkImporter {

        public Response Import(Request request) {

            var connection = request.Cfg.Connections.First();
            var fileInspection = request.Cfg.FileInspection.First().GetInspectionRequest(request.FileInfo.FullName);

            // If TableName is set, make it the entity name, which is what Tfl uses for the name of the table
            if (!string.IsNullOrEmpty(request.TableName)) {
                fileInspection.EntityName = request.TableName;
            }

            TflLogger.Info("JunkDrawer", request.FileInfo.Name, "Default data type is {0}.", fileInspection.DefaultType);
            TflLogger.Info("JunkDrawer", request.FileInfo.Name, "Default string data type length is {0}.", fileInspection.DefaultLength);
            TflLogger.Info("JunkDrawer", request.FileInfo.Name, "Inspecting for {0} data types.", fileInspection.DataTypes.Count);

            var records = new FileImporter().ImportScaler(fileInspection, connection);
            return new Response() { TableName = fileInspection.EntityName, Records = records };

        }
    }
}