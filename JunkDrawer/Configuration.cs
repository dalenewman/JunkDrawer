using System.Configuration;
using System.Linq;

namespace JunkDrawer {
    public class Configuration : ConfigurationSection {

        [ConfigurationProperty("types")]
        public TypeElementCollection Types {
            get { return this["types"] as TypeElementCollection; }
        }

        public override bool IsReadOnly() {
            return false;
        }

        public InspectionRequest GetInspectionRequest() {
            var dataTypes = Types.Cast<TypeConfigurationElement>().Select(t => t.Type).ToArray();
            return new InspectionRequest() {
                DataTypes = dataTypes,
                DefaultLength = Types.DefaultLength,
                DefaultType = Types.Default,
                Top = Types.Top,
                Sample = Types.Sample
            };
        }
    }
}