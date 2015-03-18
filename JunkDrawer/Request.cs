using System;
using System.IO;
using System.Linq;
using System.Threading;
using Transformalize.Logging;

namespace JunkDrawer {

    public class Request {

        private const string PROCESS_NAME = "JunkDrawer";

        public bool IsValid { get; private set; }
        public FileInfo FileInfo { get; private set; }
        public JunkCfg Cfg { get; private set; }
        public string Message { get; private set; }
        public string TableName { get; set; }

        public Request(string fileName, JunkCfg cfg) {

            Cfg = cfg;
            FileInfo = new FileInfo(fileName);
            Message = string.Empty;

            var attempts = 0;
            var retries = Cfg.FileInspection.First().Retries;

            if (!FileInfo.Exists) {
                Console.Beep();
                attempts++;

                if (retries == 0) {
                    TflLogger.Info(PROCESS_NAME, FileInfo.Name, "File does't exist.", attempts);
                    IsValid = false;
                    return;
                }

                TflLogger.Warn(PROCESS_NAME, FileInfo.Name, "Waiting");

                while (attempts < retries && !FileInfo.Exists) {
                    Console.Beep();
                    TflLogger.Info(PROCESS_NAME, FileInfo.Name, ".");
                    attempts++;
                    Thread.Sleep(1000);
                }

                if (!FileInfo.Exists) {
                    Message = string.Format("File '{0}' just doesn't exist!", FileInfo.FullName);
                    IsValid = false;
                    return;
                }
            }

            // If we made it this far, the file exists

            if (FileInUse(FileInfo)) {
                Console.Beep();
                TflLogger.Info(PROCESS_NAME, FileInfo.Name, "File is locked by another process.");
                if (attempts < retries) {
                    while (attempts < retries && FileInUse(FileInfo)) {
                        Console.Beep();
                        attempts++;
                        TflLogger.Info(PROCESS_NAME, FileInfo.Name, "File is in use. Attempt number {0}...", attempts);
                        Thread.Sleep(1000);
                    }
                    if (FileInUse(FileInfo)) {
                        Message = string.Format("Can not open file {0}.", FileInfo.Name);
                        IsValid = false;
                        return;
                    }
                }
            }

            IsValid = true;
        }

        private static bool FileInUse(FileSystemInfo fileInfo) {
            try {
                using (Stream stream = new FileStream(fileInfo.FullName, FileMode.Open)) {
                    return false;
                }
            } catch (IOException exception) {
                TflLogger.Debug(PROCESS_NAME, fileInfo.Name, exception.Message);
                return true;
            }
        }

    }
}