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
using Eto.Drawing;
using Eto.Forms;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace JunkDrawer.Eto.Core {

    public static class Extensions {

        public static Font H2 = new Font(new FontFamily("arial"), 12, FontStyle.Bold);
        public static Size Spacing = new Size(5, 5);
        public static Padding Padding = new Padding(10, 10, 10, 10);

        public static Cell ToCell(this Field f) {
            switch (f.Type) {
                case "bool":
                case "boolean":
                    return new CheckBoxCell(f.Alias) { Binding = new DelegateBinding<IRow,bool?>(r=>(bool)r[f])};
                default:
                    return new TextBoxCell(f.Alias) { Binding = new DelegateBinding<IRow,string>(r=>r[f].ToString())};
            }
        }

    }
}