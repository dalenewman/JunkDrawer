using System;
using System.Linq;
using Serilog;
using Serilog.Events;
using Transformalize.Configuration;
using Transformalize.Extensions;
using Transformalize.Main.Providers.Mail;

namespace JunkDrawer {

    public class JunkLogger : Transformalize.Logging.ILogger {

        private readonly ILogger _seriLogger;

        public string Entity { get; set; }

        public JunkLogger(JunkCfg junkCfg) {

            Name = "Junk Drawer";

            var cfg = new LoggerConfiguration();
            cfg.WriteTo.ColoredConsole(LogEventLevel.Information);
            _seriLogger = cfg.CreateLogger();
            _seriLogger.Information("Logging to colored console.");

            try {
                if (junkCfg.Log.Any()) {
                    foreach (var log in junkCfg.Log) {
                        switch (log.Provider) {
                            case "file":
                                _seriLogger.Information("Logging to file {0}.", log.File);
                                cfg.WriteTo.RollingFile(log.File, TranslateLevel(log.Level));
                                break;
                            case "email":
                                _seriLogger.Information("Logging to email {0}.", log.To);
                                var mail = (MailConnection)log.ConnectionInstance;
                                cfg.WriteTo.Email(log.From, log.To.Split(new[] { ';' }), mail.Server, null, null, TranslateLevel(log.Level));
                                break;
                        }
                    }
                    _seriLogger = cfg.CreateLogger();
                }
            } catch (Exception ex) {
                _seriLogger.Warning(ex, ex.Message);
            }
        }

        public void Info(string message, params object[] args) {
            EntityInfo(Entity, message, args);
        }

        public void Debug(string message, params object[] args) {
            EntityDebug(Entity, message, args);
        }

        public void Warn(string message, params object[] args) {
            EntityWarn(Entity, message, args);
        }

        public void Error(string message, params object[] args) {
            EntityError(Entity, message, args);
        }

        public void Error(Exception exception, string message, params object[] args) {
            EntityError(Entity, exception, message, args);
        }

        public void EntityInfo(string entity, string message, params object[] args) {
            _seriLogger.Information(entity + " " + message, args);
        }

        public void EntityDebug(string entity, string message, params object[] args) {
            _seriLogger.Debug(entity + " " + message, args);
        }

        public void EntityWarn(string entity, string message, params object[] args) {
            _seriLogger.Warning(entity + " " + message, args);
        }

        public void EntityError(string entity, string message, params object[] args) {
            _seriLogger.Error(entity + " " + message, args);
        }

        public void EntityError(string entity, Exception exception, string message, params object[] args) {
            _seriLogger.Error(entity + " " + exception, message, args);
        }

        public void Start(TflProcess process) {
            Entity = new string('.', process.Entities.Select(e => e.Name.Length).Max());
            _seriLogger.Information(Entity + " Transformalize Starting.");
        }

        private static LogEventLevel TranslateLevel(string level) {
            switch (level.ToLower().Left(4)) {
                case "debu":
                    return LogEventLevel.Debug;
                case "info":
                    return LogEventLevel.Information;
                case "warn":
                    return LogEventLevel.Warning;
                case "erro":
                    return LogEventLevel.Error;
                default:
                    goto case "info";
            }
        }

        public void Stop() {
            _seriLogger.Information("Transformalize Stopping.");
            _seriLogger.Information(string.Empty);
        }

        public string Name { get; set; }
    }
}
