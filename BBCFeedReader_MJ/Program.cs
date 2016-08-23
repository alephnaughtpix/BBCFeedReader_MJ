using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBCFeedReader_MJ.classes;

namespace BBCFeedReader_MJ
{
    class Program
    {
        protected static string defaultFeedURL = "http://feeds.bbci.co.uk/news/uk/rss.xml2";
        protected static string defaultDestFolder = "feeds/2";

        static void Main(string[] args)
        {
            NameValueCollection config = System.Configuration.ConfigurationManager.AppSettings;
            string feedUrl = config["FeedURL"] ?? defaultFeedURL;
            string destFolder = config["DestFolder"] ?? defaultDestFolder;
            using (RssReader feedReader = new RssReader(feedUrl))
            {
                using (JsonFeedHelper jsonProcessor = new JsonFeedHelper(destFolder))
                {
                    jsonProcessor.Load();
                    jsonProcessor.ImportRssFeed(feedReader);
                    jsonProcessor.ExportToFile();
                }
            }

        }
    }
}
