using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace RecommendationService.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
    }
}
