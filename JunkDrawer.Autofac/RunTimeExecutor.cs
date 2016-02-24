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

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using JunkDrawer.Autofac.Modules;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Logging.NLog;

namespace JunkDrawer.Autofac {

    public class RunTimeExecutor : IRunTimeExecute {
        private readonly IContext _context;

        public RunTimeExecutor(IContext context) {
            _context = context;
        }

        public void Execute(Root root) {

            foreach (var warning in root.Warnings()) {
                _context.Warn(warning);
            }

            if (root.Errors().Any()) {
                foreach (var error in root.Errors()) {
                    _context.Error(error);
                }
                _context.Error("The configuration errors must be fixed before this job will run.");
                return;
            }

            var name = string.Join("-", root.Processes.Select(p => p.Name));

            var builder = new ContainerBuilder();
            builder.RegisterInstance(new NLogPipelineLogger(name, _context.LogLevel)).As<IPipelineLogger>();
            builder.RegisterModule(new RootModule());
            builder.RegisterModule(new ContextModule(root));
            builder.RegisterModule(new AdoModule(root));
            builder.RegisterModule(new EntityControlModule(root));
            builder.RegisterModule(new EntityInputModule(root));
            builder.RegisterModule(new EntityOutputModule(root));
            builder.RegisterModule(new EntityPipelineModule(root));
            builder.RegisterModule(new ProcessControlModule(root));

            using (var scope = builder.Build().BeginLifetimeScope()) {
                foreach (var process in root.Processes) {
                    try {
                        scope.ResolveNamed<IProcessController>(process.Key).Execute();
                    } catch (Exception ex) {
                        _context.Error(ex.Message);
                    }
                }
            }
        }

        public void Execute(string cfg, string shorthand, Dictionary<string, string> parameters) {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new RootModule());
            using (var scope = builder.Build().BeginLifetimeScope()) {
                var root = scope.Resolve<Root>(
                    new NamedParameter("cfg", cfg),
                    new NamedParameter("shorthand", shorthand),
                    new NamedParameter("parameters", parameters)
                );
                Execute(root);
            }
        }

    }
}