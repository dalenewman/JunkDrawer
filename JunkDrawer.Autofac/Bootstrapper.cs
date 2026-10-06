using Cfg.Net.Reader;
using JunkDrawer.Autofac.Modern;
using Transformalize.Contracts;
using Transformalize.Logging;
using Transformalize.Providers.SQLite;
using Transformalize.Providers.SqlServer;
using Transformalize.Providers.PostgreSql;
using Transformalize.Providers.MySql;
using Transformalize.Providers.Ado;

namespace JunkDrawer.Autofac;

public sealed class Bootstrapper : IJunkBootstrapper {
    private readonly Request? _request;
    private readonly IPipelineLogger _logger;
    private Cfg? _cfg;

    public Bootstrapper(Request request, IPipelineLogger? logger = null) {
        _request = request;
        _logger = logger ?? new DebugLogger();
    }

    public Bootstrapper(IPipelineLogger? logger = null) {
        _logger = logger ?? new DebugLogger();
    }

    public T Resolve<T>() where T : IResolvable => Resolve<T>(RequireRequest());

    public T Resolve<T>(Request request) where T : IResolvable {
        if (typeof(T) == typeof(Cfg)) {
            return (T)(IResolvable)GetConfiguration(request);
        }
        if (typeof(T) == typeof(IFolder)) {
            return (T)(IResolvable)new AppDataFolder();
        }
        if (typeof(T) == typeof(Importer)) {
            var process = ImportPlanBuilder.Create(request, GetConfiguration(request), _logger);
            var connection = process.GetOutputConnection();
            IConnectionFactory factory = connection.Provider switch {
                "sqlite" => new SqliteConnectionFactory(connection),
                "sqlserver" => new SqlServerConnectionFactory(connection),
                "postgresql" => new PostgreSqlConnectionFactory(connection),
                "mysql" => new MySqlConnectionFactory(connection),
                _ => throw new NotSupportedException($"The {connection.Provider} provider is not supported.")
            };
            return (T)(IResolvable)new Importer(process, new ModernExecutor(_logger), factory);
        }
        throw new NotSupportedException($"Cannot resolve {typeof(T).Name} without a response.");
    }

    public T Resolve<T>(Request request, Response response) where T : IResolvable {
        if (typeof(T) == typeof(Pager) || typeof(T) == typeof(IPager)) {
            var process = PagePlanBuilder.Create(response);
            return (T)(IResolvable)new Pager(process, new ModernReader(_logger));
        }
        return Resolve<T>(request);
    }

    private Request RequireRequest() => _request ?? throw new InvalidOperationException("A request is required.");

    private Cfg GetConfiguration(Request request) {
        if (_cfg != null && ReferenceEquals(request, _request)) {
            return _cfg;
        }
        var cfg = new Cfg(request.Configuration, new FileReader());
        if (cfg.Errors().Any()) {
            throw new InvalidOperationException(string.Join(Environment.NewLine, cfg.Errors()));
        }
        if (ReferenceEquals(request, _request)) {
            _cfg = cfg;
        }
        return cfg;
    }

    public void Dispose() { }
}
