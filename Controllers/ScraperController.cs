using Microsoft.AspNetCore.Mvc;
using BahrainGp.Services;

namespace ArticoliWebService.AddControllers
{
    /// <summary>
    /// This controller allows you to extrapolate information on the MTGP, FE and F1 
    /// </summary>
    [ApiController]
    [Route("scraper")]
    public class BahrainGpController : ControllerBase
    {
        /// <summary>
        /// This method allows you to extrapolate information about riders tables
        /// url : Url to receive table information
        /// xpath: Indicates the name of the table
        /// 
        /// result = String with JsonArray table data
        /// </summary>
        /// <param name="url"></param>
        /// <param name="xpath"></param>
        /// <returns>result</returns>
        [HttpPost("table")]
        public IActionResult GetTableInformation([FromForm] string url, [FromForm] string xpath)
        {
            ScraperObject result = null;
            result = ScraperService.PerformScraping(url, xpath);

            return Ok(result);
        }
    }
}
