using System;
using System.Linq;
using JunkDrawer;
using NUnit.Framework;

namespace Test {

    [TestFixture]
    public class TestProgram {

        [Test]
        public void TestInspectFile() {
            var fileInformation = FileInformationFactory.Create(@"C:\Users\dnewman\Documents\DukeInventory.xlsx");

            var fieldTypes = fileInformation.FieldTypes();
            Assert.AreEqual(8, fieldTypes.Count());

            foreach (var fieldType in fieldTypes) {
                Console.WriteLine("{0} = {1}({2})", fieldType.Name, fieldType.Type, fieldType.Length);
            }
        }

    }
}
