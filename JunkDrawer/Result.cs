using System.Collections.Generic;
using Transformalize.Libs.Rhino.Etl;
using Transformalize.Main.Providers.File;

namespace JunkDrawer
{
    public class Result {
        public FileInformation FileInformation { get; set; }
        public IEnumerable<FileField> Fields { get; set; }
        public IEnumerable<Row> Rows { get; set; }
    }
}