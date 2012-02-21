﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MailDeluxe
{
    public class SafeDictionary<KT, VT> : Dictionary<KT, VT>
    {
        public SafeDictionary() { }
        public SafeDictionary(IEqualityComparer<KT> comparer) : base(comparer) { }

        public new VT this[KT key]
        {
            get
            {
                return this.Get(key);
            }
            set
            {
                this.Set(key, value);
            }
        }
    }

    public struct HeaderValue
    {
        private string _RawValue;
        private SafeDictionary<string, string> _Values;

        public HeaderValue(string value)
            : this()
        {
            _Values = new SafeDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _RawValue = (value ?? (value = string.Empty));
            _Values[string.Empty] = RawValue;

            var semicolon = value.IndexOf(';');
            if (semicolon > 0)
            {
                _Values[string.Empty] = value.Substring(0, semicolon).Trim();
                value = value.Substring(semicolon).Trim();
                ParseValues(_Values, value);
            }
        }
        public string Value { get { return this[string.Empty] ?? string.Empty; } }
        public string RawValue { get { return _RawValue ?? string.Empty; } }

        public string this[string name]
        {
            get { return _Values.Get(name, string.Empty); }
        }

        public static void ParseValues(IDictionary<string, string> result, string header)
        {
            while (header.Length > 0)
            {
                var eq = header.IndexOf('=');
                if (eq < 0) eq = header.Length;
                var name = header.Substring(0, eq).Trim().Trim(new[] { ';', ',' }).Trim();

                var value = header = header.Substring(Math.Min(header.Length, eq + 1)).Trim();

                if (value.StartsWith("\""))
                {
                    ProcessValue(1, ref header, ref value, '"');
                }
                else if (value.StartsWith("'"))
                {
                    ProcessValue(1, ref header, ref value, '\'');
                }
                else
                {
                    ProcessValue(0, ref header, ref value, ' ', ',', ';');
                }

                result.Set(name, value);
            }
        }

        private static void ProcessValue(int skip, ref string header, ref string value, params char[] lookFor)
        {
            var quote = value.IndexOfAny(lookFor, skip);
            if (quote < 0) quote = value.Length;
            header = header.Substring(Math.Min(quote + 1, header.Length));
            value = value.Substring(skip, quote - skip);
        }

        public override string ToString()
        {
            var props = _Values.Where(x => !string.IsNullOrEmpty(x.Key)).Select(x => x.Key + "=" + x.Value);
            return Value + (props.Any() ? ("; " + string.Join(", ", props)) : null);
        }
    }

    public class HeaderCollection : SafeDictionary<string, HeaderValue>
    {
        private static Regex emailAddressesRegex = new Regex("\"((?<name>[^\"]*)\"\\s<(?<email>\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}\\b)>)|((?<name>\\w*\\s\\w*)\\s<(?<email>\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}\\b)>)|((?<email>\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}\\b))",
           RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public HeaderCollection() : base(StringComparer.OrdinalIgnoreCase) { }

        public string GetBoundary()
        {
            return this["Content-Type"]["boundary"];
        }

        private static Regex[] rxDates = new[]{
        @"\d{1,2}\s+[a-z]{3}\s+\d{2,4}\s+\d{1,2}\:\d{2}\:\d{1,2}\s+[\+\-\d\:]*",
        @"\d{4}\-\d{1,2}-\d{1,2}\s+\d{1,2}\:\d{2}(?:\:\d{2})?(?:\s+[\+\-\d:]+)?",
      }.Select(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase)).ToArray();

        public DateTime GetDate()
        {
            var value = this["Date"].RawValue.ToNullDate();
            if (value == null)
            {
                foreach (var rx in rxDates)
                {
                    var match = rx.Matches(this["Received"].RawValue ?? string.Empty)
                      .Cast<Match>().LastOrDefault();
                    if (match != null)
                    {
                        value = match.Value.ToNullDate();
                        if (value != null)
                        {
                            break;
                        }
                    }
                }
            }

            //written this way so a break can be set on the null condition
            if (value == null)
                return DateTime.MinValue;
            return value.Value;
        }

        public T GetEnum<T>(string name) where T : struct, IConvertible
        {
            var value = this[name].RawValue;
            if (string.IsNullOrEmpty(value)) return default(T);
            var values = System.Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            return values.FirstOrDefault(x => x.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        public List<MailAddress> GetAddresses(string header)
        {
            string values = this[header].RawValue.Trim();
            List<MailAddress> addresses = new List<MailAddress>();
            if (String.IsNullOrWhiteSpace(values))
                return addresses;


            MatchCollection matches = emailAddressesRegex.Matches(values);
            foreach (Match m in matches)
            {
                if (!String.IsNullOrWhiteSpace(m.Groups["name"].Value))
                {
                    MailAddress address = String.Format("{0} {1}", m.Groups["name"].Value, m.Groups["email"].Value).ToEmailAddress();
                    //Could not handle the name correctly. :-)
                    if (address != null)
                    {
                        addresses.Add(address);
                        continue;
                    }
                }
                addresses.Add(m.Groups["email"].Value.ToEmailAddress());
            }
            if (addresses.Count == 0 && values.Length > 0 && values.IndexOf('@') > -1 && !values.Contains(".local"))
                throw new Exception("Unable to grab email address for " + values);
            if (addresses.Count(x => x == null) > 0)
                throw new Exception("Examine this list.");
            return addresses;
        }


        public static HeaderCollection Parse(string headers)
        {
            headers = Utilities.DecodeWords(headers);
            var temp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var lines = headers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int i;
            string key = null, value;
            foreach (var line in lines)
            {
                if (key != null && (line[0] == '\t' || line[0] == ' '))
                {
                    temp[key] += line.Trim();

                }
                else
                {
                    i = line.IndexOf(':');
                    if (i > -1)
                    {
                        key = line.Substring(0, i).Trim();
                        value = line.Substring(i + 1).Trim();
                        temp.Set(key, value);
                    }
                }
            }

            var result = new HeaderCollection();
            foreach (var item in temp)
            {
                result.Add(item.Key, new HeaderValue(item.Value));
            }
            return result;
        }
    }
}
