using System;
using System.Collections.Generic;

namespace JunkDrawer {
    public class LineStats {

        private readonly Dictionary<char, int> _counts = new Dictionary<char, int>();
        private readonly Dictionary<char, string> _lines = new Dictionary<char, string>(); 

        public LineStats(string line) {
            if (string.IsNullOrEmpty(line)) return;

            foreach (var delimiter in FileTypes.Delimiters) {
                _lines[delimiter] = line;
                _counts[delimiter] = line.Split(delimiter).Length - 1;
            }
        }

        public Tuple<int, string> this[char key] {
            get { return new Tuple<int, string>(_counts[key], _lines[key]); }
        }

    }
}