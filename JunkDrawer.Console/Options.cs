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
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using Pipeline.Contracts;

namespace JunkDrawer {

    public class Options {

        [Option('f', "file", Required = true, HelpText = "The file to import.")]
        public string File { get; set; }

        [Option('a', "arrangement", Required = false, DefaultValue = "default.xml", HelpText = "The configuration file.")]
        public string Configuration { get; set; }

        [OptionList('t',"types", Separator = ',', HelpText = "Override the configuration's inspection types, comma separated (e.g. bool, byte, short, int, long, single, double, datetime).")]
        public IList<string> Types { get; set; }

        [Option('c',"connection", Required = false, HelpText = "Override the configuration's connection type (e.g. sqlserver, mysql, postgresql, sqlite).")]
        public string Provider { get; set; }

        [Option('s',"server", Required = false,HelpText = "Override the configuration's output server.")]
        public string Server { get; set; }

        [Option('n', "port", Required = false, HelpText = "Override the configuration's output port.")]
        public int Port { get; set; }

        [Option('d', "database", Required = false, HelpText = "Override the configuration's output database.")]
        public string Database { get; set; }

        [Option('o', "owner", Required = false, HelpText = "Override the configuration's owner (schema) database.")]
        public string Schema { get; set; }

        [Option('v', "view", Required = false, HelpText = "Override the configuration's output view.")]
        public string Table { get; set; }


        [Option('u', "user", Required = false, HelpText = "Override the configuration's output user.")]
        public string User { get; set; }

        [Option('p', "password", Required = false, HelpText = "Override the configuration's output password.")]
        public string Password { get; set; }

        [Option('l', "loglevel", Required = false, DefaultValue = LogLevel.Info, HelpText = "The log level (i.e. none, info, debug, warn, error).")]
        public LogLevel LogLevel { get; set; }


        [HelpOption]
        public string GetUsage() {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }
}