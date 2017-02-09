#region license
// Test
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using JunkDrawer;
using JunkDrawer.Autofac;
using NUnit.Framework;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Transformalize.Desktop.Loggers;

namespace Test {

    [TestFixture]
    public class Integration {

        static readonly object[] Connections = {
            //new object[] { "sqlserver", "Junk", "", "", "" },
            //new object[] { "postgresql", "Junk", "", "postgres", "devdev1!" },
            //new object[] { "mysql", "Junk", "", "root", "devdev1!" },
            //new object[] { "sqlite", "", "junk.sqlite3", "", "" }
            new object[] { "sqlce", "", "junk.sdf", "", "" }
        };

        [Test]
        public void Demo() {

            Response response;
            var request = new Request(@"C:\Code\JunkDrawer\Test\sample.txt");
            using (var scope = new Bootstrapper(request)) {
                response = scope.Resolve<Importer>().Import();
            }

            Assert.AreEqual("sample", response.View);
            Assert.AreEqual(4, response.Records);

        }

        [Test, TestCaseSource(nameof(Connections))]
        public void CommaDelimited(string provider, string database, string file, string user, string password) {

            const string content = @"Name,WebSite,Created
Google|,http://www.google.com|,9/4/98
Apple|,http://www.apple.com|,4/1/1976
Microsoft|,http://www.microsoft.com|,4/4/1975";

            var fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, content);

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password};
            using (var scope = new Bootstrapper(request, new ConsoleLogger(LogLevel.Debug))) {
                var importer = scope.Resolve<Importer>();
                response = importer.Import();
            }

            var companies = new List<Company>();
            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                companies.AddRange(cn.Query<Company>(response.Sql));
            }

            Assert.AreEqual(3, companies.Count);

            var google = companies.First(c => c.Name == "Google|");
            var apple = companies.First(c => c.Name == "Apple|");

            Assert.AreEqual("http://www.google.com|", google.WebSite);
            Assert.AreEqual(Convert.ToDateTime("4/1/1976"), apple.Created);

        }


        [Test, TestCaseSource(nameof(Connections))]
        public void CommaDelimitedNoNames(string provider, string database, string file, string user, string password) {

            const string content = @"Google,http://www.google.com,9/4/98
Apple,http://www.apple.com,4/1/1976
Microsoft,http://www.microsoft.com,4/4/1975";

            var fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, content);

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password };
            using (var scope = new Bootstrapper(request)) {
                var importer = scope.Resolve<Importer>();
                response = importer.Import();
            }

            var companies = new List<Abc>();

            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                companies.AddRange(cn.Query<Abc>(response.Sql));
            }

            Assert.AreEqual(3, companies.Count);

            var google = companies.First(c => c.A == "Google");
            var apple = companies.First(c => c.A == "Apple");

            Assert.AreEqual("http://www.google.com", google.B);
            Assert.AreEqual(Convert.ToDateTime("4/1/1976"), apple.C);

        }


        [Test, TestCaseSource(nameof(Connections))]
        public void CommaSeparatedValues(string provider, string database, string file, string user, string password) {

            const string content = @"Name,WebSite,Created
""Google"",http://www.google.com,9/4/98
Apple,http://www.apple.com,""April, 1 1976""
Microsoft,""http://www.microsoft.com"",4/4/1975
""Nike, Inc."",http://www.nike.com,1/25/1964";

            var fileName = Path.GetTempFileName().Replace(".tmp", ".csv");
            File.WriteAllText(fileName, content);

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password };
            using (var scope = new Bootstrapper(request)) {
                var importer = scope.Resolve<Importer>();
                response = importer.Import();
            }

            var companies = new List<Company>();

            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                companies.AddRange(cn.Query<Company>(response.Sql));
            }

            Assert.AreEqual(4, companies.Count);

            var google = companies.First(c => c.Name == "Google");
            var apple = companies.First(c => c.Name == "Apple");
            var nike = companies.First(c => c.Name == "Nike, Inc.");

            Assert.AreEqual("http://www.google.com", google.WebSite);
            Assert.AreEqual(Convert.ToDateTime("4/1/1976"), apple.Created);
            Assert.AreEqual(Convert.ToDateTime("1/25/1964"), nike.Created);
        }


        [Test, TestCaseSource(nameof(Connections))]
        public void CsvWithQuotedColumnNames(string provider, string database, string file, string user, string password) {

            const string content = @"""Name"",""WebSite"",""Created""
""Google"",http://www.google.com,9/4/98
Apple,http://www.apple.com,""April, 1 1976""
Microsoft,""http://www.microsoft.com"",4/4/1975
""Nike, Inc."",http://www.nike.com,1/25/1964";

            var fileName = Path.GetTempFileName().Replace(".tmp", ".csv");
            File.WriteAllText(fileName, content);

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password };
            using (var scope = new Bootstrapper(request)) {
                var importer = scope.Resolve<Importer>();
                response = importer.Import();
            }

            var companies = new List<Company>();

            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                companies.AddRange(cn.Query<Company>(response.Sql));
            }

            Assert.AreEqual(4, companies.Count);

        }

        [Test, TestCaseSource(nameof(Connections))]
        public void CsvWithMissingAndQuotedColumnNames(string provider, string database, string file, string user, string password) {

            const string content = @",,""Created""
""Google"",http://www.google.com,9/4/98
Apple,http://www.apple.com,""April, 1 1976""
Microsoft,""http://www.microsoft.com"",4/4/1975
""Nike, Inc."",http://www.nike.com,1/25/1964";

            var fileName = Path.GetTempFileName().Replace(".tmp", ".csv");
            File.WriteAllText(fileName, content);

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password };
            using (var scope = new Bootstrapper(request)) {
                var importer = scope.Resolve<Importer>();
                response = importer.Import();
            }

            var companies = new List<Company>();

            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                companies.AddRange(cn.Query<Company>(response.Sql));
            }

            Assert.AreEqual(4, companies.Count);

        }

        [Test, TestCaseSource(nameof(Connections))]
        public void ExcelXls(string provider, string database, string file, string user, string password) {

            const string fileName = @"C:\Code\JunkDrawer\Test\Files\Excel1.xls";

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password };
            using (var scope = new Bootstrapper(request)) {
                var importer = scope.Resolve<Importer>();
                response = importer.Import();
            }

            var companies = new List<CompanyForOldExcel>();

            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                companies.AddRange(cn.Query<CompanyForOldExcel>(response.Sql));
            }

            Assert.AreEqual(4, companies.Count);

            var google = companies.First(c => c.Name == "Google");
            var apple = companies.First(c => c.Name == "Apple");
            var nike = companies.First(c => c.Name == "Nike, Inc.");

            Assert.AreEqual("http://www.google.com", google.WebSite);
            Assert.AreEqual(27851, apple.Created);
            Assert.AreEqual(23401, nike.Created);
        }

        [Test, TestCaseSource(nameof(Connections))]
        public void ExcelXlsx(string provider, string database, string file, string user, string password) {

            const string fileName = @"C:\Code\JunkDrawer\Test\Files\Excel2.xlsx";

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password };
            using (var scope = new Bootstrapper(request)) {
                var importer = scope.Resolve<Importer>();
                response = importer.Import();
            }

            var companies = new List<Company>();

            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                companies.AddRange(cn.Query<Company>(response.Sql));
            }

            Assert.AreEqual(4, companies.Count);

            var google = companies.First(c => c.Name == "Google");
            var apple = companies.First(c => c.Name == "Apple");
            var nike = companies.First(c => c.Name == "Nike, Inc.");

            Assert.AreEqual("http://www.google.com", google.WebSite);
            Assert.AreEqual(Convert.ToDateTime("4/1/1976"), apple.Created);
            Assert.AreEqual(Convert.ToDateTime("1/25/1964"), nike.Created);
        }

        [Test, TestCaseSource(nameof(Connections))]
        public void PipeDelimited(string provider, string database, string file, string user, string password) {

            const string content = @"Name|WebSite|Created
Google|http://www.google.com|9/4/98
Apple|http://www.apple.com|4/1/1976
Microsoft|http://www.microsoft.com|4/4/1975";

            var fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, content);

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password };
            using (var scope = new Bootstrapper(request)) {
                var importer = scope.Resolve<Importer>();
                response = importer.Import();
            }

            var companies = new List<Company>();

            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                companies.AddRange(cn.Query<Company>(response.Sql));
            }

            Assert.AreEqual(3, companies.Count);

            var google = companies.First(c => c.Name == "Google");
            var apple = companies.First(c => c.Name == "Apple");
            var ms = companies.First(c => c.Name == "Microsoft");

            Assert.AreEqual("http://www.google.com", google.WebSite);
            Assert.AreEqual(Convert.ToDateTime("4/1/1976"), apple.Created);
            Assert.AreEqual(Convert.ToDateTime("4/4/1975"), ms.Created);
        }

        [Test, TestCaseSource(nameof(Connections))]
        public void TabDelimited(string provider, string database, string file, string user, string password) {

            const string content = @"Name	WebSite	Created
Google	http://www.google.com	9/4/98
Apple	http://www.apple.com	4/1/1976
Microsoft	http://www.microsoft.com	4/4/1975";

            var fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, content);

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password };
            using (var scope = new Bootstrapper(request)) {
                response = scope.Resolve<Importer>().Import();
            }

            var companies = new List<Company>();

            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                companies.AddRange(cn.Query<Company>(response.Sql));
            }

            Assert.AreEqual(3, companies.Count);

            var google = companies.First(c => c.Name == "Google");
            var apple = companies.First(c => c.Name == "Apple");
            var ms = companies.First(c => c.Name == "Microsoft");

            Assert.AreEqual("http://www.google.com", google.WebSite);
            Assert.AreEqual(Convert.ToDateTime("4/1/1976"), apple.Created);
            Assert.AreEqual(Convert.ToDateTime("4/4/1975"), ms.Created);
        }

        [Test, TestCaseSource(nameof(Connections))]
        public void FixedWidth(string provider, string database, string file, string user, string password) {

            const string content = @"Name     WebSite                 Created
Google   http://www.google.com   9/4/98
Apple    http://www.apple.com    4/1/1976
Microsofthttp://www.microsoft.com4/4/1975";

            var fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, content);

            Response response;
            var request = new Request(fileName) { Provider = provider, Database = database, DatabaseFile = file, User = user, Password = password };
            using (var scope = new Bootstrapper(request)) {
                response = scope.Resolve<Importer>().Import();
            }

            var lines = new List<string>();

            using (var cn = ConnectionFactory.Create(new Connection { Provider = provider, Database = database, File = file, User = user, Password = password })) {
                lines.AddRange(cn.Query<string>(response.Sql));
            }

            Assert.AreEqual(3, lines.Count);

            Assert.NotNull(lines.FirstOrDefault(l => l.StartsWith("Google")));
            Assert.NotNull(lines.FirstOrDefault(l => l.StartsWith("Apple")));
            Assert.NotNull(lines.FirstOrDefault(l => l.StartsWith("Microsoft")));
        }

    }
}
