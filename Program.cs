using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;

class Program
{
    private const string directory = "https://www.merriam-webster.com/browse/thesaurus/";

    private static HttpClient httpClient = new HttpClient();
    private static HtmlDocument document = new HtmlDocument();
    private static List<HtmlNode> wordNodes = new List<HtmlNode>();

    private static void Main(string[] args)
    {
        StartCrawlerSync();
        Console.ReadKey();
    }

    private static async void StartCrawlerSync()
    {
        //Loop through ascii characters
        for (int i = 97; i <= 122; i++)
        {
            string targetURL = directory + (char) i;
            string html = await httpClient.GetStringAsync(targetURL);
            document.LoadHtml(html);
            int pageCount = GetPageCount();
            for (int j = 0; j <= pageCount; j++)
            {
                
            }
        }
    }

    private static int GetPageCount()
    {
        return Convert.ToInt32(document.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("counters")).FirstOrDefault().InnerText.Split("of ")[1]);
    }

    private static List<HtmlNode> GetPageEntries()
    {
        return document.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("entries")).ToList();
    }
}