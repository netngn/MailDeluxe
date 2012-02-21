using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MailDeluxe
{
    public class MailAddress
    {
        private static Regex _emailAddressesRegex = new Regex("\"((?<name>[^\"]*)\"\\s<(?<email>\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}\\b)>)|((?<name>\\w*\\s\\w*)\\s<(?<email>\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}\\b)>)|((?<email>\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}\\b))",
           RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public string DisplayName { get; set; }
        public string Host { get { return Address.Split('@')[1]; } }
        public string User { get { return Address.Split('@')[0]; } }
        public string Address { get; set; }

        public override string ToString()
        {
            if (String.IsNullOrWhiteSpace(Address))
                return String.Empty;
            if (String.IsNullOrWhiteSpace(DisplayName))
                return String.Format("<{0}>", Address);
            return String.Format("\"{0}\" <{1}>", DisplayName, Address);
        }

        private MailAddress()
        {
        }


        public static List<MailAddress> ExtractMailAddresses(string addressesToExtract)
        {
            List<MailAddress> addresses = new List<MailAddress>();
            if (String.IsNullOrWhiteSpace(addressesToExtract))
                return addresses;


            MatchCollection matches = _emailAddressesRegex.Matches(addressesToExtract);
            foreach (Match m in matches)
            {
                MailAddress address = new MailAddress()
                    {

                        Address = m.Groups["email"].Value
                    };
                if (!String.IsNullOrWhiteSpace(m.Groups["name"].Value))
                {
                    address.DisplayName = m.Groups["name"].Value;

                }
                addresses.Add(address);
            }
            if (addresses.Count == 0 && addressesToExtract.Length > 0 && addressesToExtract.IndexOf('@') > -1)
                throw new Exception("Unable to grab email address for " + addressesToExtract);
            if (addresses.Count(x => x == null) > 0)
                throw new Exception("Examine this list.");
            return addresses;
        }
        

        public static MailAddress ExtractMailAddress(string addressToExtract)
        {
            return ExtractMailAddresses(addressToExtract).FirstOrDefault();
        }
    }
}
