using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace ProcessingService.Entities
{
    public class Book
    {
        public Book()
        {
            Authors = new HashSet<Author>();
            Categories = new HashSet<Category>();
            IndustryIdentifiers = new HashSet<Identifier>();
            Origin = new HashSet<LinkPrice>();
        }
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string? Subtitle { get; set; }
        public string? Publisher { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string? Description { get; set; }
        public int? PageCount { get; set; }
        public string? MaturityRating { get; set; }
        public string? ThumbnailLink { get; set; }
        public byte[]? ThumbnailFile { get; set; } // 
        public string? Language { get; set; }
        //public bool? Free { get; set; }
        public float? AverageRating { get; set; }
        public int? RatingCount { get; set; }
        public DateTime TimeRetrieved { get; set; }
        public DateTime? TimeUpdated { get; set; }
        public virtual ICollection<Author> Authors { get; set; }
        public virtual ICollection<Identifier> IndustryIdentifiers { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<LinkPrice> Origin { get; set; }


    }
}
