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
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Ext;
using Cfg.Net.Reader;
using JunkDrawer.Autofac.Modules;
using Pipeline;
using Pipeline.Actions;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Logging.NLog;
using Pipeline.Nulls;

namespace JunkDrawer.Autofac {
    public class JunkModule : Module {
        private readonly JunkRequest _junkRequest;

        public JunkModule(JunkRequest junkRequest) {
            _junkRequest = junkRequest;
        }

        public static string ProcessName = "JunkDrawer";
        protected override void Load(ContainerBuilder builder) {

            // Cfg-Net Setup for JunkCfg
            builder.RegisterType<SourceDetector>();
            builder.RegisterType<FileReader>();
            builder.RegisterType<WebReader>();

            builder.Register<IReader>(ctx =>
                new DefaultReader(
                    ctx.Resolve<SourceDetector>(),
                    ctx.Resolve<FileReader>(),
                    new ReTryingReader(ctx.Resolve<WebReader>(), 3)
                )
            );

            builder.Register(ctx => {
                var cfg = new JunkCfg(
                    _junkRequest.Configuration,
                    ctx.Resolve<IReader>()
                    );
                // modify the input provider based on the file name requested
                var input = cfg.Connections.First();
                input.File = _junkRequest.FileInfo.FullName;
                if (_junkRequest.Extension.StartsWith(".xls")) {
                    input.Provider = "excel";
                }
                return cfg;
            }).As<JunkCfg>().InstancePerLifetimeScope();

            // Cfg-Net setup for Transformalize
            builder.Register<IValidators>(ctx => new Validators(new Dictionary<string, IValidator> {
                { "js", new NullValidator() },
                {"cron", new NullValidator() }
            }));

            builder.Register((ctx, p) => {
                var root = new Root(ctx.Resolve<IValidators>());
                root.Load(p.Named<string>("cfg"));
                return root;
            }).As<Root>().InstancePerLifetimeScope();

            // Junk Drawer Setup
            builder.Register(ctx => new NLogPipelineLogger(ProcessName, LogLevel.Info)).As<IPipelineLogger>().InstancePerLifetimeScope();
            builder.Register(ctx => new PipelineContext(ctx.Resolve<IPipelineLogger>(), new Process { Name = ProcessName }.WithDefaults())).As<IContext>();

            // ConnectionFactory, and Context to Create Schema Reader
            builder.RegisterType<ConnectionFactory>();
            builder.Register(ctx => new ConnectionContext(ctx.Resolve<IContext>(), ctx.Resolve<JunkCfg>().Input())).As<ConnectionContext>();
            builder.Register(ctx => ctx.Resolve<ConnectionFactory>().CreateSchemaReader(ctx.Resolve<ConnectionContext>())).As<ISchemaReader>();

            // Write Configuration based on schema results and JunkRequest
            builder.Register<ICreateConfiguration>(c => new JunkConfigurationCreator(c.Resolve<JunkCfg>(), _junkRequest, c.Resolve<ISchemaReader>())).As<ICreateConfiguration>();
            builder.Register(c => c.Resolve<ICreateConfiguration>().Create()).Named<string>("cfg").InstancePerLifetimeScope();

            builder.Register<IAction>(c =>
            {
                var root = c.Resolve<Root>(new NamedParameter("cfg", c.ResolveNamed<string>("cfg")));
                var context = c.Resolve<IContext>();
                foreach (var warning in root.Warnings()) {
                    context.Warn(warning);
                }
                if (root.Errors().Any()) {
                    foreach (var error in root.Errors()) {
                        context.Error(error);
                    }
                    return new NullAction();
                }

                var nested = new ContainerBuilder();

                nested.RegisterInstance(c.Resolve<IPipelineLogger>());
                nested.RegisterModule(new ConnectionModule(root));
                nested.RegisterModule(new EntityControlModule(root));
                nested.RegisterModule(new EntityInputModule(root));
                nested.RegisterModule(new EntityOutputModule(root));
                nested.RegisterModule(new EntityPipelineModule(root));
                nested.RegisterModule(new ProcessControlModule(root));

                return new PipelineAction(nested, root);
            }).As<IAction>();

            // Final product is a JunkImporter that executes the action above
            builder.Register(c =>
            {
                var root = c.Resolve<Root>(new NamedParameter("cfg", c.ResolveNamed<string>("cfg")));
                return new JunkImporter(root, c.Resolve<IAction>());
            }).As<JunkImporter>();
        }
    }
}
