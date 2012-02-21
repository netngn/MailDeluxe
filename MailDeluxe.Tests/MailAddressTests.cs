using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Faker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MailDeluxe.Tests
{
    [TestClass]
    public class MailAddressTests
    {
        [TestMethod]
        public void PlainOldEmail()
        {
            string emailAddress = "jared.dobson@gmail.com";
            MailAddress address = MailAddress.ExtractMailAddress(emailAddress);
            Assert.IsNotNull(address);
            Assert.IsNotNull(address.Address);
            Assert.AreEqual(emailAddress, address.Address);
            Assert.AreEqual("jared.dobson", address.User);
            Assert.AreEqual("gmail.com", address.Host);
            Assert.AreEqual("<" + emailAddress + ">", address.ToString());
        }
        [TestMethod]
        public void NameAndEmail()
        {
            string name = "Jared Michael Dobson";
            string emailAddress = "jared.dobson@gmail.com";
            string expectedToString = String.Format("\"{0}\" <{1}>", name, emailAddress);
            MailAddress address = MailAddress.ExtractMailAddress(expectedToString);
            Assert.IsNotNull(address);
            Assert.IsNotNull(address.Address);
            Assert.AreEqual(name, address.DisplayName);
            Assert.AreEqual(emailAddress, address.Address);
            Assert.AreEqual("jared.dobson", address.User);
            Assert.AreEqual("gmail.com", address.Host);
            Assert.AreEqual(expectedToString, address.ToString());
        }
        [TestMethod]
        public void GeneratedTest()
        {
            string name = Name.FullName();
            string emailAddress = Internet.Email();
            string expectedToString = String.Format("\"{0}\" <{1}>", name, emailAddress);
            MailAddress address = MailAddress.ExtractMailAddress(expectedToString);
            Assert.IsNotNull(address);
            Assert.IsNotNull(address.Address);
            Assert.AreEqual(name, address.DisplayName);
            Assert.AreEqual(emailAddress, address.Address);
            Assert.AreEqual(emailAddress.Split('@')[0], address.User);
            Assert.AreEqual(emailAddress.Split('@')[1], address.Host);
            Assert.AreEqual(expectedToString, address.ToString());
        }
    }
}
