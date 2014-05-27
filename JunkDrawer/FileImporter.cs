using System;
using System.Globalization;
using System.IO;
using Transformalize.Configuration.Builders;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {
    public class FileImporter {
        private readonly Logger _log = LogManager.GetLogger("JunkDrawer.FileImporter");

        public Result Import(string file, decimal sample = 100m) {
            return Import(new FileInfo(file), sample);
        }

        public Result Import(FileInfo fileInfo, decimal sample = 100m) {
            var request = ConfigurationFactory.Create();
            if (sample > 0m && sample < 100m) {
                request.Sample = sample;
            }
            return Import(fileInfo, request);
        }

        public Result Import(FileInfo fileInfo, InspectionRequest request) {

            var fileInformation = FileInformationFactory.Create(fileInfo, request);
            Process process;

            try {
                process = ProcessFactory.Create("JunkDrawer")[0];
            } catch (TransformalizeException tex) {
                _log.Error(tex.Message);
                throw new JunkDrawerException("You must define a JunkDrawer process with an 'output' connection defined in the transformalize configuration section.");
            }

            var entityName = fileInformation.Identifier();

            var builder = new ProcessBuilder(entityName)
                .Star(fileInformation.ProcessName)
                .Connection("input")
                    .Provider("file")
                    .File(fileInformation.FileInfo.FullName)
                    .Delimiter(fileInformation.Delimiter.ToString(CultureInfo.InvariantCulture))
                    .Start(fileInformation.FirstRowIsHeader ? 2 : 1)
                .Connection("output")
                    .Provider(process.OutputConnection.Type.ToString().ToLower())
                    .ConnectionString(process.OutputConnection.GetConnectionString())
                .Entity(entityName)
                    .PrependProcessNameToOutputName(false)
                    .DetectChanges(false);

            var fields = new FieldInspector().Inspect(fileInformation, request);

            foreach (var fieldType in fields) {
                if (fieldType.Type.Equals("string")) {
                    _log.Info("Using {0} character string for {1}.", fieldType.Length, fieldType.Name);
                } else {
                    _log.Info("Using {0} for {1}.", fieldType.Type, fieldType.Name);
                }

                builder
                    .Field(fieldType.Name)
                    .Length(fieldType.Length)
                    .Type(fieldType.Type)
                    .QuotedWith(fieldType.QuoteString());
            }

            _log.Debug(builder.Process().Serialize().Replace(Environment.NewLine, string.Empty));

            ProcessFactory.Create(builder.Process(), new Options { Mode = "init" })[0].Run();
            return new Result {
                Fields = fields,
                FileInformation = fileInformation,
                Rows = ProcessFactory.Create(builder.Process())[0].Run()[entityName]
            };
        }

    }
}