using JunkDrawer;
using JunkDrawer.Autofac;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Testcontainers.MySql;
using Testcontainers.PostgreSql;

namespace Test.Database;

[TestClass]
public sealed class DatabaseIntegration {
    [TestMethod]
    public async Task PostgreSqlImportAndPage() {
        var password = NewPassword();
        await using var container = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("junk")
            .WithUsername("postgres")
            .WithPassword(password)
            .Build();
        await container.StartAsync();
        ImportAndPage("postgresql", container.Hostname, container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort), "junk", "postgres", password);
    }

    [TestMethod]
    public async Task MySqlImportAndPage() {
        var password = NewPassword();
        await using var container = new MySqlBuilder("mysql:8.0")
            .WithDatabase("junk")
            .WithUsername("root")
            .WithPassword(password)
            .Build();
        await container.StartAsync();
        ImportAndPage("mysql", container.Hostname, container.GetMappedPublicPort(MySqlBuilder.MySqlPort), "junk", "root", password);
    }

    [TestMethod]
    public async Task SqlServerImportAndPage() {
        var password = NewPassword();
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(password)
            .Build();
        await container.StartAsync();
        var database = "junk" + Guid.NewGuid().ToString("N")[..8];
        await using (var connection = new SqlConnection(container.GetConnectionString())) {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [{database}]";
            await command.ExecuteNonQueryAsync();
        }
        ImportAndPage("sqlserver", container.Hostname, container.GetMappedPublicPort(MsSqlBuilder.MsSqlPort), database, "sa", password,
            "trust-server-certificate=\"true\" encrypt=\"true\"", Transformalize.Constants.DefaultSetting);
    }

    private static void ImportAndPage(string provider, string host, int port, string database, string user, string password, string extras = "", string? viewOverride = null) {
        var directory = Path.Combine(Path.GetTempPath(), "jd-database-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var arrangement = Path.Combine(directory, "arrangement.xml");
            File.WriteAllText(arrangement, $"""
                <jd><connections>
                  <add name="input" provider="file" file="*.*" />
                  <add name="output" provider="{provider}" server="{host}" port="{port}" database="{database}" user="{user}" password="{password}" {extras} />
                </connections></jd>
                """);
            var request = new Request(Path.Combine(AppContext.BaseDirectory, "Files", "CommaSeparatedValues.csv")) {
                Configuration = arrangement,
                View = viewOverride,
                Retries = 0
            };
            using var bootstrapper = new Bootstrapper(request);
            var response = bootstrapper.Resolve<Importer>().Import();
            Assert.AreEqual(4L, response.Records);
            var page = bootstrapper.Resolve<Pager>(request, response).GetPage(1, 2);
            Assert.AreEqual(4L, page.Hits);
            Assert.AreEqual(2, page.Rows.Length);
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string NewPassword() => "Jd!" + Guid.NewGuid().ToString("N")[..16];
}
