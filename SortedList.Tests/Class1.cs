using System;
using NUnit.Framework;

namespace SortedList.Tests
{
    public class Class1
    {
        [Test]
        public void Test()
        {
            Assert.IsTrue(true);
        }

        [Test]
        public void TestDebug()
        {
            var val = new TestClass().GetValue();
            Assert.AreEqual(1, val);
        }
    }
}
