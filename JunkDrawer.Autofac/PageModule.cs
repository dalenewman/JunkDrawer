#region license
// JunkDrawer
// An easier way to import excel or delimited files into a database.
// Copyright 2013-2017 Dale Newman
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

using Autofac;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace JunkDrawer.Autofac {
    public class PageModule : Module {

        protected override void Load(ContainerBuilder builder) {

            // when you resolve a process, you need to add a cfg parameter
            builder.Register((c, p) => {
                var context = c.Resolve<IContext>(p);
                var response = p.TypedAs<Response>();
                return new ReverseConfiguration(context, response).Create();
            }).Named<Process>("page").InstancePerLifetimeScope();

            builder.Register<IRunTimeRun>((c, p) => {
                var context = c.Resolve<IContext>(p);
                var process = c.ResolveNamed<Process>("page", p);
                var container = DefaultContainer.Create(process, c.Resolve<IPipelineLogger>());
                return new RunTimeRunner(
                    context,
                    container
                );
            }
            ).As<IRunTimeRun>();

            builder.Register((c, p) => {
                var process = c.ResolveNamed<Process>("page", p);
                var runner = c.Resolve<IRunTimeRun>();
                return new Pager(process, runner);
            }).As<Pager>();
        }

    }
}
