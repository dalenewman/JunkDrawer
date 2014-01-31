using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Transformalize.Libs.ExcelDataReader;

namespace JunkDrawer {

    public class ExcelInformationReader {

        public FileInformation Read(FileInfo fileInfo) {

            var fileInformation = new FileInformation(fileInfo, FileType.Excel);

            var columnNames = new List<string>();

            var stream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var isXml = fileInfo.Extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase);

            var excelReader = isXml ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
            excelReader.Read();
            for (var i = 0; i < excelReader.FieldCount; i++) {
                var name = excelReader.GetString(i);
                if (name != null)
                    columnNames.Add(name);
            }

            excelReader.Close();
            foreach (var value in columnNames) {
                fileInformation.Fields.Add(new Field(value));
            }

            return fileInformation;
        }
    }
}