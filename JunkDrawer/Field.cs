using System.Globalization;

namespace JunkDrawer {
    public class Field {
        private string _name;

        public string Name {
            get { return _name; }
            set { _name = Utility.CleanIdentifier(value); }
        }

        public char Quote { get; set; }
        public string Type { get; set; }
        public string Length { get; set; }

        public Field(string name)
        {
            Quote = default(char);
            Name = name;
        }

        public bool IsQuoted() {
            return Quote != default(char);
        }

        public string QuoteString() {
            return Quote == default(char) ? string.Empty : Quote.ToString(CultureInfo.InvariantCulture);
        }

    }
}