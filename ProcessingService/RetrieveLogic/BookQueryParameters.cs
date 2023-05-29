using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ProcessingService.RetrieveLogic
{
    public class BookQueryParameters //: QueryParameters
    {
        /*[FromQuery(Name = "title")]
        public string Title { get; set; }*/

        public string? Title;

        /*public uint MinPublishDate { get; set; }
        public uint MaxPublishDate { get; set; } = (uint)DateTime.Now.Year;
        public bool ValidYearRange => MaxPublishDate > MinPublishDate;*/

        /*public static ValueTask<BookQueryParameters?> BindAsync(HttpContext context,
                                                   ParameterInfo parameter)
        {
            const string titleKey = "title";
            const string sortByKey = "sortBy";
            const string sortDirectionKey = "sortDir";
            const string currentPageKey = "page";

            context.Request.Query[titleKey]

            *//*Enum.TryParse<SortDirection>(context.Request.Query[sortDirectionKey],
                                         ignoreCase: true, out var sortDirection);
            int.TryParse(context.Request.Query[currentPageKey], out var page);
            page = page == 0 ? 1 : page;

            var result = new PagingData
            {
                SortBy = context.Request.Query[sortByKey],
                SortDirection = sortDirection,
                CurrentPage = page
            };*//*

            var result = new BookQueryParameters
            {
                Title = ,
            }

            return ValueTask.FromResult<PagingData?>(result);
        }*/

    }
}
