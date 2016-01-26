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
using Autofac;
using JunkDrawer.Autofac.Modules;
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Contracts;

namespace JunkDrawer.Autofac {
    public class PipelineRunner : IRun {
        private readonly IContext _context;

        public PipelineRunner(IContext context) {
            _context = context;
        }

        public IEnumerable<Row> Run(Root root) {

            var nested = new ContainerBuilder();

            nested.RegisterInstance(_context.Logger).As<IPipelineLogger>();
            nested.RegisterModule(new ConnectionModule(root));
            nested.RegisterModule(new EntityControlModule(root));
            nested.RegisterModule(new EntityInputModule(root));
            nested.RegisterModule(new EntityOutputModule(root));
            nested.RegisterModule(new EntityPipelineModule(root));
            nested.RegisterModule(new ProcessControlModule(root));

            using (var scope = nested.Build().BeginLifetimeScope()) {
                // resolve, run, and release
                var controller = scope.ResolveNamed<IProcessController>(root.Processes[0].Key); ;
                return controller.Run();
            }
        }
    }
}