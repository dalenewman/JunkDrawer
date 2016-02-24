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
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Reader;
using Cfg.Net.Serializers;
using Pipeline.Configuration;
using Pipeline.Nulls;

namespace JunkDrawer.Autofac {
    public class UnloadedRootModule : Module {

        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<SourceDetector>().As<ISourceDetector>();
            builder.RegisterType<FileReader>().Named<IReader>("file");
            builder.RegisterType<WebReader>().Named<IReader>("web");
            builder.RegisterType<JsonSerializer>().As<ISerializer>();
            //builder.RegisterType<XmlSerializer>().As<ISerializer>();

            builder.Register<IReader>(ctx => new DefaultReader(
                ctx.Resolve<ISourceDetector>(),
                ctx.ResolveNamed<IReader>("file"),
                new ReTryingReader(ctx.ResolveNamed<IReader>("web"), attempts: 3))
            );

            builder.Register<IValidators>(ctx => 
                new Validators(new Dictionary<string, IValidator> {
                    { "js", new NullValidator() }
                }
            ));

            builder.Register((ctx, p) => new Root(
                ctx.Resolve<IValidators>(),
                ctx.Resolve<IReader>(),
                ctx.Resolve<ISerializer>()
            )).As<Root>().InstancePerDependency();

        }
    }
}
