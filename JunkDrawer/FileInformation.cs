using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace JunkDrawer {

    public class FileInformation {

        public const string CleanPattern = @"[\s\-]|^[\d]";
        private List<string> _columnNames = new List<string>();
        private bool _firstRowIsHeader = true;

        public string FileName { get; set; }
        public FileType FileType { get; set; }
        public int ColumnCount { get; set; }

        public List<string> ColumnNames {
            get { return _columnNames; }
            set { _columnNames = value; }
        }

        public bool FirstRowIsHeader {
            get { return _firstRowIsHeader; }
            set { _firstRowIsHeader = value; }
        }

        public string FileExtension { get { return Path.GetExtension(FileName ?? string.Empty).ToLower(); } }
        public string ProcessName { get { return CleanIdentifier(Path.GetFileNameWithoutExtension(FileName)); } }
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

        public string CleanIdentifier(string input) {
            return Regex.Replace(input, CleanPattern, string.Empty).Trim(' ');
        }

    }
}