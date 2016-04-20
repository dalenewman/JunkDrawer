#region license
// JunkDrawer.Eto.Core
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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Pipeline.Contracts;
using Pipeline.Extensions;

namespace JunkDrawer.Eto.Core {
    public class MainForm : Form {

        private readonly string _configuration;
        private readonly IJunkBootstrapperFactory _factory;
        private readonly Cfg _cfg;
        private readonly IContext _context;
        private readonly IFolder _folder;
        private readonly LogLevel _logLevel;
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        public Response Response { get; set; }
        public TextArea LogArea { get; } = new TextArea {
            Wrap = false,
            Font = new Font("Courier,Terminal", 9),
            TextColor = Colors.LightGreen,
            BackgroundColor = Colors.Black
        };
        public int PageSize { get; set; }
        public int Page { get; set; }

        public MainForm(
            IJunkBootstrapperFactory factory,
            Cfg cfg,
            IContext context,
            IFolder folder,
            LogLevel logLevel,
            string file,
            string configuration
        ) {
            _factory = factory;
            _cfg = cfg;
            _context = context;
            _folder = folder;
            _logLevel = logLevel;
            _configuration = configuration;

            Title = "Junk Drawer GUI";
            ClientSize = new Size(800, 600);
            CreateMenu();

            _worker.DoWork += (sender, args) => {
                args.Result = Import(args.Argument as Request);
            };

            _worker.RunWorkerCompleted += (sender, args) => {
                Application.Instance.AsyncInvoke(() => {
                    Content = ((Func<Control>)args.Result)();
                });
            };

            if (string.IsNullOrEmpty(file))
                return;

            Content = GetWorkingLayout();
            _worker.RunWorkerAsync(CreateRequest(file));
        }

        private void CreateMenu() {
            // OPEN
            _context.Debug(() => "Creating Menu.");

            var open = new Command { MenuText = "&Open", ToolBarText = "Open", Shortcut = Application.Instance.CommonModifier | Keys.O };
            open.Executed += OpenOnExecuted;

            // QUIT
            var quit = new Command { MenuText = "&Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quit.Executed += (sender, e) => Application.Instance.Quit();

            if (Platform.Supports<MenuBar>()) {

                var fileMenu = new ButtonMenuItem { Text = "&File", Items = { open } };
                var connectionMenu = new ButtonMenuItem { Text = "&Connections", Items = { } };
                var typeMenu = new ButtonMenuItem { Text = "&Types", Items = { } };

                if (Platform.Supports<CheckMenuItem>()) {
                    var types = new[] { "bool", "byte", "short", "int", "long", "single", "double", "decimal", "datetime", "guid" };
                    foreach (var type in types) {
                        typeMenu.Items.Add(new CheckMenuItem { Text = type, Checked = _cfg.Input().Types.Any(t => t.Type.StartsWith(type)) });
                    }
                    typeMenu.Items.Add(new CheckMenuItem { Text = "string", Checked = true, Enabled = false });
                }

                if (Platform.Supports<RadioMenuItem>()) {
                    var controller = new RadioMenuItem { Text = "output", Checked = true };
                    connectionMenu.Items.Add(new RadioMenuItem(controller) { Text = "output", Checked = true });
                    connectionMenu.Items.AddSeparator();
                    foreach (var c in _cfg.Connections.Where(c => !c.Name.In("input", "output"))) {
                        connectionMenu.Items.Add(new RadioMenuItem(controller) { Text = c.Name, Checked = false });
                    }
                }

                Menu = new MenuBar {
                    Items = { fileMenu, connectionMenu, typeMenu },
                    QuitItem = quit
                };
            }
        }

        private void OpenOnExecuted(object sender, EventArgs eventArgs) {
            _context.Debug(() => "Opening a file.");
            var openDialogue = new OpenFileDialog();
            if (openDialogue.ShowDialog(this) != DialogResult.Ok) {
                _context.Debug(() => "Open file cancelled.");
                return;
            }
            _context.Debug(() => $"Selected {openDialogue.FileName}.");

            Title = $"Junk Drawer GUI ({openDialogue.FileName})";

            if (_worker.IsBusy) {
                MessageBox.Show(this, "You must cancel your last request before starting another.", "Busy", MessageBoxButtons.OK);
            } else {
                _context.Logger.Clear();
                Content = GetWorkingLayout();
                _worker.RunWorkerAsync(CreateRequest(openDialogue.FileName));
            }
        }

        private static Control GetSpinner() {
            var control = new DynamicLayout { DefaultSpacing = Extensions.Spacing };
            control.AddCentered(new Spinner {
                Size = new Size(100, 100),
                Visible = true,
                Enabled = true
            });
            return control;
        }

        private Func<Control> Import(Request request) {

            var success = true;
            try {
                Page = 1;
                PageSize = 20;
                using (var scope = _factory.Produce(request)) {
                    Response = scope.Resolve<Importer>(request).Import();
                    _folder.Write(request.ToKey(_cfg), Response.Sql);
                    _context.Info($"Imported {Response.Records} records into {Response.View}.");
                }
            } catch (Exception ex) {
                _context.Error(ex, ex.Message);
                success = false;
            }

            return success ? GetSuccessLayout(request) : GetFailLayout(request);
        }

        private Func<Control> GetSuccessLayout(Request request) {
            if (_logLevel == LogLevel.None)
                return () => GetPage(request);

            return () => new Splitter {
                Panel1 = new Panel { Content = GetPage(request), Height = -1 },
                Panel2 = new Panel { Content = LogArea, Height = 150 },
                Orientation = Orientation.Vertical,
                FixedPanel = SplitterFixedPanel.Panel2
            };

        }

        private Func<Control> GetFailLayout(Request request) {
            if (_logLevel == LogLevel.None)
                return () => new TextArea { Text = $"Could not import {request.FileInfo.Name}! Turn on logging to find out more." };

            return () => new Splitter {
                Panel1 = new Panel { Content = null, Height = 0 },
                Panel2 = new Panel { Content = LogArea, Height = 200 },
                Orientation = Orientation.Vertical,
                FixedPanel = SplitterFixedPanel.Panel2
            };
        }

        private Control GetWorkingLayout() {
            if (_logLevel == LogLevel.None)
                return GetSpinner();

            return new Splitter {
                Panel1 = new Panel { Content = GetSpinner(), Height = -1 },
                Panel2 = new Panel { Content = LogArea, Height = 150 },
                Orientation = Orientation.Vertical,
                FixedPanel = SplitterFixedPanel.Panel2,
            };
        }

        private Control GetPage(Request request) {

            if (Response.Records <= 0)
                return new TextArea { Text = $"Nothing found in {request.FileInfo.Name}." };

            PageResult result;
            using (var scope = _factory.Produce(request)) {
                result = scope.Resolve<Pager>(request, Response).GetPage(Page, PageSize);
            }

            var pages = Convert.ToInt32(Math.Ceiling((decimal)result.Hits / PageSize));

            var first = new Button { Image = Icons.First, Enabled = Page > 1, Size = new Size(-1, -1) };
            first.Click += (sender, args) => {
                Page = 1;
                Content = GetSuccessLayout(request)();
            };

            var previous = new Button { Image = Icons.Previous, Enabled = Page > 1, Size = new Size(-1, -1) };
            previous.Click += (sender, args) => {
                Page = Page - 1;
                Content = GetSuccessLayout(request)();
            };

            var description = new Label { Text = $"Page {Page} of {pages} ({result.Hits} items)" };

            var next = new Button { Image = Icons.Next, Enabled = Page < pages, Size = new Size(-1, -1) };
            next.Click += (sender, args) => {
                Page = Page + 1;
                Content = GetSuccessLayout(request)();
            };

            var last = new Button { Image = Icons.Last, Enabled = Page < pages, Size = new Size(-1,-1)};
            last.Click += (sender, args) => {
                Page = pages;
                Content = GetSuccessLayout(request)();
            };

            var sql = new Button { Image = Icons.Sql, Size = new Size(-1, -1) };
            sql.Click += (sender, args) => {
                var selected = Menu.Items.GetSubmenu("Connections").Items.Where(i => !string.IsNullOrEmpty(i.Text)).Cast<RadioMenuItem>().First(mu => mu.Checked).Text;
                var connection = _cfg.Connections.Where(c => c.Name != "input").FirstOrDefault(c => selected == c.Name);

                if (string.IsNullOrEmpty(connection?.OpenWith)) {
                    MessageBox.Show(this, "You must add the open-with attribute in your output connection.  Set it to the application you want to use.", "Setup", MessageBoxButtons.OK);
                } else {
                    var arguments = string.IsNullOrEmpty(connection.File)
                        ? $" \"{(_folder.FileName(request.ToKey(_cfg)))}\""
                        : $"\"{connection.File}\"";  //sqlite

                    _context.Info($"Starting {connection.OpenWith} {arguments}");
                    var startInfo = new ProcessStartInfo { FileName = connection.OpenWith, Arguments = arguments };
                    Process.Start(startInfo);
                }
            };

            var pageSize = new NumericUpDown {
                MinValue = 10,
                MaxValue = 50,
                Increment = 5,
                DecimalPlaces = 0,
                Value = PageSize
            };

            pageSize.ValueChanged += (sender, args) => {
                Page = 1;
                PageSize = Convert.ToInt32(pageSize.Value);
                Content = GetSuccessLayout(request)();
            };

            var controls = new TableLayout {
                Spacing = Extensions.Spacing,
                Rows = { new TableRow(
                    null,
                    new TableCell(first),
                    new TableCell(previous),
                    new TableCell(description),
                    new TableCell(next),
                    new TableCell(last),
                    null,
                    new TableCell(sql),
                    new TableCell(pageSize)
                    )}
            };

            return new TableLayout {
                Rows = {
                    new TableRow(new TableCell(controls)),
                    new TableRow(new TableCell(new PageGridView(_context, result)))
                }
            };
        }

        public Request CreateRequest(string fileName) {

            var selected = Menu.Items.GetSubmenu("Connections").Items.Where(i => !string.IsNullOrEmpty(i.Text)).Cast<RadioMenuItem>().First(mu => mu.Checked).Text;
            var connection = _cfg.Connections.Where(c => !c.Name.In("input", "output")).FirstOrDefault(c => selected == c.Name);

            return new Request(fileName) {
                Configuration = _configuration,
                Provider = connection?.Provider,
                Server = connection?.Server,
                Database = connection?.Database,
                DatabaseFile = connection?.File, //sqlite
                Schema = connection?.Schema,
                View = connection?.Table,
                User = connection?.User,
                Password = connection?.Password,
                Port = connection?.Port ?? 0,
                Types = Menu.Items.GetSubmenu("Types").Items.Cast<CheckMenuItem>().Where(mi => mi.Checked && mi.Enabled).Select(mi => mi.Text).ToList()
            };
        }

    }
}