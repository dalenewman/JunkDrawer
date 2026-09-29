namespace JunkDrawer.Eto.Core;

public static class GuiLaunchOptions {
    public static (string? File, string? Arrangement) Parse(string[] args) {
        string? file = null;
        string? arrangement = null;
        for (var index = 0; index < args.Length; index++) {
            switch (args[index]) {
                case "-a":
                case "--arrangement":
                    if (++index >= args.Length) throw new ArgumentException("An arrangement path is required after -a.");
                    arrangement = args[index];
                    break;
                default:
                    if (args[index].StartsWith("-", StringComparison.Ordinal))
                        throw new ArgumentException($"Unknown option: {args[index]}");
                    if (file is not null) throw new ArgumentException("Only one input file may be opened at startup.");
                    file = args[index];
                    break;
            }
        }
        return (file, arrangement);
    }
}
