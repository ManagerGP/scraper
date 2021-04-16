using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Xml.XPath;
using Scraper.Model;
using Newtonsoft.Json;

namespace Scraper.Controllers {
/// <summary>
/// This controller allows you to extrapolate information on the MTGP, FE and F1 
/// </summary>
    [ApiController]
    [Route("scraper")]
    public class ScraperController: ControllerBase
    {      
        /// <summary>
        /// This method allows you to extrapolet information about riders tables
        /// url : Url to receive table information
        /// xpath: Indicates the name of the table
        /// 
        /// result = String with JsonArray table data
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>result</returns>
        [HttpPost("table")]
         public IActionResult GetTableInformation([FromForm] TableScraperSettings settings) {
            
            var req = HttpContext.Request;

            HtmlDocument document = ShareHtmlDocument(settings.Url); 
            var tableElem = document.DocumentNode.SelectSingleNode(settings.XPath);
            List<string> header = GetHeaderTable(tableElem);
            List<List<string>> body= GetBodyTable(tableElem);
            JArray json = BuildJsonTable(header,body, settings.ColumnsMask);  
            return Ok(json.ToString());

        }

        /// <summary>
        /// Method to download an html page
        /// </summary>
        /// <returns>document</returns>
        private HtmlDocument ShareHtmlDocument(string url) {

            HtmlDocument document = new HtmlDocument();
            string htmlString="";
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
            document.LoadHtml(htmlString);
            return document;
        }


        /// <summary>
        /// Method that allows you to create a json Array from a header and body data table
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        /// <returns>json</returns>
        private JArray BuildJsonTable(List<string> headerTable , List<List<string>> bodyTable, Dictionary<int, ColumnMask> masks) {
            JArray json = new JArray();
            foreach (var row in bodyTable)
            {
                JObject elementRow = new JObject();
                if(masks == null || masks.Count() == 0)
                {
                    for (int i = 0; i < row.Count; i++)
                    {
                        elementRow.Add(headerTable[i], new JValue(Regex.Replace(row[i], @"\n\s+", " ")));
                    }
                } else {
                    for (int i = 0; i < row.Count; i++)
                    {
                        if(masks.ContainsKey(i) && !masks[i].Exclude )
                        {
                            elementRow.Add(masks[i].Name, new JValue(Regex.Replace(row[i], masks[i].MatchingPattern, masks[i].ReplacementPattern))); 
                        }                    
                    }
                }
                json.Add(elementRow);
            }
            return json;
        }

        /// <summary>
        /// Method that allows you to select the header of a table given the HTML node
        /// </summary>
        /// <param name="tableElem"></param>
        /// <returns>headerTable</returns>
        private List<string> GetHeaderTable(HtmlNode tableElem) {
            int z = 1;
                    List<string> headerTable = tableElem.Descendants("tr")
                        .Take(1)
                        .Where(tr => tr.Elements("th").Count() > 1)
                        .SelectMany(tr => tr.Elements("th").Select(td => td.InnerText.Trim()).ToList())
                        .ToList().Select(elem => elem == string.Empty ? (z++).ToString(): elem).ToList();  
           
            return headerTable;
        }

        /// <summary>
        /// Method that allows you to select the Body of a table given the HTML node
        /// </summary>
        /// <param name="tableElem"></param>
        /// <returns>bodyTable</returns>
        private List<List<string>> GetBodyTable(HtmlNode tableElem) {
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
