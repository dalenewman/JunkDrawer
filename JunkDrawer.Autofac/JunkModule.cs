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
using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Ext;
using Cfg.Net.Reader;
using Transformalize;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Logging.NLog;

namespace JunkDrawer.Autofac {

    public class JunkModule : Module {

        private readonly Request _jr;
        private readonly IPipelineLogger _logger;

        public JunkModule(Request jr, IPipelineLogger logger = null) {
            _jr = jr;
            _logger = logger;
        }

        public JunkModule(IPipelineLogger logger = null) {
            _logger = logger;
            // sometimes you just don't have the junk request yet...
        }

        public static string ProcessName = "Junk Drawer";
        protected override void Load(ContainerBuilder builder) {

            // Cfg-Net Setup for Cfg
            builder.RegisterType<FileReader>().As<IReader>();
            builder.Register(ctx => _logger ?? new NLogPipelineLogger(ProcessName)).As<IPipelineLogger>().SingleInstance();
            builder.Register((ctx, p) => _jr ?? p.TypedAs<Request>()).As<Request>();

            builder.Register((ctx, p) => {
                var entityName = ctx.Resolve<Request>(p).FileInfo.Name;
                return new PipelineContext(
                    ctx.Resolve<IPipelineLogger>(),
                    new Process { Name = ProcessName }.WithDefaults(),
                    new Entity { Name = entityName, Alias = entityName, Key = entityName }
                        .WithDefaults()
                    );
            }).As<IContext>().InstancePerLifetimeScope();

            builder.Register((ctx, p) => {
                var request = ctx.Resolve<Request>(p);
                var cfg = new Cfg(request.Configuration, ctx.Resolve<IReader>());

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
                SetOption(() => request.DatabaseFile, option => { output.File = option; });
                SetOption(() => request.View, option => { output.Table = option; });
                SetOption(() => request.Schema, option => { output.Schema = option; });
                SetOption(() => request.User, option => { output.User = option; });
                SetOption(() => request.Password, option => { output.Password = option; });
                SetOption(() => request.Port, option => { output.Port = option; });

                // modify the types if provided from command line
                if (request.Types == null || !request.Types.Any(t => Constants.TypeSet().Contains(t)))
                    return cfg;

                var context = ctx.Resolve<IContext>();
                context.Debug(() => "Manually over-riding types.");
                cfg.Input().Types.Clear();
                foreach (var type in request.Types.Where(type => Constants.TypeSet().Contains(type))) {
                    cfg.Input().Types.Add(new TflType(type).WithDefaults());
                    context.Debug(() => $"Inspecting for type: {type}.");
                }

                return cfg;
            }).As<Cfg>().InstancePerLifetimeScope();

            builder.Register((ctx, p) => {
                var process = new Process();
                if(p.Any())
                    process.Load(p.Named<string>("cfg"));
                return process;
            }).As<Process>();
        }

        private static void SetOption<T>(Func<T> getter, Action<T> setter) {
            var value = getter();
            if (value != null && (default(T) == null ? string.Empty : default(T).ToString()) != value.ToString()) {
                setter(value);
            }
        }
    }
}
