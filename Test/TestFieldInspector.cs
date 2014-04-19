using System.Collections.Generic;
using System.IO;
using JunkDrawer;
using NUnit.Framework;

namespace Test {

    [TestFixture]
    public class TestFieldInspector {

        [Test]
        public void TestFieldQuotedCsv() {

            var file = Path.GetTempFileName().Replace(".tmp", ".csv");
            File.WriteAllText(file, @"State,Population,Shape
MI,""10,000,000"",Mitten
CA,""20,000,000"",Sock
KS,""9,000,000"",Rectangle");

            var fileInformation = FileInformationFactory.Create(new FileInfo(file));
            var inspection = new InspectionRequest() {
                DataTypes = new List<string> { "decimal" }
            };
            var actual = new FieldInspector().Inspect(fileInformation, inspection);

            Assert.AreEqual(3, actual.Count);

            Assert.AreEqual("State", actual[0].Name);
            Assert.AreEqual("Population", actual[1].Name);
            Assert.AreEqual("Shape", actual[2].Name);

            Assert.AreEqual("string", actual[0].Type);
            Assert.AreEqual("decimal", actual[1].Type);
            Assert.AreEqual("string", actual[2].Type);

            Assert.AreEqual(default(char), actual[0].Quote);
            Assert.AreEqual('\"', actual[1].Quote);
            Assert.AreEqual(default(char), actual[2].Quote);

            Assert.AreEqual("3", actual[0].Length);
            Assert.AreEqual(string.Empty, actual[1].Length);
            Assert.AreEqual("10", actual[2].Length);
        }

    }
}
