using JunkDrawer.Eto.Core;

namespace Test;

[TestClass]
public sealed class RecentFilesTests {
    [TestMethod]
    public void KeepsTenDistinctFilesInMostRecentOrderAcrossInstances() {
        var settings = Path.Combine(Path.GetTempPath(), "jd-recents-" + Guid.NewGuid().ToString("N"), "recent-files.json");
        var files = Enumerable.Range(0, 11)
            .Select(index => Path.Combine(Path.GetTempPath(), $"file-{index}.csv"))
            .ToArray();

        try {
            var recent = new RecentFiles(settings);
            foreach (var file in files) recent.Add(file);
            recent.Add(files[5]);

            var expected = new[] { files[5] }
                .Concat(files.Skip(1).Reverse().Where(file => file != files[5]))
                .ToArray();
            CollectionAssert.AreEqual(expected, new RecentFiles(settings).Files.ToArray());

            recent.Remove(files[5]);
            CollectionAssert.AreEqual(expected.Skip(1).ToArray(), new RecentFiles(settings).Files.ToArray());
        } finally {
            Directory.Delete(Path.GetDirectoryName(settings)!, recursive: true);
        }
    }
}
