namespace ProcessingService.RetrieveLogic
{
    public class QueryParameters
    {
        const int maxItemsPerCall = 500;
        public int PageNumber { get; set; } = 1;
        private int pageSize = 10;
        public int PageSize
        {
            get
            {
                return pageSize;
            }
            set
            {
                pageSize = (value > maxItemsPerCall) ? maxItemsPerCall : value;
            }
        }
    }
}
