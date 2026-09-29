using System.Diagnostics;
using Cfg.Net.Reader;
using Eto.Drawing;
using Eto.Forms;
using JunkDrawer.Autofac;
using Transformalize;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Logging;

namespace JunkDrawer.Eto.Core;

public sealed class MainForm : Form {
    private readonly string _arrangement;
    private readonly Cfg _cfg;
    private readonly DropDown _connections = new();
    private readonly List<RadioMenuItem> _connectionItems = new();
    private readonly List<CheckMenuItem> _typeItems = new();
    private readonly Label _pageLabel = new() { Text = "" };
    private readonly GridView _grid = new() { ShowHeader = true };
    private readonly TextArea _log = new() {
        ReadOnly = true,
        Wrap = false,
        TextColor = Colors.LightGreen,
        BackgroundColor = Colors.Black
    };
    private readonly GuiLogger _logger;
    private readonly Button _first = new() {
        Image = Bitmap.FromResource("JunkDrawer.Eto.Core.Images.first.png"),
        ToolTip = "First page", MinimumSize = Size.Empty, Size = new Size(32, 28), Enabled = false
    };
    private readonly Button _previous = new() {
        Image = Bitmap.FromResource("JunkDrawer.Eto.Core.Images.previous.png"),
        ToolTip = "Previous page", MinimumSize = Size.Empty, Size = new Size(32, 28), Enabled = false
    };
    private readonly Button _next = new() {
        Image = Bitmap.FromResource("JunkDrawer.Eto.Core.Images.next.png"),
        ToolTip = "Next page", MinimumSize = Size.Empty, Size = new Size(32, 28), Enabled = false
    };
    private readonly Button _last = new() {
        Image = Bitmap.FromResource("JunkDrawer.Eto.Core.Images.last.png"),
        ToolTip = "Last page", MinimumSize = Size.Empty, Size = new Size(32, 28), Enabled = false
    };
    private readonly Button _sql = new() { Text = "SQL", Enabled = false };
    private readonly Button _openWith = new() { Text = "Open with", Enabled = false };
    private readonly NumericStepper _pageSizeControl = new() {
        MinValue = 10, MaxValue = 50, Increment = 5, DecimalPlaces = 0, Value = 20
    };
    private Request? _request;
    private Response? _response;
    private int _page = 1;
    private int _lastPage = 1;
    private int _pageSize = 20;
    private bool _busy;

    public MainForm(string arrangement, string? initialFile = null) {
        _arrangement = arrangement;
        _cfg = new Cfg(arrangement, new FileReader());
        if (_cfg.Errors().Any()) throw new InvalidOperationException(string.Join(Environment.NewLine, _cfg.Errors()));
        _logger = new GuiLogger(line => Application.Instance.AsyncInvoke(() =>
            _log.Text += line + Environment.NewLine));

        Title = "Junk Drawer";
        ClientSize = new Size(900, 650);
        AppendLog($"Arrangement: {arrangement}");

        var open = new Button { Text = "Open file" };
        open.Click += (_, _) => OpenFile();
        foreach (var connection in _cfg.Connections.Where(c => c.Name != "input")) _connections.Items.Add(connection.Name);
        _connections.SelectedIndex = 0;
        _connections.SelectedIndexChanged += (_, _) => {
            if (_connections.SelectedIndex >= 0 && _connections.SelectedIndex < _connectionItems.Count)
                _connectionItems[_connections.SelectedIndex].Checked = true;
        };
        CreateMenu();
        _first.Click += (_, _) => ShowPage(1);
        _previous.Click += (_, _) => ShowPage(_page - 1);
        _next.Click += (_, _) => ShowPage(_page + 1);
        _last.Click += (_, _) => ShowPage(_lastPage);
        _sql.Click += (_, _) => ShowSql();
        _openWith.Click += (_, _) => OpenWith();
        _pageSizeControl.ValueChanged += (_, _) => {
            _pageSize = (int)_pageSizeControl.Value;
            if (_response is not null) ShowPage(1);
        };

        var top = new StackLayout {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Items = { open, new Label { Text = "Connection" }, _connections, _sql, _openWith }
        };
        var navigation = new StackLayout {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Items = { _first, _previous, _pageLabel, _next, _last,
                new Label { Text = "Page size" }, _pageSizeControl }
        };
        Content = new TableLayout {
            Padding = 10,
            Spacing = new Size(6, 6),
            Rows = {
                new TableRow(top),
                new TableRow(navigation),
                new TableRow(_grid) { ScaleHeight = true },
                new TableRow(_log) { ScaleHeight = true }
            }
        };

        if (!string.IsNullOrWhiteSpace(initialFile)) Shown += (_, _) => StartImport(initialFile);
    }

    private void OpenFile() {
        using var dialog = new OpenFileDialog();
        if (dialog.ShowDialog(this) == DialogResult.Ok) StartImport(dialog.FileName);
    }

    private void CreateMenu() {
        if (!Platform.Supports<MenuBar>()) return;

        var open = new Command {
            MenuText = "&Open", Shortcut = Application.Instance.CommonModifier | Keys.O
        };
        open.Executed += (_, _) => OpenFile();
        var quit = new Command {
            MenuText = "&Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q
        };
        quit.Executed += (_, _) => Application.Instance.Quit();
        var fileMenu = new ButtonMenuItem { Text = "&File", Items = { open } };
        var connectionMenu = new ButtonMenuItem { Text = "&Connections" };
        if (Platform.Supports<RadioMenuItem>()) {
            RadioMenuItem? controller = null;
            for (var index = 0; index < _connections.Items.Count; index++) {
                var item = controller is null ? new RadioMenuItem() : new RadioMenuItem(controller);
                controller ??= item;
                item.Text = _connections.Items[index].Text;
                item.Checked = index == _connections.SelectedIndex;
                var selectedIndex = index;
                item.CheckedChanged += (_, _) => {
                    if (item.Checked && _connections.SelectedIndex != selectedIndex)
                        _connections.SelectedIndex = selectedIndex;
                };
                _connectionItems.Add(item);
                connectionMenu.Items.Add(item);
            }
        }

        var typeMenu = new ButtonMenuItem { Text = "&Types" };
        if (Platform.Supports<CheckMenuItem>()) {
            var configuredTypes = _cfg.Input().Types.Select(type => type.Type).ToHashSet();
            foreach (var type in new[] { "bool", "byte", "short", "int", "long", "single", "double", "decimal", "datetime", "guid" }) {
                var item = new CheckMenuItem { Text = type, Checked = configuredTypes.Any(value => value.StartsWith(type, StringComparison.OrdinalIgnoreCase)) };
                _typeItems.Add(item);
                typeMenu.Items.Add(item);
            }
            typeMenu.Items.Add(new CheckMenuItem { Text = "string", Checked = true, Enabled = false });
        }
        Menu = new MenuBar { Items = { fileMenu, connectionMenu, typeMenu }, QuitItem = quit };
    }

    private void StartImport(string file) {
        if (_busy) return;
        var selected = _connections.SelectedValue?.ToString() ?? "output";
        var connection = _cfg.Connections.First(c => c.Name == selected);
        var request = new Request(file) {
            Configuration = _arrangement,
            Provider = connection.Provider,
            DatabaseFile = connection.File,
            Database = connection.Database,
            Server = connection.Server,
            Port = connection.Port,
            Schema = connection.Schema,
            User = connection.User,
            Password = connection.Password,
            View = connection.Table == Constants.DefaultSetting ? null : connection.Table,
            Types = _typeItems.Where(item => item.Checked).Select(item => item.Text).ToList()
        };
        if (!request.IsValid()) {
            AppendLog(request.Message);
            return;
        }
        _busy = true;
        _log.Text = $"Arrangement: {_arrangement}{Environment.NewLine}Importing {request.FileInfo.Name}…{Environment.NewLine}";
        _request = null;
        _response = null;
        _grid.DataStore = null;
        _grid.Columns.Clear();
        _first.Enabled = _previous.Enabled = _next.Enabled = _last.Enabled = false;
        _sql.Enabled = _openWith.Enabled = false;
        Task.Run(() => {
            try {
                using var bootstrapper = new Bootstrapper(request, _logger);
                var response = bootstrapper.Resolve<Importer>().Import();
                Application.Instance.AsyncInvoke(() => {
                    _request = request;
                    _response = response;
                    _page = 1;
                    AppendLog($"Imported {response.Records} records into {response.View}.");
                    _sql.Enabled = _openWith.Enabled = true;
                    ShowPage(1);
                    _busy = false;
                });
            } catch (Exception ex) {
                Application.Instance.AsyncInvoke(() => {
                    AppendLog("Import failed.");
                    AppendLog(ex.ToString());
                    _busy = false;
                });
            }
        });
    }

    private void AppendLog(string line) => _log.Text += line + Environment.NewLine;

    private void ShowPage(int page) {
        if (_request is null || _response is null || page < 1) return;
        try {
            using var bootstrapper = new Bootstrapper(_request, _logger);
            var result = bootstrapper.Resolve<Pager>(_request, _response).GetPage(page, _pageSize);
            _page = page;
            _grid.Columns.Clear();
            foreach (var field in result.Fields.Where(f => !f.System)) {
                var captured = field;
                _grid.Columns.Add(new GridColumn {
                    HeaderText = captured.Alias,
                    DataCell = new TextBoxCell {
                        Binding = new DelegateBinding<IRow, string>(row => row[captured]?.ToString() ?? "")
                    },
                    Resizable = true
                });
            }
            _grid.DataStore = result.Rows;
            var pages = Math.Max(1, (int)Math.Ceiling((double)result.Hits / _pageSize));
            _lastPage = pages;
            _pageLabel.Text = $"Page {_page} of {pages} ({result.Hits} rows)";
            _first.Enabled = _previous.Enabled = _page > 1;
            _next.Enabled = _last.Enabled = _page < pages;
        } catch (Exception ex) {
            _log.Text += ex + Environment.NewLine;
        }
    }

    private void ShowSql() {
        if (_response is null) return;
        new Dialog {
            Title = "Generated SQL",
            ClientSize = new Size(650, 350),
            Content = new TextArea { Text = _response.Sql, ReadOnly = true, Wrap = false }
        }.ShowModal(this);
    }

    private void OpenWith() {
        if (_response is null) return;
        var selected = _connections.SelectedValue?.ToString() ?? "output";
        var connection = _cfg.Connections.First(c => c.Name == selected);
        if (string.IsNullOrWhiteSpace(connection.OpenWith)) {
            MessageBox.Show(this, "Set open-with on this connection in the arrangement.");
            return;
        }
        var target = string.IsNullOrWhiteSpace(connection.File) ? _response.Sql : connection.File;
        if (string.IsNullOrWhiteSpace(connection.File)) {
            var folder = new AppDataFolder();
            var path = folder.FileName(_request!.ToKey(_cfg));
            File.WriteAllText(path, _response.Sql);
            target = path;
        }
        try {
            var start = new ProcessStartInfo(connection.OpenWith);
            start.ArgumentList.Add(target);
            Process.Start(start);
        } catch (Exception ex) {
            MessageBox.Show(this, ex.Message);
        }
    }
}

internal sealed class GuiLogger(Action<string> write) : BaseLogger(LogLevel.Info), IPipelineLogger {
    public void Debug(IContext context, Func<string> message) {
        if (DebugEnabled) write($"debug: {message()}");
    }

    public void Info(IContext context, string message, params object[] args) {
        if (InfoEnabled) write($"info: {string.Format(message, args)}");
    }

    public void Warn(IContext context, string message, params object[] args) {
        if (WarnEnabled) write($"warning: {string.Format(message, args)}");
    }

    public void Error(IContext context, string message, params object[] args) {
        if (ErrorEnabled) write($"error: {string.Format(message, args)}");
    }

    public void Error(IContext context, Exception exception, string message, params object[] args) {
        if (ErrorEnabled) write($"error: {string.Format(message, args)}: {exception.Message}");
    }

    public void Clear() { }
    public void SuppressConsole() { }
}
