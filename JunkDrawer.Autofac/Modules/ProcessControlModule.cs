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
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Provider.Ado;
using Process = Pipeline.Configuration.Process;

namespace JunkDrawer.Autofac.Modules {
    public class ProcessControlModule : ProcessModule {

        public ProcessControlModule(Root root) : base(root) { }

        protected override void RegisterProcess(ContainerBuilder builder, Process process) {

            if (!process.Enabled)
                return;

            builder.Register<IProcessController>(ctx => {

                var pipelines = new List<IPipeline>();

                // entity-level pipelines
                foreach (var entity in process.Entities) {
                    var pipeline = ctx.ResolveNamed<IPipeline>(entity.Key);

                    pipelines.Add(pipeline);
                    if (entity.Delete && process.Mode != "init") {
                        pipeline.Register(ctx.ResolveNamed<IEntityDeleteHandler>(entity.Key));
                    }
                }

                // process-level pipeline
                // pipelines.Add(ctx.ResolveNamed<IPipeline>(process.Key));

                var outputProvider = process.Connections.First(c => c.Name == "output").Provider;
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process);

                var controller = new ProcessController(pipelines, context);

                // output initialization
                if (process.Mode == "init") {
                    var output = new OutputContext(context, new Incrementer(context));
                    switch (outputProvider) {
                        case "mysql":
                        case "postgresql":
                        case "sqlite":
                        case "sqlserver":
                            controller.PreActions.Add(new AdoInitializer(output, ctx.ResolveNamed<IConnectionFactory>(output.Connection.Key)));
                            break;
                    }

                }

                return controller;
            }).Named<IProcessController>(process.Key);
        }


    }
}