using System;
using System.Collections.Generic;

namespace JunkDrawer
{
    public class ColumnNameGenerator {

        public IEnumerable<string> Generate(int count) {
            var names = new List<string>();
            for (var i = 0; i < count; i++) {
                names.Add(CreateDefaultColumnName(i));
            }
            return names;
        }

        private static string CreateDefaultColumnName(int index) {
            var name = Convert.ToString((char)('A' + (index % 26)));
            while (index >= 26) {
                index = (index / 26) - 1;
                name = Convert.ToString((char)('A' + (index % 26))) + name;
            }
            return name;
        }
    }
}