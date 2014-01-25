using System.Collections.Generic;
using System.IO;
using System.Linq;
using Transformalize.Libs.ExcelDataReader;

namespace JunkDrawer {

    public class ExcelInformationReader {

        public FileInformation Read(string fileName) {

            var fileInformation = new FileInformation(fileName, FileType.Excel);

            var columnNames = new List<string>();

            var stream = File.Open(fileInformation.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var isXml = fileInformation.FileExtension.Equals(".xlsx");

            var excelReader = isXml ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
            excelReader.Read();
            for (var i = 0; i < excelReader.FieldCount; i++) {
                columnNames.Add(excelReader.GetString(i));
            }

            excelReader.Close();
            fileInformation.ColumnNames = columnNames.Select(Utility.CleanIdentifier);
            return fileInformation;
        }
    }
}