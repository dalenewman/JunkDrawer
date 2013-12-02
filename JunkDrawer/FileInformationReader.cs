using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JunkDrawer {
    public class FileInformationReader {

        private readonly int _sampleSize;

        public FileInformationReader(int sampleSize) {
            _sampleSize = sampleSize;
        }

        public FileInformation Read(string fileName) {
            var lines = new List<LineStats>();

            using (var reader = new StreamReader(fileName)) {
                for (var i = 0; i < _sampleSize; i++) {
                    AddLine(lines, reader);
                }
            }

            foreach (var pair in FileTypes.DelimiterMap) {
                var counts = lines.Select(line => line[pair.Key]).ToList();
                var first = counts.First();
                if (first.Item1 > 0 && counts.All(i => i.Item1.Equals(first.Item1))) {
                    return new FileInformation(fileName) {
                        ColumnCount = first.Item1 + 1,
                        FileType = pair.Value,
                        ColumnNames = new List<string>(first.Item2.Split(pair.Key).Select(s => s.Replace(" ", string.Empty).Trim(' ')).ToArray())
                    };
                }
            }

            return new FileInformation(fileName);
        }

        private static void AddLine(ICollection<LineStats> lines, StreamReader reader) {
            if (!reader.EndOfStream) {
                lines.Add(new LineStats(reader.ReadLine()));
            }
        }
    }
}