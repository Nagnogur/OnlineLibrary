using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Gateway.Models
{
    public class IdentifierModel
    {
        public string Type { get; set; } = null!;
        public string IdentifierCode { get; set; } = null!;
    }
}
