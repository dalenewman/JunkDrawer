using System.Text.Json;

namespace JunkDrawer.Eto.Core;

internal sealed class RecentFiles {
    private const int Limit = 10;
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private readonly string _fileName;
    private readonly List<string> _files = new();

    public IReadOnlyList<string> Files => _files;

    public RecentFiles() : this(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "JunkDrawer", "recent-files.json")) { }

    internal RecentFiles(string fileName) {
        _fileName = fileName;
        try {
            if (File.Exists(fileName)) {
                foreach (var file in JsonSerializer.Deserialize<List<string>>(File.ReadAllText(fileName)) ?? new()) {
                    if (string.IsNullOrWhiteSpace(file) || !Path.IsPathFullyQualified(file) ||
                        _files.Contains(file, PathComparer)) continue;
                    _files.Add(file);
                    if (_files.Count == Limit) break;
                }
            }
        } catch (IOException) {
            // Recent files are optional; an unreadable list starts empty.
        } catch (UnauthorizedAccessException) {
        } catch (JsonException) {
        }
    }

    public void Add(string file) {
        file = Path.GetFullPath(file);
        _files.RemoveAll(existing => PathComparer.Equals(existing, file));
        _files.Insert(0, file);
        if (_files.Count > Limit) _files.RemoveRange(Limit, _files.Count - Limit);
        Save();
    }

    public void Remove(string file) {
        if (_files.RemoveAll(existing => PathComparer.Equals(existing, file)) > 0) Save();
    }

    private void Save() {
        Directory.CreateDirectory(Path.GetDirectoryName(_fileName)!);
        File.WriteAllText(_fileName, JsonSerializer.Serialize(_files));
    }
}
