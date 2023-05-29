namespace ParsingService.Models.Entities
{
    public class BookModel
    {
        public string? title { get; set; }
        public List<Author>? authors { get; set; } = new List<Author>();
        public string? description { get; set; }
        public float? averagerating { get; set; }
        public string? subtitle { get; set; }
        public string? publisher { get; set; }
        public DateTime? publishedDate { get; set; }
        public int? pageCount { get; set; }
        public string? maturityRating { get; set; }
        public string? thumbnailLink { get; set; }
        public byte[]? thumbnailFile { get; set; }  // 
        public string? language { get; set; }
        //public bool? Free { get; set; }
        public int? ratingCount { get; set; }
        public DateTime? timeRetrieved { get; set; }
        public List<Identifier>? industryIdentifiers { get; set; } = new List<Identifier>();
        public List<Category>? categories { get; set; } = new List<Category>();
        public List<LinkWithPrice>? origin { get; set; } = new List<LinkWithPrice>();
    }
}
