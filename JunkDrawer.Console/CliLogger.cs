using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Logging;

namespace JunkDrawer;

internal sealed class CliLogger(LogLevel level) : BaseLogger(level), IPipelineLogger {
    public void Debug(IContext context, Func<string> message) {
        if (DebugEnabled) Console.Error.WriteLine($"debug: {message()}");
    }

    public void Info(IContext context, string message, params object[] args) {
        if (InfoEnabled) Console.Error.WriteLine($"info: {string.Format(message, args)}");
    }

    public void Warn(IContext context, string message, params object[] args) {
        if (WarnEnabled) Console.Error.WriteLine($"warning: {string.Format(message, args)}");
    }

    public void Error(IContext context, string message, params object[] args) {
        if (ErrorEnabled) Console.Error.WriteLine($"error: {string.Format(message, args)}");
    }

    public void Error(IContext context, Exception exception, string message, params object[] args) {
        if (ErrorEnabled) Console.Error.WriteLine($"error: {string.Format(message, args)}: {exception.Message}");
    }

    public void Clear() { }
    public void SuppressConsole() { }
}
