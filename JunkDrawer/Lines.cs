using System.Collections.Generic;
using System.IO;
using System.Linq;
using Transformalize.Libs.NLog;

namespace JunkDrawer {

    public class Lines {

        private readonly FileSystemInfo _fileInfo;
        private readonly List<Line> _storage = new List<Line>();
        private Delimiter _bestDelimiter;
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public Lines(FileSystemInfo fileInfo, int sampleSize) {
            _fileInfo = fileInfo;
            _storage.AddRange(new LineLoader(fileInfo, sampleSize).Load());
        }

        public Delimiter FindDelimiter() {

            if (_bestDelimiter != null)
                return _bestDelimiter;

            var max = 0;
            var candidates = new List<Delimiter>();

            foreach (var delimiter in FileTypes.Delimiters) {
                foreach (var line in _storage) {
                    var count = line.DelimiterCounts[delimiter].Delimiter.Count;
                    if (count > 0 && _storage.All(l => l.DelimiterCounts[delimiter].Delimiter.Count.Equals(count))) {
                        candidates.Add(new Delimiter(delimiter, count));
                        if (count > max) {
                            max = count;
                        }
                    }
                }
            }

            if (!candidates.Any()) {
                _log.Warn("Can't find the delimiter for {0}.  Defaulting to single column.", _fileInfo.Name);
                return new Delimiter(default(char), 0);
            }

            _bestDelimiter = candidates.First(d => d.Count.Equals(max));
            _log.Info("Best Delimiter Found is {0}.", _bestDelimiter.Character);
            return _bestDelimiter;
        }

        public List<Field> InitialFieldTypes() {
            var fieldTypes = new List<Field>();
            var delimiter = FindDelimiter();
            var line = _storage[0];

            if (delimiter.FileType == FileType.Unknown) {
                fieldTypes.Add(new Field(line.Content) { Length = "4000"});
                return fieldTypes;
            }

            var lineInfo = line.DelimiterCounts[delimiter.Character];

            for (var i = 0; i < lineInfo.Values.Count; i++) {
                var name = lineInfo.Values[i];
                if (_storage.Any(l => l.DelimiterCounts[delimiter.Character].Values[i].Contains(delimiter.ToString()))) {
                    fieldTypes.Add(new Field(name, line.Quote));
                } else {
                    fieldTypes.Add(new Field(name));
                }
            }

            return fieldTypes;
        }

    }
}