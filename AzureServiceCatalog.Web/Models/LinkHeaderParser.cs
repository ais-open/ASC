using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    internal static class LinkHeaderParser
    {
        internal static List<LinkItem> ParseLinks(string linkHeader)
        {
            var links = linkHeader.Split(',');
            return links.Select(ParseLink).ToList();
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
    }

    internal class LinkItem
    {
        public string LinkUrl { get; set; }
        public string Rel { get; set; }
    }
}