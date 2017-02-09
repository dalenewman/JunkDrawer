#region license
// JunkDrawer.Autofac
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
using System.IO;

namespace JunkDrawer.Autofac {
    public class AppDataFolder : IFolder {

        public const string Folder = "JunkDrawer";
        public const string Ext = ".sql";
        private readonly string _path;

        public AppDataFolder() {
            var appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            _path = Path.Combine(appDataPath, Folder);

            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        public string Read(int key) {
            return File.ReadAllText(FileName(key));
        }

        public void Write(int key, string contents) {
            File.WriteAllText(FileName(key), contents);
        }

        public string FileName(int key) {
            return Path.Combine(_path, "Jd" + key + Ext);
        }
    }
}