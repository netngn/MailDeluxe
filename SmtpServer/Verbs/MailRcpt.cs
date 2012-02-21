using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace MailDeluxe.SmtpServer.Verbs
{
    class MailRcpt : IVerb
    {
        public void Do(Session session)
        {
            MailAddress address = MailAddress.ExtractMailAddress(SmtpMailVerbUtils.ParseValue(session.Commands.Last()));
            session.Message.To.Add(address);
            if(session.IsSenderAllowed(session.Message))
            {
                session.Socket.SendString(SmtpCommandUtils.SV_OK);
            } else
            {
                session.Socket.SendString(SmtpCommandUtils.SV_UNKNOWN_MAILBOX);
                session.Socket.Close();
            }
            
        }
    }
}
