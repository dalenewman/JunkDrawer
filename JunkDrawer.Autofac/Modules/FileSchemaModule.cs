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

using System;
using System.IO;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Desktop;
using Pipeline.Extensions;
using Pipeline.Nulls;
using Pipeline.Provider.Excel;
using Pipeline.Provider.File;

namespace JunkDrawer.Autofac.Modules {
    public class FileSchemaModule : ProcessModule {
        public FileSchemaModule(Root root) : base(root) { }

        protected override void RegisterProcess(ContainerBuilder builder, Process process) {
            foreach (var connection in process.Connections.Where(c => c.Provider.In("file", "excel"))) {
                builder.Register<ISchemaReader>(ctx => {
                    /* file and excel are different, have to load the content and check it to determine schema */
                    var fileInfo = new FileInfo(Path.IsPathRooted(connection.File) ? connection.File : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, connection.File));
                    var context = ctx.ResolveNamed<IConnectionContext>(connection.Key);
                    var cfg = connection.Provider == "file" ?
                        new FileInspection(context, fileInfo, 100).Create() :
                        new ExcelInspection(context, fileInfo, 100).Create();
                    var root = ctx.Resolve<Root>();
                    root.Load(cfg);

                    foreach (var warning in root.Warnings()) {
                        context.Warn(warning);
                    }

                    if (root.Errors().Any()) {
                        foreach (var error in root.Errors()) {
                            context.Error(error);
                        }
                        return new NullSchemaReader();
                    }

                    return new SchemaReader(context, new RunTimeRunner(context), root);

                }).Named<ISchemaReader>(connection.Key);
            }
        }
    }
}