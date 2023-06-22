using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace RecommendationService.Models
{
    public class ReviewModel
    {
        public string UserId { get; set; } = null!;
        public int BookId { get; set; }
        public int Rating { get; set; }
        public BookModel? Book { get; set; }
    }
}
