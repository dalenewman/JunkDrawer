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

            return fi;
        }
    }
}