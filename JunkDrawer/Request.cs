using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Transformalize.Libs.NLog;

namespace JunkDrawer {
    public class Request {

        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly bool _isValid;
        private readonly string _message = string.Empty;
        private readonly FileInfo _fileInfo;

        public bool IsValid { get { return _isValid; } }
        public FileInfo FileInfo { get { return _fileInfo; } }
        public string Message { get { return _message; } }

        public Request(IList<string> args, int retries = 5) {
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
                    var tries = 0;
                    while (tries < retries && FileInUse(_fileInfo)) {
                        Console.Beep();
                        _log.Info("Wait a second...");
                        Thread.Sleep(1000);
                        tries++;
                    }
                    _isValid = true;
                }
            }
        }

        public bool FileInUse(FileInfo fileInfo) {
            try {
                using (Stream stream = new FileStream(fileInfo.FullName, FileMode.Open)) {
                    return false;
                }
            } catch (IOException exception) {
                _log.Warn(exception.Message);
                return true;
            }
        }

    }
}