using Cfg.Net.Reader;
using Eto.Drawing;
using Eto.Forms;
using JunkDrawer.Autofac;
using SQL.Formatter;
using Transformalize;
using Order = Transformalize.Configuration.Order;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Logging;

namespace JunkDrawer.Eto.Core;

public sealed class MainForm : Form {
    private enum DetailView { Logs, Sql, Arrangement }

    private static readonly Bitmap SqlIcon = Bitmap.FromResource("JunkDrawer.Eto.Core.Images.sql.png");
    private static readonly Bitmap LogIcon = Bitmap.FromResource("JunkDrawer.Eto.Core.Images.log.png");
    private static readonly Bitmap XmlIcon = Bitmap.FromResource("JunkDrawer.Eto.Core.Images.xml.png");
    private readonly string _arrangement;
    private readonly Cfg _cfg;
    private readonly List<string> _connectionNames;
    private string _selectedConnection;
    private readonly List<CheckMenuItem> _typeItems = new();
    private readonly RecentFiles _recentFiles = new();
    private ButtonMenuItem? _recentMenu;
    private readonly Label _pageLabel = new() { Text = "", VerticalAlignment = VerticalAlignment.Center };
    private readonly GridView _grid = new() { ShowHeader = true };
    private readonly List<Order> _sorts = new();
    private readonly Dictionary<GridColumn, string> _columnFields = new();
    private readonly TextArea _log = new() {
        ReadOnly = true,
        Wrap = false,
        TextColor = Colors.LightGreen,
        BackgroundColor = Colors.Black
    };
    private readonly TextArea _detailsView = new() {
        ReadOnly = true,
        Wrap = false,
        TextColor = Colors.LightGreen,
        BackgroundColor = Colors.Black
    };
    private readonly Panel _lowerPane = new();
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
    private readonly Button _details = new() {
        Image = SqlIcon,
        ToolTip = "Show current page SQL", MinimumSize = Size.Empty, Size = new Size(32, 28), Enabled = false
    };
    private readonly NumericStepper _pageSizeControl = new() {
        MinValue = 10, MaxValue = 50, Increment = 5, DecimalPlaces = 0, Value = 20, Width = 65
    };
    private Request? _request;
    private Response? _response;
    private int _page = 1;
    private int _lastPage = 1;
    private int _pageSize = 20;
    private string _pageQuery = string.Empty;
    private string _pageArrangement = string.Empty;
    private DetailView _detailView;
    private bool _busy;

    public MainForm(string arrangement, string? initialFile = null) {
        _arrangement = arrangement;
        _cfg = new Cfg(arrangement, new FileReader());
        if (_cfg.Errors().Any()) throw new InvalidOperationException(string.Join(Environment.NewLine, _cfg.Errors()));
        _connectionNames = _cfg.Connections.Where(c => c.Name != "input").Select(c => c.Name).ToList();
        _selectedConnection = _connectionNames.FirstOrDefault() ?? "output";
        _logger = new GuiLogger(line => Application.Instance.AsyncInvoke(() =>
            _log.Text += line + Environment.NewLine));

        Title = "Junk Drawer";
        ClientSize = new Size(900, 650);
        _lowerPane.Content = _log;
        AppendLog($"Arrangement: {arrangement}");

        CreateMenu();
        _first.Click += (_, _) => ShowPage(1);
        _previous.Click += (_, _) => ShowPage(_page - 1);
        _next.Click += (_, _) => ShowPage(_page + 1);
        _last.Click += (_, _) => ShowPage(_lastPage);
        _details.Click += (_, _) => ToggleDetails();
        _grid.ColumnHeaderClick += (_, args) => ToggleSort(args.Column);
        _pageSizeControl.ValueChanged += (_, _) => {
            _pageSize = (int)_pageSizeControl.Value;
            if (_response is not null) ShowPage(1);
        };

        var navigation = new StackLayout {
            Orientation = Orientation.Horizontal,
            VerticalContentAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Items = { _first, _previous, _pageLabel, _next, _last }
        };
        var actions = new StackLayout {
            Orientation = Orientation.Horizontal,
            VerticalContentAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Items = { _details }
        };
        var pageSize = new StackLayout {
            Orientation = Orientation.Horizontal,
            VerticalContentAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Items = { new Label { Text = "Page size", VerticalAlignment = VerticalAlignment.Center }, _pageSizeControl }
        };
        var toolbar = new TableLayout {
            Spacing = new Size(8, 0),
            Rows = { new TableRow(new TableCell(navigation, true), actions, pageSize) }
        };
        var content = new Splitter {
            Orientation = Orientation.Vertical,
            FixedPanel = SplitterFixedPanel.None,
            RelativePosition = 0.7,
            Panel1MinimumSize = 100,
            Panel2MinimumSize = 100,
            Panel1 = _grid,
            Panel2 = _lowerPane
        };
        Content = new TableLayout {
            Padding = 10,
            Spacing = new Size(6, 6),
            Rows = {
                new TableRow(toolbar),
                new TableRow(content) { ScaleHeight = true }
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
        _recentMenu = new ButtonMenuItem { Text = "Open &Recent..." };
        RefreshRecentMenu();
        var settings = new Command { MenuText = "&Settings" };
        settings.Executed += (_, _) => OpenSettings();
        var quit = new Command {
            MenuText = "&Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q
        };
        quit.Executed += (_, _) => Application.Instance.Quit();
        var fileMenu = new ButtonMenuItem { Text = "&File", Items = { open, _recentMenu, settings } };
        var connectionMenu = new ButtonMenuItem { Text = "&Connections" };
        if (Platform.Supports<RadioMenuItem>()) {
            RadioMenuItem? controller = null;
            for (var index = 0; index < _connectionNames.Count; index++) {
                var item = controller is null ? new RadioMenuItem() : new RadioMenuItem(controller);
                controller ??= item;
                item.Text = _connectionNames[index];
                item.Checked = _connectionNames[index] == _selectedConnection;
                var selectedConnection = _connectionNames[index];
                item.CheckedChanged += (_, _) => {
                    if (item.Checked) _selectedConnection = selectedConnection;
                };
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

    private void RefreshRecentMenu() {
        if (_recentMenu is null) return;
        _recentMenu.Items.Clear();
        foreach (var file in _recentFiles.Files) {
            var recentFile = file;
            var item = new ButtonMenuItem { Text = file.Replace("&", "&&") };
            item.Click += (_, _) => {
                if (_busy) return;
                if (!File.Exists(recentFile)) {
                    TryUpdateRecentFiles(() => _recentFiles.Remove(recentFile));
                    MessageBox.Show(this, $"File no longer exists: {recentFile}");
                    return;
                }
                StartImport(recentFile);
            };
            _recentMenu.Items.Add(item);
        }
        _recentMenu.Enabled = _recentMenu.Items.Count > 0;
    }

    private void TryUpdateRecentFiles(Action update) {
        try {
            update();
        } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
            AppendLog($"Could not save recent files: {ex.Message}");
        }
        RefreshRecentMenu();
    }

    private void OpenSettings() {
        try {
            var target = OperatingSystem.IsMacOS() ? new Uri(_arrangement).AbsoluteUri : _arrangement;
            Application.Instance.Open(target);
        } catch (Exception ex) {
            MessageBox.Show(this, $"Could not open settings: {ex.Message}");
        }
    }

    private void StartImport(string file) {
        if (_busy) return;
        var selected = _selectedConnection;
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
        ShowLog();
        _log.Text = $"Arrangement: {_arrangement}{Environment.NewLine}Importing {request.FileInfo.Name}…{Environment.NewLine}";
        _request = null;
        _response = null;
        _pageQuery = string.Empty;
        _pageArrangement = string.Empty;
        _detailsView.Text = string.Empty;
        _sorts.Clear();
        _grid.DataStore = null;
        _grid.Columns.Clear();
        _columnFields.Clear();
        _first.Enabled = _previous.Enabled = _next.Enabled = _last.Enabled = false;
        _details.Enabled = false;
        Task.Run(() => {
            try {
                using var bootstrapper = new Bootstrapper(request, _logger);
                var response = bootstrapper.Resolve<Importer>().Import();
                Application.Instance.AsyncInvoke(() => {
                    _request = request;
                    _response = response;
                    _page = 1;
                    AppendLog($"Imported {response.Records} records into {response.View}.");
                    TryUpdateRecentFiles(() => _recentFiles.Add(request.FileInfo.FullName));
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
            var result = bootstrapper.Resolve<Pager>(_request, _response).GetPage(page, _pageSize, _sorts);
            _page = page;
            _pageQuery = result.Query;
            _pageArrangement = result.Arrangement;
            _details.Enabled = _detailView != DetailView.Logs ||
                !string.IsNullOrWhiteSpace(_pageQuery) || !string.IsNullOrWhiteSpace(_pageArrangement);
            UpdateDetailsView();
            _grid.Columns.Clear();
            _columnFields.Clear();
            foreach (var field in result.Fields.Where(f => !f.System)) {
                var captured = field;
                var fieldName = string.IsNullOrWhiteSpace(captured.Alias) ? captured.Name : captured.Alias;
                var sort = _sorts.FirstOrDefault(item => item.Field == fieldName);
                var sortIndicator = sort is null ? "" : sort.Sort == "asc" ? "▲" : "▼";
                var column = new GridColumn {
                    HeaderText = sort is null ? fieldName : $"{fieldName} {sortIndicator}",
                    HeaderToolTip = "Click to sort ascending, descending, or clear the sort",
                    DataCell = new TextBoxCell {
                        Binding = new DelegateBinding<IRow, string>(row => row[captured]?.ToString() ?? "")
                    },
                    Resizable = true,
                    Sortable = true
                };
                _grid.Columns.Add(column);
                _columnFields.Add(column, fieldName);
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

    private void ToggleSort(GridColumn column) {
        if (_response is null) return;
        if (!_columnFields.TryGetValue(column, out var fieldName)) return;
        var sort = _sorts.FirstOrDefault(item => item.Field == fieldName);
        if (sort is null) _sorts.Add(new Order { Field = fieldName, Sort = "asc" });
        else if (sort.Sort == "asc") sort.Sort = "desc";
        else _sorts.Remove(sort);
        ShowPage(1);
    }

    private void ToggleDetails() {
        switch (_detailView) {
            case DetailView.Logs when !string.IsNullOrWhiteSpace(_pageQuery):
                _detailView = DetailView.Sql;
                break;
            case DetailView.Logs or DetailView.Sql when !string.IsNullOrWhiteSpace(_pageArrangement):
                _detailView = DetailView.Arrangement;
                break;
            default:
                ShowLog();
                return;
        }
        UpdateDetailsView();
        _lowerPane.Content = _detailsView;
        _details.Image = _detailView == DetailView.Sql ? XmlIcon : LogIcon;
        _details.ToolTip = _detailView == DetailView.Sql ? "Show current page arrangement XML" : "Show logs";
    }

    private void ShowLog() {
        _lowerPane.Content = _log;
        _detailView = DetailView.Logs;
        _details.Image = SqlIcon;
        _details.ToolTip = "Show current page SQL";
    }

    private void UpdateDetailsView() {
        if (_detailView == DetailView.Sql) UpdateSqlView();
        else if (_detailView == DetailView.Arrangement) _detailsView.Text = _pageArrangement;
    }

    private void UpdateSqlView() {
        if (string.IsNullOrWhiteSpace(_pageQuery)) {
            _detailsView.Text = string.Empty;
            return;
        }
        try {
            _detailsView.Text = _response?.Connection.Provider switch {
                "mysql" => SqlFormatter.Of("mysql").Format(_pageQuery),
                "postgresql" => SqlFormatter.Of("postgresql").Format(_pageQuery),
                "sqlserver" => SqlFormatter.Of("tsql").Format(_pageQuery),
                _ => SqlFormatter.Format(_pageQuery)
            };
        } catch (Exception ex) {
            AppendLog($"Could not format page SQL: {ex.Message}");
            _detailsView.Text = _pageQuery;
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
