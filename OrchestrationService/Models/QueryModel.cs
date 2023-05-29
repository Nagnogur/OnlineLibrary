namespace ParsingService.Models
{
    public class QueryModel
    {
        public QueryModel()
        {
            this.pagingIncrement = 1;
            this.PagesToProcess = 1;
            this.pagingParameters = new KeyValuePair<string, int>("page", 1);
        }

        public QueryModel(QueryModel model)
        {
            this.pagingIncrement = model.pagingIncrement;
            this.PagesToProcess = model.PagesToProcess;
            this.pagingParameters = model.pagingParameters;
            this.changingValueParameters = model.changingValueParameters;
            //this.staticQueryParameters = model.staticQueryParameters;
        }
        public string url { get; set; } = null!;
        public Dictionary<string, string>? staticQueryParameters { get; set; }

        //  like subject:poetry  ==> subject:fiction ==> and so on
        // one at a time
        public List<(string, string)>? changingValueParameters { get; set; }
        public KeyValuePair<string, int>? pagingParameters { get; set; }
        public int pagingIncrement { get; set; }
        public int PagesToProcess { get; set; }
    }
}
