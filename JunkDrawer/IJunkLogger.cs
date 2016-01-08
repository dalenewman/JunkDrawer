namespace JunkDrawer {
    public interface IJunkLogger {
        void Error(string message);
        void Debug(string message);
        void Warn(string message);
        void Info(string message);
    }
}
