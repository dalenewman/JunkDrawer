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
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Pipeline.Contracts;
using Pipeline.Extensions;

namespace JunkDrawer.Eto.Core {
    public class MainForm : Form {

        private readonly string _configuration;
        private readonly IJunkBootstrapperFactory _factory;
        private readonly JunkCfg _junkCfg;
        private readonly IContext _context;
        private readonly LogLevel _logLevel;
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        public TextArea LogArea { get; } = new TextArea { Wrap = false, };
        public int Hits { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public JunkRequest Request { get; set; }

        public MainForm(
            IJunkBootstrapperFactory factory,
            JunkCfg junkCfg,
            IContext context,
            LogLevel logLevel,
            string file,
            string configuration
        ) {
            _factory = factory;
            _junkCfg = junkCfg;
            _context = context;
            _logLevel = logLevel;
            _configuration = configuration;

            Title = "Junk Drawer";
            ClientSize = new Size(800, 600);
            CreateMenu();

            _worker.DoWork += (sender, args) => { Import(file); };
            if (!string.IsNullOrEmpty(file)) {
                _worker.RunWorkerAsync();
            }

        }

        private void CreateMenu() {
            // OPEN
            _context.Debug(() => "Creating Menu.");
            var open = new Command { MenuText = "&Open", ToolBarText = "Open", Shortcut = Application.Instance.CommonModifier | Keys.O };

            open.Executed += (sender, e) => {
                _context.Debug(() => "Opening file.");
                var openDialogue = new OpenFileDialog();
                if (openDialogue.ShowDialog(this) != DialogResult.Ok) {
                    _context.Debug(() => "Opening file cancelled.");
                    return;
                }
                _context.Debug(() => $"Selected {openDialogue.FileName}.");

                Title = $"Junk Drawer ({openDialogue.FileName})";

                if (_worker.IsBusy) {
                    MessageBox.Show("You must cancel your last request before starting another.");
                } else {
                    Import(openDialogue.FileName);
                }
            };

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
                        typeMenu.Items.Add(new CheckMenuItem { Text = type, Checked = _junkCfg.Input().Types.Any(t => t.Type.StartsWith(type)) });
                    }
                    typeMenu.Items.Add(new CheckMenuItem { Text = "string", Checked = true, Enabled = false });
                }

                if (Platform.Supports<RadioMenuItem>()) {
                    var controller = new RadioMenuItem { Text = "output", Checked = true };
                    connectionMenu.Items.Add(new RadioMenuItem(controller) { Text = "output", Checked = true });
                    connectionMenu.Items.AddSeparator();
                    foreach (var c in _junkCfg.Connections.Where(c => !c.Name.In("input", "output"))) {
                        connectionMenu.Items.Add(new RadioMenuItem(controller) { Text = c.Name, Checked = false });
                    }
                }

                Menu = new MenuBar {
                    Items = { fileMenu, connectionMenu, typeMenu },
                    QuitItem = quit
                };
            }
        }

        private static Control GetSpinner() {
            var spinner = new Spinner { Size = new Size(100, 100), Visible = true, Enabled = true };
            var spinLayout = new DynamicLayout { DefaultSpacing = Extensions.Spacing };
            spinLayout.AddCentered(spinner);
            return spinLayout;
        }

        private void Import(string fileName) {

            Content = new Splitter {
                Panel1 = GetSpinner(),
                Panel2 = _logLevel == LogLevel.None ? null : LogArea,
                Orientation = Orientation.Vertical
            };

            if (!string.IsNullOrEmpty(fileName)) {

                var selected = Menu.Items.GetSubmenu("Connections").Items.Where(i => !string.IsNullOrEmpty(i.Text)).Cast<RadioMenuItem>().First(mu => mu.Checked).Text;
                var connection = _junkCfg.Connections.Where(c => !c.Name.In("input", "output")).FirstOrDefault(c => selected == c.Name);

                Request = new JunkRequest(fileName) {
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
                try {
                    Page = 1;
                    PageSize = 20;
                    using (var scope = _factory.Produce(Request)) {
                        Response = scope.Resolve<JunkImporter>(Request).Import();
                        _context.Info($"Imported {Response.Records} records into {Response.View}.");
                        ShowPage();
                    }
                } catch (Exception ex) {
                    _context.Error(ex, ex.Message);
                    Content = new Splitter {
                        Panel1 = null,
                        Panel2 = _logLevel == LogLevel.None ? null : LogArea,
                        Orientation = Orientation.Vertical
                    };

                }
            }

        }

        private void ShowPage() {
            Content = new Splitter {
                Panel1 = GetPage(),
                Panel2 = LogArea,
                Orientation = Orientation.Vertical
            };
        }

        public JunkResponse Response { get; set; }

        private Control GetPage() {
            GridView grid = null;
            TableLayout controls = null;

            if (Response.Records > 0) {
                grid = GetGrid();

                var pages = Convert.ToInt32(Math.Ceiling((decimal)Hits / PageSize));
                var imageSize = new Size(16, 16);

                var first = new Button { Image = Icons.First, Size = imageSize, Enabled = Page > 1 };
                first.Click += (sender, args) => {
                    Page = 1;
                    ShowPage();
                };

                var previous = new Button { Image = Icons.Previous, Size = imageSize, Enabled = Page > 1 };
                previous.Click += (sender, args) => {
                    Page = Page - 1;
                    ShowPage();
                };

                var description = new Label { Text = $"Page {Page} of {pages} ({Hits} items)" };

                var next = new Button { Image = Icons.Next, Size = imageSize, Enabled = Page < pages };
                next.Click += (sender, args) => {
                    Page = Page + 1;
                    ShowPage();
                };

                var last = new Button { Image = Icons.Last, Size = imageSize, Enabled = Page < pages };
                last.Click += (sender, args) => {
                    Page = pages;
                    ShowPage();
                };

                controls = new TableLayout {
                    Spacing = Extensions.Spacing,
                    Rows = { new TableRow(
                    null,
                    new TableCell(first),
                    new TableCell(previous),
                    new TableCell(description),
                    new TableCell(next),
                    new TableCell(last),
                    null
                )}
                };
            }

            var table = new TableLayout {
                Rows = {
                    new TableRow(new TableCell(controls)),
                    new TableRow(new TableCell(grid))
                }
            };

            return table;
        }

        private GridView GetGrid() {

            var grid = new GridView { GridLines = GridLines.None };

            grid.CellFormatting += (sender, e) => {
                e.BackgroundColor = e.Row % 2 == 0 ? Colors.White : Colors.WhiteSmoke;
                e.ForegroundColor = Colors.Black;
            };

            _context.Info("Requesting page " + Page);

            using (var scope = _factory.Produce(Request)) {
                var pager = scope.Resolve<JunkPager>(Request, Response);
                var page = pager.GetPage(Page, PageSize);
                grid.DataStore = page.Rows;
                Hits = page.Hits;

                foreach (var field in page.Fields.Where(f => f.Name != "TflKey")) {
                    grid.Columns.Add(new GridColumn {
                        DataCell = field.ToCell(),
                        HeaderText = field.Label,
                        AutoSize = true,
                        Editable = false
                    });
                }
            }

            return grid;
        }

    }
}