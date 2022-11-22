using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Inovatiqa.Core.Rss
{
    public partial class RssFeed
    {
        #region Ctor

        public RssFeed(string title, string description, Uri link, DateTimeOffset lastBuildDate)
        {
            Title = new XElement(InovatiqaRssDefaults.Title, title);
            Description = new XElement(InovatiqaRssDefaults.Description, description);
            Link = new XElement(InovatiqaRssDefaults.Link, link);
            LastBuildDate = new XElement(InovatiqaRssDefaults.LastBuildDate, lastBuildDate.ToString("r"));
        }

        public RssFeed(Uri link) : this(string.Empty, string.Empty, link, DateTimeOffset.Now)
        {
        }

        #endregion

        #region Properties

        public KeyValuePair<XmlQualifiedName, string> AttributeExtension { get; set; }

        public List<XElement> ElementExtensions { get; } = new List<XElement>();

        public List<RssItem> Items { get; set; } = new List<RssItem>();

        public XElement Title { get; private set; }

        public XElement Description { get; private set; }

        public XElement Link { get; private set; }

        public XElement LastBuildDate { get; private set; }

        #endregion

        #region Methods

        public static async Task<RssFeed> LoadAsync(Stream stream)
        {
            try
            {
                var document = await XDocument.LoadAsync(stream, LoadOptions.None, default);

                var channel = document.Root?.Element(InovatiqaRssDefaults.Channel);

                if (channel == null)
                    return null;

                var title = channel.Element(InovatiqaRssDefaults.Title)?.Value ?? string.Empty;
                var description = channel.Element(InovatiqaRssDefaults.Description)?.Value ?? string.Empty;
                var link = new Uri(channel.Element(InovatiqaRssDefaults.Link)?.Value ?? string.Empty);
                var lastBuildDateValue = channel.Element(InovatiqaRssDefaults.LastBuildDate)?.Value;
                var lastBuildDate = lastBuildDateValue == null ? DateTimeOffset.Now : DateTimeOffset.ParseExact(lastBuildDateValue, "r", null);

                var feed = new RssFeed(title, description, link, lastBuildDate);

                foreach (var item in channel.Elements(InovatiqaRssDefaults.Item))
                {
                    feed.Items.Add(new RssItem(item));
                }

                return feed;
            }
            catch
            {
                return null;
            }
        }

        public string GetContent()
        {
            var document = new XDocument();
            var root = new XElement(InovatiqaRssDefaults.RSS, new XAttribute("version", "2.0"));
            var channel = new XElement(InovatiqaRssDefaults.Channel,
                new XAttribute(XName.Get(AttributeExtension.Key.Name, AttributeExtension.Key.Namespace), AttributeExtension.Value));

            channel.Add(Title, Description, Link, LastBuildDate);

            foreach (var element in ElementExtensions)
            {
                channel.Add(element);
            }

            foreach (var item in Items)
            {
                channel.Add(item.ToXElement());
            }

            root.Add(channel);
            document.Add(root);

            return document.ToString();
        }

        #endregion
    }
}