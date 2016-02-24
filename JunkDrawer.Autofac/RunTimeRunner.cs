#region license
// Transformalize
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
using Autofac;
using JunkDrawer.Autofac.Modules;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Logging.NLog;
using Pipeline.Nulls;

namespace JunkDrawer.Autofac {
    public class RunTimeRunner : IRunTimeRun {
        private readonly IContext _context;

        public RunTimeRunner(IContext context) {
            _context = context;
        }

        public IEnumerable<IRow> Run(Root root) {

            foreach (var warning in root.Warnings()) {
                _context.Warn(warning);
            }

            if (root.Errors().Any()) {
                foreach (var error in root.Errors()) {
                    _context.Error(error);
                }
                _context.Error("The configuration errors must be fixed before this job will run.");
                return Enumerable.Empty<IRow>();
            }

            var name = string.Join("-", root.Processes.Select(p => p.Name));
            var container = new ContainerBuilder();
            container.RegisterInstance(new NLogPipelineLogger(name, _context.LogLevel)).As<IPipelineLogger>().SingleInstance();
            container.RegisterModule(new RootModule());
            container.RegisterModule(new ContextModule(root));
            container.RegisterModule(new AdoModule(root));
            container.RegisterModule(new EntityControlModule(root));

            var entity = root.Processes.First().Entities.First();
            container.RegisterType<NullUpdater>().Named<IUpdate>(entity.Key);
            container.RegisterType<NullDeleteHandler>().Named<IEntityDeleteHandler>(entity.Key);

            container.RegisterModule(new EntityInputModule(root));
            container.RegisterModule(new EntityOutputModule(root));
            container.RegisterModule(new EntityPipelineModule(root));
            container.RegisterModule(new ProcessControlModule(root));

            using (var scope = container.Build().BeginLifetimeScope()) {
                return scope.ResolveNamed<IProcessController>(root.Processes[0].Key).Run();
            }
        }
    }
}