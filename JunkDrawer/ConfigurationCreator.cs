#region license
// JunkDrawer
// An easier way to import excel or delimited files into a database.
// Copyright 2013-2017 Dale Newman
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
using System.Collections.Generic;
using System.Linq;
using Cfg.Net.Ext;
using Transformalize;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace JunkDrawer {
    public class ConfigurationCreator : ICreateConfiguration {

        private readonly Cfg _cfg;
        private readonly ISchemaReader _schemaReader;

        public ConfigurationCreator(Cfg cfg, ISchemaReader schemaReader) {
            _cfg = cfg;
            _schemaReader = schemaReader;
        }

        public string Create() {
            var schema = _schemaReader.Read();
            // create a process based on the schema
            var process = new Process { Name = "JunkDrawer" };
            process.Connections.Add(schema.Connection.Clone());
            process.Connections.Add(_cfg.Output());
            process.Entities = new List<Entity> { schema.Entities.First() };

            var entity = process.Entities.First();
            entity.PrependProcessNameToOutputName = false;

            if (!string.IsNullOrEmpty(_cfg.Output().Table) && _cfg.Output().Table != Constants.DefaultSetting) {
                entity.Alias = _cfg.Output().Table;
            }
            process.Mode = "init";

            // sqlce does not support views
            if (_cfg.Output().Provider == "sqlce") {
                process.Flatten = true;
                process.Flat = entity.Alias;
            }

            return process.Serialize();
        }
    }
}