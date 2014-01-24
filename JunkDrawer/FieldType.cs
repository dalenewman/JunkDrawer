namespace JunkDrawer
{
    public class FieldType {
        private string _length = "512";

        public string Name { get; set; }
        public string Type { get; set; }

        public string Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public FieldType(string name, string type) {
            Name = name;
            Type = type;
        }
    }
}