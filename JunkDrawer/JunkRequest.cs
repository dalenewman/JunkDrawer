#region license
// JunkDrawer
// Copyright 2013 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.IO;
using System.Threading;

namespace JunkDrawer {

    public class JunkRequest {

        public bool IsValid { get; private set; }
        public FileInfo FileInfo { get;}
        public string Message { get; private set; }
        public string TableName { get; set; }
        public string Configuration { get; set; }
        public string Extension { get; set; }

        public JunkRequest(string fileName, string configuration, int retries = 3) {

            FileInfo = new FileInfo(fileName);
            Extension = FileInfo.Extension.ToLower();
            Configuration = configuration;
            Message = string.Empty;

            var attempts = 0;

            if (!FileInfo.Exists) {
                attempts++;

                if (retries == 0) {
                    Message = $"File {FileInfo.Name} does not exist!";
                    IsValid = false;
                    return;
                }

                while (attempts < retries && !FileInfo.Exists) {
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