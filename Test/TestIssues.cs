using System;
using System.IO;
using JunkDrawer;
using NUnit.Framework;
using Transformalize.Libs.EnterpriseLibrary.Validation.Validators;

namespace Test {

    [TestFixture]
    public class TestIssues {

        [Test]
        public void TestIssue001A() {
            var file = Path.GetTempFileName().Replace(".tmp", ".csv");
            File.WriteAllText(file, @"t1,t2,t3,t4
Monday,10,1.1,1/1/2014
Tuesday,11,2.2,2/1/2014
Wednesday,12,3.3,3/1/2014
Thursday,13,4.4,4/1/2014
Friday,14,5.5,5/1/2014
Saturday,15,6.6,6/1/2014");

            var dataTypes = new[] { "int32", "double", "datetime" };
            var request = new InspectionRequest() { DataTypes = dataTypes };
            var information = FileInformationFactory.Create(new FileInfo(file));
            var fields = new FieldInspector().Inspect(information, request).ToArray();

            Assert.AreEqual("string", fields[0].Type);
            Assert.AreEqual("int32", fields[1].Type);
            Assert.AreEqual("double", fields[2].Type);
            Assert.AreEqual("datetime", fields[3].Type);

            //really do it
            //new FileImporter().Import(new FileInfo(file), request);

        }

        [Test]
        public void TestIssue002A() {
            var validator = new TypeConversionValidator(typeof(DateTime));
            var results = validator.Validate("1.1");
            Assert.IsTrue(results.IsValid);

            validator = new TypeConversionValidator(typeof(DateTime));
            results = validator.Validate(1.1);
            Assert.IsFalse(results.IsValid);

            validator = new TypeConversionValidator(typeof(DateTime));
            results = validator.Validate("1.1000000000001");
            Assert.IsFalse(results.IsValid);

        }

        [Test]
        public void TestIssue002B() {

            const string file = @"TestFiles\Headers\Issue002.xlsx";

            var dataTypes = new[] { "int32", "datetime" };
            var request = new InspectionRequest() { DataTypes = dataTypes };
            var information = FileInformationFactory.Create(new FileInfo(file));
            var fields = new FieldInspector().Inspect(information, request).ToArray();

            Assert.AreEqual("string", fields[0].Type);
            Assert.AreEqual("int32", fields[1].Type);
            Assert.AreEqual("string", fields[2].Type);
            Assert.AreEqual("datetime", fields[3].Type);

            //really do it
            //new FileImporter().Import(new FileInfo(file), request);

        }

    }
}
