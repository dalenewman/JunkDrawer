using System.Collections.Generic;
using System.IO;
using JunkDrawer;
using NUnit.Framework;

namespace Test {

    [TestFixture]
    public class TestFileInformationReader {

        private readonly List<Field> _defaultFields = new List<Field>() {
            new Field("Header1"),
            new Field("Header2"),
            new Field("Header3")
        };


        [Test]
        public void TestFileImporter() {

            var importer = new FileImporter();
            var info = new FileInfo(@"TestFiles\Headers\Headers.xlsx");
            var request = new InspectionRequest() {
                DataTypes = new[] { "boolean", "int", "datetime" },
                DefaultType = "string",
                DefaultLength = "100",
                Top = 0
            };
            importer.Import(info, request);
        }

        [Test]
        public void TestExcel() {
            var fileInfo = new FileInfo(@"TestFiles\Headers\Headers.xlsx");
            var expected = new FileInformation(fileInfo, FileType.Excel, _defaultFields);
            var actual = FileInformationFactory.Create(fileInfo);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount(), actual.ColumnCount());
            Assert.AreEqual("Header2", actual.Fields[1].Name);
        }

        [Test]
        public void TestCommas() {
            var fileInfo = new FileInfo(@"TestFiles\Headers\Headers.csv");
            var expected = new FileInformation(fileInfo, FileType.CommaDelimited, _defaultFields);
            var actual = FileInformationFactory.Create(fileInfo);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount(), actual.ColumnCount());
            Assert.AreEqual("Header2", actual.Fields[1].Name);
        }

        [Test]
        public void TestPipes() {
            var fileInfo = new FileInfo(@"TestFiles\Headers\Headers.psv");
            var expected = new FileInformation(fileInfo, FileType.PipeDelimited, _defaultFields);
            var actual = FileInformationFactory.Create(fileInfo);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount(), actual.ColumnCount());
            Assert.AreEqual("Header2", actual.Fields[1].Name);
        }

        [Test]
        public void TestTabs() {
            var fileInfo = new FileInfo(@"TestFiles\Headers\Headers.tsv");
            var expected = new FileInformation(fileInfo, FileType.TabDelimited, _defaultFields);
            var actual = FileInformationFactory.Create(fileInfo);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount(), actual.ColumnCount());
            Assert.AreEqual("Header2", actual.Fields[1].Name);
        }

        [Test]
        public void TestSingleColumn() {

            var fileInfo = new FileInfo(@"TestFiles\Headers\Single.txt");
            var expected = new FileInformation(fileInfo, FileType.Unknown, new List<Field> { new Field("Header1") });
            var actual = FileInformationFactory.Create(fileInfo);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount(), actual.ColumnCount());
            Assert.AreEqual("Header1", actual.Fields[0].Name);
            Assert.AreEqual("4000", actual.Fields[0].Length);
        }

        [Test]
        public void TestFieldQuotedCsvStep1() {
            var fileInfo = new FileInfo(@"TestFiles\Headers\FieldQuoted.csv");
            var actual = FileInformationFactory.Create(fileInfo);

            Assert.AreEqual(3, actual.Fields.Count);

            Assert.AreEqual("State", actual.Fields[0].Name);
            Assert.AreEqual("Population", actual.Fields[1].Name);
            Assert.AreEqual("Shape", actual.Fields[2].Name);

            Assert.AreEqual("string", actual.Fields[0].Type);
            Assert.AreEqual("string", actual.Fields[1].Type);
            Assert.AreEqual("string", actual.Fields[2].Type);

            Assert.AreEqual(default(char), actual.Fields[0].Quote);
            Assert.AreEqual('\"', actual.Fields[1].Quote);
            Assert.AreEqual(default(char), actual.Fields[2].Quote);

            Assert.AreEqual("512", actual.Fields[0].Length);
            Assert.AreEqual("512", actual.Fields[1].Length);
            Assert.AreEqual("512", actual.Fields[2].Length);
        }

        [Test]
        public void TestFieldQuotedCsvStep2() {

            var fileInfo = new FileInfo(@"TestFiles\Headers\FieldQuoted.csv");
            var fileInformation = FileInformationFactory.Create(fileInfo);
            var inspection = new InspectionRequest() {
                DataTypes = new[] { "decimal" }
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
