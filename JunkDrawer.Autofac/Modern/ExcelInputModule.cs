using Autofac;
using Autofac.Core;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Nulls;
using Transformalize.Providers.Excel;

namespace JunkDrawer.Autofac.Modern;

internal sealed class ExcelInputModule : Module {
    protected override void Load(ContainerBuilder builder) {
        if (!builder.Properties.TryGetValue("Process", out var value)) return;
        if (value is not Process process) return;
        foreach (var entity in process.Entities.Where(e => process.Connections.First(c => c.Name == e.Input).Provider == "excel")) {
            var connection = process.Connections.First(c => c.Name == entity.Input);
            builder.Register<ISchemaReader>(context => {
                var file = new FileInfo(connection.File);
                var input = new InputContext(new PipelineContext(context.Resolve<IPipelineLogger>(), process, entity));
                var configuration = new ExcelInspection(input, file, 100).Create();
                var inspected = new Process(configuration);
                if (inspected.Errors().Any()) return new NullSchemaReader();
                return new ExcelSchemaReader(inspected, new InputContext(new PipelineContext(context.Resolve<IPipelineLogger>(), inspected, inspected.Entities.First())));
            }).Named<ISchemaReader>(connection.Key);
            builder.RegisterType<NullInputProvider>().Named<IInputProvider>(entity.Key);
            builder.Register<IRead>(context => {
                var input = context.ResolveNamed<InputContext>(entity.Key);
                var rows = context.ResolveNamed<IRowFactory>(entity.Key, new NamedParameter("capacity", input.RowCapacity));
                return new ExcelReader(input, rows);
            }).Named<IRead>(entity.Key);
        }
    }
}
