using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MailDeluxe;

namespace MailDeluxe.SmtpServer.Verbs
{
    class Data : IVerb
    {
        public void Do(Session session)
        {
            // for readability
            var socket = session.Socket;

            socket.SendString(SmtpCommandUtils.SV_DATA_OK);
            
            socket.CommandSeperator = "\r\n.\r\n";
            string rawMessage = socket.GetNextCommand();
            socket.CommandSeperator = "\r\n";

            MailMessage message = new MailMessage();
            message.Load(rawMessage);

            if (string.IsNullOrEmpty(message.Body) || string.IsNullOrEmpty(message.Subject))
            {
                socket.SendString(SmtpCommandUtils.SV_UNKNOWN);
            }
            else
            {
                


                // all done with getting the data
                session.TriggerMessage(session.Message);

                socket.SendString(SmtpCommandUtils.SV_OK);
            }
        }
    }
}
