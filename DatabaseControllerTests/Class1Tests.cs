using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController
{
    [TestClass()]
    public class Class1Tests
    {
        [TestMethod()]
        public void MainTest()
        {
            var a = new Class1();
            a.Main();
        }
    }
}