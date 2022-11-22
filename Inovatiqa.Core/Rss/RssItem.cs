using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Inovatiqa.Core.Rss
{
    public partial class RssItem
    {
        public RssItem(string title, string content, Uri link, string id, DateTimeOffset pubDate)
        {
            Title = new XElement(InovatiqaRssDefaults.Title, title);
            Content = new XElement(InovatiqaRssDefaults.Description, content);
            Link = new XElement(InovatiqaRssDefaults.Link, link);
            Id = new XElement(InovatiqaRssDefaults.Guid, new XAttribute("isPermaLink", false), id);
            PubDate = new XElement(InovatiqaRssDefaults.PubDate, pubDate.ToString("r"));
        }

        public RssItem(XContainer item)
        {
            var title = item.Element(InovatiqaRssDefaults.Title)?.Value ?? string.Empty;
            var content = item.Element(InovatiqaRssDefaults.Content)?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(content))
                content = item.Element(InovatiqaRssDefaults.Description)?.Value ?? string.Empty;
            var link = new Uri(item.Element(InovatiqaRssDefaults.Link)?.Value ?? string.Empty);
            var pubDateValue = item.Element(InovatiqaRssDefaults.PubDate)?.Value;
            var pubDate = pubDateValue == null ? DateTimeOffset.Now : DateTimeOffset.ParseExact(pubDateValue, "r", null);
            var id = item.Element(InovatiqaRssDefaults.Guid)?.Value ?? string.Empty;

            Title = new XElement(InovatiqaRssDefaults.Title, title);
            Content = new XElement(InovatiqaRssDefaults.Description, content);
            Link = new XElement(InovatiqaRssDefaults.Link, link);
            Id = new XElement(InovatiqaRssDefaults.Guid, new XAttribute("isPermaLink", false), id);
            PubDate = new XElement(InovatiqaRssDefaults.PubDate, pubDate.ToString("r"));
        }

        #region Methods

        public XElement ToXElement()
        {
            var element = new XElement(InovatiqaRssDefaults.Item, Id, Link, Title, Content);

            foreach (var elementExtensions in ElementExtensions)
            {
                element.Add(elementExtensions);
            }

            return element;
        }

        #endregion

        #region Properties

        public XElement Title { get; private set; }

        public string TitleText => Title?.Value ?? string.Empty;

        public XElement Content { get; private set; }

        public string ContentText => XmlHelper.XmlDecode(Content?.Value ?? string.Empty);

        public XElement Link { get; private set; }

        public Uri Url => new Uri(Link.Value);

        public XElement Id { get; private set; }

        public XElement PubDate { get; private set; }

        public DateTimeOffset PublishDate => PubDate?.Value == null ? DateTimeOffset.Now : DateTimeOffset.ParseExact(PubDate.Value, "r", null);

        public List<XElement> ElementExtensions { get; } = new List<XElement>();

        #endregion
    }
}