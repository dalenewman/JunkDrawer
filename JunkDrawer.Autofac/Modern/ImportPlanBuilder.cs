using Autofac;
using Cfg.Net.Ext;
using JunkDrawer.Autofac;
using Transformalize;
using Transformalize.Configuration;
using Transformalize.Containers.Autofac;
using Transformalize.Contracts;
using Transformalize.Providers.CsvHelper.Autofac;
using Transformalize.Providers.Sqlite.Autofac;
using Transformalize.Providers.SqlServer.Autofac;
using Transformalize.Providers.PostgreSql.Autofac;
using Transformalize.Providers.MySql.Autofac;

namespace JunkDrawer.Autofac.Modern;

internal static class ImportPlanBuilder {
    public static Process Create(Request request, Cfg cfg, IPipelineLogger logger) {
        if (request.Extension is not (".csv" or ".txt" or ".xls" or ".xlsx")) {
            throw new NotSupportedException($"The {request.Extension} format has not been ported yet.");
        }

        var input = cfg.Input().Clone();
        var output = cfg.Output().Clone();
        input.File = request.FileInfo.FullName;
        if (request.Extension is ".xls" or ".xlsx") input.Provider = "excel";
        output.Provider = request.Provider ?? output.Provider;
        if (output.Provider is not ("sqlite" or "sqlserver" or "postgresql" or "mysql")) {
            throw new NotSupportedException($"The {output.Provider} provider has not been ported yet.");
        }
        if (!string.IsNullOrWhiteSpace(request.DatabaseFile)) output.File = HomePath.Expand(request.DatabaseFile);
        if (!string.IsNullOrWhiteSpace(request.Database)) output.Database = request.Database;
        if (!string.IsNullOrWhiteSpace(request.Server)) output.Server = request.Server;
        if (request.Port > 0) output.Port = request.Port;
        if (!string.IsNullOrWhiteSpace(request.Schema)) output.Schema = request.Schema;
        if (!string.IsNullOrWhiteSpace(request.User)) output.User = request.User;
        if (!string.IsNullOrWhiteSpace(request.Password)) output.Password = request.Password;
        var view = request.View == Constants.DefaultSetting ? null : request.View;
        if (!string.IsNullOrWhiteSpace(view)) output.Table = view;
        if (request.Types is { Count: > 0 }) {
            input.Types.Clear();
            foreach (var type in request.Types) {
                if (!Constants.TypeSet().Contains(type)) {
                    throw new ArgumentException($"Unsupported inspection type: {type}");
                }
                input.Types.Add(new TflType(type));
            }
        }

        var name = Utility.Identifier(Path.GetFileNameWithoutExtension(request.FileInfo.Name));
        var process = new Process {
            Name = "JunkDrawer",
            Mode = "init",
            Connections = new List<Connection> { input, output },
            Entities = new List<Entity> {
                new() {
                    Name = name,
                    Input = "input",
                    Alias = !string.IsNullOrWhiteSpace(view) ? view : null,
                    PrependProcessNameToOutputName = false
                }
            }
        };
        process.Load();
        ThrowOnErrors(process);

        using (var scope = new Container(ProviderModules.Create()).CreateScope(process, logger)) {
            if (!new SchemaService(scope).Help(process)) {
                throw new InvalidOperationException($"Could not infer columns from {request.FileInfo.Name}.");
            }
        }
        process.Load();
        ThrowOnErrors(process);
        return process;
    }

    internal static void ThrowOnErrors(Process process) {
        if (process.Errors().Any()) {
            throw new InvalidOperationException(string.Join(Environment.NewLine, process.Errors()));
        }
    }
}

internal static class ProviderModules {
    public static global::Autofac.Core.IModule[] Create() => [
        new CsvHelperProviderModule(), new ExcelInputModule(), new SqliteModule(),
        new SqlServerModule(), new PostgreSqlModule(), new MySqlModule()
    ];
}
