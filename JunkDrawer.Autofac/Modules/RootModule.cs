#region license
// Transformalize
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
using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Reader;
using Pipeline.Configuration;
using Pipeline.Nulls;

// ReSharper disable PossibleMultipleEnumeration

namespace JunkDrawer.Autofac.Modules {
    public class RootModule : Module {

        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<FileReader>().Named<IReader>("file");
            builder.RegisterType<WebReader>().Named<IReader>("web");

            builder.Register<IReader>(ctx => new DefaultReader(
                ctx.ResolveNamed<IReader>("file"),
                new ReTryingReader(ctx.ResolveNamed<IReader>("web"), 3))
            );

            builder.Register((ctx, p) => {
                var process = new Process(new NullValidator("js"), new NullValidator("sh"), ctx.Resolve<IReader>());
                switch (p.Count()) {
                    case 2:
                        process.Load(
                            p.Named<string>("cfg"),
                            p.Named<Dictionary<string, string>>("parameters")
                        );
                        break;
                    case 1:
                        process.Load(p.Named<string>("cfg"));
                        break;
                    default:
                        process.Load(p.Named<string>("cfg"));
                        break;
                }
                return process;
            }).As<Process>();

        }
    }
}
