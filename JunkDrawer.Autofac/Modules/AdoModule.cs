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

using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Extensions;
using Pipeline.Provider.Ado;
using Pipeline.Provider.MySql;
using Pipeline.Provider.PostgreSql;
using Pipeline.Provider.SqlServer;
using Pipeline.Provider.SQLite;

namespace JunkDrawer.Autofac.Modules {
    public class AdoModule : ProcessModule {

        public AdoModule(Root root) : base(root) { }

        protected override void RegisterProcess(ContainerBuilder builder, Process process) {
            foreach (var connection in process.Connections.Where(c => c.Provider.In("sqlserver", "mysql", "postgresql", "sqlite"))) {

                // Connection Factory
                builder.Register<IConnectionFactory>(ctx => {
                    switch (connection.Provider) {
                        case "sqlserver":
                            return new SqlServerConnectionFactory(connection);
                        case "mysql":
                            return new MySqlConnectionFactory(connection);
                        case "postgresql":
                            return new PostgreSqlConnectionFactory(connection);
                        case "sqlite":
                            return new SqLiteConnectionFactory(connection);
                        default:
                            return new NullConnectionFactory();
                    }
                }).Named<IConnectionFactory>(connection.Key).InstancePerLifetimeScope();

                // Schema Reader
                builder.Register<ISchemaReader>(ctx => {
                    var factory = ctx.ResolveNamed<IConnectionFactory>(connection.Key);
                    return new AdoSchemaReader(ctx.ResolveNamed<IConnectionContext>(connection.Key), factory);
                }).Named<ISchemaReader>(connection.Key);

            }
        }

    }
}