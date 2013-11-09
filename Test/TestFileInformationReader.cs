using JunkDrawer;
using NUnit.Framework;

namespace Test {

    [TestFixture]
    public class TestFileInformationReader {

        private readonly FileInformationReader _informationReader = new FileInformationReader(5);

        [Test]
        public void TestExcel() {
            const string fileName = @"TestFiles\Headers.xlsx";
            var expected = new FileInformation(fileName, FileType.Excel, 3, new[] { "Header 1", "Header 2", "Header 3" });
            var actual = _informationReader.Read(fileName);
            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
            Assert.AreEqual(expected.ColumnNames, actual.ColumnNames);
        }

        [Test]
        public void TestCommas() {
            const string fileName = @"TestFiles\Headers.csv";
            var expected = new FileInformation(fileName, FileType.CommaDelimited, 3, new[] { "Header 1", "Header 2", "Header 3" });
            var actual = _informationReader.Read(fileName);
            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
            Assert.AreEqual(expected.ColumnNames, actual.ColumnNames);
        }

        [Test]
        public void TestPipes() {
            const string fileName = @"TestFiles\Headers.psv";
            var expected = new FileInformation(fileName, FileType.PipeDelimited, 3, new[] { "Header 1", "Header 2", "Header 3" });
            var actual = _informationReader.Read(fileName);
            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
            Assert.AreEqual(expected.ColumnNames, actual.ColumnNames);
        }

        [Test]
        public void TestTabs() {
            const string fileName = @"TestFiles\Headers.tsv";
            var expected = new FileInformation(fileName, FileType.TabDelimited, 3, new[] { "Header 1", "Header 2", "Header 3" });
            var actual = _informationReader.Read(@"TestFiles\Headers.tsv");
            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
            Assert.AreEqual(expected.ColumnNames, actual.ColumnNames);
        }

    }
}
