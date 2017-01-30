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
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Transformalize.Contracts;

namespace JunkDrawer.Eto.Core {
    public class PageGridView : GridView {
        private readonly IContext _context;

        public PageGridView(IContext context) {

            _context = context;
            GridLines = GridLines.None;
            ShowHeader = true;
            CellFormatting += (sender, e) => {
                e.BackgroundColor = e.Row % 2 == 0 ? Colors.White : Colors.Gainsboro;
                e.ForegroundColor = Colors.Black;
            };
            ColumnHeaderClick += (sender, e) => _context.Info("Column Header Clicked: {0}", e.Column.HeaderText);
        }

        public PageGridView(IContext context, PageResult page) : this(context) {
            Fill(page);
        }

        public void Fill(PageResult result) {

            if (result == null)
                return;

            DataStore = result.Rows;

            foreach (var field in result.Fields.Where(f => f.Name != "TflKey")) {
                Columns.Add(new GridColumn {
                    DataCell = field.ToCell(),
                    ID = field.Alias,
                    Resizable = true,
                    HeaderText = field.Label,
                    Editable = false
                });
            }

        }
    }
}