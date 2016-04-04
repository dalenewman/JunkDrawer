#region license
// JunkDrawer
// Copyright 2013 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Pipeline;

namespace JunkDrawer {

    public class JunkRequest {
        public FileInfo FileInfo { get; }
        public string Message { get; private set; } = string.Empty;
        public string View { get; set; }
        public string Configuration { get; set; } = "default.xml";
        public string Extension { get; private set; }
        public int Retries { get; set; } = 3;
        public int Sleep { get; set; } = 1000;
        public IList<string> Types { get; set; } = new List<string>();
        public string Provider { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string FileName { get; set; }
        public string DatabaseFile { get; set; }

        public JunkRequest(
            string fileName
        ) {
            FileName = fileName;
            try {
                FileInfo = new FileInfo(FileName);
                Extension = FileInfo.Extension.ToLower();
            } catch (Exception ex) {
                Message = ex.Message;
            }

            // for backwards compatibility 
            // originally, -c was for configuration. 
            // now -a is for configuration (aka arrangement) because -c is for connection type
            // connection type couldn't be provider because -p is used for password
            if (Configuration == "default.xml" && !string.IsNullOrEmpty(Provider) && !Constants.ProviderDomain.Contains(Provider)) {
                try {
                    var fileInfo = new FileInfo(Provider);
                    Configuration = fileInfo.FullName;
                } catch (Exception) {
                    // ignored
                }
            }


        }

        public bool IsValid() {

            if (FileInfo == null) {
                return false;
            }


            var attempts = 0;

            if (!FileInfo.Exists) {
                attempts++;

                if (Retries == 0) {
                    Message = $"File {FileInfo.Name} does not exist!";
                    return false;
                }

                while (attempts < Retries && !FileInfo.Exists) {
                    attempts++;
                    Thread.Sleep(Sleep);
                }

                if (!FileInfo.Exists) {
                    Message = $"File '{FileInfo.FullName}' doesn't exist! Tried {Retries} times.";
                    return false;
                }
            }

            // If we made it this far, the file exists
            if (FileInUse(FileInfo)) {
                if (attempts < Retries) {
                    while (attempts < Retries && FileInUse(FileInfo)) {
                        Console.Beep();
                        attempts++;
                        Thread.Sleep(Sleep);
                    }
                    if (FileInUse(FileInfo)) {
                        Message = $"Can not open file {FileInfo.Name}. Tried {Retries} times.";
                        return false;
                    }
                }
            }

            return true;

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

        public int GetCacheKey(JunkCfg cfg) {
            unchecked {
                var hash = 17;
                var input = cfg.Input();
                var output = cfg.Output();

                hash = hash * 23 + input.MinLength.GetHashCode();
                hash = hash * 23 + input.MaxLength.GetHashCode();

                hash = hash * 23 + Provider?.GetHashCode() ?? output.Provider.GetHashCode();
                hash = hash * 23 + Server?.GetHashCode() ?? output.Server.GetHashCode();
                hash = hash * 23 + Database?.GetHashCode() ?? output.Database.GetHashCode();
                hash = hash * 23 + Schema?.GetHashCode() ?? output.Schema.GetHashCode();
                hash = hash * 23 + View?.GetHashCode() ?? output.Table.GetHashCode();
                hash = hash * 23 + Math.Max(Port, output.Port).GetHashCode();
                hash = hash * 23 + DatabaseFile?.GetHashCode() ?? output.File.GetHashCode();

                hash = hash * 23 + FileInfo.FullName.GetHashCode();
                hash = hash + 23 + FileInfo.LastWriteTimeUtc.GetHashCode();

                return hash;
            }
        }

    }
}