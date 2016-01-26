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
using Autofac;
using Pipeline.Configuration;

namespace JunkDrawer.Autofac.Modules {

    /// <summary>
    /// Inherit from EntityModule and override LoadEntity when want to perform something on every entity.
    /// </summary>
    public abstract class EntityModule : Module {
        readonly Root _root;

        protected EntityModule(Root root) {
            _root = root;
        }

        protected override void Load(ContainerBuilder builder) {
            foreach (var process in _root.Processes) {
                foreach (var e in process.Entities) {
                    var entity = e;
                    LoadEntity(builder, process, entity);
                }
            }
        }

        public abstract void LoadEntity(ContainerBuilder builder, Process process, Entity entity);
    }
}