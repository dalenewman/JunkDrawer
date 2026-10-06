using Transformalize.Contracts;

namespace JunkDrawer;

public sealed class Options {
    public string? File { get; private set; }
    public string? Configuration { get; private set; }
    public IList<string> Types { get; private set; } = new List<string>();
    public string? Provider { get; private set; }
    public string? Server { get; private set; }
    public int Port { get; private set; }
    public string? Database { get; private set; }
    public string? Schema { get; private set; }
    public string? View { get; private set; }
    public string? User { get; private set; }
    public string? Password { get; private set; }
    public LogLevel LogLevel { get; private set; } = LogLevel.Info;

    public const string Usage = """
        Usage: jd <file> [options]
               jd -f <file> [options]

          -f, --file          File to import
          -a, --arrangement   XML arrangement (overrides JUNKDRAWER_CONFIG and default lookup)
          -t, --types         Inspection types, comma separated
          -c, --connection    Output provider
          -s, --server        Output server
          -n, --port          Output port
          -d, --database      Output database
          -o, --owner         Output schema
          -v, --view          Output view
          -u, --user          Output user
          -p, --password      Output password
          -l, --loglevel      none, error, warn, info, or debug
              --help          Show this help
        """;

    public static bool TryParse(string[] args, out Options options, out string error) {
        options = new Options();
        error = string.Empty;
        for (var index = 0; index < args.Length; index++) {
            var option = args[index];
            if (!option.StartsWith("-", StringComparison.Ordinal)) {
                if (options.File is null) {
                    options.File = option;
                    continue;
                }
                error = $"Unexpected argument: {option}";
                return false;
            }
            if (++index >= args.Length) {
                error = $"Missing value for {option}.";
                return false;
            }
            var value = args[index];
            switch (option) {
                case "-f": case "--file": options.File = value; break;
                case "-a": case "--arrangement": options.Configuration = value; break;
                case "-t": case "--types": options.Types = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries); break;
                case "-c": case "--connection": options.Provider = value; break;
                case "-s": case "--server": options.Server = value; break;
                case "-d": case "--database": options.Database = value; break;
                case "-o": case "--owner": options.Schema = value; break;
                case "-v": case "--view": options.View = value; break;
                case "-u": case "--user": options.User = value; break;
                case "-p": case "--password": options.Password = value; break;
                case "-n": case "--port":
                    if (!int.TryParse(value, out var port) || port is < 0 or > 65535) {
                        error = $"Invalid port: {value}";
                        return false;
                    }
                    options.Port = port;
                    break;
                case "-l": case "--loglevel":
                    if (!Enum.TryParse(value, true, out LogLevel level) || !Enum.IsDefined(level)) {
                        error = $"Invalid log level: {value}";
                        return false;
                    }
                    options.LogLevel = level;
                    break;
                default:
                    error = $"Unknown option: {option}";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(options.File)) {
            error = "A file is required.";
            return false;
        }
        return true;
    }
}
