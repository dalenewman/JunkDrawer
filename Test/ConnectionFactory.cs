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
using System.Data;
using Transformalize.Configuration;
using Transformalize.Providers.Ado;
using Transformalize.Providers.MySql;
using Transformalize.Providers.PostgreSql;
using Transformalize.Providers.SqlCe;
using Transformalize.Providers.SqlServer;
using Transformalize.Providers.SQLite;

namespace Test {
    public static class ConnectionFactory {
        public static IDbConnection Create(Connection cn) {
            IDbConnection output;
            switch (cn.Provider) {
                case "sqlserver":
                    output = new SqlServerConnectionFactory(cn).GetConnection();
                    break;
                case "mysql":
                    output = new MySqlConnectionFactory(cn).GetConnection();
                    break;
                case "postgresql":
                    output = new PostgreSqlConnectionFactory(cn).GetConnection();
                    break;
                case "sqlite":
                    output = new SqLiteConnectionFactory(cn).GetConnection();
                    break;
                case "sqlce":
                    output = new SqlCeConnectionFactory(cn).GetConnection();
                    break;
                default:
                    throw new DataException($"Provider {cn.Provider} is not supported!");
            }
            output.Open();
            return output;
        }
    }
}