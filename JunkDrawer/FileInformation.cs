using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JunkDrawer {
    public class FileInformation {

        private bool _firstRowIsHeader = true;

        //properties
        public string FileName { get; set; }
        public FileType FileType { get; set; }
        public IEnumerable<string> ColumnNames { get; set; }
        public string FileExtension { get { return Path.GetExtension(FileName ?? string.Empty).ToLower(); } }
        public string ProcessName { get { return Utility.CleanIdentifier(Path.GetFileNameWithoutExtension(FileName)); } }
        public string Delimiter { get { return FileTypes.FileTypeMap[FileType]; } }
        public bool FirstRowIsHeader {
            get { return _firstRowIsHeader; }
            set { _firstRowIsHeader = value; }
        }

        //constructors
        public FileInformation(string fileName) {
            ColumnNames = new List<string>();
            FileName = fileName;
            FileType = FileType.Unknown;
        }

        public FileInformation(string fileName, FileType fileType)
            : this(fileName, fileType, new string[0]) {
        }

        public FileInformation(string fileName, FileType fileType, IEnumerable<string> columnNames) {
            FileName = fileName;
            FileType = fileType;
            ColumnNames = columnNames;
        }

        //methods

        public int ColumnCount() {
            return ColumnNames.Count();
        }

        public FieldType[] FieldTypes() {
            return new FieldInspector().Inspect(this);
        }

    }
}