using System.Linq;

namespace JunkDrawer {

    public class FileInformationReader {

        private readonly int _sampleSize;

        public FileInformationReader(int sampleSize) {
            _sampleSize = sampleSize;
        }

        public FileInformation Read(string fileName) {

            var lines = new Lines(fileName, _sampleSize);
            var bestDelimiter = lines.FindDelimiter();

            return new FileInformation(fileName) {
                FileType = bestDelimiter.FileType,
                ColumnNames = lines.PossibleHeaders().ToList()
            };
        }
    }
}

