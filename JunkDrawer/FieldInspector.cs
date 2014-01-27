using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Transformalize.Configuration.Builders;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {
    public class FieldInspector {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        public FieldType[] Inspect(FileInformation fileInformation, int sampleSize = 0) {
            return InspectFile(fileInformation, sampleSize).ToArray();
        }

        private IEnumerable<FieldType> InspectFile(FileInformation fileInformation, int sampleSize) {

            var builder = new ProcessBuilder("DataTypeCheck")
                .Connection("input")
                    .Provider("file")
                    .File(fileInformation.FileName)
                    .Delimiter(fileInformation.Delimiter)
                    .Start(fileInformation.FirstRowIsHeader ? 2 : 1)
                .Connection("output")
                    .Provider("internal")
                .Entity("Data");

            foreach (var name in fileInformation.ColumnNames) {
                builder.Field(name).Length(512);
            }

            var dataTypes = new[] { "Boolean", "Int32", "DateTime" };

            foreach (var dataType in dataTypes) {
                foreach (var name in fileInformation.ColumnNames) {
                    var result = IsDataTypeField(name, dataType);
                    builder.CalculatedField(result).Bool()
                        .Transform("typeconversion")
                        .Type(dataType)
                        .ResultField(result)
                        .MessageField(string.Empty)
                        .Parameter(name);
                }
            }

            foreach (var name in fileInformation.ColumnNames) {
                var result = name + "Length";
                builder.CalculatedField(result).Int32()
                    .Transform("length")
                    .Parameter(name);
            }

            _log.Debug("Process Configuration...", Environment.NewLine);
            _log.Debug(Environment.NewLine + builder.Process().Serialize());

            var runner = ProcessFactory.Create(builder.Process(), new Options() { Top = sampleSize });
            var results = runner.Run().First().ToList();

            var fieldTypes = new List<FieldType>();
            foreach (var name in fileInformation.ColumnNames) {
                var foundMatch = false;
                foreach (var dataType in dataTypes) {
                    var result = IsDataTypeField(name, dataType);
                    if (!foundMatch && results.All(r => r[result].Equals(true))) {
                        fieldTypes.Add(new FieldType(name, dataType) { Length = string.Empty });
                        foundMatch = true;
                    }
                }
                if (!foundMatch) {
                    var length = results.Max(r => (int)r[name + "Length"]);
                    if (length == 0)
                        length = 1;
                    fieldTypes.Add(new FieldType(name, "string") { Length = length.ToString(CultureInfo.InvariantCulture) });
                }
            }
            return fieldTypes;
        }

        private static string IsDataTypeField(string name, string dataType) {
            return name + "Is" + dataType;
        }

    }
}