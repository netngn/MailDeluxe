using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MailDeluxe.SmtpServer;

namespace MailDeluxe.Tests
{
    [TestClass]
    public class SimpleSmtpServerTests
    {
        SimpleSmtpServer server;

        [TestMethod]
        public void ExampleNewServer()
        {
            bool isRunning = true;

            server = new SimpleSmtpServer();
            server.MessageReceived += new MailMessageHandler(server_MessageReceived);
            server.RecipientReceived += new SenderAllowedHandler(server_RecipientReceived);
                Thread.Sleep(100);
            
            server.Dispose();
        }



        bool server_RecipientReceived(MailMessage m)
        {
            return false;
        }

        void server_MessageReceived(MailMessage m)
        {
            throw new NotImplementedException();
        }
    }
}
