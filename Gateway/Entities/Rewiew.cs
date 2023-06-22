using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Gateway.Entities
{
    public class Review
    {
        public string UserId { get; set; } = null!;
        public int BookId { get; set; }
        public int Rating { get; set; }
        public virtual Book? Book { get; set; }
    }
}
