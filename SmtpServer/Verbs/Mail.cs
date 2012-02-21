using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MailDeluxe;

namespace MailDeluxe.SmtpServer.Verbs
{
    class Mail : IVerb
    {
        public void Do(Session session)
        {
            session.Message = new MailMessage();

            session.From = new System.Net.Mail.MailAddress(SmtpMailVerbUtils.ParseValue(session.Commands.Last()));

            session.Socket.SendString(SmtpCommandUtils.SV_OK);
        }

        
    }
}
