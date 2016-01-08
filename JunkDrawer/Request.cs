using System;
using System.IO;
using System.Threading;

namespace JunkDrawer {

    public class Request {

        public bool IsValid { get; private set; }
        public FileInfo FileInfo { get;}
        public string Message { get; private set; }
        public string TableName { get; set; }
        public string Configuration { get; set; }
        public string Extension { get; set; }

        public Request(string fileName, string configuration, int retries = 3) {

            FileInfo = new FileInfo(fileName);
            Extension = FileInfo.Extension.ToLower();
            Configuration = configuration;
            Message = string.Empty;

            var attempts = 0;

            if (!FileInfo.Exists) {
                Console.Beep();
                attempts++;

                if (retries == 0) {
                    Message = $"File {FileInfo.Name} does not exist!";
                    IsValid = false;
                    return;
                }

                while (attempts < retries && !FileInfo.Exists) {
                    Console.Beep();
                    attempts++;
                    Thread.Sleep(1000);
                }

                if (!FileInfo.Exists) {
                    Message = $"File '{FileInfo.FullName}' just doesn't exist!  I looked for it {retries} times.";
                    IsValid = false;
                    return;
                }
            }

            // If we made it this far, the file exists

            if (FileInUse(FileInfo)) {
                Console.Beep();
                if (attempts < retries) {
                    while (attempts < retries && FileInUse(FileInfo)) {
                        Console.Beep();
                        attempts++;
                        Thread.Sleep(1000);
                    }
                    if (FileInUse(FileInfo)) {
                        Message = $"Can not open file {FileInfo.Name}. I tried opening it {retries} times.";
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
            } catch (IOException) {
                return true;
            }
        }

    }
}