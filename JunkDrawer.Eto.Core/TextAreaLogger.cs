#region license
// JunkDrawer.Eto.Core
// Copyright 2013 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using Eto.Forms;
using Pipeline.Context;
using Pipeline.Contracts;

namespace JunkDrawer.Eto.Core {
    public class TextAreaLogger : IPipelineLogger {

        const string Context = "{0} | {1} | {2} | {3}";
        public TextAreaLogger(LogLevel logLevel) {
            LogLevel = logLevel;
        }

        public void Clear() {
            if (!global::Eto.Platform.Instance.IsDesktop)
                return;
            if (Application.Instance != null) {
                var form = Application.Instance.MainForm as MainForm;
                var eventLog = form?.LogArea;

                if (eventLog != null) {
                    Application.Instance.Invoke(() => { eventLog.Text = string.Empty; });
                }
            }

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
