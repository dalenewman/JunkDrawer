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
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace JunkDrawer {

    public class Pager : IPager {
        private readonly Process _process;
        private readonly Entity _entity;
        private readonly Field[] _fields;
        private readonly IRunTimeRun _reader;

        public Pager(Process process, IRunTimeRun reader) {
            _process = process;
            _entity = _process.Entities.First();
            _reader = reader;
            _fields = _entity.Fields.Where(f => !f.System).ToArray();
        }

        public PageResult GetPage(int page, int pageSize, System.Collections.Generic.IReadOnlyList<Order> order = null) {
            var result = new PageResult();
            _entity.Page = page;
            _entity.Size = pageSize;
            _entity.Order = order?.Select(item => new Order { Field = item.Field, Sort = item.Sort }).ToList()
                ?? new System.Collections.Generic.List<Order>();
            result.Arrangement = _process.Serialize();
            result.Rows = _reader.Run(_process).ToArray(); // enumerate so i can get hits count back
            result.Fields = _fields;
            result.Hits = _entity.Hits;
            result.Query = _entity.Query ?? string.Empty;
            return result;
        }

    }
}
