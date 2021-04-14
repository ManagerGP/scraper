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
            result = ShareHtmlDocument(url);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            var tableElem = document.DocumentNode.SelectSingleNode(xpath);
            List<string> header = GetHeaderTable(tableElem);
            List<List<string>> body = GetBodyTable(tableElem);

            string response = BuildJsonStringTable(header, body);
            
            return response;
        }

        /// <summary>
        /// Method to download an html page
        /// </summary>
        /// <returns>document</returns>
        private static string ShareHtmlDocument(string url)
        {
            string htmlString = "";
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        htmlString = content.ReadAsStringAsync().Result;
                    }
                }
            }
            
            return htmlString;
        }


        /// <summary>
        /// Method that allows you to create a json Array from a header and body data table
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        /// <returns>json</returns>
        private static string BuildJsonStringTable(List<string> headerTable, List<List<string>> bodyTable)
        {
            JArray json = new JArray();
            foreach (var row in bodyTable)
            {
                JObject elementRow = new JObject();
                int i = 0;
                foreach (var cell in row)
                {
                    elementRow.Add(headerTable[i++], new JValue(Regex.Replace(cell, @"\n\s+", " ")));
                }
                json.Add(elementRow);
            }
            return json.ToString();
        }

        /// <summary>
        /// Method that allows you to select the header of a table given the HTML node
        /// </summary>
        /// <param name="tableElem"></param>
        /// <returns>headerTable</returns>
        private static List<string> GetHeaderTable(HtmlNode tableElem)
        {
            int z = 1;
            List<string> headerTable = tableElem.Descendants("tr")
                .Take(1)
                .Where(tr => tr.Elements("th").Count() > 1)
                .SelectMany(tr => tr.Elements("th").Select(td => td.InnerText.Trim()).ToList())
                .ToList().Select(elem => elem == string.Empty ? (z++).ToString() : elem).ToList();

            return headerTable;
        }

        /// <summary>
        /// Method that allows you to select the Body of a table given the HTML node
        /// </summary>
        /// <param name="tableElem"></param>
        /// <returns>bodyTable</returns>
        private static List<List<string>> GetBodyTable(HtmlNode tableElem)
        {
            List<List<string>> bodyTable = tableElem
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .ToList();
            return bodyTable;
        }
    }
}
