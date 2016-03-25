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
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Ioc.Autofac.Modules;

namespace JunkDrawer.Autofac {

    public class RunTimeExecutor : IRunTimeExecute {
        private readonly IContext _context;

        public RunTimeExecutor(IContext context) {
            _context = context;
        }

        public void Execute(Process process) {

            foreach (var warning in process.Warnings()) {
                _context.Warn(warning);
            }

            if (process.Errors().Any()) {
                foreach (var error in process.Errors()) {
                    _context.Error(error);
                }
                _context.Error("The configuration errors must be fixed before this job will run.");
                return;
            }

            var builder = new ContainerBuilder();
            builder.RegisterInstance(_context.Logger).As<IPipelineLogger>();
            builder.RegisterModule(new ContextModule(process));
            builder.RegisterModule(new AdoModule(process));
            builder.RegisterModule(new ExcelModule(process));
            builder.RegisterModule(new FileModule(process));
            builder.RegisterModule(new InternalModule(process));

            builder.RegisterModule(new EntityPipelineModule(process));
            builder.RegisterModule(new ProcessControlModule(process));

            using (var scope = builder.Build().BeginLifetimeScope()) {
                try {
                    scope.ResolveNamed<IProcessController>(process.Key).Execute();
                } catch (Exception ex) {
                    _context.Error(ex.Message);
                }
            }
        }

        public void Execute(string cfg, string shorthand, Dictionary<string, string> parameters) {
            throw new NotImplementedException();
        }
    }
}