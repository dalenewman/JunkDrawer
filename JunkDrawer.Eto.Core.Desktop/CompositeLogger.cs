using System;
using Pipeline.Context;
using Pipeline.Contracts;

namespace JunkDrawer.Eto.Core.Desktop {
    public class CompositeLogger : IPipelineLogger {
        private readonly IPipelineLogger[] _loggers;

        public CompositeLogger(params IPipelineLogger[] loggers) {
            _loggers = loggers;
        }

        public void Debug(PipelineContext context, Func<string> lambda) {
            foreach (var logger in _loggers) {
                logger.Debug(context, lambda);
            }
        }

        public void Info(PipelineContext context, string message, params object[] args) {
            foreach (var logger in _loggers) {
                logger.Info(context, message, args);
            }
        }

        public void Warn(PipelineContext context, string message, params object[] args) {
            foreach (var logger in _loggers) {
                logger.Warn(context, message, args);
            }
        }

        public void Error(PipelineContext context, string message, params object[] args) {
            foreach (var logger in _loggers) {
                logger.Error(context, message, args);
            }
        }

        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {
            foreach (var logger in _loggers) {
                logger.Error(context, exception, message, args);
            }
        }

        public LogLevel LogLevel { get; }
    }
}