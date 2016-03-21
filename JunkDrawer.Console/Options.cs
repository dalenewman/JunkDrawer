#region license
// DataProfiler
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
using CommandLine;
using CommandLine.Text;

namespace JunkDrawer {

    public class Options {

        [Option('f', "file", Required = true, HelpText = "The file to import.")]
        public string File { get; set; }

        [Option('c', "config", Required = false, DefaultValue = "default.xml", HelpText = "The configuration file.")]
        public string Configuration { get; set; }

        [OptionList('t', "types", Separator = ',', HelpText = "Specify types to check for, separated by commas. Note: this is usually set by the configuration file, but you can override it here.")]
        public IList<string> Types { get; set; }

        [HelpOption]
        public string GetUsage() {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }
}