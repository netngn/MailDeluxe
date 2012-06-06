using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace MailDeluxe.Utilities
{
    public class NetworkStatus
    {
        public static bool Ping(string hostName)
        {
            SelectQuery query = new SelectQuery("Win32_PingStatus", string.Format("Address='{0}'", hostName));
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject result in searcher.Get())
            {
                return (result["StatusCode"] != null && (0 == (UInt32)result["StatusCode"]));
            }
            return false;
        }
        public static bool IsGoogleUp()
        {
            return Ping("google.com");
        }
    }
}
