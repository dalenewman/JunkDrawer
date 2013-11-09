using System.IO;

namespace JunkDrawer {
    public class FileInformation {

        public string FileName { get; set; }
        public FileType FileType { get; set; }
        public int ColumnCount { get; set; }
        public string[] ColumnNames { get; set; }
        public string FileExtenstion { get { return Path.GetExtension(FileName ?? string.Empty).ToLower(); } }
        public string ProcessName { get { return Path.GetFileNameWithoutExtension(FileName); } }
        public string Delimiter { get { return FileTypes.FileTypeMap[FileType]; } }

        public FileInformation(string fileName) {
            FileName = fileName;
            FileType = FileType.Unknown;
            ColumnNames = new string[0];
        }

        public FileInformation(string fileName, FileType fileType, int columnCount)
            : this(fileName, fileType, columnCount, new string[0]) {
        }

        public FileInformation(string fileName, FileType fileType, int columnCount, string[] columnNames) {
            FileName = fileName;
            FileType = fileType;
            ColumnCount = columnCount;
            ColumnNames = columnNames;
        }

    }
}