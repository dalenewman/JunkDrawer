using System.Collections.Generic;

namespace JunkDrawer {

    public class InspectionRequest {

        private Dictionary<char, string> _delimiters = new Dictionary<char, string> {
            { '\t', "tab" },
            { ',', "comma" },
            { '|', "pipe" },
            { ';', "semicolon" }
        };

        public int Top { get; set; }
        public string DefaultType { get; set; }
        public string DefaultLength { get; set; }
        public decimal Sample { get; set; }
        public List<string> DataTypes { get; set; }

        public Dictionary<char, string> Delimiters {
            get { return _delimiters; }
            set { _delimiters = value; }
        }


    }
}