using System.Collections.Generic;
using System.IO;
using System.Linq;
using Transformalize.Libs.ExcelDataReader;

namespace JunkDrawer {

    public class ExcelInformationReader {

        public FileInformation Read(string fileName) {

            var fileInformation = new FileInformation(fileName, FileType.Excel, 0);

            var columnNames = new List<string>();

            var stream = File.Open(fileInformation.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var isXml = fileInformation.FileExtension.Equals(".xlsx");

            var excelReader = isXml ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
            excelReader.Read();
            fileInformation.ColumnCount = excelReader.FieldCount;
            for (var i = 0; i < fileInformation.ColumnCount; i++) {
                columnNames.Add(excelReader.GetString(i));
            }

            excelReader.Close();
            fileInformation.ColumnNames.AddRange(columnNames.Select(Utility.CleanIdentifier).ToArray());
            return fileInformation;
        }
    }
}