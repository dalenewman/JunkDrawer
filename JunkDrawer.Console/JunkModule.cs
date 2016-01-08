using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Ext;
using Cfg.Net.Reader;
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Desktop;
using Pipeline.Logging.NLog;

namespace JunkDrawer {
    public class JunkModule : Module {
        private readonly Request _request;

        public JunkModule(Request request) {
            _request = request;
        }

        protected override void Load(ContainerBuilder builder) {

            // Cfg-Net Registrations
            builder.RegisterType<SourceDetector>().As<ISourceDetector>();
            builder.RegisterType<FileReader>().Named<IReader>("file");
            builder.RegisterType<WebReader>().Named<IReader>("web");

            builder.Register<IReader>(ctx => new DefaultReader(
                ctx.Resolve<ISourceDetector>(),
                ctx.ResolveNamed<IReader>("file"),
                new ReTryingReader(ctx.ResolveNamed<IReader>("web"), attempts: 3))
            );

            builder.Register(ctx => {
                var cfg = new JunkCfg(
                    _request.Configuration,
                    ctx.Resolve<IReader>()
                );
                // modify the input provider based on the file name requested
                var input = cfg.Connections.First();
                input.File = _request.FileInfo.FullName;
                if (_request.Extension.StartsWith(".xls")) {
                    input.Provider = "excel";
                }
                return cfg;
            }).As<JunkCfg>();

            // Pipeline.Net Registrations
            builder.Register(ctx => new NLogPipelineLogger("Pipeline.Net", LogLevel.Info)).As<IPipelineLogger>().InstancePerLifetimeScope();
            builder.Register(ctx => new PipelineContext(ctx.Resolve<IPipelineLogger>(), new Process { Name = "JunkDrawer" }.WithDefaults())).As<PipelineContext>();
            builder.RegisterType<ConnectionFactory>();
            builder.Register(
                ctx => new PipelineRunner(
                    ctx.Resolve<IContext>()
                )
            ).As<IRun>().InstancePerDependency();

            builder.Register((ctx, p) => new ConnectionContext(ctx.Resolve<PipelineContext>(), p.TypedAs<Connection>())).As<ConnectionContext>();
            builder.Register((ctx, p) => ctx.Resolve<ConnectionFactory>().CreateSchemaReader(ctx.Resolve<ConnectionContext>(p))).As<ISchemaReader>();

        }
    }
}
