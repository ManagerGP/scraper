using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace BahrainGp.Services
{
    public static class ScraperService
    {
        public static String PerformScraping(string url, string xpath)
        {
            string result = string.Empty;

            result = GetHtmlPage(url);
            List<string> header = new List<string>();
            List<List<string>> table = new List<List<string>>();

            SetTableData(result, xpath, ref header, ref table);
            result = string.Empty;
            result = GetJsonStringData(header, table);
            return result;
        }

        private static string GetHtmlPage(string url)
        {
            string result = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        result = content.ReadAsStringAsync().Result;
                    }
                }
            }
            return result;
        }

        private static void SetTableData(string result, string xpath, ref List<string> header, ref List<List<string>> table)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            int z = 1;
            var tableElem = document.DocumentNode.SelectSingleNode(xpath);
            header = tableElem.Descendants("tr")
                        .Take(1)
                        .Where(tr => tr.Elements("th").Count() > 1)
                        .SelectMany(tr => tr.Elements("th").Select(td => td.InnerText.Trim()).ToList())
                        .ToList().Select(elem => elem == string.Empty ? (z++).ToString() : elem).ToList();

            table = tableElem
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .ToList();
        }

        private static string GetJsonStringData(List<string> header, List<List<string>> table)
        {
            JArray json = new JArray();
            foreach (var row in table)
            {
                JObject elementRow = new JObject();
                int i = 0;
                foreach (var cell in row)
                {
                    elementRow.Add(header[i++], new JValue(Regex.Replace(cell, @"\n\s+", " ")));
                }
                json.Add(elementRow);
            }
            return json.ToString();
        }
    }
}
