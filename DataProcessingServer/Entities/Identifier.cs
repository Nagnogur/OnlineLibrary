using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace ProcessingService.Entities
{
    public class Identifier
    {
        public int IdentifierId { get; set; }
        public string Type { get; set; } = null!;
        public string IdentifierCode { get; set; } = null!;
        public virtual Book? Books { get; set; }
    }
}
