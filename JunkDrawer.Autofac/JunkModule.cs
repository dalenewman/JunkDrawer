#region license
// JunkDrawer.Autofac
// Copyright 2013 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Ext;
using Cfg.Net.Reader;
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Context;
using Pipeline.Contracts;
using Pipeline.Desktop;
using Pipeline.Ioc.Autofac;
using Pipeline.Logging.NLog;
using Pipeline.Nulls;
using Pipeline.Provider.Excel;
using Pipeline.Provider.File;


namespace JunkDrawer.Autofac {
    public class JunkModule : Module {
        private readonly JunkRequest _jr;

        public JunkModule(JunkRequest jr) {
            _jr = jr;
        }

        public JunkModule() {
            // sometimes you just don't have the junk request yet...
        }

        public static string ProcessName = "JunkDrawer";
        protected override void Load(ContainerBuilder builder) {

            // Cfg-Net Setup for JunkCfg
            builder.RegisterType<FileReader>().As<IReader>();
            builder.Register(ctx => new NLogPipelineLogger(ProcessName, LogLevel.Info)).As<IPipelineLogger>().SingleInstance();
            builder.Register((ctx, p) => _jr ?? p.TypedAs<JunkRequest>()).As<JunkRequest>();

            builder.Register((ctx, p) => {
                var entityName = ctx.Resolve<JunkRequest>(p).FileInfo.Name;
                return new PipelineContext(
                    ctx.Resolve<IPipelineLogger>(),
                    new Process { Name = ProcessName, Key = ProcessName }.WithDefaults(),
                    new Entity { Name = entityName, Alias = entityName, Key = entityName }
                        .WithDefaults()
                    );
            }).As<IContext>().InstancePerLifetimeScope();

            builder.Register((ctx, p) => {
                var request = ctx.Resolve<JunkRequest>(p);
                var cfg = new JunkCfg(request.Configuration, ctx.Resolve<IReader>());

                // modify the input provider based on the file name requested
                var input = cfg.Input();
                input.File = request.FileInfo.FullName;
                if (request.Extension.StartsWith(".xls", StringComparison.OrdinalIgnoreCase)) {
                    input.Provider = "excel";
                }

                // modify the output connection
                var output = cfg.Output();
                if (!string.IsNullOrEmpty(request.Provider) && Constants.ProviderSet().Contains(request.Provider)) {
                    output.Provider = request.Provider;
                }

                SetOption(() => request.Server, option => { output.Server = option; });
                SetOption(() => request.Database, option => { output.Database = option; });
                SetOption(() => request.User, option => { output.User = option; });
                SetOption(() => request.Password, option => { output.Password = option; });
                SetOption(() => request.Port, option => { output.Port = option; });
                SetOption(() => request.View, option => { output.Table = option; });

                // modify the types if provided from command line
                if (request.Types == null || !request.Types.Any(t => Constants.TypeSet().Contains(t)))
                    return cfg;

                var context = ctx.Resolve<IContext>();
                context.Debug(() => "Manually over-riding types from command line");
                cfg.Input().Types.Clear();
                foreach (var type in request.Types.Where(type => Constants.TypeSet().Contains(type))) {
                    cfg.Input().Types.Add(new TflType(type).WithDefaults());
                    context.Debug(() => $"Inspecting for type: {type}.");
                }

                return cfg;
            }).As<JunkCfg>().InstancePerLifetimeScope();

            builder.Register((ctx, p) => {
                var process = new Process(new NullValidator("js"), new NullValidator("sh"));
                process.Load(p.Named<string>("cfg"));
                return process;
            }).As<Process>();

            // Junk Drawer Setup

            builder.Register<ISchemaReader>((ctx, p) => {
                var connection = ctx.Resolve<JunkCfg>(p).Input();
                var fileInfo = new FileInfo(Path.IsPathRooted(connection.File) ? connection.File : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, connection.File));
                var context = new ConnectionContext(ctx.Resolve<IContext>(p), connection);
                var cfg = connection.Provider == "file" ?
                    new FileInspection(context, fileInfo).Create() :
                    new ExcelInspection(context, fileInfo).Create();
                var process = ctx.Resolve<Process>(p.Concat(new[] { new NamedParameter("cfg", cfg) }));
                process.Pipeline = "linq";
                return new SchemaReader(context, new RunTimeRunner(context), process);
            }).As<ISchemaReader>().InstancePerLifetimeScope();

            // Write Configuration based on schema results and JunkRequest
            builder.Register<IRunTimeExecute>((c, p) => new RunTimeExecutor(c.Resolve<IContext>(p))).As<IRunTimeExecute>();
            builder.Register<IRunTimeRun>((c, p) => new RunTimeRunner(c.Resolve<IContext>(p))).As<IRunTimeRun>();

            // when you resolve a process, you need to add a cfg parameter
            builder.Register((c, p) => {
                var parameters = new List<global::Autofac.Core.Parameter>();
                parameters.AddRange(p);

                var cfg = new JunkConfigurationCreator(c.Resolve<JunkCfg>(p), c.Resolve<ISchemaReader>(p)).Create();
                parameters.Add(new NamedParameter("cfg", cfg));
                return c.Resolve<Process>(parameters);
            }).Named<Process>("import").InstancePerLifetimeScope();

            // when you resolve a process, you need to add a cfg parameter
            builder.Register((c, p) => {
                var parameters = new List<global::Autofac.Core.Parameter>();
                parameters.AddRange(p);

                var cfg = new ReverseConfiguration(p.TypedAs<JunkResponse>(), c.ResolveNamed<Process>("import", p)).Create();
                parameters.Add(new NamedParameter("cfg", cfg));
                return c.Resolve<Process>(parameters);
            }).Named<Process>("read");

            // Final products are JunkImporter and JunkPager
            builder.Register((c, p) => {
                var process = c.ResolveNamed<Process>("import", p);
                return new JunkImporter(process, c.Resolve<IRunTimeExecute>(p));
            }).As<JunkImporter>();

            builder.Register((c, p) => {
                var process = c.ResolveNamed<Process>("read", p);
                return new JunkPager(process, new RunTimeRunner(c.Resolve<IContext>(p)));
            }).As<JunkPager>();


        }

        private static void SetOption<T>(Func<T> getter, Action<T> setter) {
            var value = getter();
            if (value != null && (default(T) == null ? string.Empty : default(T).ToString()) != value.ToString()) {
                setter(value);
            }
        }
    }
}
