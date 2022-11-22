using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace Inovatiqa.Core
{
    public partial class XmlHelper
    {
        #region Methods

        public static string XmlEncode(string str)
        {
            if (str == null)
                return null;
            str = Regex.Replace(str, @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]", string.Empty, RegexOptions.Compiled);
            return XmlEncodeAsIs(str);
        }

        public static string XmlEncodeAsIs(string str)
        {
            if (str == null)
                return null;
            using var sw = new StringWriter();
            using var xwr = new XmlTextWriter(sw);
            xwr.WriteString(str);
            return sw.ToString();
        }

        public static string XmlEncodeAttribute(string str)
        {
            if (str == null)
                return null;
            str = Regex.Replace(str, @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]", string.Empty, RegexOptions.Compiled);
            return XmlEncodeAttributeAsIs(str);
        }

        public static string XmlEncodeAttributeAsIs(string str)
        {
            return XmlEncodeAsIs(str).Replace("\"", "&quot;");
        }

        public static string XmlDecode(string str)
        {
            var sb = new StringBuilder(str);
            return sb.Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").ToString();
        }

        public static string SerializeDateTime(DateTime dateTime)
        {
            var xmlS = new XmlSerializer(typeof(DateTime));
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            xmlS.Serialize(sw, dateTime);
            return sb.ToString();
        }

        public static DateTime DeserializeDateTime(string dateTime)
        {
            var xmlS = new XmlSerializer(typeof(DateTime));
            using var sr = new StringReader(dateTime);
            var test = xmlS.Deserialize(sr);
            return (DateTime)test;
        }

        #endregion
    }
}
