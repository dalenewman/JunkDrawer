using Cfg.Net.Ext;
using Transformalize.Configuration;

namespace JunkDrawer.Autofac.Modern;

internal static class PagePlanBuilder {
    public static Process Create(Response response) {
        var input = response.Connection.Clone();
        input.Name = "input";
        var process = new Process {
            Name = "Pager",
            Connections = new List<Connection> { input, new() { Name = "output", Provider = "internal" } },
            Entities = new List<Entity> {
                new() {
                    Name = response.View,
                    Input = "input",
                    Fields = response.Fields.Select(f => f.Clone()).ToList()
                }
            }
        };
        process.Load();
        ImportPlanBuilder.ThrowOnErrors(process);
        return process;
    }
}
