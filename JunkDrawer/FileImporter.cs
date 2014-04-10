using System;
using System.IO;
using Transformalize.Configuration.Builders;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {
    public class FileImporter {
        private readonly Logger _log = LogManager.GetLogger("JunkDrawer.FileImporter");

        public void Import(FileInfo fileInfo, InspectionRequest request) {

            var fileInformation = FileInformationFactory.Create(fileInfo);
            var defaultProcess = ProcessFactory.Create("JunkDrawer")[0];

            var entityName = fileInformation.Identifier();

            var builder = new ProcessBuilder(entityName)
                .Star(fileInformation.ProcessName)
                .Connection("input")
                    .Provider("file")
                    .File(fileInformation.FileInfo.FullName)
                    .Delimiter(fileInformation.Delimiter)
                    .Start(fileInformation.FirstRowIsHeader ? 2 : 1)
                .Connection("output")
                    .ConnectionString(defaultProcess.OutputConnection.GetConnectionString())
                    .Provider(defaultProcess.OutputConnection.Type.ToString().ToLower())
                .Entity(entityName)
                    .PrependProcessNameToOutputName(false)
                    .DetectChanges(false);

            var fieldTypes = new FieldInspector().Inspect(fileInformation, request);

            foreach (var fieldType in fieldTypes) {
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
            ProcessFactory.Create(builder.Process())[0].Run();
        }

    }
}