using System.IO;

namespace JunkDrawer {

    public class FileInformationReader {

        private readonly int _sampleSize;

        public FileInformationReader(int sampleSize) {
            _sampleSize = sampleSize;
        }

        public FileInformation Read(FileInfo fileInfo) {

            var lines = new Lines(fileInfo, _sampleSize);
            var bestDelimiter = lines.FindDelimiter();

            return new FileInformation(fileInfo) {
                FileType = bestDelimiter.FileType,
                Fields = lines.InitialFieldTypes()
            };
        }
    }
}

