using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public static class LinkHeaderParser
    {
        public static List<LinkItem> ParseLinks(string linkHeader, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "LinkHeaderParser:ParseLinks");
            try
            {
                var links = linkHeader.Split(',');
                return links.Select(ParseLink).ToList();
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private static LinkItem ParseLink(string linkString)
        {
            var link = new LinkItem();
            var parts = linkString.Split(';');
            link.LinkUrl = parts.First().Trim().Replace("<", null).Replace(">", null);
            link.Rel = parts.Skip(1).Select(ToTuple).SingleOrDefault(t => t.Item1 == "rel").Item2;
            return link;
        }

        private static Tuple<string, string> ToTuple(string value)
        {
            var pair = value.Trim().Replace("\"", null).Split('=');
            return new Tuple<string, string>(pair[0], pair[1]);
        }

        public static LinkItem GetNextLink(HttpResponseHeaders headers, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "LinkHeaderParser:GetNextLink");
            try
            {
                LinkItem nextLink = null;
                IEnumerable<string> values;
                if (headers.TryGetValues("Link", out values))
                {
                    nextLink = ParseLinks(values.First(), thisOperationContext)?.SingleOrDefault(l => l.Rel == "next");
                }
                return nextLink;
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}