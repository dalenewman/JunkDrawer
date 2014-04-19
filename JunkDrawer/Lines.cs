using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Transformalize.Libs.NLog;

namespace JunkDrawer {

    public class Lines {

        private readonly FileSystemInfo _fileInfo;
        private readonly InspectionRequest _request;
        private readonly List<Line> _storage = new List<Line>();
        private char _bestDelimiter;
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public Lines(FileSystemInfo fileInfo, InspectionRequest request) {
            _fileInfo = fileInfo;
            _request = request;
            _storage.AddRange(new LineLoader(fileInfo, request).Load());
        }

        public char FindDelimiter() {

            if (_bestDelimiter != default(char))
                return _bestDelimiter;

            var max = 0;
            var candidates = new Dictionary<char, int>();

            foreach (var delimiter in _request.Delimiters.Keys) {
                foreach (var line in _storage) {
                    var count = line.Values[delimiter].Length - 1;
                    if (count > 0 && _storage.All(l => l.Values[delimiter].Length-1 == count)) {
                        candidates[delimiter] = count;
                        if (count > max) {
                            max = count;
                        }
                    }
                }
            }

            if (!candidates.Any()) {
                _log.Warn("Can't find a delimiter for {0}.  Defaulting to single column.", _fileInfo.Name);
                return default(char);
            }

            _bestDelimiter = candidates.First(kv => kv.Value.Equals(max)).Key;
            _log.Info("Delimiter is '{0}'", _bestDelimiter);
            return _bestDelimiter;
        }

        public List<Field> InitialFieldTypes() {

            var fieldTypes = new List<Field>();
            var delimiter = FindDelimiter();
            var firstLine = _storage[0];

            if (delimiter == default(char)) {
                fieldTypes.Add(new Field(firstLine.Content, _request.DefaultType, _request.DefaultLength));
                return fieldTypes;
            }

            var values = firstLine.Values[delimiter];

            for (var i = 0; i < values.Length; i++) {
                var name = values[i];
                var field = new Field(name, _request.DefaultType, _request.DefaultLength);
                if (_storage.Any(l => l.Values[delimiter][i].Contains(delimiter.ToString(CultureInfo.InvariantCulture)))) {
                    field.Quote = firstLine.Quote;
                }
                fieldTypes.Add(field);
            }

            return fieldTypes;
        }

    }
}