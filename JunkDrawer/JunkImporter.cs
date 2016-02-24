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
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Contracts;

namespace JunkDrawer {
    public class JunkImporter : IResolvable {
        private readonly Root _root;
        private readonly IRunTimeExecute _executor;

        public JunkImporter(
            Root root,
           IRunTimeExecute executor
        ) {
            _root = root;
            _executor = executor;
        }

        public JunkResponse Import() {

            try {
                _executor.Execute(_root);
                var entity = _root.Processes.First().Entities.First();

                return new JunkResponse {
                    Records = entity.Inserts,
                    TableName = entity.Alias
                };

            } catch (Exception) {
                return new JunkResponse();
            }

        }

    }
}
