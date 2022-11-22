using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Inovatiqa.Web.Paging.Interfaces;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Inovatiqa.Web.Extensions;

namespace Inovatiqa.Web.Paging
{
    public partial class Pager : IHtmlContent
    {
        public readonly IPageableModel model;
        protected readonly ViewContext viewContext;
        protected string pageQueryName = "page";
        protected bool showTotalSummary;
        protected bool showPagerItems = true;
        protected bool showFirst = false;
        protected bool showPrevious = false;
        protected bool showNext = false;
        protected bool showLast = false;
        protected bool showIndividualPages = true;
        protected bool renderEmptyParameters = true;
        protected int individualPagesDisplayedCount = 5;
        protected IList<string> booleanParameterNames;
		protected string firstPageCssClass = "first-page";
		protected string previousPageCssClass = "previous-page";
        protected string currentPageCssClass = "active";
        protected string individualPageCssClass = "individual-page";
        protected string nextPageCssClass = "next-page";
        protected string lastPageCssClass = "last-page";
        protected string mainUlCssClass = "";

        private HttpRequest _request;

        public Pager(IPageableModel model, ViewContext context, HttpRequest request)
		{
            this.model = model;
            viewContext = context;
            booleanParameterNames = new List<string>();
            _request = request;
        }

		protected ViewContext ViewContext => viewContext;

        public Pager QueryParam(string value)
		{
            pageQueryName = value;
			return this;
        }
        public Pager ShowTotalSummary(bool value)
        {
            showTotalSummary = value;
            return this;
        }
        public Pager ShowPagerItems(bool value)
        {
            showPagerItems = value;
            return this;
        }
        public Pager ShowFirst(bool value)
        {
            showFirst = value;
            return this;
        }
        public Pager ShowPrevious(bool value)
        {
            showPrevious = value;
            return this;
        }
        public Pager ShowNext(bool value)
        {
            showNext = value;
            return this;
        }
        public Pager ShowLast(bool value)
        {
            showLast = value;
            return this;
        }
        public Pager ShowIndividualPages(bool value)
        {
            showIndividualPages = value;
            return this;
        }
        public Pager RenderEmptyParameters(bool value)
        {
            renderEmptyParameters = value;
            return this;
        }
        public Pager IndividualPagesDisplayedCount(int value)
        {
            individualPagesDisplayedCount = value;
            return this;
        }
        public Pager BooleanParameterName(string paramName)
        {
            booleanParameterNames.Add(paramName);
            return this;
        }
        public Pager FirstPageCssClass(string value) {
            firstPageCssClass = value;
            return this;
        }
        public Pager PreviousPageCssClass(string value) {
            previousPageCssClass = value;
            return this;
        }
        public Pager CurrentPageCssClass(string value) {
            currentPageCssClass = value;
            return this;
        }
        public Pager IndividualPageCssClass(string value) {
            individualPageCssClass = value;
            return this;
        }
        public Pager NextPageCssClass(string value) {
            nextPageCssClass = value;
            return this;
        }
        public Pager LastPageCssClass(string value) {
            lastPageCssClass = value;
            return this;
        }
        public Pager MainUlCssClass(string value) {
            mainUlCssClass = value;
            return this;
        }

	    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
	    {
            var htmlString = GenerateHtmlString();
	        writer.Write(htmlString);
	    }
	    public override string ToString()
	    {
	        return GenerateHtmlString();
	    }
        public virtual string GenerateHtmlString()
		{
            if (model.TotalItems == 0) 
				return null;

            var links = new StringBuilder();
            if (showTotalSummary && (model.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary\">");
                links.Append(string.Format("Page {0} of {1} ({2} total)", model.PageIndex + 1, model.TotalPages, model.TotalItems));
                links.Append("</li>");
            }
            if (showPagerItems && (model.TotalPages > 1))
            {
                if (showFirst)
                {
                    if ((model.PageIndex >= 3) && (model.TotalPages > individualPagesDisplayedCount))
                    {
                        links.Append(CreatePageLink(1, "First Page", firstPageCssClass));
                    }
                }
                if (showPrevious)
                {
                    if (model.PageIndex > 0)
                    {
                        links.Append(CreatePageLink(model.PageIndex, "Previous", previousPageCssClass));
                    }
                }
                if (showIndividualPages)
                {
                    var firstIndividualPageIndex = GetFirstIndividualPageIndex();
                    var lastIndividualPageIndex = GetLastIndividualPageIndex();
                    for (var i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (model.PageIndex == i)
                        {
                            links.AppendFormat("<span class=\"{0}\">{1}</span>", currentPageCssClass, (i + 1));
                        }
                        else
                        {
                            links.Append(CreatePageLink(i + 1, (i + 1).ToString(), individualPageCssClass));
                        }
                    }
                }
                if (showNext)
                {
                    if ((model.PageIndex + 1) < model.TotalPages)
                    {
                        links.Append(CreatePageLink(model.PageIndex + 2, "Next", nextPageCssClass));
                    }
                }
                if (showLast)
                {
                    if (((model.PageIndex + 3) < model.TotalPages) && (model.TotalPages > individualPagesDisplayedCount))
                    {
                        links.Append(CreatePageLink(model.TotalPages, "Last", lastPageCssClass));
                    }
                }
            }

            var result = links.ToString();
            return result;
		}
	    public virtual bool IsEmpty()
	    {
            var html = GenerateHtmlString();
	        return string.IsNullOrEmpty(html);
	    }

        protected virtual int GetFirstIndividualPageIndex()
        {
            if ((model.TotalPages < individualPagesDisplayedCount) ||
                ((model.PageIndex - (individualPagesDisplayedCount / 2)) < 0))
            {
                return 0;
            }
            if ((model.PageIndex + (individualPagesDisplayedCount / 2)) >= model.TotalPages)
            {
                return (model.TotalPages - individualPagesDisplayedCount);
            }
            return (model.PageIndex - (individualPagesDisplayedCount / 2));
        }
        protected virtual int GetLastIndividualPageIndex()
        {
            var num = individualPagesDisplayedCount / 2;
            if ((individualPagesDisplayedCount % 2) == 0)
            {
                num--;
            }
            if ((model.TotalPages < individualPagesDisplayedCount) ||
                ((model.PageIndex + num) >= model.TotalPages))
            {
                return (model.TotalPages - 1);
            }
            if ((model.PageIndex - (individualPagesDisplayedCount / 2)) < 0)
            {
                return (individualPagesDisplayedCount - 1);
            }
            return (model.PageIndex + num);
        }
		protected virtual string CreatePageLink(int pageNumber, string text, string cssClass)
		{
            var aBuilder = new TagBuilder("a");
            aBuilder.InnerHtml.AppendHtml(text);
            aBuilder.MergeAttribute("href", CreateDefaultUrl(pageNumber));
		    return aBuilder.RenderHtmlContent();
		}
        protected virtual string CreateDefaultUrl(int pageNumber)
		{
            var routeValues = new RouteValueDictionary();

            var parametersWithEmptyValues = new List<string>();
			foreach (var key in viewContext.HttpContext.Request.Query.Keys.Where(key => key != null))
			{
                //string value = "";
                //if (key.ToLower() == "pagenumber" && model.PageNumber.ToString() != viewContext.HttpContext.Request.Query[key].ToString())
                //{
                //    value = model.PageNumber.ToString();
                //    pageNumber = Convert.ToInt32(value);
                //}
                //else
                //{
                //    value = viewContext.HttpContext.Request.Query[key].ToString();
                //}
                var value = viewContext.HttpContext.Request.Query[key].ToString();
                if (renderEmptyParameters && string.IsNullOrEmpty(value))
			    {
                    parametersWithEmptyValues.Add(key);
			    }
			    else
                {
                    if (booleanParameterNames.Contains(key, StringComparer.InvariantCultureIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(value) && value.Equals("true,false", StringComparison.InvariantCultureIgnoreCase))
                        {
                            value = "true";
                        }
                    }
                    routeValues[key] = value;
			    }
			}

            if (pageNumber > 1)
            {
                routeValues[pageQueryName] = pageNumber;
            }
            else
            {
                if (routeValues.ContainsKey(pageQueryName))
                {
                    routeValues.Remove(pageQueryName);
                }
            }

		    var url = _request.Path.Value;
            if (routeValues.Count > 0)
            {
                url += "?";
                foreach (var routeValue in routeValues)
                {
                    var lowerKey = routeValue.Key.ToLower();
                    //url += lowerKey + "=" + routeValue.Value?.ToString();
                    if (routeValue.Key.ToLower() == "pagenumber")
                    {
                        url += lowerKey + "=" + routeValue.Value?.ToString();
                        url += "&";
                    }
                    if (routeValue.Key.ToLower() == "term")
                    {
                        url += lowerKey + "=" + routeValue.Value?.ToString();
                        url += "&";
                    }
                    if (routeValue.Key.ToLower() == "categoryfilter")
                    {
                        url += lowerKey + "=" + routeValue.Value?.ToString();
                        url += "&";
                    }
                    if(routeValue.Key.ToLower() == "minprice")
                    {
                        url += lowerKey + "=" + routeValue.Value?.ToString();
                        url += "&";
                    }
                    if(routeValue.Key.ToLower() == "maxprice")
                    {
                        url += lowerKey + "=" + routeValue.Value?.ToString();
                        url += "&";
                    }
                    if(routeValue.Key.ToLower() == "pagesize")
                    {
                        url += lowerKey + "=" + routeValue.Value?.ToString();
                        url += "&";
                    }
                    if(routeValue.Key.ToLower() == "manufectuererfilter")
                    {
                        string man = routeValue.Value?.ToString();
                        List<string> manlist = man.Split(',').ToList();
                        if(manlist.Count > 1)
                        {
                            foreach(var manUrl in manlist)
                            {
                                url += lowerKey + "=" + manUrl;
                                url += "&";
                            }
                        }
                        else
                        {
                            url += lowerKey + "=" + routeValue.Value?.ToString();
                            url += "&";
                        }
                    }
                    if(routeValue.Key.ToLower() == "catsearchfilter")
                    {
                        string cat = routeValue.Value?.ToString();
                        List<string> catList = cat.Split(',').ToList();
                        if(catList.Count > 1)
                        {
                            foreach(var catUrl in catList)
                            {
                                url += lowerKey + "=" + catUrl;
                                url += "&";                           }
                        }
                        else
                        {
                            url += lowerKey + "=" + routeValue.Value?.ToString();
                            url += "&";
                        }
                    }
                    if (routeValue.Key.ToLower() == "attsearchfilter")
                    {
                        string att = routeValue.Value?.ToString();
                        List<string> attList = att.Split(',').ToList();
                        if (attList.Count > 1)
                        {
                            foreach (var attUrl in attList)
                            {
                                url += lowerKey + "=" + attUrl;
                                url += "&";
                            }
                        }
                        else
                        {
                            url += lowerKey + "=" + routeValue.Value?.ToString();
                            url += "&";
                        }
                    }
                }
            }
            return url;
        }
    }
}
