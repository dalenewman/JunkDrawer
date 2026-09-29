using Autofac;
using Transformalize.Configuration;
using Transformalize.Containers.Autofac;
using Transformalize.Contracts;

namespace JunkDrawer.Autofac.Modern;

internal sealed class ModernExecutor(IPipelineLogger logger) : IRunTimeExecute {
    public void Execute(Process process) {
        ImportPlanBuilder.ThrowOnErrors(process);
        using var scope = new Container(ProviderModules.Create()).CreateScope(process, logger);
        scope.Resolve<IProcessController>().Execute();
    }

    public async Task ExecuteAsync(Process process, CancellationToken token = default) {
        ImportPlanBuilder.ThrowOnErrors(process);
        using var scope = new Container(ProviderModules.Create()).CreateScope(process, logger);
        await scope.Resolve<IProcessController>().ExecuteAsync(token);
    }

    public void Execute(string cfg, Dictionary<string, string> parameters) => Execute(new Process(cfg));

    public Task ExecuteAsync(string cfg, Dictionary<string, string> parameters, CancellationToken token = default) => ExecuteAsync(new Process(cfg), token);
}
