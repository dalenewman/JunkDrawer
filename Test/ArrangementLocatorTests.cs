using JunkDrawer;

namespace Test;

[TestClass]
public sealed class ArrangementLocatorTests {
    private string _root = null!;
    private string _working = null!;
    private string _application = null!;
    private string _home = null!;

    [TestInitialize]
    public void Initialize() {
        _root = Path.Combine(Path.GetTempPath(), "jd-arrangements-" + Guid.NewGuid().ToString("N"));
        _working = Path.Combine(_root, "working");
        _application = Path.Combine(_root, "application");
        _home = Path.Combine(_root, "home");
        Directory.CreateDirectory(_working);
        Directory.CreateDirectory(Path.Combine(_application, "Examples"));
        Directory.CreateDirectory(Path.Combine(_home, ".junkdrawer"));
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_root, recursive: true);

    [TestMethod]
    public void DefaultLookupUsesWorkingThenApplicationThenHomeThenExample() {
        var example = Create(_application, "Examples/sqlite.xml");
        Assert.AreEqual(example, Find());

        var personal = Create(Path.Combine(_home, ".junkdrawer"), "sqlite.xml");
        Assert.AreEqual(personal, Find());

        var application = Create(_application, "config.xml");
        Assert.AreEqual(application, Find());

        var working = Create(_working, "config.xml");
        Assert.AreEqual(working, Find());
    }

    [TestMethod]
    public void ExplicitAndEnvironmentSelectionOverrideDefaultsWithoutSilentFallback() {
        Create(_working, "config.xml");
        var personal = Create(Path.Combine(_home, ".junkdrawer"), "work.xml");
        Assert.AreEqual(personal, Find(environmentSelection: "work.xml"));
        Assert.AreEqual(personal, Find(selected: "work.xml", environmentSelection: "missing.xml"));
        Assert.AreEqual(personal, Find(selected: "~/.junkdrawer/work.xml"));
        Assert.ThrowsExactly<FileNotFoundException>(() => Find(selected: "missing.xml"));
    }

    private string Find(string? selected = null, string? environmentSelection = null) =>
        ArrangementLocator.Resolve(selected!, environmentSelection!, _working, _application, _home);

    private static string Create(string directory, string name) {
        var path = Path.Combine(directory, name);
        File.WriteAllText(path, "<jd />");
        return path;
    }
}
