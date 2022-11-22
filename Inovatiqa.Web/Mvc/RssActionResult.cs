using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Inovatiqa.Core.Rss;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Mvc
{
    public class RssActionResult : ContentResult
    {
        public RssActionResult(RssFeed feed, string feedPageUrl)
        {
            ContentType = "application/atom+xml";
            Feed = feed;

            XNamespace atom = "http://www.w3.org/2005/Atom";
            Feed.AttributeExtension = new KeyValuePair<XmlQualifiedName, string>(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", new Uri(feedPageUrl)), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));
        }

        public RssFeed Feed { get; set; }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            Content = Feed.GetContent();
            return base.ExecuteResultAsync(context);
        }

        public override void ExecuteResult(ActionContext context)
        {
            Content = Feed.GetContent();
            base.ExecuteResult(context);
        }
    }
}