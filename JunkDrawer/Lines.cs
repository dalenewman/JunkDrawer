using System.Collections.Generic;
using System.Linq;

namespace JunkDrawer {

    public class Lines {
        private readonly List<Line> _storage = new List<Line>();
        private Delimiter _bestDelimiter;

        public Lines(string fileName, int sampleSize) {
            _storage.AddRange(new LineLoader(new FileLineLoader(fileName, sampleSize)).Load());
        }

        public Delimiter FindDelimiter() {
            if (_bestDelimiter != null)
                return _bestDelimiter;

            var max = 0;
            var candidates = new List<Delimiter>();

            foreach (var delimiter in FileTypes.Delimiters) {
                foreach (var line in _storage) {
                    var count = line.DelimiterCounts[delimiter];
                    if (count > 0 && _storage.All(l => l.DelimiterCounts[delimiter].Equals(count))) {
                        candidates.Add(new Delimiter(delimiter, count));
                        if (count > max) {
                            max = count;
                        }
                    }
                }
            }

            _bestDelimiter = candidates.First(d => d.Count.Equals(max));
            return _bestDelimiter;
        }

        public IEnumerable<string> PossibleHeaders() {
            return _storage[0].Content.Split(FindDelimiter().Character).Select(Utility.CleanIdentifier);
        }

    }
}