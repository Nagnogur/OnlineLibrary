namespace ProcessingService.RetrieveLogic
{
    public class QueryParameters
    {
        const int maxItemsPerCall = 500;
        public int pageNumber { get; set; } = 1;
        private int itemsPerPage = 10;
        public int PageSize
        {
            get
            {
                return itemsPerPage;
            }
            set
            {
                itemsPerPage = (value > maxItemsPerCall) ? maxItemsPerCall : value;
            }
        }
    }
}
