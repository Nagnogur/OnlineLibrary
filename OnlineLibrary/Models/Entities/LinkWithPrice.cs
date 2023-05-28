namespace ParsingService.Models.Entities
{
    public class LinkWithPrice
    {
        public string? link { get; set; }
        public double? listPrice { get; set; }
        public string? currencyListPrice { get; set; }
        public double? retailPrice { get; set; }
        public string? currencyRetailPrice { get; set; }
        public string? portalDomain { get; set; }
    }
}
