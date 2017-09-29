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
using System.Linq;
using Cfg.Net.Ext;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace JunkDrawer.Autofac {
    public class ReverseConfiguration {
        private readonly IContext _context;
        private readonly Response _response;

        public ReverseConfiguration(IContext context, Response response) {
            _context = context;
            _response = response;
        }

        public Process Create() {

            var process = new Process { Name = "Pager" };
            process.Connections.Add(_response.Connection.Clone());
            process.Connections.First().Name = "input";

            var entity = new Entity { Name = _response.View, Connection = "input" };
            entity.Fields.Add(new Field { Name = Transformalize.Constants.TflKey, Alias = "JdKey", Type = "int", PrimaryKey = true });
            entity.Fields.AddRange(_response.Fields);
            process.Entities.Add(entity);

            process.Check();

            foreach (var error in process.Errors()) {
                _context.Error(error);
            }

            return process;
        }
    }

}