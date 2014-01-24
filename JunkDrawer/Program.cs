using System;

namespace JunkDrawer {

    public class Program {

        static void Main(string[] args) {

            var request = new Request(args);

            if (!request.IsValid)
                Environment.Exit(1);

            new FileProcessor().Process(FileInformationFactory.Create(request.FileInfo.FullName));
        }

    }
}
