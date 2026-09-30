using Cfg.Net.Reader;
using JunkDrawer;

namespace Test;

[TestClass]
public sealed class HomePathTests {
    [TestMethod]
    public void ExpandsOnlyLeadingHomeMarkerWithEitherSeparator() {
        var home = Path.Combine(Path.GetTempPath(), "jd-home");
        var expected = Path.GetFullPath(Path.Combine(home, ".junkdrawer", "junk.sqlite3"));

        Assert.AreEqual(expected, HomePath.Expand("~/.junkdrawer/junk.sqlite3", home));
        Assert.AreEqual(expected, HomePath.Expand("~\\.junkdrawer\\junk.sqlite3", home));
        Assert.AreEqual(expected, HomePath.Expand("~//.junkdrawer/junk.sqlite3", home));
        Assert.AreEqual(Path.GetFullPath(home), HomePath.Expand("~", home));
        Assert.AreEqual("~backup.sqlite3", HomePath.Expand("~backup.sqlite3", home));
        Assert.AreEqual("data/~backup.sqlite3", HomePath.Expand("data/~backup.sqlite3", home));
    }

    [TestMethod]
    public void ArrangementExpandsConnectionFilePath() {
        var arrangement = Path.Combine(Path.GetTempPath(), "jd-paths-" + Guid.NewGuid().ToString("N") + ".xml");
        try {
            File.WriteAllText(arrangement, """
                <jd><connections>
                  <add name="input" provider="file" file="*.*" />
                  <add name="output" provider="sqlite" file="~/.junkdrawer/junk.sqlite3" />
                </connections></jd>
                """);
            var cfg = new global::JunkDrawer.Cfg(arrangement, new FileReader());
            Assert.IsFalse(cfg.Errors().Any(), string.Join(Environment.NewLine, cfg.Errors()));
            Assert.AreEqual(HomePath.Expand("~/.junkdrawer/junk.sqlite3"), cfg.Output().File);
        } finally {
            File.Delete(arrangement);
        }
    }
}
