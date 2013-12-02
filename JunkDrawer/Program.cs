using System;
using System.IO;
using Transformalize.Configuration.Builders;
using Transformalize.Main;

namespace JunkDrawer {
    class Program {

        static void Main(string[] args) {

            var request = new Request(args);

            if (!request.IsValid)
                Environment.Exit(1);

            var fi = FileInformationFactory.Create(request, 5);

            var p = ProcessFactory.Create("Default");
            var builder = new ProcessBuilder(fi.ProcessName)
                .Star(fi.ProcessName.Replace('-', '_'))
                .Connection("input").Provider("File").File(request.FileInfo.FullName).Delimiter(fi.Delimiter).Start(request.FirstRowIsHeader ? 1 : 0)
                .Connection("output").Server(p.OutputConnection.Server).Database(p.OutputConnection.Database)
                .Entity("Data").Version("TflHashCode").IndexOptimizations(false);

            for (var i = 0; i < fi.ColumnCount; i++) {
                builder.Field(fi.ColumnNames[i]).Length(512);
            }
            builder.CalculatedField("TflHashCode").Type("System.Int32").PrimaryKey().Transform("concat").AllParameters().Transform("gethashcode");
            
            if (!Directory.Exists(request.ConfigurationDirectory)) {
                Directory.CreateDirectory(request.ConfigurationDirectory);
            }
            File.WriteAllText(request.ConfigurationFile, builder.Process().Serialize());

            ProcessFactory.Create(builder.Process(), new Options() { Mode = "init" }).Run();
            ProcessFactory.Create(builder.Process(), new Options() { Mode = "default" }).Run();

            //File.Delete(request.FileInfo.FullName);

        }
    }
}
