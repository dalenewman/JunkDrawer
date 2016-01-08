using System;
using JunkDrawer;

namespace Test {
    public class ConsoleLogger : IJunkLogger {
        public void Error(string message) {
            Console.Error.WriteLine(message);
        }

        public void Debug(string message) {
            Console.WriteLine(message);
        }

        public void Warn(string message) {
            Console.WriteLine(message);
        }

        public void Info(string message) {
            Console.WriteLine(message);
        }
    }
}
