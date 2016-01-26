#region license
// JunkDrawer
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
using Autofac;
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Provider.Ado;

namespace JunkDrawer.Autofac.Modules {

    public class ConnectionModule : ProcessModule {

        public ConnectionModule(Root root) : base(root) {
        }

        protected override void RegisterProcess(ContainerBuilder builder, Process process) {
            foreach (var c in process.Connections) {

                builder.Register(ctx => new ConnectionContext(new PipelineContext(ctx.Resolve<IPipelineLogger>(), process), c)).Named<ConnectionContext>(c.Key);
                builder.RegisterType<ConnectionFactory>().InstancePerLifetimeScope();
                builder.Register(ctx => ctx.Resolve<ConnectionFactory>().CreateFactory(ctx.ResolveNamed<ConnectionContext>(c.Key))).Named<IConnectionFactory>(c.Key);
                builder.Register(ctx => ctx.Resolve<ConnectionFactory>().CreateSchemaReader(ctx.ResolveNamed<ConnectionContext>(c.Key))).Named<ISchemaReader>(c.Key);
            }
        }
    }
}