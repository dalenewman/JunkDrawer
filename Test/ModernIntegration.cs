using JunkDrawer;
using JunkDrawer.Autofac;
using JunkDrawer.Eto.Core;
using Microsoft.Data.Sqlite;
using System.Threading;
using System.Xml.Linq;
using Transformalize.Configuration;

namespace Test;

[TestClass]
public sealed class ModernIntegration {
    private string _directory = null!;

    [TestInitialize]
    public void Initialize() {
        _directory = Path.Combine(Path.GetTempPath(), "jd-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TestCleanup]
    public void Cleanup() {
        // Some providers or background operations may leave file handles briefly open.
        // Retry deletion a few times to reduce flaky test failures caused by transient file locks.
        const int maxAttempts = 5;
        var attempt = 0;
        while (attempt++ < maxAttempts) {
            try {
                if (Directory.Exists(_directory)) {
                    // Clear read-only attributes just in case
                    foreach (var file in Directory.EnumerateFiles(_directory, "*", SearchOption.AllDirectories)) {
                        try {
                            File.SetAttributes(file, FileAttributes.Normal);
                        } catch { }
                    }
                    Directory.Delete(_directory, recursive: true);
                }
                break;
            } catch (IOException) {
                // Give other processes a moment to release handles
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(200);
            } catch (UnauthorizedAccessException) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(200);
            }
        }
    }

    [TestMethod]
    public void CsvImportCanRunTwiceAndPageResults() {
        var database = Path.Combine(_directory, $"data-{Guid.NewGuid():N}.sqlite3");
        var arrangement = CreateArrangement(database);
        var request = new Request(Path.Combine(AppContext.BaseDirectory, "Files", "CommaSeparatedValues.csv")) {
            Configuration = arrangement,
            Retries = 0
        };

        using var bootstrapper = new Bootstrapper(request);
        for (var run = 0; run < 2; run++) {
            var response = bootstrapper.Resolve<Importer>().Import();
            Assert.AreEqual(4L, response.Records);
            StringAssert.Contains(response.Sql, "WebSite");
            var page = bootstrapper.Resolve<Pager>(request, response).GetPage(1, 2);
            var nextPage = bootstrapper.Resolve<Pager>(request, response).GetPage(2, 2);
            StringAssert.Contains(page.Query, "LIMIT 0,2");
            StringAssert.Contains(nextPage.Query, "LIMIT 2,2");
            Assert.AreEqual(4L, page.Hits);
            Assert.AreEqual(2, page.Rows.Length);
        }

        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM CommaSeparatedValues";
        Assert.AreEqual(4L, (long)command.ExecuteScalar()!);
    }

    [TestMethod]
    public void PageResultsApplyRequestedSortBeforePaging() {
        var database = Path.Combine(_directory, "sorted.sqlite3");
        var request = new Request(Path.Combine(AppContext.BaseDirectory, "Files", "CommaSeparatedValues.csv")) {
            Configuration = CreateArrangement(database),
            Retries = 0
        };

        using var bootstrapper = new Bootstrapper(request);
        var response = bootstrapper.Resolve<Importer>().Import();
        var ascending = bootstrapper.Resolve<Pager>(request, response).GetPage(1, 2,
            [new Order { Field = "Name", Sort = "asc" }]);
        var descending = bootstrapper.Resolve<Pager>(request, response).GetPage(1, 2,
            [new Order { Field = "Name", Sort = "desc" }]);
        var multiple = bootstrapper.Resolve<Pager>(request, response).GetPage(1, 2,
            [new Order { Field = "Name", Sort = "asc" }, new Order { Field = "WebSite", Sort = "desc" }]);
        var name = ascending.Fields.Single(field => field.Alias == "Name");

        Assert.AreEqual("Apple", ascending.Rows[0][name]?.ToString());
        Assert.AreEqual("Google", ascending.Rows[1][name]?.ToString());
        Assert.AreEqual("Nike, Inc.", descending.Rows[0][name]?.ToString());
        StringAssert.Contains(ascending.Query.ToLowerInvariant(), "order by");
        StringAssert.Contains(descending.Query.ToLowerInvariant(), "desc");
        StringAssert.Contains(multiple.Query, "WebSite");
    }

    [TestMethod]
    public void PageResultsIncludeArrangementXml() {
        var database = Path.Combine(_directory, "arrangement-pages.sqlite3");
        var request = new Request(Path.Combine(AppContext.BaseDirectory, "Files", "CommaSeparatedValues.csv")) {
            Configuration = CreateArrangement(database),
            Retries = 0
        };

        using var bootstrapper = new Bootstrapper(request);
        var response = bootstrapper.Resolve<Importer>().Import();
        var page = bootstrapper.Resolve<Pager>(request, response).GetPage(1, 2);

        Assert.IsFalse(string.IsNullOrWhiteSpace(page.Arrangement));
        var original = XDocument.Parse(page.Arrangement);
        var simplified = XDocument.Parse(ArrangementXml.Simplify(page.Arrangement));
        Assert.IsTrue(original.Descendants("searchtypes").Any());
        Assert.IsFalse(simplified.Descendants("searchtypes").Any());
        var entity = simplified.Root!.Element("entities")!.Element("add")!;
        Assert.AreEqual("1", entity.Attribute("page")!.Value);
        Assert.AreEqual("2", entity.Attribute("size")!.Value);
        Assert.IsTrue(original.Descendants("fields").Elements("add")
            .Any(field => field.Attribute("system")?.Value == "true"));
        CollectionAssert.AreEqual(
            page.Fields.Select(field => field.Name).ToArray(),
            entity.Element("fields")!.Elements("add").Select(field => field.Attribute("name")!.Value).ToArray());
    }

    [TestMethod]
    public void DefaultTableMarkerUsesInputFileName() {
        var database = Path.Combine(_directory, "default-view.sqlite3");
        var arrangement = CreateArrangement(database);
        var request = new Request(Path.Combine(AppContext.BaseDirectory, "Files", "CommaSeparatedValues.csv")) {
            Configuration = arrangement,
            View = Transformalize.Constants.DefaultSetting,
            Retries = 0
        };

        using var bootstrapper = new Bootstrapper(request);
        var response = bootstrapper.Resolve<Importer>().Import();
        Assert.AreEqual("CommaSeparatedValues", response.View);
        Assert.AreEqual(4L, response.Records);
    }

    [TestMethod]
    public void CliReportsMissingInput() {
        var code = JunkDrawer.Program.Main(["-f", Path.Combine(_directory, "missing.csv"), "-a", CreateArrangement(Path.Combine(_directory, $"data-{Guid.NewGuid():N}.sqlite3"))]);
        Assert.AreEqual(1, code);
    }

    [TestMethod]
    [DataRow("Excel1.xls")]
    [DataRow("Excel2.xlsx")]
    public void ExcelCanImport(string fileName) {
        var database = Path.Combine(_directory, $"data-{Guid.NewGuid():N}.sqlite3");
        var arrangement = CreateArrangement(database);
        var request = new Request(Path.Combine(AppContext.BaseDirectory, "Files", fileName)) {
            Configuration = arrangement,
            Retries = 0
        };
        using var bootstrapper = new Bootstrapper(request);
        var response = bootstrapper.Resolve<Importer>().Import();
        Assert.IsGreaterThan(0L, response.Records);
    }

    [TestMethod]
    public void CliParserAcceptsBareFileAndOptions() {
        Assert.IsTrue(Options.TryParse(["file.csv", "-a", "sqlite.xml"], out var options, out var error), error);
        Assert.AreEqual("file.csv", options.File);
        Assert.AreEqual("sqlite.xml", options.Configuration);
        Assert.IsFalse(Options.TryParse(["-f", "file.csv", "-n", "invalid"], out _, out _));
    }

    [TestMethod]
    public void OutputConnectionCanBeSwitched() {
        var primary = Path.Combine(_directory, "primary.sqlite3");
        var secondary = Path.Combine(_directory, "secondary.sqlite3");
        var arrangement = Path.Combine(_directory, "connections.xml");
        File.WriteAllText(arrangement, $"""
            <jd><connections>
              <add name="input" provider="file" file="*.*" />
              <add name="output" provider="sqlite" file="{primary}" />
              <add name="other" provider="sqlite" file="{secondary}" />
            </connections></jd>
            """);
        var request = new Request(Path.Combine(AppContext.BaseDirectory, "Files", "CommaSeparatedValues.csv")) {
            Configuration = arrangement,
            Provider = "sqlite",
            DatabaseFile = secondary,
            Retries = 0
        };
        using var bootstrapper = new Bootstrapper(request);
        request.View = bootstrapper.Resolve<JunkDrawer.Cfg>().Connections.First(c => c.Name == "other").Table;
        var response = bootstrapper.Resolve<Importer>().Import();
        Assert.AreEqual(4L, response.Records);
        Assert.IsTrue(File.Exists(secondary));
        Assert.IsFalse(File.Exists(primary));
    }

    [TestMethod]
    [DataRow("CommaDelimited.txt", 3L)]
    [DataRow("PipeDelimited.txt", 3L)]
    [DataRow("TabDelimited.txt", 3L)]
    [DataRow("CsvWithDoubleQuotesAroundHeaders.csv", 4L)]
    public void DelimitersAndQuotedHeadersImport(string fileName, long expected) {
        var request = new Request(Path.Combine(AppContext.BaseDirectory, "Files", fileName)) {
            Configuration = CreateArrangement(Path.Combine(_directory, $"data-{Guid.NewGuid():N}.sqlite3")),
            Retries = 0
        };
        using var bootstrapper = new Bootstrapper(request);
        var response = bootstrapper.Resolve<Importer>().Import();
        Assert.AreEqual(expected, response.Records);
        Assert.IsTrue(response.Fields.Any(f => f.Alias == "WebSite"));
    }

    private string CreateArrangement(string database) {
        var arrangement = Path.Combine(_directory, "arrangement.xml");
        File.WriteAllText(arrangement, $"""
            <jd><connections>
              <add name="input" provider="file" file="*.*" />
              <add name="output" provider="sqlite" file="{database}" />
            </connections></jd>
            """);
        return arrangement;
    }
}
