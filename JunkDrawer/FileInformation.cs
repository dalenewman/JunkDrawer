using System.Collections.Generic;
using System.IO;

namespace JunkDrawer {
    public class FileInformation {
        private List<string> _columnNames = new List<string>();

        public string FileName { get; set; }
        public FileType FileType { get; set; }
        public int ColumnCount { get; set; }

        public List<string> ColumnNames {
            get { return _columnNames; }
            set { _columnNames = value; }
        }

        public string FileExtension { get { return Path.GetExtension(FileName ?? string.Empty).ToLower(); } }
        public string ProcessName { get { return Path.GetFileNameWithoutExtension(FileName); } }
        public string Delimiter { get { return FileTypes.FileTypeMap[FileType]; } }

        public FileInformation(string fileName) {
            FileName = fileName;
            FileType = FileType.Unknown;
        }

        public FileInformation(string fileName, FileType fileType, int columnCount)
            : this(fileName, fileType, columnCount, new List<string>()) {
        }

        public FileInformation(string fileName, FileType fileType, int columnCount, IEnumerable<string> columnNames) {
            FileName = fileName;
            FileType = fileType;
            ColumnCount = columnCount;
            ColumnNames.AddRange(columnNames);
        }

    }
}