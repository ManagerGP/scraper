using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ArticoliWebService.AddControllers{
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
        /// <param name="url"></param>
        /// <param name="xpath"></param>
        /// <returns>result</returns>
        [HttpPost("table")]
         public IActionResult GetTableInformation([FromForm] string url, [FromForm] string xpath) {
            
            HtmlDocument document = ShareHtmlDocument(url); 
            var tableElem = document.DocumentNode.SelectSingleNode(xpath);
            List<string> header = GetHeaderTable(tableElem);
            List<List<string>> body= GetBodyTable(tableElem);
            JArray json = BuildJsonTable(header,body); 
            string response = json.ToString();                 
            return Ok(response);

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
        private JArray BuildJsonTable(List<string> headerTable , List<List<string>> bodyTable) {
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
