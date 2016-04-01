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
using System.Linq;
using Eto.Forms;
using Eto.Drawing;

namespace JunkDrawer.Eto.Core {
    public class MainForm : Form {
        private readonly IJunkBootstrapperFactory _factory;
        private readonly JunkCfg _junkCfg;
        private readonly string _configuration;

        public MainForm(
            IJunkBootstrapperFactory factory,
            JunkCfg junkCfg,
            string file,
            string configuration
        ) {
            _factory = factory;
            _junkCfg = junkCfg;
            _configuration = configuration;
            Title = "Junk Drawer";
            ClientSize = new Size(800, 600);
            CreateMenu();
            if (!string.IsNullOrEmpty(file)) {
                Content = Import(file);
            }
        }

        private void CreateMenu() {
            // OPEN
            var open = new Command { MenuText = "&Open", ToolBarText = "Open", Shortcut = Application.Instance.CommonModifier | Keys.O };

            open.Executed += (sender, e) => {

                var openDialogue = new OpenFileDialog();
                if (openDialogue.ShowDialog(this) != DialogResult.Ok)
                    return;

                Title = $"Junk Drawer ({openDialogue.FileName})";

                Content = Import(openDialogue.FileName);

            };

            // QUIT
            var quit = new Command { MenuText = "&Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quit.Executed += (sender, e) => Application.Instance.Quit();

            if (Platform.Supports<MenuBar>()) {

                var fileMenu = new ButtonMenuItem { Text = "&File", Items = { open } };
                var providerMenu = new ButtonMenuItem { Text = "Provider", Items = { } };
                var typeMenu = new ButtonMenuItem { Text = "&Types", Items = { } };

                if (Platform.Supports<CheckMenuItem>()) {
                    var types = new[] { "bool", "byte", "short", "int", "long", "single", "double", "decimal", "datetime", "guid" };
                    foreach (var type in types) {
                        typeMenu.Items.Add(new CheckMenuItem { Text = type, Checked = _junkCfg.Input().Types.Any(t => t.Type.StartsWith(type)) });
                    }
                    typeMenu.Items.Add(new CheckMenuItem { Text = "string", Checked = true, Enabled = false });
                }

                if (Platform.Supports<RadioMenuItem>()) {
                    var controller = new RadioMenuItem { Text = "sqlserver", Checked = true };
                    var providers = new[] { "sqlserver", "postgresql", "mysql", "sqlite" };
                    foreach (var provider in providers) {
                        providerMenu.Items.Add(new RadioMenuItem(controller) { Text = provider, Checked = _junkCfg.Output().Provider == provider });
                    }
                }

                Menu = new MenuBar {
                    Items = { fileMenu, providerMenu, typeMenu },
                    QuitItem = quit
                };
            }
        }

        private Control Import(string fileName) {
            var grid = new GridView { GridLines = GridLines.Both };

            var request = new JunkRequest(fileName) {
                Configuration = _configuration,
                Provider = _junkCfg.Output().Provider,
                Types = _junkCfg.Input().Types.Select(t => t.Type).ToList()
            };
            try {
                using (var scope = _factory.Produce(request)) {
                    var response = scope.Resolve<JunkImporter>(request).Import();
                    if (response.Records > 0) {
                        var pager = scope.Resolve<JunkPager>(request, response);
                        var collection = pager.Read(1, 20);
                        grid.DataStore = collection;

                        foreach (var field in pager.Fields().Where(f => f.Name != "TflKey")) {
                            grid.Columns.Add(new GridColumn {
                                DataCell = field.ToCell(),
                                HeaderText = field.Label,
                                AutoSize = true,
                                Editable = false
                            });
                        }
                    }

                }
                // MessageBox.Show($"Imported {response.Records} records into {response.View}.", "Import Complete", MessageBoxButtons.OK);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, MessageBoxType.Error);
            }
            return grid;
        }
    }
}