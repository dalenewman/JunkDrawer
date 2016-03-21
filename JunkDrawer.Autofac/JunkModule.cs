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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Ext;
using Cfg.Net.Reader;
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Desktop;
using Pipeline.Logging.NLog;
using Pipeline.Nulls;
using Pipeline.Provider.Excel;
using Pipeline.Provider.File;


namespace JunkDrawer.Autofac {
    public class JunkModule : Module {
        private readonly JunkRequest _junkRequest;

        public JunkModule(JunkRequest junkRequest) {
            _junkRequest = junkRequest;
        }

        public static string ProcessName = "JunkDrawer";
        protected override void Load(ContainerBuilder builder) {

            // Cfg-Net Setup for JunkCfg
            builder.RegisterType<FileReader>();
            builder.RegisterType<WebReader>();

            builder.Register<IReader>(ctx =>
                new DefaultReader(
                    ctx.Resolve<FileReader>(),
                    new ReTryingReader(ctx.Resolve<WebReader>(), 3)
                )
            );

            builder.Register(ctx => new NLogPipelineLogger(ProcessName, LogLevel.Info)).As<IPipelineLogger>().SingleInstance();

            var entityName = Pipeline.Utility.Identifier(_junkRequest.FileInfo.Name);
            builder.Register(ctx => new PipelineContext(
                ctx.Resolve<IPipelineLogger>(),
                new Process { Name = ProcessName, Key = ProcessName }.WithDefaults(),
                new Entity { Name = entityName, Alias = entityName, Key = entityName }.WithDefaults()
            )).As<IContext>();

            builder.Register(ctx => {
                var cfg = new JunkCfg(
                    _junkRequest.Configuration,
                    ctx.Resolve<IReader>()
                );
                // modify the input provider based on the file name requested
                var input = cfg.Connections.First();
                input.File = _junkRequest.FileInfo.FullName;
                if (_junkRequest.Extension.StartsWith(".xls", StringComparison.OrdinalIgnoreCase)) {
                    input.Provider = "excel";
                }

                // modify the types if provided from command line
                if (_junkRequest.Types == null || !_junkRequest.Types.Any(t => Constants.TypeSet().Contains(t)))
                    return cfg;

                var context = ctx.Resolve<IContext>();
                context.Warn("Manually over-riding types from command line");
                cfg.Input().Types.Clear();
                foreach (var type in _junkRequest.Types.Where(type => Constants.TypeSet().Contains(type))) {
                    cfg.Input().Types.Add(new TflType(type).WithDefaults());
                    context.Warn($"Check for {type}.");
                }

                return cfg;
            }).As<JunkCfg>().InstancePerLifetimeScope();

            builder.Register((ctx, p) => {
                var process = new Process(new NullValidator("js"), new NullValidator("sh"));
                process.Load(p.Named<string>("cfg"));
                return process;
            }).As<Process>();

            // Junk Drawer Setup

            builder.Register<ISchemaReader>(ctx => {
                var connection = ctx.Resolve<JunkCfg>().Input();
                var fileInfo = new FileInfo(Path.IsPathRooted(connection.File) ? connection.File : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, connection.File));
                var context = new ConnectionContext(ctx.Resolve<IContext>(), connection);
                var cfg = connection.Provider == "file" ?
                    new FileInspection(context, fileInfo, 100).Create() :
                    new ExcelInspection(context, fileInfo, 100).Create();
                var process = ctx.Resolve<Process>(new NamedParameter("cfg", cfg));
                process.Pipeline = "linq";
                return new SchemaReader(context, new RunTimeRunner(context), process);
            }).As<ISchemaReader>();

            // Write Configuration based on schema results and JunkRequest
            builder.Register<ICreateConfiguration>(c => new JunkConfigurationCreator(c.Resolve<JunkCfg>(), _junkRequest, c.Resolve<ISchemaReader>())).As<ICreateConfiguration>();
            builder.Register(c => c.Resolve<ICreateConfiguration>().Create()).Named<string>("cfg");
            builder.Register<IRunTimeExecute>(c => new RunTimeExecutor(c.Resolve<IContext>())).As<IRunTimeExecute>();

            // Final product is a JunkImporter that executes the action above
            builder.Register(c => {
                var process = c.Resolve<Process>(new NamedParameter("cfg", c.ResolveNamed<string>("cfg")));
                return new JunkImporter(process, c.Resolve<IRunTimeExecute>());
            }).As<JunkImporter>();
        }
    }
}
