using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Scraper.Model
{
    public class ScraperSettings
    {
        [Required]
        [Url]
        public string Url { get; set; }

        [Required]
        public string XPath { get; set; }
    }

    public class ColumnMask
    {
        public string Name { get; set; }

        public string MatchingPattern { get; set; } = ".*";

        public string ReplacementPattern { get; set; } = "$&";

        public bool Exclude { get; set; }
    }

    public class TableScraperSettings : ScraperSettings
    {
       public Dictionary<int, ColumnMask> ColumnsMask { get; set; }
    }
}