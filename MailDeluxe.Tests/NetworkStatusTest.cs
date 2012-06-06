using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using MailDeluxe.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MailDeluxe.Tests
{
    [TestClass]
    public class NetworkStatusTest
    {
        [TestMethod]
        public void SimpleHostPing()
        {
            Assert.IsTrue(NetworkStatus.Ping("google.com"));
        }
    }
}
