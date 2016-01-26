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
using Pipeline.Nulls;
using Pipeline.Provider.Ado;

namespace JunkDrawer.Autofac.Modules {

    /// <summary>
    /// Currently responsible for registering:
    /// 
    /// * Entity Input Version Detector
    /// * Entity Output Version Detector
    /// * Entity Initializer
    /// </summary>
    public class EntityControlModule : EntityModule {

        public EntityControlModule(Root root) : base(root) { }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {

            builder.Register<IContext>(ctx => new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity)).Named<IContext>(entity.Key).InstancePerLifetimeScope();
            builder.Register<IIncrement>(ctx => new Incrementer(ctx.ResolveNamed<IContext>(entity.Key))).Named<IIncrement>(entity.Key).InstancePerDependency();
            builder.Register(ctx => new InputContext(ctx.ResolveNamed<IContext>(entity.Key), ctx.ResolveNamed<IIncrement>(entity.Key))).Named<InputContext>(entity.Key).InstancePerLifetimeScope();
            builder.Register(ctx => new OutputContext(ctx.ResolveNamed<IContext>(entity.Key), ctx.ResolveNamed<IIncrement>(entity.Key))).Named<OutputContext>(entity.Key).InstancePerLifetimeScope();

            builder.Register<IOutputController>(ctx => {

                var input = ctx.ResolveNamed<InputContext>(entity.Key);
                var output = ctx.ResolveNamed<OutputContext>(entity.Key);
                IAction initializer;
                var cf = ctx.ResolveNamed<IConnectionFactory>(output.Connection.Key);

                IVersionDetector inputDetector;
                switch (input.Connection.Provider) {
                    case "mysql":
                    case "postgresql":
                    case "sqlite":
                    case "sqlserver":
                        inputDetector = new AdoInputVersionDetector(input, ctx.ResolveNamed<IConnectionFactory>(input.Connection.Key));
                        break;
                    default:
                        inputDetector = new NullVersionDetector();
                        break;
                }

                switch (output.Connection.Provider) {
                    case "mysql":
                    case "postgresql":
                    case "sqlite":
                    case "sqlserver":
                        initializer = process.Mode == "init" ? (IAction)new AdoEntityInitializer(output, cf) : new NullInitializer();
                        return new AdoOutputController(
                            output,
                            initializer,
                            inputDetector,
                            new AdoOutputVersionDetector(output, cf),
                            cf
                        );
                    default:
                        return new NullOutputController();
                }

            }).Named<IOutputController>(entity.Key);
        }
    }
}