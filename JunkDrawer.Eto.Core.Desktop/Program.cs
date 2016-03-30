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
            builder.RegisterType<AutofacJunkBootstrapper>().As<IJunkBootstrapper>();

            using (var scope = builder.Build().BeginLifetimeScope()) {
                var app = new Application(Platform.Detect);
                app.Run(new MainForm(scope.Resolve<IJunkBootstrapper>()));
            }

        }
    }
}
