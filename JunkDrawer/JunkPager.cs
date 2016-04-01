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

using System.Dynamic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Contracts;

namespace JunkDrawer {
    public class JunkPager : IPager {
        private readonly Process _process;
        private readonly Entity _entity;
        private readonly Field[] _fields;
        private readonly IRunTimeRun _reader;

        public JunkPager(Process process, IRunTimeRun reader) {
            _process = process;
            _entity = _process.Entities.First();
            _reader = reader;
            _fields = _entity.Fields.Where(f => !f.System).ToArray();
        }

        public PageResult GetPage(int page, int pageSize) {
            var result = new PageResult();
            _entity.Page = page;
            _entity.PageSize = pageSize;
            result.Rows = _reader.Run(_process).ToArray(); // enumerate so i can get hits count back
            result.Fields = _fields;
            result.Hits = _entity.Hits;
            return result;
        }

    }
}