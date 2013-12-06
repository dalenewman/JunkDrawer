using System.Collections.Generic;
using System.IO;
using Transformalize.Libs.NLog;

namespace JunkDrawer {
    public class Request {

        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly bool _isValid = false;
        private readonly string _message = string.Empty;
        private readonly FileInfo _fileInfo = null;
        private readonly bool _firstRowIsHeader = false;

        public bool IsValid { get { return _isValid; } }
        public FileInfo FileInfo { get { return _fileInfo; } }
        public string Message { get { return _message; } }
        public bool FirstRowIsHeader { get { return _firstRowIsHeader; } }

        public Request(IList<string> args) {
            var fileName = args.Count > 0 ? args[0] : null;

            if (string.IsNullOrEmpty(fileName)) {
                const string message = @"Please provide a file name (i.e. jd c:\junk\header\temp.txt).";
                _log.Error(message);
                _message = message;
                _isValid = false;
            } else {
                _fileInfo = new FileInfo(fileName);
                if (!_fileInfo.Exists) {
                    var message = string.Format("File '{0}' does not exist!", _fileInfo.FullName);
                    _log.Error(message);
                } else {
                    //var folders = (FileInfo.DirectoryName ?? string.Empty).ToLower().Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                    _firstRowIsHeader = false; // folders.Contains(TryGetSetting("HeaderFolder", "Headers"));
                    _isValid = true;
                }
            }
        }

        //private static string TryGetSetting(string key, string alternate) {
        //    if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.Equals(key, StringComparison.OrdinalIgnoreCase))) {
        //        return ConfigurationManager.AppSettings[key].ToLower();
        //    }
        //    return alternate.ToLower();
        //}

    }
}