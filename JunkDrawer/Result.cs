using System.Collections.Generic;
using Transformalize.Libs.Rhino.Etl;

namespace JunkDrawer
{
    public class Result {
        public FileInformation FileInformation { get; set; }
        public IEnumerable<Field> Fields { get; set; }
        public IEnumerable<Row> Rows { get; set; }
    }
}