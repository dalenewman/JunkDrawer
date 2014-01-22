namespace JunkDrawer {

    public class Delimiter {
        public char Character { get; private set; }
        public int Count { get; private set; }

        public Delimiter(char character, int count) {
            Character = character;
            Count = count;
        }

        public FileType FileType { get { return FileTypes.DelimiterMap[Character]; } }
    }
}