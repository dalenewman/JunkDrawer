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
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Cfg.Net.Contracts;
using Pipeline.Configuration;

namespace JunkDrawer {

    public class JunkCfg : CfgNode, IResolvable {

        public JunkCfg(string cfg, params IDependency[] dependencies) : base(dependencies) {
            Load(cfg);
        }

        [Cfg(value = 3)]
        public int Retries { get; set; }

        [Cfg(required = true)]
        public List<Connection> Connections { get; set; }

        protected override void Validate() {
            if (Connections.Count < 2) {
                Error("You need two connections defined; the first one for input, the last one for output.");
            }
        }

        public Connection Input() {
            return Connections.First();
        }

        public Connection Output() {
            return Connections.Last();
        }
    }

}