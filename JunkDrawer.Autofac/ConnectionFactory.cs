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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cfg.Net.Contracts;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Desktop;
using Pipeline.Nulls;
using Pipeline.Provider.Ado;
using Pipeline.Provider.Excel;
using Pipeline.Provider.File;
using Pipeline.Provider.MySql;
using Pipeline.Provider.PostgreSql;
using Pipeline.Provider.SqlServer;
using Pipeline.Provider.SQLite;

namespace JunkDrawer.Autofac {

    public class ConnectionFactory {

        public IConnectionFactory CreateFactory(IConnectionContext input) {
            switch (input.Connection.Provider) {
                case "sqlserver":
                    return new SqlServerConnectionFactory(input.Connection);
                case "mysql":
                    return new MySqlConnectionFactory(input.Connection);
                case "postgresql":
                    return new PostgreSqlConnectionFactory(input.Connection);
                case "sqlite":
                    return new SqLiteConnectionFactory(input.Connection);
                default:
                    return new NullConnectionFactory();
            }
        }

        public ISchemaReader CreateSchemaReader(IConnectionContext input) {
            switch (input.Connection.Provider) {
                case "sqlserver":
                case "mysql":
                case "postgresql":
                case "sqlite":
                    return new AdoSchemaReader(input.Connection, CreateFactory(input));
                case "excel":
                case "file":
                    /* file and excel are different, have to load the content and check it to determine schema */
                    var fileInfo = new FileInfo(Path.IsPathRooted(input.Connection.File) ? input.Connection.File : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, input.Connection.File));
                    var cfg = input.Connection.Provider == "file" ? new FileInspection(input, fileInfo, 100).Create() : new ExcelInspection(input, fileInfo, 100).Create();
                    var validators = new Validators(new Dictionary<string, IValidator> {
                        { "js", new NullValidator() },
                        { "cron", new NullValidator() }
                    });
                    var root = new Root(validators);
                    root.Load(cfg);

                    foreach (var warning in root.Warnings()) {
                        input.Warn(warning);
                    }

                    if (root.Errors().Any()) {
                        foreach (var error in root.Errors()) {
                            input.Error(error);
                        }
                        return new NullSchemaReader();
                    }

                    return new SchemaReader(input, new PipelineRunner(input), root);
                default:
                    return new NullSchemaReader();
            }
        }

    }
}