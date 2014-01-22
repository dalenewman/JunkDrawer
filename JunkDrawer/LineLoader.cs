using System.Collections.Generic;
using System.Linq;

namespace JunkDrawer
{
    public class LineLoader {
        private readonly FileLineLoader _loader;

        public LineLoader(FileLineLoader loader) {
            _loader = loader;
        }

        public IEnumerable<Line> Load() {
            return _loader.Load().Select(content => new Line(content));
        }
    }
}