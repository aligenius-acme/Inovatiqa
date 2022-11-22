using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Inovatiqa.Services.Payments
{
    public class DictionarySerializer : IXmlSerializable
    {
        public DictionarySerializer()
        {
            Dictionary = new Dictionary<string, object>();
        }

        public DictionarySerializer(Dictionary<string, object> dictionary)
        {
            Dictionary = dictionary;
        }

        public void WriteXml(XmlWriter writer)
        {
            if (!Dictionary.Any())
                return;

            foreach (var key in Dictionary.Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteElementString("key", key);
                var value = Dictionary[key];
                writer.WriteElementString("value", value?.ToString());
                writer.WriteEndElement();
            }
        }

        public void ReadXml(XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                var key = reader.ReadElementString("key");
                var value = reader.ReadElementString("value");
                Dictionary.Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public Dictionary<string, object> Dictionary { get; }
    }
}