﻿using Goldenacre.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedVariable

namespace Goldenacre.Test
{
    [TestClass]
    public class NumberExtensionsTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var t1 = Helper.AppFolder();

            Assert.IsNotNull(t1);
        }
    }
}