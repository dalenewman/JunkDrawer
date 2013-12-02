using System;
using System.IO;

namespace JunkDrawer {
    public static class FileInformationFactory {

        public static FileInformation Create(Request request, int sampleSize = 5)
        {
            var fileName = request.FileInfo.FullName;
            var ext = (Path.GetExtension(fileName) ?? string.Empty).ToLower();

            var fi = ext.StartsWith(".xls") ?
                new ExcelInformationReader().Read(fileName) :
                new FileInformationReader(sampleSize).Read(fileName);

            if (request.FirstRowIsHeader)
                return fi;

            fi.ColumnNames.Clear();
            for (var i = 0; i < fi.ColumnCount; i++) {
                fi.ColumnNames.Add(GetColumnNameFromIndex(i));
            }

            return fi;
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