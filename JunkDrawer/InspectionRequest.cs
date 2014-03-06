namespace JunkDrawer {

    public class InspectionRequest {
        private string[] _dataTypes = { "byte", "int16", "int32", "int64", "single", "double", "datetime" };
        private string _defaultType = "string";
        private string _defaultLength = "512";
        private decimal _sample = 100m;

        public int Top { get; set; }

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