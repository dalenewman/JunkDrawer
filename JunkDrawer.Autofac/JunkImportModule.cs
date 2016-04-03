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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Context;
using Pipeline.Contracts;
using Pipeline.Desktop;
using Pipeline.Provider.Excel;
using Pipeline.Provider.File;


namespace JunkDrawer.Autofac {
    public class JunkImportModule : Module {

        protected override void Load(ContainerBuilder builder) {

            builder.Register<ISchemaReader>((ctx, p) => {
                var connection = ctx.Resolve<JunkCfg>(p).Input();
                var fileInfo = new FileInfo(Path.IsPathRooted(connection.File) ? connection.File : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, connection.File));
                var context = new ConnectionContext(ctx.Resolve<IContext>(p), connection);
                var cfg = connection.Provider == "file" ?
                    new FileInspection(context, fileInfo).Create() :
                    new ExcelInspection(context, fileInfo).Create();
                var process = ctx.Resolve<Process>(p.Concat(new[] { new NamedParameter("cfg", cfg) }));
                process.Pipeline = "linq";
                return new SchemaReader(context, new RunTimeRunner(context), process);
            }).As<ISchemaReader>().InstancePerLifetimeScope();

            // Write Configuration based on schema results and JunkRequest
            builder.Register<IRunTimeExecute>((c, p) => new RunTimeExecutor(c.Resolve<IContext>(p))).As<IRunTimeExecute>();

            // when you resolve a process, you need to add a cfg parameter
            builder.Register((c, p) => {
                var parameters = new List<global::Autofac.Core.Parameter>();
                parameters.AddRange(p);
                var cfg = new JunkConfigurationCreator(c.Resolve<JunkCfg>(p), c.Resolve<ISchemaReader>(p)).Create();
                parameters.Add(new NamedParameter("cfg", cfg));
                return c.Resolve<Process>(parameters);
            }).Named<Process>("import").InstancePerLifetimeScope();

            // Final products are JunkImporter and JunkPager
            builder.Register((c, p) => {
                var process = c.ResolveNamed<Process>("import", p);
                return new JunkImporter(process, c.Resolve<IRunTimeExecute>(p));
            }).As<JunkImporter>();

        }

    }
}
