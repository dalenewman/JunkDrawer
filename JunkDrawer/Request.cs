using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Transformalize.Logging;

namespace JunkDrawer {
    public class Request {
        private const string PROCESS_NAME = "JunkDrawer";

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
                _message = message;
                _isValid = false;
            } else {
                var attempts = 0;
                _fileInfo = new FileInfo(fileName);
                while (attempts < retries - 1 && !_fileInfo.Exists) {
                    Console.Beep();
                    attempts++;
                    TflLogger.Info(PROCESS_NAME, _fileInfo.Name, "File does't exist yet. Waiting {0}...", attempts);
                    Thread.Sleep(1000);
                }

                if (!_fileInfo.Exists) {
                    _message = string.Format("File '{0}' does not exist!", _fileInfo.FullName);
                } else {
                    attempts = 0;
                    while (attempts < retries - 1 && FileInUse(_fileInfo)) {
                        Console.Beep();
                        attempts++;
                        TflLogger.Info(PROCESS_NAME,_fileInfo.Name, "File is in use. Waiting {0}...", attempts);
                        Thread.Sleep(1000);
                    }
                    if (FileInUse(_fileInfo)) {
                        _message = string.Format("Another process is using {0}.", _fileInfo.Name);
                        _isValid = false;
                    } else {
                        _isValid = true;
                    }
                }
            }
        }

        public bool FileInUse(FileInfo fileInfo) {
            try {
                using (Stream stream = new FileStream(fileInfo.FullName, FileMode.Open)) {
                    return false;
                }
            } catch (IOException exception) {
                TflLogger.Debug(PROCESS_NAME,fileInfo.Name, exception.Message);
                return true;
            }
        }

    }
}