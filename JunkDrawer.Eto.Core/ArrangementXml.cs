using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using Cfg.Net;
using Transformalize.Configuration;

namespace JunkDrawer.Eto.Core;

internal static class ArrangementXml {
    public static string Simplify(string xml) {
        if (string.IsNullOrWhiteSpace(xml)) return xml;

        var document = XDocument.Parse(xml);
        document.Descendants()
            .Where(element => element.Name.LocalName.Equals("searchTypes", StringComparison.OrdinalIgnoreCase))
            .Remove();
        if (document.Root is not null) Simplify(document.Root, typeof(Process));
        return document.ToString();
    }

    private static void Simplify(XElement element, Type type) {
        if (typeof(Field).IsAssignableFrom(type) &&
            bool.TryParse(element.Attribute("system")?.Value, out var system) && system) {
            element.Remove();
            return;
        }

        var nodeName = element.Attribute("name")?.Value;
        foreach (var property in type.GetProperties()) {
            var cfg = property.GetCustomAttribute<CfgAttribute>();
            if (cfg is null) continue;

            // Cfg-Net uses an explicit name or the lowercase letters/digits of the property name.
            var name = string.IsNullOrEmpty(cfg.name)
                ? new string(property.Name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray())
                : cfg.name;

            if (property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)) {
                var itemType = property.PropertyType.GetGenericArguments()[0];
                foreach (var collection in element.Elements().Where(child =>
                             child.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))) {
                    foreach (var item in collection.Elements().ToArray()) Simplify(item, itemType);
                }
            } else {
                foreach (var attribute in element.Attributes().Where(attribute =>
                             !attribute.IsNamespaceDeclaration && attribute.Name.Namespace == XNamespace.None &&
                             attribute.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray()) {
                    var redundantName = nodeName is not null &&
                        property.Name is "Alias" or "Label" or "SortField" &&
                        attribute.Value.Equals(nodeName, StringComparison.Ordinal);
                    if (property.Name == "Sortable" || redundantName ||
                        (cfg.ValueIsSet && IsDefault(attribute.Value, cfg, property.PropertyType))) {
                        attribute.Remove();
                    }
                }
            }
        }
    }

    private static bool IsDefault(string value, CfgAttribute cfg, Type type) {
        if (type == typeof(string)) {
            return string.Equals(value, (string)cfg.value,
                cfg.ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        try {
            // Compare typed values so bool casing and numeric formatting do not hide defaults.
            var defaultValue = Convert.ChangeType(cfg.value, type, CultureInfo.InvariantCulture);
            return Equals(Convert.ChangeType(value, type, CultureInfo.InvariantCulture), defaultValue);
        } catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException) {
            return false;
        }
    }
}
