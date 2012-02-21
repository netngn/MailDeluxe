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
        public event SenderAllowedHandler RecipientFound;
        public void TriggerMessage(MailMessage m) { MessageRecieved(m); }
        public bool IsSenderAllowed(MailMessage m) { return RecipientFound(m); }

        public MailMessage Message;
        public System.Net.Mail.MailAddress From;

        public Session(SimpleSocket socket, MailMessageHandler messageEvent, SenderAllowedHandler recipientEvent)
        {
            this.Socket = socket;
            this.MessageRecieved += messageEvent;
            this.RecipientFound += recipientEvent;
            Socket.SendString(SmtpCommandUtils.SV_GREET);
        }

        public List<string> Commands = new List<string>();
        
    }
}
