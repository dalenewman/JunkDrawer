#region license
// JunkDrawer.Eto.Core.Desktop
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
using Eto;
using Eto.Forms;
using JunkDrawer.Autofac;

namespace JunkDrawer.Eto.Core.Desktop {
    public class Program {
        private const int Error = 1;

        [STAThread]
        public static void Main(string[] args) {

            var options = new Options();
            var modifed = new List<string>();
            if (args != null) {
                if (args.Length == 1 && !args[0].StartsWith("-f")) {
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
            builder.RegisterType<AutofacJunkBootstrapperFactory>().As<IJunkBootstrapperFactory>();
            using (var scope = builder.Build().BeginLifetimeScope()) {
                var app = new Application(Platform.Detect);
                app.Run(new MainForm(scope.Resolve<IJunkBootstrapperFactory>()));
            }

        }
    }
}
