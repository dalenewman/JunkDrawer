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

using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Ext;
using Cfg.Net.Reader;
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Desktop;
using Pipeline.Desktop.Actions;
using Pipeline.Logging.NLog;

namespace JunkDrawer.Autofac {
    public class JunkModule : Module {
        private readonly JunkRequest _junkRequest;

        public JunkModule(JunkRequest junkRequest) {
            _junkRequest = junkRequest;
        }

        public static string ProcessName = "JunkDrawer";
        protected override void Load(ContainerBuilder builder) {

            // Cfg-Net Setup
            builder.RegisterType<SourceDetector>().As<ISourceDetector>();
            builder.RegisterType<FileReader>();
            builder.RegisterType<WebReader>();

            builder.Register<IReader>(ctx => new DefaultReader(
                ctx.Resolve<ISourceDetector>(),
                ctx.Resolve<FileReader>(),
                new ReTryingReader(ctx.Resolve<WebReader>(),3)
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
            }).As<JunkCfg>();

            // Pipeline.Net Setup
            builder.Register(ctx => new NLogPipelineLogger(ProcessName, LogLevel.Info)).As<IPipelineLogger>().InstancePerLifetimeScope();
            builder.Register(ctx => new PipelineContext(ctx.Resolve<IPipelineLogger>(), new Process { Name = ProcessName }.WithDefaults())).As<IContext>();

            // ConnectionFactory, and Context to Create Schema Reader
            builder.RegisterType<ConnectionFactory>();
            builder.Register(ctx => new ConnectionContext(ctx.Resolve<IContext>(), ctx.Resolve<JunkCfg>().Input())).As<ConnectionContext>();
            builder.Register(ctx => ctx.Resolve<ConnectionFactory>().CreateSchemaReader(ctx.Resolve<ConnectionContext>())).As<ISchemaReader>();

            // Write Configuration based on schema results and JunkRequest
            builder.Register<ICreateConfiguration>(c => new JunkConfigurationCreator(c.Resolve<JunkCfg>(), _junkRequest, c.Resolve<ISchemaReader>())).As<ICreateConfiguration>();

            // Create action with configuration writer and action node
            builder.Register(c => new ActionNode {
                Action = "tfl",
                Shorthand = string.Empty,
                Content = c.Resolve<ICreateConfiguration>().Create()
            }.WithDefaults()).As<ActionNode>();
            builder.Register(c => new PipelineAction(c.Resolve<ActionNode>(), c.Resolve<IContext>())).As<PipelineAction>();

            // Final product is a JunkImporter that executes action (above)
            builder.Register(c => new JunkImporter(c.Resolve<PipelineAction>())).As<JunkImporter>();
        }
    }
}
