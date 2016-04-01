using System;
using Eto.Forms;
using Pipeline.Context;
using Pipeline.Contracts;

namespace JunkDrawer.Eto.Core.Desktop {
    public class TextAreaLogger : IPipelineLogger {

        const string Context = "{0} | {1} | {2} | {3}";
        public TextAreaLogger(LogLevel logLevel) {
            LogLevel = logLevel;
        }

        public LogLevel LogLevel { get; }
        static string ForLog(PipelineContext context) {
            return string.Format(Context, context.ForLog);
        }

        private static void Write(string message) {
            if (!global::Eto.Platform.Instance.IsDesktop)
                return;
            if (Application.Instance != null) {
                var form = Application.Instance.MainForm as MainForm;
                var eventLog = form?.LogArea;

                Application.Instance.Invoke((Action)(() => {
                    eventLog?.Append(message, true);
                }));
            }
        }

        public void Debug(PipelineContext context, Func<string> lamda) {
            if (LogLevel <= LogLevel.Debug)
                Write("debug | " + ForLog(context) + " | " + lamda() + Environment.NewLine);
        }

        public void Info(PipelineContext context, string message, params object[] args) {
            if (LogLevel <= LogLevel.Info) {
                Write("info  | " + ForLog(context) + " | " + string.Format(message, args) + Environment.NewLine);
            }
        }

        public void Warn(PipelineContext context, string message, params object[] args) {
            if (LogLevel <= LogLevel.Warn) {
                Write("warn  | " + ForLog(context) + " | " + string.Format(message, args) + Environment.NewLine);
            }

        }

        public void Error(PipelineContext context, string message, params object[] args) {
            if (LogLevel <= LogLevel.Error) {
                var custom = string.Format(message, args);
                Write("error | " + ForLog(context) + " | " + custom + Environment.NewLine);
            }
        }

        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {
            if (LogLevel <= LogLevel.Error) {
                var custom = string.Format(message, args);
                Write(exception.Message + Environment.NewLine);
                Write("error | " + ForLog(context) + " | " + custom + Environment.NewLine);
                if (exception.StackTrace != null) {
                    Write(exception.StackTrace + Environment.NewLine);
                }
            }

        }
    }
}
