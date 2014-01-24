using System;
using System.IO;
using System.Linq;

namespace JunkDrawer {
    public static class FileInformationFactory {

        public static FileInformation Create(string fileName, int sampleSize = 5) {

            var ext = (Path.GetExtension(fileName) ?? string.Empty).ToLower();

            var fileInformation = ext.StartsWith(".xls") ?
                new ExcelInformationReader().Read(fileName) :
                new FileInformationReader(sampleSize).Read(fileName);

            if (fileInformation.ColumnCount != fileInformation.ColumnNames.Distinct().Count())
            {
                fileInformation.FirstRowIsHeader = false;
                fileInformation.ColumnNames.Clear();
                for (var i = 0; i < fileInformation.ColumnCount; i++) {
                    fileInformation.ColumnNames.Add(GetColumnNameFromIndex(i));
                }                
            }

            return fileInformation;
        }

        public static string GetColumnNameFromIndex(int column) {
            var col = Convert.ToString((char)('A' + (column % 26)));
            while (column >= 26) {
                column = (column / 26) - 1;
                col = Convert.ToString((char)('A' + (column % 26))) + col;
            }
            return col;
        }

    }
}