using System.Collections.Generic;
using System.IO;
using System.Linq;
using Transformalize.Libs.ExcelDataReader;

namespace JunkDrawer
{
    public class ExcelInformationAppender {

        public FileInformation Append(FileInformation fileInformation) {

            var columnNames = new List<string>();

            var stream = File.Open(fileInformation.FileName, FileMode.Open, FileAccess.Read);
            var isXml = fileInformation.FileExtenstion.Equals(".xlsx");

            var excelReader = isXml ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
            excelReader.Read();
            fileInformation.ColumnCount = excelReader.FieldCount;
            for (var i = 0; i < fileInformation.ColumnCount; i++) {
                columnNames.Add(excelReader.GetString(i));
            }

            excelReader.Close();
            fileInformation.ColumnNames = columnNames.Select(s=>s.Replace(" ",string.Empty).Trim(' ')).ToArray();
            return fileInformation;
        }
    }
}