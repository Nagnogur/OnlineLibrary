using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace ProcessingService.Entities
{
    public class Author
    {
        public Author() 
        { 
            Books = new HashSet<Book>();
        }
        public int AuthorId { get; set; }
        public string Name { get; set; } = null!;
        public virtual ICollection<Book> Books { get; set; }
    }
}
