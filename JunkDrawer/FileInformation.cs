using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace JunkDrawer {

    public class FileInformation {

        private bool _firstRowIsHeader = true;
        private FileInfo _fileInfo;
        private FileType _fileType = FileType.Unknown;
        private List<Field> _fields = new List<Field>();

        //properties
        public FileInfo FileInfo {
            get { return _fileInfo; }
            set { _fileInfo = value; }
        }

        public FileType FileType {
            get { return _fileType; }
            set { _fileType = value; }
        }

        public List<Field> Fields {
            get { return _fields; }
            set { _fields = value; }
        }

        public string ProcessName { get { return Utility.CleanIdentifier(Path.GetFileNameWithoutExtension(_fileInfo.Name)); } }
        public string Delimiter { get { return FileTypes.FileTypeMap[FileType]; } }
        public bool FirstRowIsHeader {
            get { return _firstRowIsHeader; }
            set { _firstRowIsHeader = value; }
        }

        //constructors
        public FileInformation(FileInfo fileInfo) {
            _fileInfo = fileInfo;
        }

        public FileInformation(FileInfo fileInfo, FileType fileType) {
            _fileInfo = fileInfo;
            _fileType = fileType;
        }

        public FileInformation(FileInfo fileInfo, FileType fileType, List<Field> fieldTypes) {
            FileInfo = fileInfo;
            FileType = fileType;
            Fields = fieldTypes;
        }

        //methods
        public int ColumnCount() {
            return Fields.Count();
        }

        public List<Field> InspectedFieldTypes() {
            return new FieldInspector().Inspect(this);
        }

        public string Identifier() {
            return "JDI" + ProcessName.GetHashCode().ToString(CultureInfo.InvariantCulture).Replace("-", "0").PadRight(13, '0');
        }

    }
}