using System;
using System.IO;
using JunkDrawer;
using NUnit.Framework;

namespace Test {

    [TestFixture]
    public class TestFileInformationReader {

        [Test]
        public void TestExcel() {
            var request = new Request(new[] { @"TestFiles\Headers\Headers.xlsx" });
            var expected = new FileInformation(request.FileInfo.FullName, FileType.Excel, 3, new[] { "Header1", "Header2", "Header3" });
            var actual = FileInformationFactory.Create(request);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
            Assert.AreEqual(expected.ColumnNames[0], actual.ColumnNames[0]);
        }

        [Test]
        public void TestCommas() {
            var request = new Request(new[] { @"TestFiles\Headers\Headers.csv" });
            var expected = new FileInformation(request.FileInfo.FullName, FileType.CommaDelimited, 3, new[] { "Header1", "Header2", "Header3" });
            var actual = FileInformationFactory.Create(request);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
            Assert.AreEqual(expected.ColumnNames, actual.ColumnNames);
        }

        [Test]
        public void TestPipes() {
            var request = new Request(new[] { @"TestFiles\Headers\Headers.psv" });
            var expected = new FileInformation(request.FileInfo.FullName, FileType.PipeDelimited, 3, new[] { "Header1", "Header2", "Header3" });
            var actual = FileInformationFactory.Create(request);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
            Assert.AreEqual(expected.ColumnNames, actual.ColumnNames);
        }

        [Test]
        public void TestTabs() {
            var request = new Request(new[] { @"TestFiles\Headers\Headers.tsv" });
            var expected = new FileInformation(request.FileInfo.FullName, FileType.TabDelimited, 3, new[] { "Header1", "Header2", "Header3" });
            var actual = FileInformationFactory.Create(request);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
            Assert.AreEqual(expected.ColumnNames, actual.ColumnNames);
        }

        [Test]
        public void TestRealWorldSpreadsheet1() {

            var request = new Request(new[] { @"C:\Temp\NVE\RenoProductionDeployment.xlsx" });
            var expected = new FileInformation(request.FileInfo.FullName, FileType.Excel, 20);
            var actual = FileInformationFactory.Create(request);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
        }

        [Test]
        public void TestRealWorldSpreadsheet2()
        {

            var request = new Request(new[] {@"C:\Temp\NVE\Headers\RenoProductionDeployment.xlsx"});
            var expected = new FileInformation(request.FileInfo.FullName, FileType.Excel, 20);
            var actual = FileInformationFactory.Create(request);

            Assert.AreEqual(expected.FileType, actual.FileType);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
        }


    }
}
