using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Gateway.Models
{
    public class ReviewModel
    {
        public string UserId { get; set; } = null!;
        public int BookId { get; set; }
        public int Rating { get; set; }
    }
}
