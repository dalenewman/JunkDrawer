using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;

namespace JunkDrawer.Eto.Core {
    public class MainForm : Form {
        private readonly IJunkBootstrapper _bs;

        public MainForm(IJunkBootstrapper bs) {
            _bs = bs;
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

                using (var scope = _bs) {
                    var request = new JunkRequest(openDialogue.FileName);
                    try {
                        var result = scope.Resolve<JunkImporter>(request).Import();
                        MessageBox.Show($"Imported {result.Records} records into {result.View}.", "Import Complete", MessageBoxButtons.OK);

                        var collection = new List<Dictionary<string, object>>{
                            new Dictionary<string, object> {{"p1", 1}, {"p2", 2}},
                            new Dictionary<string, object> {{"p1", 3}, {"p2", 4}}
                        };

                        var expandos = collection.Select(kv => new {p1 = kv["p1"], p2 = kv["p2"]});

                        var grid = new GridView { DataStore = expandos};

                        grid.Columns.Add(new GridColumn {
                            DataCell = new TextBoxCell("p1"),
                            HeaderText = "P1"
                        });

                        grid.Columns.Add(new GridColumn {
                            DataCell = new TextBoxCell("p2"),
                            HeaderText = "P2"
                        });

                        Content = grid;

                    } catch (Exception ex) {
                        MessageBox.Show(ex.Message, MessageBoxType.Error);
                    }
                }
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