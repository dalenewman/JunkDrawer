using JunkDrawer.Autofac;

namespace JunkDrawer;

public static class Program {
    public static int Main(string[] args) {
        if (args.Any(arg => arg is "--help" or "-h")) {
            Console.WriteLine(Options.Usage);
            return 0;
        }

        if (!Options.TryParse(args, out var options, out var error)) {
            Console.Error.WriteLine(error);
            Console.Error.WriteLine(Options.Usage);
            return 2;
        }

        string configuration;
        try {
            configuration = ArrangementLocator.Resolve(options.Configuration);
        } catch (FileNotFoundException ex) {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }

        var request = new Request(options.File!) {
            Configuration = configuration,
            Types = options.Types,
            View = options.View,
            Provider = options.Provider,
            Server = options.Server,
            Database = options.Database,
            Schema = options.Schema,
            User = options.User,
            Password = options.Password,
            Port = options.Port
        };

        if (!request.IsValid()) {
            Console.Error.WriteLine(request.Message);
            return 1;
        }

        try {
            using var bootstrapper = new Bootstrapper(request, new CliLogger(options.LogLevel));
            var response = bootstrapper.Resolve<Importer>().Import();
            if (response.Records == 0) {
                Console.Error.WriteLine("Did not import any records.");
                return 1;
            }
            Console.WriteLine($"Imported {response.Records} records into {response.View}.");
            Console.WriteLine(response.Sql);
            return 0;
        } catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
