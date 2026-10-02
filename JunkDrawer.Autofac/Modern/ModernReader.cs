using Autofac;
using Transformalize.Configuration;
using Transformalize.Containers.Autofac;
using Transformalize.Contracts;

namespace JunkDrawer.Autofac.Modern;

internal sealed class ModernReader(IPipelineLogger logger) : IRunTimeRun {
    public IEnumerable<IRow> Run(Process process) {
        ImportPlanBuilder.ThrowOnErrors(process);
        using var scope = new Container(ProviderModules.Create()).CreateScope(process, logger);
        return scope.Resolve<IProcessController>().Read().ToArray();
    }

    public async Task<IEnumerable<IRow>> RunAsync(Process process, CancellationToken token = default) {
        ImportPlanBuilder.ThrowOnErrors(process);
        using var scope = new Container(ProviderModules.Create()).CreateScope(process, logger);
        return (await scope.Resolve<IProcessController>().ReadAsync(token)).ToArray();
    }
}
