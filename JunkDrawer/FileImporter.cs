using System;
using System.IO;
using Transformalize.Configuration.Builders;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {
    public class FileImporter {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public void Import(FileInfo fileInfo) {

            var fileInformation = FileInformationFactory.Create(fileInfo);
            var defaultProcess = ProcessFactory.Create("Default");

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
                .Entity(entityName).PrependProcessNameToOutputName(false);

            foreach (var fieldType in fileInformation.InspectedFieldTypes()) {
                if (fieldType.Type.Equals("string")) {
                    builder
                        .Field(fieldType.Name)
                            .Length(fieldType.Length)
                            .QuotedWith(fieldType.QuoteString());
                } else {
                    builder
                        .Field(fieldType.Name)
                        .Type(fieldType.Type)
                        .QuotedWith(fieldType.QuoteString())
                        .Transform("convert")
                            .To(fieldType.Type);
                }
            }

            _log.Debug(builder.Process().Serialize().Replace(Environment.NewLine, string.Empty));

            ProcessFactory.Create(builder.Process(), new Options() { Mode = "init" }).Run();
            ProcessFactory.Create(builder.Process(), new Options() { Mode = "default" }).Run();

            //File.Move(fileInformation.FileName, fileInformation.FileName + ".bak");
        }

    }
}