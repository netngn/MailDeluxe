using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailDeluxe.SmtpServer
{
    class SmtpMailVerbUtils
    {
        public static string ParseValue(string command)
        {
            if (command.Contains(':'))
            {
                string value = command.Split(':')[1].TrimStart(new char[] { '<' }).TrimEnd(new char[] { '\r', '\n', '>' });
                return value;
            }
            else
            {
                return null;
            }
        }
    }
}
