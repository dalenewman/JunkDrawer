#region license
// JunkDrawer.Autofac
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
using Autofac;
using Pipeline.Configuration;
using Pipeline.Contracts;

namespace JunkDrawer.Autofac {
    public class PageModule : Module {

       protected override void Load(ContainerBuilder builder) {

            builder.Register<IRunTimeRun>((c, p) => new RunTimeRunner(c.Resolve<IContext>(p))).As<IRunTimeRun>();

            // when you resolve a process, you need to add a cfg parameter
            builder.Register((c, p) => {
                var parameters = new List<global::Autofac.Core.Parameter>();
                parameters.AddRange(p);
                var cfg = new ReverseConfiguration(p.TypedAs<Response>()).Create();
                parameters.Add(new NamedParameter("cfg", cfg));
                return c.Resolve<Process>(parameters);
            }).Named<Process>("page");

            builder.Register((c, p) => {
                var process = c.ResolveNamed<Process>("page",p);
                return new Pager(process, c.Resolve<IRunTimeRun>());
            }).As<Pager>();
        }

    }
}
