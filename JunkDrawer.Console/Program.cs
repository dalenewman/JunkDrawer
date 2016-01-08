using System;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Environment = System.Environment;

namespace JunkDrawer {

    public class Program {

        private const int Error = 1;

        static void Main(string[] args) {

            if (args == null || args.Length == 0) {
                Console.Error.WriteLine("You must pass in the name of a file you'd like to import into your junk drawer.");
                WriteUsage();
                Environment.Exit(Error);
            }

            // check if request has valid file
            var request = new Request(args[0], args.Length > 1 ? args[1] : "default.xml", 3);
            if (!request.IsValid) {
                Console.Error.WriteLine(request.Message);
                Environment.Exit(Error);
            }

            try {
                //register
                var builder = new ContainerBuilder();
                builder.RegisterModule(new JunkModule(request));

                using (var scope = builder.Build().BeginLifetimeScope()) {
                    // resolve
                    var cfg = scope.Resolve<JunkCfg>();

                    if (cfg.Errors().Any()) {
                        foreach (var error in cfg.Errors()) {
                            Console.Error.WriteLine(error);
                        }
                        Environment.Exit(Error);
                    }


                    var reader = scope.Resolve<ISchemaReader>(new TypedParameter(typeof (Connection), cfg.Input()));

                    var schema = reader.Read();

                    Console.WriteLine(schema.Entities.First().Fields.Count());

                }  // release


            } catch (Exception ex) {
                Console.Error.WriteLine(ex.Message);
            }

        }

        private static void WriteUsage() {

            Console.WriteLine("Usage: jd.exe <filename> (<config>)");
        }
    }
}
