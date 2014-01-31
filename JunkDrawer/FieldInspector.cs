using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Transformalize.Configuration.Builders;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {
    public class FieldInspector {

        private readonly Logger _log = LogManager.GetLogger(string.Empty);
        public List<Field> Inspect(FileInformation fileInformation, int sampleSize = 0) {
            return InspectFile(fileInformation, sampleSize);
        }

        private List<Field> InspectFile(FileInformation fileInformation, int sampleSize) {

            var builder = new ProcessBuilder("JDT" + fileInformation.Identifier().TrimStart("JDI".ToCharArray()))
                .Connection("input")
                    .Provider("file")
                    .File(fileInformation.FileInfo.FullName)
                    .Delimiter(fileInformation.Delimiter)
                    .Start(fileInformation.FirstRowIsHeader ? 2 : 1)
                .Connection("output")
                    .Provider("internal")
                .Entity("Data");

            foreach (var field in fileInformation.Fields) {
                builder
                    .Field(field.Name)
                        .Length(512)
                        .Type(field.Type)
                        .QuotedWith(field.QuoteString());
            }

            var dataTypes = new[] { "boolean", "byte", "int16", "int32", "int64", "single", "double", "decimal", "datetime" };

            foreach (var dataType in dataTypes) {
                foreach (var field in fileInformation.Fields) {
                    var result = IsDataTypeField(field.Name, dataType);
                    builder.CalculatedField(result).Bool()
                        .Transform("typeconversion")
                            .Type(dataType)
                            .ResultField(result)
                            .MessageField(string.Empty)
                            .Parameter(field.Name);
                }
            }

            foreach (var field in fileInformation.Fields) {
                var result = field.Name + "Length";
                builder.CalculatedField(result).Int32()
                    .Transform("length")
                    .Parameter(field.Name);
            }

            _log.Debug(builder.Process().Serialize().Replace(Environment.NewLine,string.Empty));

            var runner = ProcessFactory.Create(builder.Process(), new Options() { Top = sampleSize });
            var results = runner.Run().First().ToList();

            foreach (var field in fileInformation.Fields) {
                var foundMatch = false;
                foreach (var dataType in dataTypes) {
                    var result = IsDataTypeField(field.Name, dataType);
                    if (!foundMatch && results.All(row => row[result].Equals(true))) {
                        field.Type = dataType;
                        field.Length = string.Empty;
                        foundMatch = true;
                    }
                }
                if (!foundMatch) {
                    var length = results.Max(row => (int)row[field.Name + "Length"]);
                    if (length == 0)
                        length = 1;

                    field.Length = length.ToString(CultureInfo.InvariantCulture);
                }
            }

            return fileInformation.Fields;
        }

        private static string IsDataTypeField(string name, string dataType) {
            return name + "Is" + dataType;
        }

    }
}