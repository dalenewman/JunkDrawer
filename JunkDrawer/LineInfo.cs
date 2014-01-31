using System.Collections.Generic;
using System.Linq;

namespace JunkDrawer
{
    public class LineInfo {
        public Delimiter Delimiter { get; set; }
        public List<string> Values { get; set; }

        public LineInfo(char delimiter, IEnumerable<string> values) {
            Values = values.ToList();
            Delimiter = new Delimiter(delimiter, Values.Count - 1);
        }
    }
}