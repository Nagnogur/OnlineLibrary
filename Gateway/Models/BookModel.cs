

namespace Gateway.Models
{
    public class BookModel
    {
        public BookModel()
        {
            Authors = new HashSet<AuthorModel>();
            Categories = new HashSet<CategoryModel>();
            IndustryIdentifiers = new HashSet<IdentifierModel>();
            Origin = new HashSet<LinkModel>();
            Reviews = new HashSet<ReviewModel>();
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
        public byte[]? ThumbnailFile { get; set; }
        public string? Language { get; set; }
        public float? AverageRating { get; set; }
        public int? RatingCount { get; set; }
        public ICollection<AuthorModel>? Authors { get; set; }
        public ICollection<IdentifierModel>? IndustryIdentifiers { get; set; }
        public ICollection<CategoryModel>? Categories { get; set; }
        public ICollection<LinkModel> Origin { get; set; }
        public ICollection<ReviewModel> Reviews { get; set; }
        public string? FileLocation { get; set; }
        public string? UserId { get; set; }
    }
}
