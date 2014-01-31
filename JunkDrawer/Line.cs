using System.Collections.Generic;

namespace JunkDrawer {
    public class Line
    {

        private readonly char _quote = default(char);
        private readonly string _content = string.Empty;
        private readonly Dictionary<char, LineInfo> _delimiterCounts = new Dictionary<char, LineInfo>();
        
        public string Content {
            get { return _content; }
        }

        public Dictionary<char, LineInfo> DelimiterCounts {
            get { return _delimiterCounts; }
        }

        public char Quote { get { return _quote; } }

        public Line(string content) {
            _content = content;
            foreach (var delimiter in FileTypes.Delimiters) {
                var values = content.Split(delimiter);
                _delimiterCounts[delimiter] = new LineInfo(delimiter, values);
            }
        }

        public Line(string content, char quote) {
            _content = content;
            _quote = quote;
            foreach (var delimiter in FileTypes.Delimiters) {
                var values = content.DelimiterSplit(delimiter, quote);
                _delimiterCounts[delimiter] = new LineInfo(delimiter, values);
            }
        }

    }
}