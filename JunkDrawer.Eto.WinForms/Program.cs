#region license
// JunkDrawer.Eto.WinForms
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
using System.Collections.Generic;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Ext;
using Cfg.Net.Reader;
using Eto;
using Eto.Forms;
using JunkDrawer.Autofac;
using JunkDrawer.Eto.Core;
using Pipeline.Configuration;
using Pipeline.Context;
using Pipeline.Contracts;
using Pipeline.Logging.NLog;
using Environment = System.Environment;

namespace JunkDrawer.Eto.WinForms {

    public class Program {

        private const string ProcessName = "JunkDrawer";
        private const int Error = 1;

        [STAThread]
        public static void Main(string[] args) {

            var options = new Options();
            var modifed = new List<string>();
            if (args != null) {
                if (args.Length == 1 && !args[0].StartsWith("-")) {
                    modifed.Add("-f");
                    modifed.Add(args[0]);
                } else {
                    modifed.AddRange(args);
                }
            }

            if (!CommandLine.Parser.Default.ParseArguments(modifed.ToArray(), options)) {
                Environment.ExitCode = Error;
                return;
            }

            var builder = new ContainerBuilder();
            builder.RegisterType<FileReader>().As<IReader>();
            builder.Register((c, p) => new Cfg(options.Configuration, c.Resolve<IReader>())).As<Cfg>();
            builder.Register<IPipelineLogger>(c => {
                if (options.LogLevel == LogLevel.None)
                    return new NLogPipelineLogger(ProcessName, options.LogLevel);

                return new CompositeLogger(new TextAreaLogger(options.LogLevel), new NLogPipelineLogger(ProcessName, options.LogLevel));
            }).As<IPipelineLogger>().SingleInstance();

            builder.Register<IContext>(c => new PipelineContext(c.Resolve<IPipelineLogger>(), new Process { Name = ProcessName, Key = ProcessName }.WithDefaults()));
            builder.Register(c => new AutofacJunkBootstrapperFactory(c.Resolve<IPipelineLogger>())).As<IJunkBootstrapperFactory>();
            builder.RegisterType<AppDataFolder>().As<IFolder>();

            using (var scope = builder.Build().BeginLifetimeScope()) {
                var app = new Application(Platform.Detect);
                app.Run(new MainForm(
                    scope.Resolve<IJunkBootstrapperFactory>(),
                    scope.Resolve<Cfg>(),
                    scope.Resolve<IContext>(),
                    scope.Resolve<IFolder>(),
                    options.LogLevel,
                    options.File,
                    options.Configuration
                    )
                );
            }

        }
    }
}
