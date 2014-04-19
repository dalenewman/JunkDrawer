using System;
using Transformalize.Libs.NLog;

namespace JunkDrawer {
    public class JunkDrawerException : Exception {
        private readonly Logger _log = LogManager.GetLogger("JunkDrawer");
        public JunkDrawerException(string format, params object[] args) {
            _log.Error(format, args);
        }
    }
}
