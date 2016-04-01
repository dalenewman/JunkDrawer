#region license
// JunkDrawer.Console
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
using System.Linq;
using JunkDrawer.Autofac;
using Environment = System.Environment;

namespace JunkDrawer {

    public class Program {

        private const int Error = 1;

        static void Main(string[] args) {

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

            var request = new JunkRequest(options.File) {
                Configuration = options.Configuration ?? "default.xml",
                Types = options.Types,
                View = options.Table,
                Provider = options.Provider,
                Server = options.Server,
                Database = options.Database,
                Schema = options.Schema,
                User = options.User,
                Password = options.Password,
                Port = options.Port
            };

            if (!request.IsValid()) {
                Console.Error.WriteLine(request.Message);
                Environment.Exit(Error);
            }

            try {

                using (var bootstrapper = new AutofacJunkBootstrapper(request)) {

                    var cfg = bootstrapper.Resolve<JunkCfg>();

                    if (cfg.Errors().Any()) {
                        foreach (var error in cfg.Errors()) {
                            Console.Error.WriteLine(error);
                            Environment.ExitCode = Error;
                        }
                    } else {
                        var response = bootstrapper.Resolve<JunkImporter>().Import();
                        if (response.Records != 0)
                            return;
                        Console.Error.WriteLine("Did not import any records!");
                        Environment.ExitCode = Error;
                    }
                }
            } catch (Exception ex) {
                Console.Error.WriteLine(ex.Message);
                Environment.ExitCode = Error;
            }

            Environment.ExitCode = 0;
        }

    }
}
