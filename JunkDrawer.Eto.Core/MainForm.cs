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

        public MainForm(IJunkBootstrapperFactory factory) {
            _factory = factory;
            Title = "Junk Drawer";
            ClientSize = new Size(800, 600);
            CreateMenu();
        }

        private void CreateMenu() {
            // OPEN
            var open = new Command { MenuText = "&Open", ToolBarText = "Open", Shortcut = Application.Instance.CommonModifier | Keys.O };

            open.Executed += (sender, e) => {

                var openDialogue = new OpenFileDialog();
                if (openDialogue.ShowDialog(this) != DialogResult.Ok)
                    return;

                Title = $"Junk Drawer ({openDialogue.FileName})";

                var grid = new GridView {GridLines = GridLines.Both};

                var request = new JunkRequest(openDialogue.FileName);
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

                Content = grid;

            };

            // QUIT
            var quit = new Command { MenuText = "&Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quit.Executed += (sender, e) => Application.Instance.Quit();

            Menu = new MenuBar {
                Items = {
					// File submenu
					new ButtonMenuItem { Text = "&File", Items = { open } }
					// new ButtonMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
					// new ButtonMenuItem { Text = "&View", Items = { /* commands/items */ } },
				},
                QuitItem = quit
            };
        }
    }
}