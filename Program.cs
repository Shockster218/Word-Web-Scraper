using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    private const string directory = "https://www.merriam-webster.com/browse/thesaurus/";
    private const string entryDirectory = "https://www.merriam-webster.com/thesaurus/";

    private static List<String> pageEntries = new List<String>();

    private static List<string> nouns = new List<string>();
    private static List<string> adjectives = new List<string>();

    private static void Main(string[] args)
    {
        StartRootCrawler();
        Console.ReadKey();
    }

    private static async void StartRootCrawler()
    {
        //Loop through ascii characters
        for (int i = 97; i <= 122; i++)
        {
            string rootURL = directory + (char) i;
            HtmlDocument mainDocument = new HtmlDocument();
            HttpClient rootClient = new HttpClient();
            mainDocument.LoadHtml(await rootClient.GetStringAsync(rootURL));
            int pageCount = GetPageCount(mainDocument);
            for (int j = 1; j <= pageCount; j++)
            {
                string countedURL = rootURL + '/' + j.ToString();
                HtmlDocument pageDocument = new HtmlDocument();
                HttpClient pageClient = new HttpClient();
                pageDocument.LoadHtml(await pageClient.GetStringAsync(countedURL));
                PopulatePageEntries(pageDocument);
                pageClient.Dispose();
                foreach (string entry in pageEntries)
                {
                    HtmlDocument entryDocument = new HtmlDocument();
                    HttpClient entryClient = new HttpClient();
                    entryDocument.LoadHtml(await entryClient.GetStringAsync(entryDirectory + entry));
                    FilterEntry(entry, entryDocument);
                    entryClient.Dispose();
                }
                Console.WriteLine($"Page {j} complete for {(char)i}");
            }
            Console.WriteLine($"Nouns added: {nouns.Count}");
            Console.WriteLine($"Adjectives added: {adjectives.Count}");
            rootClient.Dispose();
        }
    }

    private static int GetPageCount(HtmlDocument document)
    {
        return Convert.ToInt32(document.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("counters")).FirstOrDefault().InnerText.Split("of ") [1]);
    }

    private static void PopulatePageEntries(HtmlDocument document)
    {
        List<HtmlNode> entryParentNodes = new List<HtmlNode>(document.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("entries")).ToList());
        foreach (HtmlNode parentNode in entryParentNodes)
        {
            List<HtmlNode> entryNodes = new List<HtmlNode>(parentNode.Descendants("a").ToList());
            foreach (HtmlNode entryNode in entryNodes)
            {
                pageEntries.Add(entryNode.InnerText);
            }
        }
    }

    private static void FilterEntry(string entry, HtmlDocument document)
    {
        try
        {
            string lexicalCategory = document.DocumentNode.Descendants("a").Where(node => node.GetAttributeValue("class", "").Equals("important-blue-link")).FirstOrDefault().InnerText.ToLower();
            if (lexicalCategory == "noun") { nouns.Add(entry); }
            else if (lexicalCategory == "adjective" || lexicalCategory == "adj") { adjectives.Add(entry); }
        }
        catch
        {
            return;
        }
    }
}