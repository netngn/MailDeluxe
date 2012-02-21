using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MailDeluxe;

namespace MailDeluxe.SmtpServer
{
    public class Session
    {
        public SimpleSocket Socket;
        public event MailMessageHandler MessageRecieved;
        public event RecipientHandler RecipientFound;
        public void TriggerMessage(MailMessage m) { MessageRecieved(m); }
        public void TriggerRecipient(System.Net.Mail.MailAddress m) { RecipientFound(m); }

        public MailMessage Message;
        public System.Net.Mail.MailAddress From;

        public Session(SimpleSocket socket, MailMessageHandler messageEvent, RecipientHandler recipientEvent)
        {
            this.Socket = socket;
            this.MessageRecieved += messageEvent;
            this.RecipientFound += recipientEvent;
            Socket.SendString(SmtpCommandUtils.SV_GREET);
        }

        public List<string> Commands = new List<string>();
        
    }
}
