using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BBCFeedReader_MJ.classes
{
    /// <summary>
    /// RSS feed loader
    /// </summary>
    public class RssReader : IDisposable
    {

        /// <summary>
        /// Elements of the RSS feed
        /// </summary>
        public class RssFeed
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public string Link { get; set; }
            public string LastBuildDate { get; set; }
            public DateTime Date { get; set; }
            public Dictionary<string, RssItem> Items { get; set; } 
        }

        public class RssItem 
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public string Guid { get; set; }
            public string PubDate { get; set; }
            public DateTime Date { get; set; }
        }


        /// <summary>
        /// CONSTUCTOR: Sets up loading of RSS feed
        /// </summary>
        /// <param name="rssURL">RSS feed URL</param>
        public RssReader(string rssURL)
        {
            IsValid = false;
            LoadRssFeed(rssURL);
        }

        public void Dispose()
        {
            Feed.Items.Clear();
            Feed = null;
        }

        /// <summary>
        /// Load RSS feed.
        /// </summary>
        /// <param name="rssURL">RSS feed</param>
        public void LoadRssFeed(string rssURL)
        {
            try
            {
                // Load RSS feed into XML doc
                XmlDocument rssXmlDoc = new XmlDocument();
                rssXmlDoc.Load(rssURL);
                Feed = new RssFeed();
                Feed.Url = rssURL;
                // Read channel details
                XmlNode titleNode = rssXmlDoc.SelectSingleNode("rss/channel/title");
                Feed.Title = titleNode?.InnerText ?? "";
                XmlNode descriptionNode = rssXmlDoc.SelectSingleNode("rss/channel/description");
                Feed.Description = descriptionNode?.InnerText ?? "";
                XmlNode linkNode = rssXmlDoc.SelectSingleNode("rss/channel/link");
                Feed.Link = linkNode?.InnerText ?? "";
                XmlNode dateNode = rssXmlDoc.SelectSingleNode("rss/channel/lastBuildDate");
                Feed.LastBuildDate = dateNode?.InnerText ?? "";
                DateTime outputDate;
                if (!string.IsNullOrEmpty(Feed.LastBuildDate))
                    if (DateTime.TryParse(Feed.LastBuildDate, out outputDate))
                        Feed.Date = outputDate;
                // Now read all the items
                XmlNode guidNode;
                Feed.Items = new Dictionary<string, RssItem>();
                XmlNodeList itemNodes = rssXmlDoc.SelectNodes("rss/channel/item");
                if (itemNodes!=null)
                    foreach (XmlNode itemNode in itemNodes)
                    {
                        titleNode = itemNode.SelectSingleNode("title");
                        descriptionNode = itemNode.SelectSingleNode("description");
                        linkNode = itemNode.SelectSingleNode("link");
                        guidNode = itemNode.SelectSingleNode("guid");
                        dateNode = itemNode.SelectSingleNode("pubDate");
                        string guid = guidNode?.InnerText ?? "";
                        RssItem newitem = new RssItem
                        {
                            Title = titleNode?.InnerText ?? "",
                            Description = descriptionNode?.InnerText ?? "",
                            Link = linkNode?.InnerText ?? "",
                            Guid = guid,
                            PubDate = dateNode?.InnerText ?? ""
                        };
                        if (!string.IsNullOrEmpty(newitem.PubDate))
                            if (DateTime.TryParse(newitem.PubDate, out outputDate))
                                newitem.Date = outputDate;
                        if (Feed.Items.ContainsKey(guid))
                            Feed.Items[guid] = newitem;
                        else
                            Feed.Items.Add(guid, newitem);
                    }
                // Now that the feed is read, we can set the validation to true
                IsValid = true;
            }
            catch (Exception e)
            {

            }
        }

        public RssFeed Feed { get; protected set; }

        public bool IsValid { get; protected set; }
    }
}
