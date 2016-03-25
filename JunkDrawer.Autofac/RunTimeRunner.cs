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
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Ioc.Autofac.Modules;
using Pipeline.Nulls;

namespace JunkDrawer.Autofac {
    public class RunTimeRunner : IRunTimeRun {
        private readonly IContext _context;

        public RunTimeRunner(IContext context) {
            _context = context;
        }

        public IEnumerable<IRow> Run(Process process) {

            foreach (var warning in process.Warnings()) {
                _context.Warn(warning);
            }

            if (process.Errors().Any()) {
                foreach (var error in process.Errors()) {
                    _context.Error(error);
                }
                _context.Error("The configuration errors must be fixed before this job will run.");
                return Enumerable.Empty<IRow>();
            }

            var container = new ContainerBuilder();
            container.RegisterInstance(_context.Logger).As<IPipelineLogger>().SingleInstance();
            container.RegisterModule(new ContextModule(process));
            container.RegisterModule(new AdoModule(process));
            container.RegisterModule(new InternalModule(process));
            container.RegisterModule(new FileModule(process));
            container.RegisterModule(new ExcelModule(process));

            var entity = process.Entities.First();
            container.RegisterType<NullUpdater>().Named<IUpdate>(entity.Key);
            container.RegisterType<NullDeleteHandler>().Named<IEntityDeleteHandler>(entity.Key);

            container.RegisterModule(new EntityPipelineModule(process));
            container.RegisterModule(new ProcessControlModule(process));

            using (var scope = container.Build().BeginLifetimeScope()) {
                return scope.ResolveNamed<IProcessController>(process.Key).Run();
            }
        }
    }
}