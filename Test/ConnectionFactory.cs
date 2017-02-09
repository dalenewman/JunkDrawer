using System.Data;
using Transformalize.Configuration;
using Transformalize.Provider.Ado;
using Transformalize.Provider.MySql;
using Transformalize.Provider.PostgreSql;
using Transformalize.Provider.SqlCe;
using Transformalize.Provider.SqlServer;
using Transformalize.Provider.SQLite;

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