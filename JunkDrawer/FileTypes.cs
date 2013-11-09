using System.Collections.Generic;
using System.Linq;

namespace JunkDrawer {
    public static class FileTypes {

        public static readonly Dictionary<char, FileType> DelimiterMap = new Dictionary<char, FileType> {
            {'|', FileType.PipeDelimited },
            {'\t', FileType.TabDelimited},
            {',', FileType.CommaDelimited},
            {';', FileType.SemicolonDelimited}
        };

        public static readonly Dictionary<FileType, string> FileTypeMap = new Dictionary<FileType, string> {
            {FileType.PipeDelimited,"|" },
            {FileType.TabDelimited, "\t"},
            {FileType.CommaDelimited, ","},
            {FileType.SemicolonDelimited, ";" },
            {FileType.Excel, string.Empty}
        };

        public static readonly char[] Delimiters = DelimiterMap.Select(p => p.Key).ToArray();
    }
}