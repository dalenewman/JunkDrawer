using System.IO;

namespace JunkDrawer {
    public static class FileInformationFactory {

        public static FileInformation Create(string fileName, int sampleSize = 5) {

            var ext = (Path.GetExtension(fileName) ?? string.Empty).ToLower();

            var fileInformation = ext.StartsWith(".xls") ?
                new ExcelInformationReader().Read(fileName) :
                new FileInformationReader(sampleSize).Read(fileName);

            var validator = new ColumnNameValidator(fileInformation.ColumnNames);
            if (validator.Valid())
                return fileInformation;

            fileInformation.FirstRowIsHeader = false;
            fileInformation.ColumnNames = new ColumnNameGenerator().Generate(fileInformation.ColumnCount());
            return fileInformation;
        }
    }
}