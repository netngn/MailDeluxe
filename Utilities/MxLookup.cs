using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MailDeluxe.Utilities
{
    class MxRecordLookup
    {
        private const string DNS_SERVER = "8.8.8.8";  //change to your dns
        private const string QUERY_TYPE = "15"; //A=1  MX=15
        int[] resp;
        public string DoLookup(string domain)
        {
            // SEND REQUEST--------------------
            var req = CalculateBytesFromDomain(domain);
            var recv = DigDnsServer(req);

            resp = new int[recv.Length];
            for (int i = 0; i < resp.Length; i++)
                resp[i] = Convert.ToInt32(recv[i]);

            int status = resp[3];

            if (status == 1)
                throw new Exception("Malformed packet");
            if (status == 5)
                throw new Exception("DNS Server refused");
            if (status == 131)
                return String.Empty;
            if (status != 128)
                throw new Exception(String.Format("Error Occurred", status));

            int answers = resp[7];
            if (answers == 0)
                return String.Empty; ;

            int pos = domain.Length + 18;

            while (answers > 0)
            {
                int preference = resp[pos + 13];
                pos += 14; //offsetf
                string str = GetMXRecord(pos, out pos);
                Console.WriteLine(str + " " + preference);
                answers--;
            }
            return String.Empty;
        }

        private static byte[] DigDnsServer(byte[] req)
        {
            byte[] recv;
            using (UdpClient udpc = new UdpClient(DNS_SERVER, 53))
            {
                udpc.Send(req, req.Length);


                // RECEIVE RESPONSE--------------
                IPEndPoint ep = null;
                recv = udpc.Receive(ref ep);
                udpc.Close();
            }
            return recv;
        }

        private static byte[] CalculateBytesFromDomain(string domain)
        {
            List<byte> list = new List<byte>();
            list.AddRange(new byte[] {88, 89, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0});

            string[] tmp = domain.Split('.');
            foreach (string s in tmp)
            {
                list.Add(Convert.ToByte(s.Length));
                char[] chars = s.ToCharArray();
                foreach (char c in chars)
                    list.Add(Convert.ToByte(Convert.ToInt32(c)));
            }
            list.AddRange(new byte[] {0, 0, Convert.ToByte(QUERY_TYPE), 0, 1});

            byte[] req = new byte[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                req[i] = list[i];
            }
            return req;
        }

        private string GetMXRecord(int start, out int pos)
        {
            StringBuilder sb = new StringBuilder();
            int len = resp[start];
            while (len > 0)
            {
                if (len != 192)
                {
                    if (sb.Length > 0) sb.Append(".");
                    for (int i = start; i < start + len; i++)
                        sb.Append(Convert.ToChar(resp[i + 1]));
                    start += len + 1;
                    len = resp[start];
                }
                if (len == 192)
                {
                    int newpos = resp[start + 1];
                    if (sb.Length > 0) sb.Append(".");
                    sb.Append(GetMXRecord(newpos, out newpos));
                    start++;
                    break;
                }
            }
            pos = start + 1;
            return sb.ToString();
        }
    }
}
