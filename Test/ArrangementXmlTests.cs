using JunkDrawer.Eto.Core;
using System.Xml.Linq;

namespace Test;

[TestClass]
public sealed class ArrangementXmlTests {
    [TestMethod]
    public void RemovesDeclaredDefaultsThroughoutTheProcess() {
        var result = XDocument.Parse(ArrangementXml.Simplify("""
            <cfg name="Example" enabled="True" status="00" pipeline="defer" message="OK">
              <connections><add name="input" provider="file" server="localhost" port="0" minlat="NaN" /></connections>
              <entities><add name="Data" page="0" size="25">
                <fields><add name="Title" type="string" inputtype="defer" /></fields>
                <calculatedfields><add name="Total" type="int" inputtype="defer" /></calculatedfields>
                <order><add field="Title" sort="asc" /></order>
              </add></entities>
            </cfg>
            """));

        var root = result.Root!;
        Assert.AreEqual("Example", root.Attribute("name")!.Value);
        foreach (var name in new[] { "enabled", "status", "pipeline", "message" }) {
            Assert.IsNull(root.Attribute(name));
        }
        var connection = root.Element("connections")!.Element("add")!;
        Assert.IsNull(connection.Attribute("server"));
        Assert.IsNull(connection.Attribute("port"));
        Assert.IsNull(connection.Attribute("minlat"));
        Assert.AreEqual("file", connection.Attribute("provider")!.Value);
        var entity = root.Element("entities")!.Element("add")!;
        Assert.IsNull(entity.Attribute("page"));
        Assert.AreEqual("25", entity.Attribute("size")!.Value);
        var field = entity.Element("fields")!.Element("add")!;
        Assert.IsNull(field.Attribute("type"));
        Assert.IsNull(field.Attribute("inputtype"));
        var calculatedField = entity.Element("calculatedfields")!.Element("add")!;
        Assert.AreEqual("int", calculatedField.Attribute("type")!.Value);
        Assert.IsNull(calculatedField.Attribute("inputtype"));
        Assert.IsNull(entity.Element("order")!.Element("add")!.Attribute("sort"));
    }

    [TestMethod]
    public void OmitsSearchTypesAndPreservesOtherContent() {
        var result = XDocument.Parse(ArrangementXml.Simplify("""
            <cfg name="Example" enabled="false" version="1 &amp; 2" custom="true">
              <searchTypes><add name="keyword" /></searchTypes>
              <entities><add name="Data" alias="Data" label="Data" sortable="true" page="2" size="20">
                <fields>
                  <add name="TflKey" type="int" system="true" />
                  <add name="TflDeleted" type="bool" system="True" />
                  <add name="Title" type="int" system="false" alias="Title" label="Title" sortfield="Title" sortable="true" />
                  <add name="Visible" alias="DisplayName" label="VISIBLE" sortfield="Title" sortable="false" />
                </fields>
                <calculatedfields>
                  <add name="Internal" system="true"><transforms><add method="copy" /></transforms></add>
                  <add name="Total" type="int" alias="Total" label="Total" sortfield="Total" sortable="true" />
                </calculatedfields>
                <rows><add name="" enabled="true" system="true" /></rows>
              </add></entities>
              <extension enabled="true">Keep &lt;this&gt;</extension>
            </cfg>
            """));

        var root = result.Root!;
        Assert.IsFalse(result.Descendants().Any(element =>
            element.Name.LocalName.Equals("searchTypes", StringComparison.OrdinalIgnoreCase)));
        Assert.AreEqual("false", root.Attribute("enabled")!.Value);
        Assert.AreEqual("1 & 2", root.Attribute("version")!.Value);
        Assert.AreEqual("true", root.Attribute("custom")!.Value);
        var entity = root.Element("entities")!.Element("add")!;
        Assert.AreEqual("2", entity.Attribute("page")!.Value);
        Assert.IsNull(entity.Attribute("alias"));
        Assert.IsNull(entity.Attribute("label"));
        Assert.IsNull(entity.Attribute("sortable"));
        Assert.AreEqual("int", entity.Element("fields")!.Element("add")!.Attribute("type")!.Value);
        CollectionAssert.AreEqual(new[] { "Title", "Visible" },
            entity.Element("fields")!.Elements("add").Select(field => field.Attribute("name")!.Value).ToArray());
        var title = entity.Element("fields")!.Elements("add").First();
        var visible = entity.Element("fields")!.Elements("add").Last();
        foreach (var attribute in new[] { "alias", "label", "sortfield", "sortable" }) {
            Assert.IsNull(title.Attribute(attribute));
            Assert.IsNull(entity.Element("calculatedfields")!.Element("add")!.Attribute(attribute));
        }
        Assert.AreEqual("DisplayName", visible.Attribute("alias")!.Value);
        Assert.AreEqual("VISIBLE", visible.Attribute("label")!.Value);
        Assert.AreEqual("Title", visible.Attribute("sortfield")!.Value);
        Assert.IsNull(visible.Attribute("sortable"));
        Assert.AreEqual("Total", entity.Element("calculatedfields")!.Element("add")!.Attribute("name")!.Value);
        Assert.AreEqual(1, entity.Element("calculatedfields")!.Elements("add").Count());
        Assert.AreEqual("", entity.Element("rows")!.Element("add")!.Attribute("name")!.Value);
        Assert.AreEqual("true", entity.Element("rows")!.Element("add")!.Attribute("system")!.Value);
        Assert.AreEqual("true", root.Element("extension")!.Attribute("enabled")!.Value);
        Assert.AreEqual("Keep <this>", root.Element("extension")!.Value);
    }
}
