using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Gateway.Models
{
    public class LinkModel
    {
        public string? Link { get; set; }
        public double? ListPrice { get; set; }
        public string? CurrencyListPrice { get; set; }
        public double? RetailPrice { get; set; }
        public string? CurrencyRetailPrice { get; set; }
        public string? PortalDomain { get; set; }
    }
}
