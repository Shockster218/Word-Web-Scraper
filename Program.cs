using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

class Program
{
    private const char ascii = 'a';
    private const int startPage = 1;

    private const string browseDir = "https://www.merriam-webster.com/browse/thesaurus/";
    private const string entryDir = "https://www.merriam-webster.com/thesaurus/";

    private static string nounsDir = $"{ Environment.CurrentDirectory }/nouns.txt";
    private static string adjectivesDir = $"{ Environment.CurrentDirectory }/adjectives.txt";

    private static List<string> pageEntries = new List<string>();

    private static List<string> nouns = new List<string>();
    private static List<string> adjectives = new List<string>();

    private static Regex filter = new Regex(@"[^a-zA-Z]");

    private static void Main(string[] args)
    {
        if (File.Exists(nounsDir)) { File.Delete(nounsDir); }
        if (File.Exists(adjectivesDir)) { File.Delete(adjectivesDir); }
        WordCrawler();
        Console.ReadKey();
    }

    private static async void WordCrawler()
    {
        //Loop through ascii characters
        for (int i = (int) ascii; i <= ascii + 25; i++)
        {
            Console.WriteLine($"Started for {Char.ToUpper((char)i)}");
            string rootURL = browseDir + (char) i;
            int pageCount = await RetrievePageCount(rootURL);
            for (int j = startPage; j <= pageCount; j++)
            {
                await Task.Run(() => RetrievePageEntries(rootURL + '/' + j.ToString()));
                foreach (string entry in pageEntries)
                {
                    try
                    {
                        await FilterEntry(entry);
                    }
                    catch
                    {
                        break;
                    }
                }
                Console.WriteLine($"Page {j} completed for {Char.ToUpper((char)i)}");
                pageEntries.Clear();
            }
            Console.WriteLine($"Nouns added: {nouns.Count}");
            Console.WriteLine($"Adjectives added: {adjectives.Count}");
            WriteWordsToFile();
        }
    }

    private static async Task<int> RetrievePageCount(string url)
    {
        HtmlDocument mainDocument = new HtmlDocument();
        HttpClient rootClient = new HttpClient();
        mainDocument.LoadHtml(await rootClient.GetStringAsync(url));
        int pageCount = GetPageCount(mainDocument);
        rootClient.Dispose();
        return pageCount;
    }

    private static async Task RetrievePageEntries(string url)
    {
        HtmlDocument pageDocument = new HtmlDocument();
        HttpClient pageClient = new HttpClient();
        pageDocument.LoadHtml(await pageClient.GetStringAsync(url));
        PopulatePageEntries(pageDocument);
        pageClient.Dispose();
    }

    private static async Task FilterEntry(string entry)
    {
        if (entry.Length > 3 && !filter.IsMatch(entry))
        {
            HtmlDocument entryDocument = new HtmlDocument();
            HttpClient entryClient = new HttpClient();
            entryDocument.LoadHtml(await entryClient.GetStringAsync(entryDir + entry));

            try
            {
                string lexicalCategory = entryDocument.DocumentNode.Descendants("a").Where(node => node.GetAttributeValue("class", "").Equals("important-blue-link")).FirstOrDefault().InnerText.ToLower();
                if (lexicalCategory == "noun") { nouns.Add(entry); }
                else if (lexicalCategory == "adjective" || lexicalCategory == "adj") { adjectives.Add(entry); }
            }
            catch
            {
                return;
            }
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

    private static void WriteWordsToFile()
    {
        using(StreamWriter nWriter = new StreamWriter(nounsDir, true))
        {
            foreach (string noun in nouns)
            {
                nWriter.WriteLine(noun);
            }
            nWriter.Close();
        }

        using(StreamWriter aWriter = new StreamWriter(adjectivesDir, true))
        {
            foreach (string adjective in adjectives)
            {
                aWriter.WriteLine(adjective);
            }
            aWriter.Close();
        }

        nouns.Clear();
        adjectives.Clear();
    }
}