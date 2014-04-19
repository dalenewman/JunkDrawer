using System.Globalization;
using System.IO;

namespace JunkDrawer {

    public class FileInformationReader {
        private readonly InspectionRequest _request;

        public FileInformationReader(InspectionRequest request)
        {
            _request = request;
        }

        public FileInformation Read(FileInfo fileInfo) {

            var lines = new Lines(fileInfo, _request);
            var bestDelimiter = lines.FindDelimiter();

            return new FileInformation(fileInfo) {
                Delimiter = bestDelimiter,
                Fields = lines.InitialFieldTypes()
            };
        }
    }
}

