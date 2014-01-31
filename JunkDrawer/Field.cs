using System.Globalization;

namespace JunkDrawer {
    public class Field {
        //todo: move this to configuration
        private string _length = "512";
        private string _type = "string";
        private char _quote = default(char);
        private string _name;

        public string Name {
            get { return _name; }
            set { _name = Utility.CleanIdentifier(value); }
        }

        public char Quote {
            get { return _quote; }
            set { _quote = value; }
        }

        public string Type {
            get { return _type; }
            set { _type = value; }
        }

        public string Length {
            get { return _length; }
            set { _length = value; }
        }

        public Field(string name) {
            Name = name;
        }

        public Field(string name, char quote) {
            Name = name;
            Quote = quote;
        }

        public Field(string name, string type) {
            Name = name;
            Type = type;
        }

        public Field(string name, string type, char quote) {
            Name = name;
            Type = type;
            Quote = quote;
        }

        public bool IsQuoted() {
            return _quote != default(char);
        }

        public string QuoteString() {
            return _quote == default(char) ? string.Empty : Quote.ToString(CultureInfo.InvariantCulture);
        }
    }
}