using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace ProcessingService.Entities
{
    public class LinkPrice
    {
        public int LinkPriceId { get; set; }
        public string? Link { get; set; }
        public double? ListPrice { get; set; }
        public string? CurrencyListPrice { get; set; }
        public double? RetailPrice { get; set; }
        public string? CurrencyRetailPrice { get; set; }
        public string? PortalDomain { get; set; }
        public virtual Book? Books { get; set; }
    }
}
