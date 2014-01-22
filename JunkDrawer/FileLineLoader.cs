using System.Collections.Generic;
using System.IO;

namespace JunkDrawer
{
    public class FileLineLoader {
        private readonly string _fileName;
        private readonly int _lines;

        public FileLineLoader(string fileName, int lines) {
            _fileName = fileName;
            _lines = lines;
        }

        public IEnumerable<string> Load() {
            var lines = new List<string>();
            using (var reader = new StreamReader(_fileName)) {
                for (var i = 0; i < _lines; i++) {
                    if (!reader.EndOfStream) {
                        lines.Add(reader.ReadLine());
                    }
                }
            }
            return lines;
        }
    }
}