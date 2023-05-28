namespace OnlineLibrary.Models
{
    public class ApiModel
    {
        public string path { get; set; } = null!;
        public IList<string>? endpoints { get; set; }
        public Dictionary<string, string>? constantQueryParameters { get; set; }

        //  like subject:poetry  ==> subject:fiction ==> and so on
        // one at a time
        public List<(string, string)>? changingValueParameters { get; set; }
        public (string, int)? pagingParameters { get; set; }
        public int? itemsPerPage { get; set; }
    }
}
