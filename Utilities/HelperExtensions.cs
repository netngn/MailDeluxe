using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace MailDeluxe {
  internal static class HelperExtensions {
      internal static void TryDispose<T>(ref T obj) where T : class, IDisposable
      {
          try
          {
              if (obj != null)
                  obj.Dispose();
          }
          catch (Exception) { }
          obj = null;
      }
    internal static string NotEmpty(this string input, params string[] others) {
      if (!string.IsNullOrEmpty(input))
        return input;
      foreach (var item in others) {
        if (!string.IsNullOrEmpty(item)) {
          return item;
        }
      }
      return string.Empty;
    }

    internal static int ToInt(this string input) {
      int result;
      if (int.TryParse(input, out result)) {
        return result;
      } else {
        return 0;
      }
    }

    internal static DateTime? ToNullDate(this string input) {
      DateTime result;
      input = NormalizeDate(input);
      if (DateTime.TryParse(input, out result)) {
        return result;
      } else {
          return DateTimeParser.ConvertWithTimezone(input);
      }
    }

    private static Regex rxTimeZoneName = new Regex(@"\s+\([a-z]+\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase); //Mon, 28 Feb 2005 19:26:34 -0500 (EST)
    private static Regex rxTimeZoneColon = new Regex(@"\s+(\+|\-)(\d{1,2})\D(\d{2})$", RegexOptions.Compiled | RegexOptions.IgnoreCase); //Mon, 28 Feb 2005 19:26:34 -0500 (EST)
    private static Regex rxTimeZoneMinutes = new Regex(@"([\+\-]?\d{1,2})(\d{2})$", RegexOptions.Compiled); //search can be strict because the format has already been normalized
    private static Regex rxNegativeHours = new Regex(@"(?<=\s)\-(?=\d{1,2}\:)", RegexOptions.Compiled);

    internal static string NormalizeDate(string value) {
      value = rxTimeZoneName.Replace(value, string.Empty);
      value = rxTimeZoneColon.Replace(value, match => " " + match.Groups[1].Value + match.Groups[2].Value.PadLeft(2, '0') + match.Groups[3].Value);
      value = rxNegativeHours.Replace(value, string.Empty);
      var minutes = rxTimeZoneMinutes.Match(value);
      if (minutes.Groups[2].Value.ToInt() > 60) { //even if there's no match, the value = 0
        value = value.Substring(0, minutes.Index) + minutes.Groups[1].Value + "00";
      }
      return value;
    }

    internal static string GetRFC2060Date(this DateTime date) {
      CultureInfo enUsCulture = CultureInfo.GetCultureInfo("en-US");
      return date.ToString("dd-MMM-yyyy hh:mm:ss zz", enUsCulture);
    }

    internal static string QuoteString(this string value) {
      return "\"" + value
                      .Replace("\\", "\\\\")
                      .Replace("\r", "\\r")
                      .Replace("\n", "\\n")
                      .Replace("\"", "\\\"") + "\"";
    }

    internal static bool StartsWithWhiteSpace(this string line) {
      if (string.IsNullOrEmpty(line))
        return false;
      var chr = line[0];
      return chr == ' ' || chr == '\t' || chr == '\n' || chr == '\r';
    }

    internal static string DecodeQuotedPrintable(string value, Encoding encoding = null) {
      if (encoding == null) {
        encoding = System.Text.Encoding.UTF8;
      }

      value = Regex.Replace(value, @"\=[\r\n]+", string.Empty, RegexOptions.Singleline);
      var matches = Regex.Matches(value, @"\=[0-9A-F]{2}");
      foreach (var match in matches.Cast<Match>().Reverse()) {
        int ascii = int.Parse(match.Value.Substring(1), System.Globalization.NumberStyles.HexNumber);

        //http://stackoverflow.com/questions/1318933/c-sharp-int-to-byte
        byte[] result = BitConverter.GetBytes(ascii);
        if (BitConverter.IsLittleEndian)
          Array.Reverse(result);

        value = value.Substring(0, match.Index)
         + encoding.GetString(result).Trim('\0')
         + value.Substring(match.Index + match.Length);
      }
      return value;
    }

    internal static string DecodeBase64(string data, Encoding encoding = null) {
      if (!IsValidBase64String(data)) {
        return data;
      }
      var bytes = Convert.FromBase64String(data);
      return (encoding ?? System.Text.Encoding.UTF8).GetString(bytes);
    }

    #region OpenPOP.NET
    internal static string DecodeWords(string encodedWords) {
      if (string.IsNullOrEmpty(encodedWords))
        return string.Empty;

      string decodedWords = encodedWords;

      // Notice that RFC2231 redefines the BNF to
      // encoded-word := "=?" charset ["*" language] "?" encoded-text "?="
      // but no usage of this BNF have been spotted yet. It is here to
      // ease debugging if such a case is discovered.

      // This is the regex that should fit the BNF
      // RFC Says that NO WHITESPACE is allowed in this encoding, but there are examples
      // where whitespace is there, and therefore this regex allows for such.
      const string strRegEx = @"\=\?(?<Charset>\S+?)\?(?<Encoding>\w)\?(?<Content>.+?)\?\=";
      // \w	Matches any word character including underscore. Equivalent to "[A-Za-z0-9_]".
      // \S	Matches any nonwhite space character. Equivalent to "[^ \f\n\r\t\v]".
      // +?   non-gready equivalent to +
      // (?<NAME>REGEX) is a named group with name NAME and regular expression REGEX

      var matches = Regex.Matches(encodedWords, strRegEx);
      foreach (Match match in matches) {
        // If this match was not a success, we should not use it
        if (!match.Success)
          continue;

        string fullMatchValue = match.Value;

        string encodedText = match.Groups["Content"].Value;
        string encoding = match.Groups["Encoding"].Value;
        string charset = match.Groups["Charset"].Value;

        // Get the encoding which corrosponds to the character set
        Encoding charsetEncoding = ParseCharsetToEncoding(charset);

        // Store decoded text here when done
        string decodedText;

        // Encoding may also be written in lowercase
        switch (encoding.ToUpperInvariant()) {
          // RFC:
          // The "B" encoding is identical to the "BASE64" 
          // encoding defined by RFC 2045.
          // http://tools.ietf.org/html/rfc2045#section-6.8
          case "B":
            decodedText = DecodeBase64(encodedText, charsetEncoding);
            break;

          // RFC:
          // The "Q" encoding is similar to the "Quoted-Printable" content-
          // transfer-encoding defined in RFC 2045.
          // There are more details to this. Please check
          // http://tools.ietf.org/html/rfc2047#section-4.2
          // 
          case "Q":
            decodedText = DecodeQuotedPrintable(encodedText, charsetEncoding);
            break;

          default:
            throw new ArgumentException("The encoding " + encoding + " was not recognized");
        }

        // Repalce our encoded value with our decoded value
        decodedWords = decodedWords.Replace(fullMatchValue, decodedText);
      }

      return decodedWords;
    }

    //http://www.opensourcejavaphp.net/csharp/openpopdotnet/HeaderFieldParser.cs.html
    /// Parse a character set into an encoding.
    /// </summary>
    /// <param name="characterSet">The character set to parse</param>
    /// <returns>An encoding which corresponds to the character set</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="characterSet"/> is <see langword="null"/></exception>
    public static Encoding ParseCharsetToEncoding(string characterSet) {
      if (string.IsNullOrEmpty(characterSet))
        return null;

      string charSetUpper = characterSet.ToUpperInvariant();
      if (charSetUpper.Contains("WINDOWS") || charSetUpper.Contains("CP")) {
        // It seems the character set contains an codepage value, which we should use to parse the encoding
        charSetUpper = charSetUpper.Replace("CP", ""); // Remove cp
        charSetUpper = charSetUpper.Replace("WINDOWS", ""); // Remove windows
        charSetUpper = charSetUpper.Replace("-", ""); // Remove - which could be used as cp-1554

        // Now we hope the only thing left in the characterSet is numbers.
        int codepageNumber = int.Parse(charSetUpper, System.Globalization.CultureInfo.InvariantCulture);

        return Encoding.GetEncoding(codepageNumber);
      }

      // It seems there is no codepage value in the characterSet. It must be a named encoding
      return Encoding.GetEncoding(characterSet);
    }
    #endregion


    #region IsValidBase64
    //stolen from http://stackoverflow.com/questions/3355407/validate-string-is-base64-format-using-regex
    private const char Base64Padding = '=';

    private static readonly HashSet<char> Base64Characters = new HashSet<char>() { 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 
            'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/'
        };

    internal static bool IsValidBase64String(string param) {
      if (param == null) {
        // null string is not Base64 
        return false;
      }

      // replace optional CR and LF characters
      param = param.Replace("\r", String.Empty).Replace("\n", String.Empty);

      int lengthWPadding = param.Length;
      if (lengthWPadding == 0 || (lengthWPadding % 4) != 0) {
        // Base64 string should not be empty
        // Base64 string length should be multiple of 4
        return false;
      }

      // replace pad chacters
      int lengthWOPadding;

      param = param.TrimEnd(Base64Padding);
      lengthWOPadding = param.Length;

      if ((lengthWPadding - lengthWOPadding) > 2) {
        // there should be no more than 2 pad characters
        return false;
      }

      foreach (char c in param) {
        if (!Base64Characters.Contains(c)) {
          // string contains non-Base64 character
          return false;
        }
      }

      // nothing invalid found
      return true;
    }
    #endregion

    internal static VT Get<KT, VT>(this IDictionary<KT, VT> dictionary, KT key, VT defaultValue = default(VT)) {
      if (dictionary == null)
        return defaultValue;
      VT value;
      if (dictionary.TryGetValue(key, out value))
        return value;
      return defaultValue;
    }

    internal static void Set<KT, VT>(this IDictionary<KT, VT> dictionary, KT key, VT value) {
      if (!dictionary.ContainsKey(key))
        lock (dictionary)
          if (!dictionary.ContainsKey(key)) {
            dictionary.Add(key, value);
            return;
          }

      dictionary[key] = value;
    }


    internal static void Fire<T>(this EventHandler<T> events, object sender, T args) where T : EventArgs {
      if (events == null)
        return;
      events(sender, args);
    }


    internal static bool Is(this string input, string other) {
      return string.Equals(input, other, StringComparison.OrdinalIgnoreCase);
    }

    
  }
}
