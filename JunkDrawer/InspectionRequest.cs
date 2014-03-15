using System.Collections.Generic;

namespace JunkDrawer {

    public class InspectionRequest {

        private string _defaultType = "string";
        private string _defaultLength = "1024";
        private decimal _sample = 100m;
        private string[] _dataTypes = {
            "byte", 
            "int16", 
            "int32", 
            "int64", 
            "single", 
            "double", 
            "datetime"
        };
        private Dictionary<char, string> _delimiters = new Dictionary<char, string> {
            { '\t', "tab" },
            { ',', "comma" },
            { '|', "pipe" },
            { ';', "semicolon" }
        };

        public int Top { get; set; }

        public Dictionary<char, string> Delimiters {
            get { return _delimiters; }
            set { _delimiters = value; }
        }

        public decimal Sample {
            get { return _sample; }
            set { _sample = value; }
        }

        public string[] DataTypes {
            get { return _dataTypes; }
            set { _dataTypes = value; }
        }

        public string DefaultType {
            get { return _defaultType; }
            set { _defaultType = value; }
        }

        public string DefaultLength { //string to support "max" designation
            get { return _defaultLength; }
            set { _defaultLength = value; }
        }

    }
}