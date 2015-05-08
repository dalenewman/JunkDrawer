using System;
using System.IO;
using JunkDrawer;
using NUnit.Framework;

namespace Test {

    [TestFixture]
    public class CustomCfg {

        [Test]
        public void TestRoot() {

            var jd = new JunkCfg(File.ReadAllText(@"default.xml"));

            foreach (var problem in jd.Problems()) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(0, jd.Problems().Count);

        }

    }
}
