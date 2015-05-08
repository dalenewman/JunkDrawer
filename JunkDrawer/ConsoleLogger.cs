using System;
using System.Linq;
using Transformalize.Configuration;
using Transformalize.Logging;

namespace JunkDrawer {

    public class ConsoleLogger : ILogger {

        private const string LAYOUT = "{0:hh:mm:ss} | {1} | {2} | {3} | {4}";
        private readonly Func<string, string, string, string, object[], string> _formatter = (message, level, process, entity, args) => string.Format(string.Format(LAYOUT, DateTime.Now, level, process, entity, message), args);

        public string Name { get; set; }

        public ConsoleLogger() {
            Name = "Test";
        }

        public void Info(string message, params object[] args) {
            EntityInfo(NoEntity, message, args);
        }

        public void Debug(string message, params object[] args) {
            EntityDebug(NoEntity, message, args);
        }

        public void Warn(string message, params object[] args) {
            EntityWarn(NoEntity, message, args);
        }

        public void Error(string message, params object[] args) {
            EntityError(NoEntity, message, args);
        }

        public void Error(Exception exception, string message, params object[] args) {
            EntityError(NoEntity, exception, message, args);
        }

        public void EntityInfo(string entity, string message, params object[] args) {
            Console.WriteLine(_formatter(message, "info ", Name, entity, args));
        }

        public void EntityDebug(string entity, string message, params object[] args) {
            Console.WriteLine(_formatter(message, "debug", Name, entity, args));
        }

        public void EntityWarn(string entity, string message, params object[] args) {
            Console.WriteLine(_formatter(message, "warn ", Name, entity, args));
        }

        public void EntityError(string entity, string message, params object[] args) {
            Console.Error.WriteLine(_formatter(message, "ERROR", Name, entity, args));
        }

        public void EntityError(string entity, Exception exception, string message, params object[] args) {
            Console.Error.WriteLine(_formatter(message, "ERROR", Name, entity, args));
            Console.Error.WriteLine(exception.StackTrace);
        }

        public void Start(TflProcess process) {
            Name = process.Name;
            NoEntity = new string('.', process.Entities.Select(e => e.Name.Length).Max());
            Info("Start logging {0}!", process.Name);
        }

        public string NoEntity { get; set; }

        public void Stop() {
            Info("Stop logging {0}!", Name);
            Info(string.Empty);
        }
    }
}
