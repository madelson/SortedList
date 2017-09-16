using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections.Tests
{
    internal static class TestHelper
    {
        public static T ShouldEqual<T>(this T @this, T that, string message = null)
        {
            Assert.AreEqual(actual: @this, expected: that, message: message);
            return @this;
        }
    }
}
