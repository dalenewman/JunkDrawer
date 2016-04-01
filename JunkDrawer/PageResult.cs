using System.Collections.Generic;
using Pipeline.Configuration;
using Pipeline.Contracts;

namespace JunkDrawer {
    public class PageResult {
        public IRow[] Rows { get; set; }
        public Field[] Fields { get; set; }
        public int Hits { get; set; }
    }
}