using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace BBCFeedReader_MJ.classes
{
    public class JsonFeedHelper : IDisposable
    {

        // First the JSON structure needed for the feed

        public class JsonFeed
        {
            public string title;
            public string description;
            public string link;
            public List<JsonFeedItem> items;
        }

        public class JsonFeedItem
        {
            public string title;
            public string description;
            public string link;
            public string pubDate;
        }

        private string folderPath;

        private JsonFeed inputFeed;         // Feed from the outside RSS

        private JsonFeed outputFeed;        // Outside feed from today

        public JsonFeedHelper(string folder = "feed/")
        {
            folderPath = folder;
        }

        public void Dispose()
        {
            inputFeed.items.Clear();
            inputFeed = null;
            outputFeed.items.Clear();
            outputFeed = null;
        }

        /// <summary>
        /// Load all JSON files for today into today's feed
        /// </summary>
        public void Load()
        {
            inputFeed = new JsonFeed { items = new List<JsonFeedItem>() };
            if (Directory.Exists(folderPath))
            {
                // Get all of today's JSON files
                string fileMatch = DateTime.Now.Date.ToString("yyyy-MM-dd") + "*.json";
                string[] todaysFiles = Directory.GetFiles(folderPath, fileMatch);
                if (todaysFiles.Length > 0)
                    foreach (string filename in todaysFiles)
                        // For each JSON file found...
                        using (StreamReader reader = new StreamReader(filename))
                        {
                            string json = reader.ReadToEnd();
                            JsonFeed newFeed = JsonConvert.DeserializeObject<JsonFeed>(json);   // Load the feeds
                            if (string.IsNullOrEmpty(inputFeed.title)) inputFeed.title = newFeed.title;
                            if (string.IsNullOrEmpty(inputFeed.description)) inputFeed.title = newFeed.description;
                            if (string.IsNullOrEmpty(inputFeed.link)) inputFeed.title = newFeed.link;
                            inputFeed.items.AddRange(newFeed.items);
                        }
            }
        }

        /// <summary>
        /// Import an RSS feed into the JSON feed
        /// </summary>
        /// <param name="importReader">RssFeed Object</param>
        public void ImportRssFeed(RssReader importReader)
        {
            // Initialise the JSON feed
            outputFeed = new JsonFeed
            {
                items = new List<JsonFeedItem>(),
                title = importReader.Feed.Title,
                description = importReader.Feed.Description,
                link = importReader.Feed.Link
            };
            // Add each item to the feed.
            foreach (RssReader.RssItem currentItem in importReader.Feed.Items.Values)
            {
                if (!alreadyLoaded(currentItem) && isToday(currentItem))
                    outputFeed.items.Add(new JsonFeedItem
                    {
                        title = currentItem.Title,
                        description = currentItem.Description,
                        link = currentItem.Link,
                        pubDate = currentItem.PubDate
                    });

            }
        }

        /// <summary>
        /// Save the current JSON feed to a text JSON file.
        /// </summary>
        /// <param name="currentFilename">[Optional] filename</param>
        public void ExportToFile(string currentFilename = "")
        {
            if (outputFeed != null)
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                if (string.IsNullOrEmpty(currentFilename))
                    currentFilename = DateTime.Now.ToString("yyyy-MM-dd-HH") + ".json";
                currentFilename = folderPath + currentFilename;
                string outputJson = JsonConvert.SerializeObject(outputFeed, Formatting.Indented);
                File.WriteAllText(currentFilename, outputJson);
            }
        } 

        /// <summary>
        /// Has the feed element already been loaded.
        /// </summary>
        /// <param name="currentItem"></param>
        /// <returns>True/False</returns>
        private bool alreadyLoaded(RssReader.RssItem currentItem)
        {
            return inputFeed.items.Any(currentJsonFeedItem => currentJsonFeedItem.link == currentItem.Link);
        }

        /// <summary>
        /// Is the current feed item from today
        /// </summary>
        /// <param name="currentItem"></param>
        /// <returns>True/False</returns>
        private bool isToday(RssReader.RssItem currentItem)
        {
            return (currentItem.Date.Date.Equals(DateTime.Today));
        }

    }
}
