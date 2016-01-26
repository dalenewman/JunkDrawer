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
using System;
using System.Linq;
using Autofac;
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Desktop;
using Pipeline.Nulls;
using Pipeline.Transforms.System;

namespace JunkDrawer.Autofac.Modules {

    public class EntityPipelineModule : EntityModule {

        public EntityPipelineModule(Root root) : base(root) { }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {
            var type = process.Pipeline == "defer" ? entity.Pipeline : process.Pipeline;

            builder.Register(ctx => {
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                IPipeline pipeline;
                context.Debug(() => $"Registering {type} for entity {entity.Alias}.");
                switch (type) {
                    case "parallel.linq":
                        pipeline = new ParallelPipeline(new DefaultPipeline(ctx.ResolveNamed<IOutputController>(entity.Key), context));
                        break;
                    default:
                        pipeline = new DefaultPipeline(ctx.ResolveNamed<IOutputController>(entity.Key), context);
                        break;
                }

                var provider = process.Connections.First(c => c.Name == "output").Provider;

                // extract
                pipeline.Register(ctx.ResolveNamed<IRead>(entity.Key));

                // transform
                pipeline.Register(new SetSystemFields(context));
                pipeline.Register(new DefaultTransform(context, context.GetAllEntityFields()));
                pipeline.Register(TransformFactory.GetTransforms(ctx, process, entity, entity.GetAllFields().Where(f => f.Transforms.Any())));
                pipeline.Register(new StringTruncateTransfom(context));

                if (provider == "sqlserver") {
                    pipeline.Register(new MinDateTransform(context, new DateTime(1753, 1, 1)));
                }

                //load
                pipeline.Register(ctx.ResolveNamed<IWrite>(entity.Key));
                pipeline.Register(new NullUpdater()); //no update necessary
                return pipeline;

            }).Named<IPipeline>(entity.Key);
        }
    }
}