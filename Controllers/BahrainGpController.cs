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
    public class BahrainGpController: ControllerBase
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
            string result ="";

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
            
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            
            var web = new HtmlWeb();
            int z = 1;
            var tableElem = document.DocumentNode.SelectSingleNode(xpath);
            List<string> header = tableElem.Descendants("tr")
                        .Take(1)
                        .Where(tr => tr.Elements("th").Count() > 1)
                        .SelectMany(tr => tr.Elements("th").Select(td => td.InnerText.Trim()).ToList())
                        .ToList().Select(elem => elem == string.Empty ? (z++).ToString(): elem).ToList();  

            List<List<string>> table = tableElem
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .ToList();
            result = string.Empty;
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
            result=json.ToString();                 
            return Ok(result);
        }
    }
}
