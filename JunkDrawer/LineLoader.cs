using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JunkDrawer {

    public class LineLoader {
        private readonly FileLineLoader _loader;
        private readonly bool _isCsv;

        public LineLoader(FileSystemInfo fileInfo, int sampleSize) {
            _loader = new FileLineLoader(fileInfo.FullName, sampleSize);
            _isCsv = fileInfo.Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<Line> Load() {
            return _isCsv ?
                _loader.Load().Select(content => new Line(content, '\"')) :
                _loader.Load().Select(content => new Line(content));
        }
    }
}