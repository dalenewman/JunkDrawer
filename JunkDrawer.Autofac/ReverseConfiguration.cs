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
using System.Linq;
using Cfg.Net.Ext;
using Pipeline.Configuration;
using Pipeline.Contracts;

namespace JunkDrawer.Autofac {
    public class ReverseConfiguration : ICreateConfiguration {
        private readonly JunkResponse _response;

        public ReverseConfiguration(JunkResponse response) {
            _response = response;
        }

        public string Create() {

            var process = new Process().WithDefaults();
            process.Name = "Pager";
            process.Connections.Clear();
            process.Entities.Clear();

            process.Connections.Add(_response.Connection.Clone());
            process.Connections.First().Name = "input";

            var entity = new Entity { Name = _response.View, Connection = "input" }.WithDefaults();
            entity.Fields.Add(new Field { Name = Pipeline.Constants.TflKey, Alias = "Key", Type = "int", PrimaryKey = true }.WithDefaults());
            entity.Fields.AddRange(_response.Fields);
            process.Entities.Add(entity);

            return process.Serialize();
        }
    }

}