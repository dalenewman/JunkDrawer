using System.Collections.Generic;

namespace JunkDrawer {
    public class Line {
        private readonly string _content = string.Empty;
        private readonly Dictionary<char, int> _delimiterCounts = new Dictionary<char, int>();

        public string Content {
            get { return _content; }
        }

        public Dictionary<char, int> DelimiterCounts {
            get { return _delimiterCounts; }
        }

        public Line(string content) {
            _content = content;
            foreach (var delimiter in FileTypes.Delimiters) {
                _delimiterCounts[delimiter] = content.Split(delimiter).Length - 1;
            }
        }

    }
}